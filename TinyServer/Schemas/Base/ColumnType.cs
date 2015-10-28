using System;
using UnityEngine;

namespace TinyServer.Schemas
{
	public class ColumnType<T>
	{
		private T m_value;
		private bool m_changed = false;
		private bool m_inited = false;
		private bool m_key = false;

		public ColumnType()
		{
			AssignDefaultValue();
		}

		public ColumnType(bool key)
		{
			m_key = key;
			AssignDefaultValue();
		}

		/// <summary>
		/// 数据
		/// </summary>
		/// <value>The value.</value>
		public T Value
		{
			set {
				m_value = value; 
				m_changed = true;	
			}
			get { return m_value; }
		}

		/// <summary>
		/// 该字段是否更改过
		/// </summary>
		/// <value><c>true</c> if changed; otherwise, <c>false</c>.</value>
		public bool Changed
		{
			set { m_changed = value; }
			get { return m_changed; }
		}

		/// <summary>
		/// 该字段是否初始化过
		/// </summary>
		/// <value><c>true</c> if inited; otherwise, <c>false</c>.</value>
		public bool Inited
		{
			get { return m_inited; }
		}

		/// <summary>
		/// 该字段是否是主键
		/// </summary>
		/// <value><c>true</c> if key; otherwise, <c>false</c>.</value>
		public bool Key
		{
			get { return m_key; }
		}

		/// <summary>
		/// 该字段的类型
		/// </summary>
		/// <returns>The type name.</returns>
		public string GetTypeName()
		{
			return typeof(T).Name;
		}

		/// <summary>
		/// 初始化数据
		/// </summary>
		/// <param name="value">Value.</param>
		public void InitValue(T value)
		{
			System.Type type = typeof(T);
			if (type.IsPrimitive || type.Name == typeof(string).Name) {
				m_value = value;
				m_inited = true;
			}
		}

		/// <summary>
		/// 初始化T的默认数据
		/// </summary>
		private void AssignDefaultValue()
		{
			if (IsBuiltInType()) {
				m_value = default(T);
			}
			else {
				m_value = System.Activator.CreateInstance<T>();
			}
		}

		/// <summary>
		/// T是否是内置类型
		/// </summary>
		/// <returns><c>true</c> if this instance is built in type; otherwise, <c>false</c>.</returns>
		private bool IsBuiltInType()
		{
			System.Type type = typeof(T);
			if (type.IsPrimitive || type.Name == typeof(string).Name) {
				return true;
			}	
			return false;
		}
	}	
}
