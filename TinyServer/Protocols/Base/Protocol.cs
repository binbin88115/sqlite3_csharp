using System;
using System.Text;
using System.Collections.Generic;

using TinyServer.Schemas;

namespace TinyServer.Protocols
{
	public abstract class Protocol
	{
		private Dictionary<string, object> m_queries = new Dictionary<string, object>();	
		
		/// <summary>
		/// 外面调用接口，将URL传入该接口中进行协议处理
		/// </summary>
		/// <param name="url">URL.</param>
		public string Handle(string url)
		{
			string[] t = url.Split("&".ToCharArray());
			for (int i = 0; i < t.Length; ++i) {
				string[] tt = t[i].Split("=".ToCharArray());
				if (tt.Length == 2) {
					m_queries[tt[0]] = tt[1];
				}
			}
			return OnHandle();
		}

		/// <summary>
		/// 子类能过重载该接口，实现对协议的处理
		/// </summary>
		protected abstract string OnHandle();

		/// <summary>
		/// 获取表对象
		/// </summary>
		/// <returns>The schema.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		protected T GetSchema<T>(params object[] objs) where T : Schema
		{
			return SchemaManager.GetInstance().GetSchema<T>(objs);
		}

		/// <summary>
		/// 获取URL的参数.
		/// </summary>
		/// <returns>The query.</returns>
		/// <param name="name">Name.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		protected T GetQuery<T>(string name)
		{
			if (m_queries.ContainsKey(name)) {
				return (T)m_queries[name];
			}	
			return default(T);
		}

		/// <summary>
		/// 生成JSON数据.
		/// </summary>
		/// <returns>The json.</returns>
		/// <param name="code">Code.</param>
		protected string ToJson(int code)
		{
			return String.Format("{{\"code\":{0}}}", code);	
		}

		/// <summary>
		/// 生成JSON数据.
		/// </summary>
		/// <returns>The json.</returns>
		/// <param name="code">Code.</param>
		/// <param name="flags">Flags.</param>
		protected string ToJson(int code, int flags)
		{
			return String.Format("{{\"code\":{0}, \"module\":{1}}}", code, ModuleManager.GetInstance().GetModules(flags));
		}

		/// <summary>
		/// 生成JSON数据.
		/// </summary>
		/// <returns>The json.</returns>
		/// <param name="code">Code.</param>
		/// <param name="result">Result.</param>
		protected string ToJson(int code, string result)
		{
			return String.Format("{{\"code\":{0}, \"result\":{1}}}", code, result);
		}

		/// <summary>
		/// 生成JSON数据.
		/// </summary>
		/// <returns>The json.</returns>
		/// <param name="code">Code.</param>
		/// <param name="flags">Flags.</param>
		/// <param name="result">Result.</param>
		protected string ToJson(int code, int flags, string result)
		{
			return String.Format("{{\"code\":{0}, \"module\":{1}, \"result\":{2}}}", code, ModuleManager.GetInstance().GetModules(flags), result);
		}
	}
}