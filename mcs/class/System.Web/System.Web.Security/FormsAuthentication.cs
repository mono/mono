/**
 * Namespace: System.Web.Security
 * Class:     FormsAuthentication
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  ??%
 * 
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Web;

namespace System.Web.Security
{
	public sealed class FormsAuthentication
	{
		private static formsCookieName;
		private static formsCookiePath;
		
		private static bool isIntialized = false;

		public FormsAuthentication()
		{
		}
		
		public static string FormsCookieName
		{
			get
			{
				Initialize();
				return formsCookieName;
			}
		}
		
		public static string FormsCookiePath
		{
			get
			{
				Initialize();
				return formsCookiePath;
			}
		}
		
		public static bool Authenticate(string name, string password)
		{
			if(name != null && password != null)
			{
				Initialize();
				AuthenticationConfig cfg = (AuthenticatonConfig)HttpContext.Current.GetConfig("system.web/authentication");
				Hashtable db = cfg.Credentials;
				if(db == null)
				{
					//TraceBack("No_user_database");
					return false;
				}
				string passwd = (String)(db[name.ToLower()]);
				if(passwd == null)
				{
					//Traceback("No_user_in_databse")
					return false;
				}
				switch(cfg.PasswordFormat)
				{
					
				}
			}
			return false;
		}
	}
}
