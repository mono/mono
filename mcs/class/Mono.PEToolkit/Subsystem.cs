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
