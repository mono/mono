static class AssemblyRef
{
	// FIXME
	internal const string SystemConfiguration = "System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
	internal const string System = "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

	public const string EcmaPublicKey = "b77a5c561934e089";
#if MOBILE
	public const string FrameworkPublicKeyFull = "0024000004800000940000000602000000240000525341310004000001000100B5FC90E7027F67871E773A8FDE8938C81DD402BA65B9201D60593E96C492651E889CC13F1415EBB53FAC1131AE0BD333C5EE6021672D9718EA31A8AEBD0DA0072F25D87DBA6FC90FFD598ED4DA35E44C398C454307E8E33B8426143DAEC9F596836F97C8F74750E5975C64E2189F45DEF46B2A2B1247ADC3652BF5C308055DA9";
	public const string FrameworkPublicKeyFull2 = "00240000048000009400000006020000002400005253413100040000010001008D56C76F9E8649383049F383C44BE0EC204181822A6C31CF5EB7EF486944D032188EA1D3920763712CCB12D75FB77E9811149E6148E5D32FBAAB37611C1878DDC19E20EF135D0CB2CFF2BFEC3D115810C3D9069638FE4BE215DBF795861920E5AB6F7DB2E2CEEF136AC23D5DD2BF031700AEC232F6C6B1C785B4305C123B37AB";
#else
	public const string FrameworkPublicKeyFull = "00000000000000000400000000000000";
	public const string FrameworkPublicKeyFull2 = "00000000000000000400000000000000";
#endif
	public const string MicrosoftPublicKey = "b03f5f7f11d50a3a";
 
 	public const string MicrosoftJScript = Consts.AssemblyMicrosoft_JScript;
 	public const string MicrosoftVSDesigner = Consts.AssemblyMicrosoft_VSDesigner;
	public const string SystemData = Consts.AssemblySystem_Data;
	public const string SystemDesign = Consts.AssemblySystem_Design;
	public const string SystemDrawing = Consts.AssemblySystem_Drawing;
	public const string SystemWeb = Consts.AssemblySystem_Web;
	public const string SystemWebExtensions =  "System.Web.Extensions, Version=" + Consts.FxVersion + ", Culture=neutral, PublicKeyToken=31bf3856ad364e35";
	public const string SystemWindowsForms = Consts.AssemblySystem_Windows_Forms;
}