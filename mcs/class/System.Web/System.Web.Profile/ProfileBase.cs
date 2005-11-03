//
// System.Web.UI.WebControls.ProfileBase.cs
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
using System.Configuration;

namespace System.Web.Profile
{
	public class ProfileBase : SettingsBase
	{
		[MonoTODO]
		public ProfileBase ()
		{
		}

		[MonoTODO]
		public static ProfileBase Create (string username)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ProfileBase Create (string username,
						  bool isAuthenticated)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ProfileGroupBase GetProfileGroup (string groupName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Object GetPropertyValue (string propertyName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Initialize (string username,
					bool isAuthenticated)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Save ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetPropertyValue (string propertyName,
					      Object propertyValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsAnonymous {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool IsDirty {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override Object this [ string propertyName ] {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public DateTime LastActivityDate {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public DateTime LastUpdatedDate {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public static SettingsPropertyCollection Properties {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string UserName {
			get {
				throw new NotImplementedException ();
			}
		}
	}

}

#endif
