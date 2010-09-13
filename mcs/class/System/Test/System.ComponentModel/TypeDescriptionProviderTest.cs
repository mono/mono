//
// TypeDescriptionProviderTest.cs
//
// Author:
//	Carlos Alberto Cortez (calberto.cortez@gmail.com)
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class TypeDescriptionProviderTest
	{
#if NET_4_0
		[Test]
		public void IsSupportedType ()
		{
			CustomDescriptionProvider provider = new CustomDescriptionProvider ();
			Assert.IsTrue (provider.IsSupportedType (typeof (string)), "#A1");
			Assert.IsTrue (provider.IsSupportedType (typeof (object)), "#A2");
			Assert.IsTrue (provider.IsSupportedType (typeof (CustomDescriptionProvider)), "#A3");
		}

		[Test]
		public void GetRuntimeType ()
		{
			CustomDescriptionProvider provider = new CustomDescriptionProvider ();
			Assert.AreEqual (typeof (CustomDescriptionProvider), provider.GetRuntimeType (typeof (CustomDescriptionProvider)), "#A0");
			Assert.AreEqual (typeof (object), provider.GetRuntimeType (typeof (object)), "#A1");
		}

		[Test]
		public void GetExtenderProviders ()
		{
			CustomDescriptionProvider provider = new CustomDescriptionProvider ();
			IExtenderProvider [] providers = provider.GetExtenderProviders (typeof (object));
			Assert.IsNotNull (providers, "#A0");
			Assert.AreEqual (0, providers.Length, "#A1");
		}

		class CustomDescriptionProvider : TypeDescriptionProvider
		{
			public new IExtenderProvider [] GetExtenderProviders (object instance)
			{
				return base.GetExtenderProviders (instance);
			}
		}
#endif
	}
}

