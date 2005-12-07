//
// System.Web.Profile.ProfileInfo.cs
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

namespace System.Web.Profile
{
	[Serializable]
	public class ProfileInfo
	{
		string user_name;
		bool is_anonymous;
		DateTime last_activity_date;
		DateTime last_updated_date;
		int size;

		protected ProfileInfo ()
		{
		}

		public ProfileInfo (string username, 
				    bool isAnonymous, 
				    DateTime lastActivityDate, 
				    DateTime lastUpdatedDate, 
				    int size)
		{
			user_name = username;
			is_anonymous = isAnonymous;
			last_activity_date = lastActivityDate;
			last_updated_date = lastUpdatedDate;
			this.size = size;
		}

		public virtual bool IsAnonymous
		{
			get {
				return is_anonymous;
			}
		}

		public virtual DateTime LastActivityDate
		{
			get {
				return last_activity_date;
			}
		}

		public virtual DateTime LastUpdatedDate {
			get {
				return last_updated_date;
			}
		}

		public virtual int Size {
			get {
				return size;
			}
		}

		public virtual string UserName {
			get {
				return user_name;
			}
		}
	}

}
#endif
