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

namespace System.Resources
{
	[Serializable]
	public class ResourceManager
	{
		public static readonly int HeaderVersionNumber;
		// public static readonly int MagicNumber = 0xBEEFCACE;

		protected string BaseNameField;
		protected Assembly MainAssembly;
		protected Hashtable ResourceSets;
		
		private bool ignoreCase;
		private Type resourceSetType;
		
		// constructors
		protected ResourceManager () {}
		
		public ResourceManager (Type resourceSource)
		{
			if (resourceSource == null)
				throw new ArgumentNullException ("resourceSource is null.");
			
			BaseNameField = resourceSource.FullName;
			MainAssembly = resourceSource.Assembly;
			
			ignoreCase = false;
			resourceSetType = resourceSource;
		}
		
		public ResourceManager (string baseName, Assembly assembly)
		{
			if (baseName == null || assembly == null)
				throw new ArgumentNullException ("The arguments are null.");
			
			BaseNameField = baseName;
			MainAssembly = assembly;
			ignoreCase = false;
			resourceSetType = typeof (ResourceSet);
		}
			 
		public ResourceManager (string baseName, Assembly assembly, Type usingResourceSet)
		{
			if (baseName == null || assembly == null)
				throw new ArgumentNullException ("The arguments are null.");
			
			BaseNameField = baseName;
			MainAssembly = assembly;
			
			if (usingResourceSet == null) // defaults resourceSet type.
				resourceSetType = typeof (ResourceSet);
			else {
				if (!usingResourceSet.IsSubclassOf (typeof (ResourceSet)))
					throw new ArgumentException ("Type must be from ResourceSet.");
				
			}
		}
		
		[MonoTODO]
		public static ResourceManager CreateFileBasedResourceManager (string baseName,
						      string resourceDir, Type usingResourceSet)
		{
			return null;
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
			 
		[MonoTODO]
		public virtual ResourceSet GetResourceSet (CultureInfo culture,
					   bool createIfNotExists, bool tryParents)
			
		{
			if (culture == null)
				throw new ArgumentNullException ("CultureInfo is a null reference.");
			return null;
		}
		
		[MonoTODO]
		public virtual string GetString (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("Name is null.");
			if (ResourceSets.Contains (name)) {
				if (!(ResourceSets[name] is string))
					throw new InvalidOperationException ("The resource is " +
									     "not a string.");
				return ResourceSets[name].ToString();
			}
			return null;	
		}

		[MonoTODO]
		public virtual string GetString (string name, CultureInfo culture)
		{
				 if (name == null)
					 throw new ArgumentNullException ("Name is null.");
				 return null;
		}

		protected virtual string GetResourceFileName (CultureInfo culture)
		{
			return culture.Name + ".resources";
		}

		[MonoTODO]
		protected virtual ResourceSet InternalGetResourceSet (CultureInfo culture,
						   bool Createifnotexists, bool tryParents)
			 {
				 return null;
			 }
		   
		public virtual void ReleaseAllResources ()
		{
			foreach (ResourceSet r in ResourceSets)
				r.Close();
		}

		protected static CultureInfo GetNeutralResourcesLanguage (Assembly a)
		{
			foreach (Attribute attribute in a.GetCustomAttributes (false)) {
				if (attribute is NeutralResourcesLanguageAttribute)
					return new CultureInfo ((attribute as NeutralResourcesLanguageAttribute).CultureName);
			}
			return null;
		}

		protected static Version GetSatelliteContractVersion (Assembly a)
		{
			foreach (Attribute attribute in a.GetCustomAttributes (false)) {
				if (attribute is SatelliteContractVersionAttribute)
					return new Version ((attribute as SatelliteContractVersionAttribute).Version);
			}
			return null; // return null if no version was found.
		}
	}
}
