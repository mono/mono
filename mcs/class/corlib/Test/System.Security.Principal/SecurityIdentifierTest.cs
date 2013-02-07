//
// SecurityIdentifierTest.cs - NUnit Test Cases for SecurityIdentifier
//
// Author:
//	Kenneth Bell
//

using System;
using System.Security.Principal;
using System.Text;
using NUnit.Framework;

namespace MonoTests.System.Security.Principal
{
	[TestFixture]
	public class SecurityIdentifierTest
	{
		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNull ()
		{
			new SecurityIdentifier (null);
		}
		
		private void CheckStringCtor (string strValue, byte[] expectedBinary)
		{
			SecurityIdentifier sid = new SecurityIdentifier (strValue);
			byte[] buffer = new byte[sid.BinaryLength];
			sid.GetBinaryForm (buffer, 0);
			
			Assert.AreEqual (expectedBinary.Length, buffer.Length, "SID length mismatch");
			Assert.AreEqual (expectedBinary, buffer, "SIDs different in binary form");
		}

		private void CheckUnqualifiedWellKnownSid (WellKnownSidType type, string sddl)
		{
			SecurityIdentifier sid = new SecurityIdentifier (type, null);
			Assert.AreEqual (sddl, sid.Value, "Bad SID for type: " + type);
		}

		private void CheckQualifiedWellKnownSid (WellKnownSidType type, SecurityIdentifier domain, string sddl)
		{
			SecurityIdentifier sid = new SecurityIdentifier (type, domain);
			Assert.AreEqual (sddl, sid.Value, "Bad SID for type: " + type);
		}

		private void CheckWellKnownSidLookup (WellKnownSidType wellKnownSidType, string name)
		{
			Assert.AreEqual (name, ((NTAccount)new SecurityIdentifier (wellKnownSidType, null).Translate (typeof(NTAccount))).Value);
		}

		[Test]
		public void ConstructorString ()
		{
			CheckStringCtor ("S-1-0-0",
			                 new byte[] {
				0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00 });
			CheckStringCtor ("S-1-5-33",
			                 new byte[] {
				0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x21, 0x00,
				0x00, 0x00 });
			CheckStringCtor ("s-1-5-334-234",
			                 new byte[] {
				0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x4E, 0x01,
				0x00, 0x00, 0xEA, 0x00, 0x00, 0x00 });
			CheckStringCtor ("S-1-5-0x3432",
			                 new byte[] {
				0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x32, 0x34,
				0x00, 0x00 });
			CheckStringCtor ("S-1-0xCBA987654321-0",
			                 new byte[] {
				0x01, 0x01, 0xCB, 0xA9, 0x87, 0x65, 0x43, 0x21, 0x00, 0x00,
				0x00, 0x00 });
		}

		[Test]
		public void ConstructorStringSddl ()
		{
			Assert.AreEqual ("S-1-5-32-545",
			                 new SecurityIdentifier ("BU").Value);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void ConstructorStringBadRevision ()
		{
			CheckStringCtor ("S-2-0-0",
			                 new byte[] {
				0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00 });
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void ConstructorInvalidString ()
		{
			new SecurityIdentifier ("M");
		}

		[Test]
		public void ConstructorBinary ()
		{
			byte[] inForm = new byte[] {
				0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x05, 0x4E, 0x01,
				0x00, 0x00, 0xEA, 0x00, 0x00, 0x00 };
			SecurityIdentifier sid = new SecurityIdentifier (inForm, 0);
			
			byte[] outForm = new byte[inForm.Length];
			sid.GetBinaryForm (outForm, 0);
			Assert.AreEqual (inForm, outForm);
		}

		[Test]
		public void ConstructorWellKnownSids ()
		{
			CheckUnqualifiedWellKnownSid (WellKnownSidType.NullSid, "S-1-0-0");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.WorldSid, "S-1-1-0");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.LocalSid, "S-1-2-0");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.CreatorOwnerSid, "S-1-3-0");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.CreatorGroupSid, "S-1-3-1");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.CreatorOwnerServerSid, "S-1-3-2");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.CreatorGroupServerSid, "S-1-3-3");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.NTAuthoritySid, "S-1-5");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.DialupSid, "S-1-5-1");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.NetworkSid, "S-1-5-2");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.BatchSid, "S-1-5-3");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.InteractiveSid, "S-1-5-4");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.ServiceSid, "S-1-5-6");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.AnonymousSid, "S-1-5-7");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.ProxySid, "S-1-5-8");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.EnterpriseControllersSid, "S-1-5-9");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.SelfSid, "S-1-5-10");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.AuthenticatedUserSid, "S-1-5-11");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.RestrictedCodeSid, "S-1-5-12");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.TerminalServerSid, "S-1-5-13");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.RemoteLogonIdSid, "S-1-5-14");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.LocalSystemSid, "S-1-5-18");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.LocalServiceSid, "S-1-5-19");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.NetworkServiceSid, "S-1-5-20");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.BuiltinDomainSid, "S-1-5-32");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.BuiltinAdministratorsSid, "S-1-5-32-544");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.BuiltinUsersSid, "S-1-5-32-545");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.BuiltinGuestsSid, "S-1-5-32-546");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.BuiltinPowerUsersSid, "S-1-5-32-547");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.BuiltinAccountOperatorsSid, "S-1-5-32-548");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.BuiltinSystemOperatorsSid, "S-1-5-32-549");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.BuiltinPrintOperatorsSid, "S-1-5-32-550");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.BuiltinBackupOperatorsSid, "S-1-5-32-551");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.BuiltinReplicatorSid, "S-1-5-32-552");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.BuiltinPreWindows2000CompatibleAccessSid, "S-1-5-32-554");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.BuiltinRemoteDesktopUsersSid, "S-1-5-32-555");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.BuiltinNetworkConfigurationOperatorsSid, "S-1-5-32-556");
			CheckQualifiedWellKnownSid (WellKnownSidType.AccountAdministratorSid, new SecurityIdentifier ("S-1-5-21-125-3215-342"), "S-1-5-21-125-3215-342-500");
			CheckQualifiedWellKnownSid (WellKnownSidType.AccountGuestSid, new SecurityIdentifier ("S-1-5-21-125-3215-342"), "S-1-5-21-125-3215-342-501");
			CheckQualifiedWellKnownSid (WellKnownSidType.AccountKrbtgtSid, new SecurityIdentifier ("S-1-5-21-125-3215-342"), "S-1-5-21-125-3215-342-502");
			CheckQualifiedWellKnownSid (WellKnownSidType.AccountDomainAdminsSid, new SecurityIdentifier ("S-1-5-21-125-3215-342"), "S-1-5-21-125-3215-342-512");
			CheckQualifiedWellKnownSid (WellKnownSidType.AccountDomainUsersSid, new SecurityIdentifier ("S-1-5-21-125-3215-342"), "S-1-5-21-125-3215-342-513");
			CheckQualifiedWellKnownSid (WellKnownSidType.AccountDomainGuestsSid, new SecurityIdentifier ("S-1-5-21-125-3215-342"), "S-1-5-21-125-3215-342-514");
			CheckQualifiedWellKnownSid (WellKnownSidType.AccountComputersSid, new SecurityIdentifier ("S-1-5-21-125-3215-342"), "S-1-5-21-125-3215-342-515");
			CheckQualifiedWellKnownSid (WellKnownSidType.AccountControllersSid, new SecurityIdentifier ("S-1-5-21-125-3215-342"), "S-1-5-21-125-3215-342-516");
			CheckQualifiedWellKnownSid (WellKnownSidType.AccountCertAdminsSid, new SecurityIdentifier ("S-1-5-21-125-3215-342"), "S-1-5-21-125-3215-342-517");
			CheckQualifiedWellKnownSid (WellKnownSidType.AccountSchemaAdminsSid, new SecurityIdentifier ("S-1-5-21-125-3215-342"), "S-1-5-21-125-3215-342-518");
			CheckQualifiedWellKnownSid (WellKnownSidType.AccountEnterpriseAdminsSid, new SecurityIdentifier ("S-1-5-21-125-3215-342"), "S-1-5-21-125-3215-342-519");
			CheckQualifiedWellKnownSid (WellKnownSidType.AccountPolicyAdminsSid, new SecurityIdentifier ("S-1-5-21-125-3215-342"), "S-1-5-21-125-3215-342-520");
			CheckQualifiedWellKnownSid (WellKnownSidType.AccountRasAndIasServersSid, new SecurityIdentifier ("S-1-5-21-125-3215-342"), "S-1-5-21-125-3215-342-553");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.NtlmAuthenticationSid, "S-1-5-64-10");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.DigestAuthenticationSid, "S-1-5-64-21");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.SChannelAuthenticationSid, "S-1-5-64-14");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.ThisOrganizationSid, "S-1-5-15");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.OtherOrganizationSid, "S-1-5-1000");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.BuiltinIncomingForestTrustBuildersSid, "S-1-5-32-557");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.BuiltinPerformanceMonitoringUsersSid, "S-1-5-32-558");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.BuiltinPerformanceLoggingUsersSid, "S-1-5-32-559");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.BuiltinAuthorizationAccessSid, "S-1-5-32-560");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.WinBuiltinTerminalServerLicenseServersSid, "S-1-5-32-561");
			CheckUnqualifiedWellKnownSid (WellKnownSidType.MaxDefined, "S-1-5-32-561");
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void ConstructorWellKnownSidLogonIds ()
		{
			CheckQualifiedWellKnownSid (WellKnownSidType.LogonIdsSid,
			                            new SecurityIdentifier ("S-1-5-21-125-3215-342"),
			                            "S-1-5-21-125-3215-342-3");
		}

		[Test]
		public void AccountDomainSid ()
		{
			Assert.AreEqual ("S-1-5-21-125-3215-342", new SecurityIdentifier ("S-1-5-21-125-3215-342-324-1000").AccountDomainSid.Value);
			Assert.AreEqual ("S-1-5-21-125-3215-342", new SecurityIdentifier ("S-1-5-21-125-3215-342-1000").AccountDomainSid.Value);
			Assert.AreEqual ("S-1-5-21-125-3215-1", new SecurityIdentifier ("S-1-5-21-125-3215-1").AccountDomainSid.Value);
			Assert.IsNull (new SecurityIdentifier ("S-1-5-21-125-1").AccountDomainSid);
			Assert.IsNull (new SecurityIdentifier ("S-1-0-0").AccountDomainSid);
			Assert.IsNull (new SecurityIdentifier ("S-1-5-44-125-3215-1").AccountDomainSid);
		}

		[Test]
		public void BinaryLength ()
		{
			Assert.AreEqual (12, new SecurityIdentifier ("S-1-0-0").BinaryLength);
		}

		[Test]
		public void Value ()
		{
			Assert.AreEqual ("S-1-5-13362", new SecurityIdentifier ("s-1-5-0x3432").Value);
		}

		[Test]
		public void Equals ()
		{
			Assert.IsTrue (new SecurityIdentifier ("S-1-5-13362").Equals (new SecurityIdentifier ("s-1-5-0x3432")));
		}

		[Test]
		public void IsAccountSid ()
		{
			Assert.IsTrue (new SecurityIdentifier ("S-1-5-21-125-3215-342-324-1000").IsAccountSid ());
			Assert.IsTrue (new SecurityIdentifier ("S-1-5-21-125-3215-342-1000").IsAccountSid ());
			Assert.IsTrue (new SecurityIdentifier ("S-1-5-21-125-3215-1").IsAccountSid ());
			Assert.IsFalse (new SecurityIdentifier ("S-1-5-21-125-1").IsAccountSid ());
			Assert.IsFalse (new SecurityIdentifier ("S-1-0-0").IsAccountSid ());
		}

		[Test]
		public void IsEqualDomainSid ()
		{
			Assert.IsTrue (new SecurityIdentifier ("S-1-5-21-125-3215-342-1000").IsEqualDomainSid (new SecurityIdentifier ("S-1-5-21-125-3215-342-333")));
			Assert.IsTrue (new SecurityIdentifier ("S-1-5-21-125-3215-342-1000").IsEqualDomainSid (new SecurityIdentifier ("S-1-5-21-125-3215-342-324-333")));
			Assert.IsFalse (new SecurityIdentifier ("S-1-5-21-125-1").IsEqualDomainSid (new SecurityIdentifier ("S-1-5-21-125-2")));
			Assert.IsFalse (new SecurityIdentifier ("S-1-0-0").IsEqualDomainSid (new SecurityIdentifier ("S-1-0-0")));
		}

		[Test]
		public void IsValidTargetType ()
		{
			Assert.IsTrue (new SecurityIdentifier ("S-1-0-0").IsValidTargetType (typeof(SecurityIdentifier)));
			Assert.IsTrue (new SecurityIdentifier ("S-1-0-0").IsValidTargetType (typeof(NTAccount)));
			Assert.IsFalse (new SecurityIdentifier ("S-1-0-0").IsValidTargetType (typeof(WindowsPrincipal)));
			Assert.IsFalse (new SecurityIdentifier ("S-1-0-0").IsValidTargetType (typeof(WindowsIdentity)));
		}

		[Test]
		public void IsWellKnown ()
		{
			Assert.IsTrue (new SecurityIdentifier ("S-1-0-0").IsWellKnown (WellKnownSidType.NullSid));
			Assert.IsTrue (new SecurityIdentifier ("S-1-5-21-125-3215-342-500").IsWellKnown (WellKnownSidType.AccountAdministratorSid));
			Assert.IsTrue (new SecurityIdentifier ("S-1-5-21-125-3215-342-513").IsWellKnown (WellKnownSidType.AccountDomainUsersSid));
			Assert.IsFalse (new SecurityIdentifier ("S-1-6-21-125-3215-342-513").IsWellKnown (WellKnownSidType.AccountDomainUsersSid));
			Assert.IsFalse (new SecurityIdentifier ("S-1-5-22-125-3215-342-513").IsWellKnown (WellKnownSidType.AccountDomainUsersSid));
		}

		[Test]
		public void Translate ()
		{
			CheckWellKnownSidLookup (WellKnownSidType.NullSid, @"NULL SID");
			CheckWellKnownSidLookup (WellKnownSidType.WorldSid, @"Everyone");
			CheckWellKnownSidLookup (WellKnownSidType.LocalSid, @"LOCAL");
			CheckWellKnownSidLookup (WellKnownSidType.CreatorOwnerSid, @"CREATOR OWNER");
			CheckWellKnownSidLookup (WellKnownSidType.CreatorGroupSid, @"CREATOR GROUP");
			CheckWellKnownSidLookup (WellKnownSidType.CreatorOwnerServerSid, @"CREATOR OWNER SERVER");
			CheckWellKnownSidLookup (WellKnownSidType.CreatorGroupServerSid, @"CREATOR GROUP SERVER");
			CheckWellKnownSidLookup (WellKnownSidType.DialupSid, @"NT AUTHORITY\DIALUP");
			CheckWellKnownSidLookup (WellKnownSidType.NetworkSid, @"NT AUTHORITY\NETWORK");
			CheckWellKnownSidLookup (WellKnownSidType.BatchSid, @"NT AUTHORITY\BATCH");
			CheckWellKnownSidLookup (WellKnownSidType.InteractiveSid, @"NT AUTHORITY\INTERACTIVE");
			CheckWellKnownSidLookup (WellKnownSidType.ServiceSid, @"NT AUTHORITY\SERVICE");
			CheckWellKnownSidLookup (WellKnownSidType.AnonymousSid, @"NT AUTHORITY\ANONYMOUS LOGON");
			CheckWellKnownSidLookup (WellKnownSidType.ProxySid, @"NT AUTHORITY\PROXY");
			CheckWellKnownSidLookup (WellKnownSidType.EnterpriseControllersSid, @"NT AUTHORITY\ENTERPRISE DOMAIN CONTROLLERS");
			CheckWellKnownSidLookup (WellKnownSidType.SelfSid, @"NT AUTHORITY\SELF");
			CheckWellKnownSidLookup (WellKnownSidType.AuthenticatedUserSid, @"NT AUTHORITY\Authenticated Users");
			CheckWellKnownSidLookup (WellKnownSidType.RestrictedCodeSid, @"NT AUTHORITY\RESTRICTED");
			CheckWellKnownSidLookup (WellKnownSidType.TerminalServerSid, @"NT AUTHORITY\TERMINAL SERVER USER");
			CheckWellKnownSidLookup (WellKnownSidType.RemoteLogonIdSid, @"NT AUTHORITY\REMOTE INTERACTIVE LOGON");
			CheckWellKnownSidLookup (WellKnownSidType.LocalSystemSid, @"NT AUTHORITY\SYSTEM");
			CheckWellKnownSidLookup (WellKnownSidType.LocalServiceSid, @"NT AUTHORITY\LOCAL SERVICE");
			CheckWellKnownSidLookup (WellKnownSidType.NetworkServiceSid, @"NT AUTHORITY\NETWORK SERVICE");
			CheckWellKnownSidLookup (WellKnownSidType.BuiltinAdministratorsSid, @"BUILTIN\Administrators");
			CheckWellKnownSidLookup (WellKnownSidType.BuiltinUsersSid, @"BUILTIN\Users");
			CheckWellKnownSidLookup (WellKnownSidType.BuiltinGuestsSid, @"BUILTIN\Guests");
			CheckWellKnownSidLookup (WellKnownSidType.NtlmAuthenticationSid, @"NT AUTHORITY\NTLM Authentication");
			CheckWellKnownSidLookup (WellKnownSidType.DigestAuthenticationSid, @"NT AUTHORITY\Digest Authentication");
			CheckWellKnownSidLookup (WellKnownSidType.SChannelAuthenticationSid, @"NT AUTHORITY\SChannel Authentication");
			CheckWellKnownSidLookup (WellKnownSidType.ThisOrganizationSid, @"NT AUTHORITY\This Organization");
			CheckWellKnownSidLookup (WellKnownSidType.OtherOrganizationSid, @"NT AUTHORITY\Other Organization");
			CheckWellKnownSidLookup (WellKnownSidType.BuiltinPerformanceMonitoringUsersSid, @"BUILTIN\Performance Monitor Users");
			CheckWellKnownSidLookup (WellKnownSidType.BuiltinPerformanceLoggingUsersSid, @"BUILTIN\Performance Log Users");
		}

		[Test]
		[ExpectedException(typeof(IdentityNotMappedException))]
		public void TranslateUnknown ()
		{
			new SecurityIdentifier ("S-1-5-21-125-3215-342-513").Translate (typeof(NTAccount));
		}

		[Test]
		public void LengthLimits ()
		{
			Assert.AreEqual (8, SecurityIdentifier.MinBinaryLength);
			Assert.AreEqual (68, SecurityIdentifier.MaxBinaryLength);
		}
		
		[Test]
		public void CompareOrdering ()
		{
			SecurityIdentifier[] sids = new SecurityIdentifier[] {
				new SecurityIdentifier ("S-1-5-32-544"),
				new SecurityIdentifier ("S-1-5-40"),
				new SecurityIdentifier ("S-1-5-32-5432"),
				new SecurityIdentifier ("S-1-6-0"),
				new SecurityIdentifier ("S-1-5-32-99"),
				new SecurityIdentifier ("S-1-0-2")
			};

			SecurityIdentifier[] sortedSids = (SecurityIdentifier[])sids.Clone ();
			Array.Sort (sortedSids);

			Assert.AreSame (sids [5], sortedSids [0]);
			Assert.AreSame (sids [1], sortedSids [1]);
			Assert.AreSame (sids [4], sortedSids [2]);
			Assert.AreSame (sids [0], sortedSids [3]);
			Assert.AreSame (sids [2], sortedSids [4]);
			Assert.AreSame (sids [3], sortedSids [5]);
		}
		
		[Test, ExpectedExceptionAttribute (typeof (ArgumentNullException))]
		public void CompareToNull ()
		{
			SecurityIdentifier sid = new SecurityIdentifier (WellKnownSidType.BuiltinUsersSid, null);
			sid.CompareTo ((SecurityIdentifier)null);
		}

		[Test]
		public void EqualsNull ()
		{
			SecurityIdentifier sid = new SecurityIdentifier (WellKnownSidType.BuiltinUsersSid, null);
			Assert.IsFalse (sid.Equals ((object)null));
			Assert.IsFalse (sid.Equals ((SecurityIdentifier)null));
		}
		
		[Test]
		public unsafe void IntPtrRoundtrip ()
		{
			SecurityIdentifier sidIn, sidOut;
			byte[] binaryFormIn, binaryFormOut;

			sidIn = new SecurityIdentifier ("WD");
			binaryFormIn = new byte[sidIn.BinaryLength];
			sidIn.GetBinaryForm (binaryFormIn, 0);

			fixed (byte* pointerForm = binaryFormIn)
				sidOut = new SecurityIdentifier ((IntPtr)pointerForm);
			binaryFormOut = new byte[sidOut.BinaryLength];
			sidOut.GetBinaryForm (binaryFormOut, 0);

			Assert.AreEqual (sidIn, sidOut);
			Assert.AreEqual (binaryFormIn, binaryFormOut);
		}
	}
}
