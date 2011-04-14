//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2011 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class ServiceKnownTypeAttributeTest
	{
		[Test]
		public void MethodName ()
		{
			var cd = ContractDescription.GetContract (typeof (IService));
			var types = cd.Operations.First ().KnownTypes;
			Assert.AreEqual (1, types.Count, "#1");
			Assert.AreEqual (typeof (Bar), types [0], "#2");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void MethodName2 ()
		{
			ContractDescription.GetContract (typeof (IService2));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void MethodName3 ()
		{
			ContractDescription.GetContract (typeof (IService3));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void MethodName4 ()
		{
			ContractDescription.GetContract (typeof (IService4));
		}

		public class Foo
		{
			public string S { get; set; }
		}

		public class Bar : Foo
		{
			public string T { get; set; }
		}

		class TypeProvider
		{
			static IEnumerable<Type> GetTypes (ICustomAttributeProvider provider)
			{
				yield return typeof (Bar);
			}

			// wrong return value
			static Type GetTypes2 (ICustomAttributeProvider provider)
			{
				return typeof (Bar);
			}

			// wrong argument
			static IEnumerable<Type> GetTypes3 ()
			{
				yield return typeof (Bar);
			}

			// non-static
			public IEnumerable<Type> GetTypes4 ()
			{
				yield return typeof (Bar);
			}
		}

		[ServiceKnownType ("GetTypes", typeof (TypeProvider))]
		[ServiceContract]
		public interface IService
		{
			[OperationContract]
			Foo X ();
		}

		[ServiceKnownType ("GetTypes2", typeof (TypeProvider))]
		[ServiceContract]
		public interface IService2
		{
			[OperationContract]
			Foo X ();
		}

		[ServiceKnownType ("GetTypes3", typeof (TypeProvider))]
		[ServiceContract]
		public interface IService3
		{
			[OperationContract]
			Foo X ();
		}

		[ServiceKnownType ("GetTypes4", typeof (TypeProvider))]
		[ServiceContract]
		public interface IService4
		{
			[OperationContract]
			Foo X ();
		}
	}
}
