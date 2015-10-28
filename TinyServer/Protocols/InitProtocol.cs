using UnityEngine;
using System.Collections;

using TinyServer;
using TinyServer.Schemas;

namespace TinyServer.Protocols
{
	public class InitProtocol : Protocol
	{
		protected override string OnHandle()
		{
			string name = GetQuery<string>("account_name");	
			if (name.Length == 0) {
				return ToJson(500);
			}

			AccountSchema account = GetSchema<AccountSchema>(name);
			if (account == null) {
				return ToJson(500);
			}

			// 获取UID
			int uid = account.GetUserId();
			if (uid == 0) {
				return ToJson(500);
			}

			UserSchema user = GetSchema<UserSchema>(uid);
			if (user == null) {
				return ToJson(500);
			}

			ShopSchema shop = GetSchema<ShopSchema>(uid);
			if (shop == null) {
				return ToJson(500);
			}

			return ToJson(200, ModuleType.kUser | ModuleType.kShop);
		}
	}
}