//
// BindingsSectionTest.cs
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
using NUnit.Framework;
using System.ServiceModel.Configuration;
using System.ServiceModel.Channels;
using System.Configuration;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel.Configuration
{
	[TestFixture]
	public class BindingsSectionTest
	{

		[Test]
		[Category("NotWorking")]
		[Ignore ("fails under .NET; I never bothered to fix the test")]
		public void UserConfiguration () {
			ServiceModelSectionGroup config = (ServiceModelSectionGroup) ConfigurationManager.OpenExeConfiguration (TestResourceHelper.GetFullPathOfResource ("Test/config/userBinding")).GetSectionGroup ("system.serviceModel");

			BindingsSection section = config.Bindings;

			BindingCollectionElement collectionElement = section ["userBinding"];
			Assert.AreEqual (typeof (UserBindingCollectionElement), collectionElement.GetType (), "type");

			StandardBindingElementCollection<UserBindingElement> userBindings = ((UserBindingCollectionElement) collectionElement).Bindings;

			Assert.AreEqual (2, userBindings.Count, "Count");

			Assert.AreEqual ("UserBinding_1", userBindings [0].Name, "Name_1");
			Assert.AreEqual ("UserBinding_2", userBindings [1].Name, "Name_2");
		}
	}
}
#endif