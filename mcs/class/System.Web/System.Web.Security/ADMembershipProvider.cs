//
// System.Web.Security.ADMembershipProvider
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace System.Web.Security {
	public class ADMembershipProvider : IMembershipProvider {
		
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

