//
// Manifest.cs
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
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities {
	
	[ComVisible (false)]
	public abstract class Manifest {
	
		AssemblyIdentity		assemblyIdentity;
		AssemblyReferenceCollection	assemblyReferences;
		string				description;
		AssemblyReference		entryPoint;
		FileReferenceCollection	fileReferences;
		Stream				inputStream;
		OutputMessageCollection		outputMessages;
		bool				readOnly;
		string				sourcePath;
		AssemblyIdentity		xmlAssemblyIdentity;
		AssemblyReference[]		xmlAssemblyReferences;
		string				xmlDescription;
		FileReference[]			xmlFileReferences;
		string				xmlSchema;
		
		[MonoTODO]
		protected internal Manifest ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void ResolveFiles ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void ResolveFiles (string[] searchPaths)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void UpdateFileInfo ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual void Validate ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected void ValidatePlatform ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public AssemblyIdentity AssemblyIdentity {
			get { return assemblyIdentity; }
			set { assemblyIdentity = value; }
		}
		
		[MonoTODO]
		public AssemblyReferenceCollection AssemblyReferences {
			get { return assemblyReferences; }
		}
		
		[MonoTODO]
		public string Description {
			get { return description; }
			set { description = value; }
		}
		
		[MonoTODO]
		public virtual AssemblyReference EntryPoint {
			get { return entryPoint; }
			set { entryPoint = value; }
		}
		
		[MonoTODO]
		public FileReferenceCollection FileReferences {
			get { return fileReferences; }
		}
		
		[MonoTODO]
		public Stream InputStream {
			get { return inputStream; }
			set { inputStream = value; }
		}
		
		[MonoTODO]
		public OutputMessageCollection OutputMessages {
			get { return outputMessages; }
		}
		
		[MonoTODO]
		public bool ReadOnly {
			get { return readOnly; }
			set { readOnly = value; }
		}
		
		[MonoTODO]
		public string SourcePath {
			get { return sourcePath; }
			set { sourcePath = value; }
		}
		
		[MonoTODO]
		public AssemblyIdentity XmlAssemblyIdentity {
			get { return xmlAssemblyIdentity; }
			set { xmlAssemblyIdentity = value; }
		}
		
		[MonoTODO]
		public AssemblyReference[] XmlAssemblyReferences {
			get { return xmlAssemblyReferences; }
			set { xmlAssemblyReferences = value; }
		}
		
		[MonoTODO]
		public string XmlDescription {
			get { return xmlDescription; }
			set { xmlDescription = value; }
		}
		
		[MonoTODO]
		public FileReference[] XmlFileReferences {
			get { return xmlFileReferences; }
			set { xmlFileReferences = value; }
		}
		
		[MonoTODO]
		public string XmlSchema {
			get { return xmlSchema; }
			set { xmlSchema = value; }
		}
	}
}

#endif
