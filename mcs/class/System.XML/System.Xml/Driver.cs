//
// Driver.cs
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2001 Jason Diamond  http://injektilo.org/
//

using System;
using System.Xml;

public class Driver
{
	public static void Main(string[] args)
	{
		XmlReader xmlReader = null;

		if (args.Length < 1)
		{
			xmlReader = new XmlTextReader(Console.In);
		}
		else
		{
			xmlReader = new XmlTextReader(args[0]);
		}

		while (xmlReader.Read())
		{
			Console.WriteLine("NodeType = {0}", xmlReader.NodeType);
			Console.WriteLine("  Name = {0}", xmlReader.Name);
			Console.WriteLine("  IsEmptyElement = {0}", xmlReader.IsEmptyElement);
			Console.WriteLine("  HasAttributes = {0}", xmlReader.HasAttributes);
			Console.WriteLine("  AttributeCount = {0}", xmlReader.AttributeCount);
			Console.WriteLine("  HasValue = {0}", xmlReader.HasValue);
			Console.WriteLine("  Value = {0}", xmlReader.Value);
			Console.WriteLine("  Depth = {0}", xmlReader.Depth);

			if (xmlReader.HasAttributes)
			{
				while (xmlReader.MoveToNextAttribute())
				{
					Console.WriteLine("    AttributeName = {0}", xmlReader.Name);
					Console.WriteLine("    AttributeValue = {0}", xmlReader.Value);

					while (xmlReader.ReadAttributeValue())
					{
						Console.WriteLine("      AttributeValueNodeType = {0}", xmlReader.NodeType);
						Console.WriteLine("      AttributeValueName = {0}", xmlReader.Name);
						Console.WriteLine("      AttributeValueValue = {0}", xmlReader.Value);
					}
				}
			}
		}
	}
}
