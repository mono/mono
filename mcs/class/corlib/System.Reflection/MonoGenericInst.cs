//
// System.MonoType
//
// Sean MacIsaac (macisaac@ximian.com)
// Paolo Molaro (lupus@ximian.com)
// Patrik Torstensson (patrik.torstensson@labs2.com)
//
// (C) 2001 Ximian, Inc.
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
		protected extern void initialize (MethodInfo[] methods, ConstructorInfo[] ctors, FieldInfo[] fields, PropertyInfo[] properties, EventInfo[] events);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern MethodInfo[] GetMethods_internal (Type reflected_type);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern ConstructorInfo[] GetConstructors_internal (Type reflected_type);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern FieldInfo[] GetFields_internal (Type reflected_type);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern PropertyInfo[] GetProperties_internal (Type reflected_type);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern EventInfo[] GetEvents_internal (Type reflected_type);

		private const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
		BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

		EventInfo[] get_event_info ()
		{
			if (generic_type is TypeBuilder)
				return ((TypeBuilder) generic_type).GetEvents_internal (flags);
			else
				return generic_type.GetEvents (flags);
		}

		void initialize ()
		{
			if (initialized)
				return;

			MonoGenericInst parent = GetParentType ();
			if (parent != null)
				parent.initialize ();

			initialize (generic_type.GetMethods (flags),
				    generic_type.GetConstructors (flags),
				    generic_type.GetFields (flags),
				    generic_type.GetProperties (flags),
				    get_event_info ());

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
			ArrayList l = new ArrayList ();

			//
			// Walk up our class hierarchy and retrieve methods from our
			// parent classes.
			//

			Type current_type = this;
			do {
				MonoGenericInst gi = current_type as MonoGenericInst;
				if (gi != null)
					l.AddRange (gi.GetMethods_impl (bf, this));
				else if (current_type is TypeBuilder)
					l.AddRange (current_type.GetMethods (bf));
				else {
					// If we encounter a `MonoType', its
					// GetMethodsByName() will return all the methods
					// from its parent type(s), so we can stop here.
					MonoType mt = (MonoType) current_type;
					l.AddRange (mt.GetMethodsByName (null, bf, false, this));
					break;
				}

				if ((bf & BindingFlags.DeclaredOnly) != 0)
					break;
				current_type = current_type.BaseType;
			} while (current_type != null);

			MethodInfo[] result = new MethodInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		protected MethodInfo[] GetMethods_impl (BindingFlags bf, Type reftype)
		{
			ArrayList l = new ArrayList ();
			bool match;
			MethodAttributes mattrs;

			initialize ();

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
			ArrayList l = new ArrayList ();

			Type current_type = this;
			do {
				MonoGenericInst gi = current_type as MonoGenericInst;
				if (gi != null)
					l.AddRange (gi.GetConstructors_impl (bf, this));
				else if (current_type is TypeBuilder)
					l.AddRange (current_type.GetConstructors (bf));
				else {
					MonoType mt = (MonoType) current_type;
					l.AddRange (mt.GetConstructors_internal (bf, this));
					break;
				}

				if ((bf & BindingFlags.DeclaredOnly) != 0)
					break;
				current_type = current_type.BaseType;
			} while (current_type != null);

			ConstructorInfo[] result = new ConstructorInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		protected ConstructorInfo[] GetConstructors_impl (BindingFlags bf, Type reftype)
		{
			ArrayList l = new ArrayList ();
			bool match;
			MethodAttributes mattrs;

			initialize ();

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
			ArrayList l = new ArrayList ();

			Type current_type = this;
			do {
				MonoGenericInst gi = current_type as MonoGenericInst;
				if (gi != null)
					l.AddRange (gi.GetFields_impl (bf, this));
				else if (current_type is TypeBuilder)
					l.AddRange (current_type.GetFields (bf));
				else {
					MonoType mt = (MonoType) current_type;
					l.AddRange (mt.GetFields_internal (bf, this));
					break;
				}

				if ((bf & BindingFlags.DeclaredOnly) != 0)
					break;
				current_type = current_type.BaseType;
			} while (current_type != null);

			FieldInfo[] result = new FieldInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		protected FieldInfo[] GetFields_impl (BindingFlags bf, Type reftype)
		{
			ArrayList l = new ArrayList ();
			bool match;
			FieldAttributes fattrs;

			initialize ();

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

		public override PropertyInfo[] GetProperties (BindingFlags bf)
		{
			ArrayList l = new ArrayList ();

			Type current_type = this;
			do {
				MonoGenericInst gi = current_type as MonoGenericInst;
				if (gi != null)
					l.AddRange (gi.GetProperties_impl (bf, this));
				else if (current_type is TypeBuilder)
					l.AddRange (current_type.GetProperties (bf));
				else {
					MonoType mt = (MonoType) current_type;
					l.AddRange (mt.GetPropertiesByName (null, bf, false, this));
					break;
				}

				if ((bf & BindingFlags.DeclaredOnly) != 0)
					break;
				current_type = current_type.BaseType;
			} while (current_type != null);

			PropertyInfo[] result = new PropertyInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		protected PropertyInfo[] GetProperties_impl (BindingFlags bf, Type reftype)
		{
			ArrayList l = new ArrayList ();
			bool match;
			MethodAttributes mattrs;
			MethodInfo accessor;

			initialize ();

			PropertyInfo[] properties = GetProperties_internal (reftype);

			for (int i = 0; i < properties.Length; i++) {
				PropertyInfo c = properties [i];

				match = false;
				accessor = c.GetGetMethod (true);
				if (accessor == null)
					accessor = c.GetSetMethod (true);
				if (accessor == null)
					continue;
				mattrs = accessor.Attributes;
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
			PropertyInfo[] result = new PropertyInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		public override EventInfo[] GetEvents (BindingFlags bf)
		{
			ArrayList l = new ArrayList ();

			Type current_type = this;
			do {
				MonoGenericInst gi = current_type as MonoGenericInst;
				if (gi != null)
					l.AddRange (gi.GetEvents_impl (bf, this));
				else if (current_type is TypeBuilder)
					l.AddRange (current_type.GetEvents (bf));
				else {
					MonoType mt = (MonoType) current_type;
					l.AddRange (mt.GetEvents (bf));
					break;
				}

				if ((bf & BindingFlags.DeclaredOnly) != 0)
					break;
				current_type = current_type.BaseType;
			} while (current_type != null);

			EventInfo[] result = new EventInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		protected EventInfo[] GetEvents_impl (BindingFlags bf, Type reftype)
		{
			ArrayList l = new ArrayList ();
			bool match;
			MethodAttributes mattrs;
			MethodInfo accessor;

			initialize ();

			EventInfo[] events = GetEvents_internal (reftype);

			for (int i = 0; i < events.Length; i++) {
				EventInfo c = events [i];

				match = false;
				accessor = c.GetAddMethod (true);
				if (accessor == null)
					accessor = c.GetRemoveMethod (true);
				if (accessor == null)
					continue;
				mattrs = accessor.Attributes;
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
			EventInfo[] result = new EventInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		public override Type[] GetNestedTypes (BindingFlags bf)
		{
			return generic_type.GetNestedTypes (bf);
		}
	}
}
