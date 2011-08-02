//
// System.CodeDom CodeRegionDirective class
//
// Author:
//   Marek Safar (marek.safar@seznam.cz)
//
// (C) 2004 Ximian, Inc.
//

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

using System.Runtime.InteropServices;

namespace System.CodeDom
{
	[Serializable]
	[ComVisible (true), ClassInterface (ClassInterfaceType.AutoDispatch)]
	public class CodeRegionDirective: CodeDirective
	{
		CodeRegionMode regionMode;
		string regionText;

		public CodeRegionDirective () {}

		public CodeRegionDirective (CodeRegionMode regionMode, string regionText)
		{
			this.regionMode = regionMode;
			this.regionText = regionText;
		}

		public CodeRegionMode RegionMode {
			get {
				return regionMode;
			}
			set {
				regionMode = value;
			}
		}

		public string RegionText {
			get {
				if (regionText == null) {
					return string.Empty;
				}
				return regionText;
			}
			set {
				regionText = value;
			}
		}
	}
}

