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
	public class ResourceTypeTest
	{
		[Test]
		public void Constructor ()
		{
			var dummy = new ResourceType (typeof (string), ResourceTypeKind.ComplexType, null, "System", "String", false);
			ResourceType rt;

			Assert.Throws<ArgumentNullException> (() => {
				rt = new ResourceType (null, ResourceTypeKind.ComplexType, dummy, "System", "Null", false);
			}, "#A1-1");

			Assert.Throws<ArgumentNullException> (() => {
				rt = new ResourceType (typeof (string), ResourceTypeKind.ComplexType, dummy, "System", null, false);
			}, "#A1-2");

			Assert.Throws<ArgumentNullException> (() => {
				rt = new ResourceType (typeof (string), ResourceTypeKind.ComplexType, dummy, null, null, false);
			}, "#A1-3");

			Assert.Throws<ArgumentNullException> (() => {
				rt = new ResourceType (typeof (string), ResourceTypeKind.ComplexType, dummy, "System", String.Empty, false);
			}, "#A1-4");

			Assert.Throws<ArgumentException> (() => {
				rt = new ResourceType (typeof (string), ResourceTypeKind.Primitive, null, "System", "String", false);
			}, "#A2-1");
			
			Assert.Throws<ArgumentException> (() => {
				rt = new ResourceType (typeof (bool), ResourceTypeKind.Primitive, null, "System", "Bool", false);
			}, "#A2-2");

			Assert.Throws<ArgumentException> (() => {
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

			rt = new ResourceType (typeof (string), ResourceTypeKind.ComplexType, dummy, String.Empty, "String", false);
			Assert.AreEqual (typeof (string), rt.InstanceType, "#B2-1");
			Assert.AreEqual (ResourceTypeKind.ComplexType, rt.ResourceTypeKind, "#B2-2");
			Assert.AreEqual (dummy, rt.BaseType, "#B2-3");
			Assert.AreEqual (String.Empty, rt.Namespace, "#B2-4");
			Assert.AreEqual ("String", rt.Name, "#B2-5");
			Assert.AreEqual (false, rt.IsAbstract, "#B2-6");
			Assert.AreEqual ("String", rt.FullName, "#B2-7");

			rt = new ResourceType (typeof (string), ResourceTypeKind.ComplexType, null, "System", "String", false);
			Assert.AreEqual (typeof (string), rt.InstanceType, "#B3-1");
			Assert.AreEqual (ResourceTypeKind.ComplexType, rt.ResourceTypeKind, "#B3-2");
			Assert.AreEqual (null, rt.BaseType, "#B3-3");
			Assert.AreEqual ("System", rt.Namespace, "#B3-4");
			Assert.AreEqual ("String", rt.Name, "#B3-5");
			Assert.AreEqual (false, rt.IsAbstract, "#B3-6");
			Assert.AreEqual ("System.String", rt.FullName, "#B3-7");

			rt = new ResourceType (typeof (string), ResourceTypeKind.EntityType, null, "System", "String", false);
			Assert.AreEqual (typeof (string), rt.InstanceType, "#B4-1");
			Assert.AreEqual (ResourceTypeKind.EntityType, rt.ResourceTypeKind, "#B4-2");
			Assert.AreEqual (null, rt.BaseType, "#B4-3");
			Assert.AreEqual ("System", rt.Namespace, "#B4-4");
			Assert.AreEqual ("String", rt.Name, "#B4-5");
			Assert.AreEqual (false, rt.IsAbstract, "#B4-6");
			Assert.AreEqual ("System.String", rt.FullName, "#B4-7");

			rt = new ResourceType (typeof (string), ResourceTypeKind.EntityType, null, "System", "String", true);
			Assert.AreEqual (typeof (string), rt.InstanceType, "#B5-1");
			Assert.AreEqual (ResourceTypeKind.EntityType, rt.ResourceTypeKind, "#B5-2");
			Assert.AreEqual (null, rt.BaseType, "#B5-3");
			Assert.AreEqual ("System", rt.Namespace, "#B5-4");
			Assert.AreEqual ("String", rt.Name, "#B5-5");
			Assert.AreEqual (true, rt.IsAbstract, "#B5-6");
			Assert.AreEqual ("System.String", rt.FullName, "#B5-7");
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
	}
}
