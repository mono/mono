//
// System.Web.UI.HtmlControls.HtmlControl.cs
//
// Author
//   Bob Smith <bob@thestuff.net>
//
//
// (C) Bob Smith
//

using System;
using System.Globalization;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.HtmlControls{
	
	public abstract class HtmlControl : Control, IAttributeAccessor
	{
		private string _tagName = "span";
		//TODO: Is this correct, or is the StateBag really the ViewState?
		private AttributeCollection _attributes = new AttributeCollection(new StateBag(true));
		private bool _disabled = false;
		
		public HtmlControl(){}
		
		public HtmlControl(string tag)
		{
			if(tag != null && tag != String.Empty) _tagName = tag;
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
						attr = ResolveUrl(attr);
					}
					catch (Exception e) {
						throw new HttpException(attribName + " property had malformed url");
					}
					writer.WriteAttribute(attribName, attr);
					Attributes.Remove(attribName);
				}
			}
		}
		
		string System.Web.UI.IAttributeAccessor.GetAttribute(string name){
			return Attributes[name];
		}
		
		void System.Web.UI.IAttributeAccessor.SetAttribute(string name, string value){
			Attributes[name] = value;
		}
		
		protected virtual void RenderAttributes(HtmlTextWriter writer){
			if (ID != null){
				writer.WriteAttribute("id",ClientID);
			}
			Attributes.Render(writer);
		}
		
		internal static void WriteOnClickAttribute(HtmlTextWriter writer, bool submitsAutomatically, bool submitsProgramatically, bool causesValidation) {
			string local1;
			string local2;
			string local3;
			
			AttributeCollection attr = Attributes;
			local1 = null;
			if (submitsAutomatically) {
				if ((causesValidation))
					local1 = System.Web.UI.Utils.GetClientValidateEvent(Page);
			}
			else if (submitsProgramatically) {
				if (causesValidation)
					local1 = System.Web.UI.Utils.GetClientValidatedPostback(this);
				else
					local1 = Page.GetPostBackClientEvent(this, String.Empty);
			}
			if (local1 != null) {
				local2 = attr["language"];
				if (local2 != null)
					attr.Remove("language");
				writer.WriteAttribute("language", "javascript");
				local3 = attr["onclick"];
				if (local3 != null) {
					attr.Remove("onclick");
					writer.WriteAttribute("onclick", local3 + " " + local1);
					return;
				}
				writer.WriteAttribute("onclick", local1);
			}
		}
		
		public AttributeCollection Attributes
		{
			get
			{
				return _attributes;
			}
		}
		public bool Disabled
		{
			get
			{
				return _disabled;
			}
			set
			{
				_disabled = value;
			}
		}
		public CssStyleCollection Style
		{
			get
			{
				return _attributes.CssStyle;
			}
		}
		public virtual string TagName
		{
			get
			{
				return _tagName;
			}
		}
	}
}
