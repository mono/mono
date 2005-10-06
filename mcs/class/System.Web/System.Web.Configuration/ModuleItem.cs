// 
// System.Web.Configuration.ModuleItem
//
// Author:
//   Patrik Torstensson (ptorsten@hotmail.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;

namespace System.Web.Configuration {
	class ModuleItem {
		private Type _type;
		private string _name;

		public ModuleItem(string name, string type) {
			_name = name;

			_type = Type.GetType (type, true);
			if (!typeof(IHttpModule).IsAssignableFrom(_type))
				throw new HttpException(HttpRuntime.FormatResourceString("type_not_module"));
		}

		public ModuleItem(string name, Type type) {
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
