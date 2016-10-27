/*
 * processes.h:  Process handles
 *
 * Author:
 *	Dick Porter (dick@ximian.com)
 *
 * (C) 2002 Ximian, Inc.
 */

#ifndef _WAPI_PROCESSES_H_
#define _WAPI_PROCESSES_H_

#include <sys/types.h>
#ifdef HAVE_UNISTD_H
#include <unistd.h>
#endif
#include <glib.h>

#include <mono/io-layer/access.h>
#include <mono/io-layer/versioninfo.h>
#include <mono/utils/mono-os-semaphore.h>

G_BEGIN_DECLS

typedef enum {
	STARTF_USESHOWWINDOW=0x001,
	STARTF_USESIZE=0x002,
	STARTF_USEPOSITION=0x004,
	STARTF_USECOUNTCHARS=0x008,
	STARTF_USEFILLATTRIBUTE=0x010,
	STARTF_RUNFULLSCREEN=0x020,
	STARTF_FORCEONFEEDBACK=0x040,
	STARTF_FORCEOFFFEEDBACK=0x080,
	STARTF_USESTDHANDLES=0x100
} WapiStartupFlags;


typedef struct _WapiStartupInfo WapiStartupInfo;

struct _WapiStartupInfo 
{
	guint32 cb;
	guchar *lpReserved;
	guchar *lpDesktop;
	guchar *lpTitle;
	guint32 dwX;
	guint32 dwY;
	guint32 dwXSize;
	guint32 dwYSize;
	guint32 dwXCountChars;
	guint32 dwYCountChars;
	guint32 dwFillAttribute;
	WapiStartupFlags dwFlags;
	guint16 wShowWindow;
	guint16 cbReserved2;
	guint8 *lpReserved2;
	gpointer hStdInput;
	gpointer hStdOutput;
	gpointer hStdError;
};

typedef struct _WapiProcessInformation WapiProcessInformation;

struct _WapiProcessInformation 
{
	gpointer hProcess;
	gpointer hThread;
	guint32 dwProcessId;
	guint32 dwThreadId;
};

typedef enum {
	SEE_MASK_CLASSNAME	= 0x01,
	SEE_MASK_CLASSKEY	= 0x03,
	SEE_MASK_IDLIST		= 0x04,
	SEE_MASK_INVOKEIDLIST	= 0x0c,
	SEE_MASK_ICON		= 0x10,
	SEE_MASK_HOTKEY		= 0x20,
	SEE_MASK_NOCLOSEPROCESS	= 0x40,
	SEE_MASK_CONNECTNETDRV	= 0x80,
	SEE_MASK_FLAG_DDEWAIT	= 0x100,
	SEE_MASK_DOENVSUBST	= 0x200,
	SEE_MASK_FLAG_NO_UI	= 0x400,
	SEE_MASK_NO_CONSOLE	= 0x8000,
	SEE_MASK_UNICODE	= 0x10000,
	SEE_MASK_HMONITOR	= 0x200000,
	/*SEE_MASK_FLAG_LOG_USAGE,*/
	/*SEE_MASK_NOZONECHECKS,*/
} WapiShellExecuteInfoFlags;

typedef enum {
	SW_HIDE = 0,
	SW_SHOWNORMAL = 1,
	SW_SHOWMINIMIZED = 2,
	SW_MAXIMIZE = 3,
	SW_SHOWMAXIMIZED = 3,
	SW_SHOWNOACTIVATE = 4,
	SW_SHOW = 5,
	SW_MINIMIZE = 6,
	SW_SHOWMINNOACTIVE = 7,
	SW_SHOWNA = 8,
	SW_RESTORE = 9,
	SW_SHOWDEFAULT = 10,
} WapiShellExecuteShowFlags;

typedef struct _WapiShellExecuteInfo WapiShellExecuteInfo;

struct _WapiShellExecuteInfo
{
	guint32 cbSize;
	gulong fMask;
	gpointer hwnd;
	const gunichar2 *lpVerb;
	const gunichar2 *lpFile;
	const gunichar2 *lpParameters;
	const gunichar2 *lpDirectory;
	gulong nShow;
	gpointer hInstApp;
	gpointer lpIDList;
	const gunichar2 *lpClass;
	gpointer hkeyClass;
	guint32 dwHotKey;
	union 
	{
		gpointer hIcon;
		gpointer hMonitor;
	} u;
	gpointer hProcess;
};

/*
 * MonoProcess describes processes we create.
 * It contains a semaphore that can be waited on in order to wait
 * for process termination. It's accessed in our SIGCHLD handler,
 * when status is updated (and pid cleared, to not clash with 
 * subsequent processes that may get executed).
 */
typedef struct _MonoProcess MonoProcess;
struct _MonoProcess {
	pid_t pid; /* the pid of the process. This value is only valid until the process has exited. */
	MonoSemType exit_sem; /* this semaphore will be released when the process exits */
	int status; /* the exit status */
	gint32 handle_count; /* the number of handles to this mono_process instance */
	/* we keep a ref to the creating _WapiHandle_process handle until
	 * the process has exited, so that the information there isn't lost.
	 */
	gpointer handle;
	gboolean freeable;
	MonoProcess *next;
};

/* WapiHandle_process is a structure containing all the required information for process handling. */
typedef struct {
	pid_t id;
	guint32 exitstatus;
	gpointer main_thread;
	WapiFileTime create_time;
	WapiFileTime exit_time;
	char *proc_name;
	size_t min_working_set;
	size_t max_working_set;
	gboolean exited;
	MonoProcess *mono_process;
} WapiHandle_process;

/*
 * Handles > _WAPI_PROCESS_UNHANDLED are pseudo handles which represent processes
 * not started by the runtime.
 */
/* This marks a system process that we don't have a handle on */
/* FIXME: Cope with PIDs > sizeof guint */
#define _WAPI_PROCESS_UNHANDLED (1 << (8*sizeof(pid_t)-1))
#define _WAPI_PROCESS_UNHANDLED_PID_MASK (-1 & ~_WAPI_PROCESS_UNHANDLED)
#define WAPI_IS_PSEUDO_PROCESS_HANDLE(handle) ((GPOINTER_TO_UINT(handle) & _WAPI_PROCESS_UNHANDLED) == _WAPI_PROCESS_UNHANDLED)
#define WAPI_PID_TO_HANDLE(pid) GINT_TO_POINTER (_WAPI_PROCESS_UNHANDLED + (pid))
#define WAPI_HANDLE_TO_PID(handle) (GPOINTER_TO_UINT ((handle)) - _WAPI_PROCESS_UNHANDLED)

/* There doesn't seem to be a defined symbol for this */
#define _WAPI_PROCESS_CURRENT (gpointer)0xFFFFFFFF

#define DEBUG_PROCESS 0x00000001
#define DEBUG_ONLY_THIS_PROCESS 0x00000002
#define DETACHED_PROCESS 0x00000008
#define CREATE_NEW_CONSOLE 0x00000010
#define NORMAL_PRIORITY_CLASS 0x00000020
#define IDLE_PRIORITY_CLASS 0x00000040
#define HIGH_PRIORITY_CLASS 0x00000080
#define REALTIME_PRIORITY_CLASS 0x00000100
#define CREATE_NEW_PROCESS_GROUP 0x00000200
#define CREATE_UNICODE_ENVIRONMENT 0x00000400
#define CREATE_SEPARATE_WOW_VDM 0x00000800
#define CREATE_SHARED_WOW_VDM 0x00001000
#define CREATE_FORCEDOS 0x00002000
#define BELOW_NORMAL_PRIORITY_CLASS 0x00004000
#define ABOVE_NORMAL_PRIORITY_CLASS 0x00008000
#define CREATE_BREAKAWAY_FROM_JOB 0x01000000
#define CREATE_WITH_USERPROFILE 0x02000000
#define CREATE_DEFAULT_ERROR_MODE 0x04000000
#define CREATE_NO_WINDOW 0x08000000

#ifdef NEW_STUFF
#define CREATE_PRESERVE_CODE_AUTHZ_LEVEL find out the value for this one...
#endif

#define	PROCESS_TERMINATE		0x0001
#define	PROCESS_CREATE_THREAD		0x0002
#define	PROCESS_SET_SESSIONID		0x0004
#define	PROCESS_VM_OPERATION		0x0008
#define	PROCESS_VM_READ			0x0010
#define	PROCESS_VM_WRITE		0x0020
#define	PROCESS_DUP_HANDLE		0x0040
#define	PROCESS_CREATE_PROCESS		0x0080
#define	PROCESS_SET_QUOTA		0x0100
#define	PROCESS_SET_INFORMATION		0x0200
#define	PROCESS_QUERY_INFORMATION	0x0400
#define	PROCESS_ALL_ACCESS		(STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xfff)

extern gboolean ShellExecuteEx (WapiShellExecuteInfo *sei);
extern gboolean CreateProcess (const gunichar2 *appname,
			       const gunichar2 *cmdline,
			       WapiSecurityAttributes *process_attrs,
			       WapiSecurityAttributes *thread_attrs,
			       gboolean inherit_handles, guint32 create_flags,
			       gpointer environ, const gunichar2 *cwd,
			       WapiStartupInfo *startup,
			       WapiProcessInformation *process_info);
extern gboolean CreateProcessWithLogonW (const gunichar2 *username,
					 const gunichar2 *domain,
					 const gunichar2 *password,
					 const guint32 logonFlags,
					 const gunichar2 *appname,
					 const gunichar2 *cmdline,
					 guint32 create_flags,
					 gpointer environ,
					 const gunichar2 *cwd,
					 WapiStartupInfo *startup,
					 WapiProcessInformation *process_info);
#define LOGON_WITH_PROFILE 0x00000001
#define LOGON_NETCREDENTIALS_ONLY 0x00000002

extern guint32 GetProcessId (gpointer handle);
extern gboolean CloseProcess (gpointer handle);
extern gboolean GetExitCodeProcess (gpointer process, guint32 *code);
extern gboolean GetProcessTimes (gpointer process, WapiFileTime *create_time,
				 WapiFileTime *exit_time,
				 WapiFileTime *kernel_time,
				 WapiFileTime *user_time);
extern gboolean GetProcessWorkingSetSize (gpointer process, size_t *min,
					  size_t *max);
extern gboolean SetProcessWorkingSetSize (gpointer process, size_t min,
					  size_t max);

extern gboolean TerminateProcess (gpointer process, gint32 exitCode);

extern guint32 GetPriorityClass (gpointer process);
extern gboolean SetPriorityClass (gpointer process, guint32  priority_class);

gchar* wapi_process_get_path (pid_t pid);

void wapi_process_set_cli_launcher (char *path);

G_END_DECLS

#endif /* _WAPI_PROCESSES_H_ */
