// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//
// $Revision: 1.2 $
// $Modtime: $
// $Log: ImageListStreamer.cs,v $
// Revision 1.2  2004/08/11 22:20:59  pbartok
// - Signature fixes
//
// Revision 1.1  2004/07/15 20:05:28  pbartok
// - Implemented ImageList and ImageList.ImageCollection classes
// - Added ColorDepth enumeration
// - Updated SWF VS.Net project
//
//
//

// NOT COMPLETE

using System.Runtime.Serialization;

namespace System.Windows.Forms {
	[Serializable]
	public sealed class ImageListStreamer : ISerializable {
		#region Constructor
		private ImageListStreamer() {
		}
		#endregion	// Constructor

		#region Public Instance Methods
		[MonoTODO("Need to learn about streaming first - Peter")]
		public void GetObjectData(SerializationInfo si, StreamingContext context) {
			throw new NotImplementedException();
		}
		#endregion	// Public Instance Methods
	}
}
