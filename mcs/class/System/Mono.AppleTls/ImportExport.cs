#if SECURITY_DEP && MONO_FEATURE_APPLETLS
// 
// ImportExport.cs
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//     
// Copyright 2011-2014 Xamarin Inc.
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

using System;
using System.Runtime.InteropServices;
using ObjCRuntime;
using Mono.Net;

namespace Mono.AppleTls {

	internal partial class SecImportExport {
		
		[DllImport (AppleTlsContext.SecurityLibrary)]
		extern static SecStatusCode SecPKCS12Import (IntPtr pkcs12_data, IntPtr options, out IntPtr items);
		
		static public SecStatusCode ImportPkcs12 (byte[] buffer, CFDictionary options, out CFDictionary[] array)
		{
			using (CFData data = CFData.FromData (buffer)) {
				return ImportPkcs12 (data, options, out array);
			}
		}

		static public SecStatusCode ImportPkcs12 (CFData data, CFDictionary options, out CFDictionary [] array)
		{
			if (options == null)
				throw new ArgumentNullException ("options");
			
			IntPtr handle;
			SecStatusCode code = SecPKCS12Import (data.Handle, options.Handle, out handle);
			array = CFArray.ArrayFromHandle <CFDictionary> (handle, h => new CFDictionary (h, false));
			CFObject.CFRelease (handle);
			return code;
		}
	}
}
#endif
