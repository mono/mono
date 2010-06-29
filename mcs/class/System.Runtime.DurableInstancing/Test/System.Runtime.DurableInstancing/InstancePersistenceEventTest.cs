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
	public class InstancePersistenceEventTest
	{
		[Test]
		public void Value ()
		{
			Assert.IsNotNull (FooEvent.Value, "#1");
		}
		
		class FooEvent : InstancePersistenceEvent<FooEvent>
		{
			public FooEvent ()
				: base (XName.Get ("foo"))
			{
			}
		}
	}
}
