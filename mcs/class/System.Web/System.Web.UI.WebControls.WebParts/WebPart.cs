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
	public class WebPart : Part //, IWebPart, IWebActionable
	{
		private bool allowClose      = true;
		private bool allowEdit       = true;
		private bool allowHide       = true;
		private bool allowMinimize   = true;
		private bool allowZoneChange = true;
		
		private bool isStatic = true;
		private bool isStandalone = true;
		
		private PartChromeState chromeState = PartChromeState.Normal;
		private WebPartExportMode exportMode = WebPartExportMode.None;
		private WebPartHelpMode   helpMode   = WebPartHelpMode.Navigate;
		
		protected WebPart()
		{
		}
		
		public virtual bool AllowClose
		{
			get {
				return allowClose;
			}
			set {
				allowClose = value;
			}
		}
		
		public virtual bool AllowEdit
		{
			get {
				return allowEdit;
			}
			set {
				allowEdit = value;
			}
		}
		
		public virtual bool AllowHelp
		{
			get {
				return AllowHelp;
			}
			set {
				allowHelp = value;
			}
		}
		
		public virtual bool AllowMinimize
		{
			get {
				return allowMinimize;
			}
			set {
				allowMinimize = value;
			}
		}
		
		public virtual bool AllowZoneChange
		{
			get {
				return allowZoneChange;
			}
			set {
				allowZoneChange = value;
			}
		}
		
		public bool IsClosed
		{
			get {
				return isClosed;
			}
		}
		
		public bool IsStandalone
		{
			get {
				return isStandalone;
			}
		}
		
		public override PartChromeState ChromeState
		{
			get {
				return chromeState;
			}
			set {
				if(!Enum.IsDefined(typeof(PartChromeState), value))
					throw new ArgumentException("value");
				chromeState = value;
			}
		}
		
		public override PartChromeType ChromeType
		{
			get {
				return chromeType;
			}
			set {
				if(!Enum.IsDefined(typeof(PartChromeType), value))
					throw new ArgumentException("value");
				chromeType = value;
			}
		}
	}
}
