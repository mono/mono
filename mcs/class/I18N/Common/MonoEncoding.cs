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
		public MonoEncoding (int codePage)
			: base (codePage)
		{
		}

#if NET_2_0
		[CLSCompliant (false)]
		public unsafe void HandleFallback (ref EncoderFallbackBuffer buffer,
			char* chars, ref int charIndex, ref int charCount,
			byte* bytes, ref int byteIndex, ref int byteCount)
		{
			if (buffer == null)
				buffer = EncoderFallback.CreateFallbackBuffer ();
			if (Char.IsSurrogate (chars [charIndex]) && charCount > 0 &&
				Char.IsSurrogate (chars [charIndex + 1])) {
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
				byteIndex += GetBytes (tmparr, tmp.Length, bytes + byteIndex, byteCount);
			}
		}
#endif

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

#if NET_2_0
		public unsafe override int GetBytes (char* chars, int charCount,
			byte* bytes, int byteCount)

		{
			return GetBytesImpl (chars, charCount, bytes, byteCount);
		}
#endif

		[CLSCompliant (false)]
		public unsafe abstract int GetBytesImpl (char* chars, int charCount,
			byte* bytes, int byteCount);

		public abstract class MonoEncoder : Encoder
		{
			MonoEncoding encoding;

			public MonoEncoder (MonoEncoding encoding)
			{
				this.encoding = encoding;
			}

			public override int GetBytes (char [] chars, int charIndex, int charCount, byte [] bytes, int byteIndex, bool flush)
			{
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

			public unsafe abstract int GetBytesImpl (char* chars, int charCount, byte* bytes, int byteCount, bool flush);

		#if NET_2_0
			public unsafe override int GetBytes (char* chars, int charCount, byte* bytes, int byteCount, bool flush)
			{
				return GetBytesImpl (chars, charCount, bytes, byteCount, flush);
			}

			//[CLSCompliant (false)]
			public unsafe void HandleFallback (
				char* chars, ref int charIndex, ref int charCount,
				byte* bytes, ref int byteIndex, ref int byteCount)
			{
				EncoderFallbackBuffer buffer = FallbackBuffer;
				encoding.HandleFallback (ref buffer,
					chars, ref charIndex, ref charCount,
					bytes, ref byteIndex, ref byteCount);
			}
		#endif
		}
	}
}
