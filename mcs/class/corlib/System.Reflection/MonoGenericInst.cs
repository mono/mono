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
using System.Reflection.Emit;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Reflection
{
	internal class MonoGenericInst : MonoType
	{
		protected Type generic_type;
		bool initialized;

		[MonoTODO]
		internal MonoGenericInst ()
			: base (null)
		{
			// this should not be used
			throw new InvalidOperationException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern void initialize (MethodInfo[] methods, ConstructorInfo[] ctors, FieldInfo[] fields);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern MethodInfo[] GetMethods_internal (Type reflected_type);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern ConstructorInfo[] GetConstructors_internal (Type reflected_type);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern FieldInfo[] GetFields_internal (Type reflected_type);

		private const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
		BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

		void initialize ()
		{
			if (initialized)
				return;

			MonoGenericInst parent = GetParentType ();
			if (parent != null)
				parent.initialize ();

			initialize (generic_type.GetMethods (flags),
				    generic_type.GetConstructors (flags),
				    generic_type.GetFields (flags));

			initialized = true;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern MonoGenericInst GetParentType ();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern MonoGenericInst[] GetInterfaces_internal ();

		public override Type BaseType {
			get {
				MonoGenericInst parent = GetParentType ();
				return parent != null ? parent : generic_type.BaseType;
			}
		}

		public override Type DeclaringType {
			get { return generic_type.DeclaringType; }
		}

		public override Type[] GetInterfaces ()
		{
			return GetInterfaces_internal ();
		}

		protected override bool IsValueTypeImpl ()
		{
			return generic_type.IsValueType;
		}

		public override MethodInfo[] GetMethods (BindingFlags bf)
		{
			initialize ();
			return GetMethods_impl (bf, this);
		}

		protected MethodInfo[] GetMethods_impl (BindingFlags bf, Type reftype)
		{
			ArrayList l = new ArrayList ();
			bool match;
			MethodAttributes mattrs;

			if ((bf & BindingFlags.DeclaredOnly) == 0) {
				MonoGenericInst parent = GetParentType ();
				if (parent != null)
					l.AddRange (parent.GetMethods_impl (bf, reftype));
			}

			MethodInfo[] methods = GetMethods_internal (reftype);

			for (int i = 0; i < methods.Length; i++) {
				MethodInfo c = methods [i];

				match = false;
				mattrs = c.Attributes;
				if ((mattrs & MethodAttributes.MemberAccessMask) == MethodAttributes.Public) {
					if ((bf & BindingFlags.Public) != 0)
						match = true;
				} else {
					if ((bf & BindingFlags.NonPublic) != 0)
						match = true;
				}
				if (!match)
					continue;
				match = false;
				if ((mattrs & MethodAttributes.Static) != 0) {
					if ((bf & BindingFlags.Static) != 0)
						match = true;
				} else {
					if ((bf & BindingFlags.Instance) != 0)
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

		public override ConstructorInfo[] GetConstructors (BindingFlags bf)
		{
			initialize ();
			return GetConstructors_impl (bf, this);
		}

		protected ConstructorInfo[] GetConstructors_impl (BindingFlags bf, Type reftype)
		{
			ArrayList l = new ArrayList ();
			bool match;
			MethodAttributes mattrs;

			if ((bf & BindingFlags.DeclaredOnly) == 0) {
				MonoGenericInst parent = GetParentType ();
				if (parent != null)
					l.AddRange (parent.GetConstructors_impl (bf, reftype));
			}

			ConstructorInfo[] ctors = GetConstructors_internal (reftype);

			for (int i = 0; i < ctors.Length; i++) {
				ConstructorInfo c = ctors [i];

				match = false;
				mattrs = c.Attributes;
				if ((mattrs & MethodAttributes.MemberAccessMask) == MethodAttributes.Public) {
					if ((bf & BindingFlags.Public) != 0)
						match = true;
				} else {
					if ((bf & BindingFlags.NonPublic) != 0)
						match = true;
				}
				if (!match)
					continue;
				match = false;
				if ((mattrs & MethodAttributes.Static) != 0) {
					if ((bf & BindingFlags.Static) != 0)
						match = true;
				} else {
					if ((bf & BindingFlags.Instance) != 0)
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

		public override FieldInfo[] GetFields (BindingFlags bf)
		{
			initialize ();
			return GetFields_impl (bf, this);
		}

		protected FieldInfo[] GetFields_impl (BindingFlags bf, Type reftype)
		{
			ArrayList l = new ArrayList ();
			bool match;
			FieldAttributes fattrs;

			if ((bf & BindingFlags.DeclaredOnly) == 0) {
				MonoGenericInst parent = GetParentType ();
				if (parent != null)
					l.AddRange (parent.GetFields_impl (bf, reftype));
			}

			FieldInfo[] fields = GetFields_internal (reftype);

			for (int i = 0; i < fields.Length; i++) {
				FieldInfo c = fields [i];

				match = false;
				fattrs = c.Attributes;
				if ((fattrs & FieldAttributes.FieldAccessMask) == FieldAttributes.Public) {
					if ((bf & BindingFlags.Public) != 0)
						match = true;
				} else {
					if ((bf & BindingFlags.NonPublic) != 0)
						match = true;
				}
				if (!match)
					continue;
				match = false;
				if ((fattrs & FieldAttributes.Static) != 0) {
					if ((bf & BindingFlags.Static) != 0)
						match = true;
				} else {
					if ((bf & BindingFlags.Instance) != 0)
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

	internal class MonoGenericParam : MonoType
	{
		private object refobj;
		private int index;
		private string name;
		private int flags;
		private Type[] constraints;
		bool initialized;

		[MonoTODO]
		internal MonoGenericParam ()
			: base (null)
		{
			// this should not be used
			throw new InvalidOperationException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void initialize ();

		public void SetConstraints (Type[] constraints)
		{
			this.constraints = constraints;
			initialize ();
		}

		public override Type BaseType {
			get {
				if (!initialized)
					throw new InvalidOperationException ();
				if ((constraints.Length == 0) || constraints [0].IsInterface)
					return null;
				else
					return constraints [0];
			}
		}

		public override Type[] GetInterfaces ()
		{
			if (!initialized)
				throw new InvalidOperationException ();

			if ((constraints.Length == 0) || constraints [0].IsInterface)
				return constraints;
			else {
				Type[] ret = new Type [constraints.Length-1];
				Array.Copy (constraints, 1, ret, 0, constraints.Length-1);
				return ret;
			}
		}
	}
}
