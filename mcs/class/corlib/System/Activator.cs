//
// System.Activator.cs
//
// Authors:
//   Nick Drochak II (ndrochak@gol.com)
//   Gonzalo Paniagua (gonzalo@ximian.com)
//   Marek Safar (marek.safar@gmail.com)
//
// (C) 2001 Nick Drochak II
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
// Copyright (C) 2013 Xamarin Inc (http://www.xamarin.com)
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Policy;
using System.Configuration.Assemblies;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;

namespace System 
{
	[ClassInterface (ClassInterfaceType.None)]
	[ComVisible (true)]
	[ComDefaultInterface (typeof (_Activator))]
	public sealed class Activator : _Activator
	{
		const BindingFlags _flags = BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance;
		const BindingFlags _accessFlags = BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase | 
											BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
											BindingFlags.Static;

		private Activator ()
		{
		}

		[MonoTODO ("No COM support")]
		public static ObjectHandle CreateComInstanceFrom (string assemblyName, string typeName)
		{
			if (assemblyName == null)
				throw new ArgumentNullException ("assemblyName");

			if (typeName == null)
				throw new ArgumentNullException ("typeName");

			if (assemblyName.Length == 0)
				throw new ArgumentException ("assemblyName");

			throw new NotImplementedException();
		}

		[MonoTODO("Mono does not support COM")]
		public static ObjectHandle CreateComInstanceFrom (string assemblyName, string typeName,
		                                                  byte []hashValue, AssemblyHashAlgorithm hashAlgorithm)
		{
			if (assemblyName == null)
				throw new ArgumentNullException ("assemblyName");

			if (typeName == null)
				throw new ArgumentNullException ("typeName");

			if (assemblyName.Length == 0)
				throw new ArgumentException ("assemblyName");

			throw new NotImplementedException();
		}

		public static ObjectHandle CreateInstanceFrom (string assemblyFile, string typeName)
		{
			return CreateInstanceFrom (assemblyFile, typeName, null);
		}

		public static ObjectHandle CreateInstanceFrom (string assemblyFile, string typeName, object [] activationAttributes)
		{
			return Activator.CreateInstanceFrom (assemblyFile, typeName, false, _flags, null, null, null,
				activationAttributes, null);
		}

#if NET_4_0
		[Obsolete]
#endif
		public static ObjectHandle CreateInstanceFrom (string assemblyFile, string typeName, bool ignoreCase,
		                                               BindingFlags bindingAttr, Binder binder, object [] args,
		                                               CultureInfo culture, object [] activationAttributes,
		                                               Evidence securityInfo)
		{
			Assembly assembly = Assembly.LoadFrom (assemblyFile, securityInfo);
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

#if NET_4_0
		[Obsolete]
#endif
		public static ObjectHandle CreateInstance (string assemblyName, string typeName, bool ignoreCase,
		                                           BindingFlags bindingAttr, Binder binder, object [] args,
							   CultureInfo culture, object [] activationAttributes, Evidence securityInfo)
		{
			Assembly assembly = null;
			if(assemblyName == null)
				assembly = Assembly.GetCallingAssembly ();
			else
				assembly = Assembly.Load (assemblyName, securityInfo);
			Type type = assembly.GetType (typeName, true, ignoreCase);
			object obj = CreateInstance (type, bindingAttr, binder, args, culture, activationAttributes);
			return (obj != null) ? new ObjectHandle (obj) : null;
		}

		[MonoNotSupported ("no ClickOnce in mono")]
		public static ObjectHandle CreateInstance (ActivationContext activationContext)
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("no ClickOnce in mono")]
		public static ObjectHandle CreateInstance (ActivationContext activationContext, string [] activationCustomData)
		{
			throw new NotImplementedException ();
		}

		// Cross-domain instance creation

		public static ObjectHandle CreateInstanceFrom (AppDomain domain, string assemblyFile, string typeName)
		{
			if (domain == null)
				throw new ArgumentNullException ("domain");
			return domain.CreateInstanceFrom (assemblyFile, typeName);
		}


#if NET_4_0
		[Obsolete]
#endif
		public static ObjectHandle CreateInstanceFrom (AppDomain domain, string assemblyFile, string typeName,
							       bool ignoreCase, BindingFlags bindingAttr, Binder binder,
							       object [] args, CultureInfo culture,
							       object [] activationAttributes,
							       Evidence securityAttributes)
		{
			if (domain == null)
				throw new ArgumentNullException ("domain");

			return domain.CreateInstanceFrom (assemblyFile, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
		}

		public static ObjectHandle CreateInstance (AppDomain domain, string assemblyName, string typeName)
		{
			if (domain == null)
				throw new ArgumentNullException ("domain");
			return domain.CreateInstance (assemblyName, typeName);
		}

#if NET_4_0
		[Obsolete]
#endif
		public static ObjectHandle CreateInstance (AppDomain domain, string assemblyName, string typeName,
							   bool ignoreCase, BindingFlags bindingAttr, Binder binder,
							   object [] args, CultureInfo culture,
							   object [] activationAttributes,
							   Evidence securityAttributes)
		{
			if (domain == null)
				throw new ArgumentNullException ("domain");
			return domain.CreateInstance (assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
		}

		public static T CreateInstance <T> ()
		{
			return (T) CreateInstance (typeof (T));
		}

		public static object CreateInstance (Type type)
		{
			return CreateInstance (type, false);
		}

		public static object CreateInstance (Type type, params object [] args)
		{
			return CreateInstance (type, args, EmptyArray<object>.Value);
		}

		public static object CreateInstance (Type type, object [] args, object [] activationAttributes)
		{
			return CreateInstance (type, BindingFlags.Default, Binder.DefaultBinder, args, null, activationAttributes);
		}

		public static object CreateInstance (Type type, BindingFlags bindingAttr, Binder binder, object [] args,
		                                     CultureInfo culture)
		{
			return CreateInstance (type, bindingAttr, binder, args, culture, EmptyArray<object>.Value);
		}

		public static object CreateInstance (Type type, BindingFlags bindingAttr, Binder binder, object [] args,
		                                     CultureInfo culture, object [] activationAttributes)
		{
			CheckType (type);

			if (type.ContainsGenericParameters)
				throw new ArgumentException (type + " is an open generic type", "type");

			// It seems to apply the same rules documented for InvokeMember: "If the type of lookup
			// is omitted, BindingFlags.Public | BindingFlags.Instance will apply".
			if ((bindingAttr & _accessFlags) == 0)
				bindingAttr |= BindingFlags.Public | BindingFlags.Instance;

			if (binder == null)
				binder = Binder.DefaultBinder;

			object state;
			ConstructorInfo ctor = (ConstructorInfo) binder.BindToMethod (bindingAttr, type.GetConstructors (bindingAttr), ref args, null, null, null, out state);

			if (ctor == null) {
				// Not sure about this
				if (type.IsValueType && (args == null || args.Length == 0)) {
					return CreateInstanceInternal (type);
				}

				var sb = new StringBuilder ();
				if (args != null) {
					for (int i = 0; i < args.Length; i++) {
						if (i > 0)
							sb.Append (", ");

						var argument = args [i];
						var arg_type = argument != null ? argument.GetType () : null;
						sb.Append (arg_type != null ? arg_type.ToString () : "(unknown)");
					}
				}

				throw new MissingMethodException (String.Format (Locale.GetText ("No constructor found for {0}::.ctor({1})"),
										 type.FullName, sb));
			}

			CheckAbstractType (type);

			if (activationAttributes != null && activationAttributes.Length > 0) {
#if DISABLE_REMOTING
				throw new NotSupportedException ("Activation attributes are not supported");
#else
				if (!type.IsMarshalByRef) {
					string msg = Locale.GetText ("Type '{0}' doesn't derive from MarshalByRefObject.", type.FullName);
					throw new NotSupportedException (msg);
				}
				object newOb = ActivationServices.CreateProxyFromAttributes (type, activationAttributes);
				if (newOb != null) {
					// This returns null
					ctor.Invoke (newOb, bindingAttr, binder, args, culture);
					return newOb;
				}
#endif
			}

			return ctor.Invoke (bindingAttr, binder, args, culture);
		}

		public static object CreateInstance (Type type, bool nonPublic)
		{ 
			CheckType (type);

			if (type.ContainsGenericParameters)
				throw new ArgumentException (type + " is an open generic type", "type");

			MonoType monoType = type.UnderlyingSystemType as MonoType;
			if (monoType == null)
				throw new ArgumentException ("Type must be a type provided by the runtime");

			CheckAbstractType (monoType);

			var ctor = monoType.GetDefaultConstructor ();
			if (!nonPublic && ctor != null && !ctor.IsPublic) {
				ctor = null;
			}

			if (ctor == null) {
				if (type.IsValueType)
					return CreateInstanceInternal (type);

				throw new MissingMethodException (Locale.GetText ("Default constructor not found for type " + type.FullName));
			}

			return ctor.InternalInvoke (null, null);
		}

		private static void CheckType (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			if ((type == typeof (TypedReference)) || (type == typeof (ArgIterator)) || (type == typeof (void)) ||
				(type == typeof (RuntimeArgumentHandle))) {
				string msg = Locale.GetText ("CreateInstance cannot be used to create this type ({0}).", type.FullName);
				throw new NotSupportedException (msg);
			}
		}

		private static void CheckAbstractType (Type type)
		{
			if (type.IsAbstract) {
				string msg = Locale.GetText ("Cannot create an abstract class '{0}'.", type.FullName);
				throw new MissingMethodException (msg);
			}
		}

		[SecurityPermission (SecurityAction.LinkDemand, RemotingConfiguration = true)]
		public static object GetObject (Type type, string url)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			return RemotingServices.Connect (type, url);
		}

		[SecurityPermission (SecurityAction.LinkDemand, RemotingConfiguration = true)]
		public static object GetObject (Type type, string url, object state)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			return RemotingServices.Connect (type, url, state);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern object CreateInstanceInternal (Type type);

		void _Activator.GetIDsOfNames ([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException ();
		}

		void _Activator.GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException ();
		}

		void _Activator.GetTypeInfoCount (out uint pcTInfo)
		{
			throw new NotImplementedException ();
		}

		void _Activator.Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams,
			IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException ();
		}

#if NET_4_0
		public static ObjectHandle CreateInstance (string assemblyName, string typeName, bool ignoreCase,
		                                           BindingFlags bindingAttr, Binder binder, object [] args,
							   CultureInfo culture, object [] activationAttributes)
		{
			Assembly assembly = null;
			if(assemblyName == null)
				assembly = Assembly.GetCallingAssembly ();
			else
				assembly = Assembly.Load (assemblyName);
			Type type = assembly.GetType (typeName, true, ignoreCase);
			object obj = CreateInstance (type, bindingAttr, binder, args, culture, activationAttributes);
			return (obj != null) ? new ObjectHandle (obj) : null;
		}

		public static ObjectHandle CreateInstance (AppDomain domain, string assemblyName, string typeName,
							   bool ignoreCase, BindingFlags bindingAttr, Binder binder,
							   object [] args, CultureInfo culture,
							   object [] activationAttributes)
		{
			if (domain == null)
				throw new ArgumentNullException ("domain");
			return domain.CreateInstance (assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes);
		}

		public static ObjectHandle CreateInstanceFrom (string assemblyFile, string typeName, bool ignoreCase,
		                                               BindingFlags bindingAttr, Binder binder, object [] args,
		                                               CultureInfo culture, object [] activationAttributes)
		{
			Assembly assembly = Assembly.LoadFrom (assemblyFile);
			if (assembly == null)
				return null;

			Type type = assembly.GetType (typeName, true, ignoreCase);
			if (type == null)
				return null;

			object obj = CreateInstance (type, bindingAttr, binder, args, culture, activationAttributes);
			return (obj != null) ? new ObjectHandle (obj) : null;
		}

		public static ObjectHandle CreateInstanceFrom (AppDomain domain, string assemblyFile, string typeName,
							       bool ignoreCase, BindingFlags bindingAttr, Binder binder,
							       object [] args, CultureInfo culture,
							       object [] activationAttributes)
		{
			if (domain == null)
				throw new ArgumentNullException ("domain");

			return domain.CreateInstanceFrom (assemblyFile, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes);
		}
#endif
	}
}
