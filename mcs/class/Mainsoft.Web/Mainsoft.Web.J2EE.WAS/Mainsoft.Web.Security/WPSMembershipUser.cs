#if NET_2_0
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web.Security;

using com.ibm.portal.um;

namespace Mainsoft.Web.Security
{
    public class WPSMembershipUser : MembershipUser
    {
        private User _wpsUser;
        private string _userName;

        private object _providerKey;

        internal WPSMembershipUser(User user)
            : base()
        {
            if (user == null)
                throw new ArgumentNullException("user");

            _wpsUser = user;
            InitAspNetProperties();
        }

        internal WPSMembershipUser(User user, object providerKye)
            : this(user)
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
                return WPSMembershipProvider.PROVIDER_NAME;
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
                WPSMembershipUser u1 = (WPSMembershipUser)p1;
                WPSMembershipUser u2 = (WPSMembershipUser)p2;

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