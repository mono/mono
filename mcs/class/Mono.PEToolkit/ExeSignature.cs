
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
/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

namespace Mono.PEToolkit {

	public enum ExeSignature : ushort {

		UNKNOWN = 0,

		/// <summary>
		///  "MZ"
		/// </summary>
		/// <remarks>
		///  IMAGE_DOS_SIGNATURE
		/// </remarks>
		DOS = 0x5A4D,


		/// <summary>
		/// "NE"
		/// </summary>
		/// <remarks>
		///  IMAGE_OS2_SIGNATURE
		/// </remarks>
		OS2 = 0x454E,


		/// <summary>
		///  "LE"
		/// </summary>
		/// <remarks>
		///  IMAGE_OS2_SIGNATURE_LE
		/// </remarks>
		OS2_LE = 0x454C,


		/// <summary>
		///  "LE"
		/// </summary>
		/// <remarks>
		///  IMAGE_VXD_SIGNATURE
		/// </remarks>
		VXD = OS2_LE,


		/// <summary>
		///  "PE", the complete signature is "PE\0\0"
		///  (that is, NT followed by NT2).
		/// </summary>
		/// <remarks>
		///  IMAGE_NT_SIGNATURE
		/// </remarks>
		NT = 0x4550,
		NT2 = 0
	}

}
