//	
// System.Resources.ResourceManager.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
//	Dick Porter (dick@ximian.com)
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
using System.IO;

namespace System.Resources
{
	[Serializable]
	public class ResourceManager
	{
		public static readonly int HeaderVersionNumber = 1;
		public static readonly int MagicNumber = unchecked((int)0xBEEFCACE);

		protected string BaseNameField;
		protected Assembly MainAssembly;
		// Maps cultures to ResourceSet objects
		protected Hashtable ResourceSets;
		
		private bool ignoreCase;
		private Type resourceSetType;
		private String resourceDir;
		
		/* Recursing through culture parents stops here */
		private CultureInfo neutral_culture;
		
		// constructors
		protected ResourceManager () {
			ResourceSets=new Hashtable();
			ignoreCase=false;
			resourceSetType=typeof(ResourceSet);
			resourceDir=null;
			neutral_culture=null;
		}
		
		public ResourceManager (Type resourceSource) : this()
		{
			if (resourceSource == null)
				throw new ArgumentNullException ("resourceSource is null.");
			
			BaseNameField = resourceSource.FullName;
			MainAssembly = resourceSource.Assembly;

			/* Temporary workaround for bug 43567 */
			resourceSetType = typeof(ResourceSet);
			neutral_culture = GetNeutralResourcesLanguage(MainAssembly);
		}
		
		public ResourceManager (string baseName, Assembly assembly) : this()
		{
			if (baseName == null)
				throw new ArgumentNullException ("baseName is null.");
			if(assembly == null)
				throw new ArgumentNullException ("assembly is null.");
			
			BaseNameField = baseName;
			MainAssembly = assembly;
			neutral_culture = GetNeutralResourcesLanguage(MainAssembly);
		}
			 
		private Type CheckResourceSetType(Type usingResourceSet)
		{
			if(usingResourceSet==null) {
				return(typeof(ResourceSet));
			} else {
				if (!usingResourceSet.IsSubclassOf (typeof (ResourceSet)))
					throw new ArgumentException ("Type must be from ResourceSet.");
				
				return(usingResourceSet);
			}
		}
		
		public ResourceManager (string baseName, Assembly assembly, Type usingResourceSet) : this()
		{
			if (baseName == null)
				throw new ArgumentNullException ("baseName is null.");
			if(assembly == null)
				throw new ArgumentNullException ("assembly is null.");
			
			BaseNameField = baseName;
			MainAssembly = assembly;
			resourceSetType = CheckResourceSetType(usingResourceSet);
			neutral_culture = GetNeutralResourcesLanguage(MainAssembly);
		}
		
		/* Private constructor for CreateFileBasedResourceManager */
		private ResourceManager(String baseName, String resourceDir, Type usingResourceSet) : this()
		{
			if(baseName==null) {
				throw new ArgumentNullException("The base name is null");
			}
			if(baseName.EndsWith(".resources")) {
				throw new ArgumentException("The base name ends in '.resources'");
			}
			if(resourceDir==null) {
				throw new ArgumentNullException("The resourceDir is null");
			}

			BaseNameField = baseName;
			MainAssembly = null;
			resourceSetType = CheckResourceSetType(usingResourceSet);
			this.resourceDir = resourceDir;
		}
		
		public static ResourceManager CreateFileBasedResourceManager (string baseName,
						      string resourceDir, Type usingResourceSet)
		{
			return new ResourceManager(baseName, resourceDir, usingResourceSet);
		}

		public virtual string BaseName
		{
			get { return BaseNameField; }
		}

		public virtual bool IgnoreCase
		{
			get { return ignoreCase; }
			set { ignoreCase = value; }
		}

		public virtual Type ResourceSetType
		{
			get { return resourceSetType; }
		}

		public virtual object GetObject(string name)
		{
			return(GetObject(name, null));
		}

		public virtual object GetObject(string name, CultureInfo culture)
		{
			if(name==null) {
				throw new ArgumentNullException("name is null");
			}

			if(culture==null) {
				culture=CultureInfo.CurrentUICulture;
			}

			lock(this) {
				ResourceSet set=InternalGetResourceSet(culture, true, true);
				object obj=null;
				
				if(set != null) {
					obj=set.GetObject(name, ignoreCase);
					if(obj != null) {
						return(obj);
					}
				}
				
				/* Try parent cultures */

				do {
					culture=culture.Parent;

					set=InternalGetResourceSet(culture, true, true);
					if(set!=null) {
						obj=set.GetObject(name, ignoreCase);
						if(obj != null) {
							return(obj);
						}
					}
				} while(!culture.Equals(neutral_culture) &&
					!culture.Equals(CultureInfo.InvariantCulture));
			}
			
			return(null);
		}
		
		
		public virtual ResourceSet GetResourceSet (CultureInfo culture,
					   bool createIfNotExists, bool tryParents)
			
		{
			if (culture == null) {
				throw new ArgumentNullException ("CultureInfo is a null reference.");
			}

			lock(this) {
				return(InternalGetResourceSet(culture, createIfNotExists, tryParents));
			}
		}
		
		public virtual string GetString (string name)
		{
			return(GetString(name, null));
		}

		public virtual string GetString (string name, CultureInfo culture)
		{
			if (name == null) {
				throw new ArgumentNullException ("Name is null.");
			}

			if(culture==null) {
				culture=CultureInfo.CurrentUICulture;
			}

			lock(this) {
				ResourceSet set=InternalGetResourceSet(culture, true, true);
				string str=null;

				if(set!=null) {
					str=set.GetString(name, ignoreCase);
					if(str!=null) {
						return(str);
					}
				}

				/* Try parent cultures */

				do {
					culture=culture.Parent;

					set=InternalGetResourceSet(culture, true, true);
					if(set!=null) {
						str=set.GetString(name, ignoreCase);
						if(str!=null) {
							return(str);
						}
					}
				} while(!culture.Equals(neutral_culture) &&
					!culture.Equals(CultureInfo.InvariantCulture));
			}
			
			return(null);
		}

		protected virtual string GetResourceFileName (CultureInfo culture)
		{
			if(culture.Equals(CultureInfo.InvariantCulture)) {
				return(BaseNameField + ".resources");
			} else {
				return(BaseNameField + "." +  culture.Name + ".resources");
			}
		}
		
		static Stream GetManifestResourceStreamNoCase (Assembly ass, string fn)
		{
			foreach (string s in ass.GetManifestResourceNames ())
				if (String.Compare (fn, s, true, CultureInfo.InvariantCulture) == 0)
					return ass.GetManifestResourceStream (s);
			return null;
		}
		
		protected virtual ResourceSet InternalGetResourceSet (CultureInfo culture, bool Createifnotexists, bool tryParents)
		{
			ResourceSet set;
			
			if (culture == null) {
				string msg = String.Format ("Could not find any resource appropiate for the " +
							    "specified culture or its parents (assembly:{0})",
							    MainAssembly != null ? MainAssembly.GetName ().Name : "");
							    
				throw new MissingManifestResourceException (msg);
			}
			/* if we already have this resource set, return it */
			set=(ResourceSet)ResourceSets[culture];
			if(set!=null) {
				return(set);
			}

			if(MainAssembly != null) {
				/* Assembly resources */
				Stream stream;
				string filename=GetResourceFileName(culture);
				
				stream=MainAssembly.GetManifestResourceStream(filename);
				if (stream == null)
					stream = GetManifestResourceStreamNoCase (MainAssembly, filename);
				
				if(stream==null) {
					/* Try a satellite assembly */
					Version sat_version=GetSatelliteContractVersion(MainAssembly);
					Assembly a = null;
					try {
						a = MainAssembly.GetSatelliteAssembly (culture, sat_version);
						stream=a.GetManifestResourceStream(filename);
						if (stream == null)
							stream = GetManifestResourceStreamNoCase (a, filename);
					
					} catch (Exception) {} // Ignored
				}

				if(stream!=null && Createifnotexists==true) {
					object[] args=new Object[1];

					args[0]=stream;
					
					/* should we catch
					 * MissingMethodException, or
					 * just let someone else deal
					 * with it?
					 */
					set=(ResourceSet)Activator.CreateInstance(resourceSetType, args);
				} else if (culture == CultureInfo.InvariantCulture) {
					string msg = "Could not find any resource appropiate for the " +
						     "specified culture or its parents (assembly:{0})";
						     
					msg = String.Format (msg, MainAssembly != null ? MainAssembly.GetName ().Name : "");
							    
					throw new MissingManifestResourceException (msg);
				}
			} else if(resourceDir != null) {
				/* File resources */
				string filename=Path.Combine(resourceDir, this.GetResourceFileName(culture));
				if(File.Exists(filename) &&
				   Createifnotexists==true) {
					object[] args=new Object[1];

					args[0]=filename;
					
					/* should we catch
					 * MissingMethodException, or
					 * just let someone else deal
					 * with it?
					 */
					set=(ResourceSet)Activator.CreateInstance(resourceSetType, args);
				}
			}

			if(set==null && tryParents==true) {
				// avoid endless recursion
				if (!culture.Equals (neutral_culture) && !culture.Equals(CultureInfo.InvariantCulture))
					set = InternalGetResourceSet (culture.Parent, Createifnotexists, tryParents);
			}

			if(set!=null) {
				ResourceSets.Add(culture, set);
			}
			
			return(set);
		}
		   
		public virtual void ReleaseAllResources ()
		{
			lock(this) 
			{
				foreach (ResourceSet r in ResourceSets)
					r.Close();
				ResourceSets.Clear();
			}
		}

		protected static CultureInfo GetNeutralResourcesLanguage (Assembly a)
		{
			object[] attrs;

			attrs=a.GetCustomAttributes(typeof(NeutralResourcesLanguageAttribute), false);

			if(attrs.Length==0) {
				return(CultureInfo.InvariantCulture);
			} else {
				NeutralResourcesLanguageAttribute res_attr=(NeutralResourcesLanguageAttribute)attrs[0];
				
				return(new CultureInfo(res_attr.CultureName));
			}
		}

		protected static Version GetSatelliteContractVersion (Assembly a)
		{
			object[] attrs;
			
			attrs=a.GetCustomAttributes(typeof(SatelliteContractVersionAttribute), false);

			if(attrs.Length==0) {
				return(null);
			} else {
				SatelliteContractVersionAttribute sat_attr=(SatelliteContractVersionAttribute)attrs[0];

				/* Version(string) can throw
				 * ArgumentException if the version is
				 * invalid, but the spec for
				 * GetSatelliteContractVersion says we
				 * can throw the same exception for
				 * the same reason, so dont bother to
				 * catch it.
				 */
				return(new Version(sat_attr.Version));
			}
		}
	}
}
