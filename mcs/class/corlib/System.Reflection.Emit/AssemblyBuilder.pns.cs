//
// AssemblyBuilder.pns.cs
//
// Authors:
//   Marek Safar (marek.safar@gmail.com)
//
// Copyright (C) 2016 Xamarin Inc (http://www.xamarin.com)
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

#if !MONO_FEATURE_SRE
using System.Globalization;
using System.IO;

namespace System.Reflection.Emit
{
	public class AssemblyBuilder : Assembly
	{
		private AssemblyBuilder () {}

		public override string CodeBase {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override MethodInfo EntryPoint {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override string EscapedCodeBase {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override System.Security.Policy.Evidence Evidence {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override string FullName {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override bool GlobalAssemblyCache {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override string ImageRuntimeVersion {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override bool IsDynamic {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override string Location {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override Module ManifestModule {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public override bool ReflectionOnly {
			get {
				throw new PlatformNotSupportedException ();
			}
		}

		public void AddResourceFile (string name, string fileName) => throw new PlatformNotSupportedException ();

		public void AddResourceFile (string name, string fileName, ResourceAttributes attribute) => throw new PlatformNotSupportedException ();

		public ModuleBuilder DefineDynamicModule (string name, bool emitSymbolInfo) => throw new PlatformNotSupportedException ();

		public ModuleBuilder DefineDynamicModule (string name, string fileName) => throw new PlatformNotSupportedException ();

		public ModuleBuilder DefineDynamicModule (string name, string fileName, bool emitSymbolInfo) => throw new PlatformNotSupportedException ();

		public System.Resources.IResourceWriter DefineResource (string name, string description, string fileName) => throw new PlatformNotSupportedException ();

		public System.Resources.IResourceWriter DefineResource (string name, string description, string fileName, ResourceAttributes attribute) => throw new PlatformNotSupportedException ();

		public void DefineUnmanagedResource (byte[] resource) => throw new PlatformNotSupportedException ();

		public void DefineUnmanagedResource (string resourceFileName) => throw new PlatformNotSupportedException ();

		public void DefineVersionInfoResource () => throw new PlatformNotSupportedException ();

		public void DefineVersionInfoResource (string product, string productVersion, string company, string copyright, string trademark) => throw new PlatformNotSupportedException ();

		public static AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access)
		{
			throw new PlatformNotSupportedException ();
		}

		public static AssemblyBuilder DefineDynamicAssembly (AssemblyName name, AssemblyBuilderAccess access, System.Collections.Generic.IEnumerable<CustomAttributeBuilder> assemblyAttributes)
		{
			throw new PlatformNotSupportedException ();
		}

		public ModuleBuilder DefineDynamicModule (string name)
		{
			throw new PlatformNotSupportedException ();
		}

		public override bool Equals (object obj) => throw new PlatformNotSupportedException ();

		public override object[] GetCustomAttributes (bool inherit) => throw new PlatformNotSupportedException ();

		public override object[] GetCustomAttributes (System.Type attributeType, bool inherit) => throw new PlatformNotSupportedException ();

		public ModuleBuilder GetDynamicModule (string name)
		{
			throw new PlatformNotSupportedException ();
		}

		public override Type[] GetExportedTypes () => throw new PlatformNotSupportedException ();

		public override FileStream GetFile (string name) => throw new PlatformNotSupportedException ();

		public override FileStream[] GetFiles (bool getResourceModules) => throw new PlatformNotSupportedException ();

		public override int GetHashCode () => throw new PlatformNotSupportedException ();

		public override Module[] GetLoadedModules (bool getResourceModules) => throw new PlatformNotSupportedException ();

		public override ManifestResourceInfo GetManifestResourceInfo (string resourceName) => throw new PlatformNotSupportedException ();

		public override string[] GetManifestResourceNames () => throw new PlatformNotSupportedException ();

		public override Stream GetManifestResourceStream (string name) => throw new PlatformNotSupportedException ();

		public override Stream GetManifestResourceStream (Type type, string name) => throw new PlatformNotSupportedException ();

		public override Module GetModule (string name) => throw new PlatformNotSupportedException ();

		public override Module[] GetModules (bool getResourceModules) => throw new PlatformNotSupportedException ();

		public override AssemblyName GetName (bool copiedName) => throw new PlatformNotSupportedException ();

		public override AssemblyName[] GetReferencedAssemblies () => throw new PlatformNotSupportedException ();

		public override Assembly GetSatelliteAssembly (CultureInfo culture) => throw new PlatformNotSupportedException ();

		public override Assembly GetSatelliteAssembly (CultureInfo culture, Version version) => throw new PlatformNotSupportedException ();

		public override Type GetType (string name, bool throwOnError, bool ignoreCase) => throw new PlatformNotSupportedException ();

		public override bool IsDefined (Type attributeType, bool inherit) => throw new PlatformNotSupportedException ();

		public void Save (string assemblyFileName) => throw new PlatformNotSupportedException ();

		public void Save (string assemblyFileName, PortableExecutableKinds portableExecutableKind, ImageFileMachine imageFileMachine) => throw new PlatformNotSupportedException ();

		public void SetCustomAttribute (CustomAttributeBuilder customBuilder)
		{
			throw new PlatformNotSupportedException ();
		}


		public void SetCustomAttribute (ConstructorInfo con, byte[] binaryAttribute)
		{
			throw new PlatformNotSupportedException ();
		}

		public void SetEntryPoint (MethodInfo entryMethod) => throw new PlatformNotSupportedException ();

		public void SetEntryPoint (MethodInfo entryMethod, PEFileKinds fileKind) => throw new PlatformNotSupportedException ();

		public override string ToString () => throw new PlatformNotSupportedException ();
	}
}

#endif
