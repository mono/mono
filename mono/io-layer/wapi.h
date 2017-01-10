/*
 * wapi.h:  Public include files
 *
 * Author:
 *	Dick Porter (dick@ximian.com)
 *
 * (C) 2002 Ximian, Inc.
 */

#ifndef _WAPI_WAPI_H_
#define _WAPI_WAPI_H_

#include <config.h>
#include <glib.h>

#ifdef HAVE_DIRENT_H
#include <dirent.h>
#endif
#include <unistd.h>
#include <utime.h>
#include <sys/types.h>
#include <sys/stat.h>

#include <mono/io-layer/wapi-remap.h>
#include <mono/io-layer/error.h>
#include <mono/io-layer/wapi_glob.h>
#include <mono/utils/mono-logger-internals.h>

G_BEGIN_DECLS

#define WAIT_FAILED        ((gint) 0xFFFFFFFF)
#define WAIT_OBJECT_0      ((gint) 0x00000000)
#define WAIT_ABANDONED_0   ((gint) 0x00000080)
#define WAIT_TIMEOUT       ((gint) 0x00000102)
#define WAIT_IO_COMPLETION ((gint) 0x000000C0)

#define GENERIC_READ    0x80000000
#define GENERIC_WRITE   0x40000000
#define GENERIC_EXECUTE 0x20000000
#define GENERIC_ALL     0x10000000

#define FILE_SHARE_READ   0x00000001
#define FILE_SHARE_WRITE  0x00000002
#define FILE_SHARE_DELETE 0x00000004

#define CREATE_NEW        1
#define CREATE_ALWAYS     2
#define OPEN_EXISTING     3
#define OPEN_ALWAYS       4
#define TRUNCATE_EXISTING 5

#define FILE_ATTRIBUTE_READONLY            0x00000001
#define FILE_ATTRIBUTE_HIDDEN              0x00000002
#define FILE_ATTRIBUTE_SYSTEM              0x00000004
#define FILE_ATTRIBUTE_DIRECTORY           0x00000010
#define FILE_ATTRIBUTE_ARCHIVE             0x00000020
#define FILE_ATTRIBUTE_ENCRYPTED           0x00000040
#define FILE_ATTRIBUTE_NORMAL              0x00000080
#define FILE_ATTRIBUTE_TEMPORARY           0x00000100
#define FILE_ATTRIBUTE_SPARSE_FILE         0x00000200
#define FILE_ATTRIBUTE_REPARSE_POINT       0x00000400
#define FILE_ATTRIBUTE_COMPRESSED          0x00000800
#define FILE_ATTRIBUTE_OFFLINE             0x00001000
#define FILE_ATTRIBUTE_NOT_CONTENT_INDEXED 0x00002000
#define FILE_FLAG_OPEN_NO_RECALL           0x00100000
#define FILE_FLAG_OPEN_REPARSE_POINT       0x00200000
#define FILE_FLAG_POSIX_SEMANTICS          0x01000000
#define FILE_FLAG_BACKUP_SEMANTICS         0x02000000
#define FILE_FLAG_DELETE_ON_CLOSE          0x04000000
#define FILE_FLAG_SEQUENTIAL_SCAN          0x08000000
#define FILE_FLAG_RANDOM_ACCESS            0x10000000
#define FILE_FLAG_NO_BUFFERING             0x20000000
#define FILE_FLAG_OVERLAPPED               0x40000000
#define FILE_FLAG_WRITE_THROUGH            0x80000000

#define REPLACEFILE_WRITE_THROUGH       0x00000001
#define REPLACEFILE_IGNORE_MERGE_ERRORS 0x00000002

#define MAX_PATH 260

#define INVALID_SET_FILE_POINTER ((guint32) 0xFFFFFFFF)
#define INVALID_FILE_SIZE        ((guint32) 0xFFFFFFFF)
#define INVALID_FILE_ATTRIBUTES  ((guint32) 0xFFFFFFFF)

#ifdef DISABLE_IO_LAYER_TRACE
#define MONO_TRACE(...)
#else
#define MONO_TRACE(...) mono_trace (__VA_ARGS__)
#endif

#define WINAPI

typedef guint32 DWORD;
typedef gboolean BOOL;
typedef gint32 LONG;
typedef guint32 ULONG;
typedef guint UINT;

typedef gpointer HANDLE;
typedef gpointer HMODULE;

typedef struct {
	guint32 nLength;
	gpointer lpSecurityDescriptor;
	gboolean bInheritHandle;
} SECURITY_ATTRIBUTES;

typedef struct {
	guint32 Internal;
	guint32 InternalHigh;
	guint32 Offset;
	guint32 OffsetHigh;
	gpointer hEvent;
	gpointer handle1;
	gpointer handle2;
} OVERLAPPED;

typedef enum {
	STD_INPUT_HANDLE  = -10,
	STD_OUTPUT_HANDLE = -11,
	STD_ERROR_HANDLE  = -12,
} WapiStdHandle;

typedef enum {
	FILE_BEGIN   = 0,
	FILE_CURRENT = 1,
	FILE_END     = 2,
} WapiSeekMethod;

typedef enum {
	FILE_TYPE_UNKNOWN = 0x0000,
	FILE_TYPE_DISK    = 0x0001,
	FILE_TYPE_CHAR    = 0x0002,
	FILE_TYPE_PIPE    = 0x0003,
	FILE_TYPE_REMOTE  = 0x8000,
} WapiFileType;

typedef enum {
	DRIVE_UNKNOWN     = 0,
	DRIVE_NO_ROOT_DIR = 1,
	DRIVE_REMOVABLE   = 2,
	DRIVE_FIXED       = 3,
	DRIVE_REMOTE      = 4,
	DRIVE_CDROM       = 5,
	DRIVE_RAMDISK     = 6,
} WapiDriveType;

typedef enum {
	GetFileExInfoStandard = 0x0000,
	GetFileExMaxInfoLevel = 0x0001,
} GET_FILEEX_INFO_LEVELS;

typedef struct {
	guint16 wYear;
	guint16 wMonth;
	guint16 wDayOfWeek;
	guint16 wDay;
	guint16 wHour;
	guint16 wMinute;
	guint16 wSecond;
	guint16 wMilliseconds;
} SYSTEMTIME;

typedef struct {
#if G_BYTE_ORDER == G_BIG_ENDIAN
	guint32 dwHighDateTime;
	guint32 dwLowDateTime;
#else
	guint32 dwLowDateTime;
	guint32 dwHighDateTime;
#endif
} FILETIME;

typedef struct {
	guint32 dwFileAttributes;
	FILETIME ftCreationTime;
	FILETIME ftLastAccessTime;
	FILETIME ftLastWriteTime;
	guint32 nFileSizeHigh;
	guint32 nFileSizeLow;
	guint32 dwReserved0;
	guint32 dwReserved1;
	gunichar2 cFileName [MAX_PATH];
	gunichar2 cAlternateFileName [14];
} WIN32_FIND_DATA;

typedef struct {
	guint32 dwFileAttributes;
	FILETIME ftCreationTime;
	FILETIME ftLastAccessTime;
	FILETIME ftLastWriteTime;
	guint32 nFileSizeHigh;
	guint32 nFileSizeLow;
} WIN32_FILE_ATTRIBUTE_DATA;

typedef union {
	struct {
		guint32 LowPart;
		guint32 HighPart;
	} u;
	guint64 QuadPart;
} ULARGE_INTEGER;

typedef struct {
#ifdef WAPI_FILE_SHARE_PLATFORM_EXTRA_DATA
	WAPI_FILE_SHARE_PLATFORM_EXTRA_DATA
#endif
	guint64 device;
	guint64 inode;
	pid_t opened_by_pid;
	guint32 sharemode;
	guint32 access;
	guint32 handle_refs;
	guint32 timestamp;
} _WapiFileShare;

/* Currently used for both FILE, CONSOLE and PIPE handle types.
 * This may have to change in future. */
typedef struct {
	gchar *filename;
	_WapiFileShare *share_info;	/* Pointer into shared mem */
	gint fd;
	guint32 security_attributes;
	guint32 fileaccess;
	guint32 sharemode;
	guint32 attrs;
} _WapiHandle_file;

typedef struct {
	gchar **namelist;
	gchar *dir_part;
	gint num;
	gsize count;
} _WapiHandle_find;

void
wapi_init (void);

void
wapi_cleanup (void);

gboolean
CloseHandle (gpointer handle);

pid_t
wapi_getpid (void);

gpointer
CreateFile(const gunichar2 *name, guint32 fileaccess, guint32 sharemode, SECURITY_ATTRIBUTES *security, guint32 createmode, guint32 attrs, gpointer tmplate);

gboolean
DeleteFile (const gunichar2 *name);

gpointer
GetStdHandle (WapiStdHandle stdhandle);

gboolean
ReadFile (gpointer handle, gpointer buffer, guint32 numbytes, guint32 *bytesread, OVERLAPPED *overlapped);

gboolean
WriteFile (gpointer handle, gconstpointer buffer, guint32 numbytes, guint32 *byteswritten, OVERLAPPED *overlapped);

gboolean
FlushFileBuffers (gpointer handle);

gboolean
SetEndOfFile (gpointer handle);

guint32
SetFilePointer (gpointer handle, gint32 movedistance, gint32 *highmovedistance, guint32 method);

WapiFileType
GetFileType (gpointer handle);

guint32
GetFileSize (gpointer handle, guint32 *highsize);

gboolean
GetFileTime (gpointer handle, FILETIME *create_time, FILETIME *last_access, FILETIME *last_write);

gboolean
SetFileTime (gpointer handle, const FILETIME *create_time, const FILETIME *last_access, const FILETIME *last_write);

gboolean
FileTimeToSystemTime (const FILETIME *file_time, SYSTEMTIME *system_time);

gpointer
FindFirstFile (const gunichar2 *pattern, WIN32_FIND_DATA *find_data);

gboolean
FindNextFile (gpointer handle, WIN32_FIND_DATA *find_data);

gboolean
FindClose (gpointer handle);

gboolean
CreateDirectory (const gunichar2 *name, SECURITY_ATTRIBUTES *security);

gboolean
RemoveDirectory (const gunichar2 *name);

gboolean
MoveFile (const gunichar2 *name, const gunichar2 *dest_name);

gboolean
CopyFile (const gunichar2 *name, const gunichar2 *dest_name, gboolean fail_if_exists);

gboolean
ReplaceFile (const gunichar2 *replacedFileName, const gunichar2 *replacementFileName, const gunichar2 *backupFileName, guint32 replaceFlags, gpointer exclude, gpointer reserved);

guint32
GetFileAttributes (const gunichar2 *name);

gboolean
GetFileAttributesEx (const gunichar2 *name, GET_FILEEX_INFO_LEVELS level, gpointer info);

gboolean
SetFileAttributes (const gunichar2 *name, guint32 attrs);

guint32
GetCurrentDirectory (guint32 length, gunichar2 *buffer);

gboolean
SetCurrentDirectory (const gunichar2 *path);

gboolean
CreatePipe (gpointer *readpipe, gpointer *writepipe, SECURITY_ATTRIBUTES *security, guint32 size);

gint32
GetLogicalDriveStrings (guint32 len, gunichar2 *buf);

gboolean
GetDiskFreeSpaceEx (const gunichar2 *path_name, ULARGE_INTEGER *free_bytes_avail, ULARGE_INTEGER *total_number_of_bytes, ULARGE_INTEGER *total_number_of_free_bytes);

guint32
GetDriveType (const gunichar2 *root_path_name);

gboolean
LockFile (gpointer handle, guint32 offset_low, guint32 offset_high, guint32 length_low, guint32 length_high);

gboolean
UnlockFile (gpointer handle, guint32 offset_low, guint32 offset_high, guint32 length_low, guint32 length_high);

gboolean
GetVolumeInformation (const gunichar2 *path, gunichar2 *volumename, gint volumesize, gint *outserial, gint *maxcomp, gint *fsflags, gunichar2 *fsbuffer, gint fsbuffersize);

void
_wapi_io_init (void);

void
_wapi_io_cleanup (void);

gpointer
_wapi_stdhandle_create (gint fd, const gchar *name);

G_END_DECLS

#endif /* _WAPI_WAPI_H_ */
