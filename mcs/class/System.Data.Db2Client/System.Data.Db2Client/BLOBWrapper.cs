using System;

namespace System.Data.Db2Client{
	public class BLOBWrapper{
		byte[] _blob;
		int _length;

		
		public byte[] ByteArray{
			set{
				_length = ((byte[])value).Length;
				_blob = new byte[_length];
				System.Array.Copy((byte[])value, 0, _blob, 0, _length);
			}
		}
		
		public long GetBytes(long fieldOffset, byte[] buffer, int bufferOffset, int length){
			long start = fieldOffset * length;
			if (_length <= start){return 0;}
			int actuallength = (int)(((length+start)>_length)?(_length - start):length);
			System.Array.Copy(_blob, (int)start, buffer, (int)bufferOffset, actuallength);
			return (long)(((length+start)>_length)?(_length - start):length);
		}
	}
}
