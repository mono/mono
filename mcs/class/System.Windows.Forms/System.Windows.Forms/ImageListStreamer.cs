//
// System.Windows.Forms.ImageListStreamer.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Runtime.Serialization;

namespace System.Windows.Forms {

	// <summary>
	// </summary>
	[Serializable]
	public sealed class ImageListStreamer : ISerializable {
		IntPtr himl;
		ImageList imageList;

		public ImageListStreamer ( ImageList list )
		{
		}

		//Deserialization constructor.
		private ImageListStreamer (SerializationInfo info, StreamingContext context) {
			Byte[] data = ( Byte[] )info.GetValue( "Data", typeof( Byte[]) );
			if ( data != null && data.Length >=4 ) {
				// check the header ( 'MSFt' )
				if ( data[0] == 77 && data[1] == 83 && data[2] == 70 && data[3] == 116 ) {
//					ushort usMagic   ;
//					ushort usVersion ;
//					ushort cCurImage ;
//					ushort cMaxImage ;
//					ushort cGrow     ;
//					ushort cx        ;
//					ushort cy        ;
				}
			}
		}

		[MonoTODO]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context){
		}
	}
}
