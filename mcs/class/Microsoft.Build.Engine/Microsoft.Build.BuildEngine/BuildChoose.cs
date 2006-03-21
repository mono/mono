//
// BuildChoose.cs: Represents <Choose>.
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
using System.Collections;
using System.Xml;

namespace Microsoft.Build.BuildEngine {
	internal class BuildChoose {
		
		bool		isImported;
		BuildWhen	otherwise;
		Project		project;
		IList		whens;
		
		public BuildChoose (bool imported, Project project)
		{
			this.isImported = imported;
			this.project = project; 
			this.whens = new ArrayList ();
		}
		
		public void BindToXml (XmlElement chooseElement)
		{
		}
		
		public bool IsImported {
			get { return isImported; }
			set { isImported = value; }
		}
		
		public BuildWhen Otherwise {
			get { return otherwise; }
			set { otherwise = value; }
		}
		
		public IList Whens {
			get { return whens; }
			set { whens = value; }
		}
	}
}

#endif
