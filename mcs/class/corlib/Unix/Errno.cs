//
// Errno.cs: used to provide better information
//

namespace System.Private {

	internal class Errno {

		internal static string Message (int code)
		{
			switch (code){
			case Wrapper.EPERM:
				return "No permission";
			case Wrapper.ENOENT:
				return "The name does not exist";
			case Wrapper.EISDIR:
				return "error: Is a directory";
			case Wrapper.EBADF:
				return "Bad file descriptor";
			case Wrapper.ENOMEM:
				return "Out of memory";
			case Wrapper.EEXIST:
				return "File already exists";
			case Wrapper.ENOTEMPTY:
				return "Directory is not empty";
				
			case Wrapper.ESRCH:
			case Wrapper.EINTR:
			case Wrapper.EIO:
			case Wrapper.ENXIO:
			case Wrapper.E2BIG:
			case Wrapper.ENOEXEC:
			case Wrapper.ECHILD:
			case Wrapper.EAGAIN:
			case Wrapper.EACCES:
			case Wrapper.EFAULT:
			case Wrapper.ENOTBLK:
			case Wrapper.EBUSY:
			case Wrapper.EXDEV:
			case Wrapper.ENODEV:
			case Wrapper.EINVAL:
			case Wrapper.ENFILE:
			case Wrapper.EMFILE:
			case Wrapper.ENOTTY:
			case Wrapper.ETXTBSY:
			case Wrapper.EFBIG:
			case Wrapper.ENOSPC:
			case Wrapper.ESPIPE:
			case Wrapper.EROFS:
			case Wrapper.EMLINK:
			case Wrapper.EPIPE:
			case Wrapper.EDOM:
			case Wrapper.ERANGE:
			case Wrapper.EDEADLK:
			case Wrapper.ENAMETOOLONG:
			case Wrapper.ENOLCK:
			case Wrapper.ENOSYS:
			case Wrapper.ELOOP:
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
			case Wrapper.EUSERS:
			case Wrapper.ENOTSOCK:
			case Wrapper.EDESTADDRREQ:
			case Wrapper.EMSGSIZE:
			case Wrapper.EPROTOTYPE:
			case Wrapper.ENOPROTOOPT:
			case Wrapper.EPROTONOSUPPORT:
			case Wrapper.ESOCKTNOSUPPORT:
			case Wrapper.EOPNOTSUPP:
			case Wrapper.EPFNOSUPPORT:
			case Wrapper.EAFNOSUPPORT:
			case Wrapper.EADDRINUSE:
			case Wrapper.EADDRNOTAVAIL:
			case Wrapper.ENETDOWN:
			case Wrapper.ENETUNREACH:
			case Wrapper.ENETRESET:
			case Wrapper.ECONNABORTED:
			case Wrapper.ECONNRESET:
			case Wrapper.ENOBUFS:
			case Wrapper.EISCONN:
			case Wrapper.ENOTCONN:
			case Wrapper.ESHUTDOWN:
			case Wrapper.ETOOMANYREFS:
			case Wrapper.ETIMEDOUT:
			case Wrapper.ECONNREFUSED:
			case Wrapper.EHOSTDOWN:
			case Wrapper.EHOSTUNREACH:
			case Wrapper.EALREADY:
			case Wrapper.EINPROGRESS:
			case Wrapper.ESTALE:
			case Wrapper.EDQUOT:
			case Wrapper.ENOMEDIUM:
				break;
				
			case Wrapper.ENOTDIR:
				return "Not a directory";
			}
			return String.Format ("Errno code={0}", code);
		}
	}
}
