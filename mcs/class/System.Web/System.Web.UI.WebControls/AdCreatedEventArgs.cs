/**
 * Namespace: System.Web.UI.WebControls
 * Class:     AdCreatedEventArgs
 * 
 * Author:  Gaurav Vaish
 * Contact: <gvaish@iitk.ac.in>
 * Status:  100%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Collections;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public sealed class AdCreatedEventArgs: EventArgs
	{

		private IDictionary adProperties;
		private string      alternateText = string.Empty;
		private string      imageUrl      = string.Empty;
		private string      navigateUrl   = string.Empty;

		public AdCreatedEventArgs(IDictionary adProperties)
		{
			this.adProperties = adProperties;
		}
		
		public IDictionary AdProperties
		{
			get
			{
				return adProperties;
			}
		}
		
		public string AlternateText
		{
			get
			{
				return alternateText;
			}
			set
			{
				alternateText = value;
			}
		}
		
		public string ImageUrl
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
	}
}
