#if WASM

using System;

namespace System.Text {

	internal static partial class EncodingHelper {

		static volatile Encoding utf8Encoding;

		internal static Encoding UTF8 {
			get {
				if (utf8Encoding == null) {
					lock (lockobj){
						if (utf8Encoding == null){
							utf8Encoding = new UTF8Encoding (true, false);
							utf8Encoding.setReadOnly ();
						}
					}
				}

				return utf8Encoding;
			}
		}

		// The mobile profile has been default'ing to UTF8 since it's creation
		internal static Encoding GetDefaultEncoding ()
		{
			return UTF8;
		}
	}
}

#endif