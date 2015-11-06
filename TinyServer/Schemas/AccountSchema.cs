using System;
using System.Text;
using UnityEngine;
using System.Collections;

using Mono.Data.Sqlite;

namespace TinyServer.Schemas
{
	public class AccountSchema : SchemaHelper<AccountSchema.Data>
	{
		public class Data : SchemaData
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
				Data data = new Data();
				data.AccountName.Value = objs[0].ToString();
				data.UserId.Value = GetUsableUserId();
				DataList.Add(data);

				if (Save()) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 获取用户ID
		/// </summary>
		/// <returns>The user identifier.</returns>
		public int GetUserId()
		{
			Data d = GetFirstData();	
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
