//
// Mainsoft.Web.Security.WPMembershipUser
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
using System.Collections.Generic;
using System.Text;
using System.Web.Security;

using com.ibm.portal.um;

namespace Mainsoft.Web.Security
{
    public class WPMembershipUser : MembershipUser
    {
        private User _wpsUser;
        private string _userName;
        private MembershipProvider _provider;

        private object _providerKey;

        internal WPMembershipUser(User user, MembershipProvider provider)
            : base()
        {
            if (user == null)
                throw new ArgumentNullException("user");

            _provider = provider;
            _wpsUser = user;
            InitAspNetProperties();
        }

        internal WPMembershipUser(User user, object providerKye, MembershipProvider provider)
            : this(user, provider)
        {
            _providerKey = providerKye;
        }

        public virtual User WPSUser
        {
            get { return _wpsUser; }
        }

        private void InitAspNetProperties()
        {
            IPumaServicesProvider services = PumaServicesProviderFactory.CreateProvider();
            IDictionary attributes = services.GetAllAttributes(_wpsUser);
            _userName = (string) attributes["uid"];
            this.Email = (string)attributes["ibm-primaryEmail"];

        }

        public override string UserName
        {
            get
            {
                return _userName;
            }
        }

        public override string ProviderName
        {
            get
            {
                if (_provider != null)
                    return _provider.Name;
                
                return null;
            }
        }

        public override object ProviderUserKey
        {
            get
            {
                if (_providerKey == null)
                    _providerKey = this.UserName;

                return _providerKey;
            }
        }

        internal static java.util.Comparator UserNameComparator
        {
            get { return UIDComparator.INSTANCE; }
        }


        internal class UIDComparator : java.util.Comparator
        {
            internal static readonly UIDComparator INSTANCE = new UIDComparator();

            public int compare(object p1, object p2)
            {
                WPMembershipUser u1 = (WPMembershipUser)p1;
                WPMembershipUser u2 = (WPMembershipUser)p2;

                return u1.UserName.CompareTo(u2.UserName);
            }
        }



        //#region Properties
        //public override string Comment
        //{
        //    get
        //    {
        //        return base.Comment;
        //    }
        //    set
        //    {
        //        base.Comment = value;
        //    }
        //}

        //public override DateTime CreationDate
        //{
        //    get
        //    {
        //        return base.CreationDate;
        //    }
        //}

        

        //public override bool IsApproved
        //{
        //    get
        //    {
        //        return base.IsApproved;
        //    }
        //    set
        //    {
        //        base.IsApproved = value;
        //    }
        //}

        //public override bool IsLockedOut
        //{
        //    get
        //    {
        //        return base.IsLockedOut;
        //    }
        //}

        //public override DateTime LastActivityDate
        //{
        //    get
        //    {
        //        return base.LastActivityDate;
        //    }
        //    set
        //    {
        //        base.LastActivityDate = value;
        //    }
        //}

        //public override DateTime LastLockoutDate
        //{
        //    get
        //    {
        //        return base.LastLockoutDate;
        //    }
        //}

        //public override DateTime LastLoginDate
        //{
        //    get
        //    {
        //        return base.LastLoginDate;
        //    }
        //    set
        //    {
        //        base.LastLoginDate = value;
        //    }
        //}

        //public override DateTime LastPasswordChangedDate
        //{
        //    get
        //    {
        //        return base.LastPasswordChangedDate;
        //    }
        //}

        //public override string PasswordQuestion
        //{
        //    get
        //    {
        //        return base.PasswordQuestion;
        //    }
        //}

       
        //#endregion

        //#region Methods

        //public override bool ChangePassword(string oldPassword, string newPassword)
        //{
        //    return base.ChangePassword(oldPassword, newPassword);
        //}

        //public override bool ChangePasswordQuestionAndAnswer(string password, string newPasswordQuestion, string newPasswordAnswer)
        //{
        //    return base.ChangePasswordQuestionAndAnswer(password, newPasswordQuestion, newPasswordAnswer);
        //}

        //public override bool Equals(object obj)
        //{
        //    return base.Equals(obj);
        //}

        //#endregion

    }
}
#endif