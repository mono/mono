//
// System.Reflection/Module.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace System.Reflection {

	[Serializable]
	public class Module : ISerializable, ICustomAttributeProvider {
	
		public static readonly TypeFilter FilterTypeName;
		public static readonly TypeFilter FilterTypeNameIgnoreCase;
	
		private IntPtr _impl; /* a pointer to a MonoImage */
		internal Assembly assembly;
		internal string fqname;
		internal string name;
		internal string scopename;
		internal bool is_resource;
	
		const BindingFlags defaultBindingFlags = 
			BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
		
		internal Module () { }

		~Module () {
			Close ();
		}
	
		public Assembly Assembly {
			get { return assembly; }
		}
	
		public virtual string FullyQualifiedName {
			get { return fqname; }
		}
	
		public string Name {
			get { return name; }
		}
	
		public string ScopeName {
			get { return scopename; }
		}
	
		public virtual Type[] FindTypes(TypeFilter filter, object filterCriteria) 
		{
			System.Collections.ArrayList filtered = new System.Collections.ArrayList ();
			Type[] types = GetTypes ();
			foreach (Type t in types)
				if (filter (t, filterCriteria))
					filtered.Add (t);
			return (Type[])filtered.ToArray (typeof(Type));
		}
	
		public virtual object[] GetCustomAttributes(bool inherit) 
		{
			return MonoCustomAttrs.GetCustomAttributes (this, inherit);
		}
	
		public virtual object[] GetCustomAttributes(Type attributeType, bool inherit) 
		{
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
		}
	
		public FieldInfo GetField (string name) 
		{
			if (IsResource ())
				return null;

			return GetGlobalType ().GetField (name, BindingFlags.Public | BindingFlags.Static);
		}
	
		public FieldInfo GetField (string name, BindingFlags flags) 
		{
			if (IsResource ())
				return null;

			return GetGlobalType ().GetField (name, flags);
		}
	
		public FieldInfo[] GetFields () 
		{
			if (IsResource ())
				return new FieldInfo [0];

			return GetGlobalType ().GetFields (BindingFlags.Public | BindingFlags.Static);
		}
	
		public MethodInfo GetMethod (string name) 
		{
			return GetMethodImpl (name, defaultBindingFlags, null, CallingConventions.Any, Type.EmptyTypes, null);
		}
	
		public MethodInfo GetMethod (string name, Type[] types) 
		{
			return GetMethodImpl (name, defaultBindingFlags, null, CallingConventions.Any, types, null);
		}
	
		public MethodInfo GetMethod (string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) 
		{
			return GetMethodImpl (name, bindingAttr, binder, callConvention, types, modifiers);
		}
	
		protected virtual MethodInfo GetMethodImpl (string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) 
		{
			if (IsResource ())
				return null;

			return GetGlobalType ().GetMethod (name, bindingAttr, binder, callConvention, types, modifiers);
		}
	
		public MethodInfo[] GetMethods () 
		{
			if (IsResource ())
				return new MethodInfo [0];

			return GetGlobalType ().GetMethods ();
		}
	
		public virtual void GetObjectData (SerializationInfo info, StreamingContext context) 
		{
			UnitySerializationHolder.GetModuleData (this, info, context);
		}
	
		public X509Certificate GetSignerCertificate ()
		{
			try {
				return X509Certificate.CreateFromSignedFile (assembly.Location);
			}
			catch {
				return null;
			}
		}
	
		public virtual Type GetType(string className) 
		{
			return GetType (className, false, false);
		}
	
		public virtual Type GetType(string className, bool ignoreCase) 
		{
			return GetType (className, false, ignoreCase);
		}
	
		public virtual Type GetType(string className, bool throwOnError, bool ignoreCase) 
		{
			if (className == null)
				throw new ArgumentNullException ("className");
			if (className == String.Empty)
				throw new ArgumentException ("Type name can't be empty");
			return assembly.InternalGetType (this, className, throwOnError, ignoreCase);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern Type[] InternalGetTypes ();
	
		public virtual Type[] GetTypes() 
		{
			return InternalGetTypes ();
		}
	
		public virtual bool IsDefined (Type attributeType, bool inherit) 
		{
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}
	
		public bool IsResource()
		{
			return is_resource;
		}
	
		public override string ToString () 
		{
			return "Reflection.Module: " + name;
		}

		// Mono Extension: returns the GUID of this module
		public Guid Mono_GetGuid ()
		{
			return new Guid (GetGuidInternal ());
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern string GetGuidInternal ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern Type GetGlobalType ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern void Close ();
	}
}
