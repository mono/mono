// 
// System.Web.Configuration.ModuleItem
//
// Author:
//   Patrik Torstensson (ptorsten@hotmail.com)
//
using System;

namespace System.Web.Configuration {
	public class ModuleItem {
		private Type _type;
		private string _typeName;
		private string _name;

		public ModuleItem(string name, string type) {
			_typeName = type;
			_name = name;

			_type = Type.GetType (type, true);
			if (!typeof(IHttpModule).IsAssignableFrom(_type))
				throw new HttpException(HttpRuntime.FormatResourceString("type_not_module"));
		}

		public ModuleItem(string name, Type type) {
			_typeName = type.ToString ();
			_name = name;
			_type = type;
			if (!typeof(IHttpModule).IsAssignableFrom(_type))
				throw new HttpException(HttpRuntime.FormatResourceString("type_not_module"));
		}

		public IHttpModule Create() {
			return (IHttpModule) HttpRuntime.CreateInternalObject(_type);
		}

		public Type Type {
			get {
				return _type;
			}
		}

		public bool IsMatch (string name)
		{
			return (_type.Name == name || _type.FullName == name);
		}

		public string ModuleName {
			get {
				return _name;
			}
		}
	}
}
