//
// ResolveManifestFiles.cs
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

namespace Microsoft.Build.Tasks {
	public sealed class ResolveManifestFiles : TaskExtension {

		ITaskItem	entry_point;
		ITaskItem []	extra_files;
		ITaskItem []	files;
		ITaskItem []	managed_assemblies;
		ITaskItem []	native_assemblies;
		ITaskItem []	output_assemblies;
		ITaskItem []	output_files;
		ITaskItem []	publish_files;
		ITaskItem []	satellite_assemblies;
		string		target_culture;
		
		public ResolveManifestFiles ()
		{
		}

		[MonoTODO]
		public override bool Execute ()
		{
			throw new NotImplementedException ();
		}

		public ITaskItem EntryPoint {
			get { return entry_point; }
			set { entry_point = value; }
		}

		public ITaskItem [] ExtraFiles {
			get { return extra_files; }
			set { extra_files = value; }
		}

		public ITaskItem [] Files {
			get { return files; }
			set { files = value; }
		}

		public ITaskItem [] ManagedAssemblies {
			get { return managed_assemblies; }
			set { managed_assemblies = value; }
		}

		public ITaskItem [] NativeAssemblies {
			get { return native_assemblies; }
			set { native_assemblies = value; }
		}

		public ITaskItem [] OutputAssemblies {
			get { return output_assemblies; }
			set { output_assemblies = value; }
		}

		public ITaskItem [] OutputFiles {
			get { return output_files; }
			set { output_files = value; }
		}

		public ITaskItem [] PublishFiles {
			get { return publish_files; }
			set { publish_files = value; }
		}

		public ITaskItem [] SatelliteAssemblies {
			get { return satellite_assemblies; }
			set { satellite_assemblies = value; }
		}

		public string TargetCulture {
			get { return target_culture; }
			set { target_culture = value; }
		}
	}
}

#endif
