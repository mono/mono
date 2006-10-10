//
// CreateManifestResourceName.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2005 Marek Sieradzki
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

#if NET_2_0

using System;
using System.IO;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Tasks {
	public abstract class CreateManifestResourceName : TaskExtension {
	
		ITaskItem[]	manifestResourceNames;
		ITaskItem[]	resourceFiles;
		string		rootNamespace;
		
		protected CreateManifestResourceName ()
		{
		}
		
		[MonoTODO]
		public override bool Execute ()
		{
			return false;
		}
		
		[MonoTODO]
		public static string MakeValidEverettIdentifier (string name)
		{
			throw new NotImplementedException ();
		}
		
		protected abstract string CreateManifestName (string fileName,
							      string linkFileName,
							      string rootNamespace,
							      string dependentUponFileName,
							      Stream binaryStream);
		
		protected abstract bool	IsSourceFile (string fileName);
		
		[MonoTODO]
		[Output]
		public ITaskItem[] ManifestResourceNames {
			get { return manifestResourceNames; }
		}
		
		[MonoTODO]
		[Required]
		public ITaskItem[] ResourceFiles {
			get { return resourceFiles; }
			set { resourceFiles = value; }
		}
		
		[MonoTODO]
		public string RootNamespace {
			get { return rootNamespace; }
			set { rootNamespace = value; }
		}
	}
}

#endif
