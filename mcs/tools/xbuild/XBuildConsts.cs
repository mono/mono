class XBuildConsts
{
#if XBUILD_12
	public const string AssemblyVersion = "12.0.0.0";
	public const string FileVersion     = "12.0.21005.1";
#else
	public const string AssemblyVersion = Consts.FxVersion;
	public const string FileVersion     = Consts.FxFileVersion;
#endif
}
