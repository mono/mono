//
// Mono.ILASM.FileRef
//
// Author(s):
//  Jackson Harper (jackson@ximian.com)
//
// Copyright 2004 Novell, Inc (http://www.novell.com)
//


using System;

namespace Mono.ILASM {

	public class FileRef {

		private string name;
		private byte [] hash;
		private bool has_metadata;
		private bool entrypoint;

		public FileRef (string name, byte[] hash, bool has_metadata, bool entrypoint)
		{
			this.name = name;
			this.hash = hash;
			this.has_metadata = has_metadata;
			this.entrypoint = entrypoint;
		}

		public void Resolve (CodeGen codegen)
		{
			codegen.PEFile.AddFile (name, hash, has_metadata, entrypoint);
		}
	}
}

