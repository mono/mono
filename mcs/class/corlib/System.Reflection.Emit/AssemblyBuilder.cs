using System;
using System.Reflection;
using System.Resources;
using System.IO;
using System.Security.Policy;
using System.Runtime.Serialization;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System.Reflection.Emit {
	public sealed class AssemblyBuilder : Assembly {
		private IntPtr _impl;

		public override string CodeBase {get {return null;}}
		public override MethodInfo EntryPoint {get {return null;}}

		public override string Location {get {return null;}}


		public void AddResourceFile( string name, string fileName) {
		}
		public void AddResourceFile( string name, string fileName, ResourceAttributes attribute) {
		}
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern ModuleBuilder defineModule (AssemblyBuilder ab, string name, string filename);
		
		public ModuleBuilder DefineDynamicModule( string name) {
			return null;
		}
		public ModuleBuilder DefineDynamicModule( string name, bool emitSymbolInfo) {
			return null;
		}
		public ModuleBuilder DefineDynamicModule( string name, string fileName) {
			return defineModule (this, name, fileName);
		}
		public ModuleBuilder DefineDynamicModule( string name, string fileName, bool emitSymbolInfo) {
			return null;
		}
		public IResourceWriter DefineResource( string name, string description, string fileName) {
			return null;
		}
		public IResourceWriter DefineResource( string name, string description, string fileName, ResourceAttributes attribute) {
			return null;
		}
		public void DefineUnmanagedResource( byte[] resource) {
		}
		public void DefineUnmanagedResource( string resourceFileName) {
		}
		public void DefineVersionInfoResource() {
		}
		public void DefineVersionInfoResource( string product, string productVersion, string company, string copyright, string trademark) {
		}
		public ModuleBuilder GetDynamicModule( string name) {
			return null;
		}
		public override Type[] GetExportedTypes() {
			return null;
		}
		public override FileStream GetFile( string name) {
			return null;
		}
		/*public virtual FileStream[] GetFiles() {
			return null;
		}
		public override FileStream[] GetFiles( bool getResourceModules) {
			return null;
		}*/
		public Module[] GetLoadedModules() {
			return null;
		}
		public Module[] GetLoadedModules( bool getResourceModules) {
			return null;
		}
		/*public virtual ManifestResourceInfo GetManifestResourceInfo( string resourceName) {
			return null;
		}
		public virtual string[] GetManifestResourceNames() {
			return null;
		}
		public virtual Stream GetManifestResourceStream( string name) {
			return null;
		}
		public virtual Stream GetManifestResourceStream( Type type, string name) {
			return null;
		}*/
		public Module GetModule( string name) {
			return null;
		}
		public Module[] GetModules() {
			return null;
		}
		public Module[] GetModules( bool getResourceModules) {
			return null;
		}
		/*public virtual AssemblyName GetName() {
			return null;
		}
		public virtual AssemblyName GetName( bool copiedName) {
			return null;
		}
		public virtual void GetObjectData( SerializationInfo info, StreamingContext context) {
		}*/
		public AssemblyName[] GetReferencedAssemblies() {
			return null;
		}
		public Assembly GetSatelliteAssembly( CultureInfo culture) {
			return null;
		}

		public Assembly GetSatelliteAssembly( CultureInfo culture, Version version) {
			return null;
		}
		/*public virtual Type GetType( string name) {
			return null;
		}
		public virtual Type GetType( string name, bool throwOnError) {
			return null;
		}*/
		public Type GetType( string name, bool throwOnError, bool ignoreCase) {
			return null;
		}
		/*public virtual Type[] GetTypes() {
			return null;
		}
		public virtual bool IsDefined( Type attributeType, bool inherit) {
			return false;
		}*/
		public static Assembly Load( AssemblyName assemblyRef) {
			return null;
		}

		public static Assembly Load( byte[] rawAssembly) {
			return null;
		}

		public static Assembly Load( string assemblyString) {
			return null;
		}

		public static Assembly Load( AssemblyName assemblyRef, Evidence assemblySecurity) {
			return null;
		}

		public static Assembly Load( byte[] rawAssembly, byte[] rawSymbolStore) {
			return null;
		}

		public static Assembly Load( string assemblyString, Evidence assemblySecurity) {
			return null;
		}

		public static Assembly Load( byte[] rawAssembly, byte[] rawSymbolStore, Evidence securityEvidence) {
			return null;
		}
		public static Assembly LoadFrom( string assemblyFile) {
			return null;
		}

		public static Assembly LoadFrom( string assemblyFile, Evidence securityEvidence) {
			return null;
		}
		public Module LoadModule( string moduleName, byte[] rawModule) {
			return null;
		}

		public Module LoadModule( string moduleName, byte[] rawModule, byte[] rawSymbolStore) {
			return null;
		}
		public override string ToString() {
			return "AssemblyBuilder";
		}
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern int getDataChunk (AssemblyBuilder ab, int type, byte[] buf);

		public void Save( string assemblyFileName) {
			byte[] buf = new byte[4096];
			FileStream file = new FileStream (assemblyFileName, FileMode.OpenOrCreate, FileAccess.Write);
			int count;

			count = getDataChunk (this, 0, buf);
			file.Write (buf, 0, count);

			file.Close ();
		}
		public void SetEntryPoint( MethodInfo entryMethod) {
			
		}

	}
}
