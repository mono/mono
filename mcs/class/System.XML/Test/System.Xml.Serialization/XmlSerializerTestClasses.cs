//
// System.Xml.XmlSerializerTestClasses
//
// Author:
//   Erik LeBel <eriklebel@yahoo.ca>
//
// (C) 2003 Erik LeBel
//
// Classes to use in the testing of the XmlSerializer
//

using System;
using System.Collections;
using System.Xml.Serialization;

namespace MonoTests.System.Xml.TestClasses
{
	public enum SimpleEnumeration { FIRST, SECOND };
	
	public class SimpleClass
	{
		public string something = null;
	}

	public class StringCollection : CollectionBase
	{
		public void Add (String parameter) 
		{
			List.Insert (Count, parameter);
		}
			
		public String this [int index]
		{
			get
			{ 
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();
					
				return (String) List [index]; 
			}
			set { List [index] = value; }
		}
	}
	
	public class StringCollectionContainer
	{
		StringCollection messages = new StringCollection();
		
		public StringCollection Messages
		{
			get { return messages; }
		}
	}

	public class ArrayContainer
	{
		public object [] items = null;
	}
	
	public class ClassArrayContainer
	{
		public SimpleClass [] items = null;
	}
	
	[XmlRoot("simple")]
	public class SimpleClassWithXmlAttributes
	{
		[XmlAttribute("member")]
		public string something = null;
	}
}
