/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;

namespace Mono.PEToolkit.Metadata {

	/// <summary>
	/// Flags for ImplMap.
	/// </summary>
	/// <remarks>
	/// See Partition II, 22.1.7;
	/// This is similar to PInvokeMap enum found
	/// in older XML library spec (all.xml).
	/// </remarks>
	[Flags]
	public enum PInvokeAttributes {
		/// <summary>
		/// PInvoke is to use the member name as specified.
		/// </summary>
		NoMangle          = 0x0001,

		/// <summary>
		/// </summary>
		CharSetMask       = 0x0006,
		/// <summary>
		/// </summary>
		CharSetNotSpec    = 0x0000,
		/// <summary>
		/// </summary>
		CharSetAnsi       = 0x0002, // specs: CharSetAns
		/// <summary>
		/// </summary>
		CharSetUnicode    = 0x0004,
		/// <summary>
		/// </summary>
		CharSetAuto       = 0x0006,

		CallConvMask      = 0x0700,
		CallConvWinapi    = 0x0100,
		CallConvCdecl     = 0x0200,
		CallConvStdcall   = 0x0300,
		CallConvThiscall  = 0x0400,
		CallConvFastcall  = 0x0500,

		PinvokeOLE        = 0x0020, // as reported by verifier, not in specs
		                            // also value from all.xml

		SupportsLastError = 0x0040,
	}

}

