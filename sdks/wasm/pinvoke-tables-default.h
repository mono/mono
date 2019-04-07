typedef struct {
const char *name;
void *func;
} PinvokeImport;

void SystemNative_ConvertErrorPlatformToPal ();
void SystemNative_ConvertErrorPalToPlatform ();
void SystemNative_StrErrorR ();
void SystemNative_GetNonCryptographicallySecureRandomBytes ();
void SystemNative_OpenDir ();
void SystemNative_GetReadDirRBufferSize ();
void SystemNative_ReadDirR ();
void SystemNative_CloseDir ();
void SystemNative_FStat2 ();
void SystemNative_Stat2 ();
void SystemNative_LStat2 ();
void SystemNative_ChMod ();
void SystemNative_CopyFile ();
void SystemNative_GetEGid ();
void SystemNative_GetEUid ();
void SystemNative_Link ();
void SystemNative_MkDir ();
void SystemNative_Rename ();
void SystemNative_RmDir ();
void SystemNative_Stat2 ();
void SystemNative_LStat2 ();
void SystemNative_UTime ();
void SystemNative_UTimes ();
void SystemNative_Unlink ();
static PinvokeImport System_Native_imports [] = {
{"SystemNative_ConvertErrorPlatformToPal", SystemNative_ConvertErrorPlatformToPal},
{"SystemNative_ConvertErrorPalToPlatform", SystemNative_ConvertErrorPalToPlatform},
{"SystemNative_StrErrorR", SystemNative_StrErrorR},
{"SystemNative_GetNonCryptographicallySecureRandomBytes", SystemNative_GetNonCryptographicallySecureRandomBytes},
{"SystemNative_OpenDir", SystemNative_OpenDir},
{"SystemNative_GetReadDirRBufferSize", SystemNative_GetReadDirRBufferSize},
{"SystemNative_ReadDirR", SystemNative_ReadDirR},
{"SystemNative_CloseDir", SystemNative_CloseDir},
{"SystemNative_FStat2", SystemNative_FStat2},
{"SystemNative_Stat2", SystemNative_Stat2},
{"SystemNative_LStat2", SystemNative_LStat2},
{"SystemNative_ChMod", SystemNative_ChMod},
{"SystemNative_CopyFile", SystemNative_CopyFile},
{"SystemNative_GetEGid", SystemNative_GetEGid},
{"SystemNative_GetEUid", SystemNative_GetEUid},
{"SystemNative_Link", SystemNative_Link},
{"SystemNative_MkDir", SystemNative_MkDir},
{"SystemNative_Rename", SystemNative_Rename},
{"SystemNative_RmDir", SystemNative_RmDir},
{"SystemNative_Stat2", SystemNative_Stat2},
{"SystemNative_LStat2", SystemNative_LStat2},
{"SystemNative_UTime", SystemNative_UTime},
{"SystemNative_UTimes", SystemNative_UTimes},
{"SystemNative_Unlink", SystemNative_Unlink},
{NULL, NULL}
};
static void *pinvoke_tables[] = { System_Native_imports,};
static char *pinvoke_names[] = { "System.Native",};
