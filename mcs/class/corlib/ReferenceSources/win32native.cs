using System;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Microsoft.Win32
{
	static class Win32Native
	{
		internal const string ADVAPI32 = "advapi32.dll";
        internal const string KERNEL32 = "kernel32.dll";
        internal const string USER32   = "user32.dll";

		// Error codes from WinError.h
		internal const int ERROR_SUCCESS = 0x0;
		internal const int ERROR_INVALID_FUNCTION = 0x1;
		internal const int ERROR_FILE_NOT_FOUND = 0x2;
		internal const int ERROR_PATH_NOT_FOUND = 0x3;
		internal const int ERROR_ACCESS_DENIED  = 0x5;
		internal const int ERROR_INVALID_HANDLE = 0x6;
		internal const int ERROR_NOT_ENOUGH_MEMORY = 0x8;
		internal const int ERROR_INVALID_DATA = 0xd;
		internal const int ERROR_INVALID_DRIVE = 0xf;
		internal const int ERROR_NO_MORE_FILES = 0x12;
		internal const int ERROR_NOT_READY = 0x15;
		internal const int ERROR_BAD_LENGTH = 0x18;
		internal const int ERROR_SHARING_VIOLATION = 0x20;
		internal const int ERROR_NOT_SUPPORTED = 0x32;
		internal const int ERROR_FILE_EXISTS = 0x50;
		internal const int ERROR_INVALID_PARAMETER = 0x57;
		internal const int ERROR_BROKEN_PIPE = 0x6D;
		internal const int ERROR_CALL_NOT_IMPLEMENTED = 0x78;
		internal const int ERROR_INSUFFICIENT_BUFFER = 0x7A;
		internal const int ERROR_INVALID_NAME = 0x7B;
		internal const int ERROR_BAD_PATHNAME = 0xA1;
		internal const int ERROR_ALREADY_EXISTS = 0xB7;
		internal const int ERROR_ENVVAR_NOT_FOUND = 0xCB;
		internal const int ERROR_FILENAME_EXCED_RANGE = 0xCE;  // filename too long.
		internal const int ERROR_NO_DATA = 0xE8;
		internal const int ERROR_PIPE_NOT_CONNECTED = 0xE9;
		internal const int ERROR_MORE_DATA = 0xEA;
		internal const int ERROR_DIRECTORY = 0x10B;
		internal const int ERROR_OPERATION_ABORTED = 0x3E3;  // 995; For IO Cancellation
		internal const int ERROR_NOT_FOUND = 0x490;          // 1168; For IO Cancellation
		internal const int ERROR_NO_TOKEN = 0x3f0;
		internal const int ERROR_DLL_INIT_FAILED = 0x45A;
		internal const int ERROR_NON_ACCOUNT_SID = 0x4E9;
		internal const int ERROR_NOT_ALL_ASSIGNED = 0x514;
		internal const int ERROR_UNKNOWN_REVISION = 0x519;
		internal const int ERROR_INVALID_OWNER = 0x51B;
		internal const int ERROR_INVALID_PRIMARY_GROUP = 0x51C;
		internal const int ERROR_NO_SUCH_PRIVILEGE = 0x521;
		internal const int ERROR_PRIVILEGE_NOT_HELD = 0x522;
		internal const int ERROR_NONE_MAPPED = 0x534;
		internal const int ERROR_INVALID_ACL = 0x538;
		internal const int ERROR_INVALID_SID = 0x539;
		internal const int ERROR_INVALID_SECURITY_DESCR = 0x53A;
		internal const int ERROR_BAD_IMPERSONATION_LEVEL = 0x542;
		internal const int ERROR_CANT_OPEN_ANONYMOUS = 0x543;
		internal const int ERROR_NO_SECURITY_ON_OBJECT = 0x546;
		internal const int ERROR_TRUSTED_RELATIONSHIP_FAILURE = 0x6FD;

		internal const FileAttributes FILE_ATTRIBUTE_DIRECTORY = FileAttributes.Directory;

		public static string GetMessage (int hr)
		{
			return "Error " + hr;
		}

		public static int MakeHRFromErrorCode (int errorCode)
		{
			return unchecked(((int)0x80070000) | errorCode);
		}

		public class SECURITY_ATTRIBUTES
		{

		}

		// TimeZone
		internal const int TIME_ZONE_ID_INVALID = -1;
		internal const int TIME_ZONE_ID_UNKNOWN = 0;
		internal const int TIME_ZONE_ID_STANDARD = 1;
		internal const int TIME_ZONE_ID_DAYLIGHT = 2;
		internal const int MAX_PATH = 260;

		internal const int MUI_LANGUAGE_ID = 0x4;
		internal const int MUI_LANGUAGE_NAME = 0x8;
		internal const int MUI_PREFERRED_UI_LANGUAGES = 0x10;
		internal const int MUI_INSTALLED_LANGUAGES = 0x20;
		internal const int MUI_ALL_LANGUAGES = 0x40;
		internal const int MUI_LANG_NEUTRAL_PE_FILE = 0x100;
		internal const int MUI_NON_LANG_NEUTRAL_FILE = 0x200;

		internal const int LOAD_LIBRARY_AS_DATAFILE = 0x00000002;
		internal const int LOAD_STRING_MAX_LENGTH = 500;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct TimeZoneInformation {
			[MarshalAs(UnmanagedType.I4)]
			public Int32 Bias;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string StandardName;
			public Interop.mincore.SYSTEMTIME StandardDate;
			[MarshalAs(UnmanagedType.I4)]
			public Int32 StandardBias;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string DaylightName;
			public Interop.mincore.SYSTEMTIME DaylightDate;
			[MarshalAs(UnmanagedType.I4)]
			public Int32 DaylightBias;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct RegistryTimeZoneInformation {
			[MarshalAs(UnmanagedType.I4)]
			public Int32 Bias;
			[MarshalAs(UnmanagedType.I4)]
			public Int32 StandardBias;
			[MarshalAs(UnmanagedType.I4)]
			public Int32 DaylightBias;
			public Interop.mincore.SYSTEMTIME StandardDate;
			public Interop.mincore.SYSTEMTIME DaylightDate;

			public RegistryTimeZoneInformation(Byte[] bytes) {
				//
				// typedef struct _REG_TZI_FORMAT {
				// [00-03]	LONG Bias;
				// [04-07]	LONG StandardBias;
				// [08-11]	LONG DaylightBias;
				// [12-27]	SYSTEMTIME StandardDate;
				// [12-13]		WORD wYear;
				// [14-15]		WORD wMonth;
				// [16-17]		WORD wDayOfWeek;
				// [18-19]		WORD wDay;
				// [20-21]		WORD wHour;
				// [22-23]		WORD wMinute;
				// [24-25]		WORD wSecond;
				// [26-27]		WORD wMilliseconds;
				// [28-43]	SYSTEMTIME DaylightDate;
				// [28-29]		WORD wYear;
				// [30-31]		WORD wMonth;
				// [32-33]		WORD wDayOfWeek;
				// [34-35]		WORD wDay;
				// [36-37]		WORD wHour;
				// [38-39]		WORD wMinute;
				// [40-41]		WORD wSecond;
				// [42-43]		WORD wMilliseconds;
				// } REG_TZI_FORMAT;
				//
				if (bytes == null || bytes.Length != 44) {
					throw new ArgumentException(Environment.GetResourceString("Argument_InvalidREG_TZI_FORMAT"), "bytes");
				}
				Bias = BitConverter.ToInt32(bytes, 0);
				StandardBias = BitConverter.ToInt32(bytes, 4);
				DaylightBias = BitConverter.ToInt32(bytes, 8);

				StandardDate.wYear = BitConverter.ToUInt16(bytes, 12);
				StandardDate.wMonth = BitConverter.ToUInt16(bytes, 14);
				StandardDate.wDayOfWeek = BitConverter.ToUInt16(bytes, 16);
				StandardDate.wDay = BitConverter.ToUInt16(bytes, 18);
				StandardDate.wHour = BitConverter.ToUInt16(bytes, 20);
				StandardDate.wMinute = BitConverter.ToUInt16(bytes, 22);
				StandardDate.wSecond = BitConverter.ToUInt16(bytes, 24);
				StandardDate.wMilliseconds = BitConverter.ToUInt16(bytes, 26);

				DaylightDate.wYear = BitConverter.ToUInt16(bytes, 28);
				DaylightDate.wMonth = BitConverter.ToUInt16(bytes, 30);
				DaylightDate.wDayOfWeek = BitConverter.ToUInt16(bytes, 32);
				DaylightDate.wDay = BitConverter.ToUInt16(bytes, 34);
				DaylightDate.wHour = BitConverter.ToUInt16(bytes, 36);
				DaylightDate.wMinute = BitConverter.ToUInt16(bytes, 38);
				DaylightDate.wSecond = BitConverter.ToUInt16(bytes, 40);
				DaylightDate.wMilliseconds = BitConverter.ToUInt16(bytes, 42);
			}
		}

		internal class WIN32_FIND_DATA
		{
			internal int dwFileAttributes = 0;
			internal String cFileName = null;
		}


		[DllImport(KERNEL32, SetLastError = true)]
		[ResourceExposure(ResourceScope.Machine)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		internal static extern bool CloseHandle(IntPtr handle);
	}
}