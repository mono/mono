using System;
using System.Security;
using System.Runtime.InteropServices;

namespace System.Data.SqlClient
{

	class SNINativeMethodWrapper
	{
		const string msg = "It is native method used by Microsoft System.Data implementation that Mono or non-Windows platform does not support.";

		public delegate void SqlAsyncCallbackDelegate (IntPtr h, IntPtr h2, uint i);

		public class ConsumerInfo
		{
			internal int defaultBufferSize;
			internal IntPtr key;
			internal SqlAsyncCallbackDelegate readDelegate;
			internal SqlAsyncCallbackDelegate writeDelegate;
		}
		
		public class SNI_Error
		{
			internal char [] errorMessage;
			internal string function;
			internal int lineNumber;
			internal uint nativeError;
			internal ProviderEnum provider;
			internal int sniError;
		}
		
		public enum IOType
		{
			WRITE
		}
		
		public enum ConsumerNumber
		{
			SNI_Consumer_SNI
		}
		
		public enum ProviderEnum
		{
			SMUX_PROV,
			SSL_PROV,
		}
		
		public enum QTypes
		{
			SNI_QUERY_LOCALDB_HMODULE,
			SNI_QUERY_CONN_BUFSIZE,
			SNI_QUERY_CLIENT_ENCRYPT_POSSIBLE,
		}
		
		public enum SniSpecialErrors
		{
			LocalDBErrorCode,
			MultiSubnetFailoverWithMoreThan64IPs,
			MultiSubnetFailoverWithInstanceSpecified,
			MultiSubnetFailoverWithNonTcpProtocol,
			MaxErrorValue,
		}
		
		public static int SniMaxComposedSpnLength {
			get { throw new NotSupportedException (msg); }
		}
		
		public static uint SNIInitialize ()
		{
			throw new NotSupportedException (msg);
		}
		
		public static uint SNITerminate ()
		{
			throw new NotSupportedException (msg);
		}

		public static uint SNISecInitPackage (ref uint maxLength)
		{
			throw new NotSupportedException (msg);
		}
		
		public static uint SNIReadAsync (SafeHandle handle, ref IntPtr data)
		{
			throw new NotSupportedException (msg);
		}
		
		public static uint SNIReadSyncOverAsync (SafeHandle handle, ref IntPtr data, int timeout)
		{
			throw new NotSupportedException (msg);
		}
		
		public static uint SNIOpen (ConsumerInfo info, SafeHandle handle, out IntPtr result, bool b)
		{
			throw new NotSupportedException (msg);
		}
		
		public static uint SNIOpenSyncEx (ConsumerInfo info, string serverName, ref IntPtr handle, byte [] spnBuffer, byte [] instanceName, bool flushCache, bool sync, int timeout, bool parallel)
		{
			throw new NotSupportedException (msg);
		}
		
		public static uint SNICheckConnection (SNIHandle handle)
		{
			throw new NotSupportedException (msg);
		}
		
		public static uint SNIClose (IntPtr result)
		{
			throw new NotSupportedException (msg);
		}
		
		public static uint SNIAddProvider (SafeHandle handle, ProviderEnum e, ref uint result)
		{
			throw new NotSupportedException (msg);
		}
			
		public static uint SNIRemoveProvider (SafeHandle handle, ProviderEnum e)
		{
			throw new NotSupportedException (msg);
		}
		
		public static uint SNISetInfo (SafeHandle handle, QTypes q, ref uint result)
		{
			throw new NotSupportedException (msg);
		}
		
		public static uint SniGetConnectionId (SafeHandle handle, ref System.Guid id)
		{
			throw new NotSupportedException (msg);
		}
		
		public static IntPtr SNIServerEnumOpen ()
		{
			throw new NotSupportedException (msg);
		}
		
		public static void SNIServerEnumClose (IntPtr handle)
		{
			throw new NotSupportedException (msg);
		}
		
		public static int SNIServerEnumRead (IntPtr handle, char [] buffer, int bufferSize, ref bool more)
		{
			throw new NotSupportedException (msg);
		}
		
		public static byte [] GetData ()
		{
			throw new NotSupportedException (msg);
		}
		
		public static void SetData (byte [] buffer)
		{
			throw new NotSupportedException (msg);
		}
		
		public static _AppDomain GetDefaultAppDomain ()
		{
			throw new NotSupportedException (msg);
		}

		public static void SNIPacketSetData (SafeHandle handle, byte[] data, int size)
		{
			throw new NotSupportedException (msg);
		}
		
		public static void SNIPacketSetData (SafeHandle handle, byte[] data, int size, SecureString [] securePasswords, int [] securePasswordOffsets)
		{
			throw new NotSupportedException (msg);
		}
		
		public static void SNIPacketReset (SafeHandle handle, IOType io, SafeHandle handle2, ConsumerNumber cn)
		{
			throw new NotSupportedException (msg);
		}
		
		public static uint SNISecGenClientContext (SafeHandle handle, byte [] bytes, uint size, byte[] bytes2, ref uint size2, byte[] bytes3)
		{
			throw new NotSupportedException (msg);
		}
		
		public static void SNIPacketAllocate (SafeHandle handle, IOType io, ref IntPtr result)
		{
			throw new NotSupportedException (msg);
		}
		
		public static void SNIPacketRelease (IntPtr packet)
		{
			throw new NotSupportedException (msg);
		}
		
		public static uint SNIPacketGetData (IntPtr packet, byte [] buffer, ref uint size)
		{
			throw new NotSupportedException (msg);
		}
		
		public static uint SNIWritePacket (SafeHandle handle, SNIPacket packet, bool sync)
		{
			throw new NotSupportedException (msg);
		}
		
		public static void SNIGetLastError (SNI_Error error)
		{
			throw new NotSupportedException (msg);
		}
		
		public static uint SNIOpenMarsSession (ConsumerInfo info, SNIHandle parent, ref IntPtr handle, bool sync)
		{
			throw new NotSupportedException (msg);
		}

		public static uint SNIWaitForSSLHandshakeToComplete (SafeHandle handle, int timeout)
		{
			throw new NotSupportedException (msg);
		}
		
		public static uint SNIQueryInfo (QTypes q, ref uint value)
		{
			throw new NotSupportedException (msg);
		}
		
		public static uint SNIQueryInfo (QTypes q, ref IntPtr value)
		{
			throw new NotSupportedException (msg);
		}
	}
}
