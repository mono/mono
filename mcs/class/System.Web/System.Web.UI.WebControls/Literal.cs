//
// System.Web.UI.WebControls.Literal.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Sanjay Gupta (gsanjay@novell.com)
//
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
// (C) 2004, Novell, Inc. (http://www.novell.com)
//
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

using System;
using System.Web;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	[DefaultProperty("Text")]
	[ControlBuilder(typeof(LiteralControlBuilder))]
	[DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, " + Consts.AssemblySystem_Design)]
#if NET_2_0
	[DesignerAttribute ("Value not found")]
#endif
	public class Literal : Control
#if NET_2_0
//	, IStaticTextControl
#endif
	{
		public Literal () : base ()
		{
		}

		[DefaultValue (""), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The text for the literal WebControl.")]
#if NET_2_0
		[Localizable (true)]
#endif
		public string Text
		{
			get {
				object o = ViewState ["Text"];
				return (o == null) ? String.Empty : (string) o;
			}

			set { ViewState ["Text"] = value; }
		}

		protected override ControlCollection CreateControlCollection ()
		{
			return new EmptyControlCollection (this);
		}

		protected override void AddParsedSubObject (object obj)
		{
			if (!(obj is LiteralControl))
				throw new HttpException (HttpRuntime.FormatResourceString (
							"Cannot_Have_Children_Of_Type", "Literal",
							obj.GetType ().Name.ToString ()));

			Text = ((LiteralControl) obj).Text;
		}

		protected override void Render (HtmlTextWriter writer)
		{
			if (Text.Length > 0)
				writer.Write (Text);
		}

#if NET_2_0
		private LiteralMode literalMode;
		[DefaultValue (LiteralMode.Transform), WebCategory ("Behavior"), WebSysDescription ("Determines whether the text is transformed or encoded")]
		public LiteralMode Mode {
			get { return literalMode; }
			set { literalMode = value; }
		}

		[MonoTODO]
		public override void Focus ()
		{
			throw new NotImplementedException ("MS .NET throws non-visual control does not support the Focus operation"); 
		}
#endif
	}
}

