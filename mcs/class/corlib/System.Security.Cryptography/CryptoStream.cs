//
// System.Security.Cryptography CryptoStream.cs
//
// Author:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//

using System;
using System.IO;

namespace System.Security.Cryptography
{

	public class CryptoStream : Stream
	{
		private CryptoStreamMode _mode;
		
		public CryptoStream(Stream stream, ICryptoTransform transform, CryptoStreamMode mode) 
		{
			_mode = mode;
		}
		
		public override bool CanRead
		{
			get {
				switch (_mode) {
					case CryptoStreamMode.Read:
						return true;
					
					case CryptoStreamMode.Write:
						return false;
					
					default:
						return false;
				}
			}
		}

		public override bool CanSeek
		{
			get {
				return false;
			}
		}

		public override bool CanWrite
		{
			get {
				switch (_mode) {
					case CryptoStreamMode.Read:
						return false;
					
					case CryptoStreamMode.Write:
						return true;
					
					default:
						return false;
				}
			}
		}
		
		public override long Length
		{
			get {
				throw new NotSupportedException("Length property not supported by CryptoStream");
			}
		}

		public override long Position
		{
			get {
				throw new NotSupportedException("Position property not supported by CryptoStream");
			}
			set {
				throw new NotSupportedException("Position property not supported by CryptoStream");
			}
		}

		[MonoTODO]
		public override int Read(byte[] buffer, int offset, int count)
		{
			// TODO: implement
			return 0;
		}

		[MonoTODO]
		public override void Write(byte[] buffer, int offset, int count)
		{
			// TODO: implement
		}

		[MonoTODO]
		public override void Flush()
		{
			// TODO: implement
		}

		[MonoTODO]
		public void FlushFinalBlock()
		{
			if (_mode != CryptoStreamMode.Write)
				throw new NotSupportedException("cannot flush a non-writeable CryptoStream");
			
			// TODO: implement
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException("cannot seek a CryptoStream");
		}
		
		public override void SetLength(long value)
		{
			// LAMESPEC: should throw NotSupportedException like Seek??
			return;
		}
		
	} // CryptoStream
	
} // System.Security.Cryptography
