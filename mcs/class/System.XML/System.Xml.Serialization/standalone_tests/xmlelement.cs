using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

public class Test
{
	public static void Main()
	{
		Test t=new Test();
		t.Create("xmlelement.xml");
		t.Read("xmlelement.xml");
	}
	
	private void Create(string filename)
	{
		XmlSerializer ser=new XmlSerializer(typeof(XmlElement));
		XmlElement elem=new XmlDocument().CreateElement("MyElement", "ns");
		elem.InnerText="Hello, World!";
		TextWriter writer=new StreamWriter(filename);
		ser.Serialize(writer, elem);
		writer.Close();
	}

	private void Read(string filename)
	{
		XmlSerializer ser=new XmlSerializer(typeof(XmlElement));
		FileStream fs=new FileStream(filename, FileMode.Open);
		XmlElement elem;

		elem=(XmlElement)ser.Deserialize(fs);
		fs.Close();
		
		Console.WriteLine("Node name: "+elem.Name);
		Console.WriteLine("Node ns: "+elem.NamespaceURI);
		Console.WriteLine("Inner text: "+elem.InnerText);
	}
}
