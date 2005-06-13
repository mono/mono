// cs0419.cs: Ambiguous reference in cref attribute: 'System.String.Replace'. Assuming 'string.Replace(char, char)', but could have also matched other overloads including 'string.Replace(string, string)'.
// Line: 1
// Compiler options: -doc:dummy.xml -warn:3 -warnaserror
/// <summary>
/// Exposes <see cref="System.String.Replace"/> to XSLT
/// </summary>
public class Test {
}

