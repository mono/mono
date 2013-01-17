//
// System.Security.Policy.ApplicationDirectory.cs
//
// Authors:
//	Jackson Harper (Jackson@LatitudeGeo.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Jackson Harper, All rights reserved.
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using Mono.Security;

namespace System.Security.Policy {

	[Serializable]
	[ComVisible (true)]
	public sealed class ApplicationDirectory :
#if NET_4_0
		EvidenceBase,
#endif
		IBuiltInEvidence {
		
		private string directory;

		//
		// Public Constructors
		//
		
		public ApplicationDirectory (string name)
		{
			if (null == name)
				throw new ArgumentNullException ("name");
			if (name.Length < 1)
				throw new FormatException (Locale.GetText ("Empty"));
			directory = name;
		}

		//
		// Public Properties
		//
		
		public string Directory {
			get { return directory; }
		}
		
		//
		// Public Methods
		//
		
		public object Copy ()
		{	
			return new ApplicationDirectory (this.Directory);
		}
		
		public override bool Equals (object o)
		{
			ApplicationDirectory compare = (o as ApplicationDirectory);
			if (compare != null) {
				// MS "by design" behaviour (see FDBK14362)
				ThrowOnInvalid (compare.directory);
				// no C14N or other mojo here (it's done elsewhere)
				return (directory == compare.directory);
			}
			return false;
		}
		
		public override int GetHashCode ()
		{
			return Directory.GetHashCode ();
		}
		
		public override string ToString ()
		{
			// MS "by design" behaviour (see FDBK14362)
			ThrowOnInvalid (Directory);
			SecurityElement element = new SecurityElement ("System.Security.Policy.ApplicationDirectory");
			element.AddAttribute ("version", "1");
			element.AddChild (new SecurityElement ("Directory", directory));
			return element.ToString ();
		}

		// interface IBuiltInEvidence

		int IBuiltInEvidence.GetRequiredSize (bool verbose) 
		{
			return ((verbose) ? 3 : 1) + directory.Length;
		}

		[MonoTODO ("IBuiltInEvidence")]
		int IBuiltInEvidence.InitFromBuffer (char [] buffer, int position) 
		{
			return 0;
		}

		[MonoTODO ("IBuiltInEvidence")]
		int IBuiltInEvidence.OutputToBuffer (char [] buffer, int position, bool verbose) 
		{
			return 0;
		}

		// internal stuff

		private void ThrowOnInvalid (string appdir) 
		{
			if (appdir.IndexOfAny (Path.InvalidPathChars) != -1) {
				string msg = Locale.GetText ("Invalid character(s) in directory {0}");
				throw new ArgumentException (String.Format (msg, appdir), "other");
			}
		}
	}
}
