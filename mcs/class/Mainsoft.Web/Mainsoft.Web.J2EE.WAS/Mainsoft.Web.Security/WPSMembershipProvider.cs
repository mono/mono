//
// Mainsoft.Web.Security.WPSMembershipProvider
//
// Authors:
//	Ilya Kharmatsky (ilyak at mainsoft.com)
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
using System.Web;
using System.Web.Security;


using java.util;

using com.ibm.portal.um;

namespace Mainsoft.Web.Security
{

    public class WPSMembershipProvider : MembershipProvider
    {
        private static readonly string PROVIDER_DESCRIPTION = "WebSphere Portal Membership Provider";
        internal static readonly string PROVIDER_NAME = "WPSMembershipProvider";

        private string applicationName;

        #region Properties
        public override string ApplicationName
        {
            get
            {
                lock (this)
                {
                    if (applicationName == null)
                        applicationName = AppDomain.CurrentDomain.FriendlyName;
                }
                return applicationName;
            }
            set
            {
                lock (this)
                {
                    applicationName = value;
                }
            }
        }

        public override string Description 
        {
            get { return PROVIDER_DESCRIPTION; }
        }

        public override bool EnablePasswordReset
        {
            get { return false; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return false; }
        }

        [MonoTODO]
        public override int MaxInvalidPasswordAttempts
        {
            get { throw new NotImplementedException(); }
        }

        [MonoTODO]
        public override int MinRequiredNonAlphanumericCharacters
        {
            get { throw new NotImplementedException(); }
        }

        [MonoTODO]
        public override int MinRequiredPasswordLength
        {
            get { throw new NotImplementedException(); }
        }

        public override string Name
        {
            get { return PROVIDER_NAME; }
        }

        [MonoTODO]
        public override int PasswordAttemptWindow
        {
            get { throw new NotImplementedException("The method or operation is not implemented."); }
        }

        [MonoTODO]
        public override MembershipPasswordFormat PasswordFormat
        {
            get { throw new NotImplementedException("The method or operation is not implemented."); }
        }

        [MonoTODO]
        public override string PasswordStrengthRegularExpression
        {
            get { throw new NotImplementedException("The method or operation is not implemented."); }
        }

        [MonoTODO]
        public override bool RequiresQuestionAndAnswer
        {
            get { throw new NotImplementedException("The method or operation is not implemented."); }
        }

        [MonoTODO]
        public override bool RequiresUniqueEmail
        {
            get { throw new NotImplementedException("The method or operation is not implemented."); }
        }


        #endregion

        #region Implemented Methods
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            if (emailToMatch == null || emailToMatch.Length > 256) // TODO check if the string could be null, if yes replace it with "*" for any email
                throw new ArgumentException("Argument emailToMatch either null or length > 256", "emailToMatch");
            if (pageIndex < 0)
                throw new ArgumentException("Argument pageIndex could not be negative", "pageIndex");
            if (pageSize < 1)
                throw new ArgumentException("Argument pageSize could not be less than one", "pageSize");

            long upperBound = (long)pageIndex * pageSize + pageSize - 1;
            if (upperBound > Int32.MaxValue)
                throw new ArgumentException("The pageSize and pageIndex produce too long number");


            IPumaServicesProvider pumaServices = PumaServicesProviderFactory.CreateProvider();
            //Will get the list of : List<com.ibm.portal.um.User> - which should be translated to
            //System.Web.Security.MembershipUser instances...
            //TODO: The attribute "ibm-primaryEmail" is hardcoded right now - should be updated
            //to be configurable.
            java.util.List principles = 
                pumaServices.PumaLocator.findUsersByAttribute("ibm-primaryEmail", emailToMatch);


            MembershipUserCollection resCollection = new MembershipUserCollection();
            SliceRange sr = new SliceRange(pageSize, pageIndex);
            CopyToMembershipUserCollection(principles, resCollection, sr);

            //Should contain total records recieved.
            totalRecords = principles.size();
            return resCollection;

        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            if (usernameToMatch == null || usernameToMatch.Trim().Length == 0 || usernameToMatch.Trim().Length > 256)
                throw new ArgumentException("Wrong username given as a parameter - could not be null or empty string and could not have more than 255 characters", usernameToMatch);
            if (pageIndex < 0)
                throw new ArgumentException("Argument pageIndex could not be negative", "pageIndex");
            if (pageSize < 1)
                throw new ArgumentException("Argument pageSize could not be less than one", "pageSize");

            long upperBound = (long)pageIndex * pageSize + pageSize - 1;
            if (upperBound > Int32.MaxValue)
                throw new ArgumentException("The pageSize and pageIndex produce too long number");

            IPumaServicesProvider pumaServices = PumaServicesProviderFactory.CreateProvider();

            java.util.List principles =
               pumaServices.PumaLocator.findUsersByAttribute("uid", usernameToMatch);

            MembershipUserCollection resCollection = new MembershipUserCollection();
            SliceRange sr = new SliceRange(pageSize, pageIndex);

            CopyToMembershipUserCollection(principles, resCollection, sr);
            

            //Should contain total records recieved.
            totalRecords = principles.size();
            return resCollection;

        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            if (pageIndex < 0)
                throw new ArgumentException("Argument pageIndex could not be negative", "pageIndex");
            if (pageSize < 1)
                throw new ArgumentException("Argument pageSize could not be less than one", "pageSize");
            long upperBound = (long)pageIndex * pageSize + pageSize - 1;
            if (upperBound > Int32.MaxValue)
                throw new ArgumentException("The pageSize and pageIndex produce too long number");

            IPumaServicesProvider pumaServices = PumaServicesProviderFactory.CreateProvider();

            java.util.List principles =
               pumaServices.PumaLocator.findUsersByDefaultAttribute("*");

            MembershipUserCollection resCollection = new MembershipUserCollection();
            SliceRange sr = new SliceRange(pageSize, pageIndex);
            CopyToMembershipUserCollection(principles, resCollection, sr);
            

            //Should contain total records recieved.
            totalRecords = principles.size();
            return resCollection;

        }

        //the userIsOnline ignored
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            if (username.Length > 256)
                throw new ArgumentException("The username is too long", username);
            if (username == null || username == String.Empty)
            {
                com.ibm.portal.um.User user = 
                    PumaServicesProviderFactory.CreateProvider().PumaProfile.getCurrentUser();
                return new WPSMembershipUser(user);
            }
            java.util.List principles = 
                PumaServicesProviderFactory.CreateProvider().PumaLocator.findUsersByAttribute("uid", username);
            MembershipUser result = null;
            if (principles.size() > 0)
                result = new WPSMembershipUser((com.ibm.portal.um.User)principles.get(0));

            return result;

        }
        /* 
         * partially implemented :
         * the userIsOnline - ignored
         * the providerUserKey - is always considered as a UID
         * 
         */
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            if (providerUserKey is string)
                return GetUser((string)providerUserKey, userIsOnline);
            throw new NotImplementedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            //TODO check if email is NULL or empty - what is .Net beh.?
            if (email == null || email.Trim().Length == 0 || email.Trim().Length > 256)
                throw new ArgumentException("wrong format of parameter", "email");
            IPumaServicesProvider services = PumaServicesProviderFactory.CreateProvider();
            java.util.List principles =
                services.PumaLocator.findUsersByAttribute("ibm-primaryEmail", email);
            if (principles.size() == 0)
                return null;


            IDictionary dic = 
                services.GetAttributes((com.ibm.portal.um.User)principles.get(0), "uid");
            return (string) dic["uid"];
        }

        #region Helper Methods
        internal void CopyToMembershipUserCollection(java.util.List principles,
            MembershipUserCollection resCollection, SliceRange sr)
        {            
            if (sr.start >= principles.size())
                return;
         
            java.util.ArrayList sortedList = new java.util.ArrayList(principles.size());
            for (java.util.Iterator iter = principles.iterator(); iter.hasNext(); )
                sortedList.add(new WPSMembershipUser((User)iter.next()));
            
            java.util.Collections.sort(sortedList, WPSMembershipUser.UserNameComparator);

            int sortedListSize = sortedList.size();
            
            for (int i = sr.start; i < sortedListSize && i < sr.end; i++)
                resCollection.Add((MembershipUser)sortedList.get(i));
            
        }
        #endregion

        #endregion

        [MonoTODO]
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        [MonoTODO]
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        [MonoTODO]
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        [MonoTODO]
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }       

       
        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

       

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public override bool ValidateUser(string username, string password)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        #region Helper Classes and Structs

        internal struct SliceRange
        {
            internal int start;
            internal int end;

            public SliceRange(int pageSize, int pageIndex)
            {
                switch (pageIndex)
                {
                    case 0: start = 0; end = pageSize; break;
                    case 1: start = pageSize; end = 2 * pageSize; break;
                    default: start = (pageIndex - 1) * pageSize; end = pageIndex * pageSize; break;
                }

            }
        }

        #endregion
    }
}
#endif
