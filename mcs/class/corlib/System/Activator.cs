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
		
		public static object CreateInstance(Type type) {
			return CreateInstance (type, false);
		}
		
		public static object CreateInstance(Type type, object[] args) {
			return CreateInstance (type, args, new object [0]);
		}

		[MonoTODO]
		public static object CreateInstance(Type type, object[] args, object[] activationAttributes) {
			Type[] atypes = new Type [args.Length];
			for (int i = 0; i < args.Length; ++i) {
				atypes [i] = args [i].GetType ();
			}
			ConstructorInfo ctor = type.GetConstructor (atypes);
			return ctor.Invoke (args);

		}

		[MonoTODO]
		public static object CreateInstance(Type type, 
			BindingFlags bindingAttr, Binder binder, object[] args, 
			CultureInfo culture) { 
			return CreateInstance (type, bindingAttr, binder, args, culture, new object [0]);
		}

		[MonoTODO]
		public static object CreateInstance(Type type, 
				BindingFlags bindingAttr, Binder binder, object[] args, 
				CultureInfo culture, object[] activationAttributes) { 
			Type[] atypes = new Type [args.Length];
			for (int i = 0; i < args.Length; ++i) {
				atypes [i] = args [i].GetType ();
			}
			ConstructorInfo ctor = type.GetConstructor (bindingAttr, binder, atypes, null);
			return ctor.Invoke (args, bindingAttr, binder, args, culture);
		}

		[MonoTODO]
		public static object CreateInstance(Type type, bool nonPublic) { 
			ConstructorInfo ctor = type.GetConstructor (Type.EmptyTypes);
			return ctor.Invoke (null);
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
