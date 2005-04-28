//
// IRedirectOutput.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

//
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

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.JScript {

	[GuidAttribute("5B807FA1-00CD-46ee-A493-FD80AC944715")]
	[ComVisibleAttribute (true)]
	public interface IRedirectOutput
	{
		void SetOutputStream (IMessageReceiver output);
	}

	[GuidAttribute ("F062C7FB-53BF-4f0d-B0F6-D66C5948E63F")]
	[ComVisibleAttribute (true)]
	public interface IMessageReceiver 
	{
		void Message (string strValue);
	}

	public class COMCharStream : Stream {

		public COMCharStream (IMessageReceiver messageReceiver)
		{
			throw new NotImplementedException ();
		}
	
		public override bool CanWrite {
			get { throw new NotImplementedException (); }
		}
	
		public override bool CanRead {
			get { throw new NotImplementedException (); }
		}

		public override bool CanSeek {
			get { throw new NotImplementedException (); }
		}

		public override long Length {
			get { throw new NotImplementedException (); }
		}

		public override long Position {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public override void Close ()
		{
			throw new NotImplementedException ();
		}

		public override void Flush ()
		{		
			throw new NotImplementedException ();
		}

		public override int Read (byte [] buffer, int offset, int count)
		{
			throw new NotImplementedException ();
		}
	
		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotImplementedException ();
		}

		public override void SetLength (long value)
		{
			throw new NotImplementedException ();
		}

		public override void Write (byte [] buffer, int offset, int cont)
		{
			throw new NotImplementedException ();
		}
	}
}
