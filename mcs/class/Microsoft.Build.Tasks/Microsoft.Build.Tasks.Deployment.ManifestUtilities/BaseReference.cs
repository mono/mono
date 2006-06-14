//
// BaseReference.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2006 Marek Sieradzki
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
using System.Runtime.InteropServices;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities {
	
	[ComVisible (false)]
	public abstract class BaseReference {
	
		string	group;
		string	hash;
		bool	isOptional;
		string	resolvedPath;
		long	size;
		string	sourcePath;
		string	targetPath;
		string	xmlGroup;
		string	xmlHash;
		string	xmlHashAlgorithm;
		string	xmlIsOptional;
		string	xmlPath;
		string	xmlSize;
		
		[MonoTODO]
		protected internal BaseReference ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal BaseReference (string path)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public string Group {
			get { return group; }
			set { group = value; }
		}
		
		[MonoTODO]
		public string Hash {
			get { return hash; }
			set { hash = value; }
		}
		
		[MonoTODO]
		public bool IsOptional {
			get { return isOptional; }
			set { isOptional = value; }
		}
		
		[MonoTODO]
		public string ResolvedPath {
			get { return resolvedPath; }
			set { resolvedPath = value; }
		}
		
		[MonoTODO]
		public long Size {
			get { return size; }
			set { size = value; }
		}
		
		[MonoTODO]
		public string SourcePath {
			get { return sourcePath; }
			set { sourcePath = value; }
		}
		
		[MonoTODO]
		public string TargetPath {
			get { return targetPath; }
			set { targetPath = value; }
		}
		
		[MonoTODO]
		public string XmlGroup {
			get { return xmlGroup; }
			set { xmlGroup = value; }
		}
		
		[MonoTODO]
		public string XmlHash {
			get { return xmlHash; }
			set { xmlHash = value; }
		}
		
		[MonoTODO]
		public string XmlHashAlgorithm {
			get { return xmlHashAlgorithm; }
			set { xmlHashAlgorithm = value; }
		}
		
		[MonoTODO]
		public string XmlIsOptional {
			get { return xmlIsOptional; }
			set { xmlIsOptional = value; }
		}
		
		[MonoTODO]
		public string XmlPath {
			get { return xmlPath; }
			set { xmlPath = value; }
		}
		
		[MonoTODO]
		public string XmlSize {
			get { return xmlSize; }
			set { xmlSize = value; }
		}
		
		[MonoTODO]
		protected internal abstract string SortName {
			get ;
		}
	}
}

#endif
