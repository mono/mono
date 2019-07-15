using System.Threading.Tasks;

namespace System.Net.Security.Tests {
	internal static class TestConfiguration {
		public const int PassingTestTimeoutMilliseconds = 4 * 60 * 1000;
		public const int FailingTestTimeoutMiliseconds = 250;

		public const string Realm = "TEST.COREFX.NET";
		public const string KerberosUser = "krb_user";
		public const string DefaultPassword = "password";
		public const string HostTarget = "TESTHOST/testfqdn.test.corefx.net";
		public const string HttpTarget = "TESTHTTP@localhost";
		public const string Domain = "TEST";
		public const string NtlmUser = "ntlm_user";
		public const string NtlmPassword = "ntlm_password";
		public const string NtlmUserFilePath = "/var/tmp/ntlm_user_file";

		public static bool SupportsNullEncryption => false;

		public static Task WhenAllOrAnyFailedWithTimeout (params Task [] tasks)
		    => tasks.WhenAllOrAnyFailed (PassingTestTimeoutMilliseconds);
	}
}
