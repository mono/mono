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

#if NET_4_0
using System;
using System.Collections.Generic;
using System.Data.Services.Providers;
using System.Collections.ObjectModel;

using NUnit.Framework;
using MonoTests.Common;

namespace MonoTests.System.Data.Services.Providers
{
	[TestFixture]
	public class ResourceTypeTest
	{
		[Test]
		public void Constructor ()
		{
			var dummy = new ResourceType (typeof (string), ResourceTypeKind.ComplexType, null, "System", "String", false);
			ResourceType rt;

			AssertExtensions.Throws<ArgumentNullException> (() => {
				rt = new ResourceType (null, ResourceTypeKind.ComplexType, dummy, "System", "Null", false);
			}, "#A1-1");

			AssertExtensions.Throws<ArgumentNullException> (() => {
				rt = new ResourceType (typeof (string), ResourceTypeKind.ComplexType, dummy, "System", null, false);
			}, "#A1-2");

			AssertExtensions.Throws<ArgumentNullException> (() => {
				rt = new ResourceType (typeof (string), ResourceTypeKind.ComplexType, dummy, null, null, false);
			}, "#A1-3");

			AssertExtensions.Throws<ArgumentNullException> (() => {
				rt = new ResourceType (typeof (string), ResourceTypeKind.ComplexType, dummy, "System", String.Empty, false);
			}, "#A1-4");

			AssertExtensions.Throws<ArgumentException> (() => {
				rt = new ResourceType (typeof (string), ResourceTypeKind.Primitive, null, "System", "String", false);
			}, "#A2-1");
			
			AssertExtensions.Throws<ArgumentException> (() => {
				rt = new ResourceType (typeof (bool), ResourceTypeKind.Primitive, null, "System", "Bool", false);
			}, "#A2-2");

			AssertExtensions.Throws<ArgumentException> (() => {
				rt = new ResourceType (typeof (int), ResourceTypeKind.EntityType, null, "System", "Int32", true);
			}, "#A2-3");

			rt = new ResourceType (typeof (string), ResourceTypeKind.ComplexType, dummy, null, "String", false);
			Assert.AreEqual (typeof (string), rt.InstanceType, "#B1-1");
			Assert.AreEqual (ResourceTypeKind.ComplexType, rt.ResourceTypeKind, "#B1-2");
			Assert.AreEqual (dummy, rt.BaseType, "#B1-3");
			Assert.AreEqual (String.Empty, rt.Namespace, "#B1-4");
			Assert.AreEqual ("String", rt.Name, "#B1-5");
			Assert.AreEqual (false, rt.IsAbstract, "#B1-6");
			Assert.AreEqual ("String", rt.FullName, "#B1-7");
			Assert.AreEqual (false, rt.IsReadOnly, "B1-8");
			Assert.AreEqual (false, rt.IsMediaLinkEntry, "B1-9");
			Assert.AreEqual (0, rt.Properties.Count, "B1-10");
			Assert.AreEqual (false, rt.IsOpenType, "B1-11");
			Assert.AreEqual (true, rt.CanReflectOnInstanceType, "#B1-12");

			rt = new ResourceType (typeof (string), ResourceTypeKind.ComplexType, dummy, String.Empty, "String", false);
			Assert.AreEqual (typeof (string), rt.InstanceType, "#B2-1");
			Assert.AreEqual (ResourceTypeKind.ComplexType, rt.ResourceTypeKind, "#B2-2");
			Assert.AreEqual (dummy, rt.BaseType, "#B2-3");
			Assert.AreEqual (String.Empty, rt.Namespace, "#B2-4");
			Assert.AreEqual ("String", rt.Name, "#B2-5");
			Assert.AreEqual (false, rt.IsAbstract, "#B2-6");
			Assert.AreEqual ("String", rt.FullName, "#B2-7");
			Assert.AreEqual (false, rt.IsReadOnly, "B2-8");
			Assert.AreEqual (false, rt.IsMediaLinkEntry, "B2-9");
			Assert.AreEqual (0, rt.Properties.Count, "B2-10");
			Assert.AreEqual (false, rt.IsOpenType, "B2-11");
			Assert.AreEqual (true, rt.CanReflectOnInstanceType, "#B2-12");

			rt = new ResourceType (typeof (string), ResourceTypeKind.ComplexType, null, "System", "String", false);
			Assert.AreEqual (typeof (string), rt.InstanceType, "#B3-1");
			Assert.AreEqual (ResourceTypeKind.ComplexType, rt.ResourceTypeKind, "#B3-2");
			Assert.AreEqual (null, rt.BaseType, "#B3-3");
			Assert.AreEqual ("System", rt.Namespace, "#B3-4");
			Assert.AreEqual ("String", rt.Name, "#B3-5");
			Assert.AreEqual (false, rt.IsAbstract, "#B3-6");
			Assert.AreEqual ("System.String", rt.FullName, "#B3-7");
			Assert.AreEqual (false, rt.IsReadOnly, "B3-8");
			Assert.AreEqual (false, rt.IsMediaLinkEntry, "B3-9");
			Assert.AreEqual (0, rt.Properties.Count, "B3-10");
			Assert.AreEqual (false, rt.IsOpenType, "B3-11");
			Assert.AreEqual (true, rt.CanReflectOnInstanceType, "#B3-12");

			rt = new ResourceType (typeof (string), ResourceTypeKind.EntityType, null, "System", "String", false);
			Assert.AreEqual (typeof (string), rt.InstanceType, "#B4-1");
			Assert.AreEqual (ResourceTypeKind.EntityType, rt.ResourceTypeKind, "#B4-2");
			Assert.AreEqual (null, rt.BaseType, "#B4-3");
			Assert.AreEqual ("System", rt.Namespace, "#B4-4");
			Assert.AreEqual ("String", rt.Name, "#B4-5");
			Assert.AreEqual (false, rt.IsAbstract, "#B4-6");
			Assert.AreEqual ("System.String", rt.FullName, "#B4-7");
			Assert.AreEqual (false, rt.IsReadOnly, "B4-8");
			Assert.AreEqual (false, rt.IsMediaLinkEntry, "B4-9");
			Assert.AreEqual (0, rt.Properties.Count, "B4-10");
			Assert.AreEqual (false, rt.IsOpenType, "B4-11");
			Assert.AreEqual (true, rt.CanReflectOnInstanceType, "#B4-12");

			rt = new ResourceType (typeof (string), ResourceTypeKind.EntityType, null, "System", "String", true);
			Assert.AreEqual (typeof (string), rt.InstanceType, "#B5-1");
			Assert.AreEqual (ResourceTypeKind.EntityType, rt.ResourceTypeKind, "#B5-2");
			Assert.AreEqual (null, rt.BaseType, "#B5-3");
			Assert.AreEqual ("System", rt.Namespace, "#B5-4");
			Assert.AreEqual ("String", rt.Name, "#B5-5");
			Assert.AreEqual (true, rt.IsAbstract, "#B5-6");
			Assert.AreEqual ("System.String", rt.FullName, "#B5-7");
			Assert.AreEqual (false, rt.IsReadOnly, "B5-8");
			Assert.AreEqual (false, rt.IsMediaLinkEntry, "B5-9");
			Assert.AreEqual (0, rt.Properties.Count, "B5-10");
			Assert.AreEqual (false, rt.IsOpenType, "B5-11");
			Assert.AreEqual (true, rt.CanReflectOnInstanceType, "#B5-12");
		}

		[Test]
		public void CanReflectOnInstanceType ()
		{
			var rt = new ResourceType (typeof (string), ResourceTypeKind.EntityType, null, "System", "String", true);
			Assert.AreEqual (true, rt.CanReflectOnInstanceType, "#A1-1");
			rt.CanReflectOnInstanceType = false;
			Assert.AreEqual (false, rt.CanReflectOnInstanceType, "#A1-2");

			rt = new ResourceType (typeof (ResourceTypeTest), ResourceTypeKind.ComplexType, null, "MonoTests.System.Data.Services.Providers", "ResourceTypeTest", true);
			Assert.AreEqual (true, rt.CanReflectOnInstanceType, "#A2-1");
			rt.CanReflectOnInstanceType = false;
			Assert.AreEqual (false, rt.CanReflectOnInstanceType, "#A2-2");
		}

		[Test]
		public void IsMediaLinkEntry ()
		{
			var rt = new ResourceType (typeof (string), ResourceTypeKind.EntityType, null, "System", "String", true);
			Assert.AreEqual (false, rt.IsMediaLinkEntry, "#C1-1");
			rt.IsMediaLinkEntry = true;
			Assert.AreEqual (true, rt.IsMediaLinkEntry, "#C1-2");

			rt = new ResourceType (typeof (ResourceTypeTest), ResourceTypeKind.ComplexType, null, "MonoTests.System.Data.Services.Providers", "ResourceTypeTest", true);
			Assert.AreEqual (false, rt.IsMediaLinkEntry, "#C2-1");
			rt.IsMediaLinkEntry = true;
			Assert.AreEqual (true, rt.IsMediaLinkEntry, "#C2-2");
		}

		[Test]
		public void IsOpenType ()
		{
			var rt = new ResourceType (typeof (string), ResourceTypeKind.EntityType, null, "System", "String", true);
			Assert.AreEqual (false, rt.IsOpenType, "#D1-1");
			rt.IsOpenType = true;
			Assert.AreEqual (true, rt.IsOpenType, "#D1-2");

			rt = new ResourceType (typeof (ResourceTypeTest), ResourceTypeKind.ComplexType, null, "MonoTests.System.Data.Services.Providers", "ResourceTypeTest", true);
			Assert.AreEqual (false, rt.IsOpenType, "#D2-1");
			rt.IsOpenType = true;
			Assert.AreEqual (true, rt.IsOpenType, "#D2-2");
		}

		[Test]
		public void SetReadOnly ()
		{
			var rt = new ResourceType (typeof (string), ResourceTypeKind.EntityType, null, "System", "String", true);
			Assert.AreEqual (false, rt.IsReadOnly, "#E1-1");
			rt.SetReadOnly ();
			Assert.AreEqual (true, rt.IsReadOnly, "#E1-2");
		}

		[Test]
		public void AddProperty ()
		{
			ResourceType rt;

			AssertExtensions.Throws<ArgumentNullException> (() => {
				rt = new ResourceType (typeof (string), ResourceTypeKind.EntityType, null, "System", "String", true);
				rt.AddProperty (null);
			}, "#F1-1");

			rt = new ResourceType (typeof (string), ResourceTypeKind.EntityType, null, "System", "String", false);
			Assert.AreEqual (0, rt.Properties.Count, "#F2-1");
			var rp = new ResourceProperty ("String", ResourcePropertyKind.ResourceReference, rt);
			rt.AddProperty (rp);
			Assert.AreEqual (1, rt.Properties.Count, "#F2-2");

			AssertExtensions.Throws<InvalidOperationException> (() => {
				rt.AddProperty (rp);
			}, "#F3-1");
			Assert.AreEqual (1, rt.Properties.Count, "#F3-2");

			rp = new ResourceProperty ("string", ResourcePropertyKind.ResourceReference, rt);
			rt.AddProperty (rp);
			Assert.AreEqual (2, rt.Properties.Count, "#F4-1");

			AssertExtensions.Throws<InvalidOperationException> (() => {
				rt.AddProperty (rp);
			}, "#F4-1");
			Assert.AreEqual (2, rt.Properties.Count, "#F4-2");

			// currently uniqueness is checked only by Property.Name, case sensitive
			var rt2 = new ResourceType (typeof (string), ResourceTypeKind.EntityType, null, "My System", "My String", false);
			rt2.AddProperty (rp);
			Assert.AreEqual (1, rt2.Properties.Count, "#F5-1");
			var rp2 = new ResourceProperty ("string", ResourcePropertyKind.ResourceReference, rt2);
			AssertExtensions.Throws<InvalidOperationException> (() => {
				rt2.AddProperty (rp2);
			}, "#F5-2");
			Assert.AreEqual (1, rt2.Properties.Count, "#F5-3");
		}

		[Test]
		public void Properties ()
		{
			var rt = new ResourceType (typeof (string), ResourceTypeKind.EntityType, null, "System", "String", true);
			Assert.AreEqual (typeof (ReadOnlyCollection<ResourceProperty>).ToString (), rt.Properties.ToString (), "#G1-1");

			Assert.AreEqual (0, rt.Properties.Count, "#G2-1");
		}

		[Test]
		public void GetPrimitiveResourceType ()
		{
			Type notPrimitiveValueType = typeof (Nullable<Int32>);
			var prt = ResourceType.GetPrimitiveResourceType (notPrimitiveValueType);
			Assert.AreEqual (null, prt, "#H1-1");

			Type notValueType = typeof (ResourceType);
			prt = ResourceType.GetPrimitiveResourceType (notValueType);
			Assert.AreEqual (null, prt, "#H2-1");

			Type primitiveValueType = typeof (Int32);
			prt = ResourceType.GetPrimitiveResourceType (primitiveValueType);
			Assert.AreEqual (ResourceTypeKind.Primitive, prt.ResourceTypeKind, "#H3-1");
			Assert.AreEqual (true, prt.IsReadOnly, "#H3-2");
			Assert.AreEqual (false, prt.IsMediaLinkEntry, "#H3-3");
			Assert.AreEqual (false, prt.IsAbstract, "#H3-4");
			Assert.AreEqual (false, prt.IsOpenType, "#H3-5");
			Assert.AreEqual (null, prt.BaseType, "#H3-6");
			Assert.AreEqual (true, prt.CanReflectOnInstanceType, "#H3-7");
			Assert.AreEqual (typeof (Int32), prt.InstanceType, "#H3-8");
			Assert.AreEqual ("System.Int32", prt.FullName, "#H3-9");
			Assert.AreEqual ("System", prt.Namespace, "#H3-10");
			Assert.AreEqual ("Int32", prt.Name, "#H3-11");
			Assert.AreEqual (0, prt.Properties.Count, "#H3-12");
		}
	}
}
#endif
