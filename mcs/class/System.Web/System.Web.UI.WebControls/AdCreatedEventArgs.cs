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
		private string      alternateText;
		private string      imageUrl;
		private string      navigateUrl;

		public AdCreatedEventArgs(IDictionary adProperties)
		{
			super();
			Initialize();
			this.adProperties = adProperties;
			if(adProperties!=null)
			{
				imageUrl = (string)adProperties.Item["ImageUrl"];
				navigateUrl = (string)adProperties.Item["NavigateUrl"];
				alternateText = (string)adProperties.Item["AlternateText"];
			}
		}
		
		private void Initialize()
		{
			alternateText = string.Empty;
			imageUrl      = string.Empty;
			navigateUrl   = string.Empty;
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
