//	
// System.Resources.ResourceManager.cs
//
// Author:
//	Duncan Mak (duncan@ximian.com)
//
// 2001 (C) Ximian, Inc. http://www.ximian.com
//

using System.Collections;
using System.Reflection;
using System.Globalization;

namespace System.Resources {
	   public class ResourceManager {
			 public static readonly int HeaderVersionNumber;
			 public static readonly int MagicNumber;

			 protected string BaseNameField;
			 protected Assembly MainAssembly;
			 protected Hashtable Resourcesets;
			 
			 private bool ignoreCase;
			 private Type resourceSetType;

			 // constructors
			 public ResourceManager () {}
			 public ResourceManager (Type resourceSource) {
				    if (resourceSource == null)
						  throw new ArgumentNullException ("resourceSource is null.");
				    resourceSetType = resourceSource; // TODO Incomplete
			 }
			 public ResourceManager (string baseName, Assembly assembly) {
				    if (baseName == null || assembly == null)
						  throw new ArgumentNullException ("The arguments are null.");
				    // TODO
			 }
			 public ResourceManager (string baseName, Assembly assembly, Type usingResourceSet) {
				    if (baseName == null || assembly == null)
						  throw new ArgumentNullException ("The arguments are null.");

				    if (usingResourceSet != null)
						  if (!usingResourceSet.isSubclassOf (Typeof (ResourceSet)))
							 throw new ArgumentException ("Type must be derived from ResourceSet.");
			 }

			 public static ResourceManager CreateFileBasedResourceManager (string baseName,
															   string resourceDir,
															   Type usingResourceSet) {}

			 public virtual string BaseName { get { return BaseNameField; }}

			 public virtual bool IgnoreCase {
				    get { return ignoreCase; }
				    set { ignoreCase = value; }
			 }

			 public virtual Type ResourceSetType { get { return resourceSetType; }}
			 
			 public virtual ResourceSet GetResourceSet (CultureInfo culture,
											    bool createIfNotExists,
											    bool tryParents) {
				    if (culture == null)
						  throw new ArgumentNullException ("CultureInfo is a null reference.");
			 }

			 public virtual string GetString (string name) {
				    if (name == null)
						  throw new ArgumentNullException ("Name is null.");
				    if (ResourceSets.Contains (name)) {
						  if (!ResourceSets[name] is string)
								throw new InvalidOperationException ("The resource is not a string.");
						  return ResourceSets[name].ToString();
				    }
				    // TODO check for correctness.				    
			 }
			 
			 public virtual string GetString (string name, CultureInfo culture) {
				    if (name == null)
						  throw new ArgumentNullException ("Name is null.");
				return name;
			 }

			 protected virtual string GetResourceFileName (CultureInfo culture) {
				    return new culture.Name + ".resources"; // TODO check for correctness.
			 }

			 protected virtual ResourceSet InternalGetResourceSet (CultureInfo culture,
														bool Createifnotexists,
														bool tryParents) {}
						  
			 public virtual void ReleaseAllResources () {
				    foreach (ResourceSet r in ResourceSets)
						  r.Close();
			 }

			 protected static CultureInfo GetNeutralResourcesLanguage (Assembly a) {
				    foreach (Attribute attribute in a.GetCustomAttributes ()) {
						  if (attribute is NeutralResourcesLanguageAttribute)
								return new Cultureinfo (attribute.CultureName);
				    }
				    return null;
			 }

			 public static Version GetSatelliteContractVersion (Assembly a) {

				    foreach (Attribute attribute in a.GetCustomAttributes ()) {
						  if (attribute is SatelliteContractVersionAttribute)
								return new Version (attribute.Version);
				    }
				    return null;
			 }
	   }
}
