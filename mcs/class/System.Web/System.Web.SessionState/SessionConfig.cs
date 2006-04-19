//
// System.Web.SessionState.SessionConfig
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
// Copyright (C) 2005-2006 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace System.Web.SessionState
{
	class SessionConfig
	{
		internal SessionStateMode Mode;
		internal int Timeout; // minutes
		internal bool CookieLess;
		internal string StateConnectionString;
		internal string SqlConnectionString;
		internal string StateNetworkTimeout;

		public SessionConfig (object parent)
		{
			SessionConfig p = (parent as SessionConfig);
			if (p != null) {
				CookieLess = p.CookieLess;
				Mode = p.Mode;
				Timeout = p.Timeout;
				StateConnectionString = p.StateConnectionString;
				SqlConnectionString = p.SqlConnectionString;
			} else {
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

		internal void SetStateNetworkTimeout (string value)
		{
			StateNetworkTimeout = value;
		}
	}
}

