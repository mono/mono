// 
// System.Web.Configuration.HandlerItem
//
// Author:
//   Patrik Torstensson (ptorsten@hotmail.com)
//
using System;

namespace System.Web.Configuration {
	public class HandlerItem {
		private Type _type;
		private string _typeName;
		private string _path;
		private string _requestType;

		public HandlerItem(string requestType, string path, string type) {
			_typeName = type;
			_path = path;
			_requestType = requestType.Replace(" ", "");

			_type = Type.GetType(type, true);
			if (!typeof(IHttpHandler).IsAssignableFrom(_type)) {
				if (!typeof(IHttpHandlerFactory).IsAssignableFrom(_type)) {
					throw new HttpException(HttpRuntime.FormatResourceString("type_not_factory_or_handler"));
				}
			}
		}

		public object Create() {
			return HttpRuntime.CreateInternalObject(_type);
		}

		public Type Type {
			get {
				return _type;
			}
		}

		[MonoTODO()]
		public bool IsMatch(string type, string path) {
			// Debugging
			return true;
		}
	}
}
