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
using System.Collections;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Reflection
{
	internal class MonoGenericInst : MonoType
	{
		private IntPtr klass;
		protected MonoGenericInst parent;
		protected Type generic_type;
		private MethodInfo[] methods;
		private ConstructorInfo[] ctors;
		private FieldInfo[] fields;

		[MonoTODO]
		internal MonoGenericInst ()
			: base (null)
		{
			// this should not be used
			throw new InvalidOperationException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern MethodInfo inflate_method (MethodInfo method);
	
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern ConstructorInfo inflate_ctor (ConstructorInfo ctor);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern FieldInfo inflate_field (FieldInfo field);

		private const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
		BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

		void initialize ()
		{
			methods = generic_type.GetMethods (flags);
			for (int i = 0; i < methods.Length; i++)
				methods [i] = inflate_method (methods [i]);

			ctors = generic_type.GetConstructors (flags);
			for (int i = 0; i < ctors.Length; i++)
				ctors [i] = inflate_ctor (ctors [i]);

			fields = generic_type.GetFields (flags);
			for (int i = 0; i < fields.Length; i++)
				fields [i] = inflate_field (fields [i]);
		}

		public override Type BaseType {
			get { return parent != null ? parent : generic_type.BaseType; }
		}

		protected override bool IsValueTypeImpl ()
		{
			if (BaseType == typeof (Enum) || BaseType == typeof (ValueType))
				return true;

			return BaseType.IsSubclassOf (typeof (ValueType));
		}

		public override MethodInfo[] GetMethods (BindingFlags bindingAttr)
		{
			if (methods == null)
				initialize ();

			ArrayList list = new ArrayList ();
			BindingFlags new_bf = bindingAttr | BindingFlags.DeclaredOnly;

			if ((bindingAttr & BindingFlags.DeclaredOnly) == 0) {
				Type current = BaseType;
				while (current != null) {
					list.AddRange (current.GetMethods (new_bf));
					current = current.BaseType;
				}
			}

			list.AddRange (GetMethods_impl (new_bf));

			MethodInfo[] res = new MethodInfo [list.Count];
			list.CopyTo (res, 0);
			return res;
		}

		protected MethodInfo[] GetMethods_impl (BindingFlags bindingAttr)
		{
			ArrayList l = new ArrayList ();
			bool match;
			MethodAttributes mattrs;

			foreach (MethodInfo c in methods) {
				match = false;
				mattrs = c.Attributes;
				if ((mattrs & MethodAttributes.MemberAccessMask) == MethodAttributes.Public) {
					if ((bindingAttr & BindingFlags.Public) != 0)
						match = true;
				} else {
					if ((bindingAttr & BindingFlags.NonPublic) != 0)
						match = true;
				}
				if (!match)
					continue;
				match = false;
				if ((mattrs & MethodAttributes.Static) != 0) {
					if ((bindingAttr & BindingFlags.Static) != 0)
						match = true;
				} else {
					if ((bindingAttr & BindingFlags.Instance) != 0)
						match = true;
				}
				if (!match)
					continue;
				l.Add (c);
			}
			MethodInfo[] result = new MethodInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		public override ConstructorInfo[] GetConstructors (BindingFlags bindingAttr)
		{
			if (ctors == null)
				initialize ();

			ArrayList list = new ArrayList ();
			BindingFlags new_bf = bindingAttr | BindingFlags.DeclaredOnly;

			if ((bindingAttr & BindingFlags.DeclaredOnly) == 0) {
				Type current = BaseType;
				while (current != null) {
					list.AddRange (current.GetConstructors (new_bf));
					current = current.BaseType;
				}
			}

			list.AddRange (GetConstructors_impl (new_bf));

			ConstructorInfo[] res = new ConstructorInfo [list.Count];
			list.CopyTo (res, 0);
			return res;
		}

		protected ConstructorInfo[] GetConstructors_impl (BindingFlags bindingAttr)
		{
			ArrayList l = new ArrayList ();
			bool match;
			MethodAttributes mattrs;

			foreach (ConstructorInfo c in ctors) {
				match = false;
				mattrs = c.Attributes;
				if ((mattrs & MethodAttributes.MemberAccessMask) == MethodAttributes.Public) {
					if ((bindingAttr & BindingFlags.Public) != 0)
						match = true;
				} else {
					if ((bindingAttr & BindingFlags.NonPublic) != 0)
						match = true;
				}
				if (!match)
					continue;
				match = false;
				if ((mattrs & MethodAttributes.Static) != 0) {
					if ((bindingAttr & BindingFlags.Static) != 0)
						match = true;
				} else {
					if ((bindingAttr & BindingFlags.Instance) != 0)
						match = true;
				}
				if (!match)
					continue;
				l.Add (c);
			}
			ConstructorInfo[] result = new ConstructorInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		public override FieldInfo[] GetFields (BindingFlags bindingAttr)
		{
			if (fields == null)
				initialize ();

			ArrayList list = new ArrayList ();
			BindingFlags new_bf = bindingAttr | BindingFlags.DeclaredOnly;

			if ((bindingAttr & BindingFlags.DeclaredOnly) == 0) {
				Type current = BaseType;
				while (current != null) {
					list.AddRange (current.GetFields (new_bf));
					current = current.BaseType;
				}
			}

			list.AddRange (GetFields_impl (new_bf));

			FieldInfo[] res = new FieldInfo [list.Count];
			list.CopyTo (res, 0);
			return res;
		}

		protected FieldInfo[] GetFields_impl (BindingFlags bindingAttr)
		{
			ArrayList l = new ArrayList ();
			bool match;
			FieldAttributes fattrs;

			foreach (FieldInfo c in fields) {
				match = false;
				fattrs = c.Attributes;
				if ((fattrs & FieldAttributes.FieldAccessMask) == FieldAttributes.Public) {
					if ((bindingAttr & BindingFlags.Public) != 0)
						match = true;
				} else {
					if ((bindingAttr & BindingFlags.NonPublic) != 0)
						match = true;
				}
				if (!match)
					continue;
				match = false;
				if ((fattrs & FieldAttributes.Static) != 0) {
					if ((bindingAttr & BindingFlags.Static) != 0)
						match = true;
				} else {
					if ((bindingAttr & BindingFlags.Instance) != 0)
						match = true;
				}
				if (!match)
					continue;
				l.Add (c);
			}
			FieldInfo[] result = new FieldInfo [l.Count];
			l.CopyTo (result);
			return result;
		}
	}

	internal class MonoInflatedMethod : MonoMethod
	{
		private readonly MethodInfo declaring;
		private readonly Type declaring_type;

		public override Type DeclaringType {
			get {
				return declaring_type != null ? declaring_type : base.DeclaringType;
			}
		}

		public override Type ReflectedType {
			get {
				return declaring_type != null ? declaring_type : base.ReflectedType;
			}
		}

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			// FIXME
			return false;
		}
		public override object[] GetCustomAttributes (bool inherit)
		{
			// FIXME
			return new object [0];
		}
		public override object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			// FIXME
			return new object [0];
		}
	}

	internal class MonoInflatedCtor : MonoCMethod
	{
		private readonly ConstructorInfo declaring;
		private readonly Type declaring_type;

		public override Type DeclaringType {
			get {
				return declaring_type != null ? declaring_type : base.DeclaringType;
			}
		}

		public override Type ReflectedType {
			get {
				return declaring_type != null ? declaring_type : base.ReflectedType;
			}
		}

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			// FIXME
			return false;
		}
		public override object[] GetCustomAttributes (bool inherit)
		{
			// FIXME
			return new object [0];
		}
		public override object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			// FIXME
			return new object [0];
		}
	}

	internal class MonoInflatedField : MonoField
	{
		private readonly IntPtr dhandle;
	}
}
