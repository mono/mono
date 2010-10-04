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
using System.Text;

namespace System.Reflection
{
	/*
	 * MonoGenericClass represents an instantiation of a generic TypeBuilder. MS
	 * calls this class TypeBuilderInstantiation (a much better name). MS returns 
	 * NotImplementedException for many of the methods but we can't do that as gmcs
	 * depends on them.
	 */
	internal class MonoGenericClass : Type
	{
		#region Keep in sync with object-internals.h
#pragma warning disable 649
		internal Type generic_type;
		Type[] type_arguments;
		bool initialized;
#pragma warning restore 649
		#endregion

		Hashtable fields, ctors, methods;
		int event_count;
		int is_compiler_context;

		internal MonoGenericClass ()
		{
			// this should not be used
			throw new InvalidOperationException ();
		}

		internal MonoGenericClass (Type tb, Type[] args)
		{
			this.generic_type = tb;
			this.type_arguments = args;
			/*
			This is a temporary hack until we can fix the rest of the runtime
			to properly handle this class to be a complete UT.

			We must not regisrer this with the runtime after the type is created
			otherwise created_type.MakeGenericType will return an instance of MonoGenericClass,
			which is very very broken.
			*/
			if (tb is TypeBuilder && !(tb as TypeBuilder).is_created)
				register_with_runtime (this);
			
		}


		internal override bool IsCompilerContext {
			get {
				if (is_compiler_context == 0) {
					bool is_cc = generic_type.IsCompilerContext;
					foreach (Type t in type_arguments)
						is_cc |= t.IsCompilerContext;
					is_compiler_context = is_cc ? 1 : -1;
				}
				return is_compiler_context == 1;
			}
		}

		internal override Type InternalResolve ()
		{
			Type gtd = generic_type.InternalResolve ();
			Type[] args = new Type [type_arguments.Length];
			for (int i = 0; i < type_arguments.Length; ++i)
				args [i] = type_arguments [i].InternalResolve ();
			return gtd.MakeGenericType (args);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern void initialize (MethodInfo[] methods, ConstructorInfo[] ctors, FieldInfo[] fields, PropertyInfo[] properties, EventInfo[] events);

 		[MethodImplAttribute(MethodImplOptions.InternalCall)]
 		internal static extern void register_with_runtime (Type type);

		EventInfo[] GetEventsFromGTD (BindingFlags flags) {
			TypeBuilder tb = generic_type as TypeBuilder;
			if (tb == null)
				return generic_type.GetEvents (flags);

			return tb.GetEvents_internal (flags);
		}

		ConstructorInfo[] GetConstructorsFromGTD (BindingFlags flags)
		{
			TypeBuilder tb = generic_type as TypeBuilder;
			if (tb == null)
				return generic_type.GetConstructors (flags);

			return tb.GetConstructorsInternal (flags);
		}

		/*
		MethodInfo[] GetMethodsFromGTD (BindingFlags bf)
		{
			TypeBuilder tb = generic_type as TypeBuilder;
			if (tb == null)
				return generic_type.GetMethods (bf);

			MethodInfo[] res = new MethodInfo [tb.num_methods];
			if (tb.num_methods > 0)
				Array.Copy (tb.methods, res, tb.num_methods);

			return res;
		}
		*/

		FieldInfo[] GetFieldsFromGTD (BindingFlags bf)
		{
			TypeBuilder tb = generic_type as TypeBuilder;
			if (tb == null)
				return generic_type.GetFields (bf);

			FieldInfo[] res = new FieldInfo [tb.num_fields];
			if (tb.num_fields > 0)
				Array.Copy (tb.fields, res, tb.num_fields);

			return res;
		}

		/*@hint might not be honored so it required aditional filtering
		TODO move filtering into here for the TypeBuilder case and remove the hint ugliness 
		*/
		MethodInfo[] GetMethodsFromGTDWithHint (BindingFlags hint)
		{
			TypeBuilder tb = generic_type as TypeBuilder;
			if (tb == null)
				return generic_type.GetMethods (hint);

			if (tb.num_methods == 0)
				return new MethodInfo [0];
			MethodInfo[] res = new MethodInfo [tb.num_methods];
			Array.Copy (tb.methods, 0, res, 0, tb.num_methods);
			return res;
		}

		/*@hint might not be honored so it required aditional filtering
		TODO move filtering into here for the TypeBuilder case and remove the hint ugliness 
		*/
		ConstructorInfo[] GetConstructorsFromGTDWithHint (BindingFlags hint)
		{
			TypeBuilder tb = generic_type as TypeBuilder;
			if (tb == null)
				return generic_type.GetConstructors (hint);

			if (tb.ctors == null)
				return new ConstructorInfo [0];
			ConstructorInfo[] res = new ConstructorInfo [tb.ctors.Length];
			tb.ctors.CopyTo (res, 0);
			return res;
		}

		static Type PeelType (Type t) {
			if (t.HasElementType)
				return PeelType (t.GetElementType ());
			if (t.IsGenericType && !t.IsGenericParameter)
				return t.GetGenericTypeDefinition ();
			return t;
		}

		static PropertyInfo[] GetPropertiesInternal (Type type, BindingFlags bf)
		{
			TypeBuilder tb = type as TypeBuilder;
			if (tb != null)
				return tb.properties;
			return type.GetProperties (bf);	
		}

		Type[] GetInterfacesFromGTD ()
		{
			TypeBuilder tb = generic_type as TypeBuilder;
			if (tb != null)
				return tb.interfaces;
			return generic_type.GetInterfaces ();	
		}

		internal bool IsCreated {
			get {
				TypeBuilder tb = generic_type as TypeBuilder;
				return tb != null ? tb.is_created : true;
			}
		}

		private const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
		BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

		void initialize ()
		{
			if (initialized)
				return;

			MonoGenericClass parent = GetParentType () as MonoGenericClass;
			if (parent != null)
				parent.initialize ();
			EventInfo[] events = GetEventsFromGTD (flags);
			event_count = events.Length;
				
			initialize (generic_type.GetMethods (flags),
						GetConstructorsFromGTD (flags),
						generic_type.GetFields (flags),
						generic_type.GetProperties (flags),
						events);

			initialized = true;
		}

		Type GetParentType ()
		{
			return InflateType (generic_type.BaseType);		
		}

		internal Type InflateType (Type type)
		{
			return InflateType (type, type_arguments, null);
		}

		internal Type InflateType (Type type, Type[] method_args)
		{
			return InflateType (type, type_arguments, method_args);
		}

		internal static Type InflateType (Type type, Type[] type_args, Type[] method_args)
		{
			if (type == null)
				return null;
			if (!type.IsGenericParameter && !type.ContainsGenericParameters)
				return type;
			if (type.IsGenericParameter) {
				if (type.DeclaringMethod == null)
					return type_args == null ? type : type_args [type.GenericParameterPosition];
				return method_args == null ? type : method_args [type.GenericParameterPosition];
			}
			if (type.IsPointer)
				return InflateType (type.GetElementType (), type_args, method_args).MakePointerType ();
			if (type.IsByRef)
				return InflateType (type.GetElementType (), type_args, method_args).MakeByRefType ();
			if (type.IsArray) {
				if (type.GetArrayRank () > 1)
					return InflateType (type.GetElementType (), type_args, method_args).MakeArrayType (type.GetArrayRank ());
#if BOOTSTRAP_NET_2_0
				if (type.ToString ().EndsWith ("[*]"))
#else
				if (type.ToString ().EndsWith ("[*]", StringComparison.Ordinal)) /*FIXME, the reflection API doesn't offer a way around this*/
#endif
					return InflateType (type.GetElementType (), type_args, method_args).MakeArrayType (1);
				return InflateType (type.GetElementType (), type_args, method_args).MakeArrayType ();
			}

			Type[] args = type.GetGenericArguments ();
			for (int i = 0; i < args.Length; ++i)
				args [i] = InflateType (args [i], type_args, method_args);

			Type gtd = type.IsGenericTypeDefinition ? type : type.GetGenericTypeDefinition ();
			return gtd.MakeGenericType (args);
		}
		
		public override Type BaseType {
			get {
				Type parent = GetParentType ();
				return parent != null ? parent : generic_type.BaseType;
			}
		}

		Type[] GetInterfacesInternal ()
		{
			Type[] ifaces = GetInterfacesFromGTD ();
			if (ifaces == null)
				return new Type [0];
			Type[] res = new Type [ifaces.Length];
			for (int i = 0; i < res.Length; ++i)
				res [i] = InflateType (ifaces [i]);
			return res;
		}

		public override Type[] GetInterfaces ()
		{
			if (!IsCompilerContext) {
				Console.WriteLine ("--FAIL {0}", this);
				Console.WriteLine ("\tgt {0}/{1}/{2}", generic_type, generic_type.IsCompilerContext, generic_type.GetType ());
				
				foreach (Type t in type_arguments)
					Console.WriteLine ("\targ {0}/{1}/{2}", t, t.IsCompilerContext, t.GetType ());
				
				throw new NotSupportedException ();
			}
			return GetInterfacesInternal ();
		}

		protected override bool IsValueTypeImpl ()
		{
			return generic_type.IsValueType;
		}

		internal override MethodInfo GetMethod (MethodInfo fromNoninstanciated)
		{
			initialize ();

			if (methods == null)
				methods = new Hashtable ();
			if (!methods.ContainsKey (fromNoninstanciated))
				methods [fromNoninstanciated] = new MethodOnTypeBuilderInst (this, fromNoninstanciated);
			return (MethodInfo)methods [fromNoninstanciated];
		}

		internal override ConstructorInfo GetConstructor (ConstructorInfo fromNoninstanciated)
		{
			initialize ();

			if (ctors == null)
				ctors = new Hashtable ();
			if (!ctors.ContainsKey (fromNoninstanciated))
				ctors [fromNoninstanciated] = new ConstructorOnTypeBuilderInst (this, fromNoninstanciated);
			return (ConstructorInfo)ctors [fromNoninstanciated];
		}

		internal override FieldInfo GetField (FieldInfo fromNoninstanciated)
		{
			initialize ();
			if (fields == null)
				fields = new Hashtable ();
			if (!fields.ContainsKey (fromNoninstanciated))
				fields [fromNoninstanciated] = new FieldOnTypeBuilderInst (this, fromNoninstanciated);
			return (FieldInfo)fields [fromNoninstanciated];
		}
		
		public override MethodInfo[] GetMethods (BindingFlags bf)
		{
			if (!IsCompilerContext)
				throw new NotSupportedException ();

			ArrayList l = new ArrayList ();

			//
			// Walk up our class hierarchy and retrieve methods from our
			// parent classes.
			//
			if (!(generic_type is TypeBuilder)) {
				foreach (var method in generic_type.GetMethods (bf)) {
					var m = method;
					if (m.DeclaringType.IsGenericTypeDefinition)
						m = TypeBuilder.GetMethod (this, m);
					l.Add (m);
				}
			} else {
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
			}

			MethodInfo[] result = new MethodInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		MethodInfo[] GetMethodsInternal (BindingFlags bf, MonoGenericClass reftype)
		{
			if (reftype != this)
				bf |= BindingFlags.DeclaredOnly; /*To avoid duplicates*/

			MethodInfo[] methods = GetMethodsFromGTDWithHint (bf);
			if (methods.Length == 0)
				return new MethodInfo [0];

			ArrayList l = new ArrayList ();
			bool match;
			MethodAttributes mattrs;

			initialize ();

			for (int i = 0; i < methods.Length; ++i) {
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
				if (c.DeclaringType.IsGenericTypeDefinition)
					c = TypeBuilder.GetMethod (this, c);
				l.Add (c);
			}

			MethodInfo[] result = new MethodInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		public override ConstructorInfo[] GetConstructors (BindingFlags bf)
		{
			if (!IsCompilerContext)
				throw new NotSupportedException ();

			ArrayList l = new ArrayList ();

			Type current_type = this;
			do {
				MonoGenericClass gi = current_type as MonoGenericClass;
				if (gi != null)
					l.AddRange (gi.GetConstructorsInternal (bf, this));
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

		ConstructorInfo[] GetConstructorsInternal (BindingFlags bf, MonoGenericClass reftype)
		{
			ConstructorInfo[] ctors = GetConstructorsFromGTDWithHint (bf);
			if (ctors == null || ctors.Length == 0)
				return new ConstructorInfo [0];

			ArrayList l = new ArrayList ();
			bool match;
			MethodAttributes mattrs;

			initialize ();

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
				l.Add (TypeBuilder.GetConstructor (this, c));
			}

			ConstructorInfo[] result = new ConstructorInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		public override FieldInfo[] GetFields (BindingFlags bf)
		{
			if (!IsCompilerContext)
				throw new NotSupportedException ();

			ArrayList l = new ArrayList ();

			Type current_type = this;
			do {
				MonoGenericClass gi = current_type as MonoGenericClass;
				if (gi != null)
					l.AddRange (gi.GetFieldsInternal (bf, this));
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

		FieldInfo[] GetFieldsInternal (BindingFlags bf, MonoGenericClass reftype)
		{
			FieldInfo[] fields = GetFieldsFromGTD (bf);
			if (fields.Length == 0)
				return new FieldInfo [0];

			ArrayList l = new ArrayList ();
			bool match;
			FieldAttributes fattrs;

			initialize ();

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
				l.Add (TypeBuilder.GetField (this, c));
			}

			FieldInfo[] result = new FieldInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		public override PropertyInfo[] GetProperties (BindingFlags bf)
		{
			if (!IsCompilerContext)
				throw new NotSupportedException ();

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
			PropertyInfo[] props = GetPropertiesInternal (generic_type, bf);
			if (props == null || props.Length == 0)
				return new PropertyInfo [0];

			ArrayList l = new ArrayList ();
			bool match;
			MethodAttributes mattrs;
			MethodInfo accessor;

			initialize ();

			foreach (PropertyInfo pinfo in props) {
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

		public override EventInfo[] GetEvents (BindingFlags bf)
		{
			if (!IsCompilerContext)
				throw new NotSupportedException ();

			ArrayList l = new ArrayList ();

			Type current_type = this;
			do {
				MonoGenericClass gi = current_type as MonoGenericClass;
				if (gi != null)
					l.AddRange (gi.GetEventsInternal (bf, this));
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
	
		EventInfo[] GetEventsInternal (BindingFlags bf, MonoGenericClass reftype) {
			TypeBuilder tb = generic_type as TypeBuilder;
			if (tb == null) {
				EventInfo[] res = generic_type.GetEvents (bf);
				for (int i = 0; i < res.Length; ++i)
					res [i] = new EventOnTypeBuilderInst (this, res [i]);
				return res;
			}
			EventBuilder[] events = tb.events;

			if (events == null || events.Length == 0)
				return new EventInfo [0];

			initialize ();

			ArrayList l = new ArrayList ();
			bool match;
			MethodAttributes mattrs;
			MethodInfo accessor;

			for (int i = 0; i < event_count; ++i) {
				EventBuilder ev = events [i];

				match = false;
				accessor = ev.add_method;
				if (accessor == null)
					accessor = ev.remove_method;
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
				l.Add (new EventOnTypeBuilderInst (this, ev));
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

			Type[] interfaces = GetInterfacesInternal ();

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

		public override Type UnderlyingSystemType {
			get { return this; }
		}

		public override Assembly Assembly {
			get { return generic_type.Assembly; }
		}

		public override Module Module {
			get { return generic_type.Module; }
		}

		public override string Name {
			get { return generic_type.Name; }
		}

		public override string Namespace {
			get { return generic_type.Namespace; }
		}

		public override string FullName {
			get { return format_name (true, false); }
		}

		public override string AssemblyQualifiedName {
			get { return format_name (true, true); }
		}

		public override Guid GUID {
			get { throw new NotSupportedException (); }
		}

		string format_name (bool full_name, bool assembly_qualified)
		{
			StringBuilder sb = new StringBuilder (generic_type.FullName);
			bool compiler_ctx = IsCompilerContext;

			sb.Append ("[");
			for (int i = 0; i < type_arguments.Length; ++i) {
				if (i > 0)
					sb.Append (",");
				
				string name;
				if (full_name) {
					string assemblyName = type_arguments [i].Assembly.FullName;
					name = type_arguments [i].FullName;
					if (name != null && assemblyName != null)
						name = name + ", " + assemblyName;
				} else {
					name = type_arguments [i].ToString ();
				}
				if (name == null) {
					if (compiler_ctx && type_arguments [i].IsGenericParameter)
						name = type_arguments [i].Name;
					else
						return null;
				}
				if (full_name)
					sb.Append ("[");
				sb.Append (name);
				if (full_name)
					sb.Append ("]");
			}
			sb.Append ("]");
			if (assembly_qualified) {
				sb.Append (", ");
				sb.Append (generic_type.Assembly.FullName);
			}
			return sb.ToString ();
		}

		public override string ToString ()
		{
			return format_name (false, false);
		}

		public override Type GetGenericTypeDefinition ()
		{
			return generic_type;
		}

		public override Type[] GetGenericArguments ()
		{
			Type[] ret = new Type [type_arguments.Length];
			type_arguments.CopyTo (ret, 0);
			return ret;
		}

		public override bool ContainsGenericParameters {
			get {
				/*FIXME remove this once compound types are not instantiated using MGC*/
				if (HasElementType)
					return GetElementType ().ContainsGenericParameters;

				foreach (Type t in type_arguments) {
					if (t.ContainsGenericParameters)
						return true;
				}
				return false;
			}
		}

		public override bool IsGenericTypeDefinition {
			get { return false; }
		}

		public override bool IsGenericType {
			get { return !HasElementType; }
		}

		public override Type DeclaringType {
			get { return InflateType (generic_type.DeclaringType); }
		}

		public override RuntimeTypeHandle TypeHandle {
			get {
				if (!IsCompilerContext)
					throw new NotSupportedException ();
				return _impl;
			}
		}

		public override Type MakeArrayType ()
		{
			return new ArrayType (this, 0);
		}

		public override Type MakeArrayType (int rank)
		{
			if (rank < 1)
				throw new IndexOutOfRangeException ();
			return new ArrayType (this, rank);
		}

		public override Type MakeByRefType ()
		{
			return new ByRefType (this);
		}

		public override Type MakePointerType ()
		{
			return new PointerType (this);
		}

		public override Type GetElementType ()
		{
			throw new NotSupportedException ();
		}

		protected override bool HasElementTypeImpl ()
		{
			return false;
		}

		protected override bool IsCOMObjectImpl ()
		{
			return false;
		}

		protected override bool IsPrimitiveImpl ()
		{
			return false;
		}

		protected override bool IsArrayImpl ()
		{
			return false;
		}

		protected override bool IsByRefImpl ()
		{
			return false;
		}

		protected override bool IsPointerImpl ()
		{
			return false;
		}

		protected override TypeAttributes GetAttributeFlagsImpl ()
		{
			return generic_type.Attributes; 
		}

		//stuff that throws
		public override Type GetInterface (string name, bool ignoreCase)
		{
			throw new NotSupportedException ();
		}

		public override EventInfo GetEvent (string name, BindingFlags bindingAttr)
		{
			if (!IsCompilerContext)
				throw new NotSupportedException ();
			foreach (var evt in GetEvents (bindingAttr)) {
				if (evt.Name == name)
					return evt;
			}
			return null;
		}

		public override FieldInfo GetField( string name, BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override MemberInfo[] GetMembers (BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override Type GetNestedType (string name, BindingFlags bindingAttr)
		{
			throw new NotSupportedException ();
		}

		public override object InvokeMember (string name, BindingFlags invokeAttr,
						     Binder binder, object target, object[] args,
						     ParameterModifier[] modifiers,
						     CultureInfo culture, string[] namedParameters)
		{
			throw new NotSupportedException ();
		}

		protected override MethodInfo GetMethodImpl (string name, BindingFlags bindingAttr, Binder binder,
		                                             CallingConventions callConvention, Type[] types,
		                                             ParameterModifier[] modifiers)
		{
			throw new NotSupportedException ();
		}

		protected override PropertyInfo GetPropertyImpl (string name, BindingFlags bindingAttr, Binder binder,
		                                                 Type returnType, Type[] types, ParameterModifier[] modifiers)
		{
			throw new NotSupportedException ();
		}

		protected override ConstructorInfo GetConstructorImpl (BindingFlags bindingAttr,
								       Binder binder,
								       CallingConventions callConvention,
								       Type[] types,
								       ParameterModifier[] modifiers)
		{
			if (!IsCompilerContext)
				throw new NotSupportedException ();
			return MonoType.GetConstructorImpl (GetConstructors (bindingAttr), bindingAttr, binder, callConvention, types, modifiers);
		}

		//MemberInfo
		public override bool IsDefined (Type attributeType, bool inherit)
		{
			if (!IsCompilerContext)
				throw new NotSupportedException ();
			return generic_type.IsDefined (attributeType, inherit);
		}

		public override object [] GetCustomAttributes (bool inherit)
		{
			if (!IsCompilerContext)
				throw new NotSupportedException ();
			return generic_type.GetCustomAttributes (inherit);
		}

		public override object [] GetCustomAttributes (Type attributeType, bool inherit)
		{
			if (!IsCompilerContext)
				throw new NotSupportedException ();
			return generic_type.GetCustomAttributes (attributeType, inherit);
		}
	}
}

