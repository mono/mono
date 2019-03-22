//
// System.Configuration.ConfigurationSectionTest.cs - Unit tests
//
// Author:
//	Greg Smolyn
//	Gonzalo Paniagua Javier <gonzalo@novell.com
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System;
using System.Configuration;
using System.IO;
using System.Xml;
using NUnit.Framework;

using MonoTests.System.Configuration.Util;
using MonoTests.System.Configuration.ConfigurationSectionTestHelpers;

namespace MonoTests.System.Configuration {

	[TestFixture]
	public class ConfigurationSectionTest
	{
		const string DefaultConfigSectionConfig = @"
<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
    <configSections>
       <section name=""DefaultConfigSection"" type=""System.Configuration.DefaultSection"" />
    </configSections>
    <DefaultConfigSection>
    </DefaultConfigSection>
</configuration>";

		static readonly string SimpleSectionConfig = string.Format (@"
<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
    <configSections>
	   <section name=""SimpleSection"" type=""{0}"" />
    </configSections>
	<SimpleSection>
	</SimpleSection>
</configuration>", typeof(SimpleSection).AssemblyQualifiedName);

		static readonly string ProtectedCtorConfig = string.Format (@"
<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
    <configSections>
	   <section name=""ProtectedCtorSection"" type=""{0}"" />
    </configSections>
	<ProtectedCtorSection>
	</ProtectedCtorSection>
</configuration>", typeof(ProtectedCtorSection).AssemblyQualifiedName);

		[Test]
		public void GetRawXmlTest ()
		{
			TestUtil.RunWithTempFile ((tmpFileName) =>
			{
				var config = TestUtil.WriteXmlToFileAndOpenConfiguration (DefaultConfigSectionConfig, tmpFileName);

				var section = config.Sections["DefaultConfigSection"] as DefaultSection;
			 	var rawXml = section.SectionInformation.GetRawXml ();

				Assert.IsNotNull (rawXml, "#1: : GetRawXml() returns null");
				Assert.IsFalse (string.IsNullOrEmpty (rawXml), "#2: GetRawXml() returns String.Empty");
			});
		}

		[Test]
		public void SetRawXmlTest_DefaultSection ()
		{
			TestUtil.RunWithTempFile ((tmpFileName) => {
				CreateConfig (tmpFileName);
				TestConfig (tmpFileName);
			});

			void CreateConfig (string fileName)
			{
				var config = TestUtil.WriteXmlToFileAndOpenConfiguration (DefaultConfigSectionConfig, fileName);
				var section = config.Sections["DefaultConfigSection"] as DefaultSection;
			 	section.SectionInformation.SetRawXml ("<DefaultConfigSection><TestTag /></DefaultConfigSection>");
				config.Save(ConfigurationSaveMode.Full);
			}

			void TestConfig (string fileName)
			{
				var config = TestUtil.OpenConfiguration (fileName);
				var section = config.Sections["DefaultConfigSection"] as DefaultSection;
				var rawXml = section.SectionInformation.GetRawXml ();

				Assert.IsNotNull (rawXml, "#1: : GetRawXml() returns null");
				Assert.IsFalse (string.IsNullOrEmpty (rawXml), "#2: GetRawXml() returns String.Empty");

				var xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(rawXml);
				var testTagNode = xmlDoc.SelectSingleNode (@"DefaultConfigSection/TestTag");
				Assert.IsNotNull (testTagNode, "#1: :testTagNode is null");
			}
		}

		[Test]
		public void SetRawXmlTest_SimpleSection ()
		{
			TestUtil.RunWithTempFile ((tmpFileName) => {
				CreateConfig (tmpFileName, "<SimpleSection></SimpleSection>");
				TestConfig (tmpFileName, SimpleElement.Default.Value, "#1");
			});

			TestUtil.RunWithTempFile ((tmpFileName) => {
				CreateConfig (tmpFileName, "<SimpleSection><SimpleElement /></SimpleSection>");
				TestConfig (tmpFileName, SimpleElement.Default.Value, "#2");
			});

			TestUtil.RunWithTempFile ((tmpFileName) => {
				CreateConfig (tmpFileName, @"<SimpleSection><SimpleElement Value=""2"" /></SimpleSection>");
				TestConfig (tmpFileName, 2, "#3");
			});

			void CreateConfig (string fileName, string sectionRawXml)
			{
				var config = TestUtil.WriteXmlToFileAndOpenConfiguration (SimpleSectionConfig, fileName);
				var section = config.Sections["SimpleSection"] as SimpleSection;

				section.SimpleElement.Value = 1;
				section.SectionInformation.SetRawXml (sectionRawXml);

				config.Save(ConfigurationSaveMode.Full);
			}

			void TestConfig (string fileName, int expectedValue, string testLabel)
			{
				var config = TestUtil.OpenConfiguration (fileName);
				var section = config.Sections["SimpleSection"] as SimpleSection;
				Assert.AreEqual (section.SimpleElement.Value, expectedValue, testLabel);
			}
		}
	}
}

namespace MonoTests.System.Configuration.ConfigurationSectionTestHelpers
{
	public class SimpleSection : ConfigurationSection
	{
		[ConfigurationProperty("SimpleElement")]
		public SimpleElement SimpleElement
		{
			get { return (SimpleElement)this["SimpleElement"]; }
			set { this["SimpleElement"] = value; }
		}
	}

	public class SimpleElement : ConfigurationElement
	{
		public static SimpleElement Default
		{
			get { return new SimpleElement(); }
		}

		[ConfigurationProperty("Value", DefaultValue = "0")]
		public int Value
		{
			get { return (int)this["Value"]; }
			set { this["Value"] = value; }
		}	
	}
}

