//
// MonoTests.System.ComponentModel.ReferenceConverterTest
//
// Author:
//      Ivan N. Zlatev  <contact@i-nz.net>
//
// Copyright (C) 2008 Ivan N. Zlatev
//

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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

using NUnit.Framework;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class ReferenceConverterTest
	{

		class TestReferenceService : IReferenceService
		{
			private Dictionary<string, object> references;

			public TestReferenceService ()
			{
				references = new Dictionary<string, object> ();
			}

			public void AddReference (string name, object reference)
			{
				references[name] = reference;
			}

			public void ClearReferences ()
			{
				references.Clear ();
			}

			public IComponent GetComponent (object reference)
			{
				return null;
			}

			public string GetName (object reference)
			{
				foreach (KeyValuePair<string, object> entry in references) {
					if (entry.Value == reference)
						return entry.Key;
				}

				return null;
			}

			public object GetReference (string name)
			{
				if (!references.ContainsKey (name))
					return null;
				return references[name];
			}

			public object[] GetReferences()
			{
				object[] array = new object[references.Values.Count];
				references.Values.CopyTo (array, 0);
				return array;
			}

			public object[] GetReferences(Type baseType)
			{
				object[] references = GetReferences ();

				List<object> filtered = new List<object> ();
				foreach (object reference in references) {
					if (baseType.IsInstanceOfType(reference))
						filtered.Add (reference);
				}

				return filtered.ToArray();
			}
		}

		class TestTypeDescriptorContext : ITypeDescriptorContext
		{
			private IReferenceService reference_service = null;
			private IContainer container = null;

			public TestTypeDescriptorContext ()
			{
			}

			public TestTypeDescriptorContext (IReferenceService referenceService)
			{
				reference_service = referenceService;
			}


			public IContainer Container {
				get { return container; }
				set { container = value; }
			}

			public object Instance {
				get { return null; }
			}

			public PropertyDescriptor PropertyDescriptor {
				get { return null; }
			}

			public void OnComponentChanged ()
			{
			}

			public bool OnComponentChanging ()
			{
				return true;
			}

			public object GetService (Type serviceType)
			{
				if (serviceType == typeof (IReferenceService))
					return reference_service;
				return null;
			}
		}
		
		interface ITestInterface
		{
		}

		class TestComponent : Component
		{
		}

		[Test]
		public void CanConvertFrom ()
		{
			ReferenceConverter converter = new ReferenceConverter (typeof(ITestInterface));
			// without context
			Assert.IsFalse (converter.CanConvertFrom (null, typeof(string)), "#1");
			// with context
			Assert.IsTrue (converter.CanConvertFrom (new TestTypeDescriptorContext (), typeof(string)), "#2");
		}

#if !MOBILE
		[Test]
		public void ConvertFrom ()
		{
			ReferenceConverter converter = new ReferenceConverter (typeof(ITestInterface));
			string referenceName = "reference name";
			// no context
			Assert.IsNull (converter.ConvertFrom (null, null, referenceName), "#1");

			TestComponent component = new TestComponent();

			// context with IReferenceService
			TestReferenceService referenceService = new TestReferenceService ();
			referenceService.AddReference (referenceName, component);
			TestTypeDescriptorContext context = new TestTypeDescriptorContext (referenceService);
			Assert.AreSame (component, converter.ConvertFrom (context, null, referenceName), "#2");
			
			// context with Component without IReferenceService
			Container container = new Container ();
			container.Add (component, referenceName);
			context = new TestTypeDescriptorContext ();
			context.Container = container;
			Assert.AreSame (component, converter.ConvertFrom (context, null, referenceName), "#3");
		}

		[Test]
		public void ConvertTo ()
		{
			ReferenceConverter converter = new ReferenceConverter (typeof(ITestInterface));
			string referenceName = "reference name";

			Assert.AreEqual ("(none)", (string)converter.ConvertTo (null, null, null, typeof(string)), "#1");

			TestComponent component = new TestComponent();

			// no context
			Assert.AreEqual (String.Empty, (string)converter.ConvertTo (null, null, component, typeof(string)), "#2");

			// context with IReferenceService
			TestReferenceService referenceService = new TestReferenceService ();
			referenceService.AddReference (referenceName, component);
			TestTypeDescriptorContext context = new TestTypeDescriptorContext (referenceService);
			Assert.AreEqual (referenceName, (string)converter.ConvertTo (context, null, component, typeof(string)), "#3");
			
			// context with Component without IReferenceService
			Container container = new Container ();
			container.Add (component, referenceName);
			context = new TestTypeDescriptorContext ();
			context.Container = container;
			Assert.AreEqual (referenceName, (string)converter.ConvertTo (context, null, component, typeof(string)), "#4");
		}

#endif
		
		[Test]
		public void CanConvertTo ()
		{
			ReferenceConverter converter = new ReferenceConverter (typeof(ITestInterface));
			Assert.IsTrue (converter.CanConvertTo (new TestTypeDescriptorContext (), typeof(string)), "#1");
		}

		[Test]
		public void GetStandardValues ()
		{
			ReferenceConverter converter = new ReferenceConverter (typeof(TestComponent));

			TestComponent component1 = new TestComponent();
			TestComponent component2 = new TestComponent();
			TestReferenceService referenceService = new TestReferenceService ();
			referenceService.AddReference ("reference name 1", component1);
			referenceService.AddReference ("reference name 2", component2);
			ITypeDescriptorContext context = new TestTypeDescriptorContext (referenceService);

			TypeConverter.StandardValuesCollection values = converter.GetStandardValues (context);
			Assert.IsNotNull (values, "#1");
			// 2 components + 1 null value
			Assert.AreEqual (3, values.Count, "#2");
		}
	}
}
#endif
