//
// System.Security.Cryptography CryptoStream.cs
//
// Author:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//   Sebastien Pouliot (spouliot@motus.com)
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
	
	public CryptoStream(Stream stream, ICryptoTransform transform, CryptoStreamMode mode) 
	{
		_stream = stream;
		_transform = transform;
		_mode = mode;
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
			throw new NotSupportedException("Length property not supported by CryptoStream");
		}
	}

	public override long Position {
		get {
			throw new NotSupportedException("Position property not supported by CryptoStream");
		}
		set {
			throw new NotSupportedException("Position property not supported by CryptoStream");
		}
	}

	public void Clear () 
	{
		Dispose (true);
	}

	[MonoTODO("Limited support for HMACSHA1")]
	public override void Close () 
	{
		if (_mode != CryptoStreamMode.Write)
			throw new NotSupportedException ();
		// TODO: limited implemention for HMACSHA1
		byte[] buffer = new byte [0];
		_transform.TransformFinalBlock (buffer, 0, 0);
		if (_stream != null)
			_stream.Close();
	}

	[MonoTODO]
	public override int Read (byte[] buffer, int offset, int count)
	{
		if (_mode != CryptoStreamMode.Read)
			throw new NotSupportedException ();
		if ((offset < 0) || (count < 0))
			throw new ArgumentOutOfRangeException ();
		if (offset + count > buffer.Length)
			throw new ArgumentException ();
		// TODO: implement
		return 0;
	}

	[MonoTODO("Limited support for HMACSHA1")]
	public override void Write (byte[] buffer, int offset, int count)
	{
		if (_mode != CryptoStreamMode.Write)
			throw new NotSupportedException ();
		if ((offset < 0) || (count < 0))
			throw new ArgumentOutOfRangeException ();
		if (offset + count > buffer.Length)
			throw new ArgumentException ();
		// TODO: limited implemention for HMACSHA1
		byte[] output = new byte [count];
		_transform.TransformBlock (buffer, offset, count, output, 0);
	}

	[MonoTODO]
	public override void Flush ()
	{
		if (_mode != CryptoStreamMode.Write)
			throw new NotSupportedException ("cannot flush a non-writeable CryptoStream");
		// TODO: implement
	}

	[MonoTODO]
	public void FlushFinalBlock ()
	{
		if (_mode != CryptoStreamMode.Write)
			throw new NotSupportedException ("cannot flush a non-writeable CryptoStream");
		// TODO: implement
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
	}
	
} // CryptoStream
	
} // System.Security.Cryptography
