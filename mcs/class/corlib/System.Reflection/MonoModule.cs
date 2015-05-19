//
// System.Reflection/MonoModule.cs
//
// Author:
//   Rodrigo Kumpera (rkumpera@novell.com)
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security;
using System.Security.Permissions;
using System.Runtime.Serialization;

namespace System.Reflection {

	abstract class RuntimeModule : Module
	{
		
	}

	[ComVisible (true)]
	[ComDefaultInterfaceAttribute (typeof (_Module))]
	[Serializable]
	[ClassInterface(ClassInterfaceType.None)]
	class MonoModule : RuntimeModule
	{
		public
		override
		Assembly Assembly {
			get { return assembly; }
		}

		public
		override
		// Note: we do not ask for PathDiscovery because no path is returned here.
		// However MS Fx requires it (see FDBK23572 for details).
		string Name {
			get { return name; }
		}
	
		public
		override
		string ScopeName {
			get { return scopename; }
		}

		public
		override
		int MDStreamVersion {
			get {
				if (_impl == IntPtr.Zero)
					throw new NotSupportedException ();
				return GetMDStreamVersion (_impl);
			}
		}

		public
		override
		Guid ModuleVersionId {
			get {
				return GetModuleVersionId ();
			}
		}

		public override
		string FullyQualifiedName {
			get {
#if !NET_2_1
				if (SecurityManager.SecurityEnabled) {
					new FileIOPermission (FileIOPermissionAccess.PathDiscovery, fqname).Demand ();
				}
#endif
				return fqname;
			}
		}

		public
		override
		bool IsResource()
		{
			return is_resource;
		}

		public override
		Type[] FindTypes(TypeFilter filter, object filterCriteria) 
		{
			var filtered = new List<Type> ();
			Type[] types = GetTypes ();
			foreach (Type t in types)
				if (filter (t, filterCriteria))
					filtered.Add (t);
			return filtered.ToArray ();
		}

		public override
		object[] GetCustomAttributes(bool inherit) 
		{
			return MonoCustomAttrs.GetCustomAttributes (this, inherit);
		}

		public override
		object[] GetCustomAttributes(Type attributeType, bool inherit) 
		{
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
		}

		public override
		FieldInfo GetField (string name, BindingFlags bindingAttr) 
		{
			if (name == null)
				throw new ArgumentNullException("name");

			if (IsResource ())
				return null;

			Type globalType = GetGlobalType ();
			return (globalType != null) ? globalType.GetField (name, bindingAttr) : null;
		}

		public override
		FieldInfo[] GetFields (BindingFlags bindingFlags)
		{
			if (IsResource ())
				return new FieldInfo [0];

			Type globalType = GetGlobalType ();
			return (globalType != null) ? globalType.GetFields (bindingFlags) : new FieldInfo [0];
		}

		public override
		int MetadataToken {
			get { return get_MetadataToken (this); }
		}
		protected
		override
		MethodInfo GetMethodImpl (string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) 
		{
			if (IsResource ())
				return null;

			Type globalType = GetGlobalType ();
			if (globalType == null)
				return null;
			if (types == null)
				return globalType.GetMethod (name);
			return globalType.GetMethod (name, bindingAttr, binder, callConvention, types, modifiers);
		}

		public
		override
		MethodInfo[] GetMethods (BindingFlags bindingFlags) {
			if (IsResource ())
				return new MethodInfo [0];

			Type globalType = GetGlobalType ();
			return (globalType != null) ? globalType.GetMethods (bindingFlags) : new MethodInfo [0];
		}

		public override
		void GetPEKind (out PortableExecutableKinds peKind, out ImageFileMachine machine) {
			ModuleHandle.GetPEKind (out peKind, out machine);
		}

		public override
		Type GetType(string className, bool throwOnError, bool ignoreCase) 
		{
			if (className == null)
				throw new ArgumentNullException ("className");
			if (className == String.Empty)
				throw new ArgumentException ("Type name can't be empty");
			return assembly.InternalGetType (this, className, throwOnError, ignoreCase);
		}
	
		public override
		bool IsDefined (Type attributeType, bool inherit) 
		{
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}

		public
		override
		FieldInfo ResolveField (int metadataToken, Type [] genericTypeArguments, Type [] genericMethodArguments) {
			ResolveTokenError error;

			IntPtr handle = ResolveFieldToken (_impl, metadataToken, ptrs_from_types (genericTypeArguments), ptrs_from_types (genericMethodArguments), out error);
			if (handle == IntPtr.Zero)
				throw resolve_token_exception (metadataToken, error, "Field");
			else
				return FieldInfo.GetFieldFromHandle (new RuntimeFieldHandle (handle));
		}

		public
		override
		MemberInfo ResolveMember (int metadataToken, Type [] genericTypeArguments, Type [] genericMethodArguments) {

			ResolveTokenError error;

			MemberInfo m = ResolveMemberToken (_impl, metadataToken, ptrs_from_types (genericTypeArguments), ptrs_from_types (genericMethodArguments), out error);
			if (m == null)
				throw resolve_token_exception (metadataToken, error, "MemberInfo");
			else
				return m;
		}

		public
		override
		MethodBase ResolveMethod (int metadataToken, Type [] genericTypeArguments, Type [] genericMethodArguments) {
			ResolveTokenError error;

			IntPtr handle = ResolveMethodToken (_impl, metadataToken, ptrs_from_types (genericTypeArguments), ptrs_from_types (genericMethodArguments), out error);
			if (handle == IntPtr.Zero)
				throw resolve_token_exception (metadataToken, error, "MethodBase");
			else
				return MethodBase.GetMethodFromHandleNoGenericCheck (new RuntimeMethodHandle (handle));
		}

		public
		override
		string ResolveString (int metadataToken) {
			ResolveTokenError error;

			string s = ResolveStringToken (_impl, metadataToken, out error);
			if (s == null)
				throw resolve_token_exception (metadataToken, error, "string");
			else
				return s;
		}

		public
		override
		Type ResolveType (int metadataToken, Type [] genericTypeArguments, Type [] genericMethodArguments) {
			ResolveTokenError error;

			IntPtr handle = ResolveTypeToken (_impl, metadataToken, ptrs_from_types (genericTypeArguments), ptrs_from_types (genericMethodArguments), out error);
			if (handle == IntPtr.Zero)
				throw resolve_token_exception (metadataToken, error, "Type");
			else
				return Type.GetTypeFromHandle (new RuntimeTypeHandle (handle));
		}

		public
		override
		byte[] ResolveSignature (int metadataToken) {
			ResolveTokenError error;

		    byte[] res = ResolveSignature (_impl, metadataToken, out error);
			if (res == null)
				throw resolve_token_exception (metadataToken, error, "signature");
			else
				return res;
		}

		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			UnitySerializationHolder.GetUnitySerializationInfo (info, UnitySerializationHolder.ModuleUnity, this.ScopeName, this.GetRuntimeAssembly ());
		}

#if !NET_2_1

		public
		override
		X509Certificate GetSignerCertificate ()
		{
			try {
				return X509Certificate.CreateFromSignedFile (assembly.Location);
			}
			catch {
				return null;
			}
		}
#endif

		public override
		Type[] GetTypes() 
		{
			return InternalGetTypes ();
		}

		public override IList<CustomAttributeData> GetCustomAttributesData () {
			return CustomAttributeData.GetCustomAttributes (this);
		}

		internal RuntimeAssembly GetRuntimeAssembly ()
		{
			return (RuntimeAssembly)assembly;
		}
	}
}
