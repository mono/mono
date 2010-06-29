using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;

namespace System.Runtime.DurableInstancing
{
	public sealed class InstanceValue
	{
		static InstanceValue ()
		{
			DeletedValue = new InstanceValue (null);
			DeletedValue.Value = DeletedValue; // recursion!
		}
		public static InstanceValue DeletedValue { get; private set; }

		public InstanceValue (object value)
			: this (value, InstanceValueOptions.None)
		{
		}
		
		public InstanceValue (object value, InstanceValueOptions options)
		{
			Value = value;
			Options = options;
		}
		
		public bool IsDeletedValue {
			get { return this == DeletedValue; }
		}
		
		public InstanceValueOptions Options { get; private set; }
		public object Value { get; private set; }
	}
}
