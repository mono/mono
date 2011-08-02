// CS0246: The type or namespace name `XmlDocument' could not be found. Are you missing a using directive or an assembly reference?
// Line: 7
// This is bug 55770

using System;
using System.Xml;
using Document = XmlDocument;

public class Test {
	public static void Main ()
	{
	}
}