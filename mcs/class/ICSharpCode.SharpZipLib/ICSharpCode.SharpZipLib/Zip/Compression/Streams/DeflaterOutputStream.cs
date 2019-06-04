// DeflaterOutputStream.cs
//
// Copyright (C) 2001 Mike Krueger
//
// This file was translated from java, it was part of the GNU Classpath
// Copyright (C) 2001 Free Software Foundation, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// Linking this library statically or dynamically with other modules is
// making a combined work based on this library.  Thus, the terms and
// conditions of the GNU General Public License cover the whole
// combination.
// 
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module.  An independent module is a module which is not derived from
// or based on this library.  If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so.  If you do not wish to do so, delete this
// exception statement from your version.

using System;
using System.IO;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace ICSharpCode.SharpZipLib.Zip.Compression.Streams 
{

	/// <summary>
	/// A special stream deflating or compressing the bytes that are
	/// written to it.  It uses a Deflater to perform actual deflating.<br/>
	/// Authors of the original java version : Tom Tromey, Jochen Hoenicke 
	/// </summary>
	[System.ObsoleteAttribute("This assembly has been deprecated. Please use https://www.nuget.org/packages/SharpZipLib/ instead.")]
	public class DeflaterOutputStream : Stream
	{
		/// <summary>
		/// This buffer is used temporarily to retrieve the bytes from the
		/// deflater and write them to the underlying output stream.
		/// </summary>
		protected byte[] buf;
		
		/// <summary>
		/// The deflater which is used to deflate the stream.
		/// </summary>
		protected Deflater def;
		
		/// <summary>
		/// Base stream the deflater depends on.
		/// </summary>
		protected Stream baseOutputStream;

		bool isClosed = false;
		bool isStreamOwner = true;
		
		/// <summary>
		/// Get/set flag indicating ownership of underlying stream.
		/// When the flag is true <see cref="Close"></see> will close the underlying stream also.
		/// </summary>
		public bool IsStreamOwner
		{
			get { return isStreamOwner; }
			set { isStreamOwner = value; }
		}
		
		///	<summary>
		/// Allows client to determine if an entry can be patched after its added
		/// </summary>
		public bool CanPatchEntries {
			get { 
				return baseOutputStream.CanSeek; 
			}
		}
		
		/// <summary>
		/// Gets value indicating stream can be read from
		/// </summary>
		public override bool CanRead {
			get {
				return baseOutputStream.CanRead;
			}
		}
		
		/// <summary>
		/// Gets a value indicating if seeking is supported for this stream
		/// This property always returns false
		/// </summary>
		public override bool CanSeek {
			get {
				return false;
			}
		}
		
		/// <summary>
		/// Get value indicating if this stream supports writing
		/// </summary>
		public override bool CanWrite {
			get {
				return baseOutputStream.CanWrite;
			}
		}
		
		/// <summary>
		/// Get current length of stream
		/// </summary>
		public override long Length {
			get {
				return baseOutputStream.Length;
			}
		}
		
		/// <summary>
		/// The current position within the stream.
		/// Always throws a NotSupportedExceptionNotSupportedException
		/// </summary>
		/// <exception cref="NotSupportedException">Any attempt to set position</exception>
		public override long Position {
			get {
				return baseOutputStream.Position;
			}
			set {
				throw new NotSupportedException("DefalterOutputStream Position not supported");
			}
		}
		
		/// <summary>
		/// Sets the current position of this stream to the given value. Not supported by this class!
		/// </summary>
		/// <exception cref="NotSupportedException">Any access</exception>
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException("DeflaterOutputStream Seek not supported");
		}
		
		/// <summary>
		/// Sets the length of this stream to the given value. Not supported by this class!
		/// </summary>
		/// <exception cref="NotSupportedException">Any access</exception>
		public override void SetLength(long val)
		{
			throw new NotSupportedException("DeflaterOutputStream SetLength not supported");
		}
		
		/// <summary>
		/// Read a byte from stream advancing position by one
		/// </summary>
		/// <exception cref="NotSupportedException">Any access</exception>
		public override int ReadByte()
		{
			throw new NotSupportedException("DeflaterOutputStream ReadByte not supported");
		}
		
		/// <summary>
		/// Read a block of bytes from stream
		/// </summary>
		/// <exception cref="NotSupportedException">Any access</exception>
		public override int Read(byte[] b, int off, int len)
		{
			throw new NotSupportedException("DeflaterOutputStream Read not supported");
		}
		
		/// <summary>
		/// Asynchronous reads are not supported a NotSupportedException is always thrown
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <param name="callback"></param>
		/// <param name="state"></param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException">Any access</exception>
		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			throw new NotSupportedException("DeflaterOutputStream BeginRead not currently supported");
		}
		
		/// <summary>
		/// Asynchronous writes arent supported, a NotSupportedException is always thrown
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <param name="callback"></param>
		/// <param name="state"></param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException">Any access</exception>
		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			throw new NotSupportedException("DeflaterOutputStream BeginWrite not currently supported");
		}
		
		/// <summary>
		/// Deflates everything in the input buffers.  This will call
		/// <code>def.deflate()</code> until all bytes from the input buffers
		/// are processed.
		/// </summary>
		protected void Deflate()
		{
			while (!def.IsNeedingInput) {
				int len = def.Deflate(buf, 0, buf.Length);
				
				if (len <= 0) {
					break;
				}
				
				if (this.keys != null) {
					this.EncryptBlock(buf, 0, len);
				}
				
				baseOutputStream.Write(buf, 0, len);
			}
			
			if (!def.IsNeedingInput) {
				throw new SharpZipBaseException("DeflaterOutputStream can't deflate all input?");
			}
		}
		
		/// <summary>
		/// Creates a new DeflaterOutputStream with a default Deflater and default buffer size.
		/// </summary>
		/// <param name="baseOutputStream">
		/// the output stream where deflated output should be written.
		/// </param>
		public DeflaterOutputStream(Stream baseOutputStream) : this(baseOutputStream, new Deflater(), 512)
		{
		}
		
		/// <summary>
		/// Creates a new DeflaterOutputStream with the given Deflater and
		/// default buffer size.
		/// </summary>
		/// <param name="baseOutputStream">
		/// the output stream where deflated output should be written.
		/// </param>
		/// <param name="defl">
		/// the underlying deflater.
		/// </param>
		public DeflaterOutputStream(Stream baseOutputStream, Deflater defl) : this(baseOutputStream, defl, 512)
		{
		}
		
		/// <summary>
		/// Creates a new DeflaterOutputStream with the given Deflater and
		/// buffer size.
		/// </summary>
		/// <param name="baseOutputStream">
		/// The output stream where deflated output is written.
		/// </param>
		/// <param name="deflater">
		/// The underlying deflater to use
		/// </param>
		/// <param name="bufsize">
		/// The buffer size to use when deflating
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// bufsize is less than or equal to zero.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// baseOutputStream does not support writing
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// deflater instance is null
		/// </exception>
		public DeflaterOutputStream(Stream baseOutputStream, Deflater deflater, int bufsize)
		{
			if (baseOutputStream.CanWrite == false) {
				throw new ArgumentException("baseOutputStream", "must support writing");
			}

			if (deflater == null) {
				throw new ArgumentNullException("deflater");
			}
			
			if (bufsize <= 0) {
				throw new ArgumentOutOfRangeException("bufsize");
			}
			
			this.baseOutputStream = baseOutputStream;
			buf = new byte[bufsize];
			def = deflater;
		}
		
		/// <summary>
		/// Flushes the stream by calling flush() on the deflater and then
		/// on the underlying stream.  This ensures that all bytes are
		/// flushed.
		/// </summary>
		public override void Flush()
		{
			def.Flush();
			Deflate();
			baseOutputStream.Flush();
		}
		
		/// <summary>
		/// Finishes the stream by calling finish() on the deflater. 
		/// </summary>
		/// <exception cref="SharpZipBaseException">
		/// Not all input is deflated
		/// </exception>
		public virtual void Finish()
		{
			def.Finish();
			while (!def.IsFinished)  {
				int len = def.Deflate(buf, 0, buf.Length);
				if (len <= 0) {
					break;
				}
				
				if (this.keys != null) {
					this.EncryptBlock(buf, 0, len);
				}
				
				baseOutputStream.Write(buf, 0, len);
			}
			if (!def.IsFinished) {
				throw new SharpZipBaseException("Can't deflate all input?");
			}
			baseOutputStream.Flush();
			keys = null;
		}
		
		/// <summary>
		/// Calls finish() and closes the underlying
		/// stream when <see cref="IsStreamOwner"></see> is true.
		/// </summary>
		public override void Close()
		{
			if ( !isClosed ) {
				isClosed = true;
				Finish();
				if ( isStreamOwner ) {
					baseOutputStream.Close();
				}
			}
		}
		
		/// <summary>
		/// Writes a single byte to the compressed output stream.
		/// </summary>
		/// <param name="bval">
		/// The byte value.
		/// </param>
		public override void WriteByte(byte bval)
		{
			byte[] b = new byte[1];
			b[0] = bval;
			Write(b, 0, 1);
		}
		
		/// <summary>
		/// Writes bytes from an array to the compressed stream.
		/// </summary>
		/// <param name="buf">
		/// The byte array
		/// </param>
		/// <param name="off">
		/// The offset into the byte array where to start.
		/// </param>
		/// <param name="len">
		/// The number of bytes to write.
		/// </param>
		public override void Write(byte[] buf, int off, int len)
		{
			def.SetInput(buf, off, len);
			Deflate();
		}
		
		#region Encryption
		
		// TODO:  Refactor this code.  The presence of Zip specific code in this low level class is wrong
		string password = null;
		uint[] keys     = null;
		
		/// <summary>
		/// Get/set the password used for encryption.  When null no encryption is performed
		/// </summary>
		public string Password {
			get { 
				return password; 
			}
			set {
				if ( value != null && value.Length == 0 ) {
					password = null;
				} else {
					password = value; 
				}
			}
		}
		
		
		/// <summary>
		/// Encrypt a single byte 
		/// </summary>
		/// <returns>
		/// The encrypted value
		/// </returns>
		protected byte EncryptByte()
		{
			uint temp = ((keys[2] & 0xFFFF) | 2);
			return (byte)((temp * (temp ^ 1)) >> 8);
		}
		
		
		/// <summary>
		/// Encrypt a block of data
		/// </summary>
		/// <param name="buffer">
		/// Data to encrypt.  NOTE the original contents of the buffer are lost
		/// </param>
		/// <param name="offset">
		/// Offset of first byte in buffer to encrypt
		/// </param>
		/// <param name="length">
		/// Number of bytes in buffer to encrypt
		/// </param>
		protected void EncryptBlock(byte[] buffer, int offset, int length)
		{
			// TODO: refactor to use crypto transform
			for (int i = offset; i < offset + length; ++i) {
				byte oldbyte = buffer[i];
				buffer[i] ^= EncryptByte();
				UpdateKeys(oldbyte);
			}
		}
		
		/// <summary>
		/// Initializes encryption keys based on given password
		/// </summary>
		protected void InitializePassword(string password) {
			keys = new uint[] {
				0x12345678,
				0x23456789,
				0x34567890
			};
			
			for (int i = 0; i < password.Length; ++i) {
				UpdateKeys((byte)password[i]);
			}
		}

		/// <summary>
		/// Update encryption keys 
		/// </summary>		
		protected void UpdateKeys(byte ch)
		{
			keys[0] = Crc32.ComputeCrc32(keys[0], ch);
			keys[1] = keys[1] + (byte)keys[0];
			keys[1] = keys[1] * 134775813 + 1;
			keys[2] = Crc32.ComputeCrc32(keys[2], (byte)(keys[1] >> 24));
		}
		#endregion
	}
}
