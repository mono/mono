//
// System.Net.SocketAddress.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Net.Sockets;

namespace System.Net {

	public class SocketAddress {
		private byte[] data;
		
		public SocketAddress (AddressFamily family, int size)
		{
			if(size<2) {
				throw new ArgumentOutOfRangeException("size is too small");
			}
			
			data=new byte[size];
			data[0]=(byte)family;
			data[1]=(byte) ((int) family >> 8);
		}

		public SocketAddress (AddressFamily family)
			: this (family, 32)
		{
		}
		
		//LAMESPEC: the MS doc about this class is wrong. The size is not stored in byte 1. Instead
		// byte [0] and byte [1] hold the family (little endian).
		public AddressFamily Family {
			get {
				return (AddressFamily) (data [0] + (data [1] << 8));
			}
		}

		public int Size {
			get {
				return data.Length;
			}
		}

		public byte this [ int offset ] {
			get {
				return(data[offset]);
			}

			set {
				data[offset]=value;
			}
		}

		public override string ToString() {
			string af=((AddressFamily)data[0]).ToString();
			int size = data.Length;
			string ret=af+":"+size+":{";
			
			for(int i=2; i<size; i++) {
				int val=(int)data[i];
				ret=ret+val;
				if(i<size-1) {
					ret=ret+",";
				}
			}
			
			ret=ret+"}";
			
			return(ret);
		}

		public override bool Equals (object obj)
		{
			if (obj is System.Net.SocketAddress &&
			    ((System.Net.SocketAddress) obj).data.Length == data.Length){
				byte [] otherData = ((System.Net.SocketAddress) obj).data;
				for (int i = 0; i < data.Length; i++)
					if (otherData [i] != data [i])
						return false;

				return true;
			}

			return false;
		}

		public override int GetHashCode ()
		{
			int code = 0;

			for (int i = 0; i < data.Length; i++)
				code += data [i] + i;

			return code;
		}
	}
}
