//
// System.Web.UI.WebControls.WebParts.Part.cs
//
// Authors:
//   Gaurav Vaish (gaurav[DOT]vaish[AT]gmail[DOT]com)
//
// (C) 2004 Gaurav Vaish (http://www.mastergaurav.org)
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

using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI.WebControls;

namespace System.Web.UI.WebControls.WebParts
{
	public abstract class Part : Panel, INamingContainer,
	                             ICompositeControlDesignerAccessor
	{
		private Part()
		{
		}

		public virtual PartChromeState ChromeState
		{
			get {
				object o = ViewState["ChromeState"];
				if(o != null)
					return (PartChromeState)o;
				return PartChromeState.Normal;
			}
			set {
				if(!Enum.IsDefined(typeof(PartChromeState), value))
					throw new ArgumentException("value");
				ViewState["ChromeState"] = value;
			}
		}

		public virtual PartChromeType ChromeType
		{
			get {
				object o = ViewState["ChromeType"];
				if(o != null)
					return (PartChromeType)o;
				return PartChromeType.Default;
			}
			set {
				if(!Enum.IsDefined(typeof(PartChromeType), value))
					throw new ArgumentException("value");
				ViewState["ChromeType"] = value;
			}
		}

		public override ControlCollection Controls
		{
			get {
				EnsureChildControls();
				return Controls;
			}
		}

		[Localizable(true)]
		public virtual string Description
		{
			get {
				object o = ViewState["Description"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set {
				ViewState["Description"] = value;
			}
		}

		[Localizable(true)]
		public virtual string Title
		{
			get {
				object o = ViewState["Title"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set {
				ViewState["Title"] = value;
			}
		}

		public override void DataBind()
		{
			OnDataBinding(EventArgs.Empty);
			EnsureChildControls();
			DataBindChildren();
		}

		void ICompositeControlDesignerAccessor.RecreateChildControls()
		{
			ChildControlsCreated = false;
			EnsureChildControls();
		}
	}
}

#endif
