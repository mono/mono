//
// System.Windows.Forms.OSFeature.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
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

namespace System.Windows.Forms {

	// <summary>
	// </summary>

	public class OSFeature : FeatureSupport {

		private OSFeature(){//For signiture compatablity. Prevents the auto creation of public constructor
		}

		//
		//	 --- Public Fields
		//
		public static readonly object LayeredWindows;
		public static readonly object Themes;

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public static OSFeature Feature {
			get {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Public Methods
		//

		[MonoTODO]
		public override Version GetVersionPresent(object feature) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual bool IsPresent(object o) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual bool IsPresent(object o, Version v) {
			throw new NotImplementedException ();
		}
	}
}
