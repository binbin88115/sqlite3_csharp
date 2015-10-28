using UnityEngine;
using System.Text;
using System.Collections;

using TinyServer.Schemas;

namespace TinyServer
{
	public static class ModuleType
	{
		public const int kUnknown = 0;
		public const int kUser = 1 << 0;
		public const int kShop = 1 << 1;

		public const int kAll  = 0xff;
	}

	public class ModuleManager
	{
		private static ModuleManager s_instance = null;
		public static ModuleManager GetInstance()
		{
			if (s_instance == null) {
				s_instance = new ModuleManager();
			}
			return s_instance;
		}

		/// <summary>
		/// 根据FLAGS获取相关表的JSON输出
		/// </summary>
		/// <returns>The modules.</returns>
		/// <param name="flags">Flags.</param>
		public string GetModules(int flags)
		{
			StringBuilder builder = new StringBuilder();
			int flag = 1;
			do {
				if ((flags & flag) != 0) {
					Schema schema = GetSchema(flag);
					if (schema != null) {
						if (builder.Length != 0) {
							builder.Append(",");
						}
						builder.AppendFormat("\"{0}\":{1}", schema.GetJsonSchemaName(), schema.ToJson());
					}
				}
				flag <<= 1;
			} while (flag < 0xffff);

			builder.Insert(0, "{");
			builder.Append("}");
			return builder.ToString();
		}

		/// <summary>
		/// 根据FLAG获取表对象
		/// </summary>
		/// <returns>The schema.</returns>
		/// <param name="flag">Flag.</param>
		public Schema GetSchema(int flag)
		{
			Schema schema = null;
			switch (flag) {
			case ModuleType.kUser:
				schema = SchemaManager.GetInstance().GetSchemaForJson<UserSchema>();
				break;
			case ModuleType.kShop:
				schema = SchemaManager.GetInstance().GetSchemaForJson<ShopSchema>();
				break;
			default:
				break;
			}
			return schema;
		}
	}
}