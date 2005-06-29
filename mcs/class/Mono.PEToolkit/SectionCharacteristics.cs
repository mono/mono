
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
	public enum SectionCharacteristics : uint {
		/// <summary>
		/// Reserved.
		/// </summary>
		IMAGE_SCN_TYPE_REG                 =  0x00000000,

		/// <summary>
		/// Reserved.
		/// </summary>
		IMAGE_SCN_TYPE_DSECT               =  0x00000001,

		/// <summary>
		/// Reserved.
		/// </summary>
		IMAGE_SCN_TYPE_NOLOAD              =  0x00000002,

		/// <summary>
		/// Reserved.
		/// </summary>
		IMAGE_SCN_TYPE_GROUP               =  0x00000004,

		/// <summary>
		/// Reserved.
		/// </summary>
		IMAGE_SCN_TYPE_NO_PAD              =  0x00000008,

		/// <summary>
		/// Reserved.
		/// </summary>
		IMAGE_SCN_TYPE_COPY                =  0x00000010,

		/// <summary>
		/// Section contains code.
		/// </summary>
		IMAGE_SCN_CNT_CODE                 =  0x00000020,

		/// <summary>
		/// Section contains initialized data.
		/// </summary>
		IMAGE_SCN_CNT_INITIALIZED_DATA     =  0x00000040,

		/// <summary>
		/// Section contains uninitialized data.
		/// </summary>
		IMAGE_SCN_CNT_UNINITIALIZED_DATA   =  0x00000080,

		/// <summary>
		/// Reserved.
		/// </summary>
		IMAGE_SCN_LNK_OTHER                =  0x00000100,

		/// <summary>
		/// Section contains comments or some other type of information.
		/// </summary>
		IMAGE_SCN_LNK_INFO                 =  0x00000200,

		/// <summary>
		/// Reserved.
		/// </summary>
		IMAGE_SCN_TYPE_OVER                =  0x00000400,
		
		/// <summary>
		/// Section contents will not become part of image.
		/// </summary>
		IMAGE_SCN_LNK_REMOVE               =  0x00000800,

		/// <summary>
		/// Section contents comdat.
		/// </summary>
		IMAGE_SCN_LNK_COMDAT               =  0x00001000,


		/// <summary>
		/// Reset speculative exceptions handling bits in the TLB entries for this section.
		/// </summary>
		/// <remarks>
		/// IMAGE_SCN_MEM_PROTECTED - Obsolete.
		/// </remarks>
		IMAGE_SCN_NO_DEFER_SPEC_EXC        =  0x00004000,

		/// <summary>
		/// Section content can be accessed relative to GP (MIPS).
		/// </summary>
		IMAGE_SCN_GPREL                    =  0x00008000,

		/// <summary>
		/// </summary>
		IMAGE_SCN_MEM_FARDATA              =  0x00008000,

		/// <summary>
		/// Obsolete.
		/// </summary>
		IMAGE_SCN_MEM_PURGEABLE            =  0x00020000,

		/// <summary>
		/// Obsolete.
		/// </summary>
		IMAGE_SCN_MEM_16BIT                =  0x00020000,

		/// <summary>
		/// Obsolete.
		/// </summary>
		IMAGE_SCN_MEM_LOCKED               =  0x00040000,

		/// <summary>
		/// Obsolete.
		/// </summary>
		IMAGE_SCN_MEM_PRELOAD              =  0x00080000,

		/// <summary>
		/// </summary>
		/// <remarks>
		/// IMAGE_SCN_MEM_SYSHEAP  - Obsolete    0x00010000
		/// </remarks>
		IMAGE_SCN_ALIGN_1BYTES             =  0x00100000,

		IMAGE_SCN_ALIGN_2BYTES             =  0x00200000,
		IMAGE_SCN_ALIGN_4BYTES             =  0x00300000,
		IMAGE_SCN_ALIGN_8BYTES             =  0x00400000,

		// default alignment
		IMAGE_SCN_ALIGN_16BYTES            =  0x00500000,

		IMAGE_SCN_ALIGN_32BYTES            =  0x00600000,
		IMAGE_SCN_ALIGN_64BYTES            =  0x00700000,
		IMAGE_SCN_ALIGN_128BYTES           =  0x00800000,
		IMAGE_SCN_ALIGN_256BYTES           =  0x00900000,
		IMAGE_SCN_ALIGN_512BYTES           =  0x00A00000,
		IMAGE_SCN_ALIGN_1024BYTES          =  0x00B00000,
		IMAGE_SCN_ALIGN_2048BYTES          =  0x00C00000,
		IMAGE_SCN_ALIGN_4096BYTES          =  0x00D00000,
		IMAGE_SCN_ALIGN_8192BYTES          =  0x00E00000,

		IMAGE_SCN_ALIGN_MASK               =  0x00F00000,

		/// <summary>
		/// Section contains extended relocations.
		/// </summary>
		IMAGE_SCN_LNK_NRELOC_OVFL          =  0x01000000,
		
		/// <summary>
		/// Section can be discarded.
		/// </summary>
		IMAGE_SCN_MEM_DISCARDABLE          =  0x02000000,
		
		/// <summary>
		/// Section is not cachable.
		/// </summary>
		IMAGE_SCN_MEM_NOT_CACHED           =  0x04000000,

		/// <summary>
		/// Section is not pageable.
		/// </summary>
		IMAGE_SCN_MEM_NOT_PAGED            =  0x08000000,
		
		/// <summary>
		/// Section is shareable.
		/// </summary>
		IMAGE_SCN_MEM_SHARED               =  0x10000000,

		/// <summary>
		/// Section is executable.
		/// </summary>
		IMAGE_SCN_MEM_EXECUTE              =  0x20000000,

		/// <summary>
		/// Section is readable.
		/// </summary>
		IMAGE_SCN_MEM_READ                 =  0x40000000,

		/// <summary>
		/// Section is writeable.
		/// </summary>
		IMAGE_SCN_MEM_WRITE                =  0x80000000,


		/// <summary>
		/// TLS index is scaled.
		/// </summary>
		IMAGE_SCN_SCALE_INDEX              =  0x00000001,
	}

}
