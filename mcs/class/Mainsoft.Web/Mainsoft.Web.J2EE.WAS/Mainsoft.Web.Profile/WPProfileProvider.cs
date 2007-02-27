//
// Mainsoft.Web.Profile.WPProfileProvider
//
// Authors:
//	Ilya Kharmatsky (ilyak@mainsoft.com)
//
// (C) 2007 Mainsoft
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
using System.Collections.Specialized;
using System.Text;
using System.Web.Profile;
using System.Configuration;

using Mainsoft.Web.Security;

namespace Mainsoft.Web.Profile
{
    public class WPProfileProvider : ProfileProvider
    {
        private static readonly string DESCRIPTION = "WebSphere Portal Profile Provider";
        private static readonly string NAME = "WPProfileProvider";

        private string _applicationName = String.Empty;

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);
            if(_applicationName == String.Empty && config != null)
            {
                _applicationName = config["applicationName"];
                _applicationName = (_applicationName == null) ? String.Empty : _applicationName;
            }
        }

        public override string Description
        {
            get
            {
                return DESCRIPTION;
            }
        }

        public override string Name
        {
            get
            {
                return NAME;
            }
        }

        public override string ApplicationName
        {
            get
            {
                return _applicationName;
            }
            set
            {
                _applicationName = value;
            }
        }

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        #region Not Implemented Methods

        [MonoTODO]
        public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        [MonoTODO]
        public override int DeleteProfiles(string[] usernames)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        [MonoTODO]
        public override int DeleteProfiles(ProfileInfoCollection profiles)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        [MonoTODO]
        public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        [MonoTODO]
        public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        [MonoTODO]
        public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        [MonoTODO]
        public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        [MonoTODO]
        public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }
        #endregion
    }
}

#endif