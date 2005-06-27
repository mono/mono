using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

// The XmlRootAttribute allows you to set an alterate name
// (PurchaseOrder) for the XML element and its namespace. By
// default, the XmlSerializer uses the class name. The attribute
// also allows you to set the XML namespace for the element. Lastly,
// the attribute sets the IsNullable property, which specifies whether
// the xsi:null attribute appears if the class instance is set to
// a null reference.
[XmlRootAttribute("PurchaseOrder", Namespace="http://cpandl.com",
		  IsNullable=false)]
public class PurchaseOrder
{
	public Address ShipTo;
	public string OrderDate;
	// The XmlArrayAttribute changes the XML element name
	// from the default of "OrderedItems" to "Items".
	[XmlArrayAttribute("Items")]
	public OrderedItem[] OrderedItems;
	public decimal SubTotal;
	public decimal ShipCost;
	public decimal TotalCost;
}

public class Address
{
	// The XmlAttribute instructs the XmlSerializer to serialize the Name
	// field as an XML attribute instead of an XML element (the default
	// behaviour).
	[XmlAttribute]
	public string Name;
	public string Line1;

	// Setting the IsNullable property to false instructs the
	// XmlSerializer that the XML attribute will not appear if
	// the City field is set to a null reference.
	[XmlElementAttribute(IsNullable=false)]
	public string City;
	public string State;
	public string Zip;
}

public class OrderedItem
{
	public string ItemName;
	public string Description;
	public decimal UnitPrice;
	public int Quantity;
	public decimal LineTotal;

	// Calculate is a custom method that calculates the price per item
	// and stores the value in a field.
	public void Calculate()
	{
		LineTotal=UnitPrice*Quantity;
	}
}

public class Test
{
	public static void Main()
	{
		// Read and write purchase orders.
		Test t=new Test();
		t.CreatePO("potest.xml");
		t.ReadPO("potest.xml");
	}

	private void CreatePO(string filename)
	{
		// Creates an instance of the XmlSerializer class;
		// specifies the type of object to serialize.
		XmlSerializer serializer=new XmlSerializer(typeof(PurchaseOrder));
		TextWriter writer=new StreamWriter(filename);
		PurchaseOrder po=new PurchaseOrder();

		// Creates an address to ship and bill to.
		Address billAddress=new Address();
		billAddress.Name="Teresa Atkinson";
		billAddress.Line1="1 Main St.";
		billAddress.City="AnyTown";
		billAddress.State="WA";
		billAddress.Zip="00000";
		// Sets ShipTo and BillTo to the same addressee.
		po.ShipTo=billAddress;
		po.OrderDate=System.DateTime.Now.ToLongDateString();

		// Creates an OrderedItem.
		OrderedItem i1=new OrderedItem();
		i1.ItemName="Widget S";
		i1.Description="Small widget";
		i1.UnitPrice=(decimal)5.23;
		i1.Quantity=3;
		i1.Calculate();

		// Inserts the item into the array.
		OrderedItem[] items={i1};
		po.OrderedItems=items;
		// Calculate the total cost.
		decimal subTotal=new decimal();
		foreach(OrderedItem oi in items)
		{
			subTotal+=oi.LineTotal;
		}
		po.SubTotal=subTotal;
		po.ShipCost=(decimal)12.51;
		po.TotalCost=po.SubTotal+po.ShipCost;
		// Serializes the purchase order, and closes the TextWriter.
		serializer.Serialize(writer, po);
		writer.Close();
	}

	protected void ReadPO(string filename)
	{
		// Creates an instance of the XmlSerializer class;
		// specifies the type of object to be deserialized.
		XmlSerializer serializer=new XmlSerializer(typeof(PurchaseOrder));
		// If the XML document has been altered with unknown
		// nodes or attributes, handles them with the
		// UnknownNode and UnknownAttribute events.
		serializer.UnknownNode+=new XmlNodeEventHandler(serializer_UnknownNode);
		serializer.UnknownAttribute+=new XmlAttributeEventHandler(serializer_UnknownAttribute);

		// A FileStream is needed to read the XML document.
		FileStream fs=new FileStream(filename, FileMode.Open);
		// Declares an object variable of the type to be deserialized.
		PurchaseOrder po;
		// Uses the Deserialize method to restore the object's state with
		// data from the XML document. */
		po=(PurchaseOrder)serializer.Deserialize(fs);
		fs.Close();
		
		// Reads the order date.
		Console.WriteLine("OrderDate: "+po.OrderDate);

		// Reads the shipping address.
		Address shipTo=po.ShipTo;
		ReadAddress(shipTo, "Ship To:");
		// Reads the list of ordered items.
		OrderedItem[] items=po.OrderedItems;
		Console.WriteLine("Items to be shipped:");
		foreach(OrderedItem oi in items)
		{
			Console.WriteLine("\t"+
					  oi.ItemName+"\t"+
					  oi.Description+"\t"+
					  oi.UnitPrice+"\t"+
					  oi.Quantity+"\t"+
					  oi.LineTotal);
		}
		// Reads the subtotal, shipping cost, and total cost.
		Console.WriteLine("\n\t\t\t\t\t Subtotal\t"+po.SubTotal+
				  "\n\t\t\t\t\t Shipping\t"+po.ShipCost+
				  "\n\t\t\t\t\t Total\t\t"+po.TotalCost);
	}

	protected void ReadAddress(Address a, string label)
	{
		// Reads the fields of the Address.
		Console.WriteLine(label);
		Console.Write("\t"+
			      a.Name+"\n\t"+
			      a.Line1+"\n\t"+
			      a.City+"\t"+
			      a.State+"\n\t"+
			      a.Zip+"\n");
	}

	protected void serializer_UnknownNode(object sender,
					      XmlNodeEventArgs e)
	{
		Console.WriteLine("Unknown Node:"+e.Name+"\t"+e.Text);
	}

	protected void serializer_UnknownAttribute(object sender,
						   XmlAttributeEventArgs e)
	{
		System.Xml.XmlAttribute attr=e.Attr;
		Console.WriteLine("Unknown attribute "+attr.Name+"='"+attr.Value+"'");
	}
}
