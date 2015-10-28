using System;
using System.Text;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace TinyServer.Schemas
{
	public class SchemaManager
	{
		// 表集合
		private Dictionary<string, Schema> m_schemas = new Dictionary<string, Schema>();

		private static SchemaManager s_instance = null;
		public static SchemaManager GetInstance()
		{
			if (s_instance == null) {
				s_instance = new SchemaManager();
			}
			return s_instance;
		}

		/// <summary>
		/// 根据T获取表实例对象, 同时初始化表结构
		/// </summary>
		/// <returns>The schema.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T GetSchema<T>(params object[] objs) where T : Schema
		{
			Type type = typeof(T);
			if (m_schemas.ContainsKey(type.Name)) {
				T t = (T)m_schemas[type.Name];
				t.Update();
				return t;
			}

			T schema = Activator.CreateInstance<T>();
			if (schema != null && schema.Init() && schema.Load(objs)) {
				m_schemas.Add(type.Name, schema);
				schema.Update();
				return schema;
			}
			return null;
		}

		/// <summary>
		/// 获取表的实例对象，用于输出JSON
		/// </summary>
		/// <returns>The schema for json.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T GetSchemaForJson<T>() where T : Schema
		{
			Type type = typeof(T);
			if (m_schemas.ContainsKey(type.Name)) {
				return (T)m_schemas[type.Name];
			}	
			return null;
		}
	}
}