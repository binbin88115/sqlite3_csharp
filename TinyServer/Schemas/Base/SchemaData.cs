using System;
using System.Text;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

using Mono.Data.Sqlite;

namespace TinyServer.Schemas
{
	public class SchemaData
	{
		public enum StatusType
		{
			Normal,
			New,
			Delete
		}

		private StatusType m_status = StatusType.New;

		/// <summary>
		/// NOTE: 用于设置字段的初始值，该初始值会设置为表字段的默认值
		/// </summary>
		public SchemaData()
		{
		}

		/// <summary>
		/// 数据所处的状态
		/// </summary>
		/// <value>The status.</value>
		public StatusType Status
		{
			set { m_status = value; }
			get { return m_status; }
		}

		/// <summary>
		/// 重置状态
		/// </summary>
		public void ResetStatus()
		{
			FieldInfo[] fis = GetFields();
			for (int i = 0; i < fis.Length; ++i) {
				FieldInfo fi = fis[i];
				PropertyInfo pi = fi.FieldType.GetProperty("Changed");
				pi.SetValue(fi.GetValue(this), false, null);
			}
			m_status = StatusType.Normal;
		}

		/// <summary>
		/// 通过SqliteDataReader对象来初始化数据
		/// </summary>
		/// <param name="dr">Dr.</param>
		public bool SetValues(SqliteDataReader dr)
		{
			FieldInfo[] fis = GetFields();
			for (int i = 0; i < fis.Length; ++i) {
				FieldInfo fi = fis[i];

				MethodInfo miInitValue = fi.FieldType.GetMethod("InitValue");
				MethodInfo miGetTypeName = fi.FieldType.GetMethod("GetTypeName");
				string typeName = (string)miGetTypeName.Invoke(fi.GetValue(this), null);
				if (typeName == typeof(string).Name) {
					miInitValue.Invoke(fi.GetValue(this), new object[] { dr[fi.Name].ToString() });
				}
				else if (typeName == typeof(byte).Name) {
					miInitValue.Invoke(fi.GetValue(this), new object[] { Convert.ToByte(dr[fi.Name]) });
				}
				else if (typeName == typeof(short).Name) {
					miInitValue.Invoke(fi.GetValue(this), new object[] { Convert.ToInt16(dr[fi.Name]) });
				}
				else if (typeName == typeof(int).Name) {
					miInitValue.Invoke(fi.GetValue(this), new object[] { Convert.ToInt32(dr[fi.Name]) });
				}
				else if (typeName == typeof(long).Name) {
					miInitValue.Invoke(fi.GetValue(this), new object[] { Convert.ToInt64(dr[fi.Name]) });
				}
				else if (typeName == typeof(bool).Name) {
					miInitValue.Invoke(fi.GetValue(this), new object[] { Convert.ToBoolean(dr[fi.Name]) });
				}
				else if (typeName == typeof(float).Name) {
					miInitValue.Invoke(fi.GetValue(this), new object[] { float.Parse(dr[fi.Name].ToString()) });
				}
				else if (typeName == typeof(double).Name) {
					miInitValue.Invoke(fi.GetValue(this), new object[] { double.Parse(dr[fi.Name].ToString()) });	
				}
				else {
					bool ret = SetValueForCustomType(typeName, dr);
					if (!ret) {
						return false;
					}
				}
			}
			return true;
		}

		/// <summary>
		/// 为自定义类型赋值
		/// </summary>
		/// <param name="typeName">自定义的类型名称</param>
		/// <param name="dr">Dr.</param>
		protected virtual bool SetValueForCustomType(string typeName, SqliteDataReader dr)
		{
			return true;
		}

		public bool IsModifiedSchema()
		{
			FieldInfo[] fis = GetFields();
			for (int i = 0; i < fis.Length; ++i) {
				FieldInfo fi = fis[i];
				PropertyInfo pi = fi.FieldType.GetProperty("Inited");
				bool initied = (bool)pi.GetValue(fi.GetValue(this), null);
				if (!initied) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 获取所有成员变量，访问权限为PUBLIC
		/// </summary>
		/// <returns>The fields.</returns>
		private System.Reflection.FieldInfo[] GetFields()
		{
			return this.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
		}

		/// <summary>
		/// 获取SQL中创建表的语句
		/// </summary>
		/// <returns>The create text.</returns>
		/// <param name="schemaName">表名</param>
		public string GetCreateText(string schemaName)
		{
			StringBuilder builder = new StringBuilder();
			FieldInfo[] fis = GetFields();
			for (int i = 0; i < fis.Length; ++i) {
				FieldInfo fi = fis[i];

				MethodInfo miGetTypeName = fi.FieldType.GetMethod("GetTypeName");
				string typeName = (string)miGetTypeName.Invoke(fi.GetValue(this), null);
				if (!SchemaData.IsBuiltInType(typeName)) {
					continue;
				}

				if (builder.Length != 0) {
					builder.Append(",");
				}

				PropertyInfo piValue = fi.FieldType.GetProperty("Value");
				object value = piValue.GetValue(fi.GetValue(this), null);
				string dbValue  = "";
				if (value != null) {
					dbValue = value.ToString();
				}

				builder.AppendFormat("'{0}' {1} NOT NULL DEFAULT {2}", fi.Name, SchemaData.GetDbType(typeName),
					SchemaData.GetDbDefaultValue(typeName, dbValue));
			}

			builder.Insert(0, String.Format("CREATE TABLE IF NOT EXISTS '{0}' (", schemaName));
			builder.Append(")");
			return builder.ToString();
		}

		/// <summary>
		/// 获取SQL中的SELECT语句
		/// </summary>
		/// <returns>The select text.</returns>
		/// <param name="schemaName">表名</param>
		/// <param name="objs">查询所需要的参数，用于WHERE语句</param>
		public string GetSelectText(string schemaName, params object[] objs)
		{
			return String.Format("SELECT * FROM '{0}' {1}", schemaName, GetWhereClause(objs));
		}

		/// <summary>
		/// 获取SQL的INSERT语句
		/// </summary>
		/// <returns>The insert text.</returns>
		/// <param name="schemaName">表名</param>
		public string GetInsertText(string schemaName)
		{
			StringBuilder builder = new StringBuilder();
			FieldInfo[] fis = GetFields();
			for (int i = 0; i < fis.Length; ++i) {
				FieldInfo fi = fis[i];

				MethodInfo miGetTypeName = fi.FieldType.GetMethod("GetTypeName");
				string typeName = (string)miGetTypeName.Invoke(fi.GetValue(this), null);
				if (!SchemaData.IsBuiltInType(typeName)) {
					continue;
				}

				PropertyInfo piValue = fi.FieldType.GetProperty("Value");
				object value = piValue.GetValue(fi.GetValue(this), null);

				string dbValue  = "";
				if (value != null) {
					dbValue = value.ToString();
				}

				if (builder.Length != 0) {
					builder.Append(",");
				}

				string dbType = SchemaData.GetDbType(typeName);
				if (dbType == "TEXT") {
					builder.AppendFormat("'{0}'", SchemaData.GetDbValue(typeName, dbValue));
				}
				else {
					builder.AppendFormat("{0}", SchemaData.GetDbValue(typeName, dbValue));
				}
			}
			builder.Insert(0, String.Format("INSERT INTO '{0}' VALUES(", schemaName));
			builder.Append(")");
			return builder.ToString();
		}

		/// <summary>
		/// 获取Sql的Update语句，因为主键不可更改，所以无需传入主键参数
		/// </summary>
		/// <returns>The update text.</returns>
		/// <param name="schemaName">Schema name.</param>
		public string GetUpdateText(string schemaName)
		{
			StringBuilder builder = new StringBuilder();
			FieldInfo[] fis = GetFields();
			for (int i = 0; i < fis.Length; ++i) {
				FieldInfo fi = fis[i];

				PropertyInfo piChanged = fi.FieldType.GetProperty("Changed");
				bool changed = (bool)piChanged.GetValue(fi.GetValue(this), null);
				if (!changed) {
					continue;
				}

				PropertyInfo piKey = fi.FieldType.GetProperty("Key");
				bool key = (bool)piKey.GetValue(fi.GetValue(this), null);
				if (key) {
					continue;
				}
					
				MethodInfo miGetTypeName = fi.FieldType.GetMethod("GetTypeName");
				string typeName = (string)miGetTypeName.Invoke(fi.GetValue(this), null);

				PropertyInfo piValue = fi.FieldType.GetProperty("Value");
				object value = piValue.GetValue(fi.GetValue(this), null);

				string dbValue  = "";
				if (value != null) {
					dbValue = value.ToString();
				}

				if (builder.Length != 0) {
					builder.Append(",");
				}

				string dbType = SchemaData.GetDbType(typeName);
				if (dbType == "TEXT") {
					builder.AppendFormat(" '{0}'='{1}'", fi.Name, SchemaData.GetDbValue(typeName, dbValue));
				}
				else {
					builder.AppendFormat(" '{0}'={1}", fi.Name, SchemaData.GetDbValue(typeName, dbValue));
				}
			}

			if (builder.Length == 0) {
				return "";
			}
			builder.Insert(0, String.Format("UPDATE '{0}' SET ", schemaName));
			builder.Append(GetWhereClause());
			return builder.ToString();
		}

		/// <summary>
		/// 获取SQLITE的删除语句
		/// </summary>
		/// <returns>The delete text.</returns>
		/// <param name="schemaName">Schema name.</param>
		public string GetDeleteText(string schemaName)
		{
			return String.Format("DELETE FROM '{0}' {1}", schemaName, GetWhereClause());	
		}

		/// <summary>
		/// 获取Sql语句的Where语句
		/// </summary>
		/// <returns>The where clause.</returns>
		protected string GetWhereClause()
		{
			StringBuilder builder = new StringBuilder();
			FieldInfo[] fis = GetFields();
			for (int i = 0; i < fis.Length; ++i) {
				FieldInfo fi = fis[i];

				PropertyInfo piKey = fi.FieldType.GetProperty("Key");
				bool key = (bool)piKey.GetValue(fi.GetValue(this), null);
				if (key) {
					// 主键类型只能为INT
					PropertyInfo piValue = fi.FieldType.GetProperty("Value");
					int value = (int)piValue.GetValue(fi.GetValue(this), null);

					if (builder.Length != 0) {
						builder.Append(" AND ");
					}
					builder.AppendFormat(" {0}={1}", fi.Name, value);
				}
			}
			if (builder.Length != 0) {
				builder.Insert(0, " WHERE ");	
			}
			return builder.ToString();	
		}

		/// <summary>
		/// 获取Sql语句的Where语句
		/// </summary>
		/// <returns>The where clause.</returns>
		/// <param name="objs">Where参数</param>
		protected string GetWhereClause(params object[] objs)
		{
			StringBuilder builder = new StringBuilder();
			int index = 0;

			FieldInfo[] fis = GetFields();
			for (int i = 0; i < fis.Length; ++i) {
				FieldInfo fi = fis[i];

				PropertyInfo piKey = fi.FieldType.GetProperty("Key");
				bool key = (bool)piKey.GetValue(fi.GetValue(this), null);
				if (key) {
					if (builder.Length != 0) {
						builder.Append(" AND");
					}
					builder.AppendFormat(" {0}={1}", fi.Name, objs[index++]);
				}
			}
			if (builder.Length != 0) {
				builder.Insert(0, " WHERE ");	
			}
			return builder.ToString();	
		}

		/// <summary>
		/// 通过代码类型来获取SQLITE数据类型
		/// </summary>
		/// <returns>The db type.</returns>
		/// <param name="codeType">代码类型，如float, int, double, string等</param>
		public static string GetDbType(string codeType)
		{
			if (codeType == typeof(byte).Name
				|| codeType == typeof(short).Name
				|| codeType == typeof(int).Name
				|| codeType == typeof(long).Name
				|| codeType == typeof(bool).Name) {
				return "INTEGER";
			}
			else if (codeType == typeof(float).Name
				|| codeType == typeof(double).Name) {
				return "REAL";
			}
			return "TEXT";
		}

		/// <summary>
		/// 代码数据转换成SQLITE的数据
		/// </summary>
		/// <returns>The db value.</returns>
		/// <param name="codeType">代码类型</param>
		/// <param name="codeValue">Code value.</param>
		public static string GetDbValue(string codeType, string codeValue)
		{
			if (codeType == typeof(byte).Name
				|| codeType == typeof(short).Name
				|| codeType == typeof(int).Name
				|| codeType == typeof(long).Name) {
				return codeValue;
			}
			else if (codeType == typeof(bool).Name) {
				return codeValue == "True" ? "1" : "0";
			}
			return codeValue;
		}

		/// <summary>
		/// 各类型的默认数据
		/// </summary>
		/// <returns>The db default value.</returns>
		/// <param name="codeType">代码类型</param>
		public static string GetDbDefaultValue(string codeType, string codeValue)
		{
			if (codeType == typeof(byte).Name
				|| codeType == typeof(short).Name
				|| codeType == typeof(int).Name
				|| codeType == typeof(long).Name
				|| codeType == typeof(float).Name
				|| codeType == typeof(double).Name) {
				return codeValue;
			}
			else if (codeType == typeof(bool).Name) {
				return codeValue == "True" ? "1" : "0";
			}
			return String.Format("'{0}'", codeValue);
		}

		/// <summary>
		/// Determines whether this instance is built in type the specified typeName.
		/// </summary>
		/// <returns><c>true</c> if this instance is built in type the specified typeName; otherwise, <c>false</c>.</returns>
		/// <param name="typeName">Type name.</param>
		public static bool IsBuiltInType(string typeName)
		{
			if (typeName == typeof(byte).Name
				|| typeName == typeof(short).Name
				|| typeName == typeof(int).Name
				|| typeName == typeof(long).Name
				|| typeName == typeof(float).Name
				|| typeName == typeof(double).Name
				|| typeName == typeof(bool).Name
				|| typeName == typeof(string).Name) {
				return true;
			}
			return false;
		}
	}
}

