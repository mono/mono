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
