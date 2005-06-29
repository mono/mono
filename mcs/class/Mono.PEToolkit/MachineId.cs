
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

	public enum MachineId : ushort {

		/// <summary>
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_UNKNOWN
		/// </remarks>
		UNKNOWN      =   0,

		/// <summary>
		/// Intel 386.
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_I386
		/// </remarks>
		I386         =   0x014c,

		/// <summary>
		/// Intel 486.
		/// </summary>
		/// <remarks>
		/// </remarks>
		I486         =   0x014d,

		/// <summary>
		/// Intel Pentium.
		/// </summary>
		/// <remarks>
		/// </remarks>
		PENTIUM      =   0x014e,

		/// <summary>
		/// MIPS 3K big-endian
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_R3000
		/// </remarks>
		R3000_BE     =   0x0160,

		/// <summary>
		/// MIPS 3K little-endian, 0x160 big-endian
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_R3000
		/// </remarks>
		R3000        =   0x0162,

		/// <summary>
		/// MIPS 4K little-endian
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_R4000
		/// </remarks>
		R4000        =   0x0166,

		/// <summary>
		/// MIPS little-endian
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_R10000
		/// </remarks>
		R10000       =   0x0168,

		/// <summary>
		/// MIPS little-endian WCE v2
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_WCEMIPSV2
		/// </remarks>
		WCEMIPSV2    =   0x0169,

		/// <summary>
		/// Alpha_AXP
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_ALPHA
		/// </remarks>
		ALPHA        =   0x0184,

		/// <summary>
		/// SH3 little-endian
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_SH3
		/// </remarks>
		SH3          =   0x01a2,

		/// <summary>
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_SH3DSP
		/// </remarks>
		SH3DSP       =   0x01a3,

		/// <summary>
		/// SH3E little-endian
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_SH3E
		/// </remarks>
		SH3E         =   0x01a4,

		/// <summary>
		/// SH4 little-endian
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_SH4
		/// </remarks>
		SH4          =   0x01a6,

		/// <summary>
		/// SH5
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_SH5
		/// </remarks>
		SH5          =   0x01a8,

		/// <summary>
		/// ARM Little-Endian
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_ARM
		/// </remarks>
		ARM          =   0x01c0,

		/// <summary>
		///  ARM 10 Thumb family CPU.
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_THUMB
		/// http://www.arm.com/armtech/ARM10_Thumb?OpenDocument&ExpandSection=2
		/// </remarks>
		THUMB        =   0x01c2,

		/// <summary>
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_AM33
		/// </remarks>
		AM33         =   0x01d3,

		/// <summary>
		/// IBM PowerPC Little-Endian
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_POWERPC
		/// </remarks>
		POWERPC      =   0x01F0,

		/// <summary>
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_POWERPCFP
		/// </remarks>
		POWERPCFP    =   0x01f1,

		/// <summary>
		/// Intel 64
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_IA64
		/// </remarks>
		IA64         =   0x0200,

		/// <summary>
		/// MIPS
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_MIPS16
		/// </remarks>
		MIPS16       =   0x0266,

		/// <summary>
		/// ALPHA64
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_ALPHA64
		/// </remarks>
		ALPHA64      =   0x0284,

		/// <summary>
		/// MIPS
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_MIPSFPU
		/// </remarks>
		MIPSFPU      =   0x0366,

		/// <summary>
		/// MIPS
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_MIPSFPU16
		/// </remarks>
		MIPSFPU16    =   0x0466,

		/// <summary>
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_AXP64
		/// </remarks>
		AXP64        =   ALPHA64,

		/// <summary>
		/// Infineon
		/// </summary>
		/// <remarks>
		///  IMAGE_FILE_MACHINE_TRICORE
		///  http://www.infineon.com/tricore
		/// </remarks>
		TRICORE      =   0x0520,

		/// <summary>
		/// Common Executable Format (Windows CE).
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_CEF
		/// </remarks>
		CEF          =   0x0CEF,

		/// <summary>
		/// EFI Byte Code
		/// </summary>
		EBC          =   0x0EBC,

		/// <summary>
		/// AMD64 (K8)
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_AMD64
		/// </remarks>
		AMD64        =   0x8664,

		/// <summary>
		/// M32R little-endian
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_M32R
		/// </remarks>
		M32R         =   0x9104,

		/// <summary>
		/// </summary>
		/// <remarks>
		/// IMAGE_FILE_MACHINE_CEE
		/// </remarks>
		CEE          =   0xC0EE,
	}

}
