using System;
using UnityEngine;
using System.Reflection;
using System.Collections;
using TinyServer.Protocols;

namespace TinyServer
{
	public class ServerService
	{
		private static ServerService s_instance = null;
		public static ServerService GetInstance()
		{
			if (s_instance == null) {
				s_instance = new ServerService();
			}
			return s_instance;
		}

		public void Start()
		{
			
		}

		public void Stop()
		{
			
		}

		/// <summary>
		/// 处理url请求.
		/// </summary>
		/// <param name="url">URL. 格式: 协议名称?参数=值&参数=值. 如: user_init?name=xbb&pwd=123456</param>
		public string Request(string url)
		{
			string[] s = url.Split("?".ToCharArray());
			if (s.Length == 2) {
				Protocol protocol = GetProtocol(s[0]);	
				if (protocol != null) {
					ServerContext.GetInstance().BeginConnection();
					string ret = protocol.Handle(s[1]);	
					ServerContext.GetInstance().EndConnection();
					return ret;
				}
			}
			return "{\"code\":9999}";
		}

		/// <summary>
		/// 根据协议名称或者协议对象
		/// </summary>
		/// <returns>The protocol.</returns>
		/// <param name="name">Name.</param>
		public Protocol GetProtocol(string name)
		{
			string fullName = String.Format("TinyServer.Protocols.{0}Protocol", ChangeProtocolName(name));
			return (Protocol)Assembly.GetExecutingAssembly().CreateInstance(fullName);
		}

		/// <summary>
		/// 修改协议名称格式，user_init->UserInit
		/// </summary>
		/// <returns>The protocol name.</returns>
		/// <param name="name">Name.</param>
		private string ChangeProtocolName(string name)
		{
			string ret = "";
			string[] t = name.Split("_".ToCharArray());
			for (int i = 0; i < t.Length; ++i) {
				string tt = t[i];
				ret += tt.Substring(0, 1).ToUpper() + tt.Substring(1);
			}
			return ret;
		}

	}
}
