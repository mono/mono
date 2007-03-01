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
using System.Text;

using java.util;
using javax.portlet;

using vmw.portlet;

namespace Mainsoft.Web.Profile
{
    public class WPUserProfile
    {
        private WPUser _user;

        public WPUserProfile()
        {
            _user = new WPUser();
        }

        public WPUser user
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

        public string bdate
        {
            get { return GetValue("user.bdate"); }
        }

        public string gender
        {
            get { return GetValue("user.gender"); }
        }

        public string employer
        {
            get { return GetValue("user.employer"); }
        }

        public string department
        {
            get { return GetValue("user.department"); }
        }

        public string jobtitle
        {
            get { return GetValue("user.jobtitle"); }
        }

        public UserName name
        {
            get { return _name; }
        }

        /// <summary>
        /// In JSR 168 properties the name of property is "home-info", since
        /// .Net doesn't allow usage of '-' character in names of properties, we are replacing
        /// it with '_'
        /// </summary>
        public HomeInfo home_info
        {
            get { return _homeInfo; }
        }

        /// <summary>
        /// In JSR 168 properties the name of property is "home-info", since
        /// .Net doesn't allow usage of '-' character in names of properties, we are replacing
        /// it with '_'
        /// </summary>
        public BusinessInfo business_info
        {
            get { return _businessInfo; }
        }

        #region user.home-info class
        public class HomeInfo
        {
            Postal _postal = new Postal();
            Telecom _telecom = new Telecom();
            Online _online = new Online();

            public Postal postal
            {
                get { return _postal; }
            }

            public Telecom telecom
            {
                get { return _telecom; }
            }

            public Online online
            {
                get { return _online; }
            }

            #region user.home-info.postal class
            public class Postal
            {
                public string name
                {
                    get { return WPUser.GetValue("user.home-info.postal.name"); }
                }

                public string street
                {
                    get { return WPUser.GetValue("user.home-info.postal.street"); }
                }

                public string city
                {
                    get { return WPUser.GetValue("user.home-info.postal.city"); }
                }

                public string stateprov
                {
                    get { return WPUser.GetValue("user.home-info.postal.stateprov"); }
                }

                public string postalcode
                {
                    get { return WPUser.GetValue("user.home-info.postal.postalcode"); }
                }

                public string country
                {
                    get { return WPUser.GetValue("user.home-info.postal.country"); }
                }

                public string organization
                {
                    get { return WPUser.GetValue("user.home-info.postal.organization"); }
                }
            }
            #endregion

            #region user.home-info.telecom class
            public class Telecom
            {
                private Telephone _telephone = new Telephone();
                private Fax _fax = new Fax();
                private Mobile _mobile = new Mobile();
                private Pager _pager = new Pager();

                public Telephone telephone 
                {
                    get { return _telephone;}
                }

                public Fax fax 
                {
                    get { return _fax;}
                }

                public Mobile mobile
                {
                    get { return _mobile;}
                }

                public Pager pager 
                {
                    get { return _pager;}
                }

                #region user.home-info.telecom.telephone class
                public class Telephone 
                {
                    public string intcode
                    {
                        get { return WPUser.GetValue("user.home-info.telecom.telephone.intcode");}
                    }
                    
                    public string loccode
                    {
                        get { return WPUser.GetValue("user.home-info.telecom.telephone.loccode");}
                    }
                    
                    public string number
                    {
                        get { return WPUser.GetValue("user.home-info.telecom.telephone.number");}
                    }

                    public string ext
                    {
                        get { return WPUser.GetValue("user.home-info.telecom.telephone.ext");}
                    }

                    public string comment
                    {
                        get { return WPUser.GetValue("user.home-info.telecom.telephone.comment");}
                    }
                }
                #endregion

                #region user.home-info.telecom.fax class
                public class Fax 
                {
                    public string intcode
                    {
                        get { return WPUser.GetValue("user.home-info.telecom.fax.intcode");}
                    }
                    
                    public string loccode
                    {
                        get { return WPUser.GetValue("user.home-info.telecom.fax.loccode");}
                    }
                    
                    public string number
                    {
                        get { return WPUser.GetValue("user.home-info.telecom.fax.number");}
                    }

                    public string ext
                    {
                        get { return WPUser.GetValue("user.home-info.telecom.fax.ext");}
                    }
        
                    public string comment
                    {
                        get { return WPUser.GetValue("user.home-info.telecom.fax.comment");}
                    }
                }
                #endregion

                #region user.home-info.telecom.mobile class
                public class Mobile 
                {
                    public string intcode
                    {
                        get { return WPUser.GetValue("user.home-info.telecom.mobile.intcode");}
                    }
                    public string loccode
                    {
                        get { return WPUser.GetValue("user.home-info.telecom.mobile.loccode");}
                    }
                    public string number
                    {
                        get { return WPUser.GetValue("user.home-info.telecom.mobile.number");}
                    }
                    public string ext
                    {
                        get { return WPUser.GetValue("user.home-info.telecom.mobile.ext");}
                    }
                    public string comment
                    {
                        get { return WPUser.GetValue("user.home-info.telecom.mobile.comment");}
                    }
                }
                #endregion

                #region user.home-info.telecom.pager class
                public class Pager 
                {
                    public string intcode 
                    {
                        get {return WPUser.GetValue("user.home-info.telecom.pager.intcode");}
                    }

                    public string loccode 
                    {
                        get {return WPUser.GetValue("user.home-info.telecom.pager.loccode");}
                    }

                    public string number 
                    {
                        get {return WPUser.GetValue("user.home-info.telecom.pager.number");}
                    }

                    public string ext 
                    {
                        get {return WPUser.GetValue("user.home-info.telecom.pager.ext");}
                    }

                    public string comment
                    {
                        get {return WPUser.GetValue("user.home-info.telecom.pager.comment");}
                    }
                }
                #endregion
            }
            #endregion

            #region user.home-info.online class
            public class Online
            {
                public string email
                {
                    get { return WPUser.GetValue("user.home-info.online.email");}
                }

                public string uri
                {
                    get { return WPUser.GetValue("user.home-info.online.uri"); }
                }
            }
            #endregion
        }
        #endregion

        #region user.business-info class
        public class BusinessInfo
        {

            Postal _postal = new Postal();
            Telecom _telecom = new Telecom();
            Online _online = new Online();

            public Postal postal
            {
                get { return _postal; }
            }

            public Telecom telecom
            {
                get { return _telecom; }
            }

            public Online online
            {
                get { return _online; }
            }

            #region user.business-info.postal class
            public class Postal
            {
                public string name 
                {
                    get { return WPUser.GetValue("user.business-info.postal.name");}
                }
                
                public string street
                {
                    get { return WPUser.GetValue("user.business-info.postal.street");}
                }

                public string city
                {
                    get { return WPUser.GetValue("user.business-info.postal.city");}
                }
                
                public string stateprov
                {
                    get { return WPUser.GetValue("user.business-info.postal.stateprov");}
                }
                
                public string postalcode
                {
                    get { return WPUser.GetValue("user.business-info.postal.postalcode");}
                }
                
                public string country
                {
                    get { return WPUser.GetValue("user.business-info.postal.country");}
                }

                public string organization
                {
                    get { return WPUser.GetValue("user.business-info.postal.organization");}
                }

            }
            #endregion

            #region user.business-info.telecom
            public class Telecom
            {
                private Pager _pager = new Pager();
                private Telephone _telephone = new Telephone();
                private Fax _fax = new Fax();
                private Mobile _mobile = new Mobile();


                public Pager pager { get { return _pager; } }
                public Telephone telephone { get { return _telephone; } }
                public Fax fax { get { return _fax; } }
                public Mobile mobile { get { return _mobile; } }

                public class Pager
                {
                    public string intcode
                    {
                        get { return WPUser.GetValue("user.business-info.telecom.pager.intcode"); }
                    }

                    public string loccode
                    {
                        get { return WPUser.GetValue("user.business-info.telecom.pager.loccode"); }
                    }

                    public string number
                    {
                        get { return WPUser.GetValue("user.business-info.telecom.pager.number"); }
                    }

                    public string ext
                    {
                        get { return WPUser.GetValue("user.business-info.telecom.pager.ext"); }
                    }

                    public string comment
                    {
                        get { return WPUser.GetValue("user.business-info.telecom.pager.comment"); }
                    }
                }

                public class Mobile
                {
                    public string intcode
                    {
                        get { return WPUser.GetValue("user.business-info.telecom.mobile.intcode"); }
                    }
                    public string loccode
                    {
                        get { return WPUser.GetValue("user.business-info.telecom.mobile.loccode"); }
                    }
                    public string number
                    {
                        get { return WPUser.GetValue("user.business-info.telecom.mobile.number"); }
                    }
                    public string ext
                    {
                        get { return WPUser.GetValue("user.business-info.telecom.mobile.ext"); }
                    }
                    public string comment
                    {
                        get { return WPUser.GetValue("user.business-info.telecom.mobile.comment"); }
                    }
                }

                public class Telephone
                {
                    public string intcode 
                    {
                        get {return WPUser.GetValue("user.business-info.telecom.telephone.intcode");}
                    }

                    public string loccode 
                    {
                        get {return WPUser.GetValue("user.business-info.telecom.telephone.loccode");}
                    }
                    
                    public string number
                    {
                        get {return WPUser.GetValue("user.business-info.telecom.telephone.number");}
                    }

                    public string ext
                    {
                        get {return WPUser.GetValue("user.business-info.telecom.telephone.ext");}
                    }

                    public string comment
                    {
                        get {return WPUser.GetValue("user.business-info.telecom.telephone.comment");}
                    }     
                }

                public class Fax
                {
                    public string intcode
                    {
                        get { return WPUser.GetValue("user.business-info.telecom.fax.intcode"); }
                    }

                    public string loccode
                    {
                        get { return WPUser.GetValue("user.business-info.telecom.fax.loccode"); }
                    }

                    public string number
                    {
                        get { return WPUser.GetValue("user.business-info.telecom.fax.number"); }
                    }

                    public string ext
                    {
                        get { return WPUser.GetValue("user.business-info.telecom.fax.ext"); }
                    }

                    public string comment
                    {
                        get { return WPUser.GetValue("user.business-info.telecom.fax.comment"); }
                    }
                }
            }
            #endregion

            #region user.business-info.online
            public class Online
            {
                public string email
                {
                    get { return WPUser.GetValue("user.business-info.online.email"); }
                }

                public string uri
                {
                    get { return WPUser.GetValue("user.business-info.online.uri"); }
                }
            }
            #endregion

        }
        #endregion

        #region user.name class
        public class UserName
        {
            public string prefix
            {
                get { return WPUser.GetValue("user.name.prefix"); }
            }

            public string given
            {
                get { return WPUser.GetValue("user.name.given"); }
            }

            public string family
            {
                get { return WPUser.GetValue("user.name.family"); }
            }

            public string middle
            {
                get { return WPUser.GetValue("user.name.middle"); }
            }

            public string suffix
            {
                get { return WPUser.GetValue("user.name.suffix"); }
            }

            public string nickName
            {
                get { return WPUser.GetValue("user.name.nickName"); }
            }
        }
        #endregion
    }
    #endregion
}
#endif
