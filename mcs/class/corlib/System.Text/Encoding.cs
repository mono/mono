/*
 * Encoding.cs - Implementation of the "System.Text.Encoding" class.
 *
 * Copyright (c) 2001, 2002  Southern Storm Software, Pty Ltd
 * Copyright (c) 2002, Ximian, Inc.
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR
 * OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
 * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */

namespace System.Text
{

using System;
using System.Reflection;
using System.Globalization;
using System.Security;
using System.Runtime.CompilerServices;

[Serializable]
public abstract class Encoding
{
	// Code page used by this encoding.
	internal int codePage;

	// Constructor.
	protected Encoding ()
	{
		codePage = 0;
	}
#if ECMA_COMPAT
	protected internal
#else
	protected
#endif
	Encoding (int codePage)
	{
		this.codePage = codePage;
	}

	// until we change the callers:
	internal static string _ (string arg) {
		return arg;
	}

	// Convert between two encodings.
	public static byte[] Convert (Encoding srcEncoding, Encoding dstEncoding,
								 byte[] bytes)
	{
		if (srcEncoding == null) {
			throw new ArgumentNullException ("srcEncoding");
		}
		if (dstEncoding == null) {
			throw new ArgumentNullException ("dstEncoding");
		}
		if (bytes == null) {
			throw new ArgumentNullException ("bytes");
		}
		return dstEncoding.GetBytes (srcEncoding.GetChars (bytes, 0, bytes.Length));
	}
	public static byte[] Convert (Encoding srcEncoding, Encoding dstEncoding,
								 byte[] bytes, int index, int count)
	{
		if (srcEncoding == null) {
			throw new ArgumentNullException ("srcEncoding");
		}
		if (dstEncoding == null) {
			throw new ArgumentNullException ("dstEncoding");
		}
		if (bytes == null) {
			throw new ArgumentNullException ("bytes");
		}
		if (index < 0 || index > bytes.Length) {
			throw new ArgumentOutOfRangeException
				("index", _("ArgRange_Array"));
		}
		if (count < 0 || (bytes.Length - index) < count) {
			throw new ArgumentOutOfRangeException
				("count", _("ArgRange_Array"));
		}
		return dstEncoding.GetBytes (srcEncoding.GetChars (bytes, index, count));
	}

	// Determine if two Encoding objects are equal.
	public override bool Equals (Object obj)
	{
		Encoding enc = (obj as Encoding);
		if (enc != null) {
			return (codePage == enc.codePage);
		} else {
			return false;
		}
	}

	// Get the number of characters needed to encode a character buffer.
	public abstract int GetByteCount (char[] chars, int index, int count);

	// Convenience wrappers for "GetByteCount".
	public virtual int GetByteCount (String s)
	{
		if (s != null) {
			char[] chars = s.ToCharArray ();
			return GetByteCount (chars, 0, chars.Length);
		} else {
			throw new ArgumentNullException ("s");
		}
	}
	public virtual int GetByteCount (char[] chars)
	{
		if (chars != null) {
			return GetByteCount (chars, 0, chars.Length);
		} else {
			throw new ArgumentNullException ("chars");
		}
	}

	// Get the bytes that result from encoding a character buffer.
	public abstract int GetBytes (char[] chars, int charIndex, int charCount,
								 byte[] bytes, int byteIndex);

	// Convenience wrappers for "GetBytes".
	public virtual int GetBytes (String s, int charIndex, int charCount,
								byte[] bytes, int byteIndex)
	{
		if (s == null) {
			throw new ArgumentNullException ("s");
		}
		return GetBytes (s.ToCharArray(), charIndex, charCount, bytes, byteIndex);
	}
	public virtual byte[] GetBytes (String s)
	{
		if (s == null) {
			throw new ArgumentNullException ("s");
		}
		char[] chars = s.ToCharArray ();
		int numBytes = GetByteCount (chars, 0, chars.Length);
		byte[] bytes = new byte [numBytes];
		GetBytes (chars, 0, chars.Length, bytes, 0);
		return bytes;
	}
	public virtual byte[] GetBytes (char[] chars, int index, int count)
	{
		int numBytes = GetByteCount (chars, index, count);
		byte[] bytes = new byte [numBytes];
		GetBytes (chars, index, count, bytes, 0);
		return bytes;
	}
	public virtual byte[] GetBytes (char[] chars)
	{
		int numBytes = GetByteCount (chars, 0, chars.Length);
		byte[] bytes = new byte [numBytes];
		GetBytes (chars, 0, chars.Length, bytes, 0);
		return bytes;
	}

	// Get the number of characters needed to decode a byte buffer.
	public abstract int GetCharCount (byte[] bytes, int index, int count);

	// Convenience wrappers for "GetCharCount".
	public virtual int GetCharCount (byte[] bytes)
	{
		if (bytes == null) {
			throw new ArgumentNullException ("bytes");
		}
		return GetCharCount (bytes, 0, bytes.Length);
	}

	// Get the characters that result from decoding a byte buffer.
	public abstract int GetChars (byte[] bytes, int byteIndex, int byteCount,
								 char[] chars, int charIndex);

	// Convenience wrappers for "GetChars".
	public virtual char[] GetChars (byte[] bytes, int index, int count)
	{
		int numChars = GetCharCount (bytes, index, count);
		char[] chars = new char [numChars];
		GetChars (bytes, index, count, chars, 0);
		return chars;
	}
	public virtual char[] GetChars (byte[] bytes)
	{
		if (bytes == null) {
			throw new ArgumentNullException ("bytes");
		}
		int numChars = GetCharCount (bytes, 0, bytes.Length);
		char[] chars = new char [numChars];
		GetChars (bytes, 0, bytes.Length, chars, 0);
		return chars;
	}

	// Get a decoder that forwards requests to this object.
	public virtual Decoder GetDecoder ()
	{
		return new ForwardingDecoder (this);
	}

	// Get an encoder that forwards requests to this object.
	public virtual Encoder GetEncoder ()
	{
		return new ForwardingEncoder (this);
	}

	// Loaded copy of the "I18N" assembly.  We need to move
	// this into a class in "System.Private" eventually.
	private static Assembly i18nAssembly;
	private static bool i18nDisabled;

	// Invoke a specific method on the "I18N" manager object.
	// Returns NULL if the method failed.
	private static Object InvokeI18N (String name, params Object[] args)
	{
		lock (typeof(Encoding)) {
			// Bail out if we previously detected that there
			// is insufficent engine support for I18N handling.
			if (i18nDisabled) {
				return null;
			}

			// Find or load the "I18N" assembly.
			if (i18nAssembly == null) {
				try {
					try {
						i18nAssembly = Assembly.Load ("I18N");
					} catch (NotImplementedException) {
						// Assembly loading unsupported by the engine.
						i18nDisabled = true;
						return null;
					}
					if (i18nAssembly == null) {
						return null;
					}
				} catch (SystemException) {
					return null;
				}
			}

			// Find the "I18N.Common.Manager" class.
			Type managerClass;
			try {
				managerClass = i18nAssembly.GetType ("I18N.Common.Manager");
			} catch (NotImplementedException) {
				// "GetType" is not supported by the engine.
				i18nDisabled = true;
				return null;
			}
			if (managerClass == null) {
				return null;
			}

			// Get the value of the "PrimaryManager" property.
			Object manager;
			try {
				manager = managerClass.InvokeMember
						("PrimaryManager",
						 BindingFlags.GetProperty |
						 	BindingFlags.Static |
							BindingFlags.Public,
						 null, null, null, null, null, null);
				if (manager == null) {
					return null;
				}
			} catch (MissingMethodException) {
				return null;
			} catch (SecurityException) {
				return null;
			} catch (NotImplementedException) {
				// "InvokeMember" is not supported by the engine.
				i18nDisabled = true;
				return null;
			}

			// Invoke the requested method on the manager.
			try {
				return managerClass.InvokeMember
						(name,
						 BindingFlags.InvokeMethod |
						 	BindingFlags.Instance |
							BindingFlags.Public,
						 null, manager, args, null, null, null);
			} catch (MissingMethodException) {
				return null;
			} catch (SecurityException) {
				return null;
			}
		}
	}

	// Get an encoder for a specific code page.
#if ECMA_COMPAT
	private
#else
	public
#endif
	static Encoding GetEncoding (int codePage)
	{
		// Check for the builtin code pages first.
		switch (codePage) {
			case 0: return Default;

			case ASCIIEncoding.ASCII_CODE_PAGE:
				return ASCII;

			case UTF7Encoding.UTF7_CODE_PAGE:
				return UTF7;

			case UTF8Encoding.UTF8_CODE_PAGE:
				return UTF8;

			case UnicodeEncoding.UNICODE_CODE_PAGE:
				return Unicode;

			case UnicodeEncoding.BIG_UNICODE_CODE_PAGE:
				return BigEndianUnicode;

			case Latin1Encoding.ISOLATIN_CODE_PAGE:
				return ISOLatin1;

			default: break;
		}

		// Try to obtain a code page handler from the I18N handler.
		Encoding enc = (Encoding)(InvokeI18N ("GetEncoding", codePage));
		if (enc != null) {
			return enc;
		}

#if false
		// Build a code page class name.
		String cpName = "System.Text.CP" + codePage.ToString ();

		// Look for a code page converter in this assembly.
		Assembly assembly = Assembly.GetExecutingAssembly ();
		Type type = assembly.GetType (cpName);
		if (type != null) {
			return (Encoding)(Activator.CreateInstance (type));
		}

		// Look in any assembly, in case the application
		// has provided its own code page handler.
		type = Type.GetType (cpName);
		if (type != null) {
			return (Encoding)(Activator.CreateInstance (type));
		}
#endif

		// We have no idea how to handle this code page.
		throw new NotSupportedException
			(String.Format (_("NotSupp_CodePage"), codePage.ToString ()));
	}

#if !ECMA_COMPAT

	// Table of builtin web encoding names and the corresponding code pages.
	private static readonly String[] encodingNames =
		{"ascii", "us-ascii", "utf-7", "utf-8", "utf-16",
		 "unicodeFFFE", "iso-8859-1"};
	private static readonly int[] encodingCodePages =
		{ASCIIEncoding.ASCII_CODE_PAGE,
		 ASCIIEncoding.ASCII_CODE_PAGE,
		 UTF7Encoding.UTF7_CODE_PAGE,
		 UTF8Encoding.UTF8_CODE_PAGE,
		 UnicodeEncoding.UNICODE_CODE_PAGE,
		 UnicodeEncoding.BIG_UNICODE_CODE_PAGE,
		 Latin1Encoding.ISOLATIN_CODE_PAGE};

	// Get an encoding object for a specific web encoding name.
	public static Encoding GetEncoding (String name)
	{
		// Validate the parameters.
		if (name == null) {
			throw new ArgumentNullException ("name");
		}

		// Search the table for a name match.
		int posn;
		for (posn = 0; posn < encodingNames.Length; ++posn) {
			if (String.Compare(name, encodingNames[posn], true,
							  CultureInfo.InvariantCulture) == 0) {
				return GetEncoding (encodingCodePages[posn]);
			}
		}

		// Try to obtain a web encoding handler from the I18N handler.
		Encoding enc = (Encoding)(InvokeI18N ("GetEncoding", name));
		if (enc != null) {
			return enc;
		}

#if false
		// Build a web encoding class name.
		String encName = "System.Text.ENC" +
						 name.ToLower (CultureInfo.InvariantCulture)
						 	.Replace ('-', '_');

		// Look for a code page converter in this assembly.
		Assembly assembly = Assembly.GetExecutingAssembly ();
		Type type = assembly.GetType (encName);
		if (type != null) {
			return (Encoding)(Activator.CreateInstance (type));
		}

		// Look in any assembly, in case the application
		// has provided its own code page handler.
		type = Type.GetType (encName);
		if (type != null) {
			return (Encoding)(Activator.CreateInstance (type));
		}
#endif

		// We have no idea how to handle this encoding name.
		throw new NotSupportedException (String.Format (_("NotSupp_EncodingName"), name));
	}

#endif // !ECMA_COMPAT

	// Get a hash code for this instance.
	public override int GetHashCode ()
	{
		return codePage;
	}

	// Get the maximum number of bytes needed to encode a
	// specified number of characters.
	public abstract int GetMaxByteCount (int charCount);

	// Get the maximum number of characters needed to decode a
	// specified number of bytes.
	public abstract int GetMaxCharCount (int byteCount);

	// Get the identifying preamble for this encoding.
	public virtual byte[] GetPreamble ()
	{
		return new byte [0];
	}

	// Decode a buffer of bytes into a string.
	public virtual String GetString (byte[] bytes, int index, int count)
	{
		return new String (GetChars(bytes, index, count));
	}
	public virtual String GetString (byte[] bytes)
	{
		return new String (GetChars(bytes));
	}

#if !ECMA_COMPAT

	// Get the mail body name for this encoding.
	public virtual String BodyName
	{
		get {
			return null;
		}
	}

	// Get the code page represented by this object.
	public virtual int CodePage
	{
		get {
			return codePage;
		}
	}

	// Get the human-readable name for this encoding.
	public virtual String EncodingName
	{
		get {
			return null;
		}
	}

	// Get the mail agent header name for this encoding.
	public virtual String HeaderName
	{
		get {
			return null;
		}
	}

	// Determine if this encoding can be displayed in a Web browser.
	public virtual bool IsBrowserDisplay
	{
		get {
			return false;
		}
	}

	// Determine if this encoding can be saved from a Web browser.
	public virtual bool IsBrowserSave
	{
		get {
			return false;
		}
	}

	// Determine if this encoding can be displayed in a mail/news agent.
	public virtual bool IsMailNewsDisplay
	{
		get {
			return false;
		}
	}

	// Determine if this encoding can be saved from a mail/news agent.
	public virtual bool IsMailNewsSave
	{
		get {
			return false;
		}
	}

	// Get the IANA-preferred Web name for this encoding.
	public virtual String WebName
	{
		get {
			return null;
		}
	}

	// Get the Windows code page represented by this object.
	public virtual int WindowsCodePage
	{
		get {
			// We make no distinction between normal and
			// Windows code pages in this implementation.
			return codePage;
		}
	}

#endif // !ECMA_COMPAT

	// Storage for standard encoding objects.
	private static Encoding asciiEncoding = null;
	private static Encoding bigEndianEncoding = null;
	private static Encoding defaultEncoding = null;
	private static Encoding utf7Encoding = null;
	private static Encoding utf8EncodingWithMarkers = null;
	private static Encoding utf8EncodingWithoutMarkers = null;
	private static Encoding unicodeEncoding = null;
	private static Encoding isoLatin1Encoding = null;
	private static Encoding unixConsoleEncoding = null;

	// Get the standard ASCII encoding object.
	public static Encoding ASCII
	{
		get {
			if (asciiEncoding == null) {
				lock (typeof(Encoding)) {
					if (asciiEncoding == null) {
						asciiEncoding = new ASCIIEncoding ();
					}
				}
			}

			return asciiEncoding;
		}
	}

	// Get the standard big-endian Unicode encoding object.
	public static Encoding BigEndianUnicode
	{
		get {
			if (bigEndianEncoding == null) {
				lock (typeof(Encoding)) {
					if (bigEndianEncoding == null) {
						bigEndianEncoding = new UnicodeEncoding (true, true);
					}
				}
			}

			return bigEndianEncoding;
		}
	}

	[MethodImpl (MethodImplOptions.InternalCall)]
	extern private static string InternalCodePage ();
	
	// Get the default encoding object.
	public static Encoding Default
	{
		get {
			if (defaultEncoding == null) {
				lock (typeof(Encoding)) {
					if (defaultEncoding == null) {
						// See if the underlying system knows what
						// code page handler we should be using.
						string codePage = InternalCodePage ();
						try {
							defaultEncoding = GetEncoding (codePage);
						} catch (NotSupportedException) {
							defaultEncoding = UTF8Unmarked;
						}
					}
				}
			}

			return defaultEncoding;
		}
	}

	// Get the ISO Latin1 encoding object.
	private static Encoding ISOLatin1
	{
		get {
			if (isoLatin1Encoding == null) {
				lock (typeof(Encoding)) {
					if (isoLatin1Encoding == null) {
						isoLatin1Encoding = new Latin1Encoding ();
					}
				}
			}

			return isoLatin1Encoding;
		}
	}

	// Get the standard UTF-7 encoding object.
#if ECMA_COMPAT
	private
#else
	public
#endif
	static Encoding UTF7
	{
		get {
			if (utf7Encoding == null) {
				lock (typeof(Encoding)) {
					if (utf7Encoding == null) {
						utf7Encoding = new UTF7Encoding ();
					}
				}
			}

			return utf7Encoding;
		}
	}

	// Get the standard UTF-8 encoding object.
	public static Encoding UTF8
	{
		get {
			if (utf8EncodingWithMarkers == null) {
				lock (typeof(Encoding)) {
					if (utf8EncodingWithMarkers == null) {
						utf8EncodingWithMarkers = new UTF8Encoding (true);
					}
				}
			}

			return utf8EncodingWithMarkers;
		}
	}

	//
	// Only internal, to be used by the class libraries
	//
	internal static Encoding UTF8Unmarked {
		get {
			if (utf8EncodingWithoutMarkers == null) {
				lock (typeof (Encoding)){
					if (utf8EncodingWithoutMarkers == null){
						utf8EncodingWithoutMarkers = new UTF8Encoding (false, true);
					}
				}
			}

			return utf8EncodingWithoutMarkers;
		}
	}
	
	// Get the standard little-endian Unicode encoding object.
	public static Encoding Unicode
	{
		get {
			if (unicodeEncoding == null) {
				lock (typeof(Encoding)) {
					if (unicodeEncoding == null) {
						unicodeEncoding = new UnicodeEncoding (false, true);
					}
				}
			}

			return unicodeEncoding;
		}
	}

	// Forwarding decoder implementation.
	private sealed class ForwardingDecoder : Decoder
	{
		private Encoding encoding;

		// Constructor.
		public ForwardingDecoder (Encoding enc)
		{
			encoding = enc;
		}

		// Override inherited methods.
		public override int GetCharCount (byte[] bytes, int index, int count)
		{
			return encoding.GetCharCount (bytes, index, count);
		}
		public override int GetChars (byte[] bytes, int byteIndex,
									 int byteCount, char[] chars,
									 int charIndex)
		{
			return encoding.GetChars (bytes, byteIndex, byteCount, chars, charIndex);
		}

	} // class ForwardingDecoder

	// Forwarding encoder implementation.
	private sealed class ForwardingEncoder : Encoder
	{
		private Encoding encoding;

		// Constructor.
		public ForwardingEncoder (Encoding enc)
		{
			encoding = enc;
		}

		// Override inherited methods.
		public override int GetByteCount (char[] chars, int index, int count, bool flush)
		{
			return encoding.GetByteCount (chars, index, count);
		}
		public override int GetBytes (char[] chars, int charIndex,
									 int charCount, byte[] bytes,
									 int byteCount, bool flush)
		{
			return encoding.GetBytes (chars, charIndex, charCount, bytes, byteCount);
		}

	} // class ForwardingEncoder

}; // class Encoding

}; // namespace System.Text
