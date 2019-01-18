using System.Reflection;
using System.Globalization;

namespace System.Resources
{
	public class ResourceManager
	{
		public static readonly int HeaderVersionNumber;
		public static readonly int MagicNumber;
		protected System.Reflection.Assembly MainAssembly;
		protected ResourceManager() { }
		public ResourceManager(string baseName, System.Reflection.Assembly assembly) { }
		public ResourceManager(string baseName, System.Reflection.Assembly assembly, System.Type usingResourceSet) { }
		public ResourceManager(System.Type resourceSource) { }
		public virtual string BaseName { get { throw null; } }
		public virtual bool IgnoreCase { get { throw null; } set { } }
		public virtual System.Type ResourceSetType { get { throw null; } }
		protected static System.Globalization.CultureInfo GetNeutralResourcesLanguage(System.Reflection.Assembly a) { throw null; }
		public virtual object GetObject(string name) { throw null; }
		public virtual object GetObject(string name, System.Globalization.CultureInfo culture) { throw null; }
		protected virtual string GetResourceFileName(System.Globalization.CultureInfo culture) { throw null; }
		public virtual ResourceSet GetResourceSet(System.Globalization.CultureInfo culture, bool createIfNotExists, bool tryParents) { throw null; }
		protected static System.Version GetSatelliteContractVersion(System.Reflection.Assembly a) { throw null; }
		public System.IO.UnmanagedMemoryStream GetStream(string name) { throw null; }
		public System.IO.UnmanagedMemoryStream GetStream(string name, System.Globalization.CultureInfo culture) { throw null; }
		public virtual string GetString(string name) { throw null; }
		public virtual string GetString(string name, System.Globalization.CultureInfo culture) { throw null; }
		protected virtual ResourceSet InternalGetResourceSet(System.Globalization.CultureInfo culture, bool createIfNotExists, bool tryParents) { throw null; }
		public virtual void ReleaseAllResources() { }
		protected UltimateResourceFallbackLocation FallbackLocation { get { throw null; } set { } }
		public static ResourceManager CreateFileBasedResourceManager(string baseName, string resourceDir, System.Type usingResourceSet) { throw null; }


		internal static bool IsDefaultType(string asmTypeName, string typeName) { throw null; }
		internal const string ResReaderTypeName = "System.Resources.ResourceReader";
	}
}