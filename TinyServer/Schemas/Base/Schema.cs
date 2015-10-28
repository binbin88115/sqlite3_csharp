using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Data;
using Mono.Data.Sqlite;

namespace TinyServer.Schemas
{
	public abstract class Schema 
	{
		private List<SchemaData> m_datas = new List<SchemaData>();
		protected List<SchemaData> DataList { get { return m_datas; } }

		/// <summary>
		/// 派生类需要继承该方法，返回SchemaData子类的实例对象 
		/// </summary>
		/// <returns>The schema data.</returns>
		protected abstract SchemaData GetSchemaData();

		/// <summary>
		/// 需要更新表数据的重载该方法，在每次GetSchema<T>(...)时，都会被调用一次
		/// </summary>
		public virtual void Update()
		{
		}

		/// <summary>
		/// 创建SQLITE中对应的表.
		/// </summary>
		public virtual bool Init()
		{
			if (IsCreated()) {
				return true;
			}

			SchemaData template = GetSchemaData();
			if (template == null) {
				return false;
			}

			try {
				SqliteCommand cmd = GetConnection().CreateCommand();
				cmd.CommandText = template.GetCreateText(GetSchemaName());
				cmd.ExecuteNonQuery();
			}
			catch (Exception ex) {
				Debug.Log(ex.Message);
				return false;
			}

			if (!CreateAfterInited()) {
				return false;
			}

			return InitSubSchemas();
		}

		/// <summary>
		/// 初始化子表结构.
		/// </summary>
		/// <returns><c>true</c>, if sub schemas was inited, <c>false</c> otherwise.</returns>
		protected virtual bool InitSubSchemas()
		{
			SchemaData template = GetSchemaData();
			if (template == null) {
				return false;
			}

			FieldInfo[] fis = template.GetType().GetFields();
			for (int j = 0; j < fis.Length; ++j) {
				FieldInfo fi = fis[j];

				MethodInfo miGetTypeName = fi.FieldType.GetMethod("GetTypeName");
				string typeName = (string)miGetTypeName.Invoke(fi.GetValue(template), null);
				if (!SchemaData.IsBuiltInType(typeName)) {
					PropertyInfo piValue = fi.FieldType.GetProperty("Value");
					Schema schema = (Schema)piValue.GetValue(fi.GetValue(template), null);	
					if (schema == null || !schema.Init()) {
						return false;
					}
				}
			}
			return true;
		}

		/// <summary>
		/// 初始化后调用该接口, 用于创建表的一些初始数据, 一般跟具体的玩家信息无关
		/// </summary>
		/// <returns><c>true</c>, if after inited was created, <c>false</c> otherwise.</returns>
		protected virtual bool CreateAfterInited()
		{
			return true;
		}

		/// <summary>
		/// 加载数据, 主要进行SELECT查询, 然后填充数据.
		/// </summary>
		/// <param name="objs">WHERE语句所需要的参数，第一个参数必须是UID</param>
		public virtual bool Load(params object[] objs)
		{
			if (m_datas.Count != 0) {
				return true;
			}

			SchemaData template = GetSchemaData();
			if (template == null) {
				return false;
			}

			try {
				SqliteCommand cmd = GetConnection().CreateCommand();
				cmd.CommandText = template.GetSelectText(GetSchemaName(), objs);
				SqliteDataReader dr = cmd.ExecuteReader();
				if (dr.HasRows) {
					while (dr.Read()) {
						SchemaData data = GetSchemaData();
						if (!data.SetValues(dr)) {
							return false;
						}
						data.Status = SchemaData.StatusType.Normal;
						m_datas.Add(data);
					}
				}
				else {
					if (!CreateAfterNotFound(objs)) {
						return false;	
					}
				}	
			}
			catch (Exception ex) {
				Debug.Log(ex.Message);
				return false;
			}

			return true;
		}

		/// <summary>
		/// 查找表数据，在表数据为空的情况下，调用该接口
		/// 子表的这个接口，只有在父表有数据，子表没有数据的情况下才会触发
		/// </summary>
		/// <param name="objs">Objects.</param>
		protected virtual bool CreateAfterNotFound(params object[] objs) 
		{
			return true;	
		}

		/// <summary>
		/// 保存数据.
		/// </summary>
		public virtual bool Save()
		{
			List<string> sqls = new List<string>();
			for (int i = 0; i < m_datas.Count; ++i) {
				SchemaData data = m_datas[i];
				if (data.Status == SchemaData.StatusType.New) {
					sqls.Add(data.GetInsertText(GetSchemaName()));	
				}
				else if (data.Status == SchemaData.StatusType.Delete) {
					sqls.Add(data.GetUpdateText(GetSchemaName()));
				}
				else {
					string sql = data.GetUpdateText(GetSchemaName());
					if (sql.Length != 0) {
						sqls.Add(sql);
					}
				}
			}

			// 处理SQLITE语句
			if (sqls.Count != 0) {
				SqliteCommand cmd = GetConnection().CreateCommand();
				SqliteTransaction tran = GetConnection().BeginTransaction();
				cmd.Transaction = tran;
				try {
					for (int i = 0; i < sqls.Count; ++i) {
						cmd.CommandText = sqls[i];
						cmd.ExecuteNonQuery();
					}
					tran.Commit();
				}
				catch (Exception ex) {
					Debug.Log(ex.Message);
					tran.Rollback();
					return false;
				}	

				for (int i = 0; i < m_datas.Count; ++i) {
					m_datas[i].ResetStatus();
				}
			}

			// 保存子表
			for (int i = 0; i < m_datas.Count; ++i) {
				SchemaData data = m_datas[i];
				FieldInfo[] fis = data.GetType().GetFields();
				for (int j = 0; j < fis.Length; ++j) {
					FieldInfo fi = fis[j];

					MethodInfo miGetTypeName = fi.FieldType.GetMethod("GetTypeName");
					string typeName = (string)miGetTypeName.Invoke(fi.GetValue(data), null);
					if (!SchemaData.IsBuiltInType(typeName)) {
						PropertyInfo piValue = fi.FieldType.GetProperty("Value");
						Schema schema = (Schema)piValue.GetValue(fi.GetValue(data), null);	
						if (schema == null || !schema.Save()) {
							return false;
						}
					}
				}
			}

			return true;
		}

		/// <summary>
		/// 转成JSON数据. 该版本只对内容进行常规输出，如果输出内容不满足要求，重载该方法.
		/// </summary>
		/// <returns>The json.</returns>
		public virtual string ToJson()
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < m_datas.Count; ++i) {
				SchemaData data = m_datas[i];

				StringBuilder tmp = new StringBuilder();
				FieldInfo[] fis = data.GetType().GetFields();
				for (int j = 0; j < fis.Length; ++j) {
					FieldInfo fi = fis[j];
					if (!EnableJsonNode(fi.Name)) {
						continue;
					}

					if (tmp.Length != 0) {
						tmp.Append(",");
					}

					tmp.AppendFormat("\"{0}\":", JsonNodeName(Schema.ToJsonNodePattern(fi.Name)));

					PropertyInfo piValue = fi.FieldType.GetProperty("Value");
					object value = piValue.GetValue(fi.GetValue(data), null);

					MethodInfo miGetTypeName = fi.FieldType.GetMethod("GetTypeName");
					string typeName = (string)miGetTypeName.Invoke(fi.GetValue(data), null);
					if (SchemaData.IsBuiltInType(typeName)) {
						tmp.AppendFormat("{0}", JsonNodeValue(fi.Name, typeName, value.ToString()));
					}
					else {
						Schema schema = (Schema)value;
						tmp.AppendFormat("{0}", schema.ToJson());
					}
				}
				tmp.Insert(0, "{");
				tmp.Append("}");

				if (builder.Length != 0) {
					builder.Append(",");
				}
				builder.Append(tmp.ToString());

				if (SingleJsonNode()) {
					break;	
				}
			}

			if (!SingleJsonNode()) {
				builder.Insert(0, "[");
				builder.Append("]");
			}
			return builder.ToString();
		}

		/// <summary>
		/// 是否输出'noeeName'节点. 如果不想输出'nodeName'节点，返回false.
		/// </summary>
		/// <returns><c>true</c>, if json node was enabled, <c>false</c> otherwise.</returns>
		/// <param name="title">Title.</param>
		protected virtual bool EnableJsonNode(string nodeName)
		{
			return true;
		}

		/// <summary>
		/// 输出的JSON节点名称，如里需要更改某个输出节点，重载该方法.
		/// </summary>
		/// <returns>The node name.</returns>
		/// <param name="nodeName">Node name.</param>
		protected virtual string JsonNodeName(string nodeName)
		{
			return nodeName;
		}

		/// <summary>
		/// 输出的JSON的节点的VALUE. 如果需要更改节点内容的输出，重载该方法
		/// </summary>
		/// <returns>The node value.</returns>
		/// <param name="nodeName">Node name.</param>
		/// <param name="nodeValue">Node value.</param>
		protected virtual string JsonNodeValue(string nodeName, string nodeType, string nodeValue)
		{
			if (nodeType == typeof(string).Name) {
				return String.Format("\"{0}\"", nodeValue);
			}
			else if (nodeType == typeof(bool).Name) {
				return nodeValue == "True" ? "1" : "0";
			}
			return nodeValue;
		}

		/// <summary>
		/// 标识该表是否输出当节点, 有些表的纪录只有一条, 这样的话, 重载该方法返回TRUE
		/// </summary>
		/// <returns><c>true</c>, if json node was singled, <c>false</c> otherwise.</returns>
		protected virtual bool SingleJsonNode()
		{
			return false;
		}
			
		/// <summary>
		/// 清空数据.
		/// </summary>
		protected virtual void Clear() 
		{
			m_datas.Clear();	
		}

		/// <summary>
		/// 对应的SQLITE中的表是否创建
		/// </summary>
		/// <returns><c>true</c> if this instance is created; otherwise, <c>false</c>.</returns>
		protected bool IsCreated()
		{
			SqliteCommand cmd = GetConnection().CreateCommand();
			cmd.CommandText = String.Format("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{0}'", GetSchemaName());
			return Convert.ToInt32(cmd.ExecuteScalar()) != 0;
		}

		/// <summary>
		/// 获取对应的表名，JSON风格(hero_item, shop_item)
		/// </summary>
		/// <returns>The json schema name.</returns>
		public string GetJsonSchemaName()
		{
			return Schema.ToJsonNodePattern(GetSchemaName());
		}

		/// <summary>
		/// 对应的SQLITE的表名
		/// </summary>
		/// <returns>The schema name.</returns>
		public string GetSchemaName()
		{
			string className = this.GetType().Name;
			if (className.Length > typeof(Schema).Name.Length) {
				return className.Substring(0, className.Length - 6);
			}
			return "unknown";
		}

		/// <summary>
		/// 获取数据库连接对象
		/// </summary>
		/// <returns>The connection.</returns>
		protected SqliteConnection GetConnection()
		{
			return ServerContext.GetInstance().GetConnection();
		}

		/// <summary>
		/// 转换命名规范：将'ShopHeroItem'=>'shop_hero_item'
		/// </summary>
		/// <returns>The json pattern .</returns>
		/// <param name="name">Name.</param>
		public static string ToJsonNodePattern(string name)
		{
			string t = name;
			for (int i = 0; i < t.Length; ++i) {
				if (i != 0 && t[i] >= 'A' && t[i] <= 'Z') {
					t = t.Insert(i++, "_");
				}
			}
			return t.ToLower();
		}

		/// <summary>
		/// 获取首条数据
		/// </summary>
		/// <returns>The first data.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		protected T GetFirstData<T>() where T : SchemaData
		{
			return GetData<T>(0);
		}

		/// <summary>
		/// 获取最后一条数据
		/// </summary>
		/// <returns>The last data.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		protected T GetLastData<T>() where T : SchemaData
		{
			return GetData<T>(m_datas.Count - 1);
		}

		/// <summary>
		/// 根据索引，获取指定下标的数据
		/// </summary>
		/// <returns>The data.</returns>
		/// <param name="index">Index.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		protected T GetData<T>(int index) where T : SchemaData
		{
			if (index < 0 || index >= m_datas.Count) {
				return null;
			}
			return (T)m_datas[index];
		}

		/// <summary>
		/// 获取所有数据
		/// </summary>
		/// <returns>The all data.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		protected List<T> GetAllData<T>() where T : SchemaData
		{
			List<T> t = new List<T>();
			for (int i = 0; i < m_datas.Count; ++i) {
				t.Add((T)m_datas[i]);	
			}
			return t;
		}
	}	
}

