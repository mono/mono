//
// System.Activator.cs
//
// Authors:
//   Nick Drochak II (ndrochak@gol.com)
//   Gonzalo Paniagua (gonzalo@ximian.com)
//
// (C) 2001 Nick Drochak II
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
//

using System.Globalization;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;
using System.Runtime.CompilerServices;
using System.Security.Policy;

namespace System 
{
	public sealed class Activator
	{
		private static BindingFlags _flags = BindingFlags.CreateInstance |
						     BindingFlags.Public |
						     BindingFlags.Instance;

		private Activator () {}

		[MonoTODO]
		public static ObjectHandle CreateComInstanceFrom (string assemblyName, string typeName)
		{
			throw new NotImplementedException(); 
		}

		public static ObjectHandle CreateInstanceFrom (string assemblyFile, string typeName)
		{
			return CreateInstanceFrom (assemblyFile, typeName, null);
		}

		public static ObjectHandle CreateInstanceFrom (string assemblyFile,
							       string typeName,
							       object [] activationAttributes)
		{
			return Activator.CreateInstanceFrom (assemblyFile,
							     typeName,
							     false,
							     _flags,
							     null,
							     null,
							     null,
							     activationAttributes,
							     null);
		}
		
		[MonoTODO]
		public static ObjectHandle CreateInstanceFrom (string assemblyFile,
							       string typeName,
							       bool ignoreCase,
							       BindingFlags bindingAttr,
							       Binder binder,
							       object [] args,
							       CultureInfo culture,
							       object [] activationAttributes,
							       Evidence securityInfo)
		{
			//TODO: when Assembly implements security, use it.
			//Assembly assembly = Assembly.LoadFrom (assemblyFile, securityInfo);
			Assembly assembly = Assembly.LoadFrom (assemblyFile);
			if (assembly == null)
				return null;

			Type type = assembly.GetType (typeName, true, ignoreCase);
			if (type == null)
				return null;

			object obj = CreateInstance (type, bindingAttr, binder, args, culture, activationAttributes);
			return (obj != null) ? new ObjectHandle (obj) : null;
		}
		
		public static ObjectHandle CreateInstance (string assemblyName, string typeName)
		{
			return Activator.CreateInstance (assemblyName, typeName, null);
		}
		
		public static ObjectHandle CreateInstance (string assemblyName,
							   string typeName,
							   object [] activationAttributes)
		{
			return Activator.CreateInstance (assemblyName,
							 typeName,
							 false,
							 _flags,
							 null,
							 null,
							 null,
							 activationAttributes,
							 null);
		}
		
		[MonoTODO]
		public static ObjectHandle CreateInstance (string assemblyName,
							   string typeName,
							   bool ignoreCase,
							   BindingFlags bindingAttr,
							   Binder binder,
							   object [] args,
							   CultureInfo culture,
							   object [] activationAttributes,
							   Evidence securityInfo)
		{
			//TODO: when Assembly implements security, use it.
			//Assembly assembly = Assembly.Load (assemblyFile, securityInfo);
			Assembly assembly = Assembly.Load (assemblyName);
			Type type = assembly.GetType (typeName, true, ignoreCase);
			object obj = CreateInstance (type, bindingAttr, binder, args, culture, activationAttributes);
			return (obj != null) ? new ObjectHandle (obj) : null;
		}
		
		public static object CreateInstance (Type type)
		{
			return CreateInstance (type, false);
		}
		
		public static object CreateInstance (Type type, object [] args)
		{
			return CreateInstance (type, args, new object [0]);
		}

		[MonoTODO]
		public static object CreateInstance (Type type, object [] args, object [] activationAttributes)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			if (type.IsAbstract)
				throw new MemberAccessException ("Cannot create an abstract class");

			int length = 0;
			if (args != null)
				length = args.Length;

			Type [] atypes = new Type [length];
			for (int i = 0; i < length; ++i) {
				atypes [i] = args [i].GetType ();
			}
			ConstructorInfo ctor = type.GetConstructor (atypes);
			if (ctor == null) {
				if (type.IsValueType && atypes.Length == 0)
					return CreateInstanceInternal (type);

				throw new MissingMethodException ("Constructor not found");
			}

			if (activationAttributes != null && activationAttributes.Length > 0) {
				object newOb = ActivationServices.CreateProxyFromAttributes (type, activationAttributes);
				if (newOb != null)
					return ctor.Invoke (newOb, args);
			}

			return ctor.Invoke (args);
		}

		public static object CreateInstance (Type type,
						     BindingFlags bindingAttr,
						     Binder binder,
						     object [] args,
						     CultureInfo culture)
		{
			return CreateInstance (type, bindingAttr, binder, args, culture, new object [0]);
		}

		[MonoTODO]
		public static object CreateInstance (Type type,
						     BindingFlags bindingAttr,
						     Binder binder,
						     object [] args,
						     CultureInfo culture,
						     object [] activationAttributes)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
		
			if (type.IsAbstract)
				throw new MemberAccessException ("Cannot create an abstract class");

			int length = 0;
			if (args != null)
				length = args.Length;

			Type[] atypes = new Type [length];
			for (int i = 0; i < length; ++i) {
				atypes [i] = args [i].GetType ();
			}
			ConstructorInfo ctor = type.GetConstructor (bindingAttr, binder, atypes, null);
			if (ctor == null) {
				// Not sure about this
				if (type.IsValueType && atypes.Length == 0) {
					return CreateInstanceInternal (type);
				}

				throw new MissingMethodException ("Constructor not found");
			}

			if (activationAttributes != null && activationAttributes.Length > 0) {
				object newOb = ActivationServices.CreateProxyFromAttributes (type, activationAttributes);
				if (newOb != null)
					return ctor.Invoke (newOb, bindingAttr, binder, args, culture);
			}

			return ctor.Invoke (bindingAttr, binder, args, culture);
		}

		public static object CreateInstance (Type type, bool nonPublic)
		{ 
			if (type == null)
				throw new ArgumentNullException ("type");
		
			if (type.IsAbstract)
				throw new MemberAccessException ("Cannot create an abstract class");

			BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
			if (nonPublic)
				flags |= BindingFlags.NonPublic;

			ConstructorInfo ctor = type.GetConstructor (
				flags, null, CallingConventions.Any, Type.EmptyTypes, null);
			if (ctor == null) {
				if (type.IsValueType)
					return CreateInstanceInternal (type);

				throw new MissingMethodException ("Default constructor not found");
			}

			return ctor.Invoke (null);
		}

		public static object GetObject (Type type, string url)
		{
			return RemotingServices.Connect (type, url);
		}

		public static object GetObject (Type type, string url, object state)
		{ 
			return RemotingServices.Connect (type, url, state);
		}


		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern object CreateInstanceInternal (Type type);
	}
}

