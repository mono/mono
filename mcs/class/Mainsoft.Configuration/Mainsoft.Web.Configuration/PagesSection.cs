using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Mainsoft.Web.Configuration
{
	public class PagesSection : ConfigurationSection
	{
		[ConfigurationPropertyAttribute ("multiForm", DefaultValue = false)]
		public bool MultiForm {
			get {
				return (bool) this ["multiForm"];
			}
			set {
				this ["multiForm"] = value;
			}
		}
	}
}
