// CS1574: XML comment on `Test' has cref attribute `System.Xml.XmlDocument' that could not be resolved
// Line: 9
// Compiler options: -doc:dummy.xml -warnaserror -noconfig

using System;

/// <seealso cref="System.Xml.XmlDocument"/>
/// with /noconfig, it cannot be resolved.
public class Test
{
}
