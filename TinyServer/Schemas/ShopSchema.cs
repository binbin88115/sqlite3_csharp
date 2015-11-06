using System;
using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using Mono.Data.Sqlite;

using TinyServer.Schemas;

namespace TinyServer.Schemas
{
	public class ShopSchema : SchemaHelper<ShopSchema.Data>
	{
		public class Data : SchemaData
		{
			public ColumnType<int> Uid = new ColumnType<int>(true);
			public ColumnType<int> ShopType = new ColumnType<int>();
			public ColumnType<int> OpenTime = new ColumnType<int>();
			public ColumnType<ShopItemSchema> Items = new ColumnType<ShopItemSchema>();	

			/// <summary>
			/// 为ShopItemSchema赋值
			/// </summary>
			/// <param name="typeName">自定义的类型名称</param>
			/// <param name="dr">Dr.</param>
			/// <returns><c>true</c>, if value for custom type was set, <c>false</c> otherwise.</returns>
			protected override bool SetValueForCustomType(string typeName, SqliteDataReader dr)
			{
				if (typeName == typeof(ShopItemSchema).Name) {
					int uid = dr.GetInt32(0);
					int shopType = dr.GetInt32(1);
					if (!Items.Value.Load(uid, shopType)) {
						return false;
					}
				}
				else {
					// TODO: ...
				}
				return true;
			}
		}

		/// <summary>
		/// 不输出Uid节点
		/// </summary>
		/// <returns>true</returns>
		/// <c>false</c>
		/// <param name="title">Title.</param>
		/// <param name="nodeName">Node name.</param>
		protected override bool EnableJsonNode (string nodeName)
		{
			if (nodeName == "Uid") {
				return false;
			}	
			return true;
		}

		/// <summary>
		/// 查找表数据，在表数据为空的情况下，调用该接口
		/// </summary>
		/// <param name="objs">Objects.</param>
		protected override bool CreateAfterNotFound(params object[] objs)
		{
			if (objs.Length != 0) {
				Data d = new Data();
				d.Uid.Value = (int)objs[0];
				d.ShopType.Value = 1;
				d.OpenTime.Value = 1000;
				d.Items.Value.AddShopItem(d.Uid.Value, d.ShopType.Value, 100, 20, 12.5f);
				d.Items.Value.AddShopItem(d.Uid.Value, d.ShopType.Value, 101, 5, 23.2f);
				d.Items.Value.AddShopItem(d.Uid.Value, d.ShopType.Value, 102, 10, 108.7f);
				DataList.Add(d);

				if (Save()) {
					return true;
				}
			}
			return false;
		}
	}
}
