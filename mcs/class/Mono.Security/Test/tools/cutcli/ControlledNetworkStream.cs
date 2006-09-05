using System;
using System.IO;
using System.Net.Sockets;

namespace Mono.Test {

	public class ControlledNetworkStream : NetworkStream {

		private int _maxRead;
		private int _maxWrite;
		private int _curRead;
		private int _curWrite;

		public ControlledNetworkStream (Socket socket) 
			: base (socket)
		{
			_maxRead = -1;
			_maxWrite = -1;
			_curRead = 0;
			_curWrite = 0;
		}

		public ControlledNetworkStream (Socket socket, bool ownsSocket)
			: base (socket, ownsSocket)
		{
			_maxRead = -1;
			_maxWrite = -1;
			_curRead = 0;
			_curWrite = 0;
		}

		// properties

		public int CurrentRead {
			get { return _curRead; }
		}

		public int CurrentWrite {
			get { return _curWrite; }
		}

		public int MaximumRead {
			get { return _maxRead; }
			set { _maxRead = value; }
		}

		public int MaximumWrite {
			get { return _maxWrite; }
			set { _maxWrite = value; }
		}

		// methods

		public override IAsyncResult BeginRead (byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			return base.BeginRead (buffer, offset, PreCheckRead (size), callback, state);
		}

		public override int EndRead (IAsyncResult asyncResult)
		{
			return PostCheckRead (base.EndRead (asyncResult));
		}

		public override int Read (byte[] buffer, int offset, int size)
		{
			return PostCheckRead (base.Read (buffer, offset, PreCheckRead (size)));
		}

		public override IAsyncResult BeginWrite (byte[] buffer, int offset, int size, AsyncCallback callback, object state)
		{
			return base.BeginWrite (buffer, offset, PreCheckWrite (size), callback, state);
		}

		public override void EndWrite (IAsyncResult asyncResult)
		{
			PostCheckWrite ();
			base.EndWrite (asyncResult);
		}

		public override void Write (byte[] buffer, int offset, int size)
		{
			base.Write (buffer, offset, PreCheckWrite (size));
			PostCheckWrite ();
		}

		// internal stutff

		private int PreCheckRead (int size) 
		{
			if (_maxRead < 0)
				return size;

			if (_curRead + size > _maxRead)
				size = _maxRead - _curRead;

			return size;
		}

		private int PostCheckRead (int size)
		{
			_curRead += size;
			return size;
		}

		private int PreCheckWrite (int size)
		{
			if (_maxWrite < 0)
				return size;

			if (_curWrite + size > _maxWrite)
				size = _maxWrite - _curWrite;

			return size;
		}

		private void PostCheckWrite ()
		{
		}
	}
}
