using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal static partial class Interop
{
	internal static partial class Sys
	{
		internal static unsafe Error Accept (SafeHandle safeHandle, byte *socketAddress, int *socketAddressLen, IntPtr *acceptedFd)
		{
			var socketHandle = (SafeSocketHandle)safeHandle;
			try {
				socketHandle.RegisterForBlockingSyscall ();
				var errno = Socket_Accept_internal (socketHandle.DangerousGetHandle (), socketAddress, socketAddressLen, acceptedFd);
				return Interop.Sys.ConvertErrorPlatformToPal (errno);
			} finally {
				socketHandle.UnRegisterForBlockingSyscall ();
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern unsafe int Socket_Accept_internal (IntPtr socket, byte* socketAddress, int* socketAddressLen, IntPtr* acceptedFd);

		internal static unsafe Error ReceiveMessage (SafeHandle safeHandle, MessageHeader* messageHeader, SocketFlags flags, long* received)
		{
			var socketHandle = (SafeSocketHandle)safeHandle;
			try {
				socketHandle.RegisterForBlockingSyscall ();
				var errno = Socket_ReceiveMessage_internal (socketHandle.DangerousGetHandle (), messageHeader, flags, received);
				return Interop.Sys.ConvertErrorPlatformToPal (errno);
			} finally {
				socketHandle.UnRegisterForBlockingSyscall ();
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern unsafe int Socket_ReceiveMessage_internal (IntPtr socket, MessageHeader* messageHeader, SocketFlags flags, long* received);

		internal static unsafe Error GetBytesAvailable (SafeHandle safeHandle, int* available)
		{
			var socketHandle = (SafeSocketHandle)safeHandle;
			try {
				socketHandle.RegisterForBlockingSyscall ();
				var errno = Socket_GetBytesAvailable_internal (socketHandle.DangerousGetHandle (), available);
				return Interop.Sys.ConvertErrorPlatformToPal (errno);
			} finally {
				socketHandle.UnRegisterForBlockingSyscall ();
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern unsafe int Socket_GetBytesAvailable_internal (IntPtr socket, int* available);

		internal static int GetControlMessageBufferSize (bool isIPv4, bool isIPv6)
		{
			throw new NotImplementedException ();
		}

		internal static bool PlatformSupportsDualModeIPv4PacketInfo ()
		{
			return true;
		}

	}
}
