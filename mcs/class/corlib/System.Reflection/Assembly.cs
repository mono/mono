//
// System.Reflection/Assembly.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Security.Policy;
using System.Runtime.Serialization;
using System.Reflection.Emit;
using System.IO;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections;

namespace System.Reflection {

	[Serializable]
	public class Assembly : System.Reflection.ICustomAttributeProvider,
		System.Security.IEvidenceFactory, System.Runtime.Serialization.ISerializable {
		private IntPtr _mono_assembly;

		internal Assembly () {}

		//TODO: when adding this, MonoReflectionAssembly must be modified too.
		// Probably, adding a delegate field after _mono_assbmely and using it in add/remove 
		// is the way to go (to avoid the compiler inserting the delegate field before).
		//public event ModuleResolveEventHandler ModuleResolve;

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern string get_code_base ();
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern string get_location ();
		
		public virtual string CodeBase {
			get {
				return get_code_base ();
			}
		}

		internal virtual string CopiedCodeBase {
			get {
				return get_code_base ();
			}
		} 

		[MonoTODO]
		public virtual string EscapedCodeBase {
			get {
				//FIXME: escape characters -> Uri
				return get_code_base ();
			}
		}

		public virtual string FullName {
			get {
				//
				// FIXME: This is wrong, but it gets us going
				// in the compiler for now
				//
				return GetName (false).ToString ();
			}
		}

		public virtual extern MethodInfo EntryPoint {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		public virtual Evidence Evidence {
			get {
				return null;
			}
		}

		public bool GlobalAssemblyCache {
			get {
				//TODO: if we ever have a GAC, fix this.
				return false;
			}
		}
		
		public virtual String Location {
			get {
				return get_location ();
			}
		}

		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			UnitySerializationHolder.GetAssemblyData (this, info, context);
		}

		public virtual bool IsDefined (Type attributeType, bool inherit)
		{
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}

		public virtual object [] GetCustomAttributes (bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, inherit);
		}

		public virtual object [] GetCustomAttributes (Type attributeType, bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern object GetFilesInternal (String name);

		public virtual FileStream[] GetFiles ()
		{
			string[] names = (string[]) GetFilesInternal (null);
			if (names == null)
				return new FileStream [0];

			FileStream[] res = new FileStream [names.Length];
			for (int i = 0; i < names.Length; ++i)
				res [i] = new FileStream (names [i], FileMode.Open, FileAccess.Read);
			return res;
		}

		[MonoTODO]
		public virtual FileStream [] GetFiles (bool getResourceModules)
		{
			throw new NotImplementedException ();
		}

		public virtual FileStream GetFile (String name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			string filename = (string)GetFilesInternal (name);
			if (filename != null)
				return new FileStream (filename, FileMode.Open, FileAccess.Read);
			else
				return null;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern object GetManifestResourceInternal (String name);

		public virtual Stream GetManifestResourceStream (String name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (name == "")
				throw new ArgumentException ("name cannot have zero length.");

			ManifestResourceInfo info = GetManifestResourceInfo (name);
			if (info == null)
				return null;

			if (info.ReferencedAssembly != null)
				return info.ReferencedAssembly.GetManifestResourceStream (name);

			object data = GetManifestResourceInternal (name);
			string filename = data as string;
			if (data == null)
				return null;
			if (filename != null) {
				if ((info.ResourceLocation & ResourceLocation.Embedded) != 0)
					throw new NotImplementedException ("Reading from modules is not implemented");
				else
					return new FileStream (filename, FileMode.Open, FileAccess.Read);
			} else {
				return new MemoryStream ((byte[])data, false);
			}
		}

		public virtual Stream GetManifestResourceStream (Type type, String name)
		{
			string ns;
			if (type != null)
				ns = type.Namespace;
			else 
				ns = null;

			if ((ns == null) || (ns == ""))
				return GetManifestResourceStream (name);
			else
				return GetManifestResourceStream (ns + "." + name);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern Type[] GetTypes (bool exportedOnly);
		
		public virtual Type[] GetTypes ()
		{
			return GetTypes (false);
		}

		public virtual Type[] GetExportedTypes ()
		{
			return GetTypes (true);
		}

		public virtual Type GetType (String name, Boolean throwOnError)
		{
			return GetType (name, throwOnError, false);
		}

		public virtual Type GetType (String name) {
			return GetType (name, false, false);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern Type InternalGetType (String name, Boolean throwOnError, Boolean ignoreCase);

		public Type GetType (string name, bool throwOnError, bool ignoreCase)
		{
			if (name == null)
				throw new ArgumentNullException (name);

			return InternalGetType (name, throwOnError, ignoreCase);
		}
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void FillName (Assembly ass, AssemblyName aname);
		
		public virtual AssemblyName GetName (Boolean copiedName)
		{
			AssemblyName aname = new AssemblyName ();
			FillName (this, aname);
			return aname;
		}

		public virtual AssemblyName GetName ()
		{
			return GetName (false);
		}

		public override String ToString ()
		{
			return GetName ().Name;
		}

		[MonoTODO]
		public static String CreateQualifiedName (String assemblyName, String typeName) 
		{
			return typeName + "," + assemblyName;
		}

		public static Assembly GetAssembly (Type type)
		{
			if (type != null)
				return type.Assembly;
			throw new ArgumentNullException ("type");
		}


		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public static extern Assembly GetEntryAssembly();

		public Assembly GetSatelliteAssembly (CultureInfo culture)
		{
			return GetSatelliteAssembly (culture, null);
		}

		public Assembly GetSatelliteAssembly (CultureInfo culture, Version version)
		{
			if (culture == null)
				throw new ArgumentException ("culture");

			AssemblyName aname = GetName (true);
			if (version != null)
				aname.Version = version;

			aname.CultureInfo = culture;
			aname.Name = aname.Name + ".resources";
			return Load (aname);
		}
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static Assembly LoadFrom (String assemblyFile);

		[MonoTODO]
		public static Assembly LoadFrom (String assemblyFile, Evidence securityEvidence)
		{
			throw new NotImplementedException ();
		}

		public static Assembly Load (String assemblyString)
		{
			return AppDomain.CurrentDomain.Load (assemblyString);
		}
		
		public static Assembly Load (String assemblyString, Evidence assemblySecurity)
		{
			return AppDomain.CurrentDomain.Load (assemblyString, assemblySecurity);
		}

		public static Assembly Load (AssemblyName assemblyRef)
		{
			return AppDomain.CurrentDomain.Load (assemblyRef);
		}

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

		public static Assembly Load (Byte[] rawAssembly, Byte[] rawSymbolStore,
					     Evidence securityEvidence)
		{
			return AppDomain.CurrentDomain.Load (rawAssembly, rawSymbolStore, securityEvidence);
		}

		public static Assembly LoadWithPartialName (string partialName)
		{
			return LoadWithPartialName (partialName, null);
		}

		[MonoTODO]
		public Module LoadModule (string moduleName, byte [] rawModule)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Module LoadModule (string moduleName, byte [] rawModule, byte [] rawSymbolStore)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Assembly LoadWithPartialName (string partialName, Evidence securityEvidence)
		{
			return AppDomain.CurrentDomain.Load (partialName, securityEvidence);
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

			return Activator.CreateInstance (t);
		}

		public Object CreateInstance (String typeName, Boolean ignoreCase,
					      BindingFlags bindingAttr, Binder binder,
					      Object[] args, CultureInfo culture,
					      Object[] activationAttributes)
		{
			Type t = GetType (typeName, false, ignoreCase);
			if (t == null)
				return null;

			return Activator.CreateInstance (t, bindingAttr, binder, args, culture, activationAttributes);
		}

		public Module[] GetLoadedModules ()
		{
			return GetLoadedModules (false);
		}

		[MonoTODO]
		public Module[] GetLoadedModules (bool getResourceModules)
		{
			// Currently, the two sets of modules are equal
			return GetModules (getResourceModules);
		}

		public Module[] GetModules ()
		{
			return GetModules (false);
		}

		public Module GetModule (String name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (name == "")
				throw new ArgumentException ("Name can't be empty");

			Module[] modules = GetModules (true);
			foreach (Module module in GetModules (true)) {
				if (module.ScopeName == name)
					return module;
			}

			return null;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern Module[] GetModulesInternal ();

		public Module[] GetModules (bool getResourceModules) {
			Module[] modules = GetModulesInternal ();

			if (!getResourceModules) {
				ArrayList result = new ArrayList (modules.Length);
				foreach (Module m in modules)
					if (!m.IsResource ())
						result.Add (m);
				return (Module[])result.ToArray (typeof (Module));
			}
			else
				return modules;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern string[] GetNamespaces ();
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern virtual String[] GetManifestResourceNames ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static Assembly GetExecutingAssembly ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static Assembly GetCallingAssembly ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern AssemblyName[] GetReferencedAssemblies ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern bool GetManifestResourceInfoInternal (String name, ManifestResourceInfo info);

		public virtual ManifestResourceInfo GetManifestResourceInfo (String resourceName)
		{
			if (resourceName == null)
				throw new ArgumentNullException ("resourceName");
			if (resourceName == "")
				throw new ArgumentException ("String cannot have zero length.");
			ManifestResourceInfo result = new ManifestResourceInfo ();
			bool found = GetManifestResourceInfoInternal (resourceName, result);
			if (found)
				return result;
			else
				return null;
		}

		//
		// The following functions are only for the Mono Debugger.
		//

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern MethodBase MonoDebugger_GetMethod (int token);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern int MonoDebugger_GetMethodToken (MethodBase method);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern Type MonoDebugger_GetLocalTypeFromSignature (byte[] signature);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern Type MonoDebugger_GetType (int token);
	}
}
