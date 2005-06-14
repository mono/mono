// Compiler options: -doc:xml-038.xml
using System;
using System.Reflection;
using System.Xml;

/// <summary>
/// <see cref="AppDomain.AssemblyResolve" />
/// </summary>
public class Whatever {
  /// <summary>
  /// </summary>
  public static void Main() {
	foreach (MemberInfo mi in typeof (XmlDocument).FindMembers (
		MemberTypes.All,
		BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance,
		Type.FilterName,
		"NodeInserted"))
		Console.WriteLine (mi.GetType ());
  }
}

