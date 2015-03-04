//
// MonoEncoding.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
//
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace I18N.Common
{
	[Serializable]
	public abstract class MonoEncoding : Encoding
	{
		readonly int win_code_page;

		public MonoEncoding (int codePage)
			: this (codePage, 0)
		{
		}

		public MonoEncoding (int codePage, int windowsCodePage)
			: base (codePage)
		{
			win_code_page = windowsCodePage;
		}

		public override int WindowsCodePage {
			get { return win_code_page != 0 ? win_code_page : base.WindowsCodePage; }
		}

		/// <summary>
		/// GetBytes method used internally by state-full encoders/encodings.
		/// </summary>
		/// <param name="chars">The chars.</param>
		/// <param name="charIndex">Index of the char.</param>
		/// <param name="charCount">The char count.</param>
		/// <param name="bytes">The bytes.</param>
		/// <param name="byteIndex">Index of the byte.</param>
		/// <param name="flush">if set to <c>true</c> [flush].</param>
		/// <param name="encoding">The encoding class to use (or null if state-less).</param>
		/// <returns></returns>
		/// <remarks>
		/// Only state-full encoders need to implement this method (ie. ISO-2022-JP)
		/// </remarks>
		protected unsafe virtual int GetBytesInternal(char *chars, int charCount,
				byte *bytes, int byteCount, bool flush, object state)
		{
			throw new NotImplementedException("Statefull encoding is not implemented (yet?) by this encoding class.");
		}

		public unsafe void HandleFallback (ref EncoderFallbackBuffer buffer,
			char* chars, ref int charIndex, ref int charCount,
			byte* bytes, ref int byteIndex, ref int byteCount, object state)
		{
			if (buffer == null)
				buffer = EncoderFallback.CreateFallbackBuffer ();

			if (charCount > 1 && (Char.IsSurrogate (chars [charIndex]) && Char.IsSurrogate (chars [charIndex + 1]))) {
				buffer.Fallback (chars [charIndex], chars [charIndex + 1], charIndex);
				charIndex++;
				charCount--;
			}
			else
				buffer.Fallback (chars [charIndex], charIndex);
			char [] tmp = new char [buffer.Remaining];
			int idx = 0;
			while (buffer.Remaining > 0)
				tmp [idx++] = buffer.GetNextChar ();

			fixed (char* tmparr = tmp) {
				var outbytes = bytes == null ? null : bytes + byteIndex;
				var len = state == null ?
					GetBytes(tmparr, tmp.Length, outbytes, byteCount)
					: GetBytesInternal(tmparr, tmp.Length, outbytes, byteCount, true, state);

				byteIndex += len;
				byteCount -= len;
			}
		}

		public unsafe void HandleFallback (ref EncoderFallbackBuffer buffer,
			char* chars, ref int charIndex, ref int charCount,
			byte* bytes, ref int byteIndex, ref int byteCount)
		{
			HandleFallback(ref buffer, chars, ref charIndex, ref charCount,
				bytes, ref byteIndex, ref byteCount, null);
		}

		// Get the bytes that result from encoding a character buffer.
		public override int GetByteCount (
			char [] chars, int index, int count)
		{
			if (chars == null)
				throw new ArgumentNullException ("chars");
			if (index < 0 || index > chars.Length)
				throw new ArgumentOutOfRangeException
					("index", Strings.GetString ("ArgRange_Array"));
			if (count < 0 || count > (chars.Length - index))
				throw new ArgumentOutOfRangeException
					("count", Strings.GetString ("ArgRange_Array"));

			if (count == 0)
				return 0;

			unsafe {
				fixed (char* cptr = chars) {
					return GetByteCountImpl (
						cptr + index, count);
				}
			}
		}

		// Get the bytes that result from encoding a character buffer.
		public override int GetBytes (
			char [] chars, int charIndex, int charCount,
			byte [] bytes, int byteIndex)
		{
			if (chars == null)
				throw new ArgumentNullException ("chars");
			if (bytes == null)
				throw new ArgumentNullException ("bytes");
			if (charIndex < 0 || charIndex > chars.Length)
				throw new ArgumentOutOfRangeException
					("charIndex", Strings.GetString ("ArgRange_Array"));
			if (charCount < 0 || charCount > (chars.Length - charIndex))
				throw new ArgumentOutOfRangeException
					("charCount", Strings.GetString ("ArgRange_Array"));
			if (byteIndex < 0 || byteIndex > bytes.Length)
				throw new ArgumentOutOfRangeException
					("byteIndex", Strings.GetString ("ArgRange_Array"));
			if (bytes.Length - byteIndex < charCount)
				throw new ArgumentException (Strings.GetString ("Arg_InsufficientSpace"), "bytes");

			if (charCount == 0)
				return 0;

			unsafe {
				fixed (char* cptr = chars) {
					fixed (byte* bptr = bytes) {
						return GetBytesImpl (
							cptr + charIndex,
							charCount,
							bptr + byteIndex,
							bytes.Length - byteIndex);
					}
				}
			}
		}

		// Convenience wrappers for "GetBytes".
		public override int GetBytes (string s, int charIndex, int charCount,
			byte [] bytes, int byteIndex)
		{
			// Validate the parameters.
			if(s == null)
				throw new ArgumentNullException("s");
			if(bytes == null)
				throw new ArgumentNullException("bytes");
			if(charIndex < 0 || charIndex > s.Length)
				throw new ArgumentOutOfRangeException
					("charIndex",
					 Strings.GetString("ArgRange_StringIndex"));
			if(charCount < 0 || charCount > (s.Length - charIndex))
				throw new ArgumentOutOfRangeException
					("charCount",
					 Strings.GetString("ArgRange_StringRange"));
			if(byteIndex < 0 || byteIndex > bytes.Length)
				throw new ArgumentOutOfRangeException
					("byteIndex",
					 Strings.GetString("ArgRange_Array"));
			if((bytes.Length - byteIndex) < charCount)
				throw new ArgumentException
					(Strings.GetString("Arg_InsufficientSpace"), "bytes");

			if (charCount == 0 || bytes.Length == byteIndex)
				return 0;
			unsafe {
				fixed (char* cptr = s) {
					fixed (byte* bptr = bytes) {
						return GetBytesImpl (
							cptr + charIndex,
							charCount,
							bptr + byteIndex,
							bytes.Length - byteIndex);
					}
				}
			}
		}

		public unsafe override int GetByteCount (char* chars, int count)

		{
			return GetByteCountImpl (chars, count);
		}

		public unsafe override int GetBytes (char* chars, int charCount,
			byte* bytes, int byteCount)

		{
			return GetBytesImpl (chars, charCount, bytes, byteCount);
		}

		//[CLSCompliant (false)]
		public unsafe abstract int GetByteCountImpl (char* chars, int charCount);

		//[CLSCompliant (false)]
		public unsafe abstract int GetBytesImpl (char* chars, int charCount,
			byte* bytes, int byteCount);
		
		public override Encoder GetEncoder ()
		{
			return new MonoEncodingDefaultEncoder (this);
		}
	}

		public abstract class MonoEncoder : Encoder
		{
			MonoEncoding encoding;

			public MonoEncoder (MonoEncoding encoding)
			{
				this.encoding = encoding;
			}

			public override int GetByteCount (
				char [] chars, int index, int count, bool refresh)
			{
				if (chars == null)
					throw new ArgumentNullException ("chars");
				if (index < 0 || index > chars.Length)
					throw new ArgumentOutOfRangeException
						("index", Strings.GetString ("ArgRange_Array"));
				if (count < 0 || count > (chars.Length - index))
					throw new ArgumentOutOfRangeException
						("count", Strings.GetString ("ArgRange_Array"));

				if (count == 0)
					return 0;

				unsafe {
					fixed (char* cptr = chars) {
						return GetByteCountImpl (
							cptr + index, count, refresh);
					}
				}
			}

			public override int GetBytes (char [] chars, int charIndex, int charCount, byte [] bytes, int byteIndex, bool flush)
			{
				if (chars == null)
					throw new ArgumentNullException ("chars");
				if (bytes == null)
					throw new ArgumentNullException ("bytes");
				if (charIndex < 0 || charIndex > chars.Length)
					throw new ArgumentOutOfRangeException
						("charIndex", Strings.GetString ("ArgRange_Array"));
				if (charCount < 0 || charCount > (chars.Length - charIndex))
					throw new ArgumentOutOfRangeException
						("charCount", Strings.GetString ("ArgRange_Array"));
				if (byteIndex < 0 || byteIndex > bytes.Length)
					throw new ArgumentOutOfRangeException
						("byteIndex", Strings.GetString ("ArgRange_Array"));
				if (bytes.Length - byteIndex < charCount)
					throw new ArgumentException (Strings.GetString ("Arg_InsufficientSpace"), "bytes");

				if (charCount == 0)
					return 0;
				unsafe {
					fixed (char* cptr = chars) {
						fixed (byte* bptr = bytes) {
							return GetBytesImpl (cptr + charIndex, 
								charCount,
								bptr + byteIndex,
								bytes.Length - byteIndex,
								flush);
						}
					}
				}
			}

			public unsafe abstract int GetByteCountImpl (char* chars, int charCount, bool refresh);

			public unsafe abstract int GetBytesImpl (char* chars, int charCount, byte* bytes, int byteCount, bool refresh);

			public unsafe override int GetBytes (char* chars, int charCount, byte* bytes, int byteCount, bool flush)
			{
				return GetBytesImpl (chars, charCount, bytes, byteCount, flush);
			}

			public unsafe void HandleFallback (
				char* chars, ref int charIndex, ref int charCount,
				byte* bytes, ref int byteIndex, ref int byteCount, object state)
			{
				EncoderFallbackBuffer buffer = FallbackBuffer;
				encoding.HandleFallback (ref buffer,
					chars, ref charIndex, ref charCount,
					bytes, ref byteIndex, ref byteCount, state);
			}

/*			public unsafe void HandleFallback(
				char* chars, ref int charIndex, ref int charCount,
				byte* bytes, ref int byteIndex, ref int byteCount)
			{
				HandleFallback(chars, ref charIndex, ref charCount,
					bytes, ref byteIndex, ref byteCount, null);
			}*/
		}

	public class MonoEncodingDefaultEncoder : ReferenceSourceDefaultEncoder
	{
		public MonoEncodingDefaultEncoder (Encoding encoding)
			: base (encoding)
		{
		}

		[CLSCompliant (false)]
		[ComVisible (false)]
		public unsafe override void Convert (
			char* chars, int charCount,
			byte* bytes, int byteCount, bool flush,
			out int charsUsed, out int bytesUsed, out bool completed)
		{
			CheckArguments (chars, charCount, bytes, byteCount);

			charsUsed = charCount;
			while (true) {
				bytesUsed = GetByteCount (chars, charsUsed, flush);
				if (bytesUsed <= byteCount)
					break;
				flush = false;
				charsUsed >>= 1;
			}
			completed = charsUsed == charCount;
			bytesUsed = GetBytes (chars, charsUsed, bytes, byteCount, flush);
		}

		[ComVisible (false)]
		public override void Convert (
			char [] chars, int charIndex, int charCount,
			byte [] bytes, int byteIndex, int byteCount, bool flush,
			out int charsUsed, out int bytesUsed, out bool completed)
		{
			if (chars == null)
				throw new ArgumentNullException ("chars");
			if (bytes == null)
				throw new ArgumentNullException ("bytes");
			if (charIndex < 0)
				throw new ArgumentOutOfRangeException ("charIndex");
			if (charCount < 0 || chars.Length < charIndex + charCount)
				throw new ArgumentOutOfRangeException ("charCount");
			if (byteIndex < 0)
				throw new ArgumentOutOfRangeException ("byteIndex");
			if (byteCount < 0 || bytes.Length < byteIndex + byteCount)
				throw new ArgumentOutOfRangeException ("byteCount");

			charsUsed = charCount;
			while (true) {
				bytesUsed = GetByteCount (chars, charIndex, charsUsed, flush);
				if (bytesUsed <= byteCount)
					break;
				flush = false;
				charsUsed >>= 1;
			}
			completed = charsUsed == charCount;
			bytesUsed = GetBytes (chars, charIndex, charsUsed, bytes, byteIndex, flush);
		}

		unsafe void CheckArguments (char* chars, int charCount, byte* bytes, int byteCount)
		{
			if (chars == null)
				throw new ArgumentNullException ("chars");
			if (bytes == null)
				throw new ArgumentNullException ("bytes");
			if (charCount < 0)
				throw new ArgumentOutOfRangeException ("charCount");
			if (byteCount < 0)
				throw new ArgumentOutOfRangeException ("byteCount");
		}
	}
}
