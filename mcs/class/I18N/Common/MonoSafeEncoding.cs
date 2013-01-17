//
// MonoEncoding.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//  Pablo Ruiz García <pruiz@netway.org>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
// Copyright (C) 2011 Pablo Ruiz García
//
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace I18N.Common
{
#if DISABLE_UNSAFE
	[Serializable]
	public abstract class MonoSafeEncoding : Encoding
	{
		readonly int win_code_page;

		public MonoSafeEncoding (int codePage)
			: this (codePage, 0)
		{
		}

		public MonoSafeEncoding(int codePage, int windowsCodePage)
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
		protected virtual int GetBytesInternal(char[] chars, int charIndex, int charCount, 
			byte[] bytes, int byteIndex, bool flush, object state)
		{
			throw new NotImplementedException("Statefull encoding is not implemented (yet?) by this encoding class.");
		}

		public void HandleFallback(ref EncoderFallbackBuffer buffer,
			char[] chars, ref int charIndex, ref int charCount,
			byte[] bytes, ref int byteIndex, ref int byteCount, object state)
		{
			if (buffer == null)
				buffer = EncoderFallback.CreateFallbackBuffer();

			if (charCount > 1 && (Char.IsSurrogate(chars[charIndex]) && Char.IsSurrogate(chars[charIndex + 1])))
			{
				buffer.Fallback (chars[charIndex], chars[charIndex + 1], charIndex);
				charIndex++;
				charCount--;
			}
			else
				buffer.Fallback (chars[charIndex], charIndex);

			char[] tmp = new char[buffer.Remaining];
			int idx = 0;
			while (buffer.Remaining > 0)
				tmp[idx++] = buffer.GetNextChar();

			var len = state == null ?
				GetBytes(tmp, 0, tmp.Length, bytes, byteIndex)
				: GetBytesInternal(tmp, 0, tmp.Length, bytes, byteIndex, true, state);
			byteIndex += len;
			byteCount -= len;
		}

	}

		public abstract class MonoSafeEncoder : Encoder
		{
#if NET_2_0
			MonoSafeEncoding encoding;
#endif

			public MonoSafeEncoder (MonoSafeEncoding encoding)
			{
#if NET_2_0
				this.encoding = encoding;
#endif
			}

			public void HandleFallback(
				char[] chars, ref int charIndex, ref int charCount,
				byte[] bytes, ref int byteIndex, ref int byteCount, object state)
			{
				EncoderFallbackBuffer buffer = FallbackBuffer;
				encoding.HandleFallback(ref buffer, chars, ref charIndex, ref charCount,
					bytes, ref byteIndex, ref byteCount, state);
			}
		}
#endif
}
