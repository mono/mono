//
// MonoTests.System.Web.Services.Description.BindingCollectionTest.cs
//
// Author:
//   Erik LeBel <eriklebel@yahoo.ca>
//
// (C) 2003 Erik LeBel
//

using NUnit.Framework;

using System;
using System.Web.Services.Description;

namespace MonoTests.System.Web.Services.Description
{
	[TestFixture]
	public class BindingCollectionTest
	{
		BindingCollection bc;

		[SetUp]
		public void InitializeBindingCollection ()
		{
			// workaround for internal constructor
			ServiceDescription desc = new ServiceDescription ();
			bc = desc.Bindings;
		}

		[Test]
		public void TestDefaultProperties()
		{
			Assert.IsNull (bc["hello"]);
			Assert.AreEqual (0, bc.Count);
		}
		
		[Test]
		public void TestAddBinding ()
		{
			const string bindingName = "testBinding";
			
			Binding b = new Binding ();
			b.Name = bindingName;
			
			bc.Add (b);

			Assert.AreEqual (1, bc.Count);
			Assert.AreEqual (b, bc[bindingName]);
		}
	}
}
