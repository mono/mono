//
// System.Security.Cryptography CryptoStream.cs
//
// Authors:
//	Thomas Neidhart (tome@sbox.tugraz.at)
//	Sebastien Pouliot (spouliot@motus.com)
//
// Portions (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.IO;

namespace System.Security.Cryptography {

public class CryptoStream : Stream {
	private Stream _stream;
	private ICryptoTransform _transform;
	private CryptoStreamMode _mode;
	private byte[] workingBlock;
	private byte[] partialBlock;
	private int workPos;
	private bool disposed;
	private bool _flushedFinalBlock;
	private int blockSize;
	private int partialCount;
	
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
				blockSize = transform.InputBlockSize;
			else if (mode == CryptoStreamMode.Write)
				blockSize = transform.OutputBlockSize;
			workingBlock = new byte [blockSize];
			partialBlock = new byte [blockSize];
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

		// only flush in write mode (bugzilla 46143)
		if ((!_flushedFinalBlock) && (_mode == CryptoStreamMode.Write))
			FlushFinalBlock ();

		if (_stream != null)
			_stream.Close ();
	}

	private int ReadBlock (byte[] buffer, int offset, byte[] workspace) 
	{
		_stream.Read (workspace, 0, workspace.Length);
		if (_stream.Position == _stream.Length) {
			// last block
			byte[] input = _transform.TransformFinalBlock (workspace, 0, workspace.Length);
			Array.Copy (input, 0, buffer, offset, input.Length);
			// zeroize this last block
			Array.Clear (input, 0, input.Length);
			// return past blocks + last block size
			return input.Length;
		} 
		return _transform.TransformBlock (workspace, 0, workspace.Length, buffer, offset);
	}

	private int ReadBlocks (byte[] buffer, int offset, int numBlock) 
	{
		int result = 0;
		// if supported do a single transform, if not iterate for each block
		// but only if numBlock > 1 as we don't want to re-allocate memory for 1 block
		if ((numBlock > 1) && (_transform.CanTransformMultipleBlocks)) {
			int size = numBlock * blockSize;
			byte[] multiBlocks = new byte [size];
			result = ReadBlock (buffer, offset, multiBlocks);
			// zeroize data
			Array.Clear (multiBlocks, 0, size);
		}
		else {
			for (int i=0; i < numBlock; i++) {
				int written = ReadBlock (buffer, offset, workingBlock);
				result += written;
				offset += written;
			}
		}
		return result;
	}

	public override int Read (byte[] buffer, int offset, int count)
	{
		if (_mode != CryptoStreamMode.Read)
			throw new NotSupportedException ("not in Read mode");
		if (offset < 0) 
			throw new ArgumentOutOfRangeException ("offset", "negative");
		if (count < 0)
			throw new ArgumentOutOfRangeException ("count", "negative");
		if (offset + count > buffer.Length)
			throw new ArgumentException ("(offset+count)", "buffer overflow");

		// reached end of stream ?
		if (_stream.Position == _stream.Length)
			return 0;

		int result = 0;
		int bufferPos = offset;

		// is there a previous partial block to complete ?
		if (partialCount > 0) {
			// if yes, the copy this (already decrypted) block
			int remainder = (blockSize - partialCount);
			Array.Copy (partialBlock, partialCount, buffer, bufferPos, remainder);
			// zeroize the partial block
			Array.Clear (partialBlock, 0, blockSize);
			bufferPos += remainder;
			count -= remainder;
		}
		
		// read all complete blocks
		int written = ReadBlocks (buffer, bufferPos, (count / blockSize));
		bufferPos += written;
		result += written;
		
		// is there a partial block ?
		partialCount = (count % blockSize);
		if (partialCount > 0) {
			// if yes we must read the process the next entire block
			ReadBlocks (partialBlock, 0, 1);
			result += partialCount;
			// return a copy of the first part (as requested)
			Array.Copy (partialBlock, 0, buffer, bufferPos, partialCount);
			// and keep the partial block for "possible" next read (no zeroize)
		}

		return result;
	}

	public override void Write (byte[] buffer, int offset, int count)
	{
		if (_mode != CryptoStreamMode.Write)
			throw new NotSupportedException ("not in Write mode");
		if (offset < 0) 
			throw new ArgumentOutOfRangeException ("offset", "negative");
		if (count < 0)
			throw new ArgumentOutOfRangeException ("count", "negative");
		if (offset + count > buffer.Length)
			throw new ArgumentException ("(offset+count)", "buffer overflow");

		int bufferPos = offset;
		while (count > 0) {
			int len = Math.Min (workingBlock.Length - workPos, count);
			Array.Copy (buffer, bufferPos, workingBlock, workPos, len);
			bufferPos += len;
			workPos += len;
			count -= len;
			if (workPos == workingBlock.Length) {
				workPos = 0;
				byte[] output = new byte[_transform.OutputBlockSize];
				_transform.TransformBlock (workingBlock, 0, workingBlock.Length, output, 0);
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
		byte[] finalBuffer = _transform.TransformFinalBlock (workingBlock, 0, workPos);
		if (_stream != null) {
			_stream.Write (finalBuffer, 0, finalBuffer.Length);
			_stream.Flush ();
		}
		// zeroize
		Array.Clear (finalBuffer, 0, finalBuffer.Length);
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
			// always cleared for security reason
			Array.Clear (workingBlock, 0, workingBlock.Length);
			Array.Clear (partialBlock, 0, partialBlock.Length);
			if (disposing) {
				_stream = null;
				workingBlock = null;
				partialBlock = null;
			}
		}
	}
	
} // CryptoStream
	
} // System.Security.Cryptography
