//
// System.Security.Cryptography CryptoStream.cs
//
// Authors:
//	Thomas Neidhart (tome@sbox.tugraz.at)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Portions (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace System.Security.Cryptography {

	public class CryptoStream : Stream {
		private Stream _stream;
		private ICryptoTransform _transform;
		private CryptoStreamMode _mode;
		private byte[] _previousBlock;
		private byte[] _currentBlock;
		private bool _disposed;
		private bool _flushedFinalBlock;
		private int _blockSize;
		private int _partialCount;
		private bool _endOfStream;
	
		private byte[] _waitingBlock;
		private int _waitingCount;
	
		private byte[] _transformedBlock;
		private int _transformedPos;
		private int _transformedCount;
	
		private byte[] _workingBlock;
		private int _workingCount;
		
		public CryptoStream (Stream stream, ICryptoTransform transform, CryptoStreamMode mode)
		{
			if ((mode == CryptoStreamMode.Read) && (!stream.CanRead)) {
				throw new ArgumentException (
					Locale.GetText ("Can't read on stream"));
			}
			if ((mode == CryptoStreamMode.Write) && (!stream.CanWrite)) {
				throw new ArgumentException (
					Locale.GetText ("Can't write on stream"));
			}
			_stream = stream;
			_transform = transform;
			_mode = mode;
			_disposed = false;
			if (transform != null) {
				if (mode == CryptoStreamMode.Read)
					_blockSize = transform.InputBlockSize;
				else if (mode == CryptoStreamMode.Write)
					_blockSize = transform.OutputBlockSize;
				_workingBlock = new byte [_blockSize];
				_currentBlock = new byte [_blockSize];
			}
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
			get { throw new NotSupportedException ("Length"); }
		}
	
		public override long Position {
			get { throw new NotSupportedException ("Position"); }
			set { throw new NotSupportedException ("Position"); }
		}
	
		public void Clear () 
		{
			Dispose (true);
			GC.SuppressFinalize (this); // not called in Stream.Dispose
		}
	
		// LAMESPEC: A CryptoStream can be close in read mode
		public override void Close () 
		{
			// only flush in write mode (bugzilla 46143)
			if ((!_flushedFinalBlock) && (_mode == CryptoStreamMode.Write))
				FlushFinalBlock ();
	
			if (_stream != null)
				_stream.Close ();
		}
	
		public override int Read ([In,Out] byte[] buffer, int offset, int count)
		{
			if (_mode != CryptoStreamMode.Read) {
				throw new NotSupportedException (
					Locale.GetText ("not in Read mode"));
			}
			if (offset < 0) {
				throw new ArgumentOutOfRangeException ("offset", 
					Locale.GetText ("negative"));
			}
			if (count < 0) {
				throw new ArgumentOutOfRangeException ("count",
					Locale.GetText ("negative"));
			}
			// yes - buffer.Length will throw a NullReferenceException if buffer is null
			// but by doing so we match MS implementation
			if (offset + count > buffer.Length) {
				throw new ArgumentException ("(offset+count)", 
					Locale.GetText ("buffer overflow"));
			}
			// for some strange reason Object_disposedException isn't throw
			// instead we get a ArgumentNullException (probably from an internal method)
			if (_workingBlock == null) {
				throw new ArgumentNullException (
					Locale.GetText ("object _disposed"));
			}
	
			int result = 0;
			if ((count == 0) || ((_transformedPos == _transformedCount) && (_endOfStream)))
				return result;
	
			if (_waitingBlock == null) {
				_transformedBlock = new byte [_blockSize << 2];
				_transformedPos = 0;
				_transformedCount = 0;
				_waitingBlock = new byte [_blockSize];
				_waitingCount = _stream.Read (_waitingBlock, 0, _waitingBlock.Length);
			}
			
			while (count > 0) {
				// transformed but not yet returned
				int length = (_transformedCount - _transformedPos);
	
				// need more data - at least one full block must be available if we haven't reach the end of the stream
				if (length < _blockSize) {
					int transformed = 0;
	
					// load a new block
					_workingCount = _stream.Read (_workingBlock, 0, _workingBlock.Length);
					_endOfStream = (_workingCount < _blockSize);
	
					if (!_endOfStream) {
						// transform the waiting block
						transformed = _transform.TransformBlock (_waitingBlock, 0, _waitingBlock.Length, _transformedBlock, _transformedCount);
	
						// transfer temporary to waiting
						Buffer.BlockCopy (_workingBlock, 0, _waitingBlock, 0, _workingCount);
						_waitingCount = _workingCount;
					}
					else {
						if (_workingCount > 0) {
							// transform the waiting block
							transformed = _transform.TransformBlock (_waitingBlock, 0, _waitingBlock.Length, _transformedBlock, _transformedCount);
	
							// transfer temporary to waiting
							Buffer.BlockCopy (_workingBlock, 0, _waitingBlock, 0, _workingCount);
							_waitingCount = _workingCount;
	
							length += transformed;
							_transformedCount += transformed;
						}
						byte[] input = _transform.TransformFinalBlock (_waitingBlock, 0, _waitingCount);
						transformed = input.Length;
						Array.Copy (input, 0, _transformedBlock, _transformedCount, input.Length);
						// zeroize this last block
						Array.Clear (input, 0, input.Length);
					}
	
					length += transformed;
					_transformedCount += transformed;
				}
				// compaction
				if (_transformedPos > _blockSize) {
					Buffer.BlockCopy (_transformedBlock, _transformedPos, _transformedBlock, 0, length);
					_transformedCount -= _transformedPos;
					_transformedPos = 0;
				}
	
				length = ((count < length) ? count : length);
				Buffer.BlockCopy (_transformedBlock, _transformedPos, buffer, offset, length);
				_transformedPos += length;
	
				result += length;
				offset += length;
				count -= length;
	
				// there may not be enough data in the stream for a 
				// complete block
				if ((length != _blockSize) || (_endOfStream)) {
					count = 0;	// no more data can be read
				}
			}
			
			return result;
		}
	
		public override void Write (byte[] buffer, int offset, int count)
		{
			if (_mode != CryptoStreamMode.Write) {
				throw new NotSupportedException (
					Locale.GetText ("not in Write mode"));
			}
			if (offset < 0) { 
				throw new ArgumentOutOfRangeException ("offset", 
					Locale.GetText ("negative"));
			}
			if (count < 0) {
				throw new ArgumentOutOfRangeException ("count", 
					Locale.GetText ("negative"));
			}
			if (offset + count > buffer.Length) {
				throw new ArgumentException ("(offset+count)", 
					Locale.GetText ("buffer overflow"));
			}
	
			// partial block (in progress)
			if ((_partialCount > 0) && (_partialCount != _blockSize)) {
				int remainder = _blockSize - _partialCount;
				remainder = ((count < remainder) ? count : remainder);
				Buffer.BlockCopy (buffer, offset, _workingBlock, _partialCount, remainder);
				_partialCount += remainder;
				offset += remainder;
				count -= remainder;
			}
	
			int bufferPos = offset;
			while (count > 0) {
				if (_partialCount == _blockSize) {
					_partialCount = 0;
					// use partial block to avoid (re)allocation
					_transform.TransformBlock (_workingBlock, 0, _blockSize, _currentBlock, 0);
					_stream.Write (_currentBlock, 0, _currentBlock.Length);
				}
	
				if (_transform.CanTransformMultipleBlocks) {
					// transform all except the last block (which may be the last block
					// of the stream and require TransformFinalBlock)
					int numBlock = ((_partialCount + count) / _blockSize);
					if (((_partialCount + count) % _blockSize) == 0) // partial block ?
						numBlock--; // no then reduce
					int multiSize = (numBlock * _blockSize);
					if (numBlock > 0) {
						byte[] multiBlocks = new byte [multiSize];
						_transform.TransformBlock (buffer, offset, multiSize, multiBlocks, 0);
						_stream.Write (multiBlocks, 0, multiSize); 
						// copy last block into _currentBlock
						_partialCount = count - multiSize;
						Array.Copy (buffer, offset + multiSize, _workingBlock, 0, _partialCount);
					}
					else {
						Array.Copy (buffer, offset, _workingBlock, _partialCount, count);
						_partialCount += count;
					}
					count = 0; // the last block, if any, is in _workingBlock
				}
				else {
					int len = Math.Min (_blockSize - _partialCount, count);
					Array.Copy (buffer, bufferPos, _workingBlock, _partialCount, len);
					bufferPos += len;
					_partialCount += len;
					count -= len;
					// here block may be full, but we wont TransformBlock it until next iteration
					// so that the last block will be called in FlushFinalBlock using TransformFinalBlock
				}
			}
		}
	
		public override void Flush ()
		{
			if (_stream != null)
				_stream.Flush ();
		}
	
		public void FlushFinalBlock ()
		{
			if (_flushedFinalBlock) {
				throw new NotSupportedException (
					Locale.GetText ("This method cannot be called twice."));
			}
			if (_mode != CryptoStreamMode.Write) {
				throw new NotSupportedException (
					Locale.GetText ("cannot flush a non-writeable CryptoStream"));
			}
	
			_flushedFinalBlock = true;
			byte[] finalBuffer = _transform.TransformFinalBlock (_workingBlock, 0, _partialCount);
			if (_stream != null) {
				_stream.Write (finalBuffer, 0, finalBuffer.Length);
				_stream.Flush ();
			}
			// zeroize
			Array.Clear (finalBuffer, 0, finalBuffer.Length);
		}
	
		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotSupportedException ("Seek");
		}
		
		// LAMESPEC: Exception NotSupportedException not documented
		public override void SetLength (long value)
		{
			throw new NotSupportedException ("SetLength");
		}
	
		protected virtual void Dispose (bool disposing) 
		{
			if (!_disposed) {
				_disposed = true;
				// always cleared for security reason
				if (_workingBlock != null)
					Array.Clear (_workingBlock, 0, _workingBlock.Length);
				if (_currentBlock != null)
					Array.Clear (_currentBlock, 0, _currentBlock.Length);
				if (_previousBlock != null)
					Array.Clear (_previousBlock, 0, _previousBlock.Length);
				if (disposing) {
					_stream = null;
					_workingBlock = null;
					_currentBlock = null;
					_previousBlock = null;
				}
			}
		}
	}
}
