//
// Mono.Posix/Syscall.cs
//
// Authors:
//   Miguel de Icaza (miguel@novell.com)
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2003 Novell, Inc.
// (C) 2004 Jonathan Pryor
//
// This file implements the low-level syscall interface to the POSIX
// subsystem.
//
// This file tries to stay close to the low-level API as much as possible
// using enumerations, structures and in a few cases, using existing .NET
// data types.
//
// Implementation notes:
//
//    Since the values for the various constants on the API changes
//    from system to system (even Linux on different architectures will
//    have different values), we define our own set of values, and we
//    use a set of C helper routines to map from the constants we define
//    to the values of the native OS.
//
//    Bitfields are flagged with the [Map] attribute, and a helper program
//    generates a set of routines that we can call to convert from our value 
//    definitions to the value definitions expected by the OS; see
//    PosixConvert for the conversion routines.
//
//    Methods that require tuning are bound as `private sys_NAME' methods
//    and then a `NAME' method is exposed.
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using Mono.Posix;

[assembly:Mono.Posix.IncludeAttribute (
	new string [] {"sys/types.h", "sys/stat.h", "sys/poll.h", "sys/wait.h", "sys/mount.h",
		"unistd.h", "fcntl.h", "signal.h", "poll.h", "grp.h", "errno.h"}, 
	new string [] {"_GNU_SOURCE", "_XOPEN_SOURCE"})]

namespace Mono.Posix {

	[Map]
	public enum Error : int {
		// errors & their values liberally copied from
		// FC2 /usr/include/asm/errno.h
		
		EPERM           =   1, // Operation not permitted 
		ENOENT          =   2, // No such file or directory 
		ESRCH           =   3, // No such process 
		EINTR           =   4, // Interrupted system call 
		EIO             =   5, // I/O error 
		ENXIO           =   6, // No such device or address 
		E2BIG           =   7, // Arg list too long 
		ENOEXEC         =   8, // Exec format error 
		EBADF           =   9, // Bad file number 
		ECHILD          =  10, // No child processes 
		EAGAIN          =  11, // Try again 
		ENOMEM          =  12, // Out of memory 
		EACCES          =  13, // Permission denied 
		EFAULT          =  14, // Bad address 
		ENOTBLK         =  15, // Block device required 
		EBUSY           =  16, // Device or resource busy 
		EEXIST          =  17, // File exists 
		EXDEV           =  18, // Cross-device link 
		ENODEV          =  19, // No such device 
		ENOTDIR         =  20, // Not a directory 
		EISDIR          =  21, // Is a directory 
		EINVAL          =  22, // Invalid argument 
		ENFILE          =  23, // File table overflow 
		EMFILE          =  24, // Too many open files 
		ENOTTY          =  25, // Not a typewriter 
		ETXTBSY         =  26, // Text file busy 
		EFBIG           =  27, // File too large 
		ENOSPC          =  28, // No space left on device 
		ESPIPE          =  29, // Illegal seek 
		EROFS           =  30, // Read-only file system 
		EMLINK          =  31, // Too many links 
		EPIPE           =  32, // Broken pipe 
		EDOM            =  33, // Math argument out of domain of func 
		ERANGE          =  34, // Math result not representable 
		EDEADLK         =  35, // Resource deadlock would occur 
		ENAMETOOLONG    =  36, // File name too long 
		ENOLCK          =  37, // No record locks available 
		ENOSYS          =  38, // Function not implemented 
		ENOTEMPTY       =  39, // Directory not empty 
		ELOOP           =  40, // Too many symbolic links encountered 
		EWOULDBLOCK     =  EAGAIN, // Operation would block 
		ENOMSG          =  42, // No message of desired type 
		EIDRM           =  43, // Identifier removed 
		ECHRNG          =  44, // Channel number out of range 
		EL2NSYNC        =  45, // Level 2 not synchronized 
		EL3HLT          =  46, // Level 3 halted 
		EL3RST          =  47, // Level 3 reset 
		ELNRNG          =  48, // Link number out of range 
		EUNATCH         =  49, // Protocol driver not attached 
		ENOCSI          =  50, // No CSI structure available 
		EL2HLT          =  51, // Level 2 halted 
		EBADE           =  52, // Invalid exchange 
		EBADR           =  53, // Invalid request descriptor 
		EXFULL          =  54, // Exchange full 
		ENOANO          =  55, // No anode 
		EBADRQC         =  56, // Invalid request code 
		EBADSLT         =  57, // Invalid slot 
                      
		EDEADLOCK	      =  EDEADLK,
                      
		EBFONT          =  59, // Bad font file format 
		ENOSTR          =  60, // Device not a stream 
		ENODATA         =  61, // No data available 
		ETIME           =  62, // Timer expired 
		ENOSR           =  63, // Out of streams resources 
		ENONET          =  64, // Machine is not on the network 
		ENOPKG          =  65, // Package not installed 
		EREMOTE         =  66, // Object is remote 
		ENOLINK         =  67, // Link has been severed 
		EADV            =  68, // Advertise error 
		ESRMNT          =  69, // Srmount error 
		ECOMM           =  70, // Communication error on send 
		EPROTO          =  71, // Protocol error 
		EMULTIHOP       =  72, // Multihop attempted 
		EDOTDOT         =  73, // RFS specific error 
		EBADMSG         =  74, // Not a data message 
		EOVERFLOW       =  75, // Value too large for defined data type 
		ENOTUNIQ        =  76, // Name not unique on network 
		EBADFD          =  77, // File descriptor in bad state 
		EREMCHG         =  78, // Remote address changed 
		ELIBACC         =  79, // Can not access a needed shared library 
		ELIBBAD         =  80, // Accessing a corrupted shared library 
		ELIBSCN         =  81, // .lib section in a.out corrupted 
		ELIBMAX         =  82, // Attempting to link in too many shared libraries 
		ELIBEXEC        =  83, // Cannot exec a shared library directly 
		EILSEQ          =  84, // Illegal byte sequence 
		ERESTART        =  85, // Interrupted system call should be restarted 
		ESTRPIPE        =  86, // Streams pipe error 
		EUSERS          =  87, // Too many users 
		ENOTSOCK        =  88, // Socket operation on non-socket 
		EDESTADDRREQ    =  89, // Destination address required 
		EMSGSIZE        =  90, // Message too long 
		EPROTOTYPE      =  91, // Protocol wrong type for socket 
		ENOPROTOOPT     =  92, // Protocol not available 
		EPROTONOSUPPORT =  93, // Protocol not supported 
		ESOCKTNOSUPPORT	=  94, // Socket type not supported 
		EOPNOTSUPP      =  95, // Operation not supported on transport endpoint 
		EPFNOSUPPORT    =  96, // Protocol family not supported 
		EAFNOSUPPORT    =  97, // Address family not supported by protocol 
		EADDRINUSE      =  98, // Address already in use 
		EADDRNOTAVAIL   =  99, // Cannot assign requested address 
		ENETDOWN        = 100, // Network is down 
		ENETUNREACH     = 101, // Network is unreachable 
		ENETRESET       = 102, // Network dropped connection because of reset 
		ECONNABORTED    = 103, // Software caused connection abort 
		ECONNRESET      = 104, // Connection reset by peer 
		ENOBUFS         = 105, // No buffer space available 
		EISCONN         = 106, // Transport endpoint is already connected 
		ENOTCONN        = 107, // Transport endpoint is not connected 
		ESHUTDOWN       = 108, // Cannot send after transport endpoint shutdown 
		ETOOMANYREFS    = 109, // Too many references: cannot splice 
		ETIMEDOUT       = 110, // Connection timed out 
		ECONNREFUSED    = 111, // Connection refused 
		EHOSTDOWN       = 112, // Host is down 
		EHOSTUNREACH    = 113, // No route to host 
		EALREADY        = 114, // Operation already in progress 
		EINPROGRESS     = 115, // Operation now in progress 
		ESTALE          = 116, // Stale NFS file handle 
		EUCLEAN         = 117, // Structure needs cleaning 
		ENOTNAM         = 118, // Not a XENIX named type file 
		ENAVAIL         = 119, // No XENIX semaphores available 
		EISNAM          = 120, // Is a named type file 
		EREMOTEIO       = 121, // Remote I/O error 
		EDQUOT          = 122, // Quota exceeded 

		ENOMEDIUM       = 123, // No medium found 
		EMEDIUMTYPE     = 124, // Wrong medium type 
	}

	[Map][Flags]
	public enum OpenFlags : int {
		//
		// One of these
		//
		O_RDONLY    = 0,
		O_WRONLY    = 1,
		O_RDWR      = 2,

		//
		// Or-ed with zero or more of these
		//
		O_CREAT     = 4,
		O_EXCL      = 8,
		O_NOCTTY    = 16,
		O_TRUNC     = 32,
		O_APPEND    = 64,
		O_NONBLOCK  = 128,
		O_SYNC      = 256,

		//
		// These are non-Posix.  Using them will result in errors/exceptions on
		// non-supported platforms.
		//
		// (For example, "C-wrapped" system calls -- calls with implementation in
		// MonoPosixHelper -- will return -1 with errno=EINVAL.  C#-wrapped system
		// calls will generate an exception in PosixConvert, as the value can't be
		// converted on the target platform.)
		//
		
		O_NOFOLLOW  = 512,
		O_DIRECTORY = 1024,
		O_DIRECT    = 2048,
		O_ASYNC     = 4096,
		O_LARGEFILE = 8192
	}
	
	// mode_t
	[Flags][Map]
	public enum FilePermissions : uint {
		S_ISUID     = 0x0800, // Set user ID on execution
		S_ISGID     = 0x0400, // Set gorup ID on execution
		S_ISVTX     = 0x0200, // Save swapped text after use (sticky).
		S_IRUSR     = 0x0100, // Read by owner
		S_IWUSR     = 0x0080, // Write by owner
		S_IXUSR     = 0x0040, // Execute by owner
		S_IRGRP     = 0x0020, // Read by group
		S_IWGRP     = 0x0010, // Write by group
		S_IXGRP     = 0x0008, // Execute by group
		S_IROTH     = 0x0004, // Read by other
		S_IWOTH     = 0x0002, // Write by other
		S_IXOTH     = 0x0001, // Execute by other

		S_IRWXG     = (S_IRGRP | S_IWGRP | S_IXGRP),
		S_IRWXU     = (S_IRUSR | S_IWUSR | S_IXUSR),
		S_IRWXO     = (S_IROTH | S_IWOTH | S_IXOTH),
		ACCESSPERMS = (S_IRWXU | S_IRWXG | S_IRWXO), // 0777
		ALLPERMS    = (S_ISUID | S_ISGID | S_ISVTX | S_IRWXU | S_IRWXG | S_IRWXO), // 07777
		DEFFILEMODE = (S_IRUSR | S_IWUSR | S_IRGRP | S_IWGRP | S_IROTH | S_IWOTH), // 0666

		// Device types
		// Why these are held in "mode_t" is beyond me...
		S_IFMT      = 0xF000, // Bits which determine file type
		S_IFDIR     = 0x4000, // Directory
		S_IFCHR     = 0x2000, // Character device
		S_IFBLK     = 0x6000, // Block device
		S_IFREG     = 0x8000, // Regular file
		S_IFIFO     = 0x1000, // FIFO
		S_IFLNK     = 0xA000, // Symbolic link
		S_IFSOCK    = 0xC000, // Socket
	}

	public struct Flock {
		public LockType         l_type;    // Type of lock: F_RDLCK, F_WRLCK, F_UNLCK
		public SeekFlags        l_whence;  // How to interpret l_start
		public /* off_t */ long l_start;   // Starting offset for lock
		public /* off_t */ long l_len;     // Number of bytes to lock
		public /* pid_t */ int  l_pid;     // PID of process blocking our lock (F_GETLK only)
	}

	[Map]
	public enum FcntlCommand : int {
		// Form /usr/include/bits/fcntl.h
		F_DUPFD    =    0, // Duplicate file descriptor.
		F_GETFD    =    1, // Get file descriptor flags.
		F_SETFD    =    2, // Set file descriptor flags.
		F_GETFL    =    3, // Get file status flags.
		F_SETFL    =    4, // Set file status flags.
		F_GETLK    =   12, // Get record locking info. [64]
		F_SETLK    =   13, // Set record locking info (non-blocking). [64]
		F_SETLKW   =   14, // Set record locking info (blocking). [64]
		F_SETOWN   =    8, // Set owner of socket (receiver of SIGIO).
		F_GETOWN   =    9, // Get owner of socket (receiver of SIGIO).
		F_SETSIG   =   10, // Set number of signal to be sent.
		F_GETSIG   =   11, // Get number of signal to be sent.
		F_SETLEASE = 1024, // Set a lease.
		F_GETLEASE = 1025, // Enquire what lease is active.
		F_NOTIFY   = 1026, // Required notifications on a directory
	}

	[Map]
	public enum LockType : short {
		F_RDLCK = 0, // Read lock.
		F_WRLCK = 1, // Write lock.
		F_UNLCK = 2, // Remove lock.
	}

	[Map]
	public enum SeekFlags : short {
		// values liberally copied from /usr/include/unistd.h
		SEEK_SET = 0, // Seek from beginning of file.
		SEEK_CUR = 1, // Seek from current position.
		SEEK_END = 2, // Seek from end of file.

		L_SET    = SEEK_SET, // BSD alias for SEEK_SET
		L_INCR   = SEEK_CUR, // BSD alias for SEEK_CUR
		L_XTND   = SEEK_END, // BSD alias for SEEK_END
	}
	
	[Map, Flags]
	public enum DirectoryNotifyFlags : int {
		// from /usr/include/bits/fcntl.h
		DN_ACCESS    = 0x00000001, // File accessed.
		DN_MODIFY    = 0x00000002, // File modified.
		DN_CREATE    = 0x00000004, // File created.
		DN_DELETE    = 0x00000008, // File removed.
		DN_RENAME    = 0x00000010, // File renamed.
		DN_ATTRIB    = 0x00000020, // File changed attributes.
		DN_MULTISHOT = unchecked ((int)0x80000000), // Don't remove notifier
	}

	[Map]
	public enum PosixFadviseAdvice : int {
		POSIX_FADV_NORMAL     = 0,  // No further special treatment.
		POSIX_FADV_RANDOM     = 1,  // Expect random page references.
		POSIX_FADV_SEQUENTIAL = 2,  // Expect sequential page references.
		POSIX_FADV_WILLNEED   = 3,  // Will need these pages.
		POSIX_FADV_DONTNEED   = 4,  // Don't need these pages.
		POSIX_FADV_NOREUSE    = 5,  // Data will be accessed once.
	}

	[Map]
	public enum PosixMadviseAdvice : int {
		POSIX_MADV_NORMAL     = 0,  // No further special treatment.
		POSIX_MADV_RANDOM     = 1,  // Expect random page references.
		POSIX_MADV_SEQUENTIAL = 2,  // Expect sequential page references.
		POSIX_MADV_WILLNEED   = 3,  // Will need these pages.
		POSIX_MADV_DONTNEED   = 4,  // Don't need these pages.
	}

	[Map]
	public enum Signum : int {
		SIGHUP    =  1, // Hangup (POSIX).
		SIGINT    =  2, // Interrupt (ANSI).
		SIGQUIT   =  3, // Quit (POSIX).
		SIGILL    =  4, // Illegal instruction (ANSI).
		SIGTRAP   =  5, // Trace trap (POSIX).
		SIGABRT   =  6, // Abort (ANSI).
		SIGIOT    =  6, // IOT trap (4.2 BSD).
		SIGBUS    =  7, // BUS error (4.2 BSD).
		SIGFPE    =  8, // Floating-point exception (ANSI).
		SIGKILL   =  9, // Kill, unblockable (POSIX).
		SIGUSR1   = 10, // User-defined signal 1 (POSIX).
		SIGSEGV   = 11, // Segmentation violation (ANSI).
		SIGUSR2   = 12, // User-defined signal 2 (POSIX).
		SIGPIPE   = 13, // Broken pipe (POSIX).
		SIGALRM   = 14, // Alarm clock (POSIX).
		SIGTERM   = 15, // Termination (ANSI).
		SIGSTKFLT = 16, // Stack fault.
		SIGCLD    = SIGCHLD, // Same as SIGCHLD (System V).
		SIGCHLD   = 17, // Child status has changed (POSIX).
		SIGCONT   = 18, // Continue (POSIX).
		SIGSTOP   = 19, // Stop, unblockable (POSIX).
		SIGTSTP   = 20, // Keyboard stop (POSIX).
		SIGTTIN   = 21, // Background read from tty (POSIX).
		SIGTTOU   = 22, // Background write to tty (POSIX).
		SIGURG    = 23, // Urgent condition on socket (4.2 BSD).
		SIGXCPU   = 24, // CPU limit exceeded (4.2 BSD).
		SIGXFSZ   = 25, // File size limit exceeded (4.2 BSD).
		SIGVTALRM = 26, // Virtual alarm clock (4.2 BSD).
		SIGPROF   = 27, // Profiling alarm clock (4.2 BSD).
		SIGWINCH  = 28, // Window size change (4.3 BSD, Sun).
		SIGPOLL   = SIGIO, // Pollable event occurred (System V).
		SIGIO     = 29, // I/O now possible (4.2 BSD).
		SIGPWR    = 30, // Power failure restart (System V).
		SIGSYS    = 31, // Bad system call.
		SIGUNUSED = 31
	}

	[Map][Flags]
	public enum MountFlags : ulong {
		MS_RDONLY      = 1,    // Mount read-only.
		MS_NOSUID      = 2,    // Ignore suid and sgid bits.
		MS_NODEV       = 4,    // Disallow access to device special files.
		MS_NOEXEC      = 8,    // Disallow program execution.
		MS_SYNCHRONOUS = 16,   // Writes are synced at once.
		MS_REMOUNT     = 32,   // Alter flags of a mounted FS.
		MS_MANDLOCK    = 64,   // Allow mandatory locks on an FS.
		S_WRITE        = 128,  // Write on file/directory/symlink.
		S_APPEND       = 256,  // Append-only file.
		S_IMMUTABLE    = 512,  // Immutable file.
		MS_NOATIME     = 1024, // Do not update access times.
		MS_NODIRATIME  = 2048, // Do not update directory access times.
		MS_BIND        = 4096, // Bind directory at different place.
		MS_RMT_MASK    = (MS_RDONLY | MS_MANDLOCK),
		MS_MGC_VAL     = 0xc0ed0000, // Magic flag number to indicate "new" flags
		MS_MGC_MSK     = 0xffff0000, // Magic flag number mask
	}

	[Map][Flags]
	public enum UmountFlags : int {
		MNT_FORCE  = 1, // Force unmount even if busy
		MNT_DETACH,     // Perform a lazy unmount.
	}

	[Flags][Map]
	public enum WaitOptions : int {
		WNOHANG   = 1,  // Don't block waiting
		WUNTRACED = 2,  // Report status of stopped children
	}

  [Flags][Map]
	public enum AccessMode : int {
		R_OK = 1,
		W_OK = 2,
		X_OK = 4,
		F_OK = 8,
	}

	[Map]
	public enum PathConf : int {
		_PC_LINK_MAX,
		_PC_MAX_CANON,
		_PC_MAX_INPUT,
		_PC_NAME_MAX,
		_PC_PATH_MAX,
		_PC_PIPE_BUF,
		_PC_CHOWN_RESTRICTED,
		_PC_NO_TRUNC,
		_PC_VDISABLE,
		_PC_SYNC_IO,
		_PC_ASYNC_IO,
		_PC_PRIO_IO,
		_PC_SOCK_MAXBUF,
		_PC_FILESIZEBITS,
		_PC_REC_INCR_XFER_SIZE,
		_PC_REC_MAX_XFER_SIZE,
		_PC_REC_MIN_XFER_SIZE,
		_PC_REC_XFER_ALIGN,
		_PC_ALLOC_SIZE_MIN,
		_PC_SYMLINK_MAX,
		_PC_2_SYMLINKS
	}

	[Map]
	public enum SysConf : int {
		_SC_ARG_MAX,
		_SC_CHILD_MAX,
		_SC_CLK_TCK,
		_SC_NGROUPS_MAX,
		_SC_OPEN_MAX,
		_SC_STREAM_MAX,
		_SC_TZNAME_MAX,
		_SC_JOB_CONTROL,
		_SC_SAVED_IDS,
		_SC_REALTIME_SIGNALS,
		_SC_PRIORITY_SCHEDULING,
		_SC_TIMERS,
		_SC_ASYNCHRONOUS_IO,
		_SC_PRIORITIZED_IO,
		_SC_SYNCHRONIZED_IO,
		_SC_FSYNC,
		_SC_MAPPED_FILES,
		_SC_MEMLOCK,
		_SC_MEMLOCK_RANGE,
		_SC_MEMORY_PROTECTION,
		_SC_MESSAGE_PASSING,
		_SC_SEMAPHORES,
		_SC_SHARED_MEMORY_OBJECTS,
		_SC_AIO_LISTIO_MAX,
		_SC_AIO_MAX,
		_SC_AIO_PRIO_DELTA_MAX,
		_SC_DELAYTIMER_MAX,
		_SC_MQ_OPEN_MAX,
		_SC_MQ_PRIO_MAX,
		_SC_VERSION,
		_SC_PAGESIZE,
		_SC_RTSIG_MAX,
		_SC_SEM_NSEMS_MAX,
		_SC_SEM_VALUE_MAX,
		_SC_SIGQUEUE_MAX,
		_SC_TIMER_MAX,
		/* Values for the argument to `sysconf'
			 corresponding to _POSIX2_* symbols.  */
		_SC_BC_BASE_MAX,
		_SC_BC_DIM_MAX,
		_SC_BC_SCALE_MAX,
		_SC_BC_STRING_MAX,
		_SC_COLL_WEIGHTS_MAX,
		_SC_EQUIV_CLASS_MAX,
		_SC_EXPR_NEST_MAX,
		_SC_LINE_MAX,
		_SC_RE_DUP_MAX,
		_SC_CHARCLASS_NAME_MAX,
		_SC_2_VERSION,
		_SC_2_C_BIND,
		_SC_2_C_DEV,
		_SC_2_FORT_DEV,
		_SC_2_FORT_RUN,
		_SC_2_SW_DEV,
		_SC_2_LOCALEDEF,
		_SC_PII,
		_SC_PII_XTI,
		_SC_PII_SOCKET,
		_SC_PII_INTERNET,
		_SC_PII_OSI,
		_SC_POLL,
		_SC_SELECT,
		_SC_UIO_MAXIOV,
		_SC_IOV_MAX = _SC_UIO_MAXIOV,
		_SC_PII_INTERNET_STREAM,
		_SC_PII_INTERNET_DGRAM,
		_SC_PII_OSI_COTS,
		_SC_PII_OSI_CLTS,
		_SC_PII_OSI_M,
		_SC_T_IOV_MAX,
		/* Values according to POSIX 1003.1c (POSIX threads).  */
		_SC_THREADS,
		_SC_THREAD_SAFE_FUNCTIONS,
		_SC_GETGR_R_SIZE_MAX,
		_SC_GETPW_R_SIZE_MAX,
		_SC_LOGIN_NAME_MAX,
		_SC_TTY_NAME_MAX,
		_SC_THREAD_DESTRUCTOR_ITERATIONS,
		_SC_THREAD_KEYS_MAX,
		_SC_THREAD_STACK_MIN,
		_SC_THREAD_THREADS_MAX,
		_SC_THREAD_ATTR_STACKADDR,
		_SC_THREAD_ATTR_STACKSIZE,
		_SC_THREAD_PRIORITY_SCHEDULING,
		_SC_THREAD_PRIO_INHERIT,
		_SC_THREAD_PRIO_PROTECT,
		_SC_THREAD_PROCESS_SHARED,
		_SC_NPROCESSORS_CONF,
		_SC_NPROCESSORS_ONLN,
		_SC_PHYS_PAGES,
		_SC_AVPHYS_PAGES,
		_SC_ATEXIT_MAX,
		_SC_PASS_MAX,
		_SC_XOPEN_VERSION,
		_SC_XOPEN_XCU_VERSION,
		_SC_XOPEN_UNIX,
		_SC_XOPEN_CRYPT,
		_SC_XOPEN_ENH_I18N,
		_SC_XOPEN_SHM,
		_SC_2_CHAR_TERM,
		_SC_2_C_VERSION,
		_SC_2_UPE,
		_SC_XOPEN_XPG2,
		_SC_XOPEN_XPG3,
		_SC_XOPEN_XPG4,
		_SC_CHAR_BIT,
		_SC_CHAR_MAX,
		_SC_CHAR_MIN,
		_SC_INT_MAX,
		_SC_INT_MIN,
		_SC_LONG_BIT,
		_SC_WORD_BIT,
		_SC_MB_LEN_MAX,
		_SC_NZERO,
		_SC_SSIZE_MAX,
		_SC_SCHAR_MAX,
		_SC_SCHAR_MIN,
		_SC_SHRT_MAX,
		_SC_SHRT_MIN,
		_SC_UCHAR_MAX,
		_SC_UINT_MAX,
		_SC_ULONG_MAX,
		_SC_USHRT_MAX,
		_SC_NL_ARGMAX,
		_SC_NL_LANGMAX,
		_SC_NL_MSGMAX,
		_SC_NL_NMAX,
		_SC_NL_SETMAX,
		_SC_NL_TEXTMAX,
		_SC_XBS5_ILP32_OFF32,
		_SC_XBS5_ILP32_OFFBIG,
		_SC_XBS5_LP64_OFF64,
		_SC_XBS5_LPBIG_OFFBIG,
		_SC_XOPEN_LEGACY,
		_SC_XOPEN_REALTIME,
		_SC_XOPEN_REALTIME_THREADS,
		_SC_ADVISORY_INFO,
		_SC_BARRIERS,
		_SC_BASE,
		_SC_C_LANG_SUPPORT,
		_SC_C_LANG_SUPPORT_R,
		_SC_CLOCK_SELECTION,
		_SC_CPUTIME,
		_SC_THREAD_CPUTIME,
		_SC_DEVICE_IO,
		_SC_DEVICE_SPECIFIC,
		_SC_DEVICE_SPECIFIC_R,
		_SC_FD_MGMT,
		_SC_FIFO,
		_SC_PIPE,
		_SC_FILE_ATTRIBUTES,
		_SC_FILE_LOCKING,
		_SC_FILE_SYSTEM,
		_SC_MONOTONIC_CLOCK,
		_SC_MULTI_PROCESS,
		_SC_SINGLE_PROCESS,
		_SC_NETWORKING,
		_SC_READER_WRITER_LOCKS,
		_SC_SPIN_LOCKS,
		_SC_REGEXP,
		_SC_REGEX_VERSION,
		_SC_SHELL,
		_SC_SIGNALS,
		_SC_SPAWN,
		_SC_SPORADIC_SERVER,
		_SC_THREAD_SPORADIC_SERVER,
		_SC_SYSTEM_DATABASE,
		_SC_SYSTEM_DATABASE_R,
		_SC_TIMEOUTS,
		_SC_TYPED_MEMORY_OBJECTS,
		_SC_USER_GROUPS,
		_SC_USER_GROUPS_R,
		_SC_2_PBS,
		_SC_2_PBS_ACCOUNTING,
		_SC_2_PBS_LOCATE,
		_SC_2_PBS_MESSAGE,
		_SC_2_PBS_TRACK,
		_SC_SYMLOOP_MAX,
		_SC_STREAMS,
		_SC_2_PBS_CHECKPOINT,
		_SC_V6_ILP32_OFF32,
		_SC_V6_ILP32_OFFBIG,
		_SC_V6_LP64_OFF64,
		_SC_V6_LPBIG_OFFBIG,
		_SC_HOST_NAME_MAX,
		_SC_TRACE,
		_SC_TRACE_EVENT_FILTER,
		_SC_TRACE_INHERIT,
		_SC_TRACE_LOG,
		_SC_LEVEL1_ICACHE_SIZE,
		_SC_LEVEL1_ICACHE_ASSOC,
		_SC_LEVEL1_ICACHE_LINESIZE,
		_SC_LEVEL1_DCACHE_SIZE,
		_SC_LEVEL1_DCACHE_ASSOC,
		_SC_LEVEL1_DCACHE_LINESIZE,
		_SC_LEVEL2_CACHE_SIZE,
		_SC_LEVEL2_CACHE_ASSOC,
		_SC_LEVEL2_CACHE_LINESIZE,
		_SC_LEVEL3_CACHE_SIZE,
		_SC_LEVEL3_CACHE_ASSOC,
		_SC_LEVEL3_CACHE_LINESIZE,
		_SC_LEVEL4_CACHE_SIZE,
		_SC_LEVEL4_CACHE_ASSOC,
		_SC_LEVEL4_CACHE_LINESIZE
	}

	[Map]
	public enum ConfStr : int {
		_CS_PATH,			/* The default search path.  */
		_CS_V6_WIDTH_RESTRICTED_ENVS,
		_CS_GNU_LIBC_VERSION,
		_CS_GNU_LIBPTHREAD_VERSION,
		_CS_LFS_CFLAGS = 1000,
		_CS_LFS_LDFLAGS,
		_CS_LFS_LIBS,
		_CS_LFS_LINTFLAGS,
		_CS_LFS64_CFLAGS,
		_CS_LFS64_LDFLAGS,
		_CS_LFS64_LIBS,
		_CS_LFS64_LINTFLAGS,
		_CS_XBS5_ILP32_OFF32_CFLAGS = 1100,
		_CS_XBS5_ILP32_OFF32_LDFLAGS,
		_CS_XBS5_ILP32_OFF32_LIBS,
		_CS_XBS5_ILP32_OFF32_LINTFLAGS,
		_CS_XBS5_ILP32_OFFBIG_CFLAGS,
		_CS_XBS5_ILP32_OFFBIG_LDFLAGS,
		_CS_XBS5_ILP32_OFFBIG_LIBS,
		_CS_XBS5_ILP32_OFFBIG_LINTFLAGS,
		_CS_XBS5_LP64_OFF64_CFLAGS,
		_CS_XBS5_LP64_OFF64_LDFLAGS,
		_CS_XBS5_LP64_OFF64_LIBS,
		_CS_XBS5_LP64_OFF64_LINTFLAGS,
		_CS_XBS5_LPBIG_OFFBIG_CFLAGS,
		_CS_XBS5_LPBIG_OFFBIG_LDFLAGS,
		_CS_XBS5_LPBIG_OFFBIG_LIBS,
		_CS_XBS5_LPBIG_OFFBIG_LINTFLAGS,
		_CS_POSIX_V6_ILP32_OFF32_CFLAGS,
		_CS_POSIX_V6_ILP32_OFF32_LDFLAGS,
		_CS_POSIX_V6_ILP32_OFF32_LIBS,
		_CS_POSIX_V6_ILP32_OFF32_LINTFLAGS,
		_CS_POSIX_V6_ILP32_OFFBIG_CFLAGS,
		_CS_POSIX_V6_ILP32_OFFBIG_LDFLAGS,
		_CS_POSIX_V6_ILP32_OFFBIG_LIBS,
		_CS_POSIX_V6_ILP32_OFFBIG_LINTFLAGS,
		_CS_POSIX_V6_LP64_OFF64_CFLAGS,
		_CS_POSIX_V6_LP64_OFF64_LDFLAGS,
		_CS_POSIX_V6_LP64_OFF64_LIBS,
		_CS_POSIX_V6_LP64_OFF64_LINTFLAGS,
		_CS_POSIX_V6_LPBIG_OFFBIG_CFLAGS,
		_CS_POSIX_V6_LPBIG_OFFBIG_LDFLAGS,
		_CS_POSIX_V6_LPBIG_OFFBIG_LIBS,
		_CS_POSIX_V6_LPBIG_OFFBIG_LINTFLAGS
	}

	[Map]
	public enum LockFlags : int {
		F_ULOCK = 0, // Unlock a previously locked region.
		F_LOCK  = 1, // Lock a region for exclusive use.
		F_TLOCK = 2, // Test and lock a region for exclusive use.
		F_TEST  = 3, // Test a region for other process locks.
	}

	[Map][Flags]
	public enum PollEvents : short {
		POLLIN      = 0x0001, // There is data to read
		POLLPRI     = 0x0002, // There is urgent data to read
		POLLOUT     = 0x0004, // Writing now will not block
		POLLERR     = 0x0008, // Error condition
		POLLHUP     = 0x0010, // Hung up
		POLLNVAL    = 0x0020, // Invalid request; fd not open
		// XPG4.2 definitions (via _XOPEN_SOURCE)
		POLLRDNORM  = 0x0040, // Normal data bay be read
		POLLRDBAND  = 0x0080, // Priority data may be read
		POLLWRNORM  = 0x0100, // Writing now will not block
		POLLWRBAND  = 0x0200, // Priority data may be written
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Pollfd {
		public int fd;
		public PollEvents events;
		public PollEvents revents;
	}

	public sealed class Dirent
	{
		public /* ino_t */ ulong  d_ino;
		public /* off_t */ long   d_off;
		public ushort             d_reclen;
		public byte               d_type;
		public string             d_name;

		public override int GetHashCode ()
		{
			return d_ino.GetHashCode () ^ d_off.GetHashCode () ^ 
				d_reclen.GetHashCode () ^ d_type.GetHashCode () ^
				d_name.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;
			Dirent d = (Dirent) obj;
			return d.d_ino == d_ino && d.d_off == d_off &&
				d.d_reclen == d_reclen && d.d_type == d_type &&
				d.d_name == d.d_name;
		}

		public override string ToString ()
		{
			return d_name;
		}

		public static bool operator== (Dirent lhs, Dirent rhs)
		{
			return Object.Equals (lhs, rhs);
		}

		public static bool operator!= (Dirent lhs, Dirent rhs)
		{
			return Object.Equals (lhs, rhs);
		}
	}

	public sealed class Group
	{
		public string           gr_name;
		public string           gr_passwd;
		public /* gid_t */ uint gr_gid;
		public string[]         gr_mem;

		public override int GetHashCode ()
		{
			int memhc = 0;
			for (int i = 0; i < gr_mem.Length; ++i)
				memhc ^= gr_mem[i].GetHashCode ();

			return gr_name.GetHashCode () ^ gr_passwd.GetHashCode () ^ 
				gr_gid.GetHashCode () ^ memhc;
		}

		public override bool Equals (object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;
			Group g = (Group) obj;
			if (g.gr_gid != gr_gid)
				return false;
			if (g.gr_gid == gr_gid && g.gr_name == g.gr_name &&
				g.gr_passwd == g.gr_passwd && g.gr_mem.Length == gr_mem.Length) {
				for (int i = 0; i < gr_mem.Length; ++i)
					if (gr_mem[i] != g.gr_mem[i])
						return false;
				return true;
			}
			return false;
		}

		// Generate string in /etc/group format
		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.AppendFormat ("{0}:{1}:{2}:", gr_name, gr_passwd, gr_gid);
			GetMembers (sb, gr_mem);
			return sb.ToString ();
		}

		private static void GetMembers (StringBuilder sb, string[] members)
		{
			if (members.Length > 0)
				sb.Append (members[0]);
			for (int i = 1; i < members.Length; ++i) {
				sb.Append (",");
				sb.Append (members[i]);
			}
		}

		public static bool operator== (Group lhs, Group rhs)
		{
			return Object.Equals (lhs, rhs);
		}

		public static bool operator!= (Group lhs, Group rhs)
		{
			return Object.Equals (lhs, rhs);
		}
	}

	public sealed class Passwd
	{
		public string           pw_name;
		public string           pw_passwd;
		public /* uid_t */ uint pw_uid;
		public /* gid_t */ uint pw_gid;
		public string           pw_gecos;
		public string           pw_dir;
		public string           pw_shell;

		public override int GetHashCode ()
		{
			return pw_name.GetHashCode () ^ pw_passwd.GetHashCode () ^ 
				pw_uid.GetHashCode () ^ pw_gid.GetHashCode () ^
				pw_gecos.GetHashCode () ^ pw_dir.GetHashCode () ^
				pw_dir.GetHashCode () ^ pw_shell.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;
			Passwd p = (Passwd) obj;
			return p.pw_uid == pw_uid && p.pw_gid == pw_gid && p.pw_name == pw_name && 
				p.pw_passwd == pw_passwd && p.pw_gecos == p.pw_gecos && 
				p.pw_dir == p.pw_dir && p.pw_shell == p.pw_shell;
		}

		// Generate string in /etc/passwd format
		public override string ToString ()
		{
			return string.Format ("{0}:{1}:{2}:{3}:{4}:{5}:{6}",
				pw_name, pw_passwd, pw_uid, pw_gid, pw_gecos, pw_dir, pw_shell);
		}

		public static bool operator== (Passwd lhs, Passwd rhs)
		{
			return Object.Equals (lhs, rhs);
		}

		public static bool operator!= (Passwd lhs, Passwd rhs)
		{
			return Object.Equals (lhs, rhs);
		}
	}

	public struct Stat {
		public  /* dev_t */     ulong   st_dev;     // device
		public  /* ino_t */     ulong   st_ino;     // inode
		public  FilePermissions         st_mode;    // protection
		private uint                    _padding_;  // padding for structure alignment
		public  /* nlink_t */   ulong   st_nlink;   // number of hard links
		public  /* uid_t */     uint    st_uid;     // user ID of owner
		public  /* gid_t */     uint    st_gid;     // group ID of owner
		public  /* dev_t */     ulong   st_rdev;    // device type (if inode device)
		public  /* off_t */     long    st_size;    // total size, in bytes
		public  /* blksize_t */ long    st_blksize; // blocksize for filesystem I/O
		public  /* blkcnt_t */  long    st_blocks;  // number of blocks allocated
		public  /* time_t */    long    st_atime;   // time of last access
		public  /* time_t */    long    st_mtime;   // time of last modification
		public  /* time_t */    long    st_ctime;   // time of last status change
	}

	public struct Timeval {
		public  /* time_t */      long    tv_sec;   // seconds
		public  /* suseconds_t */ long    tv_usec;  // microseconds
	}

	public struct Timezone {
		public  int tz_minuteswest; // minutes W of Greenwich
		private int tz_dsttime;     // type of dst correction (OBSOLETE)
	}

	public struct Utimbuf {
		public  /* time_t */      long    actime;   // access time
		public  /* time_t */      long    modtime;  // modification time
	}

	//
	// Convention: Functions *not* part of the standard C library AND part of
	// a POSIX and/or Unix standard (X/Open, SUS, XPG, etc.) go here.
	//
	// For example, the man page should be similar to:
	//
	//    CONFORMING TO (or CONFORMS TO)
	//           XPG2, SUSv2, POSIX, etc.
	//
	// BSD- and GNU-specific exports can also be placed here.
	//
	// The only methods in here should be:
	//  (1) low-level functions
	//  (2) "Trivial" function overloads.  For example, if the parameters to a
	//      function are related (e.g. getgroups(2))
	//  (3) The return type SHOULD NOT be changed.  If you want to provide a
	//      convenience function with a nicer return type, place it into one of
	//      the Posix* wrapper classes, and give it a .NET-styled name.
	//  (4) Exceptions SHOULD NOT be thrown.  EXCEPTIONS: 
	//      - If you're wrapping *broken* methods which make assumptions about 
	//        input data, such as that an argument refers to N bytes of data.  
	//        This is currently limited to cuserid(3) and encrypt(3).
	//      - If you call functions which themselves generate exceptions.  
	//        This is the case for using PosixConvert, which will throw an
	//        exception if an invalid/unsupported value is used.
	//
	public sealed class Syscall : Stdlib
	{
		private const string LIBC = "libc";
		private const string MPH = "MonoPosixHelper";
		private const string CRYPT = "crypt";

		private Syscall () {}

		//
		// <aio.h>
		//

		// TODO: aio_cancel(3), aio_error(3), aio_fsync(3), aio_read(3), 
		// aio_return(3), aio_suspend(3), aio_write(3)
		//
		// Then update PosixStream.BeginRead to use the aio* functions.

		//
		// <dirent.h>
		//

		[DllImport (LIBC, SetLastError=true)]
		public static extern IntPtr opendir (string name);

		[DllImport (LIBC, SetLastError=true)]
		public static extern int closedir (IntPtr dir);

		// seekdir(3):
		//    void seekdir (DIR *dir, off_t offset);
		//    Slight modification.  Returns -1 on error, 0 on success.
		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_seekdir")]
		public static extern int seekdir (IntPtr dir, long offset);

		// telldir(3)
		//    off_t telldir(DIR *dir);
		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_telldir")]
		public static extern long telldir (IntPtr dir);

		[DllImport (LIBC, SetLastError=true)]
		public static extern void rewinddir (IntPtr dir);

		private struct _Dirent {
			public /* ino_t */ ulong  d_ino;
			public /* off_t */ long   d_off;
			public ushort             d_reclen;
			public byte               d_type;
			public IntPtr             d_name;
		}

		private static void CopyDirent (Dirent to, ref _Dirent from)
		{
			try {
				to.d_ino    = from.d_ino;
				to.d_off    = from.d_off;
				to.d_reclen = from.d_reclen;
				to.d_type   = from.d_type;
				if (from.d_name != IntPtr.Zero)
					to.d_name = PosixMarshal.PtrToString (from.d_name);
				else 
					to.d_name = null;
			}
			finally {
				Stdlib.free (from.d_name);
				from.d_name = IntPtr.Zero;
			}
		}

		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_readdir")]
		private static extern int sys_readdir (IntPtr dir, out _Dirent dentry);

		public static Dirent readdir (IntPtr dir)
		{
			_Dirent dentry;
			int r = sys_readdir (dir, out dentry);
			if (r != 0)
				return null;
			Dirent d = new Dirent ();
			CopyDirent (d, ref dentry);
			return d;
		}

		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_readdir_r")]
		private static extern int sys_readdir_r (IntPtr dirp, out _Dirent entry, out IntPtr result);

		public static int readdir_r (IntPtr dirp, Dirent entry, out IntPtr result)
		{
			entry.d_ino    = 0;
			entry.d_off    = 0;
			entry.d_reclen = 0;
			entry.d_type   = 0;
			entry.d_name   = null;

			_Dirent _d;
			int r = sys_readdir_r (dirp, out _d, out result);

			if (r == 0 && result != IntPtr.Zero) {
				CopyDirent (entry, ref _d);
			}

			return r;
		}

		[DllImport (LIBC, SetLastError=true)]
		public static extern int dirfd (IntPtr dir);

		//
		// <errno.h>
		//

		public static Error GetLastError ()
		{
			int errno = Marshal.GetLastWin32Error ();
			return PosixConvert.ToError (errno);
		}

		[DllImport (MPH, EntryPoint="Mono_Posix_Syscall_SetLastError")]
		private static extern void SetLastError (int error);

		public static void SetLastError (Error error)
		{
			int _error = PosixConvert.FromError (error);
			SetLastError (_error);
		}

		// strerror_r(3)
		//    int strerror_r(int errnum, char *buf, size_t n);
		[DllImport (LIBC, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_strerror_r")]
		private static extern int sys_strerror_r (int errnum, 
				[Out] StringBuilder buf, ulong n);

		public static int strerror_r (Error errnum, StringBuilder buf, ulong n)
		{
			int e = PosixConvert.FromError (errnum);
			return sys_strerror_r (e, buf, n);
		}

		public static int strerror_r (Error errnum, StringBuilder buf)
		{
			return strerror_r (errnum, buf, (ulong) buf.Capacity);
		}

		//
		// <fcntl.h>
		//
		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_fcntl")]
		public static extern int fcntl (int fd, FcntlCommand cmd);

		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_fcntl_arg")]
		public static extern int fcntl (int fd, FcntlCommand cmd, long arg);

		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_fcntl_lock")]
		public static extern int fcntl (int fd, FcntlCommand cmd, ref Flock @lock);

		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_open")]
		public static extern int open (string pathname, OpenFlags flags);

		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_open_mode")]
		public static extern int open (string pathname, OpenFlags flags, FilePermissions mode);

		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_creat")]
		public static extern int creat (string pathname, FilePermissions mode);

		// posix_fadvise(2)
		//    int posix_fadvise(int fd, off_t offset, off_t len, int advice);
		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_posix_fadvise")]
		public static extern int posix_fadvise (int fd, long offset, 
			long len, PosixFadviseAdvice advice);

		// posix_fallocate(P)
		//    int posix_fallocate(int fd, off_t offset, size_t len);
		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_posix_fallocate")]
		public static extern int posix_fallocate (int fd, long offset, long len);

		//
		// <grp.h>
		//

		// setgroups(2)
		//    int setgroups (size_t size, const gid_t *list);
		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_setgroups")]
		public static extern int setgroups (ulong size, uint[] list);

		public static int setgroups (uint [] list)
		{
			return setgroups ((ulong) list.Length, list);
		}

		private struct _Group
		{
			public IntPtr           gr_name;
			public IntPtr           gr_passwd;
			public /* gid_t */ uint gr_gid;
			public int              _gr_nmem_;
			public IntPtr           gr_mem;
			public IntPtr           _gr_buf_;
		}

		private static void CopyGroup (Group to, ref _Group from)
		{
			try {
				to.gr_gid    = from.gr_gid;
				to.gr_name   = PosixMarshal.PtrToString (from.gr_name);
				to.gr_passwd = PosixMarshal.PtrToString (from.gr_passwd);
				to.gr_mem    = PosixMarshal.PtrToStringArray (from._gr_nmem_, from.gr_mem);
			}
			finally {
				Stdlib.free (from.gr_mem);
				Stdlib.free (from._gr_buf_);
				from.gr_mem   = IntPtr.Zero;
				from._gr_buf_ = IntPtr.Zero;
			}
		}

		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_getgrnam")]
		private static extern int sys_getgrnam (string name, out _Group group);

		public static Group getgrnam (string name)
		{
			_Group group;
			int r = sys_getgrnam (name, out group);
			if (r != 0)
				return null;
			Group gr = new Group ();
			CopyGroup (gr, ref group);
			return gr;
		}

		// getgrgid(3)
		//    struct group *getgrgid(gid_t gid);
		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_getgrgid")]
		private static extern int sys_getgrgid (uint uid, out _Group group);

		public static Group getgrgid (uint uid)
		{
			_Group group;
			int r = sys_getgrgid (uid, out group);
			if (r != 0)
				return null;
			Group gr = new Group ();
			CopyGroup (gr, ref group);
			return gr;
		}

		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_getgrnam_r")]
		private static extern int sys_getgrnam_r (string name, out _Group grbuf, out IntPtr grbufp);

		public static int getgrnam_r (string name, Group grbuf, out Group grbufp)
		{
			grbufp = null;
			_Group group;
			IntPtr _grbufp;
			int r = sys_getgrnam_r (name, out group, out _grbufp);
			if (r == 0 && _grbufp != IntPtr.Zero) {
				CopyGroup (grbuf, ref group);
				grbufp = grbuf;
			}
			return r;
		}

		// getgrgid_r(3)
		//    int getgrgid_r(gid_t gid, struct group *gbuf, char *buf,
		//        size_t buflen, struct group **gbufp);
		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_getgrgid_r")]
		private static extern int sys_getgrgid_r (uint uid, out _Group grbuf, out IntPtr grbufp);

		public static int getgrgid_r (uint uid, Group grbuf, out Group grbufp)
		{
			grbufp = null;
			_Group group;
			IntPtr _grbufp;
			int r = sys_getgrgid_r (uid, out group, out _grbufp);
			if (r == 0 && _grbufp != IntPtr.Zero) {
				CopyGroup (grbuf, ref group);
				grbufp = grbuf;
			}
			return r;
		}

		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_getgrent")]
		private static extern int sys_getgrent (out _Group grbuf);

		public static Group getgrent ()
		{
			_Group group;
			int r = sys_getgrent (out group);
			if (r != 0)
				return null;
			Group gr = new Group();
			CopyGroup (gr, ref group);
			return gr;
		}

		[DllImport (LIBC, SetLastError=true)]
		public static extern void setgrent ();

		[DllImport (LIBC, SetLastError=true)]
		public static extern void endgrent ();

		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_fgetgrent")]
		private static extern int sys_fgetgrent (IntPtr stream, out _Group grbuf);

		public static Group fgetgrent (IntPtr stream)
		{
			_Group group;
			int r = sys_fgetgrent (stream, out group);
			if (r != 0)
				return null;
			Group gr = new Group ();
			CopyGroup (gr, ref group);
			return gr;
		}

		//
		// <pwd.h>
		//
		private struct _Passwd
		{
			public IntPtr           pw_name;
			public IntPtr           pw_passwd;
			public /* uid_t */ uint pw_uid;
			public /* gid_t */ uint pw_gid;
			public IntPtr           pw_gecos;
			public IntPtr           pw_dir;
			public IntPtr           pw_shell;
			public IntPtr           _pw_buf_;
		}

		private static void CopyPasswd (Passwd to, ref _Passwd from)
		{
			try {
				to.pw_name   = PosixMarshal.PtrToString (from.pw_name);
				to.pw_passwd = PosixMarshal.PtrToString (from.pw_passwd);
				to.pw_uid    = from.pw_uid;
				to.pw_gid    = from.pw_gid;
				to.pw_gecos  = PosixMarshal.PtrToString (from.pw_gecos);
				to.pw_dir    = PosixMarshal.PtrToString (from.pw_dir);
				to.pw_shell  = PosixMarshal.PtrToString (from.pw_shell);
			}
			finally {
				Stdlib.free (from._pw_buf_);
				from._pw_buf_ = IntPtr.Zero;
			}
		}

		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_getpwnam")]
		private static extern int sys_getpwnam (string name, out _Passwd passwd);

		public static Passwd getpwnam (string name)
		{
			_Passwd passwd;
			int r = sys_getpwnam (name, out passwd);
			if (r != 0)
				return null;
			Passwd pw = new Passwd ();
			CopyPasswd (pw, ref passwd);
			return pw;
		}

		// getpwuid(3)
		//    struct passwd *getpwnuid(uid_t uid);
		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_getpwuid")]
		private static extern int sys_getpwuid (uint uid, out _Passwd passwd);

		public static Passwd getpwuid (uint uid)
		{
			_Passwd passwd;
			int r = sys_getpwuid (uid, out passwd);
			if (r != 0)
				return null;
			Passwd pw = new Passwd ();
			CopyPasswd (pw, ref passwd);
			return pw;
		}

		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_getpwnam_r")]
		private static extern int sys_getpwnam_r (string name, out _Passwd pwbuf, out IntPtr pwbufp);

		public static int getpwnam_r (string name, Passwd pwbuf, out Passwd pwbufp)
		{
			pwbufp = null;
			_Passwd passwd;
			IntPtr _pwbufp;
			int r = sys_getpwnam_r (name, out passwd, out _pwbufp);
			if (r == 0 && _pwbufp != IntPtr.Zero) {
				CopyPasswd (pwbuf, ref passwd);
				pwbufp = pwbuf;
			}
			return r;
		}

		// getpwuid_r(3)
		//    int getpwuid_r(uid_t uid, struct passwd *pwbuf, char *buf, size_t
		//        buflen, struct passwd **pwbufp);
		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_getpwuid_r")]
		private static extern int sys_getpwuid_r (uint uid, out _Passwd pwbuf, out IntPtr pwbufp);

		public static int getpwuid_r (uint uid, Passwd pwbuf, out Passwd pwbufp)
		{
			pwbufp = null;
			_Passwd passwd;
			IntPtr _pwbufp;
			int r = sys_getpwuid_r (uid, out passwd, out _pwbufp);
			if (r == 0 && _pwbufp != IntPtr.Zero) {
				CopyPasswd (pwbuf, ref passwd);
				pwbufp = pwbuf;
			}
			return r;
		}

		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_getpwent")]
		private static extern int sys_getpwent (out _Passwd pwbuf);

		public static Passwd getpwent ()
		{
			_Passwd passwd;
			int r = sys_getpwent (out passwd);
			if (r != 0)
				return null;
			Passwd pw = new Passwd ();
			CopyPasswd (pw, ref passwd);
			return pw;
		}

		[DllImport (LIBC, SetLastError=true)]
		public static extern void setpwent ();

		[DllImport (LIBC, SetLastError=true)]
		public static extern void endpwent ();

		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_fgetpwent")]
		private static extern int sys_fgetpwent (IntPtr stream, out _Passwd pwbuf);

		public static Passwd fgetpwent (IntPtr stream)
		{
			_Passwd passwd;
			int r = sys_fgetpwent (stream, out passwd);
			if (r != 0)
				return null;
			Passwd pw = new Passwd ();
			CopyPasswd (pw, ref passwd);
			return pw;
		}

		//
		// <signal.h>
		//
		[DllImport (LIBC, SetLastError=true)]
		private static extern void psignal (int sig, string s);

		public static void psignal (Signum sig, string s)
		{
			int signum = PosixConvert.FromSignum (sig);
			psignal (signum, s);
		}

		// kill(2)
		//    int kill(pid_t pid, int sig);
		[DllImport (LIBC, SetLastError=true)]
		private static extern int sys_kill (int pid, int sig);

		public static int kill (int pid, Signum sig)
		{
			int _sig = PosixConvert.FromSignum (sig);
			return sys_kill (pid, _sig);
		}

		[DllImport (LIBC, SetLastError=true, EntryPoint="strsignal")]
		private static extern IntPtr sys_strsignal (int sig);

		public static IntPtr sys_strsignal (Signum sig)
		{
			int s = PosixConvert.FromSignum (sig);
			return sys_strsignal (s);
		}

		public static string strsignal (Signum sig)
		{
			IntPtr r = sys_strsignal (sig);
			return PosixMarshal.PtrToString (r);
		}

		// TODO: sigaction(2)
		// TODO: sigsuspend(2)
		// TODO: sigpending(2)


		//
		// <stdio.h>
		//
		[DllImport (MPH, EntryPoint="Mono_Posix_Syscall_L_ctermid")]
		private static extern int _L_ctermid ();

		public static readonly int L_ctermid = _L_ctermid ();

		[DllImport (MPH, EntryPoint="Mono_Posix_Syscall_L_cuserid")]
		private static extern int _L_cuserid ();

		public static readonly int L_cuserid = _L_cuserid ();

		[DllImport (LIBC, SetLastError=true, EntryPoint="cuserid")]
		private static extern IntPtr sys_cuserid ([Out] StringBuilder @string);

		[Obsolete ("\"Nobody knows precisely what cuserid() does... DO NOT USE cuserid()." +
				"`string' must hold L_cuserid characters.  Use Unistd.getlogin_r instead.")]
		public static string cuserid (StringBuilder @string)
		{
			if (@string.Capacity < L_cuserid) {
				throw new ArgumentOutOfRangeException ("string", "string.Capacity < L_cuserid");
			}
			IntPtr r = sys_cuserid (@string);
			return PosixMarshal.PtrToString (r);
		}

		//
		// <stdlib.h>
		//
		[DllImport (LIBC, SetLastError=true)]
		public static extern int ttyslot ();

		[DllImport (CRYPT, SetLastError=true)]
		public static extern void setkey (string key);

		//
		// <sys/mman.h>
		//

		// posix_madvise(P)
		//    int posix_madvise(void *addr, size_t len, int advice);
		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_posix_madvise")]
		public static extern int posix_madvise (IntPtr addr, ulong len, 
			PosixMadviseAdvice advice);

		//
		// <sys/mount.h>
		//
		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_mount")]
		public static extern int mount (string source, string target, 
				string filesystemtype, MountFlags mountflags, string data);

		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_umount")]
		public static extern int umount (string target);

		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_umount2")]
		private static extern int sys_umount2 (string target, int flags);

		public static int umount2 (string target, UmountFlags flags)
		{
			int _flags = PosixConvert.FromUmountFlags (flags);
			return sys_umount2 (target, _flags);
		}

		//
		// <sys/poll.h>
		//
		private struct _pollfd {
			public int fd;
			public short events;
			public short revents;
		}

		[DllImport (LIBC, SetLastError=true, EntryPoint="poll")]
		private static extern int sys_poll (_pollfd[] ufds, uint nfds, int timeout);

		public static int poll (Pollfd [] fds, uint nfds, int timeout)
		{
			if (fds.Length < nfds)
				throw new ArgumentOutOfRangeException ("fds", "Must refer to at least `nfds' elements");

			_pollfd[] send = new _pollfd[nfds];

			for (int i = 0; i < send.Length; i++) {
				send [i].fd     = fds [i].fd;
				send [i].events = PosixConvert.FromPollEvents (fds [i].events);
			}

			int r = sys_poll (send, nfds, timeout);

			for (int i = 0; i < send.Length; i++) {
				fds [i].revents = PosixConvert.ToPollEvents (send [i].revents);
			}

			return r;
		}

		public static int poll (Pollfd [] fds, int timeout)
		{
			return poll (fds, (uint) fds.Length, timeout);
		}

		//
		// <sys/ptrace.h>
		//

		// TODO: ptrace(2)

		//
		// <sys/resource.h>
		//

		// TODO: setrlimit(2)
		// TODO: getrlimit(2)
		// TODO: getrusage(2)

		//
		// <sys/sendfile.h>
		//

		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_sendfile")]
		public static extern long sendfile (int out_fd, int in_fd, 
				ref long offset, ulong count);

		//
		// <sys/stat.h>
		//
		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_stat")]
		public static extern int stat (string file_name, out Stat buf);

		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_fstat")]
		public static extern int fstat (int filedes, out Stat buf);

		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_lstat")]
		public static extern int lstat (string file_name, out Stat buf);

		[DllImport (LIBC, SetLastError=true, EntryPoint="chmod")]
		private static extern int sys_chmod (string path, uint mode);

		public static int chmod (string path, FilePermissions mode)
		{
			uint _mode = PosixConvert.FromFilePermissions (mode);
			return sys_chmod (path, _mode);
		}

		[DllImport (LIBC, SetLastError=true, EntryPoint="fchmod")]
		private static extern int sys_fchmod (int filedes, uint mode);

		public static int fchmod (int filedes, FilePermissions mode)
		{
			uint _mode = PosixConvert.FromFilePermissions (mode);
			return sys_fchmod (filedes, _mode);
		}

		[DllImport (LIBC, SetLastError=true, EntryPoint="umask")]
		private static extern int sys_umask (uint mask);

		public static int umask (FilePermissions mask)
		{
			uint _mask = PosixConvert.FromFilePermissions (mask);
			return sys_umask (_mask);
		}

		[DllImport (LIBC, SetLastError=true, EntryPoint="mkdir")]
		public static extern int sys_mkdir (string oldpath, uint mode);

		public static int mkdir (string oldpath, FilePermissions mode)
		{
			uint _mode = PosixConvert.FromFilePermissions (mode);
			return sys_mkdir (oldpath, _mode);
		}

		// mknod(2)
		//    int mknod (const char *pathname, mode_t mode, dev_t dev);
		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_mknod")]
		public static extern int mknod (string pathname, FilePermissions mode, ulong dev);

		//
		// <sys/time.h>
		//

		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_gettimeofday")]
		public static extern int gettimeofday (out Timeval tv, out Timezone tz);

		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_gettimeofday")]
		private static extern int gettimeofday (out Timeval tv, IntPtr ignore);

		public static int gettimeofday (out Timeval tv)
		{
			return gettimeofday (out tv, IntPtr.Zero);
		}

		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_gettimeofday")]
		private static extern int gettimeofday (IntPtr ignore, out Timezone tz);

		public static int gettimeofday (out Timezone tz)
		{
			return gettimeofday (IntPtr.Zero, out tz);
		}

		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_gettimeofday")]
		public static extern int settimeofday (ref Timeval tv, ref Timezone tz);

		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_gettimeofday")]
		private static extern int settimeofday (ref Timeval tv, IntPtr ignore);

		public static int settimeofday (ref Timeval tv)
		{
			return settimeofday (ref tv, IntPtr.Zero);
		}

		//
		// <sys/timeb.h>
		//

		// TODO: ftime(3)

		//
		// <sys/times.h>
		//

		// TODO: times(2)

		//
		// <sys/wait.h>
		//

		// wait(2)
		//    pid_t wait(int *status);
		[DllImport (LIBC, SetLastError=true)]
		public static extern int wait (out int status);

		// waitpid(2)
		//    pid_t waitpid(pid_t pid, int *status, int options);
		[DllImport (LIBC, SetLastError=true)]
		private static extern int waitpid (int pid, out int status, int options);

		public static int waitpid (int pid, out int status, WaitOptions options)
		{
			int _options = PosixConvert.FromWaitOptions (options);
			return waitpid (pid, out status, _options);
		}

		[DllImport (MPH, EntryPoint="Mono_Posix_Syscall_WIFEXITED")]
		private static extern int _WIFEXITED (int status);

		public static bool WIFEXITED (int status)
		{
			return _WIFEXITED (status) != 0;
		}

		[DllImport (MPH, EntryPoint="Mono_Posix_Syscall_WEXITSTATUS")]
		public static extern int WEXITSTATUS (int status);

		[DllImport (MPH, EntryPoint="Mono_Posix_Syscall_WIFSIGNALED")]
		private static extern int _WIFSIGNALED (int status);

		public static bool WIFSIGNALED (int status)
		{
			return _WIFSIGNALED (status) != 0;
		}

		[DllImport (MPH, EntryPoint="Mono_Posix_Syscall_WTERMSIG")]
		private static extern int _WTERMSIG (int status);

		public static Signum WTERMSIG (int status)
		{
			int r = _WTERMSIG (status);
			return PosixConvert.ToSignum (r);
		}

		[DllImport (MPH, EntryPoint="Mono_Posix_Syscall_WIFSTOPPED")]
		private static extern int _WIFSTOPPED (int status);

		public static bool WIFSTOPPED (int status)
		{
			return _WIFSTOPPED (status) != 0;
		}

		[DllImport (MPH, EntryPoint="Mono_Posix_Syscall_WSTOPSIG")]
		private static extern int _WSTOPSIG (int status);

		public static Signum WSTOPSIG (int status)
		{
			int r = _WSTOPSIG (status);
			return PosixConvert.ToSignum (r);
		}

		//
		// <termios.h>
		//

		//
		// <time.h>
		//

		// stime(2)
		//    int stime(time_t *t);
		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_stime")]
		public static extern int stime (ref long t);

		// time(2)
		//    time_t time(time_t *t);
		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_time")]
		public static extern long time (out long t);

		//
		// <ulimit.h>
		//

		// TODO: ulimit(3)

		//
		// <unistd.h>
		//
		[DllImport (LIBC, SetLastError=true, EntryPoint="access")]
		private static extern int sys_access (string pathname, int mode);

		public static int access (string pathname, AccessMode mode)
		{
			int _mode = PosixConvert.FromAccessMode (mode);
			return sys_access (pathname, _mode);
		}

		// lseek(2)
		//    off_t lseek(int filedes, off_t offset, int whence);
		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_lseek")]
		public static extern long lseek (int fd, long offset, SeekFlags whence);

    [DllImport (LIBC, SetLastError=true)]
		public static extern int close (int fd);

		// read(2)
		//    ssize_t read(int fd, void *buf, size_t count);
		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_read")]
		public static extern long read (int fd, IntPtr buf, ulong count);

		public static unsafe long read (int fd, void *buf, ulong count)
		{
			return read (fd, (IntPtr) buf, count);
		}

		// write(2)
		//    ssize_t write(int fd, const void *buf, size_t count);
		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_write")]
		public static extern long write (int fd, IntPtr buf, ulong count);

		public static unsafe long write (int fd, void *buf, ulong count)
		{
			return write (fd, (IntPtr) buf, count);
		}

		// pread(2)
		//    ssize_t pread(int fd, void *buf, size_t count, off_t offset);
		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_pread")]
		public static extern long pread (int fd, IntPtr buf, ulong count, long offset);

		public static unsafe long pread (int fd, void *buf, ulong count, long offset)
		{
			return pread (fd, (IntPtr) buf, count, offset);
		}

		// pwrite(2)
		//    ssize_t pwrite(int fd, const void *buf, size_t count, off_t offset);
		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_pwrite")]
		public static extern long pwrite (int fd, IntPtr buf, ulong count, long offset);

		public static unsafe long pwrite (int fd, void *buf, ulong count, long offset)
		{
			return pwrite (fd, (IntPtr) buf, count, offset);
		}

		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_pipe")]
		public static extern int pipe (out int reading, out int writing);

		public static int pipe (int[] filedes)
		{
			if (filedes == null || filedes.Length != 2) {
				// TODO: set errno
				return -1;
			}
			int reading, writing;
			int r = pipe (out reading, out writing);
			filedes[0] = reading;
			filedes[1] = writing;
			return r;
		}

		[DllImport (LIBC, SetLastError=true)]
		public static extern uint alarm (uint seconds);

		[DllImport (LIBC, SetLastError=true)]
		public static extern int sleep (uint seconds);

		[DllImport (LIBC, SetLastError=true)]
		public static extern uint ualarm (uint usecs, uint interval);

		[DllImport (LIBC, SetLastError=true)]
		public static extern int pause ();

		// chown(2)
		//    int chown(const char *path, uid_t owner, gid_t group);
		[DllImport (LIBC, SetLastError=true)]
		public static extern int chown (string path, uint owner, uint group);

		// fchown(2)
		//    int fchown(int fd, uid_t owner, gid_t group);
		[DllImport (LIBC, SetLastError=true)]
		public static extern int fchown (int fd, uint owner, uint group);

		// lchown(2)
		//    int lchown(const char *path, uid_t owner, gid_t group);
		[DllImport (LIBC, SetLastError=true)]
		public static extern int lchown (string path, uint owner, uint group);

		[DllImport (LIBC, SetLastError=true)]
		public static extern int chdir (string path);

		[DllImport (LIBC, SetLastError=true)]
		public static extern int fchdir (int fd);

		// getcwd(3)
		//    char *getcwd(char *buf, size_t size);
		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_getcwd")]
		public static extern IntPtr getcwd ([Out] StringBuilder buf, ulong size);

		public static StringBuilder getcwd (StringBuilder buf)
		{
			getcwd (buf, (ulong) buf.Capacity);
			return buf;
		}

		// getwd(2) is deprecated; don't expose it.

		[DllImport (LIBC, SetLastError=true)]
		public static extern int dup (int fd);

		[DllImport (LIBC, SetLastError=true)]
		public static extern int dup2 (int fd, int fd2);

		// TODO: does Mono marshal arrays properly?
		[DllImport (LIBC, SetLastError=true)]
		public static extern int execve (string path, string[] argv, string[] envp);

		[DllImport (LIBC, SetLastError=true)]
		public static extern int execv (string path, string[] argv);

		// TODO: execle, execl
		[DllImport (LIBC, SetLastError=true)]
		public static extern int execvp (string path, string[] argv);

		[DllImport (LIBC, SetLastError=true)]
		public static extern int nice (int inc);

		[DllImport (LIBC, SetLastError=true)]
		public static extern int _exit (int status);

		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_fpathconf")]
		public static extern long fpathconf (int filedes, PathConf name);

		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_pathconf")]
		public static extern long pathconf (string path, PathConf name);

		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_sysconf")]
		public static extern long sysconf (SysConf name);

		// confstr(3)
		//    size_t confstr(int name, char *buf, size_t len);
		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_confstr")]
		public static extern ulong confstr (ConfStr name, [Out] StringBuilder buf, ulong len);

		// getpid(2)
		//    pid_t getpid(void);
		[DllImport (LIBC, SetLastError=true)]
		public static extern int getpid ();

		// setpgid(2)
		//    int setpgid(pid_t pid, pid_t pgid);
		[DllImport (LIBC, SetLastError=true)]
		public static extern int setpgid (int pid, int pgid);

		// getpgid(2)
		//    pid_t getpgid(pid_t pid);
		[DllImport (LIBC, SetLastError=true)]
		public static extern int getpgid (int pid);

		[DllImport (LIBC, SetLastError=true)]
		public static extern int setpgrp ();

		// getpgrp(2)
		//    pid_t getpgrp(void);
		[DllImport (LIBC, SetLastError=true)]
		public static extern int getpgrp ();

		// setsid(2)
		//    pid_t setsid(void);
		[DllImport (LIBC, SetLastError=true)]
		public static extern int setsid ();

		// getsid(2)
		//    pid_t getsid(pid_t pid);
		[DllImport (LIBC, SetLastError=true)]
		public static extern int getsid (int pid);

		// getuid(2)
		//    uid_t getuid(void);
		[DllImport (LIBC, SetLastError=true)]
		public static extern uint getuid ();

		// geteuid(2)
		//    uid_t geteuid(void);
		[DllImport (LIBC, SetLastError=true)]
		public static extern uint geteuid ();

		// getgid(2)
		//    gid_t getgid(void);
		[DllImport (LIBC, SetLastError=true)]
		public static extern uint getgid ();

		// getegid(2)
		//    gid_t getgid(void);
		[DllImport (LIBC, SetLastError=true)]
		public static extern uint getegid ();

		// getgroups(2)
		//    int getgroups(int size, gid_t list[]);
		[DllImport (LIBC, SetLastError=true)]
		public static extern int getgroups (int size, uint[] list);

		public static int getgroups (uint[] list)
		{
			return getgroups (list.Length, list);
		}

		// setuid(2)
		//    int setuid(uid_t uid);
		[DllImport (LIBC, SetLastError=true)]
		public static extern int setuid (uint uid);

		// setreuid(2)
		//    int setreuid(uid_t ruid, uid_t euid);
		[DllImport (LIBC, SetLastError=true)]
		public static extern int setreuid (uint ruid, uint euid);

		// setregid(2)
		//    int setregid(gid_t ruid, gid_t euid);
		[DllImport (LIBC, SetLastError=true)]
		public static extern int setregid (uint rgid, uint egid);

		// seteuid(2)
		//    int seteuid(uid_t euid);
		[DllImport (LIBC, SetLastError=true)]
		public static extern int seteuid (uint euid);

		// setegid(2)
		//    int setegid(gid_t euid);
		[DllImport (LIBC, SetLastError=true)]
		public static extern int setegid (uint uid);

		// setgid(2)
		//    int setgid(gid_t gid);
		[DllImport (LIBC, SetLastError=true)]
		public static extern int setgid (uint gid);

		// getresuid(2)
		//    int getresuid(uid_t *ruid, uid_t *euid, uid_t *suid);
		[DllImport (LIBC, SetLastError=true)]
		public static extern int getresuid (out uint ruid, out uint euid, out uint suid);

		// getresgid(2)
		//    int getresgid(gid_t *ruid, gid_t *euid, gid_t *suid);
		[DllImport (LIBC, SetLastError=true)]
		public static extern int getresgid (out uint rgid, out uint egid, out uint sgid);

		// setresuid(2)
		//    int setresuid(uid_t ruid, uid_t euid, uid_t suid);
		[DllImport (LIBC, SetLastError=true)]
		public static extern int setresuid (uint ruid, uint euid, uint suid);

		// setresgid(2)
		//    int setresgid(gid_t ruid, gid_t euid, gid_t suid);
		[DllImport (LIBC, SetLastError=true)]
		public static extern int setresgid (uint rgid, uint egid, uint sgid);

		// fork(2)
		//    pid_t fork(void);
		[DllImport (LIBC, SetLastError=true)]
		[Obsolete ("DO NOT directly call fork(2); it bypasses essential " + 
				"shutdown code.\nUse System.Diagnostics.Process instead")]
		public static extern int fork ();

		// vfork(2)
		//    pid_t vfork(void);
		[DllImport (LIBC, SetLastError=true)]
		[Obsolete ("DO NOT directly call vfork(2); it bypasses essential " + 
				"shutdown code.\nUse System.Diagnostics.Process instead")]
		public static extern int vfork ();

		[DllImport (LIBC, SetLastError=true, EntryPoint="ttyname")]
		[Obsolete ("Not re-entrant.  Use ttyname_r instead.")]
		public static extern IntPtr sys_ttyname (int fd);

		[Obsolete ("Not re-entrant.  Use ttyname_r instead.")]
		public static string ttyname (int fd)
		{
			IntPtr r = sys_ttyname (fd);
			return PosixMarshal.PtrToString (r);
		}

		// ttyname_r(3)
		//    int ttyname_r(int fd, char *buf, size_t buflen);
		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_ttyname_r")]
		public static extern int ttyname_r (int fd, [Out] StringBuilder buf, ulong buflen);

		public static int ttyname_r (int fd, StringBuilder buf)
		{
			return ttyname_r (fd, buf, (ulong) buf.Capacity);
		}

		[DllImport (LIBC, EntryPoint="isatty")]
		private static extern int sys_isatty (int fd);

		public static bool isatty (int fd)
		{
			return sys_isatty (fd) == 1;
		}

		[DllImport (LIBC, SetLastError=true)]
		public static extern int link (string oldpath, string newpath);

		[DllImport (LIBC, SetLastError=true)]
		public static extern int symlink (string oldpath, string newpath);

		// readlink(2)
		//    int readlink(const char *path, char *buf, size_t bufsize);
		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_readlink")]
		public static extern int readlink (string path, [Out] StringBuilder buf, ulong bufsiz);

		public static int readlink (string path, [Out] StringBuilder buf)
		{
			return readlink (path, buf, (ulong) buf.Capacity);
		}

		[DllImport (LIBC, SetLastError=true)]
		public static extern int unlink (string pathname);

		[DllImport (LIBC, SetLastError=true)]
		public static extern int rmdir (string pathname);

		// tcgetpgrp(3)
		//    pid_t tcgetpgrp(int fd);
		[DllImport (LIBC, SetLastError=true)]
		public static extern int tcgetpgrp (int fd);

		// tcsetpgrp(3)
		//    int tcsetpgrp(int fd, pid_t pgrp);
		[DllImport (LIBC, SetLastError=true)]
		public static extern int tcsetpgrp (int fd, int pgrp);

		[DllImport (LIBC, SetLastError=true, EntryPoint="getlogin")]
		[Obsolete ("Not re-entrant.  Use getlogin_r instead.")]
		public static extern IntPtr sys_getlogin ();

		[Obsolete ("Not re-entrant.  Use getlogin_r instead.")]
		public static string getlogin ()
		{
			IntPtr r = sys_getlogin ();
			return PosixMarshal.PtrToString (r);
		}

		// getlogin_r(3)
		//    int getlogin_r(char *buf, size_t bufsize);
		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_getlogin_r")]
		public static extern int getlogin_r ([Out] StringBuilder name, ulong bufsize);

		public static int getlogin_r (StringBuilder name)
		{
			return getlogin_r (name, (ulong) name.Capacity);
		}

		// gethostname(2)
		//    int gethostname(char *name, size_t len);
		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_gethostname")]
		public static extern int gethostname ([Out] StringBuilder name, ulong len);

		public static int gethostname (StringBuilder name)
		{
			return gethostname (name, (ulong) name.Capacity);
		}

		// sethostname(2)
		//    int gethostname(const char *name, size_t len);
		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_gethostname")]
		public static extern int sethostname (string name, ulong len);

		public static int sethostname (string name)
		{
			return sethostname (name, (ulong) name.Length);
		}

		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_gethostid")]
		public static extern long gethostid ();

		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_sethostid")]
		public static extern int sethostid (long hostid);

		// getdomainname(2)
		//    int getdomainname(char *name, size_t len);
		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_getdomainname")]
		public static extern int getdomainname ([Out] StringBuilder name, ulong len);

		public static int getdomainname (StringBuilder name)
		{
			return getdomainname (name, (ulong) name.Capacity);
		}

		// setdomainname(2)
		//    int setdomainname(const char *name, size_t len);
		[DllImport (MPH, SetLastError=true,
				EntryPoint="Mono_Posix_Syscall_setdomainname")]
		public static extern int setdomainname (string name, ulong len);

		public static int setdomainname (string name)
		{
			return setdomainname (name, (ulong) name.Length);
		}

		[DllImport (LIBC, SetLastError=true)]
		public static extern int vhangup ();

		// Revoke doesn't appear to be POSIX.  Include it?
		[DllImport (LIBC, SetLastError=true)]
		public static extern int revoke (string file);

		// TODO: profil?  It's not POSIX.

		[DllImport (LIBC, SetLastError=true)]
		public static extern int acct (string filename);

		[DllImport (LIBC, SetLastError=true, EntryPoint="getusershell")]
		public static extern IntPtr sys_getusershell ();

		public static string getusershell ()
		{
			IntPtr r = sys_getusershell ();
			return PosixMarshal.PtrToString (r);
		}

		[DllImport (LIBC, SetLastError=true)]
		public static extern void setusershell ();

		[DllImport (LIBC, SetLastError=true)]
		public static extern void endusershell ();

		[DllImport (LIBC, SetLastError=true)]
		private static extern int daemon (int nochdir, int noclose);

		public static int daemon (bool nochdir, bool noclose)
		{
			return daemon (nochdir ? 1 : 0, noclose ? 1 : 0);
		}

		[DllImport (LIBC, SetLastError=true)]
		public static extern int chroot (string path);

		// skipping getpass(3) as the man page states:
		//   This function is obsolete.  Do not use it.

		[DllImport (LIBC, SetLastError=true)]
		public static extern int fsync (int fd);

		[DllImport (LIBC, SetLastError=true)]
		public static extern int fdatasync (int fd);

		[DllImport (LIBC, SetLastError=true)]
		public static extern void sync ();

		[DllImport (LIBC, SetLastError=true)]
		[Obsolete ("Dropped in POSIX 1003.1-2001.  " +
				"Use Unistd.sysconf (SysConf._SC_PAGESIZE).")]
		public static extern int getpagesize ();

		// truncate(2)
		//    int truncate(const char *path, off_t length);
		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_truncate")]
		public static extern int truncate (string path, long length);

		// ftruncate(2)
		//    int ftruncate(int fd, off_t length);
		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_ftruncate")]
		public static extern int ftruncate (int fd, long length);

		[DllImport (LIBC, SetLastError=true)]
		public static extern int getdtablesize ();

		[DllImport (LIBC, SetLastError=true)]
		public static extern int brk (IntPtr end_data_segment);

		[DllImport (LIBC, SetLastError=true)]
		public static extern IntPtr sbrk (IntPtr increment);

		// TODO: syscall(2)?
		// Probably safer to skip entirely.

		// lockf(3)
		//    int lockf(int fd, int cmd, off_t len);
		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_lockf")]
		public static extern int lockf (int fd, LockFlags cmd, long len);

		[DllImport (CRYPT, SetLastError=true, EntryPoint=CRYPT)]
		public static extern IntPtr sys_crypt (string key, string salt);

		public static string crypt (string key, string salt)
		{
			IntPtr r = sys_crypt (key, salt);
			return PosixMarshal.PtrToString (r);
		}

		[DllImport (CRYPT, SetLastError=true, EntryPoint="encrypt")]
		private static extern void sys_encrypt ([In, Out] byte[] block, int edflag);

		public static void encrypt (byte[] block, bool decode)
		{
			if (block.Length < 64)
				throw new ArgumentOutOfRangeException ("block", "Must refer to at least 64 bytes");
			sys_encrypt (block, decode ? 1 : 0);
		}

		// swab(3)
		//    void swab(const void *from, void *to, ssize_t n);
		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_swab")]
		public static extern void swab (IntPtr from, IntPtr to, long n);

		public static unsafe void swab (void* from, void* to, long n)
		{
			swab ((IntPtr) from, (IntPtr) to, n);
		}

		//
		// <utime.h>
		//

		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_utime")]
		public static extern int utime (string filename, ref Utimbuf buf);

		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_utime")]
		private static extern int utime (string filename, IntPtr buf);

		public static int utime (string filename)
		{
			return utime (filename, IntPtr.Zero);
		}

		[DllImport (MPH, SetLastError=true, 
				EntryPoint="Mono_Posix_Syscall_utimes")]
		public static extern int utimes (string filename, ref Timeval tvp);

		// Obsolete this after 1.2 is out:
		//[Obsolete ("Use Mono.Posix.Stdlib.strerror")]
		public static string strerror (int errnum)
		{
			IntPtr r = Stdlib.sys_strerror ((Error) errnum);
			return PosixMarshal.PtrToString (r);
		}


	}
}

// vim: noexpandtab
