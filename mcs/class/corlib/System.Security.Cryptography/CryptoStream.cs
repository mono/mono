//
// System.Security.Cryptography CryptoStream.cs
//
// Authors:
//	Thomas Neidhart (tome@sbox.tugraz.at)
//	Sebastien Pouliot (spouliot@motus.com)
//
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.IO;

namespace System.Security.Cryptography {

public class CryptoStream : Stream {
	private Stream _stream;
	private ICryptoTransform _transform;
	private CryptoStreamMode _mode;
	private byte[] work;
	private int workPos;
	private bool disposed;
	private bool _flushedFinalBlock;
	
	public CryptoStream (Stream stream, ICryptoTransform transform, CryptoStreamMode mode)
	{
		if ((mode == CryptoStreamMode.Read) && (!stream.CanRead))
			throw new ArgumentException ("Can't read on stream");
		if ((mode == CryptoStreamMode.Write) && (!stream.CanWrite))
			throw new ArgumentException ("Can't write on stream");
		_stream = stream;
		_transform = transform;
		_mode = mode;
		disposed = false;
		if (transform != null) {
			if (mode == CryptoStreamMode.Read)
				work = new byte [transform.InputBlockSize];
			else if (mode == CryptoStreamMode.Write)
				work = new byte [transform.OutputBlockSize];
		}
		workPos = 0;
	}

	~CryptoStream () 
	{
		Dispose (false);
	}
	
	public override bool CanRead {
		get { return (_mode == CryptoStreamMode.Read); }
	}

	public override bool CanSeek {
		get { return false; }
	}

	public override bool CanWrite {
		get { return (_mode == CryptoStreamMode.Write); }
	}
	
	public override long Length {
		get {
			throw new NotSupportedException ("Length property not supported by CryptoStream");
		}
	}

	public override long Position {
		get {
			throw new NotSupportedException ("Position property not supported by CryptoStream");
		}
		set {
			throw new NotSupportedException ("Position property not supported by CryptoStream");
		}
	}

	public void Clear () 
	{
		Dispose (true);
	}

	public override void Close () 
	{
		// LAMESPEC: A CryptoStream can be close in read mode
		//if (_mode != CryptoStreamMode.Write)
		//	throw new NotSupportedException ();

		if (!_flushedFinalBlock)
			FlushFinalBlock ();

		if (_stream != null)
			_stream.Close ();
	}

	public override int Read (byte[] buffer, int offset, int count)
	{
		if (_mode != CryptoStreamMode.Read)
			throw new NotSupportedException ();
		if ((offset < 0) || (count < 0))
			throw new ArgumentOutOfRangeException ();
		if (offset + count > buffer.Length)
			throw new ArgumentException ();
		// reached end of stream ?
		if (_stream.Position == _stream.Length)
			return 0;

		int result = 0;
		int bufferPos = offset;
		while (count > 0) {
			int len = Math.Min (work.Length - workPos, count);
			_stream.Read (work, workPos, len);
			workPos += len;
			count -= len;
			if (_stream.Position == _stream.Length) {
				_flushedFinalBlock = true; // in case Close is called
				byte[] input = _transform.TransformFinalBlock (work, 0, work.Length);
				Array.Copy (input, 0, buffer, bufferPos, input.Length);
				result += input.Length;
				break;
			} else if (workPos == work.Length) {
				workPos = 0;
				result += _transform.TransformBlock (work, 0, work.Length, buffer, bufferPos);
			}
			bufferPos += len;
		}
		return result;
	}

	public override void Write (byte[] buffer, int offset, int count)
	{
		if (_mode != CryptoStreamMode.Write)
			throw new NotSupportedException ();
		if ((offset < 0) || (count < 0))
			throw new ArgumentOutOfRangeException ();
		if (offset + count > buffer.Length)
			throw new ArgumentException ();

		int bufferPos = offset;
		while (count > 0) {
			int len = Math.Min (work.Length - workPos, count);
			Array.Copy (buffer, bufferPos, work, workPos, len);
			bufferPos += len;
			workPos += len;
			count -= len;
			if (workPos == work.Length) {
				workPos = 0;
				byte[] output = new byte[_transform.OutputBlockSize];
				_transform.TransformBlock (work, 0, work.Length, output, 0);
				_stream.Write (output, 0, output.Length);
			}
		}
	}

	public override void Flush ()
	{
		if (_mode != CryptoStreamMode.Write)
			throw new NotSupportedException ("cannot flush a non-writeable CryptoStream");

		if (_stream != null)
			_stream.Flush ();
	}

	public void FlushFinalBlock ()
	{
		if (_flushedFinalBlock)
			throw new NotSupportedException ("This method cannot be called twice.");

		if (_mode != CryptoStreamMode.Write)
			throw new NotSupportedException ("cannot flush a non-writeable CryptoStream");

		_flushedFinalBlock = true;
		byte[] finalBuffer = _transform.TransformFinalBlock (work, 0, workPos);
		if (_stream != null) {
			_stream.Write (finalBuffer, 0, finalBuffer.Length);
			_stream.Flush ();
		}
	}

	public override long Seek (long offset, SeekOrigin origin)
	{
		throw new NotSupportedException ("cannot Seek a CryptoStream");
	}
	
	// LAMESPEC: Exception NotSupportedException not documented
	public override void SetLength (long value)
	{
		throw new NotSupportedException ("cannot SetLength a CryptoStream");
	}

	protected virtual void Dispose (bool disposing) 
	{
		if (!disposed) {
			if (_stream != null)
				_stream.Close ();
		}
	}
	
} // CryptoStream
	
} // System.Security.Cryptography
