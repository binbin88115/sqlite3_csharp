using System;
using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using Mono.Data.Sqlite;

namespace TinyServer.Schemas
{
	public class UserSchema : Schema
	{
		public class UserData : SchemaData
		{
			// 传入true参数，标识这个字段在SQLITE语句中充当WHERE语句的参数
			public ColumnType<int> Uid = new ColumnType<int>(true);
			public ColumnType<int> Age = new ColumnType<int>();
			public ColumnType<string> Name = new ColumnType<string>();
			public ColumnType<string> Address = new ColumnType<string>();
			public ColumnType<bool> Sex = new ColumnType<bool>();
		}

		/// <summary>
		/// 查找表数据，在表数据为空的情况下，调用该接口
		/// </summary>
		/// <param name="objs">Objects.</param>
		protected override bool CreateAfterNotFound(params object[] objs)
		{
			if (objs.Length != 0) {
				UserData d = new UserData();
				d.Uid.Value = (int)objs[0];
				d.Age.Value = 10;
				d.Name.Value = "lansey";
				d.Address.Value = "FuZhou";
				d.Sex.Value = true;
				DataList.Add(d);

				if (Save()) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 返回该表的数据结构对象
		/// </summary>
		/// <returns>The schema data.</returns>
		protected override SchemaData GetSchemaData()
		{
			return new UserData();
		}

		/// <summary>
		/// 因为这张表是User表，而User表一个玩家一条记录，因为JSON输出单条就OK
		/// </summary>
		/// <returns>true</returns>
		/// <c>false</c>
		protected override bool SingleJsonNode()
		{
			return true;
		}

		/// <summary>
		/// 获取用户的年龄
		/// </summary>
		/// <returns>The age.</returns>
		public int GetAge()
		{
			UserData d = GetFirstData<UserData>();
			if (d != null) {
				return d.Age.Value;
			}
			return 0;
		}

		/// <summary>
		/// 设置用户的年龄
		/// </summary>
		/// <param name="age">Age.</param>
		public void SetAge(int age)
		{
			UserData d = GetFirstData<UserData>();
			if (d != null) {
				d.Age.Value = age;
			}
		}
	}	
}
