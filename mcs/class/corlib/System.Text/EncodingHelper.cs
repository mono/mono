using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Text
{

internal static class EncodingHelper
{
	//
	// Only internal, to be used by the class libraries: Unmarked and non-input-validating
	//
	internal static Encoding UTF8Unmarked {
		get {
			if (utf8EncodingWithoutMarkers == null) {
				lock (lockobj){
					if (utf8EncodingWithoutMarkers == null){
						utf8EncodingWithoutMarkers = new UTF8Encoding (false, false);
//						utf8EncodingWithoutMarkers.is_readonly = true;
					}
				}
			}

			return utf8EncodingWithoutMarkers;
		}
	}
	
	//
	// Only internal, to be used by the class libraries: Unmarked and non-input-validating
	//
	internal static Encoding UTF8UnmarkedUnsafe {
		get {
			if (utf8EncodingUnsafe == null) {
				lock (lockobj){
					if (utf8EncodingUnsafe == null){
						utf8EncodingUnsafe = new UTF8Encoding (false, false);
						typeof (Encoding).GetField ("m_isReadOnly", BindingFlags.NonPublic | BindingFlags.Instance).SetValue (utf8EncodingUnsafe, false);
						utf8EncodingUnsafe.DecoderFallback = new DecoderReplacementFallback (String.Empty);
						typeof (Encoding).GetField ("m_isReadOnly", BindingFlags.NonPublic | BindingFlags.Instance).SetValue (utf8EncodingUnsafe, true);
					}
				}
			}

			return utf8EncodingUnsafe;
		}
	}

	// Get the standard big-endian UTF-32 encoding object.
	internal static Encoding BigEndianUTF32
	{
		get {
			if (bigEndianUTF32Encoding == null) {
				lock (lockobj) {
					if (bigEndianUTF32Encoding == null) {
						bigEndianUTF32Encoding = new UTF32Encoding (true, true);
//						bigEndianUTF32Encoding.is_readonly = true;
					}
				}
			}

			return bigEndianUTF32Encoding;
		}
	}
	static volatile Encoding utf8EncodingWithoutMarkers;
	static volatile Encoding utf8EncodingUnsafe;
	static volatile Encoding bigEndianUTF32Encoding;
	static readonly object lockobj = new object ();

	[MethodImpl (MethodImplOptions.InternalCall)]
	extern internal static string InternalCodePage (ref int code_page);
}

}