//
// System.Security.Cryptography CryptoStream.cs
//
// Authors:
//	Thomas Neidhart (tome@sbox.tugraz.at)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// Portions (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005, 2007 Novell, Inc (http://www.novell.com)
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

using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace System.Security.Cryptography {

	[ComVisible (true)]
	public class CryptoStream : Stream {
		private Stream _stream;
		private ICryptoTransform _transform;
		private CryptoStreamMode _mode;
		private byte[] _currentBlock;
		private bool _disposed;
		private bool _flushedFinalBlock;
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
			if (mode == CryptoStreamMode.Read) {
				if (!stream.CanRead)
					throw new ArgumentException (Locale.GetText ("Can't read on stream"));
			} else if (mode == CryptoStreamMode.Write) {
				if (!stream.CanWrite)
					throw new ArgumentException (Locale.GetText ("Can't write on stream"));
			} else {
				throw new ArgumentException ("mode");
			}
			_stream = stream;
			_transform = transform;
			_mode = mode;
			_disposed = false;
			if (transform != null) {
				_workingBlock = new byte [transform.InputBlockSize];
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
			Close ();
		}

		// LAMESPEC: A CryptoStream can be close in read mode
		public override void Close () 
		{
			base.Close ();
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
			// re-ordered to avoid integer overflow
			if (offset > buffer.Length - count) {
				throw new ArgumentException ("(offset+count)", 
					Locale.GetText ("buffer overflow"));
			}
			// for some strange reason ObjectDisposedException isn't throw
			if (_workingBlock == null) {
				return 0;
			}

			int result = 0;
			if ((count == 0) || ((_transformedPos == _transformedCount) && (_endOfStream)))
				return result;

			if (_waitingBlock == null) {
				_transformedBlock = new byte [_transform.OutputBlockSize << 2];
				_transformedPos = 0;
				_transformedCount = 0;
				_waitingBlock = new byte [_transform.InputBlockSize];
				_waitingCount = _stream.Read (_waitingBlock, 0, _waitingBlock.Length);
			}
			
			while (count > 0) {
				// transformed but not yet returned
				int length = (_transformedCount - _transformedPos);

				// need more data - at least one full block must be available if we haven't reach the end of the stream
				if (length < _transform.InputBlockSize) {
					int transformed = 0;

					// load a new block
					_workingCount = _stream.Read (_workingBlock, 0, _transform.InputBlockSize);
					_endOfStream = (_workingCount < _transform.InputBlockSize);

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
						if (!_flushedFinalBlock) {
							byte[] input = _transform.TransformFinalBlock (_waitingBlock, 0, _waitingCount);
							transformed = input.Length;
							Buffer.BlockCopy (input, 0, _transformedBlock, _transformedCount, input.Length);
							// zeroize this last block
							Array.Clear (input, 0, input.Length);
							_flushedFinalBlock = true;
						}
					}

					length += transformed;
					_transformedCount += transformed;
				}
				// compaction
				if (_transformedPos > _transform.OutputBlockSize) {
					Buffer.BlockCopy (_transformedBlock, _transformedPos, _transformedBlock, 0, length);
					_transformedCount -= _transformedPos;
					_transformedPos = 0;
				}

				length = ((count < length) ? count : length);
				if (length > 0) {
					Buffer.BlockCopy (_transformedBlock, _transformedPos, buffer, offset, length);
					_transformedPos += length;

					result += length;
					offset += length;
					count -= length;
				}

				// there may not be enough data in the stream for a 
				// complete block
				if (((length != _transform.InputBlockSize) && (_waitingCount != _transform.InputBlockSize)) || (_endOfStream)) {
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
			// re-ordered to avoid integer overflow
			if (offset > buffer.Length - count) {
				throw new ArgumentException ("(offset+count)", 
					Locale.GetText ("buffer overflow"));
			}

			if (_stream == null)
				throw new ArgumentNullException ("inner stream was disposed");

			int buffer_length = count;

			// partial block (in progress)
			if ((_partialCount > 0) && (_partialCount != _transform.InputBlockSize)) {
				int remainder = _transform.InputBlockSize - _partialCount;
				remainder = ((count < remainder) ? count : remainder);
				Buffer.BlockCopy (buffer, offset, _workingBlock, _partialCount, remainder);
				_partialCount += remainder;
				offset += remainder;
				count -= remainder;
			}

			int bufferPos = offset;
			while (count > 0) {
				if (_partialCount == _transform.InputBlockSize) {
					if (_currentBlock == null)
						_currentBlock = new byte [_transform.OutputBlockSize];

					// use partial block to avoid (re)allocation
					int len = _transform.TransformBlock (_workingBlock, 0, _partialCount, _currentBlock, 0);
					_stream.Write (_currentBlock, 0, len);
					// reset
					_partialCount = 0;
				}

				if (_transform.CanTransformMultipleBlocks) {
					// get the biggest multiple of InputBlockSize in count (without mul or div)
					int size = (count & ~(_transform.InputBlockSize - 1));
					int rem = (count & (_transform.InputBlockSize - 1));
					// avoid reallocating memory at each call (reuse same buffer whenever possible)
					int sizeWorkingBlock = (1 + size / _transform.InputBlockSize) * _transform.OutputBlockSize;
					if (_workingBlock.Length < sizeWorkingBlock) {
						Array.Clear (_workingBlock, 0, _workingBlock.Length);
						_workingBlock = new byte [sizeWorkingBlock];
					}

					if (size > 0) {
						int len = _transform.TransformBlock (buffer, offset, size, _workingBlock, 0);
						_stream.Write (_workingBlock, 0, len);
					}

					if (rem > 0)
						Buffer.BlockCopy (buffer, buffer_length - rem, _workingBlock, 0, rem);
					_partialCount = rem;
					count = 0; // the last block, if any, is in _workingBlock
				} else {
					int len = Math.Min (_transform.InputBlockSize - _partialCount, count);
					Buffer.BlockCopy (buffer, bufferPos, _workingBlock, _partialCount, len);
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
		}

		public void FlushFinalBlock ()
		{
			if (_flushedFinalBlock)
				throw new NotSupportedException (Locale.GetText ("This method cannot be called twice."));
			if (_disposed)
				throw new NotSupportedException (Locale.GetText ("CryptoStream was disposed."));

			_flushedFinalBlock = true;
			byte[] finalBuffer = _transform.TransformFinalBlock (_workingBlock, 0, _partialCount);
			if (_stream != null && _mode == CryptoStreamMode.Write) {
				_stream.Write (finalBuffer, 0, finalBuffer.Length);
			}
			if (_stream is CryptoStream) {
				// for cascading crypto streams
				(_stream as CryptoStream).FlushFinalBlock ();
			} else {
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

		protected override void Dispose (bool disposing) 
		{
			if (!_disposed) {
				if (disposing) {
					if (!_flushedFinalBlock) {
						FlushFinalBlock ();
					}

					if (_stream != null)
						_stream.Close ();
				}
				_disposed = true;
				// always cleared for security reason
				if (_workingBlock != null)
					Array.Clear (_workingBlock, 0, _workingBlock.Length);
				if (_currentBlock != null)
					Array.Clear (_currentBlock, 0, _currentBlock.Length);
				if (disposing) {
					_stream = null;
					_workingBlock = null;
					_currentBlock = null;
				}
			}
		}
	}
}
