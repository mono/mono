//	
// System.Resources.ResourceManager.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
//	Dick Porter (dick@ximian.com)
// 	Alexander Olk (alex.olk@googlemail.com)
//
// (C) 2001, 2002 Ximian, Inc. http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Reflection;
using System.Globalization;
using System.Runtime.InteropServices;
using System.IO;

namespace System.Resources
{
	[Serializable]
#if NET_2_0
	[ComVisible (true)]
#endif
	public class ResourceManager
	{
		public static readonly int HeaderVersionNumber = 1;
		public static readonly int MagicNumber = unchecked((int)0xBEEFCACE);

		protected string BaseNameField;
		protected Assembly MainAssembly;
		// Maps cultures to ResourceSet objects
		protected Hashtable ResourceSets;
		
		private bool ignoreCase;
		private Type resourceSource;
		private Type resourceSetType;
		private String resourceDir;
		
		/* Recursing through culture parents stops here */
		private CultureInfo neutral_culture;

#if NET_2_0
		private UltimateResourceFallbackLocation fallbackLocation;
#endif
		
		// constructors
		protected ResourceManager ()
		{
			resourceSetType = typeof (ResourceSet);
		}
		
		public ResourceManager (Type resourceSource) : this ()
		{
			if (resourceSource == null)
				throw new ArgumentNullException ("resourceSource");

			this.resourceSource = resourceSource;
			ResourceSets = new Hashtable();
			BaseNameField = resourceSource.Name;
			MainAssembly = resourceSource.Assembly;

			/* Temporary workaround for bug 43567 */
			resourceSetType = typeof (ResourceSet);
			neutral_culture = GetNeutralResourcesLanguage (MainAssembly);
		}

		public ResourceManager (string baseName, Assembly assembly) : this()
		{
			CheckBaseName (baseName);
			if (assembly == null)
				throw new ArgumentNullException ("assembly");
			
			ResourceSets = new Hashtable ();
			BaseNameField = baseName;
			MainAssembly = assembly;
			neutral_culture = GetNeutralResourcesLanguage (MainAssembly);
		}

		private Type CheckResourceSetType (Type usingResourceSet, bool verifyType)
		{
			if (usingResourceSet == null) {
				return typeof (ResourceSet);
			} else {
				if (verifyType && !typeof (ResourceSet).IsAssignableFrom (usingResourceSet))
					throw new ArgumentException ("Type parameter"
						+ " must refer to a subclass of"
						+ " ResourceSet.", "usingResourceSet");
				return usingResourceSet;
			}
		}

		public ResourceManager (string baseName, Assembly assembly, Type usingResourceSet) : this()
		{
			CheckBaseName (baseName);
			if (assembly == null)
				throw new ArgumentNullException ("assembly");
			
			ResourceSets = new Hashtable ();
			BaseNameField = baseName;
			MainAssembly = assembly;
			resourceSetType = CheckResourceSetType (usingResourceSet, true);
			neutral_culture = GetNeutralResourcesLanguage (MainAssembly);
		}

		/* Private constructor for CreateFileBasedResourceManager */
		private ResourceManager(String baseName, String resourceDir, Type usingResourceSet) : this()
		{
			CheckBaseName (baseName);
			if (resourceDir == null)
				throw new ArgumentNullException("resourceDir");

			ResourceSets = new Hashtable ();
			BaseNameField = baseName;
			MainAssembly = null;
			resourceSetType = CheckResourceSetType (usingResourceSet, false);
			this.resourceDir = resourceDir;
		}
		
		public static ResourceManager CreateFileBasedResourceManager (string baseName,
						      string resourceDir, Type usingResourceSet)
		{
			return new ResourceManager (baseName, resourceDir, usingResourceSet);
		}

		public virtual string BaseName {
			get { return BaseNameField; }
		}

		public virtual bool IgnoreCase {
			get { return ignoreCase; }
			set { ignoreCase = value; }
		}

		public virtual Type ResourceSetType {
			get { return resourceSetType; }
		}

		public virtual object GetObject (string name)
		{
			return GetObject (name, null);
		}

		public virtual object GetObject (string name, CultureInfo culture)
		{
			if (name == null)
				throw new ArgumentNullException("name");

			if (culture == null)
				culture = CultureInfo.CurrentUICulture;

			lock (this) {
				ResourceSet set = InternalGetResourceSet(culture, true, true);
				object obj = null;
				
				if (set != null) {
					obj = set.GetObject(name, ignoreCase);
					if (obj != null)
						return obj;
				}
				
				/* Try parent cultures */

				do {
					culture = culture.Parent;

					set = InternalGetResourceSet (culture, true, true);
					if (set != null) {
						obj = set.GetObject (name, ignoreCase);
						if (obj != null)
							return obj;
					}
				} while (!culture.Equals (neutral_culture) &&
					!culture.Equals (CultureInfo.InvariantCulture));
			}

			return null;
		}

		public virtual ResourceSet GetResourceSet (CultureInfo culture,
					   bool createIfNotExists, bool tryParents)
			
		{
			if (culture == null)
				throw new ArgumentNullException ("culture");

			lock (this) {
				return InternalGetResourceSet (culture, createIfNotExists, tryParents);
			}
		}
		
		public virtual string GetString (string name)
		{
			return GetString (name, null);
		}

		public virtual string GetString (string name, CultureInfo culture)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			if (culture == null)
				culture = CultureInfo.CurrentUICulture;

			lock (this) {
				ResourceSet set = InternalGetResourceSet (culture, true, true);
				string str = null;

				if (set != null) {
					str = set.GetString (name, ignoreCase);
					if (str != null)
						return str;
				}

				/* Try parent cultures */

				do {
					culture = culture.Parent;
					set = InternalGetResourceSet (culture, true, true);
					if (set != null) {
						str = set.GetString(name, ignoreCase);
						if (str != null)
							return str;
					}
				} while (!culture.Equals (neutral_culture) &&
					!culture.Equals (CultureInfo.InvariantCulture));
			}
			
			return null;
		}

		protected virtual string GetResourceFileName (CultureInfo culture)
		{
			if (culture.Equals (CultureInfo.InvariantCulture))
				return BaseNameField + ".resources";
			else
				return BaseNameField + "." +  culture.Name + ".resources";
		}
		
		Stream GetManifestResourceStreamNoCase (Assembly ass, string fn)
		{
			string resourceName = GetManifestResourceName (fn);

			foreach (string s in ass.GetManifestResourceNames ())
				if (String.Compare (resourceName, s, true, CultureInfo.InvariantCulture) == 0)
					return ass.GetManifestResourceStream (s);
			return null;
		}

#if NET_2_0
		[CLSCompliant (false)]
		[ComVisible (false)]
		public UnmanagedMemoryStream GetStream (string name)
		{
			return GetStream (name, (CultureInfo) null);
		}

		[CLSCompliant (false)]
		[ComVisible (false)]
		public UnmanagedMemoryStream GetStream (string name, CultureInfo culture)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (culture == null)
				culture = CultureInfo.CurrentUICulture;
			ResourceSet set = InternalGetResourceSet (culture, true, true);
			if (set == null)
				throw new MissingManifestResourceException (String.Format ("Could not find any resource appropriate for the specified culture or its parents (resource file : \"{0}\")", GetResourceFileName(culture)));

			return set.GetStream (name);
		}
#endif
		protected virtual ResourceSet InternalGetResourceSet (CultureInfo culture, bool Createifnotexists, bool tryParents)
		{
			ResourceSet set;
			
			if (culture == null) {
				string msg = String.Format ("Could not find any resource appropriate for the " +
							    "specified culture or its parents (assembly:{0})",
							    MainAssembly != null ? MainAssembly.GetName ().Name : "");
				throw new MissingManifestResourceException (msg);
			}
			/* if we already have this resource set, return it */
			set = (ResourceSet) ResourceSets [culture];
			if (set != null)
				return set;

			if (MainAssembly != null) {
				/* Assembly resources */
				CultureInfo resourceCulture = culture;

				// when the specified culture matches the neutral culture,
				// then use the invariant resources
				if (culture.Equals (neutral_culture))
					resourceCulture = CultureInfo.InvariantCulture;

				Stream stream = null;

				string filename = GetResourceFileName (resourceCulture);
				if (!resourceCulture.Equals (CultureInfo.InvariantCulture)) {
					/* Try a satellite assembly */
					Version sat_version = GetSatelliteContractVersion (MainAssembly);
					try {
						Assembly a = MainAssembly.GetSatelliteAssembly (
							resourceCulture, sat_version);
						stream = a.GetManifestResourceStream (filename);
						if (stream == null)
							stream = GetManifestResourceStreamNoCase (a, filename);
					} catch (Exception) {
						// Ignored
					}
				} else {
					stream = MainAssembly.GetManifestResourceStream (
						resourceSource, filename);
					if (stream == null)
						stream = GetManifestResourceStreamNoCase (
							MainAssembly, filename);
				}

				if (stream != null && Createifnotexists) {
					object [] args = new Object [1] { stream };
					
					/* should we catch
					 * MissingMethodException, or
					 * just let someone else deal
					 * with it?
					 */
					set = (ResourceSet) Activator.CreateInstance (resourceSetType, args);
				} else if (resourceCulture.Equals (CultureInfo.InvariantCulture)) {
					string msg = "Could not find any resource appropriate for the " +
						"specified culture or the neutral culture. " +
						"Make sure \"{1}\" was correctly embedded or " +
						"linked into assembly \"{0}\".";
					msg = String.Format (msg, MainAssembly != null ?
						MainAssembly.GetName ().Name : "",
						GetManifestResourceName (filename));
					throw new MissingManifestResourceException (msg);
				}
			} else if (resourceDir != null || BaseNameField != null) {
				/* File resources */
				string filename;
				
				if (resourceDir != null)
					filename = Path.Combine (resourceDir, GetResourceFileName (culture));
				else
					filename = GetResourceFileName(culture);

				if (Createifnotexists && File.Exists (filename)) {
					object [] args = new Object [1] { filename };

					/* should we catch
					 * MissingMethodException, or
					 * just let someone else deal
					 * with it?
					 */
					set = (ResourceSet) Activator.CreateInstance(
						resourceSetType, args);
				}
			}

			if (set == null && tryParents) {
				// avoid endless recursion
				if (!culture.Equals (CultureInfo.InvariantCulture))
					set = InternalGetResourceSet (culture.Parent,
						Createifnotexists, tryParents);
			}

			if (set != null)
				ResourceSets.Add (culture, set);

			return set;
		}

		public virtual void ReleaseAllResources ()
		{
			lock(this) {
				foreach (ResourceSet r in ResourceSets.Values)
					r.Close();
				ResourceSets.Clear();
			}
		}

		protected static CultureInfo GetNeutralResourcesLanguage (Assembly a)
		{
			object [] attrs = a.GetCustomAttributes (
				typeof (NeutralResourcesLanguageAttribute),
				false);

			if (attrs.Length == 0) {
				return CultureInfo.InvariantCulture;
			} else {
				NeutralResourcesLanguageAttribute res_attr = (NeutralResourcesLanguageAttribute) attrs [0];
				return new CultureInfo (res_attr.CultureName);
			}
		}

		protected static Version GetSatelliteContractVersion (Assembly a)
		{
			object [] attrs = a.GetCustomAttributes (
				typeof (SatelliteContractVersionAttribute),
				false);
			if (attrs.Length == 0) {
				return null;
			} else {
				SatelliteContractVersionAttribute sat_attr =
					(SatelliteContractVersionAttribute) attrs[0];

				/* Version(string) can throw
				 * ArgumentException if the version is
				 * invalid, but the spec for
				 * GetSatelliteContractVersion says we
				 * can throw the same exception for
				 * the same reason, so dont bother to
				 * catch it.
				 */
				return new Version (sat_attr.Version);
			}
		}

#if NET_2_0
		[MonoTODO ("the property exists but is not respected")]
		protected UltimateResourceFallbackLocation FallbackLocation {
			get { return fallbackLocation; }
			set { fallbackLocation = value; }
		}
#endif

		void CheckBaseName (string baseName)
		{
			if (baseName == null)
				throw new ArgumentNullException ("baseName");
#if ONLY_1_1
			if (baseName.EndsWith (".resources"))
				throw new ArgumentException ("ResourceManager base"
					+ " name should not end in .resources. It"
					+ " should be similar to MyResources,"
					+ " which the ResourceManager can convert"
					+ " into MyResources.<culture>.resources;"
					+ " for example, MyResources.en-US.resources");
#endif
		}

		string GetManifestResourceName (string fn)
		{
			string resourceName = null;
			if (resourceSource != null) {
				if (resourceSource.Namespace != null && resourceSource.Namespace.Length > 0)
					resourceName = string.Concat (resourceSource.Namespace,
						".", fn);
				else
					resourceName = fn;
			} else {
				resourceName = fn;
			}
			return resourceName;
		}
	}
}
