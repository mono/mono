//
// System.Web.UI.HtmlControls.HtmlControl.cs
//
// Author
//   Bob Smith <bob@thestuff.net>
//
//
// (C) Bob Smith
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
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
using System.ComponentModel.Design;
using System.Globalization;
using System.Security.Permissions;

namespace System.Web.UI.HtmlControls{
	
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[ToolboxItem(false)]
	[Designer ("System.Web.UI.Design.HtmlIntrinsicControlDesigner, " + Consts.AssemblySystem_Design,
			"System.ComponentModel.Design.IDesigner")]
	public abstract class HtmlControl : Control, IAttributeAccessor
	{
		internal string _tagName;
		AttributeCollection _attributes;

		protected HtmlControl() : this ("span") {}
		
		protected HtmlControl(string tag)
		{
			_tagName = tag;
		}
		
		protected override ControlCollection CreateControlCollection ()
		{
			return new EmptyControlCollection (this);
		}

		internal static string AttributeToString(int n){
			if (n != -1)return n.ToString(NumberFormatInfo.InvariantInfo);
			return null;
		}
		
		internal static string AttributeToString(string s){
			if (s != null && s.Length != 0) return s;
			return null;
		}
		
		internal void PreProcessRelativeReference(HtmlTextWriter writer, string attribName){
			string attr = Attributes[attribName];
			if (attr != null){
				if (attr.Length != 0){
					try{
						attr = ResolveClientUrl(attr);
					}
					catch (Exception) {
						throw new HttpException(attribName + " property had malformed url");
					}
					writer.WriteAttribute(attribName, attr);
					Attributes.Remove(attribName);
				}
			}
		}

		/* keep these two methods in sync with the
		 * IAttributeAccessor iface methods below */
		protected virtual string GetAttribute (string name)
		{
			return Attributes[name];
		}

		protected virtual void SetAttribute (string name, string value)
		{
			Attributes[name] = value;
		}
		
		string System.Web.UI.IAttributeAccessor.GetAttribute(string name){
			return Attributes[name];
		}
		
		void System.Web.UI.IAttributeAccessor.SetAttribute(string name, string value){
			Attributes[name] = value;
		}
		
		protected virtual void RenderBeginTag (HtmlTextWriter writer)
		{
			writer.WriteBeginTag (TagName);
			RenderAttributes (writer);
			writer.Write ('>');
		}

		protected internal override void Render (HtmlTextWriter writer)
		{
			RenderBeginTag (writer);
		}
		
		protected virtual void RenderAttributes(HtmlTextWriter writer){
			if (ID != null){
				writer.WriteAttribute("id",ClientID);
			}
			Attributes.Render(writer);
		}
		
		[Browsable(false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public AttributeCollection Attributes {
			get { 
				if (_attributes == null)
					_attributes = new AttributeCollection (ViewState);
				return _attributes;
			}
		}

		[DefaultValue(false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Behavior")]
		[TypeConverter (typeof(MinimizableAttributeTypeConverter))]
		public bool Disabled {
			get {
				string disableAttr = Attributes["disabled"] as string;
				return (disableAttr != null);
                        }
			set {
                                if (!value)
                                        Attributes.Remove ("disabled");
                                else
                                        Attributes["disabled"] = "disabled";
                        }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public CssStyleCollection Style {
			get { return Attributes.CssStyle; }
		}

		[DefaultValue("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Appearance")]
		public virtual string TagName {
			get { return _tagName; }
		}

		protected override bool ViewStateIgnoresCase {
			get {
				return true;
			}
		}
	}
}
