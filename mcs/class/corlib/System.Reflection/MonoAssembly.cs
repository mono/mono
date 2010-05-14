//
// System.Reflection/MonoAssembly.cs
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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Reflection.Emit;


namespace System.Reflection {

#if NET_4_0
	[ComVisible (true)]
	[ComDefaultInterfaceAttribute (typeof (_Assembly))]
	[Serializable]
	[ClassInterface(ClassInterfaceType.None)]
	class MonoAssembly : Assembly {
#else
	public partial class Assembly {
#endif
		public
#if NET_4_0
		override
#endif
		Type GetType (string name, bool throwOnError, bool ignoreCase)
		{
			Type res;
			if (name == null)
				throw new ArgumentNullException (name);
			if (name.Length == 0)
			throw new ArgumentException ("name", "Name cannot be empty");

			res = InternalGetType (null, name, throwOnError, ignoreCase);
#if !NET_4_0
			if (res is TypeBuilder) {
				if (throwOnError)
					throw new TypeLoadException (string.Format ("Could not load type '{0}' from assembly '{1}'", name, this));
				return null;
			}
#endif
			return res;
		}

		public
#if NET_4_0
		override
#endif
		Module GetModule (String name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (name.Length == 0)
				throw new ArgumentException ("Name can't be empty");

			Module[] modules = GetModules (true);
			foreach (Module module in modules) {
				if (module.ScopeName == name)
					return module;
			}

			return null;
		}

		public
#if NET_4_0
		override
#endif
		AssemblyName[] GetReferencedAssemblies () {
			return GetReferencedAssemblies (this);
		}

		public
#if NET_4_0
		override
#endif
		Module[] GetModules (bool getResourceModules) {
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

		[MonoTODO ("Always returns the same as GetModules")]
		public
#if NET_4_0
		override
#endif
		Module[] GetLoadedModules (bool getResourceModules)
		{
			return GetModules (getResourceModules);
		}

		public
#if NET_4_0
		override
#endif
		Assembly GetSatelliteAssembly (CultureInfo culture)
		{
			return GetSatelliteAssembly (culture, null, true);
		}

		public
#if NET_4_0
		override
#endif
		Assembly GetSatelliteAssembly (CultureInfo culture, Version version)
		{
			return GetSatelliteAssembly (culture, version, true);
		}

		//FIXME remove GetManifestModule under v4, it's a v2 artifact
		[ComVisible (false)]
		public
#if NET_4_0
		override
#endif
		Module ManifestModule {
			get {
				return GetManifestModule ();
			}
		}

#if !MOONLIGHT
		public
#if NET_4_0
		override
#endif
		bool GlobalAssemblyCache {
			get {
				return get_global_assembly_cache ();
			}
		}
#endif

	}
}


