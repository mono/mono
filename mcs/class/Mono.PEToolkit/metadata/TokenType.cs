/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;

namespace Mono.PEToolkit.Metadata {

	/// <summary>
	/// TokenType enum.
	/// See mdt* constants in CorHdr.
	/// </summary>
	/// <remarks>
	/// See Metadata Unmanaged API, 9.1 Token Types
	/// </remarks>
	public enum TokenType : int {
		__shift = 24,
		__mask  = 0xFF << __shift,

		Module               = 0x00 << __shift,
		TypeRef              = 0x01 << __shift,
		TypeDef              = 0x02 << __shift,
		FieldDef             = 0x04 << __shift,
		MethodDef            = 0x06 << __shift,
		ParamDef             = 0x08 << __shift,
		InterfaceImpl        = 0x09 << __shift,
		MemberRef            = 0x0a << __shift,
		CustomAttribute      = 0x0c << __shift,
		Permission           = 0x0e << __shift,
		Signature            = 0x11 << __shift,
		Event                = 0x14 << __shift,
		Property             = 0x17 << __shift,
		ModuleRef            = 0x1a << __shift,
		TypeSpec             = 0x1b << __shift,
		Assembly             = 0x20 << __shift,
		AssemblyRef          = 0x23 << __shift,
		File                 = 0x26 << __shift,
		ExportedType         = 0x27 << __shift,
		ManifestResource     = 0x28 << __shift,

		String               = 0x70 << __shift,
		Name                 = 0x71 << __shift,
		BaseType             = 0x72 << __shift,
	}

}
