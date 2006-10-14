//
// GenerateManifestBase.cs
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
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Build.Tasks.Deployment.ManifestUtilities;

namespace Microsoft.Build.Tasks {
	public abstract class GenerateManifestBase : Task {
	
		string		assemblyName;
		string		assemblyVersion;
		string		description;
		ITaskItem	entryPoint;
		ITaskItem	inputManifest;
		int		maxTargetPath;
		ITaskItem	outputManifest;
		string		platform;
		string		targetCulture;
	
		protected GenerateManifestBase ()
		{
		}

		[MonoTODO]
		public override bool Execute ()
		{
			return false;
		}
		
		[MonoTODO]
		protected internal AssemblyReference AddAssemblyFromItem (ITaskItem item)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal AssemblyReference AddAssemblyNameFromItem (ITaskItem item,
									      AssemblyReferenceType referenceType)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal AssemblyReference AddEntryPointFromItem (ITaskItem item,
									    AssemblyReferenceType referenceType)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal FileReference AddFileFromItem (ITaskItem item)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal FileReference FindFileFromItem (ITaskItem item)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected abstract Type GetObjectType ();
		
		[MonoTODO]
		protected abstract bool OnManifestLoaded (Manifest manifest);
		
		[MonoTODO]
		protected abstract bool OnManifestResolved (Manifest manifest);
		
		[MonoTODO]
		protected internal virtual bool ValidateInputs ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal virtual bool ValidateOutput ()
		{
			throw new NotImplementedException ();
		}
		
		public string AssemblyName {
			get { return assemblyName; }
			set { assemblyName = value; }
		}
		
		public string AssemblyVersion {
			get { return assemblyVersion; }
			set { assemblyVersion = value; }
		}
		
		public string Description {
			get { return description; }
			set { description = value; }
		}
		
		public ITaskItem EntryPoint {
			get { return entryPoint; }
			set { entryPoint = value; }
		}
		
		public ITaskItem InputManifest {
			get { return inputManifest; }
			set { inputManifest = value; }
		}
		
		public int MaxTargetPath {
			get { return maxTargetPath; }
			set { maxTargetPath = value; }
		}
		
		[Output]
		public ITaskItem OutputManifest {
			get { return outputManifest; }
			set { outputManifest = value; }
		}
		
		public string Platform {
			get { return platform; }
			set { platform = value; }
		}
		
		public string TargetCulture {
			get { return targetCulture; }
			set { targetCulture = value; }
		}
	}
}

#endif
