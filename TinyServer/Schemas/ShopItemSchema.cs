using System;
using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using Mono.Data.Sqlite;

namespace TinyServer.Schemas
{
	public class ShopItemSchema : SchemaHelper<ShopItemSchema.Data>
	{
		public class Data : SchemaData
		{
			public ColumnType<int> Uid = new ColumnType<int>(true);
			public ColumnType<int> ShopType = new ColumnType<int>(true);
			public ColumnType<int> ItemId = new ColumnType<int>();
			public ColumnType<int> Num = new ColumnType<int>();
			public ColumnType<float> Price = new ColumnType<float>();
		}

		/// <summary>
		///	不输出Uid, ShopType两节点
		/// </summary>
		/// <returns>true</returns>
		/// <c>false</c>
		/// <param name="title">Title.</param>
		/// <param name="nodeName">Node name.</param>
		protected override bool EnableJsonNode(string nodeName)
		{
			if (nodeName == "Uid" || nodeName == "ShopType") {
				return false;
			}
			return true;
		}

		public void AddShopItem(int uid, int shopType, int itemId, int num, float price)
		{
			Data d = new Data();
			d.Uid.Value = uid;
			d.ShopType.Value = shopType;
			d.ItemId.Value = itemId;
			d.Num.Value = num;
			d.Price.Value = price;
			DataList.Add(d);
		}
	}
}