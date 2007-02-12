//
// System.Web.UI.WebControls.Literal.cs
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[ControlBuilder(typeof(LiteralControlBuilder))]
	[DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, " + Consts.AssemblySystem_Design)]
	[DefaultProperty("Text")]
#if NET_2_0
	[Designer ("System.Web.UI.Design.WebControls.LiteralDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
#endif		
	public class Literal : Control
#if NET_2_0
	, ITextControl
#endif	
	{

		public Literal ()
		{
		}

#if NET_2_0
		[DefaultValue (LiteralMode.Transform)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public LiteralMode Mode 
		{
			get {
				return ViewState ["Mode"] == null ? LiteralMode.Transform : (LiteralMode) ViewState ["Mode"];
			}
			set {
				if (((int) value) < 0 || ((int) value) > 2)
					throw new ArgumentOutOfRangeException ();
				ViewState ["Mode"] = value;
			}
		}
#endif		

		[Bindable(true)]
		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
#if NET_2_0
		[Localizable (true)]
#endif		
		public string Text {
			get {
				return ViewState.GetString ("Text", String.Empty);
			}
			set {
				ViewState ["Text"] = value;
			}
		}

#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override void Focus ()
		{
			throw new NotSupportedException ();
		}
#endif		

		protected override ControlCollection CreateControlCollection ()
		{
			return new EmptyControlCollection (this);
		}

		protected override void AddParsedSubObject (object obj)
		{
			LiteralControl literal = obj as LiteralControl;
			if (literal != null) {
				Text = literal.Text;
				return;
			}

			throw new HttpException (Locale.GetText (
			      "'Literal' cannot have children of type '{0}'",
			      obj.GetType ()));
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void Render (HtmlTextWriter output)
		{
#if NET_2_0
			if (Mode == LiteralMode.Encode)
				output.Write (HttpUtility.HtmlEncode (Text));
			else
#endif
			output.Write (Text);
		}
	}
}

