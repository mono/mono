//
// System.Web.Security.AccessMembershipProvider
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
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
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace System.Web.Security {
	public class AccessMembershipProvider : IMembershipProvider {
		
		[MonoTODO]
		public virtual bool ChangePassword (string username, string oldPwd, string newPwd)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual bool ChangePasswordQuestionAndAnswer (string username, string password, string newPwdQuestion, string newPwdAnswer)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual MembershipUser CreateUser (string username, string password, string email,  out MembershipCreateStatus status)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual bool DeleteUser (string username)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual string GeneratePassword ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public MembershipUserCollection GetAllUsers ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public int GetNumberOfUsersOnline ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual string GetPassword (string username, string answer)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual MembershipUser GetUser (string username, bool userIsOnline)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual string GetUserNameByEmail (string email)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual void Initialize (string name, NameValueCollection config)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual string ResetPassword (string username, string answer)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual void UpdateUser (MembershipUser user)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public virtual bool ValidateUser (string username, string password)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public string ApplicationName {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public virtual string Description {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public virtual bool EnablePasswordReset {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public virtual bool EnablePasswordRetrieval {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public virtual string Name {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public virtual MembershipPasswordFormat PasswordFormat {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public virtual bool RequiresQuestionAndAnswer {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public virtual bool RequiresUniqueEmail {
			get { throw new NotImplementedException (); }
		}
	}
}
#endif

