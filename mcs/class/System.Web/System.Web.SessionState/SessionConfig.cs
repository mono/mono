//
// System.Web.SessionState.SessionConfig
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System;

namespace System.Web.SessionState
{
	class SessionConfig
	{
		internal SessionStateMode Mode;
		internal int Timeout; // minutes
		internal bool CookieLess;
		internal string StateConnectionString;
		internal string SqlConnectionString;

		public SessionConfig (object parent)
		{
			if (parent is SessionConfig) {
				SessionConfig p = (SessionConfig) parent;
				CookieLess = p.CookieLess;
				Mode = p.Mode;
				Timeout = p.Timeout;
				StateConnectionString = p.StateConnectionString;
				SqlConnectionString = p.SqlConnectionString;
			} else {
				CookieLess = false;
				Mode = SessionStateMode.InProc;
				Timeout = 20;
				StateConnectionString = "127.0.0.1:42424";
				SqlConnectionString = "user id=sa;password=;data source=127.0.0.1";
			}
		}

		internal bool SetMode (string value)
		{
			try {
				Mode = (SessionStateMode) Enum.Parse (typeof (SessionStateMode), value, true);
			} catch {
				return false;
			}

			return true;
		}

		internal bool SetCookieLess (string value)
		{
			if (value == null)
				return false;

			CookieLess = (0 == String.Compare ("true", value, true));
			if (!CookieLess && String.Compare ("false", value, true) != 0)
				return false;

			return true;
		}

		internal bool SetTimeout (string value)
		{
			try {
				Timeout = Int32.Parse (value);
			} catch {
				return false;
			}
			
			return true;
		}

		internal void SetStateConnectionString (string value)
		{
			StateConnectionString = value;
		}

		internal void SetSqlConnectionString (string value)
		{
			SqlConnectionString = value;
		}
	}
}

