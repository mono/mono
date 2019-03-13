using System;
using System.Collections.Generic;
using System.Text;

namespace System.Web.Script.Services {
	public static class MonoProxyGenerator {
		public static string GetClientProxyScript (Type type, string path, bool debug) {
			return LogicalTypeInfo.GetLogicalTypeInfo(type, path).Proxy;
		}
	}
}