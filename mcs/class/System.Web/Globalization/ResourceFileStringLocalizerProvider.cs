using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Security.Permissions;
using System.Web.Compilation;

namespace System.Web.Globalization
{
  public sealed class ResourceFileStringLocalizerProvider : IStringLocalizerProvider
  {
    private readonly ConcurrentDictionary<string, object> _missingManifestCache = new ConcurrentDictionary<string, object>();
    private ResourceManager _resourceManager;
    private bool _loadedResourceAssembly;
    public const string ResourceFileName = "DataAnnotation.Localization";

    public string GetLocalizedString(CultureInfo culture, string name, params object[] arguments)
    {
      if (culture == null)
        throw new ArgumentNullException(nameof (culture));
      if (string.IsNullOrEmpty(name))
        return name;
      string stringSafely = this.GetStringSafely(name, culture);
      if (stringSafely != null)
        return string.Format(stringSafely, arguments);
      return stringSafely;
    }

    private string GetStringSafely(string name, CultureInfo culture)
    {
      if (culture == null)
        throw new ArgumentNullException(nameof (culture));
      this.EnsureResourceManager();
      string str = (string) null;
      if (this._resourceManager == null)
        return str;
      string key = string.Format("n={0}&c={1}", (object) name, (object) culture.Name);
      if (this._missingManifestCache.ContainsKey(key))
        return str;
      try
      {
        str = (string) this._resourceManager.GetObject(name, culture);
      }
      catch (Exception ex)
      {
        this._missingManifestCache.TryAdd(key, (object) null);
      }
      return str;
    }

    private ResourceManager EnsureResourceManager()
    {
      if (this._loadedResourceAssembly)
        return this._resourceManager;
      Assembly resourceAssembly = this.GetLocalResourceAssembly();
      if (resourceAssembly != (Assembly) null)
      {
        this._resourceManager = new ResourceManager("DataAnnotation.Localization", resourceAssembly);
        this._resourceManager.IgnoreCase = true;
      }
      this._loadedResourceAssembly = true;
      return this._resourceManager;
    }

    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    private Assembly GetLocalResourceAssembly()
    {
      BuildResult buildResultFromCache = BuildManager.GetBuildResultFromCache(BuildManager.GetLocalResourcesAssemblyName(VirtualPath.Create(HttpRuntime.AppDomainAppVirtualPath)));
      if (buildResultFromCache != null)
        return ((BuildResultCompiledAssemblyBase) buildResultFromCache).ResultAssembly;
      return (Assembly) null;
    }
  }
}
