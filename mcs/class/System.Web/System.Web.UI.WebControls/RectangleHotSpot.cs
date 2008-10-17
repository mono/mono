//
// System.Web.UI.WebControls.RectangleHotSpot.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	public sealed class RectangleHotSpot: HotSpot
	{
		public override string GetCoordinates ()
		{
			return Left + "," + Top + "," + Right + "," + Bottom;
		}
		
		protected internal override string MarkupName {
			get { return "rect"; }
		}
		
	    [DefaultValueAttribute (0)]
		public int Left {
			get {
				object o = ViewState ["Left"];
				return o != null ? (int) o : 0;
			}
			set {
				ViewState ["Left"] = value;
			}
		}

	    [DefaultValueAttribute (0)]
		public int Top {
			get {
				object o = ViewState ["Top"];
				return o != null ? (int) o : 0;
			}
			set {
				ViewState ["Top"] = value;
			}
		}

	    [DefaultValueAttribute (0)]
		public int Right {
			get {
				object o = ViewState ["Right"];
				return o != null ? (int) o : 0;
			}
			set {
				ViewState ["Right"] = value;
			}
		}

	    [DefaultValueAttribute (0)]
		public int Bottom {
			get {
				object o = ViewState ["Bottom"];
				return o != null ? (int) o : 0;
			}
			set {
				ViewState ["Bottom"] = value;
			}
		}
	}
}

#endif
