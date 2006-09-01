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
// Authors:
//
//	Copyright (C) 2006 Jordi Mas i Hernandez <jordimash@gmail.com>
//

using System;
using NUnit.Framework;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Workflow.ComponentModel;

namespace MonoTests.System.Workflow.ComponentModel
{


	[TestFixture]
	public class PropertyMetadataTest
	{
		private string val = "value";

		[Test]
		public void Constructor1 ()
		{
			PropertyMetadata pm = new PropertyMetadata ();

			Assert.AreEqual (null, pm.DefaultValue, "C1#1");
			Assert.AreEqual (null, pm.GetValueOverride, "C1#2");
			Assert.AreEqual (null, pm.SetValueOverride, "C1#3");
			Assert.AreEqual (DependencyPropertyOptions.Default, pm.Options, "C1#4");
		}

		[Test]
		public void Constructor2 ()
		{
			PropertyMetadata pm = new PropertyMetadata (DependencyPropertyOptions.Readonly);
			Assert.AreEqual (DependencyPropertyOptions.Readonly, pm.Options, "C2#1");
		}

		[Test]
		public void Constructor3 ()
		{
			Attribute[] attributes = new Attribute [2];
			attributes[0] = new ObsoleteAttribute ();
			attributes[1] = new ObsoleteAttribute ();
			PropertyMetadata pm = new PropertyMetadata (attributes);
			Assert.AreEqual (2, pm.GetAttributes().Length, "C3#1");
		}

		[Test]
		public void Constructor4 ()
		{
			Attribute[] attributes = new Attribute [2];
			attributes[0] = new ObsoleteAttribute ();
			attributes[1] = new ObsoleteAttribute ();
			PropertyMetadata pm = new PropertyMetadata (val, attributes);
			Assert.AreEqual (2, pm.GetAttributes().Length, "C4#1");
			Assert.AreEqual (val, pm.DefaultValue, "C4#2");
		}

		[Test]
		public void Constructor5 ()
		{
			PropertyMetadata pm = new PropertyMetadata (val, DependencyPropertyOptions.Readonly);
			Assert.AreEqual (DependencyPropertyOptions.Readonly, pm.Options, "C5#1");
			Assert.AreEqual (val, pm.DefaultValue, "C5#2");
		}

		[Test]
		public void Constructor6 ()
		{
			Attribute[] attributes = new Attribute [2];
			attributes[0] = new ObsoleteAttribute ();
			attributes[1] = new ObsoleteAttribute ();
			PropertyMetadata pm = new PropertyMetadata (DependencyPropertyOptions.Readonly, attributes);
			Assert.AreEqual (DependencyPropertyOptions.Readonly, pm.Options, "C6#1");
			Assert.AreEqual (2, pm.GetAttributes().Length, "C6#2");
		}

		[Test]
		public void Constructor7 ()
		{
			Attribute[] attributes = new Attribute [2];
			attributes[0] = new ObsoleteAttribute ();
			attributes[1] = new ObsoleteAttribute ();
			PropertyMetadata pm = new PropertyMetadata (val, DependencyPropertyOptions.Readonly, attributes);
			Assert.AreEqual (DependencyPropertyOptions.Readonly, pm.Options, "C7#1");
			Assert.AreEqual (2, pm.GetAttributes().Length, "C7#2");
			Assert.AreEqual (val, pm.DefaultValue, "C7#3");
		}

		[Test]
		public void Accessors ()
		{
			PropertyMetadata pm = new PropertyMetadata ();
			pm.Options = DependencyPropertyOptions.Readonly | DependencyPropertyOptions.NonSerialized;
			pm.DefaultValue = val;

			Assert.AreEqual (val, pm.DefaultValue, "C8#1");
			Assert.AreEqual (true, pm.IsNonSerialized, "C8#2");
			Assert.AreEqual (false, pm.IsMetaProperty, "C8#3");
			Assert.AreEqual (true, pm.IsReadOnly, "C8#4");

			pm.Options = DependencyPropertyOptions.Metadata;
			Assert.AreEqual (true, pm.IsMetaProperty, "C8#5");
			Assert.AreEqual (false, pm.IsNonSerialized, "C8#6");
			Assert.AreEqual (false, pm.IsReadOnly, "C8#7");
		}

		[Test]
		public void Attributes ()
		{
			Attribute[] attributes = new Attribute [3];
			Attribute[] attributes2;

			attributes[0] = new ObsoleteAttribute ();
			attributes[1] = new ObsoleteAttribute ();
			attributes[2] = new NonSerializedAttribute ();
			PropertyMetadata pm = new PropertyMetadata (attributes);
			attributes2 = pm.GetAttributes ();
			Assert.AreEqual (3, attributes2.Length, "C9#1");
			Assert.AreEqual (attributes[0], attributes2[0], "C9#2");
			Assert.AreEqual (attributes[1], attributes2[1], "C9#3");
			Assert.AreEqual (attributes[2], attributes2[2], "C9#4");

			attributes2 = pm.GetAttributes (typeof (NonSerializedAttribute));
			Assert.AreEqual (1, attributes2.Length, "C9#5");
			Assert.AreEqual (attributes[2], attributes2[0], "C9#6");

			attributes2 = pm.GetAttributes (typeof (ObsoleteAttribute));
			Assert.AreEqual (2, attributes2.Length, "C9#7");
			Assert.AreEqual (attributes[0], attributes2[0], "C9#8");
			Assert.AreEqual (attributes[1], attributes2[1], "C9#9");

			attributes2 = pm.GetAttributes (typeof (ParamArrayAttribute));
			Assert.AreEqual (0, attributes2.Length, "C9#10");



		}
	}
}

