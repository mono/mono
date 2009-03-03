//
// System.Web.UI.DataBoundLiteralControl.cs
//
// Authors:
// 	Duncan Mak  (duncan@ximian.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com)
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Security.Permissions;
using System.Text;

namespace System.Web.UI {

	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[ToolboxItem(false)]
	public sealed class DataBoundLiteralControl : Control
#if NET_2_0
	, ITextControl
#endif
	{
		int staticLiteralsCount;
		string [] staticLiterals;
		string [] dataBoundLiterals;
		
		public DataBoundLiteralControl (int staticLiteralsCount,
						int dataBoundLiteralCount)
		{
			this.staticLiteralsCount = staticLiteralsCount;
			dataBoundLiterals = new string [dataBoundLiteralCount];
			AutoID = false;
		}

		public string Text {
			get {
				StringBuilder text = new StringBuilder ();
				int stLength = staticLiterals == null ? 0 : staticLiterals.Length;
				int dbLength = dataBoundLiterals.Length;
				int max = (stLength > dbLength) ? stLength : dbLength;
				for (int i = 0; i < max; i++){
					if (i < stLength)
						text.Append (staticLiterals [i]);
					if (i < dbLength)
						text.Append (dataBoundLiterals [i]);
				}

				return text.ToString ();
			}
		}

		protected override ControlCollection CreateControlCollection ()
		{
			return new EmptyControlCollection (this);
		}

		protected override void LoadViewState (object savedState)
		{
			if (savedState != null) {
				Array source = (Array) savedState;
				if (source.Length == dataBoundLiterals.Length)
					source.CopyTo (dataBoundLiterals, 0);
			}
		}

#if NET_2_0
		protected internal
#else
		protected
#endif
		override void Render (HtmlTextWriter output)
		{
			int stLength = staticLiterals == null ? 0 : staticLiterals.Length;
			int dbLength = dataBoundLiterals.Length;
			int max = (stLength > dbLength) ? stLength : dbLength;

			for (int i = 0; i < max; i++){
				if (i < stLength)
					output.Write (staticLiterals [i]);
				if (i < dbLength)
					output.Write (dataBoundLiterals [i]);
			}
		}

		protected override object SaveViewState ()
		{
			if (dataBoundLiterals.Length == 0)
				return null;
			return dataBoundLiterals;
		}

		public void SetDataBoundString (int index, string s)
		{
			dataBoundLiterals [index] = s;
		}

		public void SetStaticString (int index, string s)
		{
			if (staticLiterals == null)
				staticLiterals = new string [staticLiteralsCount];
			staticLiterals [index] = s;
		}

#if NET_2_0
		string ITextControl.Text {
			get {
				return Text;
			}
			set {
				throw new NotSupportedException ();
			}
		}
#endif		
	}
}

