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
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

	public sealed class ImageListStreamer : ISerializable {

		//
		//  --- Public Methods
		//
		[MonoTODO]
		public override bool Equals(object o) {
			throw new NotImplementedException ();
		}
		//public static bool Equals(object o1, object o2) {
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}
		[MonoTODO]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context){
			throw new NotImplementedException ();
		}
	}
}
