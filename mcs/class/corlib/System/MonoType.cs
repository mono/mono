//
// System.MonoType
//
// Sean MacIsaac (macisaac@ximian.com)
// Paolo Molaro (lupus@ximian.com)
// Patrik Torstensson (patrik.torstensson@labs2.com)
//
// (C) 2001 Ximian, Inc.
//

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Globalization;

namespace System
{
	internal struct MonoTypeInfo {
		public string name;
		public string name_space;
		public Type etype;
		public Type nested_in;
		public Assembly assembly;
		public int rank;
		public bool isprimitive;
	}

	internal sealed class MonoType : Type
	{

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void type_from_obj (MonoType type, Object obj);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void get_type_info (RuntimeTypeHandle type, out MonoTypeInfo info);

		[MonoTODO]
		internal MonoType (Object obj)
		{
			// this should not be used - lupus
			type_from_obj (this, obj);
			
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern TypeAttributes get_attributes (Type type);
	
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
			if (bindingAttr == BindingFlags.Default)
				bindingAttr = BindingFlags.Public | BindingFlags.Instance;

			ConstructorInfo[] methods = GetConstructors (bindingAttr);
			ConstructorInfo found = null;
			MethodBase[] match;
			int count = 0;
			foreach (ConstructorInfo m in methods) {
				if (callConvention != CallingConventions.Any && m.CallingConvention != callConvention)
					continue;
				found = m;
				count++;
			}
			if (count == 0)
				return null;
			if (types == null) {
				if (count > 1)
					throw new AmbiguousMatchException ();
				return found;
			}
			match = new MethodBase [count];
			if (count == 1)
				match [0] = found;
			else {
				count = 0;
				foreach (ConstructorInfo m in methods) {
					if (callConvention != CallingConventions.Any && m.CallingConvention != callConvention)
						continue;
					match [count++] = m;
				}
			}
			if (binder == null)
				binder = Binder.DefaultBinder;
			return (ConstructorInfo)binder.SelectMethod (bindingAttr, match, types, modifiers);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override ConstructorInfo[] GetConstructors (BindingFlags bindingAttr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern EventInfo InternalGetEvent (string name, BindingFlags bindingAttr);

		public override EventInfo GetEvent (string name, BindingFlags bindingAttr)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			return InternalGetEvent (name, bindingAttr);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override EventInfo[] GetEvents (BindingFlags bindingAttr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override FieldInfo GetField (string name, BindingFlags bindingAttr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override FieldInfo[] GetFields (BindingFlags bindingAttr);

		public override Type GetInterface (string name, bool ignoreCase)
		{
			if (name == null)
				throw new ArgumentNullException ();

			Type[] interfaces = GetInterfaces();

			foreach (Type type in interfaces) {
				if (String.Compare (type.Name, name, ignoreCase) == 0)
					return type;
				if (String.Compare (type.FullName, name, ignoreCase) == 0)
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
		public extern override MethodInfo[] GetMethods (BindingFlags bindingAttr);

		protected override MethodInfo GetMethodImpl (string name, BindingFlags bindingAttr,
							     Binder binder,
							     CallingConventions callConvention,
							     Type[] types, ParameterModifier[] modifiers)
		{
			MethodInfo[] methods = GetMethods (bindingAttr);
			MethodInfo found = null;
			MethodBase[] match;
			int count = 0;
			foreach (MethodInfo m in methods) {
				if (m.Name != name)
					continue;
				if (callConvention != CallingConventions.Any && m.CallingConvention != callConvention)
					continue;
				found = m;
				count++;
			}
			if (count == 0)
				return null;
			if (types == null) {
				if (count > 1)
					throw new AmbiguousMatchException ();
				return found;
			}
			match = new MethodBase [count];
			if (count == 1)
				match [0] = found;
			else {
				count = 0;
				foreach (MethodInfo m in methods) {
					if (m.Name != name)
						continue;
					if (callConvention != CallingConventions.Any && m.CallingConvention != callConvention)
						continue;
					match [count++] = m;
				}
			}
			if (binder == null)
				binder = Binder.DefaultBinder;
			return (MethodInfo)binder.SelectMethod (bindingAttr, match, types, modifiers);
		}
		
		public override Type GetNestedType( string name, BindingFlags bindingAttr)
		{
			// FIXME
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override Type[] GetNestedTypes (BindingFlags bindingAttr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override PropertyInfo[] GetProperties( BindingFlags bindingAttr);

		[MonoTODO]
		protected override PropertyInfo GetPropertyImpl (string name, BindingFlags bindingAttr,
								 Binder binder, Type returnType,
								 Type[] types,
								 ParameterModifier[] modifiers)
		{
			// fixme: needs to use the binder, and send the modifiers to that binder
			if (null == name || types == null)
				throw new ArgumentNullException ();
			
			PropertyInfo ret = null;
			PropertyInfo [] props = GetProperties(bindingAttr);

			foreach (PropertyInfo info in props) {
					if (info.Name != name) 
						continue;

					if (returnType != null)
						if (info.GetGetMethod().ReturnType != returnType)
							continue;

					if (types.Length > 0) {
						ParameterInfo[] parameterInfo = info.GetIndexParameters ();

						if (parameterInfo.Length != types.Length)
							continue;

						int i;
						bool match = true;

						for (i = 0; i < types.Length; i ++)
							if (parameterInfo [i].ParameterType != types [i]) {
								match = false;
								break;
							}

						if (!match)
							continue;
					}

					if (null != ret)
						throw new AmbiguousMatchException();

					ret = info;
			}

			return ret;
		}

		protected override bool HasElementTypeImpl ()
		{
			return IsArrayImpl() || IsByRefImpl() || IsPointerImpl ();
		}

		protected override bool IsArrayImpl ()
		{
			return type_is_subtype_of (this, typeof (System.Array), false) && this != typeof (System.Array);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern override bool IsByRefImpl ();

		protected override bool IsCOMObjectImpl ()
		{
			return false;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern override bool IsPointerImpl ();

		protected override bool IsPrimitiveImpl ()
		{
			MonoTypeInfo info;

			get_type_info (_impl, out info);
			return info.isprimitive;
		}

		protected override bool IsValueTypeImpl ()
		{
			return type_is_subtype_of (this, typeof (System.ValueType), false) &&
				this != typeof (System.ValueType) &&
				this != typeof (System.Enum);
		}
		
		public override object InvokeMember (string name, BindingFlags invokeAttr,
						     Binder binder, object target, object[] args,
						     ParameterModifier[] modifiers,
						     CultureInfo culture, string[] namedParameters)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			if ((invokeAttr & BindingFlags.CreateInstance) != 0) {
				if ((invokeAttr & (BindingFlags.GetField |
						BindingFlags.GetField | BindingFlags.GetProperty |
						BindingFlags.SetProperty)) != 0)
					throw new ArgumentException ("invokeAttr");
			}
			if ((invokeAttr & BindingFlags.GetField) != 0 && (invokeAttr & BindingFlags.SetField) != 0)
				throw new ArgumentException ("invokeAttr");
			if ((invokeAttr & BindingFlags.GetProperty) != 0 && (invokeAttr & BindingFlags.SetProperty) != 0)
				throw new ArgumentException ("invokeAttr");
			if ((invokeAttr & BindingFlags.InvokeMethod) != 0 && (invokeAttr & (BindingFlags.SetProperty|BindingFlags.SetField)) != 0)
				throw new ArgumentException ("invokeAttr");
			if ((invokeAttr & BindingFlags.SetField) != 0 && ((args == null) || args.Length != 1))
				throw new ArgumentException ("invokeAttr");
			if ((namedParameters != null) && ((args == null) || args.Length < namedParameters.Length))
				throw new ArgumentException ("namedParameters");

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
				if (ctor == null)
					throw new MissingMethodException ();
				object result = ctor.Invoke (target, invokeAttr, binder, args, culture);
				binder.ReorderArgumentArray (ref args, state);
				return result;
			}
			if (name == String.Empty && Attribute.IsDefined (this, typeof (DefaultMemberAttribute))) {
				DefaultMemberAttribute attr = (DefaultMemberAttribute) Attribute.GetCustomAttribute (this, typeof (DefaultMemberAttribute));
				name = attr.MemberName;
			}
			bool ignoreCase = (invokeAttr & BindingFlags.IgnoreCase) != 0;
			if ((invokeAttr & BindingFlags.InvokeMethod) != 0) {
				MethodInfo[] methods = GetMethods (invokeAttr);
				object state = null;
				int i, count = 0;
				for (i = 0; i < methods.Length; ++i) {
					if (String.Compare (methods [i].Name, name, ignoreCase) == 0)
						count++;
				}
				MethodBase[] smethods = new MethodBase [count];
				count = 0;
				for (i = 0; i < methods.Length; ++i) {
					if (String.Compare (methods [i].Name, name, ignoreCase) == 0)
						smethods [count++] = methods [i];
				}
				MethodBase m = binder.BindToMethod (invokeAttr, smethods, ref args, modifiers, culture, namedParameters, out state);
				if (m == null)
					throw new MissingMethodException ();
				object result = m.Invoke (target, invokeAttr, binder, args, culture);
				binder.ReorderArgumentArray (ref args, state);
				return result;
			}
			if ((invokeAttr & BindingFlags.GetField) != 0) {
				FieldInfo f = GetField (name, invokeAttr);
				if (f != null) {
					return f.GetValue (target);
				} else if ((invokeAttr & BindingFlags.GetProperty) == 0) {
					throw new MissingFieldException ();
				}
				/* try GetProperty */
			} else if ((invokeAttr & BindingFlags.SetField) != 0) {
				FieldInfo f = GetField (name, invokeAttr);
				if (f != null) {
					f.SetValue (target, args [0]);
					return null;
				} else if ((invokeAttr & BindingFlags.SetProperty) == 0) {
					throw new MissingFieldException ();
				}
				/* try SetProperty */
			}
			if ((invokeAttr & BindingFlags.GetProperty) != 0) {
				PropertyInfo[] properties = GetProperties (invokeAttr);
				object state = null;
				int i, count = 0;
				for (i = 0; i < properties.Length; ++i) {
					if (String.Compare (properties [i].Name, name, ignoreCase) == 0 && (properties [i].GetGetMethod () != null))
						count++;
				}
				MethodBase[] smethods = new MethodBase [count];
				count = 0;
				for (i = 0; i < properties.Length; ++i) {
					MethodBase mb = properties [i].GetGetMethod ();
					if (String.Compare (properties [i].Name, name, ignoreCase) == 0 && (mb != null))
						smethods [count++] = mb;
				}
				MethodBase m = binder.BindToMethod (invokeAttr, smethods, ref args, modifiers, culture, namedParameters, out state);
				if (m == null)
					throw new MissingFieldException ();
				object result = m.Invoke (target, invokeAttr, binder, args, culture);
				binder.ReorderArgumentArray (ref args, state);
				return result;
			} else if ((invokeAttr & BindingFlags.SetProperty) != 0) {
				PropertyInfo[] properties = GetProperties (invokeAttr);
				object state = null;
				int i, count = 0;
				for (i = 0; i < properties.Length; ++i) {
					if (String.Compare (properties [i].Name, name, ignoreCase) == 0 && (properties [i].GetSetMethod () != null))
						count++;
				}
				MethodBase[] smethods = new MethodBase [count];
				count = 0;
				for (i = 0; i < properties.Length; ++i) {
					MethodBase mb = properties [i].GetSetMethod ();
					if (String.Compare (properties [i].Name, name, ignoreCase) == 0 && (mb != null))
						smethods [count++] = mb;
				}
				MethodBase m = binder.BindToMethod (invokeAttr, smethods, ref args, modifiers, culture, namedParameters, out state);
				if (m == null)
					throw new MissingFieldException ();
				object result = m.Invoke (target, invokeAttr, binder, args, culture);
				binder.ReorderArgumentArray (ref args, state);
				return result;
			}
			return null;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override Type GetElementType ();

		public override Type UnderlyingSystemType {
			get {
				MonoTypeInfo info;
				get_type_info (_impl, out info);
				return info.etype;
			}
		}

		public override Assembly Assembly {
			get {
				MonoTypeInfo info;
				get_type_info (_impl, out info);
				return info.assembly;
			}
		}

		public override string AssemblyQualifiedName {
			get {
				return getFullName () + ", " + Assembly.ToString ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern string getFullName();

		public extern override Type BaseType {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public override string FullName {
			get {
				return getFullName ();
			}
		}

		public override Guid GUID {
			get {
				return Guid.Empty;
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
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
		}

		public override MemberTypes MemberType {
			get {
				MonoTypeInfo info;
				get_type_info (_impl, out info);
				return info.nested_in == null? MemberTypes.TypeInfo: MemberTypes.NestedType;
			}
		}

		public override string Name {
			get {
				MonoTypeInfo info;
				get_type_info (_impl, out info);
				return info.name;
			}
		}

		public override string Namespace {
			get {
				MonoTypeInfo info;
				get_type_info (_impl, out info);
				if (info.nested_in == null)
					return info.name_space;
				else
					return info.nested_in.Namespace;
			}
		}

		public extern override Module Module {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public override Type DeclaringType {
			get {
				MonoTypeInfo info;
				get_type_info (_impl, out info);
				return info.nested_in;
			}
		}

		public override Type ReflectedType {
			get {
				MonoTypeInfo info;
				get_type_info (_impl, out info);
				return info.nested_in;
			}
		}

		public override RuntimeTypeHandle TypeHandle {
			get {
				return _impl;
			}
		}

		public override int GetArrayRank ()
		{
			MonoTypeInfo info;
			
			get_type_info (_impl, out info);
			return info.rank;
		}
	}
}
