using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace System.Text
{

internal static partial class EncodingHelper
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
						utf8EncodingWithoutMarkers.setReadOnly ();
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
						utf8EncodingUnsafe.setReadOnly (false);
						utf8EncodingUnsafe.DecoderFallback = new DecoderReplacementFallback (String.Empty);
						utf8EncodingUnsafe.setReadOnly ();
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
						bigEndianUTF32Encoding.setReadOnly ();
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

#if !MONOTOUCH && !WASM
	internal static Encoding GetDefaultEncoding ()
	{
		Encoding enc = null;
						// See if the underlying system knows what
						// code page handler we should be using.
						int code_page = 1;
						
						string code_page_name = InternalCodePage (ref code_page);
						try {
							if (code_page == -1)
								enc = Encoding.GetEncoding (code_page_name);
							else {
								// map the codepage from internal to our numbers
								code_page = code_page & 0x0fffffff;
								switch (code_page){
								case 1: code_page = 20127; break; // ASCIIEncoding.ASCII_CODE_PAGE
								case 2: code_page = 65007; break; // UTF7Encoding.UTF7_CODE_PAGE
								case 3: code_page = 65001; break; // UTF8Encoding.UTF8_CODE_PAGE
								case 4: code_page = 1200; break; // UnicodeEncoding.UNICODE_CODE_PAGE
								case 5: code_page = 1201; break; // UnicodeEncoding.BIG_UNICODE_CODE_PAGE
								case 6: code_page = 1252; break; // Latin1Encoding.ISOLATIN_CODE_PAGE
								}
								enc = Encoding.GetEncoding (code_page);
							}
						} catch (NotSupportedException) {
							// code_page is not supported on underlying platform
							enc = EncodingHelper.UTF8Unmarked;
						} catch (ArgumentException) {
							// code_page_name is not a valid code page, or is 
							// not supported by underlying OS
							enc = EncodingHelper.UTF8Unmarked;
						}
		return enc;
	}
#endif

	// Loaded copy of the "I18N" assembly.  We need to move
	// this into a class in "System.Private" eventually.
	private static Assembly i18nAssembly;
	private static bool i18nDisabled;

	// Invoke a specific method on the "I18N" manager object.
	// Returns NULL if the method failed.
	internal static Object InvokeI18N (String name, params Object[] args)
	{
		lock (lockobj) {
			// Bail out if we previously detected that there
			// is insufficent engine support for I18N handling.
			if (i18nDisabled) {
				return null;
			}

			// Find or load the "I18N" assembly.
			if (i18nAssembly == null) {
				try {
					try {
						i18nAssembly = Assembly.Load (Consts.AssemblyI18N);
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
}

}