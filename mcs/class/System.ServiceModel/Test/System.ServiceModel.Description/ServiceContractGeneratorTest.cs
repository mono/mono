//
// ServiceContractGeneratorTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://novell.com
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
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Description
{
	static class CollectionBaseEnumerable
	{
		public static T FirstOrDefault<T> (this CollectionBase coll, Func<T,bool> func)
		{
			foreach (T t in coll)
				if (func (t))
					return t;
			return default (T);
		}
	}

	[TestFixture]
	public class ServiceContractGeneratorTest
	{
		[Test]
		public void GenerateServiceContractType ()
		{
			var g = new ServiceContractGenerator ();
			g.GenerateServiceContractType (ContractDescription.GetContract (typeof (ITestService)));
			var cns = g.TargetCompileUnit.Namespaces [0];
			Assert.AreEqual (3, cns.Types.Count, "#1");
			var iface = cns.Types.FirstOrDefault<CodeTypeDeclaration> (t => t.Name == "ITestService");
			Assert.IsNotNull (iface.Members.FirstOrDefault<CodeTypeMember> (m => m.Name == "DoWork"), "#2-1");
			Assert.IsNotNull (iface.Members.FirstOrDefault<CodeTypeMember> (m => m.Name == "DoWork2"), "#2-2");
			var proxy = cns.Types.FirstOrDefault<CodeTypeDeclaration> (t => t.Name == "TestServiceClient");
			Assert.IsNotNull (proxy.Members.FirstOrDefault<CodeTypeMember> (m => m.Name == "DoWork"), "#3-1");
			Assert.IsNotNull (proxy.Members.FirstOrDefault<CodeTypeMember> (m => m.Name == "DoWork2"), "#3-2");
		}

		[ServiceContract]
		public interface ITestService
		{
			[OperationContract]
			void DoWork (string s);
			
			[OperationContract]
			string DoWork2 (string s1, string s2);
		}
	}
}

