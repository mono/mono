// System.Security.Policy.ApplicationDirectory
//
// Author:
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2002 Jackson Harper, All rights reserved.

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System;

namespace System.Security.Policy {

	[MonoTODO("This class should use a URLString like class instead of just a string")]
	[Serializable]
	public sealed class ApplicationDirectory : IBuiltInEvidence {
		
		private string directory;

		//
		// Public Constructors
		//
		
		public ApplicationDirectory (string name)
		{
			if (null == name)
				throw new ArgumentNullException ("name");		
			directory = name;
		}

		//
		// Public Properties
		//
		
		public string Directory {
			get { return directory;	}
		}
		
		//
		// Public Methods
		//
		
		public object Copy ()
		{	
			return new ApplicationDirectory (Directory);
		}
		
		[MonoTODO("This needs to check for security subsets")]
		public override bool Equals (object other)
		{
			if (null != other && (other is ApplicationDirectory)) {
				ApplicationDirectory compare = (ApplicationDirectory) other;
				return compare.directory.Equals (directory);
			}
			return false;
		}
		
		/// <summary>
		///   This does not return the exact same results as the MS version
		/// </summary>
		public override int GetHashCode ()
		{
			return directory.GetHashCode ();
		}
		
		public override string ToString ()
		{
			return ToXml ().ToString ();
		}

		private SecurityElement ToXml ()
		{
			SecurityElement element = new SecurityElement (GetType().FullName);
			element.AddAttribute ("version", "1");
			element.AddAttribute ("Directory", Directory);
			return element;
		}

		// interface IBuiltInEvidence

		[MonoTODO]
		int IBuiltInEvidence.GetRequiredSize (bool verbose) 
		{
			return 0;
		}

		[MonoTODO]
		int IBuiltInEvidence.InitFromBuffer (char [] buffer, int position) 
		{
			return 0;
		}

		[MonoTODO]
		int IBuiltInEvidence.OutputToBuffer (char [] buffer, int position, bool verbose) 
		{
			return 0;
		}
	}
}
