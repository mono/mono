namespace System.Net {

	partial class SocketAddress
	{
		internal SocketAddress (byte[] buffer, int size)
		{
			InternalSize = size;
			Buffer = new byte[(size/IntPtr.Size+2)*IntPtr.Size];//sizeof DWORD

			global::System.Buffer.BlockCopy (buffer, 0, Buffer, 0, buffer.Length);
		}
	}
}
