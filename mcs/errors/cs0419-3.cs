// CS0419: Ambiguous reference in cref attribute `XmlDocument.Load'. Assuming `System.Xml.XmlDocument.Load(System.IO.Stream)' but other overloads including `System.Xml.XmlDocument.Load(string)' have also matched
// Line: 10
// Compiler options: -doc:dummy.xml -warnaserror -warn:4
// 
// NOTE: this error message is dependent on the order of members, so feel free to modify the message if is going not to match.

using System.Xml;

/// <summary>
/// <see cref="XmlDocument.Load" />
/// </summary>
public class EntryPoint
{
	static void Main () {
	}
}
