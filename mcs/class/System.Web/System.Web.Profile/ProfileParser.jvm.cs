//
// System.Web.UI.MasterPageParser
//
// Authors:
//	Vladimir Krasnov (vladimirk@mainsoft.com)
//
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
using System.Collections;
using System.IO;
using System.Web;
using System.Web.Compilation;
using System.Web.Util;
using System.Web.J2EE;

namespace System.Web.Profile
{
	internal sealed class ProfileParser
	{
		const string virtualPathCommon = "~/App_Code/ProfileCommon";
		const string virtualPathGroup = "~/App_Code/ProfileGroup";

		internal ProfileParser (HttpContext context)
		{
		}

		public static Type GetProfileCommonType (HttpContext context)
		{
			if (!ProfileCommonTypeExists)
				return null;

			string resolvedUrl = System.Web.Util.UrlUtils.ResolveVirtualPathFromAppAbsolute (virtualPathCommon).TrimEnd ('/');
			Type profileBaseType = PageMapper.GetObjectType (context, resolvedUrl, false);

			ProfileCommonTypeExists = profileBaseType != null;
			return profileBaseType;
		}

		public static Type GetProfileGroupType (HttpContext context, string groupName)
		{
			if (!ProfileGroupTypeExists)
				return null;

			string resolvedUrl = System.Web.Util.UrlUtils.ResolveVirtualPathFromAppAbsolute (virtualPathGroup + groupName).TrimEnd ('/');
			Type profileGroupType = PageMapper.GetObjectType (context, resolvedUrl, false);

			ProfileGroupTypeExists = profileGroupType != null;
			return profileGroupType;
		}

		const string profileKey = "Profile.ProfileCommonType";
		static bool ProfileCommonTypeExists
		{
			get
			{
				object o = AppDomain.CurrentDomain.GetData (profileKey);
				if (o == null)
					return true;
				return (bool) o;
			}
			set { AppDomain.CurrentDomain.SetData (profileKey, value); }
		}

		const string groupKey = "Profile.ProfileGroupType";
		static bool ProfileGroupTypeExists
		{
			get
			{
				object o = AppDomain.CurrentDomain.GetData (groupKey);
				if (o == null)
					return true;
				return (bool) o;
			}
			set { AppDomain.CurrentDomain.SetData (groupKey, value); }
		}
	}
}

#endif 