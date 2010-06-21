//
// System.Web.UI.WebControls.CircleHotSpot.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005-2010 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	public sealed class CircleHotSpot: HotSpot
	{
		public override string GetCoordinates ()
		{
			return X + "," + Y + "," + Radius;
		}
		
		protected internal override string MarkupName {
			get { return "circle"; }
		}
		
		[DefaultValueAttribute (0)]
		public int Radius {
			get { return ViewState.GetInt ("Radius", 0); }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ();
				ViewState ["Radius"] = value;
			}
		}

		[DefaultValueAttribute (0)]
		public int X {
			get { return ViewState.GetInt ("X", 0); }
			set { ViewState ["X"] = value; }
		}

		[DefaultValueAttribute (0)]
		public int Y {
			get { return ViewState.GetInt ("Y", 0); }
			set { ViewState ["Y"] = value; }
		}
	}
}

