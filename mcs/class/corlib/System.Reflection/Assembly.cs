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

namespace System.Reflection {

	[Serializable]
	public class Assembly : System.Reflection.ICustomAttributeProvider,
		System.Security.IEvidenceFactory, System.Runtime.Serialization.ISerializable {
		private IntPtr _mono_assembly;

		internal Assembly () {}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern string get_code_base ();
		
		public virtual string CodeBase {
			get {
				return get_code_base ();
			}
		}

		public virtual string CopiedCodeBase {
			get {
				return null;
			}
		} 

		public virtual string FullName {
			get {
				//
				// FIXME: This is wrong, but it gets us going
				// in the compiler for now
				//
				return CodeBase;
			}
		}

		public virtual MethodInfo EntryPoint {
			get {
				return null;
			}
		}

		public virtual Evidence Evidence {
			get {
				return null;
			}
		}

		public virtual String Location {
			get {
				return null;
			}
		}

		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
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

		public virtual FileStream[] GetFiles ()
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

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern Type[] GetTypes (bool exportedOnly);
		
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
		public extern Type GetType (String name, Boolean throwOnError, Boolean ignoreCase);
		
		public virtual AssemblyName GetName (Boolean copiedName)
		{
			throw new NotImplementedException ();
		}

		public virtual AssemblyName GetName ()
		{
			throw new NotImplementedException ();
		}

		public override String ToString ()
		{
			return GetName ().Name;
		}

		[MonoTODO]
		public static String CreateQualifiedName (String assemblyName, String typeName) 
		{
			return "FIXME: assembly";
		}

		[MonoTODO]
		public static String nCreateQualifiedName (String assemblyName, String typeName)
		{
			return "FIXME: assembly";
		}

		[MonoTODO]
		public static Assembly GetAssembly (Type type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Assembly GetSatelliteAssembly (CultureInfo culture)
		{
			throw new NotImplementedException ();
		}

		public static Assembly LoadFrom (String assemblyFile)
		{
			return AppDomain.CurrentDomain.Load (assemblyFile);
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
		public static Assembly LoadWithPartialName (string partialName, Evidence securityEvidence)
		{
			return AppDomain.CurrentDomain.Load (partialName, securityEvidence);
		}


		public Object CreateInstance (String typeName) 
		{
			throw new NotImplementedException ();
		}

		public Object CreateInstance (String typeName, Boolean ignoreCase)
		{
			throw new NotImplementedException ();
		}

		public Object CreateInstance (String typeName, Boolean ignoreCase,
					      BindingFlags bindingAttr, Binder binder,
					      Object[] args, CultureInfo culture,
					      Object[] activationAttributes)
		{
			throw new NotImplementedException ();
		}

		public Module[] GetLoadedModules ()
		{
			throw new NotImplementedException ();
		}

		public Module[] GetModules ()
		{
			throw new NotImplementedException ();
		}

		public Module GetModule (String name)
		{
			throw new NotImplementedException ();
		}

		public virtual String[] GetManifestResourceNames ()
		{
			throw new NotImplementedException ();
		}

		public static Assembly GetExecutingAssembly ()
		{
			throw new NotImplementedException ();
		}

		public AssemblyName[] GetReferencedAssemblies ()
		{
			throw new NotImplementedException ();
		}

		public virtual ManifestResourceInfo GetManifestResourceInfo (String resourceName)
		{
			throw new NotImplementedException ();
		}
	}
}
