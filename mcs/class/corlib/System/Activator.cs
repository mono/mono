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

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Globalization;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Configuration.Assemblies;

namespace System 
{
	public sealed class Activator
	{
		const BindingFlags _flags = BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance;
		const BindingFlags _accessFlags = BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase | 
											BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
											BindingFlags.Static;

		private Activator ()
		{
		}

		[MonoTODO]
		public static ObjectHandle CreateComInstanceFrom (string assemblyName, string typeName)
		{
			if (assemblyName == null)
				throw new ArgumentNullException ("assemblyName");

			if (typeName == null)
				throw new ArgumentNullException ("typeName");

			throw new NotImplementedException();
		}

#if NET_1_1
		[MonoTODO]
		public static ObjectHandle CreateComInstanceFrom (string assemblyName, string typeName,
		                                                  byte []hashValue, AssemblyHashAlgorithm hashAlgorithm)
		{
			if (assemblyName == null)
				throw new ArgumentNullException ("assemblyName");

			if (typeName == null)
				throw new ArgumentNullException ("typeName");

			throw new NotImplementedException();
		}
#endif

		public static ObjectHandle CreateInstanceFrom (string assemblyFile, string typeName)
		{
			return CreateInstanceFrom (assemblyFile, typeName, null);
		}

		public static ObjectHandle CreateInstanceFrom (string assemblyFile, string typeName, object [] activationAttributes)
		{
			return Activator.CreateInstanceFrom (assemblyFile, typeName, false, _flags, null, null, null,
				activationAttributes, null);
		}

		[MonoTODO ("security")]
		public static ObjectHandle CreateInstanceFrom (string assemblyFile, string typeName, bool ignoreCase,
		                                               BindingFlags bindingAttr, Binder binder, object [] args,
		                                               CultureInfo culture, object [] activationAttributes,
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
			if (assemblyName == null)
				assemblyName = Assembly.GetCallingAssembly ().GetName ().Name;

			return Activator.CreateInstance (assemblyName, typeName, null);
		}

		public static ObjectHandle CreateInstance (string assemblyName, string typeName, object [] activationAttributes)
		{
			if (assemblyName == null)
				assemblyName = Assembly.GetCallingAssembly ().GetName ().Name;

			return Activator.CreateInstance (assemblyName, typeName, false, _flags, null, null, null,
				activationAttributes, null);
		}

		[MonoTODO ("security")]
		public static ObjectHandle CreateInstance (string assemblyName, string typeName, bool ignoreCase,
		                                           BindingFlags bindingAttr, Binder binder, object [] args,
							   CultureInfo culture, object [] activationAttributes, Evidence securityInfo)
		{
			//TODO: when Assembly implements security, use it.
			//Assembly assembly = Assembly.Load (assemblyFile, securityInfo);
			Assembly assembly = null;
			if(assemblyName == null)
				assembly = Assembly.GetCallingAssembly ();
			else
				assembly = Assembly.Load (assemblyName);
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

		public static object CreateInstance (Type type, object [] args, object [] activationAttributes)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			if (type.IsAbstract)
				throw new MemberAccessException (Locale.GetText ("Cannot create an abstract class."));

			int length = 0;
			if (args != null)
				length = args.Length;

			Type [] atypes = new Type [length];
			for (int i = 0; i < length; ++i)
				if (args [i] != null)
					atypes [i] = args [i].GetType ();
			
			ConstructorInfo ctor = type.GetConstructor (atypes);
			if (ctor == null) {
				if (type.IsValueType && atypes.Length == 0)
					return CreateInstanceInternal (type);

				throw new MissingMethodException (Locale.GetText ("Constructor not found."));
			}

			if (activationAttributes != null && activationAttributes.Length > 0 && type.IsMarshalByRef) {
				object newOb = ActivationServices.CreateProxyFromAttributes (type, activationAttributes);
				if (newOb != null)
					return ctor.Invoke (newOb, args);
			}

			return ctor.Invoke (args);
		}

		public static object CreateInstance (Type type, BindingFlags bindingAttr, Binder binder, object [] args,
		                                     CultureInfo culture)
		{
			return CreateInstance (type, bindingAttr, binder, args, culture, new object [0]);
		}

		public static object CreateInstance (Type type, BindingFlags bindingAttr, Binder binder, object [] args,
		                                     CultureInfo culture, object [] activationAttributes)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
		
			if (type.IsAbstract)
				throw new MemberAccessException (Locale.GetText ("Cannot create an abstract class."));
				
			// It seems to apply the same rules documented for InvokeMember: "If the type of lookup
			// is omitted, BindingFlags.Public | BindingFlags.Instance will apply".
			if ((bindingAttr & _accessFlags) == 0)
				bindingAttr |= BindingFlags.Public | BindingFlags.Instance;

			int length = 0;
			if (args != null)
				length = args.Length;

			Type[] atypes = new Type [length];
			for (int i = 0; i < length; ++i)
				if (args [i] != null)
					atypes [i] = args [i].GetType ();
				
			ConstructorInfo ctor = type.GetConstructor (bindingAttr, binder, atypes, null);
			if (ctor == null) {
				// Not sure about this
				if (type.IsValueType && atypes.Length == 0) {
					return CreateInstanceInternal (type);
				}

				throw new MissingMethodException (Locale.GetText ("Constructor not found."));
			}

			if (activationAttributes != null && activationAttributes.Length > 0 && type.IsMarshalByRef) {
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
				throw new MemberAccessException (Locale.GetText ("Cannot create an abstract class."));

			BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
			if (nonPublic)
				flags |= BindingFlags.NonPublic;

			ConstructorInfo ctor = type.GetConstructor (flags, null, CallingConventions.Any, Type.EmptyTypes, null);

			if (ctor == null) {
				if (type.IsValueType)
					return CreateInstanceInternal (type);

				throw new MissingMethodException (Locale.GetText ("Default constructor not found."));
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

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern object CreateInstanceInternal (Type type);
	}
}
