// System.EnterpriseServices.Internal.IComSoapMetadata.cs
//
// Author:  Mike Kestner (mkestner@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices.Internal {

#if NET_1_1
	[Guid("d8013ff0-730b-45e2-ba24-874b7242c425")]
	public interface IComSoapMetadata {
		[return: MarshalAs(UnmanagedType.BStr)]
		[DispId(1)]
		string Generate ([MarshalAs(UnmanagedType.BStr)] string SrcTypeLibFileName, [MarshalAs(UnmanagedType.BStr)] string OutPath);
		[return: MarshalAs(UnmanagedType.BStr)]
		[DispId(2)]
		string GenerateSigned ([MarshalAs(UnmanagedType.BStr)] string SrcTypeLibFileName, [MarshalAs(UnmanagedType.BStr)] string OutPath, [MarshalAs(UnmanagedType.Bool)] bool InstallGac, [MarshalAs(UnmanagedType.BStr)] out string Error);
	}
#endif
}
