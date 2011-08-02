//
// System.Xml.XmlSerializerTestClasses
//
// Authors:
//   Erik LeBel <eriklebel@yahoo.ca>
//   Hagit Yidov <hagity@mainsoft.com>
//
// (C) 2003 Erik LeBel
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//
// Classes to use in the testing of the XmlSerializer
//

using System;
using System.Collections;
#if NET_2_0
using System.Collections.Generic;
#endif
using System.ComponentModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MonoTests.System.Xml.TestClasses
{
	public enum SimpleEnumeration { FIRST, SECOND };

	[Flags]
	public enum EnumDefaultValue { e1 = 1, e2 = 2, e3 = 3 }
	public enum EnumDefaultValueNF { e1 = 1, e2 = 2, e3 = 3 }

	[Flags]
	public enum FlagEnum
	{
		[XmlEnum ("one")]
		e1 = 1,
		[XmlEnum ("two")]
		e2 = 2,
		[XmlEnum ("four")]
		e4 = 4
	}

	[Flags]
	[SoapType ("flagenum")]
	public enum FlagEnum_Encoded
	{
		[SoapEnum ("one")]
		e1 = 1,
		[SoapEnum ("two")]
		e2 = 2,
		[SoapEnum ("four")]
		e4 = 4
	}

	[Flags]
	public enum ZeroFlagEnum
	{
		[XmlEnum ("zero")]
		e0 = 0,
		[XmlEnum ("o<n>e")]
		e1 = 1,
		[XmlEnum ("tns:t<w>o")]
		e2 = 2,
		[XmlEnum ("four")]
		[XmlIgnore]
		e4 = 4
	}

	#region GenericsTestClasses

#if NET_2_0
	public class GenSimpleClass<T>
	{
		public T something = default (T);
	}

	public struct GenSimpleStruct<T>
	{
		public T something;
		public GenSimpleStruct (int dummy)
		{
			something = default (T);
		}
	}

	public class GenListClass<T>
	{
		public List<T> somelist = new List<T> ();
	}

	public class GenArrayClass<T>
	{
		public T[] arr = new T[3];
	}

	public class GenTwoClass<T1, T2>
	{
		public T1 something1 = default (T1);
		public T2 something2 = default (T2);
	}

	public class GenDerivedClass<T1, T2> : GenTwoClass<string, int>
	{
		public T1 another1 = default (T1);
		public T2 another2 = default (T2);
	}

	public class GenDerived2Class<T1, T2> : GenTwoClass<T1, T2>
	{
		public T1 another1 = default (T1);
		public T2 another2 = default (T2);
	}

	public class GenNestedClass<TO, TI>
	{
		public TO outer = default (TO);
		public class InnerClass<T>
		{
			public TI inner = default (TI);
			public T something = default (T);
		}
	}

	public struct GenComplexStruct<T1, T2>
	{
		public T1 something;
		public GenSimpleClass<T1> simpleclass;
		public GenSimpleStruct<T1> simplestruct;
		public GenListClass<T1> listclass;
		public GenArrayClass<T1> arrayclass;
		public GenTwoClass<T1, T2> twoclass;
		public GenDerivedClass<T1, T2> derivedclass;
		public GenDerived2Class<T1, T2> derived2;
		public GenNestedClass<T1, T2> nestedouter;
		public GenNestedClass<T1, T2>.InnerClass<T1> nestedinner;
		public GenComplexStruct (int dummy)
		{
			something = default (T1);
			simpleclass = new GenSimpleClass<T1> ();
			simplestruct = new GenSimpleStruct<T1> ();
			listclass = new GenListClass<T1> ();
			arrayclass = new GenArrayClass<T1> ();
			twoclass = new GenTwoClass<T1, T2> ();
			derivedclass = new GenDerivedClass<T1, T2> ();
			derived2 = new GenDerived2Class<T1, T2> ();
			nestedouter = new GenNestedClass<T1, T2> ();
			nestedinner = new GenNestedClass<T1, T2>.InnerClass<T1> ();
		}
	}
		
	public class WithNulls
	{
		[XmlElement (IsNullable=true)]
		public int? nint;
		
		[XmlElement (IsNullable=true)]
		public TestEnumWithNulls? nenum;
		
		[XmlElement (IsNullable=true)]
		public DateTime? ndate;
	}
	
	public enum TestEnumWithNulls
	{
		aa,
		bb
	}
	
#endif

	#endregion // GenericsTestClasses

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

		public String this[int index]
		{
			get
			{
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ();

				return (String) List[index];
			}
			set { List[index] = value; }
		}
	}

	public class StringCollectionContainer
	{
		StringCollection messages = new StringCollection ();

		public StringCollection Messages
		{
			get { return messages; }
		}
	}

	public class ArrayContainer
	{
		public object[] items = null;
	}

	public class ClassArrayContainer
	{
		public SimpleClass[] items = null;
	}

	[XmlRoot ("simple")]
	public class SimpleClassWithXmlAttributes
	{
		[XmlAttribute ("member")]
		public string something = null;
	}

	[XmlRoot ("field")]
	public class Field
	{
		[XmlAttribute ("flag1")]
		[DefaultValue (1)]
		public FlagEnum Flags1;

		[XmlAttribute ("flag2")]
		[DefaultValue (FlagEnum.e1)]
		public FlagEnum Flags2;

		[XmlAttribute ("flag3", Form = XmlSchemaForm.Qualified)]
		[DefaultValue (FlagEnum.e1 | FlagEnum.e2)]
		public FlagEnum Flags3;

		[XmlAttribute ("flag4")]
		public FlagEnum Flags4;

		[XmlAttribute ("modifiers")]
		public MapModifiers Modifiers;

		[XmlAttribute ("modifiers2", Form = XmlSchemaForm.Unqualified)]
		public MapModifiers Modifiers2;

		[XmlAttribute ("modifiers3")]
		[DefaultValue (0)]
		public MapModifiers Modifiers3;

		[XmlAttribute ("modifiers4", Form = XmlSchemaForm.Unqualified)]
		[DefaultValue (MapModifiers.Protected)]
		public MapModifiers Modifiers4;

		[XmlAttribute ("modifiers5", Form = XmlSchemaForm.Qualified)]
		[DefaultValue (MapModifiers.Public)]
		public MapModifiers Modifiers5;

		[XmlAttribute ("names")]
		public string[] Names;

		[XmlAttribute ("street")]
		public string Street;
	}

	[SoapType ("field", Namespace = "some:urn")]
	public class Field_Encoded
	{
		[SoapAttribute ("flag1")]
		[DefaultValue (FlagEnum_Encoded.e1)]
		public FlagEnum_Encoded Flags1;

		[SoapAttribute ("flag2")]
		[DefaultValue (FlagEnum_Encoded.e1)]
		public FlagEnum_Encoded Flags2;

		[SoapAttribute ("flag3")]
		[DefaultValue (FlagEnum_Encoded.e1 | FlagEnum_Encoded.e2)]
		public FlagEnum_Encoded Flags3;

		[SoapAttribute ("flag4")]
		public FlagEnum_Encoded Flags4;

		[SoapAttribute ("modifiers")]
		public MapModifiers Modifiers;

		[SoapAttribute ("modifiers2")]
		public MapModifiers Modifiers2;

		[SoapAttribute ("modifiers3")]
		[DefaultValue (MapModifiers.Public)]
		public MapModifiers Modifiers3;

		[SoapAttribute ("modifiers4")]
		[DefaultValue (MapModifiers.Protected)]
		public MapModifiers Modifiers4;

		[SoapAttribute ("modifiers5")]
		[DefaultValue (MapModifiers.Public)]
		public MapModifiers Modifiers5;

		public string[] Names;

		[SoapAttribute ("street")]
		public string Street;
	}

	[Flags]
	public enum MapModifiers
	{
		[XmlEnum ("public")]
		[SoapEnum ("PuBlIc")]
		Public = 0,
		[XmlEnum ("protected")]
		Protected = 1,
	}

	public class MyList : ArrayList
	{
		object container;

		// NOTE: MyList has no public constructor
		public MyList (object container)
			: base ()
		{
			this.container = container;
		}
	}

	public class Container
	{
		public MyList Items;

		public Container ()
		{
			Items = new MyList (this);
		}
	}

	public class Container2
	{
		public MyList Items;

		public Container2 ()
		{
		}

		public Container2 (bool b)
		{
			Items = new MyList (this);
		}
	}

	public class MyElem : XmlElement
	{
		public MyElem (XmlDocument doc)
			: base ("", "myelem", "", doc)
		{
			SetAttribute ("aa", "1");
		}

		[XmlAttribute]
		public int kk = 1;
	}

	public class MyDocument : XmlDocument
	{
		public MyDocument ()
		{
		}

		[XmlAttribute]
		public int kk = 1;
	}

	public class CDataContainer
	{
		public XmlCDataSection cdata;
	}

	public class NodeContainer
	{
		public XmlNode node;
	}

	public class Choices
	{
		[XmlElementAttribute ("ChoiceZero", typeof (string), IsNullable = false)]
		[XmlElementAttribute ("ChoiceOne", typeof (string), IsNullable = false)]
		[XmlElementAttribute ("ChoiceTwo", typeof (string), IsNullable = false)]
		[XmlChoiceIdentifier ("ItemType")]
		public string MyChoice;

		[XmlIgnore]
		public ItemChoiceType ItemType;
	}

	[XmlType (IncludeInSchema = false)]
	public enum ItemChoiceType
	{
		ChoiceZero,
		[XmlEnum ("ChoiceOne")]
		StrangeOne,
		ChoiceTwo,
	}

	public class WrongChoices
	{
		[XmlElementAttribute ("ChoiceZero", typeof (string), IsNullable = false)]
		[XmlElementAttribute ("StrangeOne", typeof (string), IsNullable = false)]
		[XmlElementAttribute ("ChoiceTwo", typeof (string), IsNullable = false)]
		[XmlChoiceIdentifier ("ItemType")]
		public string MyChoice;

		[XmlIgnore]
		public ItemChoiceType ItemType;
	}

	[XmlType ("Type with space")]
	public class TestSpace
	{
		[XmlElement (ElementName = "Element with space")]
		public int elem;

		[XmlAttribute (AttributeName = "Attribute with space")]
		public int attr;
	}

	[Serializable]
	public class ReadOnlyProperties
	{
		string[] strArr = new string[2] { "string1", "string2" };

		public string[] StrArr
		{
			get { return strArr; }
		}

		public string dat
		{
			get { return "fff"; }
		}
	}

	[XmlRoot ("root")]
	public class ListDefaults
	{
		public ListDefaults ()
		{
			ed = new SimpleClass ();
			str = "hola";
		}

		public ArrayList list2;

		public MyList list3;

		public string[] list4;

		[XmlElement ("e", typeof (SimpleClass))]
		public ArrayList list5;

		[DefaultValue (null)]
		public SimpleClass ed;

		[DefaultValue (null)]
		public string str;
	}

	public class clsPerson
	{
		public IList EmailAccounts;
	}

	public class ArrayClass
	{
		public object names = new object[] { "un", "dos" };
	}

	public class CompositeValueType
	{
		public void Init ()
		{
			Items = new object[] { 1, 2 };
			ItemsElementName = new ItemsChoiceType[] { ItemsChoiceType.In, ItemsChoiceType.Es };
		}

		[XmlElementAttribute ("Es", typeof (int))]
		[XmlElementAttribute ("In", typeof (int))]
		[XmlChoiceIdentifierAttribute ("ItemsElementName")]
		public object[] Items;

		[XmlElementAttribute ("ItemsElementName")]
		[XmlIgnoreAttribute ()]
		public ItemsChoiceType[] ItemsElementName;
	}

	public enum ItemsChoiceType
	{
		In, Es
	}

	public class ArrayAttributeWithType
	{
		[XmlAttribute (DataType = "anyURI")]
		public string[] at = new string[] { "a", "b" };

		[XmlAttribute (DataType = "base64Binary")]
		public byte[][] bin1 = new byte[][] { new byte[] { 1, 2 }, new byte[] { 1, 2 } };

		[XmlAttribute (DataType = "base64Binary")]
		public byte[] bin2 = new byte[] { 1, 2 };
	}

	public class ArrayAttributeWithWrongType
	{
		[XmlAttribute (DataType = "int")]
		public string[] at = new string[] { "a", "b" };
	}

	[XmlType ("Container")]
	public class EntityContainer
	{
		EntityCollection collection1;
		EntityCollection collection2;
		EntityCollection collection3 = new EntityCollection ("root");
		EntityCollection collection4 = new EntityCollection ("root");

		[XmlArray (IsNullable = true)]
		public EntityCollection Collection1
		{
			get { return collection1; }
			set { collection1 = value; collection1.Container = "assigned"; }
		}

		[XmlArray (IsNullable = false)]
		public EntityCollection Collection2
		{
			get { return collection2; }
			set { collection2 = value; collection2.Container = "assigned"; }
		}

		[XmlArray (IsNullable = true)]
		public EntityCollection Collection3
		{
			get { return collection3; }
			set { collection3 = value; collection3.Container = "assigned"; }
		}

		[XmlArray (IsNullable = false)]
		public EntityCollection Collection4
		{
			get { return collection4; }
			set { collection4 = value; collection4.Container = "assigned"; }
		}
	}

	[XmlType ("Container")]
	public class ArrayEntityContainer
	{
		Entity[] collection1;
		Entity[] collection2;
		Entity[] collection3 = new Entity[0];
		Entity[] collection4 = new Entity[0];

		[XmlArray (IsNullable = true)]
		public Entity[] Collection1
		{
			get { return collection1; }
			set { collection1 = value; }
		}

		[XmlArray (IsNullable = false)]
		public Entity[] Collection2
		{
			get { return collection2; }
			set { collection2 = value; }
		}

		[XmlArray (IsNullable = true)]
		public Entity[] Collection3
		{
			get { return collection3; }
			set { collection3 = value; }
		}

		[XmlArray (IsNullable = false)]
		public Entity[] Collection4
		{
			get { return collection4; }
			set { collection4 = value; }
		}
	}

	public class Entity
	{
		private string _name = string.Empty;
		private string _parent = null;

		[XmlAttribute]
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		[XmlIgnore]
		public string Parent
		{
			get { return _parent; }
			set { _parent = value; }
		}
	}

	public class EntityCollection : ArrayList
	{
		public string _container;

		public EntityCollection ()
		{
		}

		public EntityCollection (string c)
		{
			_container = c;
		}

		public string Container
		{
			get { return _container; }
			set { _container = value; }
		}

		public int Add (Entity value)
		{
			if (_container != null)
				value.Parent = _container;

			return base.Add (value);
		}

		public new Entity this[int index]
		{
			get { return (Entity) base[index]; }
			set { base[index] = value; }
		}
	}

	[XmlType ("Container")]
	public class ObjectWithReadonlyCollection
	{
		EntityCollection collection1 = new EntityCollection ("root");

		public EntityCollection Collection1
		{
			get { return collection1; }
		}
	}

	[XmlType ("Container")]
	public class ObjectWithReadonlyNulCollection
	{
		EntityCollection collection1;

		public EntityCollection Collection1
		{
			get { return collection1; }
		}
	}

	[XmlType ("Container")]
	public class ObjectWithReadonlyArray
	{
		Entity[] collection1 = new Entity[0];

		public Entity[] Collection1
		{
			get { return collection1; }
		}
	}

	[XmlInclude (typeof (SubclassTestSub))]
	public class SubclassTestBase
	{
	}

	public class SubclassTestSub : SubclassTestBase
	{
	}

	public class SubclassTestExtra
	{
	}

	public class SubclassTestContainer
	{
		[XmlElement ("a", typeof (SubclassTestBase))]
		[XmlElement ("b", typeof (SubclassTestExtra))]
		public object data;
	}

	public class DictionaryWithIndexer : DictionaryBase
	{
		public TimeSpan this[int index]
		{
			get { return TimeSpan.MinValue; }
		}

		public void Add (TimeSpan value)
		{
		}
	}

	[XmlRoot (Namespace = "some:urn")]
	[SoapTypeAttribute (Namespace = "another:urn")]
	public class PrimitiveTypesContainer
	{
		public PrimitiveTypesContainer ()
		{
			Number = 2004;
			Name = "some name";
			Index = (byte) 56;
			Password = new byte[] { 243, 15 };
			PathSeparatorCharacter = '/';
		}

		public int Number;
		public string Name;
		public byte Index;
		public byte[] Password;
		public char PathSeparatorCharacter;
	}

	public class TestSchemaForm1
	{
		public PrintTypeResponse p1;

		[XmlElement (Namespace = "urn:oo")]
		public PrintTypeResponse p2;
	}

	[XmlType (Namespace = "urn:testForm")]
	public class TestSchemaForm2
	{
		public PrintTypeResponse p1;

		[XmlElement (Namespace = "urn:oo")]
		public PrintTypeResponse p2;
	}

	[XmlType (Namespace = "urn:responseTypes")]
	public class PrintTypeResponse
	{
		[XmlElement (Form = XmlSchemaForm.Unqualified, IsNullable = true)]
		public OutputType result;
		public PrintTypeResponse intern;

		public void Init ()
		{
			result = new OutputType ();
			result.data = "data1";
			intern = new PrintTypeResponse ();
			intern.result = new OutputType ();
			intern.result.data = "data2";
		}
	}

	[XmlType (Namespace = "urn:responseTypes")]
	public class OutputType
	{

		[XmlElement (Form = XmlSchemaForm.Unqualified, IsNullable = true)]
		public string data;
	}

	[XmlRootAttribute ("testDefault", Namespace = "urn:myNS", IsNullable = false)]
	[SoapType ("testDefault", Namespace = "urn:myNS")]
	public class TestDefault
	{
		public string str;

		[DefaultValue ("Default Value")]
		public string strDefault = "Default Value";

		[DefaultValue (true)]
		public bool boolT = true;

		[DefaultValue (false)]
		public bool boolF = false;

		[DefaultValue (typeof (decimal), "10")]
		public decimal decimalval = 10m;

		[DefaultValue (FlagEnum.e1 | FlagEnum.e4)]
		public FlagEnum flag = (FlagEnum.e1 | FlagEnum.e4);

		[DefaultValue (FlagEnum_Encoded.e1 | FlagEnum_Encoded.e4)]
		public FlagEnum_Encoded flagencoded = (FlagEnum_Encoded.e1 | FlagEnum_Encoded.e4);
	}

	[XmlType ("optionalValueType", Namespace = "some:urn")]
	[XmlRootAttribute ("optionalValue", Namespace = "another:urn", IsNullable = false)]
	public class OptionalValueTypeContainer
	{
		[DefaultValue (FlagEnum.e1 | FlagEnum.e4)]
		public FlagEnum Attributes = FlagEnum.e1 | FlagEnum.e4;

		[DefaultValue (FlagEnum.e1)]
		public FlagEnum Flags = FlagEnum.e1;

		[XmlIgnore]
		[SoapIgnore]
		public bool FlagsSpecified;

		[DefaultValue (false)]
		public bool IsEmpty;

		[XmlIgnore]
		[SoapIgnore]
		public bool IsEmptySpecified
		{
			get { return _isEmptySpecified; }
			set { _isEmptySpecified = value; }
		}

		[DefaultValue (false)]
		public bool IsNull;

		private bool _isEmptySpecified;
	}

	public class Group
	{
		[SoapAttribute (Namespace = "http://www.cpandl.com")]
		public string GroupName;

		[SoapAttribute (DataType = "base64Binary")]
		public Byte[] GroupNumber;

		[SoapAttribute (DataType = "date", AttributeName = "CreationDate")]
		public DateTime Today;

		[SoapElement (DataType = "nonNegativeInteger", ElementName = "PosInt")]
		public string PostitiveInt;

		[SoapIgnore]
		public bool IgnoreThis;

		[DefaultValue (GroupType.B)]
		public GroupType Grouptype;
		public Vehicle MyVehicle;

		[SoapInclude (typeof (Car))]
		public Vehicle myCar (string licNumber)
		{
			Vehicle v;
			if (licNumber == string.Empty) {
				v = new Car ();
				v.licenseNumber = "!!!!!!";
			}
			else {
				v = new Car ();
				v.licenseNumber = licNumber;
			}
			return v;
		}
	}

	[SoapInclude (typeof (Car))]
	public abstract class Vehicle
	{
		public string licenseNumber;
		[SoapElement (DataType = "date")]
		public DateTime makeDate;
		[DefaultValue ("450")]
		public string weight;
	}

	public class Car : Vehicle
	{
	}

	public enum GroupType
	{
		[SoapEnum ("Small")]
		A,
		[SoapEnum ("Large")]
		B
	}

	public class ErrorneousGetSchema : IXmlSerializable
	{
		public XmlSchema GetSchema ()
		{
			throw new ApplicationException ("unexpected");
		}

		public void ReadXml (XmlReader reader)
		{
		}

		public void WriteXml (XmlWriter writer)
		{
		}

		// it should be serialized IF it is NOT IXmlSerializable.
		public string Whoa = "whoa";
	}

	[XmlRoot ("DefaultDateTimeContainer", Namespace = "urn:foo")]
	public class DefaultDateTimeContainer // bug #378696
	{
		public DateTime SimpleDateTime;

		[DefaultValue(typeof(DateTime), "2001-02-03T04:05:06")]
		public DateTime FancyDateTime;

		[DefaultValue (typeof (int), "123456")]
		public int Numeric;
	}

	public class XmlSerializableImplicitConvertible
	{
		public BaseClass B = new DerivedClass ();

		public class XmlSerializable : IXmlSerializable
		{
			public void WriteXml (XmlWriter writer)
			{
			}

			public void ReadXml (XmlReader reader)
			{
			}

			public XmlSchema GetSchema ()
			{
				return null;
			}
		}

		public class BaseClass
		{
			public static implicit operator XmlSerializable (BaseClass b)
			{
				return new XmlSerializable ();

			}

			public static implicit operator BaseClass (XmlSerializable x)
			{
				return new BaseClass ();
			}
		}

		public class DerivedClass : BaseClass
		{
		}
	}

	public class Bug704813Type
	{
		IEnumerable<string> foo = new List<string> ();
		public IEnumerable<string> Foo {
			get { return foo; }
		}
	}

	[XmlRoot("root")]
	public class ExplicitlyOrderedMembersType1
	{
		[XmlElement("child0", Order = 4)]
		public string Child0;

		[XmlElement("child", Order = 0)]
		public string Child1;

		[XmlElement("child", Order = 2)]
		public string Child2;
	}

	[XmlRoot("root")]
	public class ExplicitlyOrderedMembersType2
	{
		[XmlElement("child0", Order = 4)]
		public string Child0;

		[XmlElement("child")] // wrong. Needs to be Ordered as well.
		public string Child1;

		[XmlElement("child", Order = 2)]
		public string Child2;
	}

	[XmlRoot("root")]
	public class ExplicitlyOrderedMembersType3
	{
		[XmlElement("child0", Order = 1)] // it's between 0 and 2. After two "child" elements, child0 is not recognized as this member.
		public string Child0;

		[XmlElement("child", Order = 0)]
		public string Child1;

		[XmlElement("child", Order = 2)]
		public string Child2;
	}

}

