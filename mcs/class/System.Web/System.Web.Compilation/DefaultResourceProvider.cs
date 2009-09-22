// DefaultResourceProvider.cs
//
// Authors:
//	Marek Habersack (mhabersack@novell.com)
//
// (C) 2009 Novell, Inc (http://novell.com)

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
#if NET_2_0
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Reflection;
using System.Web;

namespace System.Web.Compilation
{
	sealed class DefaultResourceProvider : IResourceProvider
	{
		sealed class ResourceManagerCacheKey
		{
			readonly string _name;
			readonly Assembly _asm;

			public ResourceManagerCacheKey (string name, Assembly asm)
			{
				_name = name;
				_asm = asm;
			}

			public override bool Equals (object obj)
			{
				if (!(obj is ResourceManagerCacheKey))
					return false;
				ResourceManagerCacheKey key = (ResourceManagerCacheKey) obj;
				return key._asm == _asm && _name.Equals (key._name, StringComparison.Ordinal);
			}

			public override int GetHashCode ()
			{
				return _name.GetHashCode () + _asm.GetHashCode ();
			}
		}
		
		[ThreadStatic]
		static Dictionary <ResourceManagerCacheKey, ResourceManager> resourceManagerCache;
		
		string resource;
		bool isGlobal;
		
		public IResourceReader ResourceReader {
			get {
				Assembly asm;
				string path;
					
				if (isGlobal) {
					asm = HttpContext.AppGlobalResourcesAssembly;
					path = resource;
				} else {
					asm = GetLocalResourcesAssembly ();
					path = Path.GetFileName (resource);

					if (String.IsNullOrEmpty (path))
						return null;

					path += ".resources";
				}
							
				if (asm == null)
					return null;

				Stream ms = asm.GetManifestResourceStream (path);
				if (ms == null)
					return null;
				
				return new ResourceReader (ms);
			}
		}
		
		public DefaultResourceProvider (string resource, bool isGlobal)
		{
			if (String.IsNullOrEmpty (resource))
				throw new ArgumentNullException ("resource");
			
			this.resource = resource;
			this.isGlobal = isGlobal;
		}

		public object GetObject (string resourceKey, CultureInfo culture)
		{
 			if (String.IsNullOrEmpty (resourceKey))
				return null;
			
			ResourceManager rm = GetResourceManager ();
			if (rm == null)
				return null;

			return rm.GetObject (resourceKey, culture);
		}

		Assembly GetLocalResourcesAssembly ()
		{
			string path;
			Assembly asm;

			path = VirtualPathUtility.GetDirectory (resource);
			asm = AppResourcesCompiler.GetCachedLocalResourcesAssembly (path);
			if (asm == null) {
				AppResourcesCompiler ac = new AppResourcesCompiler (path);
				asm = ac.Compile ();
				if (asm == null)
					throw new MissingManifestResourceException ("A resource object was not found at the specified virtualPath.");
			}

			return asm;
		}
		
		ResourceManager GetResourceManager ()
		{
			string path;
			Assembly asm;

			if (isGlobal) {
				asm = HttpContext.AppGlobalResourcesAssembly;
				path = resource;
			} else {
				asm = GetLocalResourcesAssembly ();
				path = Path.GetFileName (resource);

				if (String.IsNullOrEmpty (path))
					return null;
			}

			if (asm == null)
				return null;
			
			ResourceManager rm;
			try {
				if (resourceManagerCache == null)
					resourceManagerCache = new Dictionary <ResourceManagerCacheKey, ResourceManager> ();
				
				ResourceManagerCacheKey key = new ResourceManagerCacheKey (path, asm);
				if (!resourceManagerCache.TryGetValue (key, out rm)) {
					rm = new ResourceManager (path, asm);
					rm.IgnoreCase = true;
					resourceManagerCache.Add (key, rm);
				}
				
				return rm;
			} catch (MissingManifestResourceException) {
				throw;
			} catch (Exception ex) {
				throw new HttpException ("Failed to retrieve the specified global resource object.", ex);
			}
		}
	}
}
#endif
