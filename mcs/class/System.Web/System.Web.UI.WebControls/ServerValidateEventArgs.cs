/**
 * Namespace: System.Web.UI.WebControls
 * Class:     ServerValidateEventArgs
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 * 
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public sealed class ServerValidateEventArgs : EventArgs
	{
		private bool isValid;
		private string value;
		
		public ServerValidateEventArgs(string value, bool isValid)
		{
			this.value = value;
			this.isValid = isvalid;
		}
		
		public bool IsValid
		{
			get
			{
				return isValid;
			}
		}
		
		public string Value
		{
			get
			{
				return value;
			}
		}
	}
}
