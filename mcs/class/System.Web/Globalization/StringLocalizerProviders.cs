namespace System.Web.Globalization
{
  public static class StringLocalizerProviders
  {
    private static IStringLocalizerProvider _dataAnnotationStringLocalizerProvider;
    private static bool _setStringLocalizerProvider;

    public static IStringLocalizerProvider DataAnnotationStringLocalizerProvider
    {
      get
      {
        if (StringLocalizerProviders._dataAnnotationStringLocalizerProvider == null && !StringLocalizerProviders._setStringLocalizerProvider)
          StringLocalizerProviders._dataAnnotationStringLocalizerProvider = (IStringLocalizerProvider) new ResourceFileStringLocalizerProvider();
        return StringLocalizerProviders._dataAnnotationStringLocalizerProvider;
      }
      set
      {
        StringLocalizerProviders._dataAnnotationStringLocalizerProvider = value;
        StringLocalizerProviders._setStringLocalizerProvider = true;
      }
    }
  }
}