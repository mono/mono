
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

	/// <summary>
	/// </summary>
	public enum Subsystem : short {

		/// <summary>
		/// Unknown subsystem.
		/// </summary>
		/// <remarks>
		///  IMAGE_SUBSYSTEM_UNKNOWN
		/// </remarks>
		UNKNOWN                  =   0,

		/// <summary>
		/// Image doesn't require a subsystem.
		/// </summary>
		/// <remarks>
		/// IMAGE_SUBSYSTEM_NATIVE
		/// </remarks>
		NATIVE                   =   1,

		/// <summary>
		/// Image runs in the Windows GUI subsystem.
		/// </summary>
		/// <remarks>
		///  IMAGE_SUBSYSTEM_WINDOWS_GUI
		/// </remarks>
		WINDOWS_GUI              =   2,

		/// <summary>
		/// Image runs in the Windows character subsystem.
		/// </summary>
		/// <remarks>
		///  IMAGE_SUBSYSTEM_WINDOWS_CUI
		/// </remarks>
		WINDOWS_CUI              =   3,

		/// <summary>
		/// Image runs in the OS/2 character subsystem.
		/// </summary>
		/// <remarks>
		///  IMAGE_SUBSYSTEM_OS2_CUI
		/// </remarks>
		OS2_CUI                  =   5,

		/// <summary>
		///  Image runs in the Posix character subsystem.
		/// </summary>
		/// <remarks>
		///  IMAGE_SUBSYSTEM_POSIX_CUI
		/// </remarks>
		POSIX_CUI                =   7,

		/// <summary>
		/// Image is a native Win9x driver.
		/// </summary>
		/// <remarks>
		///  IMAGE_SUBSYSTEM_NATIVE_WINDOWS
		/// </remarks>
		NATIVE_WINDOWS           =   8,

		/// <summary>
		/// Image runs in the Windows CE subsystem.
		/// </summary>
		/// <remarks>
		///  IMAGE_SUBSYSTEM_WINDOWS_CE_GUI
		/// </remarks>
		WINDOWS_CE_GUI           =   9,

		/// <summary>
		/// </summary>
		/// <remarks>
		///  IMAGE_SUBSYSTEM_EFI_APPLICATION
		/// </remarks>
		EFI_APPLICATION          =  10,

		/// <summary>
		/// </summary>
		/// <remarks>
		///  IMAGE_SUBSYSTEM_EFI_BOOT_SERVICE_DRIVER
		/// </remarks>
		EFI_BOOT_SERVICE_DRIVER  =  11,

		/// <summary>
		/// </summary>
		/// <remarks>
		///  IMAGE_SUBSYSTEM_EFI_RUNTIME_DRIVER
		/// </remarks>
		EFI_RUNTIME_DRIVER       =  12,

		/// <summary>
		/// </summary>
		/// <remarks>
		///  IMAGE_SUBSYSTEM_EFI_ROM
		/// </remarks>
		EFI_ROM                  =  13,

		/// <summary>
		/// </summary>
		/// <remarks>
		///  IMAGE_SUBSYSTEM_XBOX
		/// </remarks>
		XBOX                     =  14,
	}

}
