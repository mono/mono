using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

public class Test
{
	public static void Main()
	{
		Test t=new Test();
		t.Create("xmlnode.xml");
		t.Read("xmlnode.xml");
	}
	
	private void Create(string filename)
	{
		XmlSerializer ser=new XmlSerializer(typeof(XmlNode));
		XmlNode node=new XmlDocument().CreateNode(XmlNodeType.Element, "MyNode", "ns");
		node.InnerText="Hello, World!";
		TextWriter writer=new StreamWriter(filename);
		ser.Serialize(writer, node);
		writer.Close();
	}

	private void Read(string filename)
	{
		XmlSerializer ser=new XmlSerializer(typeof(XmlNode));
		FileStream fs=new FileStream(filename, FileMode.Open);
		XmlNode node;

		node=(XmlNode)ser.Deserialize(fs);
		fs.Close();

		Console.WriteLine("Node type: "+node.NodeType);
		Console.WriteLine("Node name: "+node.Name);
		Console.WriteLine("Node ns: "+node.NamespaceURI);
		Console.WriteLine("Node inner text: "+node.InnerText);
	}
}
