using System;
using System.Collections.Generic;

namespace TinyServer.Schemas
{
	/// <summary>
	/// 一个Schema的辅助类，用于提供一些非核心，但常用的功能 
	/// </summary>
	public class SchemaHelper<Td> : Schema where Td : SchemaData
	{
		/// <summary>
		/// 通过泛型来代替该函数的重载，省去麻烦 
		/// </summary>
		/// <returns>The schema data.</returns>
		protected override SchemaData GetSchemaData()
		{
			return System.Activator.CreateInstance<Td>();
		}

		/// <summary>
		/// 获取首条数据
		/// </summary>
		/// <returns>The first data.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		protected Td GetFirstData()
		{
			return GetData(0);
		}

		/// <summary>
		/// 获取最后一条数据
		/// </summary>
		/// <returns>The last data.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		protected Td GetLastData()
		{
			return GetData(m_datas.Count - 1);
		}

		/// <summary>
		/// 根据索引，获取指定下标的数据
		/// </summary>
		/// <returns>The data.</returns>
		/// <param name="index">Index.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		protected Td GetData(int index)
		{
			if (index < 0 || index >= m_datas.Count) {
				return null;
			}
			return (Td)m_datas[index];
		}

		/// <summary>
		/// 获取所有数据
		/// </summary>
		/// <returns>The all data.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		protected List<Td> GetAllData()
		{
			List<Td> t = new List<Td>();
			for (int i = 0; i < m_datas.Count; ++i) {
				t.Add((Td)m_datas[i]);	
			}
			return t;
		}
	}	
}

