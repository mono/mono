// ***********************************************************************
// Copyright (c) 2007 Charlie Poole
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
// ***********************************************************************

using System;
using System.Threading;
using System.Globalization;
using NUnit.Framework.Api;
using NUnit.TestData.CultureAttributeData;
using NUnit.TestUtilities;

namespace NUnit.Framework.Internal
{
	/// <summary>
	/// Summary description for CultureDetectionTests.
	/// </summary>
	[TestFixture]
	public class CultureSettingAndDetectionTests
	{
		private NUnit.Framework.Internal.CultureDetector detector = new NUnit.Framework.Internal.CultureDetector("fr-FR");

		private void ExpectMatch( string culture )
		{
			if ( !detector.IsCultureSupported( culture ) )
				Assert.Fail( string.Format( "Failed to match \"{0}\"" , culture ) );
		}

		private void ExpectMatch( CultureAttribute attr )
		{
			if ( !detector.IsCultureSupported( attr ) )
				Assert.Fail( string.Format( "Failed to match attribute with Include=\"{0}\",Exclude=\"{1}\"", attr.Include, attr.Exclude ) );
		}

		private void ExpectFailure( string culture )
		{
			if ( detector.IsCultureSupported( culture ) )
				Assert.Fail( string.Format( "Should not match \"{0}\"" , culture ) );
			Assert.AreEqual( "Only supported under culture " + culture, detector.Reason );
		}

		private void ExpectFailure( CultureAttribute attr, string msg )
		{
			if ( detector.IsCultureSupported( attr ) )
				Assert.Fail( string.Format( "Should not match attribute with Include=\"{0}\",Exclude=\"{1}\"",
					attr.Include, attr.Exclude ) );
			Assert.AreEqual( msg, detector.Reason );
		}

		[Test]
		public void CanMatchStrings()
		{
			ExpectMatch( "fr-FR" );
			ExpectMatch( "fr" );
			ExpectMatch( "fr-FR,fr-BE,fr-CA" );
			ExpectMatch( "en,de,fr,it" );
			ExpectFailure( "en-GB" );
			ExpectFailure( "en" );
			ExpectFailure( "fr-CA" );
			ExpectFailure( "fr-BE,fr-CA" );
			ExpectFailure( "en,de,it" );
		}

		[Test]
		public void CanMatchAttributeWithInclude()
		{
			ExpectMatch( new CultureAttribute( "fr-FR" ) );
			ExpectMatch( new CultureAttribute( "fr-FR,fr-BE,fr-CA" ) );
			ExpectFailure( new CultureAttribute( "en" ), "Only supported under culture en" );
			ExpectFailure( new CultureAttribute( "en,de,it" ), "Only supported under culture en,de,it" );
		}

		[Test]
		public void CanMatchAttributeWithExclude()
		{
			CultureAttribute attr = new CultureAttribute();
			attr.Exclude = "en";
			ExpectMatch( attr );
			attr.Exclude = "en,de,it";
			ExpectMatch( attr );
			attr.Exclude = "fr";
			ExpectFailure( attr, "Not supported under culture fr");
			attr.Exclude = "fr-FR,fr-BE,fr-CA";
			ExpectFailure( attr, "Not supported under culture fr-FR,fr-BE,fr-CA" );
		}

		[Test]
		public void CanMatchAttributeWithIncludeAndExclude()
		{
			CultureAttribute attr = new CultureAttribute( "en,fr,de,it" );
			attr.Exclude="fr-CA,fr-BE";
			ExpectMatch( attr );
			attr.Exclude = "fr-FR";
			ExpectFailure( attr, "Not supported under culture fr-FR" );
		}

#if !NETCF
		[Test,SetCulture("fr-FR")]
		public void LoadWithFrenchCulture()
		{
			Assert.AreEqual( "fr-FR", CultureInfo.CurrentCulture.Name, "Culture not set correctly" );
			TestSuite fixture = TestBuilder.MakeFixture( typeof( FixtureWithCultureAttribute ) );
			Assert.AreEqual( RunState.Runnable, fixture.RunState, "Fixture" );
			foreach( Test test in fixture.Tests )
			{
				RunState expected = test.Name == "FrenchTest" ? RunState.Runnable : RunState.Skipped;
				Assert.AreEqual( expected, test.RunState, test.Name );
			}
		}

		[Test,SetCulture("fr-CA")]
		public void LoadWithFrenchCanadianCulture()
		{
			Assert.AreEqual( "fr-CA", CultureInfo.CurrentCulture.Name, "Culture not set correctly" );
			TestSuite fixture = TestBuilder.MakeFixture( typeof( FixtureWithCultureAttribute ) );
			Assert.AreEqual( RunState.Runnable, fixture.RunState, "Fixture" );
			foreach( Test test in fixture.Tests )
			{
				RunState expected = test.Name.StartsWith( "French" ) ? RunState.Runnable : RunState.Skipped;
				Assert.AreEqual( expected, test.RunState, test.Name );
			}
		}

		[Test,SetCulture("ru-RU")]
		public void LoadWithRussianCulture()
		{
			Assert.AreEqual( "ru-RU", CultureInfo.CurrentCulture.Name, "Culture not set correctly" );
			TestSuite fixture = TestBuilder.MakeFixture( typeof( FixtureWithCultureAttribute ) );
			Assert.AreEqual( RunState.Skipped, fixture.RunState, "Fixture" );
			foreach( Test test in fixture.Tests )
				Assert.AreEqual( RunState.Skipped, test.RunState, test.Name );
		}

        [Test]
        public void SettingInvalidCultureOnFixtureGivesError()
        {
            ITestResult result = TestBuilder.RunTestFixture(typeof(FixtureWithInvalidSetCultureAttribute));
            Assert.AreEqual(ResultState.Error, result.ResultState);
            Assert.That(result.Message, Is.StringStarting("System.ArgumentException").Or.StringStarting("System.Globalization.CultureNotFoundException"));
            Assert.That(result.Message, Is.StringContaining("xx-XX").IgnoreCase);
        }

        [Test]
        public void SettingInvalidCultureOnTestGivesError()
        {
            ITestResult result = TestBuilder.RunTestCase(typeof(FixtureWithInvalidSetCultureAttributeOnTest), "InvalidCultureSet");
            Assert.AreEqual(ResultState.Error, result.ResultState);
            Assert.That(result.Message, Is.StringStarting("System.ArgumentException").Or.StringStarting("System.Globalization.CultureNotFoundException"));
            Assert.That(result.Message, Is.StringContaining("xx-XX").IgnoreCase);
        }

        [TestFixture, SetCulture("en-GB")]
		public class NestedFixture
		{
			[Test]
			public void CanSetCultureOnFixture()
			{
				Assert.AreEqual( "en-GB", CultureInfo.CurrentCulture.Name );
			}
		}
#endif
	}
}
