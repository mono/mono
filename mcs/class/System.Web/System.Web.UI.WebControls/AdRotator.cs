/**
 * Namespace: System.Web.UI.WebControls
 * Class:     AdRotator
 * 
 * Author:  Gaurav Vaish
 * Contact: <gvaish@iitk.ac.in>
 * Status:  10??%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Collections;

namespace System.Web.UI.WebControls
{
	public class AdRotator: WebControl
	{

		private string advertisementFile;
		private string keywordFilter;
		private string target;

		public event AdCreatedEventHandler AdCreated;

		public AdRotator()
		{
			advertisementFile = string.Empty;
			keywordFilter     = string.Empty;
			target            = string.Empty;
		}
		
		public string AdvertisementFile
		{
			get
			{
				return advertisementFile;
			}
			set
			{
				advertisementFile = value;
			}
		}
		
		public string KeywordFilter
		{
			get
			{
				return keywordFilter;
			}
			set
			{
				keywordFilter = value;
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
		
		protected override void Render(HtmlTextWriter writer)
		{
			HyperLink hLink = new HyperLink();
			Image     image;

			AttributeCollection attributeColl = base.Attributes;
			ICollection keys = attributeColl.Keys;
			IEnumerator iterator = keys.GetEnumerator();
			
			
		}
	}
}
