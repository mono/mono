//
// ClaimsIdentityTest.cs - NUnit Test Cases for System.Security.Claims.ClaimsIdentity
//


using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace MonoTests.System.Security.Claims
{
	[TestFixture]
	public class ClaimsIdentityTest
	{
		#region Ctor Empty

		[Test]
		public void EmptyCtorWorks ()
		{
			var id = new ClaimsIdentity ();
			Assert.IsNull (id.AuthenticationType, "#1");
			Assert.IsNull (id.Actor, "#2");
			Assert.IsNull (id.BootstrapContext, "#3");
			Assert.IsNotNull (id.Claims, "#4");
			Assert.AreEqual (0, id.Claims.Count (), "#5");
			Assert.IsFalse (id.IsAuthenticated, "#6");
			Assert.IsNull (id.Label, "#7");
			Assert.IsNull (id.Name, "#8");
			Assert.AreEqual (ClaimsIdentity.DefaultNameClaimType, id.NameClaimType, "#9");
			Assert.AreEqual (ClaimsIdentity.DefaultRoleClaimType, id.RoleClaimType, "#10");
		}

		#endregion

		#region Ctor AuthTypeOnly

		[Test]
		public void AuthTypeOnlyCtorEmptyWorks ()
		{
			var id = new ClaimsIdentity ("");
			//NOTE: According to MSDN the id.AuthenticationType should be null, but it isn't on .Net 4.5
			//Assert.IsNull (id.AuthenticationType, "#1");
			Assert.AreEqual ("", id.AuthenticationType, "#1");
			Assert.IsNull (id.Actor, "#2");
			Assert.IsNull (id.BootstrapContext, "#3");
			Assert.IsNotNull (id.Claims, "#4");
			Assert.AreEqual (0, id.Claims.Count (), "#5");
			Assert.IsFalse (id.IsAuthenticated, "#6");
			Assert.IsNull (id.Label, "#7");
			Assert.IsNull (id.Name, "#8");
			Assert.AreEqual (ClaimsIdentity.DefaultNameClaimType, id.NameClaimType, "#9");
			Assert.AreEqual (ClaimsIdentity.DefaultRoleClaimType, id.RoleClaimType, "#10");
		}

		[Test]
		public void AuthTypeOnlyCtorNullWorks ()
		{
			var id = new ClaimsIdentity ((string)null);
			Assert.IsNull (id.AuthenticationType, "#1");
			Assert.IsNull (id.Actor, "#2");
			Assert.IsNull (id.BootstrapContext, "#3");
			Assert.IsNotNull (id.Claims, "#4");
			Assert.AreEqual (0, id.Claims.Count (), "#5");
			Assert.IsFalse (id.IsAuthenticated, "#6");
			Assert.IsNull (id.Label, "#7");
			Assert.IsNull (id.Name, "#8");
			Assert.AreEqual (ClaimsIdentity.DefaultNameClaimType, id.NameClaimType, "#9");
			Assert.AreEqual (ClaimsIdentity.DefaultRoleClaimType, id.RoleClaimType, "#10");
		}

		[Test]
		public void AuthTypeOnlyCtorNotEmptyWorks ()
		{
			var id = new ClaimsIdentity ("auth_type");
			Assert.AreEqual ("auth_type", id.AuthenticationType, "#1");
			Assert.IsNull (id.Actor, "#2");
			Assert.IsNull (id.BootstrapContext, "#3");
			Assert.IsNotNull (id.Claims, "#4");
			Assert.AreEqual (0, id.Claims.Count (), "#5");
			Assert.IsTrue (id.IsAuthenticated, "#6");
			Assert.IsNull (id.Label, "#7");
			Assert.IsNull (id.Name, "#8");
			Assert.AreEqual (ClaimsIdentity.DefaultNameClaimType, id.NameClaimType, "#9");
			Assert.AreEqual (ClaimsIdentity.DefaultRoleClaimType, id.RoleClaimType, "#10");
		}

		#endregion

		#region Ctor IEnumerable<Claim>

		[Test]
		public void EnumClaimsCtorNullWorks ()
		{
			var id = new ClaimsIdentity ((IEnumerable<Claim>)null);
			Assert.IsNull (id.AuthenticationType, "#1");
			Assert.IsNull (id.Actor, "#2");
			Assert.IsNull (id.BootstrapContext, "#3");
			Assert.IsNotNull (id.Claims, "#4");
			Assert.AreEqual (0, id.Claims.Count (), "#5");
			Assert.IsFalse (id.IsAuthenticated, "#6");
			Assert.IsNull (id.Label, "#7");
			Assert.IsNull (id.Name, "#8");
			Assert.AreEqual (ClaimsIdentity.DefaultNameClaimType, id.NameClaimType, "#9");
			Assert.AreEqual (ClaimsIdentity.DefaultRoleClaimType, id.RoleClaimType, "#10");
		}

		[Test]
		public void EnumClaimsCtorEmptyWorks ()
		{
			var id = new ClaimsIdentity ((IEnumerable<Claim>)null);
			Assert.IsNull (id.AuthenticationType, "#1");
			Assert.IsNull (id.Actor, "#2");
			Assert.IsNull (id.BootstrapContext, "#3");
			Assert.IsNotNull (id.Claims, "#4");
			Assert.AreEqual (0, id.Claims.Count (), "#5");
			Assert.IsFalse (id.IsAuthenticated, "#6");
			Assert.IsNull (id.Label, "#7");
			Assert.IsNull (id.Name, "#8");
			Assert.AreEqual (ClaimsIdentity.DefaultNameClaimType, id.NameClaimType, "#9");
			Assert.AreEqual (ClaimsIdentity.DefaultRoleClaimType, id.RoleClaimType, "#10");
		}

		[Test]
		public void EnumClaimsCtorWithClaimsWithNameWorks ()
		{
			var id = new ClaimsIdentity (
					   new [] {
					new Claim ("claim_type", "claim_value"),
					new Claim (ClaimsIdentity.DefaultNameClaimType, "claim_name_value"), 
				});
			Assert.IsNull (id.AuthenticationType, "#1");
			Assert.IsNull (id.Actor, "#2");
			Assert.IsNull (id.BootstrapContext, "#3");
			Assert.IsNotNull (id.Claims, "#4");
			Assert.AreEqual (2, id.Claims.Count (), "#5");
			Assert.IsFalse (id.IsAuthenticated, "#6");
			Assert.IsNull (id.Label, "#7");
			Assert.AreEqual ("claim_name_value", id.Name, "#8");
			Assert.AreEqual (ClaimsIdentity.DefaultNameClaimType, id.NameClaimType, "#9");
			Assert.AreEqual (ClaimsIdentity.DefaultRoleClaimType, id.RoleClaimType, "#10");
		}

		[Test]
		public void EnumClaimsCtorWithClaimsWithoutNameWorks ()
		{
			var id = new ClaimsIdentity (
					   new [] {
					new Claim ("claim_type", "claim_value"),
					new Claim (ClaimsIdentity.DefaultNameClaimType + "_x", "claim_name_value"), 
				});
			Assert.IsNull (id.AuthenticationType, "#1");
			Assert.IsNull (id.Actor, "#2");
			Assert.IsNull (id.BootstrapContext, "#3");
			Assert.IsNotNull (id.Claims, "#4");
			Assert.AreEqual (2, id.Claims.Count (), "#5");
			Assert.IsFalse (id.IsAuthenticated, "#6");
			Assert.IsNull (id.Label, "#7");
			Assert.IsNull (id.Name, "#8");
			Assert.AreEqual (ClaimsIdentity.DefaultNameClaimType, id.NameClaimType, "#9");
			Assert.AreEqual (ClaimsIdentity.DefaultRoleClaimType, id.RoleClaimType, "#10");
		}

		#endregion

		#region Ctor IEnumerable<Claim>, authType, nameType, roleType

		[Test]
		public void EnumClaimsAuthNameRoleTypeCtorWorks ()
		{
			var id = new ClaimsIdentity (new [] {
				new Claim ("claim_type", "claim_value"),
				new Claim (ClaimsIdentity.DefaultNameClaimType, "claim_name_value"), 
				new Claim ("claim_role_type", "claim_role_value"), 
			},
					   "test_auth_type", "test_name_type", "claim_role_type");
			Assert.AreEqual ("test_auth_type", id.AuthenticationType, "#1");
			Assert.IsNull (id.Actor, "#2");
			Assert.IsNull (id.BootstrapContext, "#3");
			Assert.IsNotNull (id.Claims, "#4");
			Assert.AreEqual (3, id.Claims.Count (), "#5");
			Assert.IsTrue (id.IsAuthenticated, "#6");
			Assert.IsNull (id.Label, "#7");
			Assert.IsNull (id.Name, "#8");
			Assert.AreEqual ("test_name_type", id.NameClaimType, "#9");
			Assert.AreEqual ("claim_role_type", id.RoleClaimType, "#10");
		}

		[Test]
		public void EnumClaimsAuthNameRoleTypeCtorAllArgsNullWorks ()
		{
			var id = new ClaimsIdentity ((IEnumerable<Claim>)null, (string)null, (string)null, (string)null);
			Assert.IsNull (id.AuthenticationType, "#1");
			Assert.IsNull (id.Actor, "#2");
			Assert.IsNull (id.BootstrapContext, "#3");
			Assert.IsNotNull (id.Claims, "#4");
			Assert.AreEqual (0, id.Claims.Count (), "#5");
			Assert.IsFalse (id.IsAuthenticated, "#6");
			Assert.IsNull (id.Label, "#7");
			Assert.IsNull (id.Name, "#8");
			Assert.AreEqual (ClaimsIdentity.DefaultNameClaimType, id.NameClaimType, "#9");
			Assert.AreEqual (ClaimsIdentity.DefaultRoleClaimType, id.RoleClaimType, "#10");
		}

		[Test]
		public void EnumClaimsAuthNameRoleTypeCtorAllArgsEmptyWorks ()
		{
			var id = new ClaimsIdentity (new Claim [0], "", "", "");
			//NOTE: According to MSDN the id.AuthenticationType should be null, but it isn't on .Net 4.5
			//Assert.IsNull (id.AuthenticationType, "#1");
			Assert.AreEqual ("", id.AuthenticationType, "#1");
			Assert.IsNull (id.Actor, "#2");
			Assert.IsNull (id.BootstrapContext, "#3");
			Assert.IsNotNull (id.Claims, "#4");
			Assert.AreEqual (0, id.Claims.Count (), "#5");
			Assert.IsFalse (id.IsAuthenticated, "#6");
			Assert.IsNull (id.Label, "#7");
			Assert.IsNull (id.Name, "#8");
			Assert.AreEqual (ClaimsIdentity.DefaultNameClaimType, id.NameClaimType, "#9");
			Assert.AreEqual (ClaimsIdentity.DefaultRoleClaimType, id.RoleClaimType, "#10");
		}

		[Test]
		public void EnumClaimsAuthNameRoleTypeCtorWithTwoClaimsAndTypesEmptyWorks ()
		{
			var id = new ClaimsIdentity (
					   new [] {
					new Claim ("claim_type", "claim_value"),
					new Claim (ClaimsIdentity.DefaultNameClaimType, "claim_name_value"), 
				},
					   "", "", "");
			//NOTE: According to MSDN the id.AuthenticationType should be null, but it isn't on .Net 4.5
			//Assert.IsNull (id.AuthenticationType, "#1");
			Assert.AreEqual ("", id.AuthenticationType, "#1");
			Assert.IsNull (id.Actor, "#2");
			Assert.IsNull (id.BootstrapContext, "#3");
			Assert.IsNotNull (id.Claims, "#4");
			Assert.AreEqual (2, id.Claims.Count (), "#5");
			Assert.IsFalse (id.IsAuthenticated, "#6");
			Assert.IsNull (id.Label, "#7");
			Assert.AreEqual ("claim_name_value", id.Name, "#8");
			Assert.AreEqual (ClaimsIdentity.DefaultNameClaimType, id.NameClaimType, "#9");
			Assert.AreEqual (ClaimsIdentity.DefaultRoleClaimType, id.RoleClaimType, "#10");
		}

		[Test]
		public void EnumClaimsAuthNameRoleTypeCtorWithTwoClaimsAndTypesNullWorks ()
		{
			var id = new ClaimsIdentity (
					   new [] {
					new Claim ("claim_type", "claim_value"),
					new Claim (ClaimsIdentity.DefaultNameClaimType, "claim_name_value"), 
				}, 
					   (string)null, (string)null, (string)null);
			Assert.IsNull (id.AuthenticationType, "#1");
			Assert.IsNull (id.Actor, "#2");
			Assert.IsNull (id.BootstrapContext, "#3");
			Assert.IsNotNull (id.Claims, "#4");
			Assert.AreEqual (2, id.Claims.Count (), "#5");
			Assert.IsFalse (id.IsAuthenticated, "#6");
			Assert.IsNull (id.Label, "#7");
			Assert.AreEqual ("claim_name_value", id.Name, "#8");
			Assert.AreEqual (ClaimsIdentity.DefaultNameClaimType, id.NameClaimType, "#9");
			Assert.AreEqual (ClaimsIdentity.DefaultRoleClaimType, id.RoleClaimType, "#10");
		}

		#endregion

		#region Ctor IIdentity, IEnumerable<Claim>, authType, nameType, roleType

		[Test]
		public void IdentityEnumClaimsAuthNameRoleTypeCtorNullsWorks ()
		{
			var id = new ClaimsIdentity ((IIdentity)null, (IEnumerable<Claim>)null, (string)null, (string)null, (string)null);
			Assert.IsNull (id.AuthenticationType, "#1");
			Assert.IsNull (id.Actor, "#2");
			Assert.IsNull (id.BootstrapContext, "#3");
			Assert.IsNotNull (id.Claims, "#4");
			Assert.AreEqual (0, id.Claims.Count (), "#5");
			Assert.IsFalse (id.IsAuthenticated, "#6");
			Assert.IsNull (id.Label, "#7");
			Assert.IsNull (id.Name, "#8");
			Assert.AreEqual (ClaimsIdentity.DefaultNameClaimType, id.NameClaimType, "#9");
			Assert.AreEqual (ClaimsIdentity.DefaultRoleClaimType, id.RoleClaimType, "#10");
		}

		[Test]
		public void IdentityEnumClaimsAuthNameRoleTypeCtorIdentityNullRestEmptyWorks ()
		{
			var id = new ClaimsIdentity (null, new Claim [0], "", "", "");
			//NOTE: According to MSDN the id.AuthenticationType should be null, but it isn't on .Net 4.5
			//Assert.IsNull (id.AuthenticationType, "#1");
			Assert.AreEqual ("", id.AuthenticationType, "#1");
			Assert.IsNull (id.Actor, "#2");
			Assert.IsNull (id.BootstrapContext, "#3");
			Assert.IsNotNull (id.Claims, "#4");
			Assert.AreEqual (0, id.Claims.Count (), "#5");
			Assert.IsFalse (id.IsAuthenticated, "#6");
			Assert.IsNull (id.Label, "#7");
			Assert.IsNull (id.Name, "#8");
			Assert.AreEqual (ClaimsIdentity.DefaultNameClaimType, id.NameClaimType, "#9");
			Assert.AreEqual (ClaimsIdentity.DefaultRoleClaimType, id.RoleClaimType, "#10");
		}

		[Test]
		public void IdentityEnumClaimsAuthNameRoleTypeCtorNullClaimsArrayEmptyTypesWorks ()
		{
			var id = new ClaimsIdentity (
					   null,
					   new [] {
					new Claim ("claim_type", "claim_value"),
					new Claim (ClaimsIdentity.DefaultNameClaimType, "claim_name_value"), 
				}, 
					   "", "", "");
			//NOTE: According to MSDN the id.AuthenticationType should be null, but it isn't on .Net 4.5
			//Assert.IsNull (id.AuthenticationType, "#1");
			Assert.AreEqual ("", id.AuthenticationType, "#1");
			Assert.IsNull (id.Actor, "#2");
			Assert.IsNull (id.BootstrapContext, "#3");
			Assert.IsNotNull (id.Claims, "#4");
			Assert.AreEqual (2, id.Claims.Count (), "#5");
			Assert.IsFalse (id.IsAuthenticated, "#6");
			Assert.IsNull (id.Label, "#7");
			Assert.AreEqual ("claim_name_value", id.Name, "#8");
			Assert.AreEqual (ClaimsIdentity.DefaultNameClaimType, id.NameClaimType, "#9");
			Assert.AreEqual (ClaimsIdentity.DefaultRoleClaimType, id.RoleClaimType, "#10");
		}

		[Test]
		public void IdentityEnumClaimsAuthNameRoleTypeCtorNullClaimsArrayNullsWorks ()
		{
			var id = new ClaimsIdentity (
					   null,
					   new [] {
					new Claim ("claim_type", "claim_value"),
					new Claim (ClaimsIdentity.DefaultNameClaimType, "claim_name_value"), 
				},
					   (string)null, (string)null, (string)null);
			Assert.IsNull (id.AuthenticationType, "#1");
			Assert.IsNull (id.Actor, "#2");
			Assert.IsNull (id.BootstrapContext, "#3");
			Assert.IsNotNull (id.Claims, "#4");
			Assert.AreEqual (2, id.Claims.Count (), "#5");
			Assert.IsFalse (id.IsAuthenticated, "#6");
			Assert.IsNull (id.Label, "#7");
			Assert.AreEqual ("claim_name_value", id.Name, "#8");
			Assert.AreEqual (ClaimsIdentity.DefaultNameClaimType, id.NameClaimType, "#9");
			Assert.AreEqual (ClaimsIdentity.DefaultRoleClaimType, id.RoleClaimType, "#10");
		}

		[Test]
		public void IdentityEnumClaimsAuthNameRoleTypeCtorNullIdentityRestFilledWorks ()
		{
			var id = new ClaimsIdentity (
					   null,
					   new [] {
					new Claim ("claim_type", "claim_value"),
					new Claim (ClaimsIdentity.DefaultNameClaimType, "claim_name_value"), 
					new Claim ("claim_role_type", "claim_role_value"), 
				},
					   "test_auth_type", "test_name_type", "claim_role_type");
			Assert.AreEqual ("test_auth_type", id.AuthenticationType, "#1");
			Assert.IsNull (id.Actor, "#2");
			Assert.IsNull (id.BootstrapContext, "#3");
			Assert.IsNotNull (id.Claims, "#4");
			Assert.AreEqual (3, id.Claims.Count (), "#5");
			Assert.IsTrue (id.IsAuthenticated, "#6");
			Assert.IsNull (id.Label, "#7");
			Assert.IsNull (id.Name, "#8");
			Assert.AreEqual ("test_name_type", id.NameClaimType, "#9");
			Assert.AreEqual ("claim_role_type", id.RoleClaimType, "#10");
		}

		[Test]
		public void IdentityEnumClaimsAuthNameRoleTypeCtorClaimsIdentityRestFilledWorks ()
		{
			var baseId = new ClaimsIdentity (
						   new[] { new Claim ("base_claim_type", "base_claim_value") },
						   "base_auth_type");

			baseId.Actor = new ClaimsIdentity ("base_actor");
			baseId.BootstrapContext = "bootstrap_context";
			baseId.Label = "base_label";

			Assert.IsTrue (baseId.IsAuthenticated, "#0");

			var id = new ClaimsIdentity (
					   baseId,
					   new [] {
					new Claim ("claim_type", "claim_value"),
					new Claim (ClaimsIdentity.DefaultNameClaimType, "claim_name_value"), 
					new Claim ("claim_role_type", "claim_role_value"), 
				},
					   "test_auth_type", "test_name_type", "claim_role_type");

			Assert.AreEqual ("test_auth_type", id.AuthenticationType, "#1");

			Assert.IsNotNull (id.Actor, "#2");
			Assert.AreEqual ("base_actor", id.Actor.AuthenticationType, "#2b");
			Assert.AreEqual ("bootstrap_context", id.BootstrapContext, "#3");
			Assert.IsNotNull (id.Claims, "#4");
			Assert.AreEqual (4, id.Claims.Count (), "#5");
			Assert.AreEqual ("base_claim_type", id.Claims.First ().Type);
			Assert.IsTrue (id.IsAuthenticated, "#6");
			Assert.AreEqual ("base_label", id.Label, "#7");
			Assert.IsNull (id.Name, "#8");
			Assert.AreEqual ("test_name_type", id.NameClaimType, "#9");
			Assert.AreEqual ("claim_role_type", id.RoleClaimType, "#10");
		}

		[Test]
		public void IdentityEnumClaimsAuthNameRoleTypeCtorNonClaimsIdentityRestEmptyWorks ()
		{
			var baseId = new TestIdentity { Name = "base_name", AuthenticationType = "TestId_AuthType" };

			var id = new ClaimsIdentity (
					   baseId,
					   new [] {
					new Claim ("claim_type", "claim_value"),
					new Claim (ClaimsIdentity.DefaultNameClaimType, "claim_name_value"), 
					new Claim ("claim_role_type", "claim_role_value"), 
				},
					   "", "", "");

			Assert.AreEqual ("TestId_AuthType", id.AuthenticationType, "#1");

			Assert.IsNull (id.Actor, "#2");
			Assert.IsNull (id.BootstrapContext, "#3");
			Assert.IsNotNull (id.Claims, "#4");
			Assert.AreEqual (4, id.Claims.Count (), "#5");
			Assert.AreEqual (2, id.Claims.Count (_ => _.Type == ClaimsIdentity.DefaultNameClaimType), "#5b");
			Assert.IsTrue (id.IsAuthenticated, "#6");
			Assert.IsNull (id.Label, "#7");
			Assert.AreEqual ("base_name", id.Name, "#8");
			Assert.AreEqual (ClaimsIdentity.DefaultNameClaimType, id.NameClaimType, "#9");
			Assert.AreEqual (ClaimsIdentity.DefaultRoleClaimType, id.RoleClaimType, "#10");
		}

		#endregion

		#region Ctor IIdentity, IEnumerable<Claim>

		[Test]
		public void IdentityEnumClaimsCtorClaimsIdentityClaimsWorks ()
		{
			var baseId = new ClaimsIdentity (
						   new [] { new Claim ("base_claim_type", "base_claim_value") },
						   "base_auth_type", "base_name_claim_type", null);

			baseId.Actor = new ClaimsIdentity ("base_actor");
			baseId.BootstrapContext = "bootstrap_context";
			baseId.Label = "base_label";

			Assert.IsTrue (baseId.IsAuthenticated, "#0");

			var id = new ClaimsIdentity (
					   baseId,
					   new [] {
					new Claim ("claim_type", "claim_value"),
					new Claim (ClaimsIdentity.DefaultNameClaimType, "claim_name_value"), 
					new Claim ("claim_role_type", "claim_role_value"), 
				});

			Assert.AreEqual ("base_auth_type", id.AuthenticationType, "#1");

			Assert.IsNotNull (id.Actor, "#2");
			Assert.AreEqual ("base_actor", id.Actor.AuthenticationType, "#2b");
			Assert.AreEqual ("bootstrap_context", id.BootstrapContext, "#3");
			Assert.IsNotNull (id.Claims, "#4");
			Assert.AreEqual (4, id.Claims.Count (), "#5");
			Assert.AreEqual ("base_claim_type", id.Claims.First ().Type, "#5b");
			Assert.IsTrue (id.IsAuthenticated, "#6");
			Assert.AreEqual ("base_label", id.Label, "#7");
			Assert.IsNull (id.Name, "#8");
			Assert.AreEqual ("base_name_claim_type", id.NameClaimType, "#9");
			Assert.AreEqual (ClaimsIdentity.DefaultRoleClaimType, id.RoleClaimType, "#10");
		}

		[Test]
		public void IdentityEnumClaimsCtorNonClaimsIdentityClaimsWorks ()
		{
			var baseId = new TestIdentity {
				Name = "base_name", AuthenticationType = "TestId_AuthType"
			};

			var id = new ClaimsIdentity (
					   baseId,
					   new [] {
					new Claim ("claim_type", "claim_value"),
					new Claim (ClaimsIdentity.DefaultNameClaimType, "claim_name_value"), 
					new Claim ("claim_role_type", "claim_role_value"), 
				});

			Assert.AreEqual ("TestId_AuthType", id.AuthenticationType, "#1");

			Assert.IsNull (id.Actor, "#2");
			Assert.IsNull (id.BootstrapContext, "#3");
			Assert.IsNotNull (id.Claims, "#4");
			Assert.AreEqual (4, id.Claims.Count (), "#5");
			Assert.AreEqual (2, id.Claims.Count (_ => _.Type == ClaimsIdentity.DefaultNameClaimType), "#5b");
			Assert.IsTrue (id.IsAuthenticated, "#6");
			Assert.IsNull (id.Label, "#7");
			Assert.AreEqual ("base_name", id.Name, "#8");
			Assert.AreEqual (ClaimsIdentity.DefaultNameClaimType, id.NameClaimType, "#9");
			Assert.AreEqual (ClaimsIdentity.DefaultRoleClaimType, id.RoleClaimType, "#10");
		}

		#endregion

		[Test]
		public void FindCaseInsensivity ()
		{
			var claim_type = new Claim("TYpe", "value");
			var id = new ClaimsIdentity (
				new[] { claim_type },
				"base_auth_type", "base_name_claim_type", null);

			var f1 = id.FindFirst ("tyPe");
			Assert.AreEqual ("value", f1.Value, "#1");

			var f2 = id.FindAll ("tyPE").First ();
			Assert.AreEqual ("value", f2.Value, "#2");
		}

		[Test]
		public void HasClaim_typeValue_Works()
		{
			var id = new ClaimsIdentity(
			new[] {
				new Claim ("claim_type", "claim_value"),
				new Claim (ClaimsIdentity.DefaultNameClaimType, "claim_name_value"), 
				new Claim ("claim_role_type", "claim_role_value"), 
			}, "test_authority");

			Assert.IsTrue (id.HasClaim("claim_type", "claim_value"), "#1");
			Assert.IsTrue (id.HasClaim("cLaIm_TyPe", "claim_value"), "#2");
			Assert.IsFalse (id.HasClaim("claim_type", "cLaIm_VaLuE"), "#3");
			Assert.IsFalse (id.HasClaim("Xclaim_type", "claim_value"), "#4");
			Assert.IsFalse (id.HasClaim("claim_type", "Xclaim_value"), "#5");
	  }
	}

	class TestIdentity : IIdentity
	{
		public string Name {
			get;
			set;
		}

		public string AuthenticationType {
			get;
			set;
		}

		public bool IsAuthenticated {
			get { throw new NotImplementedException (); }
		}
	}
}

