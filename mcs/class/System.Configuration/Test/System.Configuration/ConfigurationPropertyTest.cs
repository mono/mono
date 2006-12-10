//
// System.Configuration.ConfigurationElementTest.cs - Unit tests
// for System.Configuration.ConfigurationElement.
//
// Author:
//	Konstantin Triger <kostat@mainsoft.com>
//
// Copyright (C) 2006 Mainsoft, Inc (http://www.mainsoft.com)
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
using System.Configuration;
using NUnit.Framework;

namespace MonoTests.System.Configuration {
	[TestFixture]
	public class ConfigurationPropertyTest
	{
		[Test]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void CostructorTest () {
			ConfigurationProperty poker = new ConfigurationProperty("Name", typeof(char), 5);
		}
		
		[Test]
		public void CostructorTest1 () {
			ConfigurationProperty poker = new ConfigurationProperty("Name", typeof(String));
			Assert.IsNotNull (poker.Validator, "A1");
			Assert.IsNotNull (poker.Converter, "A2");
		}

		[Test]
		public void DefaultValueTest () {
			ConfigurationProperty poker = new ConfigurationProperty("Name", typeof(char));
			Assert.AreEqual (typeof (char), poker.DefaultValue.GetType(), "A1");
			
			ConfigurationProperty poker1 = new ConfigurationProperty("Name", typeof(ConfigurationProperty));
			Assert.AreEqual (null, poker1.DefaultValue, "A2");
			
			ConfigurationProperty poker2 = new ConfigurationProperty("Name", typeof(String));
			Assert.AreEqual (String.Empty, poker2.DefaultValue, "A1");
		}
	}
}

#endif