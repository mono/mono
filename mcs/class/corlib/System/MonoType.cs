//
// System.MonoType
//
// Authors: 
// 	Sean MacIsaac (macisaac@ximian.com)
// 	Paolo Molaro (lupus@ximian.com)
// 	Patrik Torstensson (patrik.torstensson@labs2.com)
//	Gonzalo Paniagua (gonzalo@ximian.com)
//
// (c) 2001-2003 Ximian, Inc.
// Copyright (C) 2003-2005 Novell, Inc (http://www.novell.com)
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

using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security;

namespace System
{
	// Contains information about the type which is expensive to compute
	internal class MonoTypeInfo {
		public string full_name;
		public ConstructorInfo default_ctor;
	}
		
	[Serializable]
	internal class MonoType : Type, ISerializable
	{
		[NonSerialized]
		MonoTypeInfo type_info;

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void type_from_obj (MonoType type, Object obj);
		
		internal MonoType (Object obj)
		{
			// this should not be used - lupus
			type_from_obj (this, obj);
			
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern TypeAttributes get_attributes (Type type);

		internal ConstructorInfo GetDefaultConstructor () {
			ConstructorInfo ctor = null;
			
			if (type_info == null)
				type_info = new MonoTypeInfo ();
			if ((ctor = type_info.default_ctor) == null) {
				const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
	
				ctor = type_info.default_ctor = GetConstructor (flags,  null, CallingConventions.Any, Type.EmptyTypes, null);
			}

			return ctor;
		}

		protected override TypeAttributes GetAttributeFlagsImpl ()
		{
			return get_attributes (this);
		}

		protected override ConstructorInfo GetConstructorImpl (BindingFlags bindingAttr,
								       Binder binder,
								       CallingConventions callConvention,
								       Type[] types,
								       ParameterModifier[] modifiers)
		{
			ConstructorInfo[] methods = GetConstructors (bindingAttr);
			return GetConstructorImpl (methods, bindingAttr, binder, callConvention, types, modifiers);
		}

		internal static ConstructorInfo GetConstructorImpl (ConstructorInfo[] methods, BindingFlags bindingAttr,
								       Binder binder,
								       CallingConventions callConvention,
								       Type[] types,
								       ParameterModifier[] modifiers)
		{
			if (bindingAttr == BindingFlags.Default)
				bindingAttr = BindingFlags.Public | BindingFlags.Instance;

			ConstructorInfo found = null;
			MethodBase[] match;
			int count = 0;
			foreach (ConstructorInfo m in methods) {
				// Under MS.NET, Standard|HasThis matches Standard...
				if (callConvention != CallingConventions.Any && ((m.CallingConvention & callConvention) != callConvention))
					continue;
				found = m;
				count++;
			}
			if (count == 0)
				return null;
			if (types == null) {
				if (count > 1)
					throw new AmbiguousMatchException ();
				return (ConstructorInfo) CheckMethodSecurity (found);
			}
			match = new MethodBase [count];
			if (count == 1)
				match [0] = found;
			else {
				count = 0;
				foreach (ConstructorInfo m in methods) {
					if (callConvention != CallingConventions.Any && ((m.CallingConvention & callConvention) != callConvention))
						continue;
					match [count++] = m;
				}
			}
			if (binder == null)
				binder = Binder.DefaultBinder;
			return (ConstructorInfo) CheckMethodSecurity (binder.SelectMethod (bindingAttr, match, types, modifiers));
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern ConstructorInfo[] GetConstructors_internal (BindingFlags bindingAttr, Type reflected_type);

		public override ConstructorInfo[] GetConstructors (BindingFlags bindingAttr)
		{
			return GetConstructors_internal (bindingAttr, this);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern EventInfo InternalGetEvent (string name, BindingFlags bindingAttr);

		public override EventInfo GetEvent (string name, BindingFlags bindingAttr)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			return InternalGetEvent (name, bindingAttr);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern EventInfo[] GetEvents_internal (BindingFlags bindingAttr, Type reflected_type);

		public override EventInfo[] GetEvents (BindingFlags bindingAttr)
		{
			return GetEvents_internal (bindingAttr, this);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override FieldInfo GetField (string name, BindingFlags bindingAttr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern FieldInfo[] GetFields_internal (BindingFlags bindingAttr, Type reflected_type);

		public override FieldInfo[] GetFields (BindingFlags bindingAttr)
		{
			return GetFields_internal (bindingAttr, this);
		}
		
		public override Type GetInterface (string name, bool ignoreCase)
		{
			if (name == null)
				throw new ArgumentNullException ();

			Type[] interfaces = GetInterfaces();

			foreach (Type type in interfaces) {
				/*We must compare against the generic type definition*/
				Type t = type.IsGenericType ? type.GetGenericTypeDefinition () : type;

				if (String.Compare (t.Name, name, ignoreCase, CultureInfo.InvariantCulture) == 0)
					return type;
				if (String.Compare (t.FullName, name, ignoreCase, CultureInfo.InvariantCulture) == 0)
					return type;
			}

			return null;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override Type[] GetInterfaces();
		
		public override MemberInfo[] GetMembers( BindingFlags bindingAttr)
		{
			return FindMembers (MemberTypes.All, bindingAttr, null, null);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern MethodInfo [] GetMethodsByName (string name, BindingFlags bindingAttr, bool ignoreCase, Type reflected_type);

		public override MethodInfo [] GetMethods (BindingFlags bindingAttr)
		{
			return GetMethodsByName (null, bindingAttr, false, this);
		}

		protected override MethodInfo GetMethodImpl (string name, BindingFlags bindingAttr,
							     Binder binder,
							     CallingConventions callConvention,
							     Type[] types, ParameterModifier[] modifiers)
		{
			bool ignoreCase = ((bindingAttr & BindingFlags.IgnoreCase) != 0);
			MethodInfo[] methods = GetMethodsByName (name, bindingAttr, ignoreCase, this);
			MethodInfo found = null;
			MethodBase[] match;
			int count = 0;
			
			foreach (MethodInfo m in methods) {
				// Under MS.NET, Standard|HasThis matches Standard...
				if (callConvention != CallingConventions.Any && ((m.CallingConvention & callConvention) != callConvention))
					continue;
				found = m;
				count++;
			}

			if (count == 0)
				return null;
			
			if (count == 1 && types == null) 
				return (MethodInfo) CheckMethodSecurity (found);

			match = new MethodBase [count];
			if (count == 1)
				match [0] = found;
			else {
				count = 0;
				foreach (MethodInfo m in methods) {
					if (callConvention != CallingConventions.Any && ((m.CallingConvention & callConvention) != callConvention))
						continue;
					match [count++] = m;
				}
			}

			if (types == null) 
				return (MethodInfo) CheckMethodSecurity (Binder.FindMostDerivedMatch (match));

			if (binder == null)
				binder = Binder.DefaultBinder;
			
			return (MethodInfo) CheckMethodSecurity (binder.SelectMethod (bindingAttr, match, types, modifiers));
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern MethodInfo GetCorrespondingInflatedMethod (MethodInfo generic);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern ConstructorInfo GetCorrespondingInflatedConstructor (ConstructorInfo generic);

		internal override MethodInfo GetMethod (MethodInfo fromNoninstanciated)
                {
			if (fromNoninstanciated == null)
				throw new ArgumentNullException ("fromNoninstanciated");
                        return GetCorrespondingInflatedMethod (fromNoninstanciated);
                }

		internal override ConstructorInfo GetConstructor (ConstructorInfo fromNoninstanciated)
		{
			if (fromNoninstanciated == null)
				throw new ArgumentNullException ("fromNoninstanciated");
                        return GetCorrespondingInflatedConstructor (fromNoninstanciated);
		}

		internal override FieldInfo GetField (FieldInfo fromNoninstanciated)
		{
			/* create sensible flags from given FieldInfo */
			BindingFlags flags = fromNoninstanciated.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
			flags |= fromNoninstanciated.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
			return GetField (fromNoninstanciated.Name, flags);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override Type GetNestedType (string name, BindingFlags bindingAttr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override Type[] GetNestedTypes (BindingFlags bindingAttr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern PropertyInfo[] GetPropertiesByName (string name, BindingFlags bindingAttr, bool icase, Type reflected_type);

		public override PropertyInfo [] GetProperties (BindingFlags bindingAttr)
		{
			return GetPropertiesByName (null, bindingAttr, false, this);
		}

		protected override PropertyInfo GetPropertyImpl (string name, BindingFlags bindingAttr,
								 Binder binder, Type returnType,
								 Type[] types,
								 ParameterModifier[] modifiers)
		{
			bool ignoreCase = ((bindingAttr & BindingFlags.IgnoreCase) != 0);
			PropertyInfo [] props = GetPropertiesByName (name, bindingAttr, ignoreCase, this);
			int count = props.Length;
			if (count == 0)
				return null;
			
			if (count == 1 && (types == null || types.Length == 0) && 
			    (returnType == null || returnType == props[0].PropertyType))
				return props [0];

			if (binder == null)
				binder = Binder.DefaultBinder;

			return binder.SelectProperty (bindingAttr, props, returnType, types, modifiers);
		}

		protected override bool HasElementTypeImpl ()
		{
			return IsArrayImpl() || IsByRefImpl() || IsPointerImpl ();
		}

		protected override bool IsArrayImpl ()
		{
			return Type.IsArrayImpl (this);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern override bool IsByRefImpl ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		protected extern override bool IsCOMObjectImpl ();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern override bool IsPointerImpl ();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern override bool IsPrimitiveImpl ();

		public override bool IsSubclassOf (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			return base.IsSubclassOf (type);
		}

		public override object InvokeMember (string name, BindingFlags invokeAttr,
						     Binder binder, object target, object[] args,
						     ParameterModifier[] modifiers,
						     CultureInfo culture, string[] namedParameters)
		{
			const string bindingflags_arg = "bindingFlags";


			if ((invokeAttr & BindingFlags.CreateInstance) != 0) {
				if ((invokeAttr & (BindingFlags.GetField |
						BindingFlags.GetField | BindingFlags.GetProperty |
						BindingFlags.SetProperty)) != 0)
					throw new ArgumentException (bindingflags_arg);
			} else if (name == null)
				throw new ArgumentNullException ("name");
			if ((invokeAttr & BindingFlags.GetField) != 0 && (invokeAttr & BindingFlags.SetField) != 0)
				throw new ArgumentException ("Cannot specify both Get and Set on a field.", bindingflags_arg);
			if ((invokeAttr & BindingFlags.GetProperty) != 0 && (invokeAttr & BindingFlags.SetProperty) != 0)
				throw new ArgumentException ("Cannot specify both Get and Set on a property.", bindingflags_arg);
			if ((invokeAttr & BindingFlags.InvokeMethod) != 0) {
				if ((invokeAttr & BindingFlags.SetField) != 0)
					throw new ArgumentException ("Cannot specify Set on a field and Invoke on a method.", bindingflags_arg);
				if ((invokeAttr & BindingFlags.SetProperty) != 0)
					throw new ArgumentException ("Cannot specify Set on a property and Invoke on a method.", bindingflags_arg);
			}
			if ((namedParameters != null) && ((args == null) || args.Length < namedParameters.Length))
				throw new ArgumentException ("namedParameters cannot be more than named arguments in number");
			if ((invokeAttr & (BindingFlags.InvokeMethod|BindingFlags.CreateInstance|BindingFlags.GetField|BindingFlags.SetField|BindingFlags.GetProperty|BindingFlags.SetProperty)) == 0)
				throw new ArgumentException ("Must specify binding flags describing the invoke operation required.", bindingflags_arg);

			/* set some defaults if none are provided :-( */
			if ((invokeAttr & (BindingFlags.Public|BindingFlags.NonPublic)) == 0)
				invokeAttr |= BindingFlags.Public;
			if ((invokeAttr & (BindingFlags.Static|BindingFlags.Instance)) == 0)
				invokeAttr |= BindingFlags.Static|BindingFlags.Instance;

			if (binder == null)
				binder = Binder.DefaultBinder;
			if ((invokeAttr & BindingFlags.CreateInstance) != 0) {
				/* the name is ignored */
				invokeAttr |= BindingFlags.DeclaredOnly;
				ConstructorInfo[] ctors = GetConstructors (invokeAttr);
				object state = null;
				MethodBase ctor = binder.BindToMethod (invokeAttr, ctors, ref args, modifiers, culture, namedParameters, out state);
				if (ctor == null) {
					if (this.IsValueType && args == null)
						return Activator.CreateInstanceInternal (this);
					
					throw new MissingMethodException ("Constructor on type '" + FullName + "' not found.");
				}
				object result = ctor.Invoke (target, invokeAttr, binder, args, culture);
				binder.ReorderArgumentArray (ref args, state);
				return result;
			}
			if (name == String.Empty && Attribute.IsDefined (this, typeof (DefaultMemberAttribute))) {
				DefaultMemberAttribute attr = (DefaultMemberAttribute) Attribute.GetCustomAttribute (this, typeof (DefaultMemberAttribute));
				name = attr.MemberName;
			}
			bool ignoreCase = (invokeAttr & BindingFlags.IgnoreCase) != 0;
			string throwMissingMethodDescription = null;
			bool throwMissingFieldException = false;
			
			if ((invokeAttr & BindingFlags.InvokeMethod) != 0) {
				MethodInfo[] methods = GetMethodsByName (name, invokeAttr, ignoreCase, this);
				object state = null;
				if (args == null)
					args = new object [0];
				MethodBase m = binder.BindToMethod (invokeAttr, methods, ref args, modifiers, culture, namedParameters, out state);
				if (m == null) {
					if (methods.Length > 0)
						throwMissingMethodDescription = "The best match for method " + name + " has some invalid parameter.";
					else
						throwMissingMethodDescription = "Cannot find method " + name + ".";
				} else {
					ParameterInfo[] parameters = m.GetParameters();
					for (int i = 0; i < parameters.Length; ++i) {
						if (System.Reflection.Missing.Value == args [i] && (parameters [i].Attributes & ParameterAttributes.HasDefault) != ParameterAttributes.HasDefault)
							throw new ArgumentException ("Used Missing.Value for argument without default value", "parameters");
					}
					object result = m.Invoke (target, invokeAttr, binder, args, culture);
					binder.ReorderArgumentArray (ref args, state);
					return result;
				}
			}
			if ((invokeAttr & BindingFlags.GetField) != 0) {
				FieldInfo f = GetField (name, invokeAttr);
				if (f != null) {
					return f.GetValue (target);
				} else if ((invokeAttr & BindingFlags.GetProperty) == 0) {
					throwMissingFieldException = true;
				}
				/* try GetProperty */
			} else if ((invokeAttr & BindingFlags.SetField) != 0) {
				FieldInfo f = GetField (name, invokeAttr);
				if (f != null) {
					if (args == null)
						throw new ArgumentNullException ("providedArgs");
					if ((args == null) || args.Length != 1)
						throw new ArgumentException ("Only the field value can be specified to set a field value.", bindingflags_arg);
					f.SetValue (target, args [0]);
					return null;
				} else if ((invokeAttr & BindingFlags.SetProperty) == 0) {
					throwMissingFieldException = true;
				}
				/* try SetProperty */
			}
			if ((invokeAttr & BindingFlags.GetProperty) != 0) {
				PropertyInfo[] properties = GetPropertiesByName (name, invokeAttr, ignoreCase, this);
				object state = null;
				int i, count = 0;
				for (i = 0; i < properties.Length; ++i) {
					if ((properties [i].GetGetMethod (true) != null))
						count++;
				}
				MethodBase[] smethods = new MethodBase [count];
				count = 0;
				for (i = 0; i < properties.Length; ++i) {
					MethodBase mb = properties [i].GetGetMethod (true);
					if (mb != null)
						smethods [count++] = mb;
				}
				MethodBase m = binder.BindToMethod (invokeAttr, smethods, ref args, modifiers, culture, namedParameters, out state);
				if (m == null) {
					throwMissingFieldException = true;
				} else {
					object result = m.Invoke (target, invokeAttr, binder, args, culture);
					binder.ReorderArgumentArray (ref args, state);
					return result;
				}
			} else if ((invokeAttr & BindingFlags.SetProperty) != 0) {
				PropertyInfo[] properties = GetPropertiesByName (name, invokeAttr, ignoreCase, this);
				object state = null;
				int i, count = 0;
				for (i = 0; i < properties.Length; ++i) {
					if (properties [i].GetSetMethod (true) != null)
						count++;
				}
				MethodBase[] smethods = new MethodBase [count];
				count = 0;
				for (i = 0; i < properties.Length; ++i) {
					MethodBase mb = properties [i].GetSetMethod (true);
					if (mb != null)
						smethods [count++] = mb;
				}
				MethodBase m = binder.BindToMethod (invokeAttr, smethods, ref args, modifiers, culture, namedParameters, out state);
				if (m == null) {
					throwMissingFieldException = true;
				} else {
					object result = m.Invoke (target, invokeAttr, binder, args, culture);
					binder.ReorderArgumentArray (ref args, state);
					return result;
				}
			}
			if (throwMissingMethodDescription != null)
				throw new MissingMethodException(throwMissingMethodDescription);
			if (throwMissingFieldException)
				throw new MissingFieldException("Cannot find variable " + name + ".");

			return null;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override Type GetElementType ();

		public override Type UnderlyingSystemType {
			get {
				// This has _nothing_ to do with getting the base type of an enum etc.
				return this;
			}
		}

		public extern override Assembly Assembly {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public override string AssemblyQualifiedName {
			get {
				return getFullName (true, true);
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern string getFullName(bool full_name, bool assembly_qualified);

		public extern override Type BaseType {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public override string FullName {
			get {
				string fullName;
				// This doesn't need locking
				if (type_info == null)
					type_info = new MonoTypeInfo ();
				if ((fullName = type_info.full_name) == null)
					fullName = type_info.full_name = getFullName (true, false);

				return fullName;
			}
		}

		public override Guid GUID {
			get {
				object[] att = GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), true);
				if (att.Length == 0)
					return Guid.Empty;
				return new Guid(((System.Runtime.InteropServices.GuidAttribute)att[0]).Value);
			}
		}

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}

		public override object[] GetCustomAttributes (bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, inherit);
		}

		public override object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
		}

		public override MemberTypes MemberType {
			get {
				if (DeclaringType != null && !IsGenericParameter)
					return MemberTypes.NestedType;
				else
					return MemberTypes.TypeInfo;
			}
		}

		public extern override string Name {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public extern override string Namespace {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public extern override Module Module {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public extern override Type DeclaringType {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public override Type ReflectedType {
			get {
				return DeclaringType;
			}
		}

		public override RuntimeTypeHandle TypeHandle {
			get {
				return _impl;
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override int GetArrayRank ();

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			UnitySerializationHolder.GetTypeData (this, info, context);
		}

		public override string ToString()
		{
			return getFullName (false, false);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override Type [] GetGenericArguments ();

		public override bool ContainsGenericParameters {
			get {
				if (IsGenericParameter)
					return true;

				if (IsGenericType) {
					foreach (Type arg in GetGenericArguments ())
						if (arg.ContainsGenericParameters)
							return true;
				}

				if (HasElementType)
					return GetElementType ().ContainsGenericParameters;

				return false;
			}
		}

		public extern override bool IsGenericParameter {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public extern override MethodBase DeclaringMethod {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public override Type GetGenericTypeDefinition () {
			Type res = GetGenericTypeDefinition_impl ();
			if (res == null)
				throw new InvalidOperationException ();

			return res;
		}

#if NET_4_0
		public override IList<CustomAttributeData> GetCustomAttributesData () {
			return CustomAttributeData.GetCustomAttributes (this);
		}


		public override Array GetEnumValues () {
			if (!IsEnum)
				throw new ArgumentException ("Type is not an enumeration", "enumType");

			return Enum.GetValues (this);
		}
#endif

		static MethodBase CheckMethodSecurity (MethodBase mb)
		{
#if NET_2_1
			return mb;
#else
			if (!SecurityManager.SecurityEnabled || (mb == null))
				return mb;

			// Sadly we have no way to know which kind of security action this is
			// so we must do it the hard way. Actually this isn't so bad 
			// because we can skip the (mb.Attributes & MethodAttributes.HasSecurity)
			// icall required (and do it ourselves)

			// this (unlike the Invoke step) is _and stays_ a LinkDemand (caller)
			return SecurityManager.ReflectedLinkDemandQuery (mb) ? mb : null;
#endif
		}

#if NET_4_0
		//seclevel { transparent = 0, safe-critical = 1, critical = 2}
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern int get_core_clr_security_level ();

		public override bool IsSecurityTransparent
		{
			get { return get_core_clr_security_level () == 0; }
		}

		public override bool IsSecurityCritical
		{
			get { return get_core_clr_security_level () > 0; }
		}

		public override bool IsSecuritySafeCritical
		{
			get { return get_core_clr_security_level () == 1; }
		}
#endif

	}
}
