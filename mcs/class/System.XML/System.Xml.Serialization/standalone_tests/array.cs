using System;
using System.IO;
using System.Xml.Serialization;

public class PurchaseOrder
{
	public Item[] ItemsOrdered;
}

public class Item
{
	public string ItemID;
	public decimal ItemPrice;

	/* Needed so it can be serialized */
	public Item()
	{}
	
	public Item(string id, decimal price) 
	{
		ItemID=id;
		ItemPrice=price;
	}
}

public class Test
{
	public static void Main()
	{
		Test t=new Test();
		t.Create("array.xml");
		t.Read("array.xml");
	}
	
	private void Create(string filename)
	{
		XmlSerializer ser=new XmlSerializer(typeof(PurchaseOrder));
		PurchaseOrder po=new PurchaseOrder();
		Item item1=new Item("aaa111", (decimal)34.22);
		Item item2=new Item("bbb222", (decimal)2.89);

		po.ItemsOrdered=new Item[2];
		po.ItemsOrdered[0]=item1;
		po.ItemsOrdered[1]=item2;

		TextWriter writer=new StreamWriter(filename);
		ser.Serialize(writer, po);
		writer.Close();
	}

	private void Read(string filename)
	{
		XmlSerializer ser=new XmlSerializer(typeof(PurchaseOrder));
		FileStream fs=new FileStream(filename, FileMode.Open);
		PurchaseOrder po;

		po=(PurchaseOrder)ser.Deserialize(fs);
		fs.Close();

		foreach(Item item in po.ItemsOrdered) 
		{
			Console.WriteLine("Item: "+item.ItemID);
			Console.WriteLine("Price: "+item.ItemPrice);
		}
	}
}
