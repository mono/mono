using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Mainsoft.Web.Configuration
{
	public class MainsoftWebSectionGroup : ConfigurationSectionGroup
	{
		[ConfigurationProperty ("pages")]
		public PagesSection Pages {
			get { return (PagesSection) Sections ["pages"]; }
		}
	}
}
