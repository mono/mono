//
// System.Reflection.MonoGenericClass
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

#if NET_2_0 || BOOTSTRAP_NET_2_0

namespace System.Reflection
{
	/*
	 * MonoGenericClass represents an instantiation of a generic TypeBuilder. MS
	 * calls this class TypeBuilderInstantiation (a much better name). MS returns 
	 * NotImplementedException for many of the methods but we can't do that as gmcs
	 * depends on them.
	 */
	internal class MonoGenericClass : MonoType
	{
		#region Keep in sync with object-internals.h
#pragma warning disable 649
		internal TypeBuilder generic_type;
		bool initialized;
#pragma warning restore 649
		#endregion

#if NET_2_0
		Hashtable fields, ctors, methods;
#endif

		internal MonoGenericClass ()
			: base (null)
		{
			// this should not be used
			throw new InvalidOperationException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern void initialize (MethodInfo[] methods, ConstructorInfo[] ctors, FieldInfo[] fields, PropertyInfo[] properties, EventInfo[] events);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern MethodInfo GetCorrespondingInflatedMethod (MethodInfo generic);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern FieldInfo GetCorrespondingInflatedField (string generic);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern ConstructorInfo GetCorrespondingInflatedConstructor (ConstructorInfo generic);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern ConstructorInfo[] GetConstructors_internal (Type reflected_type);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern FieldInfo[] GetFields_internal (Type reflected_type);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern EventInfo[] GetEvents_internal (Type reflected_type);

		private const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
		BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

		void initialize ()
		{
			if (initialized)
				return;

			MonoGenericClass parent = GetParentType () as MonoGenericClass;
			if (parent != null)
				parent.initialize ();

			initialize (generic_type.GetMethods (flags),
						generic_type.GetConstructors (flags),
						generic_type.GetFields (flags),
						generic_type.GetProperties (flags),
						generic_type.GetEvents_internal (flags));

			initialized = true;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern Type GetParentType ();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern Type InflateType_internal (Type type);

		internal Type InflateType (Type type)
		{
			if (type == null)
				return null;
			if (!type.IsGenericParameter && !type.ContainsGenericParameters)
				return type;
			return InflateType_internal (type);
		}
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern MonoGenericClass[] GetInterfaces_internal ();

		public override Type BaseType {
			get {
				Type parent = GetParentType ();
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

		internal override MethodInfo GetMethod (MethodInfo fromNoninstanciated)
		{
			initialize ();

#if NET_2_0
			if (fromNoninstanciated is MethodOnTypeBuilderInst) {
				MethodOnTypeBuilderInst mbinst = (MethodOnTypeBuilderInst)fromNoninstanciated;
				if (((ModuleBuilder)mbinst.mb.Module).assemblyb.IsCompilerContext)
					fromNoninstanciated = mbinst.mb;
				else
					throw new ArgumentException ("method declaring type is not the generic type definition of type", "method");
			}

			if (fromNoninstanciated is MethodBuilder) {
				MethodBuilder mb = (MethodBuilder)fromNoninstanciated;

				// FIXME: We can't yet handle creating generic instantiations of
				// MethodOnTypeBuilderInst objects
				// Also, mono_image_get_method_on_inst_token () can't handle generic
				// methods
				if (!mb.IsGenericMethodDefinition) {
					if (methods == null)
						methods = new Hashtable ();
					if (!methods.ContainsKey (mb))
						methods [mb] = new MethodOnTypeBuilderInst (this, mb);
					return (MethodInfo)methods [mb];
				}
			}
#endif

			return GetCorrespondingInflatedMethod (fromNoninstanciated);
		}

		internal override ConstructorInfo GetConstructor (ConstructorInfo fromNoninstanciated)
		{
			initialize ();
		
#if NET_2_0
			if (fromNoninstanciated is ConstructorBuilder) {
				ConstructorBuilder cb = (ConstructorBuilder)fromNoninstanciated;
				if (ctors == null)
					ctors = new Hashtable ();
				if (!ctors.ContainsKey (cb))
					ctors [cb] = new ConstructorOnTypeBuilderInst (this, cb);
				return (ConstructorInfo)ctors [cb];
			}
			
#endif
			return GetCorrespondingInflatedConstructor (fromNoninstanciated);
		}

		internal override FieldInfo GetField (FieldInfo fromNoninstanciated)
		{
			initialize ();

#if NET_2_0
			if (fromNoninstanciated is FieldBuilder) {
				FieldBuilder fb = (FieldBuilder)fromNoninstanciated;
				if (fields == null)
					fields = new Hashtable ();
				if (!fields.ContainsKey (fb))
					fields [fb] = new FieldOnTypeBuilderInst (this, fb);
				return (FieldInfo)fields [fb];
			}
#endif
			return GetCorrespondingInflatedField (fromNoninstanciated.Name);
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
				MonoGenericClass gi = current_type as MonoGenericClass;
				if (gi != null)
					l.AddRange (gi.GetMethodsInternal (bf, this));
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

		MethodInfo[] GetMethodsInternal (BindingFlags bf, MonoGenericClass reftype)
		{
			if (generic_type.num_methods == 0)
				return new MethodInfo [0];

			ArrayList l = new ArrayList ();
			bool match;
			MethodAttributes mattrs;
			MethodInfo accessor;

			initialize ();

			for (int i = 0; i < generic_type.num_methods; ++i) {
				MethodInfo c = generic_type.methods [i];

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
				c = TypeBuilder.GetMethod (this, c);
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
				MonoGenericClass gi = current_type as MonoGenericClass;
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
				MonoGenericClass gi = current_type as MonoGenericClass;
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
				MonoGenericClass gi = current_type as MonoGenericClass;
				if (gi != null)
					l.AddRange (gi.GetPropertiesInternal (bf, this));
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

		PropertyInfo[] GetPropertiesInternal (BindingFlags bf, MonoGenericClass reftype)
		{
			if (generic_type.properties == null)
				return new PropertyInfo [0];

			ArrayList l = new ArrayList ();
			bool match;
			MethodAttributes mattrs;
			MethodInfo accessor;

			initialize ();

			foreach (PropertyInfo pinfo in generic_type.properties) {
				match = false;
				accessor = pinfo.GetGetMethod (true);
				if (accessor == null)
					accessor = pinfo.GetSetMethod (true);
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
				l.Add (new PropertyOnTypeBuilderInst (reftype, pinfo));
			}
			PropertyInfo[] result = new PropertyInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		protected override PropertyInfo GetPropertyImpl (string name, BindingFlags bindingAttr,
								 Binder binder, Type returnType,
								 Type[] types,
								 ParameterModifier[] modifiers)
		{
			bool ignoreCase = ((bindingAttr & BindingFlags.IgnoreCase) != 0);
			PropertyInfo [] props = GetProperties (bindingAttr);

			ArrayList al = null;
			for (int i = 0; i < props.Length; ++i) {
				if (String.Compare (props [i].Name, name, ignoreCase) == 0) {
					if (al == null)
						al = new ArrayList ();
					al.Add (props [i]);
				}
			}
			if (al == null)
				return null;

			props = (PropertyInfo[])al.ToArray (typeof (PropertyInfo));
			
			int count = props.Length;
			
			if (count == 1 && (types == null || types.Length == 0) &&
			    (returnType == null || returnType == props[0].PropertyType))
				return props [0];

			if (binder == null)
				binder = Binder.DefaultBinder;
			
			return binder.SelectProperty (bindingAttr, props, returnType, types, modifiers);
		}

		public override EventInfo[] GetEvents (BindingFlags bf)
		{
			ArrayList l = new ArrayList ();

			Type current_type = this;
			do {
				MonoGenericClass gi = current_type as MonoGenericClass;
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

		public override bool IsAssignableFrom (Type c)
		{
			if (c == this)
				return true;

			MonoGenericClass[] interfaces = GetInterfaces_internal ();

			if (c.IsInterface) {
				if (interfaces == null)
					return false;
				foreach (Type t in interfaces)
					if (c.IsAssignableFrom (t))
						return true;
				return false;
			}

			Type parent = GetParentType ();
			if (parent == null)
				return c == typeof (object);
			else
				return c.IsAssignableFrom (parent);
		}
	}
}

#endif
