//
// Driver.cs
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2001 Jason Diamond  http://injektilo.org/
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
