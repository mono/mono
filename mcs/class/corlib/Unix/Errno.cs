//
// Errno.cs: used to provide better information
//

namespace System.Private {

	internal class Errno {

		internal static string Message (int code)
		{
			switch (code) {
			case Wrapper.EPERM:
				return "Operation not permitted";
			case Wrapper.ENOENT:
				return "No such file or directory";
			case Wrapper.EISDIR:
				return "Is a directory";
			case Wrapper.EBADF:
				return "Bad file descriptor";
			case Wrapper.ENOMEM:
				return "Cannot allocate memory";
			case Wrapper.EEXIST:
				return "File already exists";
			case Wrapper.ENOTEMPTY:
				return "Directory not empty";
			case Wrapper.ESRCH:
				return "No such process";
			case Wrapper.EINTR:
				return "Interrupted function call";
			case Wrapper.EIO:
				return "Input/output error";
			case Wrapper.ENXIO:
				return "No such device or address";
			case Wrapper.E2BIG:
				return "Argument list too long";
			case Wrapper.ENOEXEC:
				return "Exec format error";
			case Wrapper.ECHILD:
				return "No child processes";
			case Wrapper.EAGAIN:
				return "Resource temporarily unavailable";
			case Wrapper.EACCES:
				return "Permission denied";
			case Wrapper.EFAULT:
				return "Bad address";
			case Wrapper.ENOTBLK:
				return "Block device required";
			case Wrapper.EBUSY:
				return "Device or resource busy";
			case Wrapper.EXDEV:
				return "Invalid cross-device link";
			case Wrapper.ENODEV:
				return "No such device";
			case Wrapper.ENOTDIR:
				return "Not a directory";
			case Wrapper.EINVAL:
				return "Invalid argument";
			case Wrapper.ENFILE:
				return "Too many open files in system";
			case Wrapper.EMFILE:
				return "Too many open files";
			case Wrapper.ENOTTY:
				return "Inappropriate ioctl for device";
			case Wrapper.ETXTBSY:
				return "Text file busy";
			case Wrapper.EFBIG:
				return "File too large";
			case Wrapper.ENOSPC:
				return "No space left on device";
			case Wrapper.ESPIPE:
				return "Illegal seek";
			case Wrapper.EROFS:
				return "Read-only file system";
			case Wrapper.EMLINK:
				return "Too many links";
			case Wrapper.EPIPE:
				return "Broken pipe";
			case Wrapper.EDOM:
				return "Numerical argument out of domain";
			case Wrapper.ERANGE:
				return "Numerical result out of range";
			case Wrapper.EDEADLK:
				return "Resource deadlock avoided";
			case Wrapper.ENAMETOOLONG:
				return "File name too long";
			case Wrapper.ENOLCK:
				return "No locks available";
			case Wrapper.ENOSYS:
				return "Function not implemented";
			case Wrapper.ELOOP:
				return "Too many levels of symbolic links";
			case Wrapper.EUSERS:
				return "Too many users";
			case Wrapper.ENOTSOCK:
				return "Socket operation on non-socket";
			case Wrapper.EDESTADDRREQ:
				return "Destination address required";
			case Wrapper.EMSGSIZE:
				return "Message too long";
			case Wrapper.EPROTOTYPE:
				return "Protocol wrong type for socket";
			case Wrapper.ENOPROTOOPT:
				return "Protocol not available";
			case Wrapper.EPROTONOSUPPORT:
				return "Protocol not supported";
			case Wrapper.ESOCKTNOSUPPORT:
				return "Socket type not supported";
			case Wrapper.EOPNOTSUPP:
				return "Operation not supported";
			case Wrapper.EPFNOSUPPORT:
				return "Protocol family not supported";
			case Wrapper.EAFNOSUPPORT:
				return "Address family not supported by protocol";
			case Wrapper.EADDRINUSE:
				return "Address already in use";
			case Wrapper.EADDRNOTAVAIL:
				return "Cannot assign requested address";
			case Wrapper.ENETDOWN:
				return "Network is down";
			case Wrapper.ENETUNREACH:
				return "Network is unreachable";
			case Wrapper.ENETRESET:
				return "Network dropped connection on reset";
			case Wrapper.ECONNABORTED:
				return "Software caused connection abort";
			case Wrapper.ECONNRESET:
				return "Connection reset by peer";
			case Wrapper.ENOBUFS:
				return "No buffer space available";
			case Wrapper.EISCONN:
				return "Transport endpoint is already connected";
			case Wrapper.ENOTCONN:
				return "Transport endpoint is not connected";
			case Wrapper.ESHUTDOWN:
				return "Cannot send after transport endpoint shutdown";
			case Wrapper.ETOOMANYREFS:
				return "Too many references: cannot splice";
			case Wrapper.ETIMEDOUT:
				return "Connection timed out";
			case Wrapper.ECONNREFUSED:
				return "Connection refused";
			case Wrapper.EHOSTDOWN:
				return "Host is down";
			case Wrapper.EHOSTUNREACH:
				return "No route to host";
			case Wrapper.EALREADY:
				return "Operation already in progress";
			case Wrapper.EINPROGRESS:
				return "Operation now in progress";
			case Wrapper.ESTALE:
				return "Stale NFS file handle";
			case Wrapper.EDQUOT:
				return "Disk quota exceeded";
			case Wrapper.ENOMEDIUM:
			case Wrapper.ENOMSG:
			case Wrapper.EIDRM:
			case Wrapper.ECHRNG:
			case Wrapper.EL2NSYNC:
			case Wrapper.EL3HLT:
			case Wrapper.EL3RST:
			case Wrapper.ELNRNG:
			case Wrapper.EUNATCH:
			case Wrapper.ENOCSI:
			case Wrapper.EL2HLT:
			case Wrapper.EBADE:
			case Wrapper.EBADR:
			case Wrapper.EXFULL:
			case Wrapper.ENOANO:
			case Wrapper.EBADRQC:
			case Wrapper.EBADSLT:
			case Wrapper.EBFONT:
			case Wrapper.ENOSTR:
			case Wrapper.ENODATA:
			case Wrapper.ETIME:
			case Wrapper.ENOSR:
			case Wrapper.ENONET:
			case Wrapper.ENOPKG:
			case Wrapper.EREMOTE:
			case Wrapper.ENOLINK:
			case Wrapper.EADV:
			case Wrapper.ESRMNT:
			case Wrapper.ECOMM:
			case Wrapper.EPROTO:
			case Wrapper.EMULTIHOP:
			case Wrapper.EDOTDOT:
			case Wrapper.EBADMSG:
			case Wrapper.ENOTUNIQ:
			case Wrapper.EBADFD:
			case Wrapper.EREMCHG:
			case Wrapper.ELIBACC:
			case Wrapper.ELIBBAD:
			case Wrapper.ELIBSCN:
			case Wrapper.ELIBMAX:
			case Wrapper.ELIBEXEC:
				break;
			}
			return String.Format ("Errno code={0}", code);
		}
	}
}
