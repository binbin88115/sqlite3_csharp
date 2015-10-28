using System;
using System.Text;
using UnityEngine;
using System.Collections;

using Mono.Data.Sqlite;

namespace TinyServer.Schemas
{
	public class AccountSchema : Schema
	{
		public class AccountData : SchemaData
		{
			public ColumnType<string> AccountName = new ColumnType<string>();
			public ColumnType<int> UserId = new ColumnType<int>();
		}

		/// <summary>
		/// 查找表数据，在表数据为空的情况下，调用该接口
		/// </summary>
		/// <param name="objs">Objects.</param>
		protected override bool CreateAfterNotFound(params object[] objs)
		{
			// 这里测试，在找不到这个帐号的情况下，会创建一个新帐号
			if (objs.Length != 0) {
				AccountData data = new AccountData();
				data.AccountName.Value = objs[0].ToString();
				data.UserId.Value = GetUsableUserId();
				DataList.Add(data);

				if (Save()) {
					return true;
				}
			}
			return false;
		}

		protected override SchemaData GetSchemaData()
		{
			return new AccountData();
		}

		/// <summary>
		/// 获取用户ID
		/// </summary>
		/// <returns>The user identifier.</returns>
		public int GetUserId()
		{
			AccountData d = GetFirstData<AccountData>();	
			if (d != null) {
				return d.UserId.Value;
			}
			return 0;
		}

		protected int GetUsableUserId()
		{
			SqliteCommand cmd = GetConnection().CreateCommand();
			cmd.CommandText = System.String.Format("SELECT COUNT(*) FROM {0}", GetSchemaName());
			return Convert.ToInt32(cmd.ExecuteScalar()) + 10000;
		}
	}
}
