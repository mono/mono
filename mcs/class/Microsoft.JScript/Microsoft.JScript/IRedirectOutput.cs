//
// IRedirectOutput.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
	using System;
	using System.IO;

	public interface IRedirectOutput
	{
		void SetOutputStream (IMessageReceiver output);
	}

	public interface IMessageReceiver 
	{
		void Message (string strValue);
	}

	public class COMCharStream : Stream
	{
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