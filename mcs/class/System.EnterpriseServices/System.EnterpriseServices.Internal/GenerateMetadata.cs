// System.EnterpriseServices.Internal.GenerateMetadata.cs
//
// Author:  Mike Kestner (mkestner@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.

using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices.Internal
{
#if NET_1_1
	[Guid("d8013ff1-730b-45e2-ba24-874b7242c425")]
	public class GenerateMetadata : IComSoapMetadata {

		[MonoTODO]
		public GenerateMetadata ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string Generate (string strSrcTypeLib, string outPath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GenerateMetaData (string strSrcTypeLib, string outPath, byte[] PublicKey, StrongNameKeyPair KeyPair)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GenerateSigned (string strSrcTypeLib, string outPath, bool InstallGac, out string Error)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static int SearchPath (string path, string fileName, string extension, int numBufferChars, string buffer, int[] filePart)
		{
			throw new NotImplementedException ();
		}

	}
#endif
}
