/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.Web;
using System.Web.UI;
using System.Globalization;

namespace System.Web.UI.HtmlControls
{
	
	public abstract class HtmlInputControl : HtmlControl
	{
		
		public HtmlInputControl (string type) : base ("input")
		{
			Attributes ["type"] = type;
		}
		
		protected override void RenderAttributes (HtmlTextWriter writer)
		{
			writer.WriteAttribute ("name",RenderedName);
			Attributes.Remove ("name");
			base.RenderAttributes (writer);
			writer.Write (" /");
		}
		
		public virtual string Name
		{
			get { return UniqueID; }
			set { ID = value; } // Is this ok?
		}
		
		protected virtual string RenderedName
		{
			get { return Name; }
		}

		public string Type
		{
			get {
				string _type = Attributes ["type"];
				return ((_type != null) ? _type : String.Empty);
			}
		}
		
		public virtual string Value
		{
			get {
				string attr = Attributes ["value"];
				return ((attr != null) ? attr : String.Empty);
			}

			set { Attributes["value"] = value; }
		}
	} // class HtmlInputControl
} // namespace System.Web.UI.HtmlControls

