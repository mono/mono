using System;
using System.Collections;

namespace System.Data.Db2Client{
	public class BLOBWrapperCollection{

		private Hashtable _blobs = new Hashtable();
		
		public void Add(int ColumnNumber){
			BLOBWrapper _blob = new BLOBWrapper();
			_blobs.Add(ColumnNumber, _blob);
		}
		
		public BLOBWrapper this[int index]{
			get{
				return (BLOBWrapper)_blobs[index];
			}
		}
		
		public long GetBytes(int col, long fieldOffset, byte[] buffer, int bufferOffset, int length){
			BLOBWrapper _blob = (BLOBWrapper)_blobs[col];
			return _blob.GetBytes(fieldOffset, buffer, bufferOffset, length);
		}
	}
}
