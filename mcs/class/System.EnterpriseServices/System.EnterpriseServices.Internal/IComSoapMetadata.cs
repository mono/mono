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

		string Generate (string SrcTypeLibFileName, string OutPath);

		string GenerateSigned (string SrcTypeLibFileName, string OutPath, bool InstallGac, out string Error);

	}
#endif
}
