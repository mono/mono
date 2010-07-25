//
// Mainsoft.Web.Security.Tests.DerbyMembershipProviderTests
//
// Authors:
//	Vladimir Krasnov (vladimirk@mainsoft.com)
//
// (C) 2006 Mainsoft
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
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Configuration;
using System.Web.Security;
using System.Data;
using System.Data.OleDb;
using System.Data.Common;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Mainsoft.Web.Security;
using NUnit.Framework;
 

namespace Mainsoft.Web.Security.Tests
{
	[TestFixture] 
	public class DerbyMembershipProviderTests
	{
		public void Init ()
		{
			InitDB ();
		}
		public void InitDB ()
		{
			if (Directory.Exists ("AspDB"))
				Directory.Delete ("AspDB", true);
		}
		public void Finish ()
		{
			ReleaseDB ();
		}

		public void ReleaseDB ()
		{
		}
		private MembershipProvider CreateMembershipProvider ()
		{
			NameValueCollection nvc = new NameValueCollection ();
			nvc.Add ("applicationName", "testapp");
			nvc.Add ("connectionStringName", "derby");
			nvc.Add ("requiresUniqueEmail", "false");
			nvc.Add ("enablePasswordRetrieval", "true");
			nvc.Add ("passwordFormat", "Clear");

			WebConfigurationManager.ConnectionStrings.Add (new System.Configuration.ConnectionStringSettings ("derby", "JdbcDriverClassName=org.apache.derby.jdbc.EmbeddedDriver;JdbcURL=jdbc:derby:AspDB;create=true", "System.Data.OleDb"));
			DerbyMembershipProvider p = new DerbyMembershipProvider ();
			p.Initialize ("DerbyMembershipProvider", nvc);

			return p;
		}

		[Test] 
		public void CreateUserTest ()
		{
			MembershipProvider p = CreateMembershipProvider ();
			MembershipCreateStatus st;
			MembershipUser u = p.CreateUser("username", "123123!", "username@email.com", "q", "a", true, null, out st);
			Assert.IsNotNull (u);
			Assert.AreEqual ("username", u.UserName);
		}

		[Test] 
		public void ChangePasswordTest ()
		{
			MembershipProvider p = CreateMembershipProvider ();
			MembershipCreateStatus st;
			MembershipUser u = p.CreateUser("userpwd", "123123!", "username2@email.com", "q", "a", true, null, out st);
			bool b = p.ChangePassword ("userpwd", "123123!", "123123!123");
			Assert.IsTrue (b);

			b = p.ChangePassword ("userpwd", "123123!", "123123!123");
			Assert.IsFalse (b);
		}

		[Test] 
		public void ChangePasswordQuestionAndAnswerTest ()
		{
			MembershipProvider p = CreateMembershipProvider ();
			MembershipCreateStatus st;
			MembershipUser u = p.CreateUser("userpwd2", "123123!", "username3@email.com", "q", "a", true, null, out st);

			bool b = p.ChangePasswordQuestionAndAnswer ("userpwd2", "123123!", "q2", "a2");
			Assert.IsTrue (b);
			b = p.ChangePasswordQuestionAndAnswer ("userpwd2", "123123!123", "q2", "a2");
			Assert.IsFalse (b);
		}

		[Test] 
		public void DeleteUserTest ()
		{
			MembershipProvider p = CreateMembershipProvider ();
			MembershipCreateStatus st;
			p.CreateUser ("user3", "123123!", "username4@email.com", "q", "a", true, null, out st);

			bool b = p.DeleteUser ("user3", true);
			Assert.IsTrue (b);

			MembershipUser u = p.GetUser ("user2", false);
			Assert.IsNotNull (u);
		}

		[Test] 
		public void GetUserNameByEmailTest ()
		{
			MembershipProvider p = CreateMembershipProvider ();
			string u = p.GetUserNameByEmail ("username@email.com");
			Assert.AreEqual ("username", u);
		}

		[Test] 
		public void ValidateUserTest ()
		{
			MembershipProvider p = CreateMembershipProvider ();
			bool b = p.ValidateUser ("username", "123");
			Assert.IsFalse (b);
			b = p.ValidateUser ("username", "123");
			Assert.IsFalse (b);
			b = p.ValidateUser ("username", "123");
			Assert.IsFalse (b);
			b = p.ValidateUser ("username", "123");
			Assert.IsFalse (b);
			b = p.ValidateUser ("username", "123");
			Assert.IsFalse (b);
			b = p.ValidateUser ("username", "123");
			Assert.IsFalse (b);
			b = p.ValidateUser ("username", "123");
			Assert.IsFalse (b);
			b = p.ValidateUser ("username", "123");
			Assert.IsFalse (b);
			b = p.UnlockUser ("username");
			Assert.IsFalse (b);
		}

		[Test] 
		public void UpdateUserTest ()
		{
			MembershipProvider p = CreateMembershipProvider ();
			MembershipCreateStatus st;
			MembershipUser u = p.CreateUser ("user5", "123123!", "user5@email.com", "q", "a", true, null, out st);
			
			u.Comment = "comment2";
			u.Email = "email2";
			u.IsApproved = false;
		
			p.UpdateUser (u);
			MembershipUser u2 = p.GetUser (u.ProviderUserKey, false);

			Assert.AreEqual (u.Comment, u2.Comment);
			Assert.AreEqual (u.Email, u2.Email);
			Assert.AreEqual (u.IsApproved, u2.IsApproved);
		}

		[Test] 
		public void GetPasswordTest ()
		{
			MembershipProvider p = CreateMembershipProvider ();
			MembershipCreateStatus st;
			p.CreateUser ("user7", "123123!", "user5@email.com", "q", "a", true, null, out st);
			string pass = p.GetPassword ("user7", "a");
			Assert.AreEqual ("123123!", pass);
		}

		[Test] 
		public void FindUsersByName ()
		{
			MembershipProvider p = CreateMembershipProvider ();
			MembershipCreateStatus st;
			p.CreateUser ("user_a", "123123!", "user_a@email.com", "q", "a", true, null, out st);
			p.CreateUser ("user_b", "123123!", "user_b@email.com", "q", "a", true, null, out st);
			p.CreateUser ("user_c", "123123!", "user_c@email.com", "q", "a", true, null, out st);
			p.CreateUser ("user_d", "123123!", "user_d@email.com", "q", "a", true, null, out st);
			p.CreateUser ("user_e", "123123!", "user_e@email.com", "q", "a", true, null, out st);
			int tr = 0;
			MembershipUserCollection u = p.FindUsersByName ("%user_%", 0, 10, out tr);
			Assert.AreEqual (5, tr);
			Assert.AreEqual (5, u.Count);
		}

		[Test] 
		public void FindUsersByEmail ()
		{
			MembershipProvider p = CreateMembershipProvider ();
			MembershipCreateStatus st;
			p.CreateUser ("user_7a", "123123!", "user_7a@email.com", "q", "a", true, null, out st);
			p.CreateUser ("user_7b", "123123!", "user_7b@email.com", "q", "a", true, null, out st);
			p.CreateUser ("user_7c", "123123!", "user_7c@email.com", "q", "a", true, null, out st);
			p.CreateUser ("user_7d", "123123!", "user_7d@email.com", "q", "a", true, null, out st);
			p.CreateUser ("user_7e", "123123!", "user_7e@email.com", "q", "a", true, null, out st);
			int tr = 0;
			MembershipUserCollection u = p.FindUsersByEmail ("%user_7%", 0, 10, out tr);
			Assert.AreEqual (5, tr);
			Assert.AreEqual (5, u.Count);
		}
	}
}

#endif