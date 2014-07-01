using System;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace MonoTests.System {
	[TestFixture]
	public class UriPermutationsTest {

		// Set this to true to generate the expected values
		// The tests should run first on .NET with CreateMode = true
		// The generated files should then be used when running the tests in Mono with CreateMode = false
		private const bool createMode = false;

		private const string location = "./Test/System/UriPermutationsTest/";

		private const string nonAsciiTestedChars = "☕";

		private static readonly string [] schemes = {
			"http://", "https://", "file://", "ftp://", "gopher://", "ldap://", "mailto:",
			"net.pipe://", "net.tcp://", "news:", "nntp://", "telnet://", "custom:", "custom://"
		};

		private static readonly string [] componentLocations = {
			"a/a", "a/a/a", "a/a?", "a/a#"
		};

		private static readonly string [] reduceLocations = {
			"a/b/a./a", "a/b/.a/a", "a/b/./a", "a/b/a../a", "a/b/..a/a", "a/b/../a", "a/b/.../a"
		};

		public static readonly bool IriParsing;

		static UriPermutationsTest ()
		{
			FieldInfo iriParsingField = typeof (Uri).GetField ("s_IriParsing",
				BindingFlags.Static | BindingFlags.GetField | BindingFlags.NonPublic);
			if (iriParsingField != null)
				IriParsing = (bool) iriParsingField.GetValue (null);
		}

		[SetUp]
		public void Setup()
		{
			StringTester.CreateMode = createMode;
			StringTester.Location = location;
			StringTester.Location += (IriParsing) ? "IriParsing" : "NoIriParsing";
		}

		[TearDown]
		public void Teardown()
		{
			StringTester.Save();
		}

		private string GetTestedChars ()
		{
			var sb = new StringBuilder ();
			for (int c = 0; c <= 0x7f; c++)
				sb.Append ((char) c);

			foreach (char c in nonAsciiTestedChars)
				sb.Append (c);

			return sb.ToString ();
		}

		internal static string HexEscapeMultiByte (char character)
		{
			const string hex_upper_chars = "0123456789ABCDEF";
			string ret = "";
			byte [] bytes = Encoding.UTF8.GetBytes (new [] {character});
			foreach (byte b in bytes)
				ret += "%" + hex_upper_chars [((b & 0xf0) >> 4)] + hex_upper_chars [((b & 0x0f))];

			return ret;
		}

		private void TestScheme(Action<string> action)
		{
			foreach (string scheme in schemes)
				action(scheme);
		}

		private delegate string UriToStringDelegate (Uri uri);

		private void TestLocation (string id, string str, UriToStringDelegate toString, bool testRelative = true)
		{
			TestScheme (scheme => {
				string uri = scheme + str;
				string actual = toString (new Uri (scheme + str, UriKind.Absolute));
				StringTester.Assert (scheme + id, actual, "");
			});

			if (!testRelative)
				return;

			string relActual = toString (new Uri ("./" + str, UriKind.Relative));
			StringTester.Assert ("./" + id, relActual, "");
		}

		private void TestLocations (string [] locations, string id, string str, UriToStringDelegate toString,
			bool testRelative = true)
		{
			foreach (string location in locations)
				TestLocation (location + id, location + str, toString, testRelative);
		}

		private void TestPercentageEncoding (UriToStringDelegate toString, bool testRelative = false, string id = "")
		{
			string unescapedStr = GetTestedChars ();

			var sb = new StringBuilder ();
			foreach (char c in unescapedStr)
				sb.Append (HexEscapeMultiByte (c));
			string escapedStr = sb.ToString ();

			TestLocations (componentLocations, "[un]" + id, escapedStr, toString, testRelative);
			TestLocations (componentLocations, "[es]" + id, escapedStr, toString, testRelative);
		}

		private void TestReduce (UriToStringDelegate toString, bool testRelative = true)
		{
			TestLocations (reduceLocations, "", "", toString, testRelative);
		}

		private void TestComponent (UriComponents component)
		{
			TestPercentageEncoding (uri => uri.GetComponents (component, UriFormat.SafeUnescaped), id: "[SafeUnescaped]");
			TestPercentageEncoding (uri => uri.GetComponents (component, UriFormat.Unescaped), id: "[Unescaped]");
			TestPercentageEncoding (uri => uri.GetComponents (component, UriFormat.UriEscaped), id: "[UriEscaped]");
		}

		[Test]
		public void PercentageEncoding_AbsoluteUri ()
		{
			TestPercentageEncoding (uri => uri.AbsoluteUri);
		}

		[Test]
		public void PercentageEncoding_Fragment ()
		{
			TestPercentageEncoding (uri => uri.Fragment);
		}

		[Test]
		public void PercentageEncoding_GetComponents_AbsoluteUri ()
		{
			TestComponent (UriComponents.AbsoluteUri);
		}

		[Test]
		public void PercentageEncoding_GetComponents_Fragment ()
		{
			TestComponent (UriComponents.Fragment);
		}

		[Test]
		public void PercentageEncoding_GetComponents_Host ()
		{
			TestComponent (UriComponents.Host);
		}

		[Test]
		public void PercentageEncoding_GetComponents_Path ()
		{
			TestComponent (UriComponents.Path);
		}

		[Test]
		public void PercentageEncoding_GetComponents_PathAndQuery ()
		{
			TestComponent (UriComponents.PathAndQuery);
		}

		[Test]
		public void PercentageEncoding_GetComponents_Query ()
		{
			TestComponent (UriComponents.Query);
		}

		[Test]
		public void PercentageEncoding_LocalPath ()
		{
			TestPercentageEncoding (uri => uri.LocalPath);
		}

		[Test]
		public void PercentageEncoding_Query ()
		{
			TestPercentageEncoding (uri => uri.Query);
		}

		[Test]
		public void PercentageEncoding_ToString ()
		{
			TestPercentageEncoding (uri => uri.ToString (), true);
		}

		[Test]
		public void Reduce_AbsoluteUri ()
		{
			TestReduce (uri => uri.AbsoluteUri, false);
		}

		[Test]
		public void Reduce_LocalPath ()
		{
			TestReduce (uri => uri.LocalPath, false);
		}

		[Test]
		public void Reduce_ToString ()
		{
			TestReduce (uri => uri.ToString (), true);
		}
	}
}