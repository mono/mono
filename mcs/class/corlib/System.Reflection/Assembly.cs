//
// System.Reflection/Assembly.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
// Copyright 2011 Xamarin Inc (http://www.xamarin.com).
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

using System.Security;
using System.Security.Policy;
using System.Security.Permissions;
using System.Runtime.Serialization;
using System.Reflection;
using System.IO;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.Threading;
using System.Text;
using System.Diagnostics.Contracts;

using Mono.Security;

namespace System.Reflection {

	[ComVisible (true)]
	[ComDefaultInterfaceAttribute (typeof (_Assembly))]
	[Serializable]
	[ClassInterface(ClassInterfaceType.None)]
	[StructLayout (LayoutKind.Sequential)]
#if MOBILE
	public partial class Assembly : ICustomAttributeProvider, ISerializable
#else
	public abstract class Assembly : ICustomAttributeProvider, _Assembly, IEvidenceFactory, ISerializable
#endif
	{
		internal class ResolveEventHolder {	
#pragma warning disable 67
			public event ModuleResolveEventHandler ModuleResolve;
#pragma warning restore
		}

		//
		// We can't store the event directly in this class, since the
		// compiler would silently insert the fields before _mono_assembly
		//
		public virtual event ModuleResolveEventHandler ModuleResolve {
			[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
			add {
				throw new NotImplementedException ();
			}
			[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
			remove {
				throw new NotImplementedException ();
			}
		}

		public virtual string CodeBase {
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual string EscapedCodeBase {
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual string FullName {
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual MethodInfo EntryPoint {
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual Evidence Evidence {
			[SecurityPermission (SecurityAction.Demand, ControlEvidence = true)]
			get {
				throw new NotImplementedException ();
			}
		}

		internal virtual Evidence UnprotectedGetEvidence ()
		{
			throw new NotImplementedException ();
		}

		internal virtual IntPtr MonoAssembly {
			get {
				throw new NotImplementedException ();
			}
		}

		internal virtual bool FromByteArray {
			set {
				throw new NotImplementedException ();
			}
		}

		public virtual String Location {
			get {
				throw new NotImplementedException ();
			}
		}

		[ComVisible (false)]
		public virtual string ImageRuntimeVersion {
			get {
				throw new NotImplementedException ();
			}
		}

		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		public virtual bool IsDefined (Type attributeType, bool inherit)
		{
			throw new NotImplementedException ();
		}

		public virtual object [] GetCustomAttributes (bool inherit)
		{
			throw new NotImplementedException ();
		}

		public virtual object [] GetCustomAttributes (Type attributeType, bool inherit)
		{
			throw new NotImplementedException ();
		}

		public virtual FileStream[] GetFiles ()
		{
			return GetFiles (false);
		}

		public virtual FileStream [] GetFiles (bool getResourceModules)
		{
			throw new NotImplementedException ();
		}

		public virtual FileStream GetFile (String name)
		{
			throw new NotImplementedException ();
		}

		public virtual Stream GetManifestResourceStream (String name)
		{
			throw new NotImplementedException ();
		}

		public virtual Stream GetManifestResourceStream (Type type, String name)
		{
			throw new NotImplementedException ();
		}

		internal Stream GetManifestResourceStream (Type type, String name, bool skipSecurityCheck, ref StackCrawlMark stackMark)
		{
			StringBuilder sb = new StringBuilder ();
			if (type == null) {
				if (name == null)
						throw new ArgumentNullException ("type");
			} else {
				String nameSpace = type.Namespace;
				if (nameSpace != null) {
					sb.Append (nameSpace);
					if (name != null)
						sb.Append (Type.Delimiter);
				}
			}

			if (name != null)
				sb.Append(name);

			return GetManifestResourceStream (sb.ToString());
		}

		internal unsafe Stream GetManifestResourceStream(String name, ref StackCrawlMark stackMark, bool skipSecurityCheck)
		{
			return GetManifestResourceStream (null, name, skipSecurityCheck, ref stackMark);
		}

		internal String GetSimpleName()
		{
			AssemblyName aname = GetName (true);
			return aname.Name;
		}

		internal byte[] GetPublicKey()
		{
			AssemblyName aname = GetName (true);
			return aname.GetPublicKey ();
		}

		internal Version GetVersion()
		{
			AssemblyName aname = GetName (true);
			return aname.Version;
		}

		private AssemblyNameFlags GetFlags()
		{
			AssemblyName aname = GetName (true);
			return aname.Flags;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal virtual extern Type[] GetTypes (bool exportedOnly);
		
		public virtual Type[] GetTypes ()
		{
			return GetTypes (false);
		}

		public virtual Type[] GetExportedTypes ()
		{
			throw new NotImplementedException ();
		}

		public virtual Type GetType (String name, Boolean throwOnError)
		{
			return GetType (name, throwOnError, false);
		}

		public virtual Type GetType (String name) {
			return GetType (name, false, false);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern Type InternalGetType (Module module, String name, Boolean throwOnError, Boolean ignoreCase);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern unsafe static void InternalGetAssemblyName (string assemblyFile, out Mono.MonoAssemblyName aname, out string codebase);

		public virtual AssemblyName GetName (Boolean copiedName)
		{
			throw new NotImplementedException ();
		}

		public virtual AssemblyName GetName ()
		{
			return GetName (false);
		}

		public override string ToString ()
		{
			return base.ToString ();
		}

		public static String CreateQualifiedName (String assemblyName, String typeName) 
		{
			return typeName + ", " + assemblyName;
		}

		public static Assembly GetAssembly (Type type)
		{
			if (type != null)
				return type.Assembly;
			throw new ArgumentNullException ("type");
		}


		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public static extern Assembly GetEntryAssembly();

		internal Assembly GetSatelliteAssembly (CultureInfo culture, Version version, bool throwOnError, ref StackCrawlMark stackMark)
		{
			if (culture == null)
				throw new ArgumentNullException("culture");
			Contract.EndContractBlock();

			String name = GetSimpleName() + ".resources";
			return InternalGetSatelliteAssembly(name, culture, version, true, ref stackMark);
		}

		internal RuntimeAssembly InternalGetSatelliteAssembly (String name, CultureInfo culture, Version version, bool throwOnFileNotFound, ref StackCrawlMark stackMark)
		{
			AssemblyName an = new AssemblyName ();

			an.SetPublicKey (GetPublicKey ());
			an.Flags = GetFlags () | AssemblyNameFlags.PublicKey;

			if (version == null)
				an.Version = GetVersion ();
			else
				an.Version = version;

			an.CultureInfo = culture;
			an.Name = name;

			Assembly assembly;

			try {
				assembly = AppDomain.CurrentDomain.LoadSatellite (an, false, ref stackMark);
				if (assembly != null)
					return (RuntimeAssembly)assembly;
			} catch (FileNotFoundException) {
				assembly = null;
				// ignore
			}

			if (String.IsNullOrEmpty (Location))
				return null;

			// Try the assembly directory
			string location = Path.GetDirectoryName (Location);
			string fullName = Path.Combine (location, Path.Combine (culture.Name, an.Name + ".dll"));

			try {
				return (RuntimeAssembly)LoadFrom (fullName, false, ref stackMark);
			} catch {
				if (!throwOnFileNotFound && !File.Exists (fullName))
					return null;
				throw;
			}
		}

#if !MOBILE
		Type _Assembly.GetType ()
		{
			// Required or object::GetType becomes virtual final
			return base.GetType ();
		}		
#endif

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static Assembly LoadFrom (String assemblyFile, bool refOnly, ref StackCrawlMark stackMark);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static Assembly LoadFile_internal (String assemblyFile, ref StackCrawlMark stackMark);

		[MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
		public static Assembly LoadFrom (String assemblyFile)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return LoadFrom (assemblyFile, false, ref stackMark);
		}

		[Obsolete]
		[MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
		public static Assembly LoadFrom (String assemblyFile, Evidence securityEvidence)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			Assembly a = LoadFrom (assemblyFile, false, ref stackMark);
#if !MOBILE
			if ((a != null) && (securityEvidence != null)) {
				// merge evidence (i.e. replace defaults with provided evidences)
				a.Evidence.Merge (securityEvidence);
			}
#endif
			return a;
		}

		[Obsolete]
		[MonoTODO("This overload is not currently implemented")]
		// FIXME: What are we missing?
		public static Assembly LoadFrom (String assemblyFile, Evidence securityEvidence, byte[] hashValue, AssemblyHashAlgorithm hashAlgorithm)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Assembly LoadFrom (String assemblyFile, byte [] hashValue, AssemblyHashAlgorithm hashAlgorithm)
		{
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
		public static Assembly UnsafeLoadFrom (String assemblyFile)
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return LoadFrom (assemblyFile, false, ref stackMark);
		}

		[Obsolete]
		[MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
		public static Assembly LoadFile (String path, Evidence securityEvidence)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			if (path == String.Empty)
				throw new ArgumentException ("Path can't be empty", "path");
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			Assembly a = LoadFile_internal (path, ref stackMark);
			if (a != null && securityEvidence != null) {
				throw new NotImplementedException ();
			}
			return a;
		}

		public static Assembly LoadFile (String path)
		{
			return LoadFile (path, null);
		}

		public static Assembly Load (String assemblyString)
		{
			return AppDomain.CurrentDomain.Load (assemblyString);
		}

		[Obsolete]
		public static Assembly Load (String assemblyString, Evidence assemblySecurity)
		{
			return AppDomain.CurrentDomain.Load (assemblyString, assemblySecurity);
		}

		public static Assembly Load (AssemblyName assemblyRef)
		{
			return AppDomain.CurrentDomain.Load (assemblyRef);
		}

		[Obsolete]
		public static Assembly Load (AssemblyName assemblyRef, Evidence assemblySecurity)
		{
			return AppDomain.CurrentDomain.Load (assemblyRef, assemblySecurity);
		}

		public static Assembly Load (Byte[] rawAssembly)
		{
			return AppDomain.CurrentDomain.Load (rawAssembly);
		}

		public static Assembly Load (Byte[] rawAssembly, Byte[] rawSymbolStore)
		{
			return AppDomain.CurrentDomain.Load (rawAssembly, rawSymbolStore);
		}

		[Obsolete]
		public static Assembly Load (Byte[] rawAssembly, Byte[] rawSymbolStore,
					     Evidence securityEvidence)
		{
			return AppDomain.CurrentDomain.Load (rawAssembly, rawSymbolStore, securityEvidence);
		}

		[MonoLimitation ("Argument securityContextSource is ignored")]
		public static Assembly Load (byte [] rawAssembly, byte [] rawSymbolStore, SecurityContextSource securityContextSource)
		{
			return AppDomain.CurrentDomain.Load (rawAssembly, rawSymbolStore);
		}

		public static Assembly ReflectionOnlyLoad (byte[] rawAssembly)
		{
			return AppDomain.CurrentDomain.Load (rawAssembly, null, null, true);
		}

		[MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
		public static Assembly ReflectionOnlyLoad (string assemblyString) 
		{
			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return AppDomain.CurrentDomain.Load (assemblyString, null, true, ref stackMark);
		}

		[MethodImplAttribute(MethodImplOptions.NoInlining)] // Methods containing StackCrawlMark local var has to be marked non-inlineable
		public static Assembly ReflectionOnlyLoadFrom (string assemblyFile) 
		{
			if (assemblyFile == null)
				throw new ArgumentNullException ("assemblyFile");

			StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
			return LoadFrom (assemblyFile, true, ref stackMark);
		}

        [Obsolete("This method has been deprecated. Please use Assembly.Load() instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		public static Assembly LoadWithPartialName (string partialName)
		{
			return LoadWithPartialName (partialName, null);
		}

		[MonoTODO ("Not implemented")]
		public Module LoadModule (string moduleName, byte [] rawModule)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Not implemented")]
		public
		virtual
		Module LoadModule (string moduleName, byte [] rawModule, byte [] rawSymbolStore)
		{
			throw new NotImplementedException ();
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern Assembly load_with_partial_name (string name, Evidence e);

        [Obsolete("This method has been deprecated. Please use Assembly.Load() instead. http://go.microsoft.com/fwlink/?linkid=14202")]
		public static Assembly LoadWithPartialName (string partialName, Evidence securityEvidence)
		{
			return LoadWithPartialName (partialName, securityEvidence, true);
		}

		/**
		 * LAMESPEC: It is possible for this method to throw exceptions IF the name supplied
		 * is a valid gac name and contains filesystem entry charachters at the end of the name
		 * ie System/// will throw an exception. However ////System will not as that is canocolized
		 * out of the name.
		 */

		// FIXME: LoadWithPartialName must look cache (no CAS) or read from disk (CAS)
		internal static Assembly LoadWithPartialName (string partialName, Evidence securityEvidence, bool oldBehavior)
		{
			if (!oldBehavior)
				throw new NotImplementedException ();

			if (partialName == null)
				throw new NullReferenceException ();

			return load_with_partial_name (partialName, securityEvidence);
		}

		public Object CreateInstance (String typeName) 
		{
			return CreateInstance (typeName, false);
		}

		public Object CreateInstance (String typeName, Boolean ignoreCase)
		{
			Type t = GetType (typeName, false, ignoreCase);
			if (t == null)
				return null;

			try {
				return Activator.CreateInstance (t);
			} catch (InvalidOperationException) {
				throw new ArgumentException ("It is illegal to invoke a method on a Type loaded via ReflectionOnly methods.");
			}
		}

		public
		virtual
		Object CreateInstance (String typeName, Boolean ignoreCase,
					      BindingFlags bindingAttr, Binder binder,
					      Object[] args, CultureInfo culture,
					      Object[] activationAttributes)
		{
			Type t = GetType (typeName, false, ignoreCase);
			if (t == null)
				return null;

			try {
				return Activator.CreateInstance (t, bindingAttr, binder, args, culture, activationAttributes);
			} catch (InvalidOperationException) {
				throw new ArgumentException ("It is illegal to invoke a method on a Type loaded via ReflectionOnly methods.");
			}
		}

		public Module[] GetLoadedModules ()
		{
			return GetLoadedModules (false);
		}

		public Module[] GetModules ()
		{
			return GetModules (false);
		}

		internal virtual Module[] GetModulesInternal () {
			throw new NotImplementedException ();
		}
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static Assembly GetExecutingAssembly ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static Assembly GetCallingAssembly ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern IntPtr InternalGetReferencedAssemblies (Assembly module);

		public virtual String[] GetManifestResourceNames ()
		{
			throw new NotImplementedException ();
		}

		internal static AssemblyName[] GetReferencedAssemblies (Assembly module)
		{
			using (var nativeNames = new Mono.SafeGPtrArrayHandle (InternalGetReferencedAssemblies (module))) {
				var numAssemblies = nativeNames.Length;
				try {
					AssemblyName [] result = new AssemblyName[numAssemblies];
					const bool addVersion = true;
					const bool addPublicKey = false;
					const bool defaultToken = true;
					const bool assemblyRef = true;
					for (int i = 0; i < numAssemblies; i++) {
						AssemblyName name = new AssemblyName ();
						unsafe {
							Mono.MonoAssemblyName *nativeName = (Mono.MonoAssemblyName*) nativeNames[i];
							name.FillName (nativeName, null, addVersion, addPublicKey, defaultToken, assemblyRef);
							result[i] = name;
						}
					}
					return result;
				} finally {
					for (int i = 0; i < numAssemblies; i++) {
						unsafe {
							Mono.MonoAssemblyName* nativeName = (Mono.MonoAssemblyName*) nativeNames[i];
							Mono.RuntimeMarshal.FreeAssemblyName (ref *nativeName, true);
						}
					}
				}
			}
		}

		public virtual ManifestResourceInfo GetManifestResourceInfo (String resourceName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Currently it always returns zero")]
		[ComVisible (false)]
		public
		virtual
		long HostContext {
			get { return 0; }
		}

		internal virtual Module GetManifestModule () {
			throw new NotImplementedException ();
		}

		[ComVisible (false)]
		public virtual bool ReflectionOnly {
			get {
				throw new NotImplementedException ();
			}
		}
		
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public override bool Equals (object o)
		{
			return base.Equals (o);
		}

#if !MOBILE
		internal virtual PermissionSet GrantedPermissionSet {
			get {
				throw new NotImplementedException ();
			}
		}

		internal virtual PermissionSet DeniedPermissionSet {
			get {
				throw new NotImplementedException ();
			}
		}
		
		public virtual PermissionSet PermissionSet {
			get {
				throw new NotImplementedException ();
			}
		}
#endif

		public virtual SecurityRuleSet SecurityRuleSet {
			get { throw CreateNIE (); }
		}

		static Exception CreateNIE ()
		{
			return new NotImplementedException ("Derived classes must implement it");
		}
		
		public virtual IList<CustomAttributeData> GetCustomAttributesData ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsFullyTrusted {
			get { return true; }
		}

		public virtual Type GetType (string name, bool throwOnError, bool ignoreCase)
		{
			throw CreateNIE ();
		}

		public virtual Module GetModule (String name)
		{
			throw CreateNIE ();
		}

		public virtual AssemblyName[] GetReferencedAssemblies ()
		{
			throw CreateNIE ();
		}

		public virtual Module[] GetModules (bool getResourceModules)
		{
			throw CreateNIE ();
		}

		[MonoTODO ("Always returns the same as GetModules")]
		public virtual Module[] GetLoadedModules (bool getResourceModules)
		{
			throw CreateNIE ();
		}

		public virtual Assembly GetSatelliteAssembly (CultureInfo culture)
		{
			throw CreateNIE ();
		}

		public virtual Assembly GetSatelliteAssembly (CultureInfo culture, Version version)
		{
			throw CreateNIE ();
		}

		public virtual Module ManifestModule {
			get { throw CreateNIE (); }
		}

		public virtual bool GlobalAssemblyCache {
			get { throw CreateNIE (); }
		}

		public virtual bool IsDynamic {
			get { return false; }
		}

		public static bool operator == (Assembly left, Assembly right)
		{
			if ((object)left == (object)right)
				return true;
			if ((object)left == null ^ (object)right == null)
				return false;
			return left.Equals (right);
		}

		public static bool operator != (Assembly left, Assembly right)
		{
			if ((object)left == (object)right)
				return false;
			if ((object)left == null ^ (object)right == null)
				return true;
			return !left.Equals (right);
		}

		public virtual IEnumerable<TypeInfo> DefinedTypes {
			get {
				foreach (var type in GetTypes ()) {
					yield return type.GetTypeInfo ();
				}
			}
		}

		public virtual IEnumerable<Type> ExportedTypes {
			get { return GetExportedTypes (); }
		}

		public virtual IEnumerable<Module> Modules {
			get { return GetModules (); }
		}

		public virtual IEnumerable<CustomAttributeData> CustomAttributes {
			get { return GetCustomAttributesData (); }
		}

		public virtual Type[] GetForwardedTypes() => throw new PlatformNotSupportedException();
	}
}
