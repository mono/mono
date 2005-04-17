// Compiler options: -doc:xml-037.xml
using System;
using System.Reflection;

/// <summary>
/// <see cref="AppDomain.AssemblyResolve" />
/// </summary>
public class Whatever {
  /// <summary>
  /// </summary>
  public static void Main() {
	foreach (MemberInfo mi in typeof (AppDomain).FindMembers (
		MemberTypes.All,
		BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance,
		Type.FilterName,
		"AssemblyResolve"))
		Console.WriteLine (mi.GetType ());
  }
}

