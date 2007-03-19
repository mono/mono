//
// UpdateManifest.cs
//
// Author:
//      Leszek Ciesielski  <skolima@gmail.com>
//
// Copyright (C) 2006 Forcom (http://www.forcom.com.pl/)
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
//

#if NET_2_0

using System;
using System.Collections.Specialized;
using Microsoft.Build.Framework;

namespace Microsoft.Build.Tasks {

	[MonoTODO]
	public class VCBuild : ToolTaskExtension {

		[MonoTODO]
		public VCBuild ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ITaskItem[] AdditionalLibPaths {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string AdditionalOptions {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool Clean {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string Configuration {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public ITaskItem Override {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string Platform {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		[Required]
		public ITaskItem[] Projects {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool Rebuild {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public ITaskItem SolutionFile {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool UserEnvironment {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		protected override StringDictionary EnvironmentOverride {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		protected override string ToolName {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override bool Execute ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override void AddCommandLineCommands (
				CommandLineBuilderExtension commandLine )
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override string GenerateFullPathToTool ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool ValidateParameters ()
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
