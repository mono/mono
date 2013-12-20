class XBuildConsts
{
#if XBUILD_12
	public const string Version = "12.0";
	public const string AssemblyVersion = "12.0.0.0";
	public const string FileVersion     = "12.0.21005.1";
#elif NET_4_0
	public const string Version = "4.0";
	public const string AssemblyVersion = Consts.FxVersion;
	public const string FileVersion     = Consts.FxFileVersion;
#elif NET_3_5
	public const string Version = "3.5";
	public const string AssemblyVersion = Consts.FxVersion;
	public const string FileVersion     = Consts.FxFileVersion;
#else
	public const string Version = "2.0";
	public const string AssemblyVersion = Consts.FxVersion;
	public const string FileVersion     = Consts.FxFileVersion;
#endif
}
