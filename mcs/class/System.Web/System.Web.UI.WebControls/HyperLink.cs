/**
 * Namespace: System.Web.UI.WebControls
 * Class:     HyperLink
 * 
 * Author:  Gaurav Vaish
 * Contact: <gvaish@iitk.ac.in>
 * Status:  10% (??)
 * 
 * (C) Gaurav Vaish (2001)
 */

namespace System.Web.UI.WebControls
{
	public class HyperLink: WebControl
	{
		string imageUrl;
		string navigateUrl;
		string target;
		string text;
		
		public HyperLink()
		{
			imageUrl = string.Empty;
			navigateUrl = string.Empty;
			target = string.Empty;
			text = string.Empty;
		}
		
		public virtual string ImageUrl
		{
			get
			{
				return imageUrl;
			}
			set
			{
				imageUrl = value;
			}
		}
		
		public string NavigateUrl
		{
			get
			{
				return navigateUrl;
			}
			set
			{
				navigateUrl = value;
			}
		}
		
		public string Target
		{
			get
			{
				return target;
			}
			set
			{
				target = value;
			}
		}
		
		public virtual string Text
		{
			get
			{
				return text;
			}
			set
			{
				text = value;
			}
		}
	}
}
