//
// System.Runtime.InteropServices._Assembly interface
//
// Author:
//	Andreas Nahr <ClassDevelopment@A-SoftTech.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.IO;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Policy;

namespace System.Runtime.InteropServices
{
	[ComVisible (true)]
	[CLSCompliant (false)]
	[InterfaceType (ComInterfaceType.InterfaceIsDual)]
	[Guid ("17156360-2F1A-384A-BC52-FDE93C215C5B")]
#if !FULL_AOT_RUNTIME
	[TypeLibImportClass (typeof(Assembly))]
#endif
	public interface _Assembly
	{
		string ToString ();

		bool Equals (object other);

		int GetHashCode ();

		Type GetType ();

		string CodeBase { get; }

		string EscapedCodeBase { get; }

		AssemblyName GetName ();

		AssemblyName GetName (bool copiedName);

		string FullName { get; }

		MethodInfo EntryPoint { get; }

		Type GetType (string name);

		Type GetType (string name, bool throwOnError);

		Type[] GetExportedTypes ();

		Type[] GetTypes();

		Stream GetManifestResourceStream (Type type, string name);

		Stream GetManifestResourceStream (string name);

		FileStream GetFile (string name);

		FileStream[] GetFiles ();

		FileStream[] GetFiles (bool getResourceModules);

		string[] GetManifestResourceNames ();

		ManifestResourceInfo GetManifestResourceInfo (string resourceName);

		string Location { get; }

#if !MOBILE
		Evidence Evidence { get; }
#endif

		object[] GetCustomAttributes (Type attributeType, bool inherit);

		object[] GetCustomAttributes (bool inherit);

		bool IsDefined (Type attributeType, bool inherit);

		//[SecurityPermission (SecurityAction.LinkDemand, SerializationFormatter = true)]
		void GetObjectData (SerializationInfo info, StreamingContext context);

		Type GetType (string name, bool throwOnError, bool ignoreCase);

		Assembly GetSatelliteAssembly (CultureInfo culture);

		Assembly GetSatelliteAssembly (CultureInfo culture, Version version);

		Module LoadModule (string moduleName, byte[] rawModule);

		Module LoadModule (string moduleName, byte[] rawModule, byte[] rawSymbolStore);

		object CreateInstance (string typeName);

		object CreateInstance (string typeName, bool ignoreCase);

		object CreateInstance (string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args,
		                       CultureInfo culture, object[] activationAttributes);

		Module[] GetLoadedModules ();

		Module[] GetLoadedModules (bool getResourceModules);

		Module[] GetModules ();

		Module[] GetModules (bool getResourceModules);

		Module GetModule (string name);

		AssemblyName[] GetReferencedAssemblies ();

#if !MOBILE
		bool GlobalAssemblyCache { get; }
#endif

		event ModuleResolveEventHandler ModuleResolve;
	}
}

