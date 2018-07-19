//
//
// ClaimsPrincipalTest.cs - NUnit Test Cases for System.Security.Claims.ClaimsPrincipal
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
	public class ClaimsPrincipalTest
	{
		#region Ctor Empty

		[Test]
		public void EmptyCtorWorks ()
		{
			var p = new ClaimsPrincipal ();
			Assert.IsNotNull (p.Identities, "#1");
			Assert.AreEqual (p.Identities.ToArray ().Length, 0, "#2");

			Assert.IsNotNull (p.Claims, "#3");
			Assert.AreEqual (p.Claims.ToArray ().Length, 0, "#4");

			Assert.IsNull (p.Identity, "#5");
		}

		#endregion

		#region Ctor IIdentity

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IIdentityCtorNullThrows ()
		{
			var p = new ClaimsPrincipal ((IIdentity)null);
		}

		[Test]
		public void IIdentityCtorClaimsIdentityWorks ()
		{
			var id = new ClaimsIdentity (
				       new List<Claim> { new Claim ("claim_type", "claim_value") },
				       "");
			var p = new ClaimsPrincipal (id);

			Assert.IsNotNull (p.Identities, "#1");
			Assert.AreEqual (1, p.Identities.Count (), "#2");

			Assert.AreEqual (id, p.Identities.First (), "#3");

			Assert.AreEqual (id, p.Identity, "#4");

			Assert.IsNotNull (p.Claims, "#5");
			Assert.AreEqual (1, p.Claims.Count (), "#6");
			Assert.IsTrue (p.Claims.Any (claim => claim.Type == "claim_type" && claim.Value == "claim_value"), "#7");
		}

		[Test]
		public void IIdentityCtorNonClaimsIdentityWorks ()
		{
			var id = new TestIdentity {
				Name = "test_name", 
				AuthenticationType = "test_auth"
			};
			var p = new ClaimsPrincipal (id);

			Assert.IsNotNull (p.Identities, "#1");
			Assert.AreEqual (1, p.Identities.Count (), "#2");

			Assert.AreNotEqual (id, p.Identities.First (), "#3");
			Assert.AreNotEqual (id, p.Identity, "#4");
			Assert.AreEqual (id.Name, p.Identity.Name, "#5");

			Assert.IsNotNull (p.Claims, "#6");
			Assert.AreEqual (1, p.Claims.Count (), "#7");
			Assert.IsTrue (p.Claims.Any (claim => claim.Type == ClaimsIdentity.DefaultNameClaimType && claim.Value == "test_name"), "#8");
		}

		#endregion

		#region Ctor IPrincipal

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IPrincipalCtorNullThrows ()
		{
			var p = new ClaimsPrincipal ((IPrincipal)null);
		}

		[Test]
		public void IPrincipalCtorClaimsPrincipalWorks ()
		{
			var baseId = new ClaimsIdentity (
				           new List<Claim> { new Claim ("claim_type", "claim_value") },
				           "");
			var basePrincipal = new ClaimsPrincipal ();
			basePrincipal.AddIdentity (baseId);

			var p = new ClaimsPrincipal (basePrincipal);

			Assert.IsNotNull (p.Identities, "#1");
			Assert.AreEqual (1, p.Identities.Count (), "#2");

			Assert.AreEqual (baseId, p.Identities.First (), "#3");

			Assert.AreEqual (baseId, p.Identity, "#4");

			Assert.IsNotNull (p.Claims, "$5");
			Assert.AreEqual (1, p.Claims.Count (), "#6");
			Assert.IsTrue (p.Claims.Any (claim => claim.Type == "claim_type" && claim.Value == "claim_value"), "#7");
		}

		[Test]
		public void IPrincipalCtorNonClaimsPrincipalWithNonClaimsIdentityWorks ()
		{
			var id = new TestIdentity {
				Name = "test_name", 
				AuthenticationType = "test_auth"
			};
			var basePrincipal = new TestPrincipal { Identity = id };
			var p = new ClaimsPrincipal (basePrincipal);

			Assert.IsNotNull (p.Identities, "#1");
			Assert.AreEqual (1, p.Identities.Count (), "#2");

			Assert.AreNotEqual (id, p.Identities.First (), "#3");
			Assert.AreNotEqual (id, p.Identity, "#4");
			Assert.AreEqual (id.Name, p.Identity.Name, "#5");

			Assert.IsNotNull (p.Claims, "#6");
			Assert.AreEqual (1, p.Claims.Count (), "#7");
			Assert.IsTrue (p.Claims.Any (claim => claim.Type == ClaimsIdentity.DefaultNameClaimType && claim.Value == "test_name"), "#8");
		}

		[Test]
		public void IPrincipalCtorNonClaimsPrincipalWithoutIdentityWorks ()
		{
			var p = new ClaimsPrincipal (new TestPrincipal ());
			Assert.IsNotNull (p.Identities, "#1");
			Assert.AreEqual (p.Identities.ToArray ().Length, 1, "#2");

			Assert.IsNotNull (p.Claims, "#3");
			Assert.AreEqual (p.Claims.ToArray ().Length, 0, "#4");

			Assert.IsNotNull (p.Identity, "#5");
			Assert.IsFalse (p.Identity.IsAuthenticated, "#6");
		}

		[Test]
		[Category ("Ctor_IPrincipal")]
		public void IPrincipalCtorClaimsPrincipalWithoutIdentityWorks ()
		{
			var p = new ClaimsPrincipal (new ClaimsPrincipal ());
			Assert.IsNotNull (p.Identities, "#1");
			Assert.AreEqual (p.Identities.ToArray ().Length, 0, "#2");

			Assert.IsNotNull (p.Claims, "#3");
			Assert.AreEqual (p.Claims.ToArray ().Length, 0, "#4");

			Assert.IsNull (p.Identity, "#5");
		}

		[Test]
		public void IPrincipalCtorClaimsPrincipalWithMultipleIdentitiesWorks ()
		{
			var baseId1 = new ClaimsIdentity ("baseId1");
			var baseId2 = new GenericIdentity ("generic_name", "baseId2");
			var baseId3 = WindowsIdentity.GetAnonymous ();

			var basePrincipal = new ClaimsPrincipal (baseId1);
			basePrincipal.AddIdentity (baseId2);
			basePrincipal.AddIdentity (baseId3);

			var p = new ClaimsPrincipal (basePrincipal);
			Assert.IsNotNull (p.Identities, "#1");
			Assert.AreEqual (3, p.Identities.Count (), "#2");

			Assert.IsNotNull (p.Claims, "#3");
			Assert.AreEqual (1, p.Claims.Count (), "#4");

			// The Identity property favours WindowsIdentity
			Assert.AreEqual (baseId3, p.Identity, "#5");

			Assert.IsTrue (p.Claims.Any (claim => claim.Type == ClaimsIdentity.DefaultNameClaimType && claim.Value == "generic_name"), "#6");

			Assert.AreEqual (baseId2.Claims.First (), p.Claims.First (), "#7");
		}

		#endregion

		#region Ctor IEnumerable<ClaimsIdentity>

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IEnumClaimsIdCtorNullThrows ()
		{
			var p = new ClaimsPrincipal ((IEnumerable<ClaimsIdentity>)null);
		}

		[Test]
		public void IEnumClaimsIdCtorEmptyWorks ()
		{
			var p = new ClaimsPrincipal (new ClaimsIdentity [0]);
			Assert.IsNotNull (p.Identities, "#1");
			Assert.AreEqual (p.Identities.ToArray ().Length, 0, "#2");
			Assert.IsNotNull (p.Claims, "#3");
			Assert.AreEqual (p.Claims.ToArray ().Length, 0, "#4");
			Assert.IsNull (p.Identity, "#5");
		}

		[Test]
		public void IEnumClaimsIdCtorMultipleIdentitiesWorks ()
		{
			var baseId1 = new ClaimsIdentity ("baseId1");
			var baseId2 = new GenericIdentity ("generic_name2", "baseId2");
			var baseId3 = new GenericIdentity ("generic_name3", "baseId3");

			var p = new ClaimsPrincipal (new List<ClaimsIdentity> { baseId1, baseId2, baseId3 });
			Assert.IsNotNull (p.Identities, "#1");
			Assert.AreEqual (3, p.Identities.Count (), "#2");

			Assert.IsNotNull (p.Claims, "#3");
			Assert.AreEqual (2, p.Claims.Count (), "#4");

			Assert.AreEqual (baseId1, p.Identity, "#5");

			Assert.IsTrue (p.Claims.Any (claim => claim.Type == ClaimsIdentity.DefaultNameClaimType && claim.Value == "generic_name2"), "#6");
			Assert.IsTrue (p.Claims.Any (claim => claim.Type == ClaimsIdentity.DefaultNameClaimType && claim.Value == "generic_name3"), "#7");

			Assert.AreEqual (baseId2.Claims.First (), p.Claims.First (), "#7");
			Assert.AreEqual (baseId3.Claims.Last (), p.Claims.Last (), "#8");
		}

		#endregion

		internal class TestPrincipal : IPrincipal
		{
			public IIdentity Identity { get; set; }

			public bool IsInRole (string role)
			{
				throw new NotImplementedException ();
			}
		}
	}
}

