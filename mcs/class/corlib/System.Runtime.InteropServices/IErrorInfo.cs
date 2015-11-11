// IErrorInfo interface
//
// Eberhard Beilharz (eb1@sil.org)
//
// Copyright (C) 2011 SIL International

#if FEATURE_COMINTEROP

using System;
using System.Runtime.CompilerServices;
using System.Security;

namespace System.Runtime.InteropServices
{
	[ComImport]
	[SuppressUnmanagedCodeSecurity]
	[Guid ("1CF2B120-547D-101B-8E65-08002B2BD119")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IErrorInfo
	{
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		[PreserveSig]
		int GetGUID (out Guid pGuid);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		[PreserveSig]
		int GetSource ([MarshalAs (UnmanagedType.BStr)] out string pBstrSource);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		[PreserveSig]
		int GetDescription ([MarshalAs (UnmanagedType.BStr)] out string pbstrDescription);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		[PreserveSig]
		int GetHelpFile ([MarshalAs (UnmanagedType.BStr)] out string pBstrHelpFile);

		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType=MethodCodeType.Runtime)]
		[PreserveSig]
		int GetHelpContext (out uint pdwHelpContext);
	}
}

#endif
