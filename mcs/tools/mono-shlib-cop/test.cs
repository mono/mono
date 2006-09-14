//
// test.cs: Unit test for mono-shlib-cop.exe
//
// Compile as:
//    mcs -target:library test.cs -out:test.dll
//
// Authors:
//  Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2005 Jonathan Pryor
//

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

namespace Mono.Unmanaged.Check {

	class Test {
		// OK
		[DllImport ("libgmodule-2.0.so")]
		private static extern int g_module_close (IntPtr handle);

		// Warning
		[DllImport ("libglib-2.0.so")]
		private static extern void g_free (IntPtr mem);

		// Error: no such library
		[DllImport ("does-not-exist")]
		private static extern void Foo ();

		// Error: no such method (library name remapped in .dll.config)
		[DllImport ("renamed-lib")]
		private static extern void RenameMe ();

		// Error: no such method
		[DllImport ("libc")]
		private static extern void DoesNotExist ();

		Test ()
		{
			g_module_close (IntPtr.Zero);
			g_free (IntPtr.Zero);
			Foo ();
			RenameMe ();
			DoesNotExist ();
		}
	}
}

