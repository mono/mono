using System;
using System.IO;
using System.Xml.Serialization;

public class PurchaseOrder
{
	public Address MyAddress;
}

public class Address
{
	public string FirstName;
}

public class Test
{
	public static void Main()
	{
		Test t=new Test();
		t.Create("complex.xml");
		t.Read("complex.xml");
	}
	
	private void Create(string filename)
	{
		XmlSerializer ser=new XmlSerializer(typeof(PurchaseOrder));
		PurchaseOrder po=new PurchaseOrder();
		Address addr=new Address();
		addr.FirstName="George";
		po.MyAddress=addr;

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

		Console.WriteLine("Name: "+po.MyAddress.FirstName);
	}
}
