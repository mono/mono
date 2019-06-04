//
// ExtensionsSectionTest.cs
//
// Author:
//	Igor Zelmanovich <igorz@mainsoft.com>
//
// Copyright (C) 2008 Mainsoft, Inc.  http://www.mainsoft.com
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
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel.Configuration;
using System.Configuration;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Configuration
{
	[TestFixture]
	public class ExtensionsSectionTest
	{
		class Poker : ExtensionsSection
		{
			public ConfigurationPropertyCollection GetProperties () {
				return Properties;
			}

			[ConfigurationProperty ("myProperty")]
			string MyProperty {
				get { return "myProperty"; }
				set { }
			}
		}

		[Test]
		public void Properties () {

			Poker p1 = new Poker ();
			Poker p2 = new Poker ();

			Assert.AreEqual (false, p1.GetProperties ().Contains ("myProperty"), "Contains myProperty");
			Assert.AreEqual (false, p1.GetProperties () == p2.GetProperties (), "#");
		}

		[Test]
		public void BindingExtensions () {
			ServiceModelSectionGroup config = (ServiceModelSectionGroup) ConfigurationManager.OpenExeConfiguration (TestResourceHelper.GetFullPathOfResource ("Test/config/extensions")).GetSectionGroup ("system.serviceModel");

			Assert.AreEqual (typeof (BasicHttpBindingCollectionElement).AssemblyQualifiedName, config.Extensions.BindingExtensions ["basicHttpBinding"].Type, "baseHttpBinding");
			Assert.AreEqual (typeof (NetTcpBindingCollectionElement).AssemblyQualifiedName, config.Extensions.BindingExtensions ["netTcpBinding"].Type, "baseHttpBinding");
			Assert.AreEqual (typeof (CustomBindingCollectionElement).AssemblyQualifiedName, config.Extensions.BindingExtensions ["customBinding"].Type, "baseHttpBinding");
			Assert.AreEqual ("MyBindingCollectionElement", config.Extensions.BindingExtensions ["bindingExtensions1"].Type, "MyBindingCollectionElement");
			Assert.AreEqual ("AnotherBindingCollectionElement", config.Extensions.BindingExtensions ["bindingExtensions2"].Type, "AnotherBindingCollectionElement");
		}

		[Test]
		public void BehaviorExtensions () {
			ServiceModelSectionGroup config = (ServiceModelSectionGroup) ConfigurationManager.OpenExeConfiguration (TestResourceHelper.GetFullPathOfResource ("Test/config/extensions")).GetSectionGroup ("system.serviceModel");

			Assert.AreEqual (typeof (ServiceAuthorizationElement).AssemblyQualifiedName, config.Extensions.BehaviorExtensions ["serviceAuthorization"].Type, "serviceAuthorization");
			Assert.AreEqual ("MyBehaviorElement", config.Extensions.BehaviorExtensions ["behaviorExtensions1"].Type, "MyBehaviorElement");
			Assert.AreEqual ("AnotherBehaviorElement", config.Extensions.BehaviorExtensions ["behaviorExtensions2"].Type, "AnotherBehaviorElement");
		}

		[Test]
		public void BindingElementExtensions () {
			ServiceModelSectionGroup config = (ServiceModelSectionGroup) ConfigurationManager.OpenExeConfiguration (TestResourceHelper.GetFullPathOfResource ("Test/config/extensions")).GetSectionGroup ("system.serviceModel");

			Assert.AreEqual (typeof (BinaryMessageEncodingElement).AssemblyQualifiedName, config.Extensions.BindingElementExtensions ["binaryMessageEncoding"].Type, "binaryMessageEncoding");
			Assert.AreEqual ("MyBindingElementElement", config.Extensions.BindingElementExtensions ["bindingElementExtensions1"].Type, "MyBindingElementElement");
			Assert.AreEqual ("AnotherBindingElementElement", config.Extensions.BindingElementExtensions ["bindingElementExtensions2"].Type, "AnotherBindingElementElement");
		}
	}
}
#endif
