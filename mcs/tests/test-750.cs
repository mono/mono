// Compiler options: -warnaserror -warn:4
using System;

public interface IA
{
	string this[int index] { get; set; }
}

public interface IB : IA
{
	int this[string name] { get; set; }
	new int this[int index] { get; set; }
	int this[string namespaceURI, string localName] {get; set;}
}

class M
{
	public static void Main ()
	{
	}
}