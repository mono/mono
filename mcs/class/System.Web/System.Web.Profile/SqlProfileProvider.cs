//
// System.Web.UI.WebControls.SqlProfileProvider.cs
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
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

#if NET_2_0
using System;
using System.Configuration;
using System.Collections.Specialized;

namespace System.Web.Profile
{
	public class SqlProfileProvider : ProfileProvider
	{
		[MonoTODO]
		public SqlProfileProvider ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int DeleteInactiveProfiles (ProfileAuthenticationOption authenticationOption,
							    DateTime userInactiveSinceDate)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int DeleteProfiles (ProfileInfoCollection profiles)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int DeleteProfiles (string[] usernames)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override ProfileInfoCollection FindInactiveProfilesByUserName (ProfileAuthenticationOption authenticationOption,
										      string usernameToMatch,
										      DateTime userInactiveSinceDate,
										      int pageIndex,
										      int pageSize,
										      out int totalRecords)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override ProfileInfoCollection FindProfilesByUserName (ProfileAuthenticationOption authenticationOption,
									      string usernameToMatch,
									      int pageIndex,
									      int pageSize,
									      out int totalRecords)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override ProfileInfoCollection GetAllInactiveProfiles (ProfileAuthenticationOption authenticationOption,
									      DateTime userInactiveSinceDate,
									      int pageIndex,
									      int pageSize,
									      out int totalRecords)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override ProfileInfoCollection GetAllProfiles (ProfileAuthenticationOption authenticationOption,
								      int pageIndex,
								      int pageSize,
								      out int totalRecords)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetNumberOfInactiveProfiles (ProfileAuthenticationOption authenticationOption,
								 DateTime userInactiveSinceDate)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override SettingsPropertyValueCollection GetPropertyValues (SettingsContext sc,
										   SettingsPropertyCollection properties)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Initialize (string name,
						 NameValueCollection config)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void SetPropertyValues (SettingsContext sc,
							SettingsPropertyValueCollection properties)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ApplicationName {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}
}

#endif
