
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

using System;

namespace Mono.PEToolkit {

	[Flags]
	public enum Characteristics : ushort {


		/// <summary>
		/// Relocation info stripped from file.
		/// </summary>
		/// <remarks>
		///  IMAGE_FILE_RELOCS_STRIPPED
		/// </remarks>
		RELOCS_STRIPPED         =  0x0001,



		/// <summary>
		/// File is executable
		/// (i.e. file is neither object file nor library file,
		/// so there are no unresolved externel references).
		/// </summary>
		/// <remarks>
		///  IMAGE_FILE_EXECUTABLE_IMAGE
		/// </remarks>
		EXECUTABLE_IMAGE        =  0x0002,


		/// <summary>
		/// Line nunbers stripped from file.
		/// </summary>
		/// <remarks>
		///  IMAGE_FILE_LINE_NUMS_STRIPPED
		/// </remarks>
		LINE_NUMS_STRIPPED      =  0x0004,


		/// <summary>
		/// Local symbols stripped from file.
		/// </summary>
		/// <remarks>
		///  IMAGE_FILE_LOCAL_SYMS_STRIPPED
		/// </remarks>
		LOCAL_SYMS_STRIPPED     =   0x0008,


		/// <summary>
		/// Agressively trim working set
		/// </summary>
		/// <remarks>
		///  IMAGE_FILE_AGGRESIVE_WS_TRIM
		/// </remarks>
		AGGRESIVE_WS_TRIM       =   0x0010,


		/// <summary>
		/// App can handle >2gb addresses
		/// </summary>
		/// <remarks>
		///  IMAGE_FILE_LARGE_ADDRESS_AWARE
		/// </remarks>
		LARGE_ADDRESS_AWARE     =  0x0020,


		/// <summary>
		/// Bytes of machine word are reversed.
		/// </summary>
		/// <remarks>
		///  IMAGE_FILE_BYTES_REVERSED_LO
		/// </remarks>
		BYTES_REVERSED_LO       =  0x0080,


		/// <summary>
		/// 32 bit word machine.
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_32BIT_MACHINE
		/// </remarks>
		MACHINE_32BIT           =  0x0100,


		/// <summary>
		/// Debugging info stripped from file in .DBG file
		/// </summary>
		/// <remarks>
		///  IMAGE_FILE_DEBUG_STRIPPED
		/// </remarks>
		DEBUG_STRIPPED          =  0x0200,


		/// <summary>
		/// If Image is on removable media, copy and run from the swap file.
		/// </summary>
		/// <remarks>
		///  IMAGE_FILE_REMOVABLE_RUN_FROM_SWAP
		/// </remarks>
		REMOVABLE_RUN_FROM_SWAP =  0x0400,


		/// <summary>
		/// If Image is on Net, copy and run from the swap file.
		/// </summary>
		/// <remarks>
		///  IMAGE_FILE_NET_RUN_FROM_SWAP
		/// </remarks>
		NET_RUN_FROM_SWAP       =  0x0800,


		/// <summary>
		/// This flag is used to indicate that the file
		/// is a system sile, such as device driver.
		/// </summary>
		/// <remarks>
		///  IMAGE_FILE_SYSTEM
		/// </remarks>
		SYSTEM                  =  0x1000,


		/// <summary>
		/// This flag indicates that the file
		/// is a dynamic library (DLL).
		/// </summary>
		/// <remarks>
		///  IMAGE_FILE_DLL
		/// </remarks>
		DLL                =  0x2000,


		/// <summary>
		/// File should only be run on a uni-processor (UP) machine.
		/// </summary>
		/// <remarks>
		///  IMAGE_FILE_UP_SYSTEM_ONLY
		/// </remarks>
		UP_SYSTEM_ONLY          =  0x4000,


		/// <summary>
		/// Bytes of machine word are reversed.
		/// </summary>
		/// <remarks>
		///  IMAGE_FILE_BYTES_REVERSED_HI
		/// </remarks>
		BYTES_REVERSED_HI       =  0x8000,



		/// <summary>
		/// Default flags that must be set in CIL-only image.
		/// </summary>
		/// <remarks>
		/// See Partition II, 24.2.2.1
		/// </remarks>
		CIL_DEFAULT = LINE_NUMS_STRIPPED  |
		              LOCAL_SYMS_STRIPPED |
		              DEBUG_STRIPPED
	}

}
