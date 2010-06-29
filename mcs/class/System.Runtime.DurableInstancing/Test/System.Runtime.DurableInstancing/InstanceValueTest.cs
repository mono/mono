using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.DurableInstancing;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;

namespace MonoTests.System.Runtime.DurableInstancing
{
	[TestFixture]
	public class InstanceValueTest
	{
		[Test]
		public void DeletedValue ()
		{
			Assert.AreEqual (InstanceValue.DeletedValue, InstanceValue.DeletedValue.Value, "#1"); // eek!
			Assert.AreEqual (InstanceValueOptions.None, InstanceValue.DeletedValue.Options, "#2");
		}
	}
}
