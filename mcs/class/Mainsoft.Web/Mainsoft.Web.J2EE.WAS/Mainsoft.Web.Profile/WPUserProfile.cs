//
// Mainsoft.Web.Profile.WPUserProfile
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
using System.Collections.Generic;
using System.Web.Profile;
using System.Text;

using java.util;
using javax.portlet;

using vmw.portlet;

namespace Mainsoft.Web.Profile
{
    public class WPUserProfile : ProfileBase
    {
        private WPUser _user;

        public WPUserProfile()
        {
            _user = new WPUser();
        }

        public virtual WPUser user
        {
            get { return _user; }
        }
    }

    /// <summary>
    /// The class is stateless container, needed mainly as a workarround for naming convention problem -
    /// the user attribute properties in JSR 168 defines properties with dot '.' symbol in the name.
    /// The dot symbol is forbidden in names of .Net properties, therefor we are creating sub class - WPUser...
    /// </summary>
    #region user class
    public class WPUser
    {
        UserName _name;
        HomeInfo _homeInfo;
        BusinessInfo _businessInfo;

        public WPUser ()
        {
            _name = new UserName();
            _homeInfo = new HomeInfo();
            _businessInfo = new BusinessInfo();
        }

        private static java.util.Map UserInfo
        {
            get
            {
                PortletRequest pr = vmw.portlet.PortletUtils.getPortletRequest();
                if (pr == null)
                    return null;
                return (java.util.Map)pr.getAttribute(PortletRequest__Finals.USER_INFO);
            }
        }

        protected internal static string GetValue(string attribName)
        {
            java.util.Map userInfo = UserInfo;

            if (userInfo == null)
                return null;

            return (string)userInfo.get(attribName);
        }

        public virtual string bdate
        {
            get { return GetValue("user.bdate"); }
        }

        public virtual string gender
        {
            get { return GetValue("user.gender"); }
        }

        public virtual string employer
        {
            get { return GetValue("user.employer"); }
        }

        public virtual string department
        {
            get { return GetValue("user.department"); }
        }

        public virtual string jobtitle
        {
            get { return GetValue("user.jobtitle"); }
        }

        public virtual UserName name
        {
            get { return _name; }
        }

        /// <summary>
        /// In JSR 168 properties the name of property is "home-info", since
        /// .Net doesn't allow usage of '-' character in names of properties, we are replacing
        /// it with '_'
        /// </summary>
        public virtual HomeInfo home_info
        {
            get { return _homeInfo; }
        }

        /// <summary>
        /// In JSR 168 properties the name of property is "home-info", since
        /// .Net doesn't allow usage of '-' character in names of properties, we are replacing
        /// it with '_'
        /// </summary>
        public virtual BusinessInfo business_info
        {
            get { return _businessInfo; }
        }

        #region user.home-info class
        public class HomeInfo
        {
            Postal _postal = new Postal("user.home-info.postal.");
            Telecom _telecom = new Telecom("user.home-info.telecom.");
            Online _online = new Online("user.home-info.online.");

            public virtual Postal postal
            {
                get { return _postal; }
            }

            public virtual Telecom telecom
            {
                get { return _telecom; }
            }

            public virtual Online online
            {
                get { return _online; }
            }
        }
        #endregion

        #region user.business-info class
        public class BusinessInfo
        {

            Postal _postal = new Postal("user.business-info.postal.");
            Telecom _telecom = new Telecom("user.business-info.telecom.");
            Online _online = new Online("user.business-info.online.");

            public virtual Postal postal
            {
                get { return _postal; }
            }

            public virtual Telecom telecom
            {
                get { return _telecom; }
            }

            public virtual Online online
            {
                get { return _online; }
            }
        }
        #endregion

        #region user.name class
        public class UserName
        {
            public virtual string prefix
            {
                get { return WPUser.GetValue("user.name.prefix"); }
            }

            public virtual string given
            {
                get { return WPUser.GetValue("user.name.given"); }
            }

            public virtual string family
            {
                get { return WPUser.GetValue("user.name.family"); }
            }

            public virtual string middle
            {
                get { return WPUser.GetValue("user.name.middle"); }
            }

            public virtual string suffix
            {
                get { return WPUser.GetValue("user.name.suffix"); }
            }

            public virtual string nickName
            {
                get { return WPUser.GetValue("user.name.nickName"); }
            }
        }
        #endregion
    }
    #endregion

    #region TelecomInfo class (contains telecom devices specific info)
    public class TelecomInfo
    {

        private string _intcode;
        private string _loccode;
        private string _number;
        private string _ext;
        private string _comment;

        public TelecomInfo(string prefix)
        {
            if(prefix == null)
                throw new ArgumentNullException("prefix");

            _intcode = prefix + "intcode";
            _loccode = prefix + "loccode";
            _number  = prefix + "number";
            _ext = prefix + "ext";
            _comment = prefix + "comment";
        }

        public virtual string intcode
        {
            get { return WPUser.GetValue(_intcode); }
        }

        public virtual string loccode
        {
            get { return WPUser.GetValue(_loccode); }
        }

        public virtual string number
        {
            get { return WPUser.GetValue(_number); }
        }

        public virtual string ext
        {
            get { return WPUser.GetValue(_ext); }
        }

        public virtual string comment
        {
            get { return WPUser.GetValue(_comment); }
        }
    }
    #endregion

    #region Postal class (contains ground postal info)
    public class Postal
    {
        
        private string _name;
        private string _street;
        private string _city;
        private string _stateprov;
        private string _postalcode;
        private string _country;
        private string _organization;


        public Postal(string prefix)
        {
            if (prefix == null)
                throw new ArgumentNullException("prefix");

            _name   = prefix + "name";
            _street = prefix + "street";
            _city = prefix + "city";
            _stateprov = prefix + "stateprov";
            _postalcode = prefix + "postalcode";
            _country = prefix + "country";
            _organization = prefix + "organization";
        }

        public virtual string name
        {
            get { return WPUser.GetValue(_name); }
        }

        public virtual string street
        {
            get { return WPUser.GetValue(_organization); }
        }

        public virtual string city
        {
            get { return WPUser.GetValue(_city); }
        }

        public virtual string stateprov
        {
            get { return WPUser.GetValue(_stateprov); }
        }

        public virtual string postalcode
        {
            get { return WPUser.GetValue(_postalcode); }
        }

        public virtual string country
        {
            get { return WPUser.GetValue(_country); }
        }

        public virtual string organization
        {
            get { return WPUser.GetValue(_organization); }
        }

    }
    #endregion

    #region Online class (contains on-line info)
    public class Online
    {
        private string namespacePrefix;

        public Online(string prefix)
        {
            namespacePrefix = prefix;
        }
        public virtual string email
        {
            get { return WPUser.GetValue(namespacePrefix + "email"); }
        }

        public virtual string uri
        {
            get { return WPUser.GetValue(namespacePrefix + "uri"); }
        }
    }
    #endregion

    #region Telecom class (contains telecom info - see TelecomInfo)
    public class Telecom
    {
        private string namespacePrefix;



        private TelecomInfo _pager;
        private TelecomInfo _telephone;
        private TelecomInfo _fax;
        private TelecomInfo _mobile;

        public Telecom(string prefix)
        {
            namespacePrefix = prefix;
            _pager = new TelecomInfo(namespacePrefix + "pager.");
            _telephone = new TelecomInfo(namespacePrefix + "telephone.");
            _fax = new TelecomInfo(namespacePrefix + "fax.");
            _mobile = new TelecomInfo(namespacePrefix + "mobile.");
        }

        public virtual TelecomInfo pager { get { return _pager; } }
        public virtual TelecomInfo telephone { get { return _telephone; } }
        public virtual TelecomInfo fax { get { return _fax; } }
        public virtual TelecomInfo mobile { get { return _mobile; } }

    }
    #endregion
}
#endif
