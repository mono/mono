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
			 protected Hashtable ResourceSets;
			 
			 private bool ignoreCase;
			 private Type resourceSetType;

			 // constructors
			 public ResourceManager () {}

		         [MonoTODO]
			 public ResourceManager (Type resourceSource) {
				    if (resourceSource == null)
						  throw new ArgumentNullException ("resourceSource is null.");
				    resourceSetType = resourceSource; // TODO Incomplete
			 }

		         [MonoTODO]
			 public ResourceManager (string baseName, Assembly assembly) {
				    if (baseName == null || assembly == null)
						  throw new ArgumentNullException ("The arguments are null.");
				    // TODO
			 }
			 public ResourceManager (string baseName, Assembly assembly, Type usingResourceSet) {
				    if (baseName == null || assembly == null)
						  throw new ArgumentNullException ("The arguments are null.");

				    if (usingResourceSet != null)
						  if (!usingResourceSet.IsSubclassOf (typeof(ResourceSet)))
							 throw new ArgumentException ("Type must be derived from ResourceSet.");
			 }

			 [MonoTODO]
			 public static ResourceManager CreateFileBasedResourceManager (string baseName,
															   string resourceDir,
															   Type usingResourceSet) {
				return null;
			 }

			 public virtual string BaseName { get { return BaseNameField; }}

			 public virtual bool IgnoreCase {
				    get { return ignoreCase; }
				    set { ignoreCase = value; }
			 }

			 public virtual Type ResourceSetType {
					get { return resourceSetType; }
			 }
			 
			 [MonoTODO]
			 public virtual ResourceSet GetResourceSet (CultureInfo culture,
											    bool createIfNotExists,
											    bool tryParents) {
				    if (culture == null)
						  throw new ArgumentNullException ("CultureInfo is a null reference.");
				return null;
			 }

		         [MonoTODO]
			 public virtual string GetString (string name) {
				    if (name == null)
						  throw new ArgumentNullException ("Name is null.");
				    if (ResourceSets.Contains (name)) {
						  if (!(ResourceSets[name] is string))
								throw new InvalidOperationException ("The resource is " +
												     "not a string.");
						  return ResourceSets[name].ToString();
				    }
				    // TODO check for correctness.				    
				    return null;	
			 }
			 
			 public virtual string GetString (string name, CultureInfo culture) {
				    if (name == null)
						  throw new ArgumentNullException ("Name is null.");
				return name;
			 }

		         [MonoTODO]
			 protected virtual string GetResourceFileName (CultureInfo culture) {
				    return culture.Name + ".resources"; // TODO check for correctness.
			 }

			 protected virtual ResourceSet InternalGetResourceSet (CultureInfo culture,
									       bool Createifnotexists,
									       bool tryParents) {
				    return null;
			 }
		   
			 public virtual void ReleaseAllResources () {
				    foreach (ResourceSet r in ResourceSets)
						  r.Close();
			 }

			 [MonoTODO]
			 protected static CultureInfo GetNeutralResourcesLanguage (Assembly a) {
				    foreach (Attribute attribute in a.GetCustomAttributes (false)) {
						  if (attribute is NeutralResourcesLanguageAttribute)
								// ???
								// return new CultureInfo (attribute.CultureName);
								// TODO: Line containing "new CultureInfo" commented out
								// because the CultureInfo(string) constructor has not
								// yet been defined
								// return new CultureInfo (attribute.ToString());
								return null;
				    }
				    return null;
			 }

			 [MonoTODO]
			 public static Version GetSatelliteContractVersion (Assembly a) {

				    foreach (Attribute attribute in a.GetCustomAttributes (false)) {
						  if (attribute is SatelliteContractVersionAttribute)
								// TODO: ??? "Attribute" does not have a property "Version"
								// Do you mean "attribute.ToString()"?
								// return new Version (attribute.Version);
								return new Version (attribute.ToString());
				    }
				    return null;
			 }
	   }
}
