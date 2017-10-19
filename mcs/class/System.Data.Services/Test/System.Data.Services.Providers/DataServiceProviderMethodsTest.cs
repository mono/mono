// 
// DataServiceProviderMethods.cs
//  
// Author:
//       Marek Habersack <grendel@twistedcode.net>
// 
// Copyright (c) 2011 Novell, Inc
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Data.Services.Providers;

using NUnit.Framework;

namespace MonoTests.System.Data.Services.Providers
{
	[TestFixture]
	public class DataServiceProviderMethodsTest
	{
		[Test]
		public void TypeIs ()
		{
			var rt = new ResourceType (typeof (string), ResourceTypeKind.ComplexType, null, "System", "String", false);
			Assert.Throws<NotImplementedException> (() => {
				DataServiceProviderMethods.TypeIs ("test", rt);
			}, "#A1");

			Assert.Throws<NotImplementedException> (() => {
				DataServiceProviderMethods.TypeIs (null, null);
			}, "#A2");
		}

		[Test]
		public void Convert ()
		{
			var rt = new ResourceType (typeof (string), ResourceTypeKind.ComplexType, null, "System", "String", false);
			Assert.Throws<NotImplementedException> (() => {
				DataServiceProviderMethods.Convert ("test", rt);
			}, "#A1");

			Assert.Throws<NotImplementedException> (() => {
				DataServiceProviderMethods.Convert (null, null);
			}, "#A2");
		}

		[Test]
		public void GetSequenceValue ()
		{
			var rt = new ResourceType (typeof (string), ResourceTypeKind.ComplexType, null, "System", "String", false);
			var rp = new ResourceProperty ("Length", ResourcePropertyKind.ComplexType, rt);
			Assert.Throws<NotImplementedException> (() => {
				DataServiceProviderMethods.GetSequenceValue<string> ("test", rp);
			}, "#A1");

			Assert.Throws<NotImplementedException> (() => {
				DataServiceProviderMethods.GetSequenceValue<string> (null, null);
			}, "#A2");
		}

		[Test]
		public void GetValue ()
		{
			var rt = new ResourceType (typeof (string), ResourceTypeKind.ComplexType, null, "System", "String", false);
			var rp = new ResourceProperty ("Length", ResourcePropertyKind.ComplexType, rt);
			Assert.Throws<NotImplementedException> (() => {
				DataServiceProviderMethods.GetValue ("test", rp);
			}, "#A1");

			Assert.Throws<NotImplementedException> (() => {
				DataServiceProviderMethods.GetValue (null, null);
			}, "#A2");
		}

		[Test]
		public void Compare_String_String ()
		{
			Assert.AreEqual (1, DataServiceProviderMethods.Compare ("right", "left"), "#A1");
			Assert.AreEqual (0, DataServiceProviderMethods.Compare ("right", "right"), "#A2");
			Assert.AreEqual (0, DataServiceProviderMethods.Compare (String.Empty, String.Empty), "#A3");
			Assert.AreEqual (-1, DataServiceProviderMethods.Compare ("left", "right"), "#A4");
			Assert.AreEqual (-1, DataServiceProviderMethods.Compare (null, "right"), "#A5");
			Assert.AreEqual (1, DataServiceProviderMethods.Compare ("right", null), "#A6");
			Assert.AreEqual (0, DataServiceProviderMethods.Compare ((string)null, (string)null), "#A7");
		}

		[Test]
		public void Compare_Bool_Bool ()
		{
			Assert.AreEqual (-1, DataServiceProviderMethods.Compare (false, true), "#A1");
			Assert.AreEqual (0, DataServiceProviderMethods.Compare (false, false), "#A2");
			Assert.AreEqual (1, DataServiceProviderMethods.Compare (true, false), "#A3");
			Assert.AreEqual (0, DataServiceProviderMethods.Compare (true, true), "#A4");
		}

		[Test]
		public void Compare_NullableBool_NullableBool ()
		{
			Assert.AreEqual (-1, DataServiceProviderMethods.Compare ((bool?) false, (bool?) true), "#A1");
			Assert.AreEqual (0, DataServiceProviderMethods.Compare ((bool?) false, (bool?) false), "#A2");
			Assert.AreEqual (1, DataServiceProviderMethods.Compare ((bool?) true, (bool?) false), "#A3");
			Assert.AreEqual (0, DataServiceProviderMethods.Compare ((bool?) true, (bool?) true), "#A4");

			Assert.AreEqual (1, DataServiceProviderMethods.Compare ((bool?) false, null), "#B1");
			Assert.AreEqual (1, DataServiceProviderMethods.Compare ((bool?) true, null), "#B2");
			Assert.AreEqual (-1, DataServiceProviderMethods.Compare (null, (bool?)false), "#B3");
			Assert.AreEqual (-1, DataServiceProviderMethods.Compare (null, (bool?) true), "#B4");
			Assert.AreEqual (0, DataServiceProviderMethods.Compare ((bool?) null, (bool?) null), "#B5");
		}

		[Test]
		public void Compare_Guid_Guid ()
		{
			var guid1 = new Guid ("bdec809c-f8c5-4bc9-8b56-fb34a12a3e1c");
			var guid2 = new Guid ("898b2fe2-3530-4f56-85de-79344e59a90f");

			Assert.AreEqual (1, DataServiceProviderMethods.Compare (guid1, guid2), "#A1");
			Assert.AreEqual (0, DataServiceProviderMethods.Compare (guid1, guid1), "#A2");
			Assert.AreEqual (-1, DataServiceProviderMethods.Compare (guid2, guid1), "#A3");

			guid1 = new Guid ("00000000-0000-0000-0000-000000000000");
			guid2 = new Guid ("00000000-0000-0000-0000-000000000001");
			Assert.AreEqual (-1, DataServiceProviderMethods.Compare (guid1, guid2), "#B1");
			Assert.AreEqual (1, DataServiceProviderMethods.Compare (guid2, guid1), "#B2");
		}

		[Test]
		public void Compare_NullableGuid_NullableGuid ()
		{
			var guid1 = new Guid ("bdec809c-f8c5-4bc9-8b56-fb34a12a3e1c");
			var guid2 = new Guid ("898b2fe2-3530-4f56-85de-79344e59a90f");

			Assert.AreEqual (1, DataServiceProviderMethods.Compare (guid1, guid2), "#A1");
			Assert.AreEqual (0, DataServiceProviderMethods.Compare (guid1, guid1), "#A2");
			Assert.AreEqual (-1, DataServiceProviderMethods.Compare (guid2, guid1), "#A3");

			guid1 = new Guid ("00000000-0000-0000-0000-000000000000");
			guid2 = new Guid ("00000000-0000-0000-0000-000000000001");
			Assert.AreEqual (-1, DataServiceProviderMethods.Compare (guid1, guid2), "#B1");
			Assert.AreEqual (1, DataServiceProviderMethods.Compare (guid2, guid1), "#B2");

			Assert.AreEqual (1, DataServiceProviderMethods.Compare ((Guid?)guid1, (Guid?)null), "#C1");
			Assert.AreEqual (0, DataServiceProviderMethods.Compare ((Guid?)null, (Guid?)null), "#C2");
			Assert.AreEqual (-1, DataServiceProviderMethods.Compare ((Guid?) null, (Guid?)guid1), "#C3");
		}
	}
}
