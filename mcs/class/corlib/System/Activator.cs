//
// System.Activator.cs
//
// Author:
//   Nick Drochak II (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak II
//

using System.Runtime.Remoting;
using System.Reflection;
using System.Globalization;
using System.Security.Policy;

namespace System 
{
	// FIXME: This class is just stubs to get System.dll to compile
	public sealed class Activator
	{
		private Activator () {}

		[MonoTODO]
		public static ObjectHandle CreateComInstanceFrom(string assemblyName, 
			string typeName) { 
			throw new NotImplementedException(); 
		}

		[MonoTODO]
		public static ObjectHandle CreateInstanceFrom(string assemblyFile, 
			string typeName) { 
			throw new NotImplementedException(); 
		}

		[MonoTODO]
		public static ObjectHandle CreateInstanceFrom(string assemblyFile, 
			string typeName, object[] activationAttributes) { 
			throw new NotImplementedException(); 
		}
		
		[MonoTODO]
		public static ObjectHandle CreateInstanceFrom(string assemblyFile, 
			string typeName, bool ignoreCase, BindingFlags bindingAttr, 
			Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, 
			Evidence securityInfo) { 
			throw new NotImplementedException(); 
		}
		
		[MonoTODO]
		public static ObjectHandle CreateInstance(string assemblyName, 
			string typeName) { 
			throw new NotImplementedException(); 
		}
		
		[MonoTODO]
		public static ObjectHandle CreateInstance(string assemblyName, 
			string typeName, object[] activationAttributes) { 
			throw new NotImplementedException(); 
		}
		
		[MonoTODO]
		public static ObjectHandle CreateInstance(string assemblyName, 
			string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, 
			object[] args, CultureInfo culture, object[] activationAttributes, 
			Evidence securityInfo) { 
			throw new NotImplementedException(); 
		}
		
		[MonoTODO]
		public static object CreateInstance(Type type)  { throw new NotImplementedException(); }
		
		[MonoTODO]
		public static object CreateInstance(Type type, 
			object[] args) { throw new NotImplementedException(); }

		[MonoTODO]
		public static object CreateInstance(Type type, 
			object[] args, object[] activationAttributes) { 
			throw new NotImplementedException(); 
		}

		[MonoTODO]
		public static object CreateInstance(Type type, 
			BindingFlags bindingAttr, Binder binder, object[] args, 
			CultureInfo culture) { 
			throw new NotImplementedException(); 
		}

		[MonoTODO]
		public static object CreateInstance(Type type, 
			BindingFlags bindingAttr, Binder binder, object[] args, 
			CultureInfo culture, object[] activationAttributes) { 
			throw new NotImplementedException(); 
		}

		[MonoTODO]
		public static object CreateInstance(Type type, 
			bool nonPublic) { 
			throw new NotImplementedException(); 
		}

		[MonoTODO]
		public static object GetObject(Type type, 
			string url) { 
			throw new NotImplementedException(); 
		}

		[MonoTODO]
		public static object GetObject(Type type, 
			string url, object state) { 
			throw new NotImplementedException(); 
		}
	}
}
