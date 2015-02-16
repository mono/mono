static class XBuildConsts
{
#if XBUILD_14
	public const string Version = "14.0";
	public const string AssemblyVersion = "14.0.0.0";
	public const string FileVersion     = "14.0.22310.1";
#elif XBUILD_12
	public const string Version = "12.0";
	public const string AssemblyVersion = "12.0.0.0";
	public const string FileVersion     = "12.0.21005.1";
#else
	public const string Version = "4.0";
	public const string AssemblyVersion = Consts.FxVersion;
	public const string FileVersion     = Consts.FxFileVersion;
#endif
}
