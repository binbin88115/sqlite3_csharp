using System;
using UnityEngine;
using Mono.Data.Sqlite;

namespace TinyServer
{
	public class ServerContext
	{
		private SqliteConnection m_connection = null;

		private static ServerContext s_instance = null;
		public static ServerContext GetInstance()
		{
			if (s_instance == null) {
				s_instance = new ServerContext();	
			}	
			return s_instance;
		}

		protected ServerContext()
		{
		}

		/// <summary>
		/// 连接SQLITE数据库
		/// </summary>
		public void BeginConnection()
		{
			if (m_connection == null) {
				SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder();
				builder.DataSource = Application.persistentDataPath + "/" + GetDatabaseName();
				Debug.Log(String.Format("Database Path: {0}", builder.DataSource));

				m_connection = new SqliteConnection(builder.ConnectionString);
				m_connection.Open();
			}
		}

		/// <summary>
		/// 断开SQLITE数据库的连接.
		/// </summary>
		public void EndConnection()
		{
			if (m_connection != null) {
				m_connection.Close();	
				m_connection = null;
			}
		}

		/// <summary>
		/// SQLITE连接对象.
		/// </summary>
		/// <returns>The connection.</returns>
		public SqliteConnection GetConnection()
		{
			return m_connection;
		}

		/// <summary>
		/// SQLITE数据库名称
		/// </summary>
		/// <returns>The database name.</returns>
		private string GetDatabaseName()
		{
			return "data.db";
		}
	}	
}
