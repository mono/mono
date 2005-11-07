//
// MakeMap.cs: Builds a C map of constants defined on C# land
//
// Authors:
//  Miguel de Icaza (miguel@novell.com)
//  Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2003 Novell, Inc.
// (C) 2004 Jonathan Pryor
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
using System.Collections;
using System.IO;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

delegate void CreateFileHandler (string assembly_name, string file_prefix);
delegate void AssemblyAttributesHandler (Assembly assembly);
delegate void TypeHandler (Type t, string ns, string fn);
delegate void CloseFileHandler (string file_prefix);

class MakeMap {

	public static int Main (string [] args)
	{
		FileGenerator[] generators = new FileGenerator[]{
			new HeaderFileGenerator (),
			new SourceFileGenerator (),
			new ConvertFileGenerator (),
			new ConvertDocFileGenerator (),
			new MphPrototypeFileGenerator (),
		};

		MakeMap composite = new MakeMap ();
		foreach (FileGenerator g in generators) {
			composite.FileCreators += new CreateFileHandler (g.CreateFile);
			composite.AssemblyAttributesHandler += 
				new AssemblyAttributesHandler (g.WriteAssemblyAttributes);
			composite.TypeHandler += new TypeHandler (g.WriteType);
			composite.FileClosers += new CloseFileHandler (g.CloseFile);
		}

		return composite.Run (args);
	}

	event CreateFileHandler FileCreators;
	event AssemblyAttributesHandler AssemblyAttributesHandler;
	event TypeHandler TypeHandler;
	event CloseFileHandler FileClosers;

	int Run (string[] args)
	{
		if (args.Length != 2){
			Console.WriteLine ("Usage is: make-map assembly output");
			return 1;
		}
		
		string assembly_name = args[0];
		string output = args[1];

		FileCreators (assembly_name, output);

		Assembly assembly = Assembly.LoadFrom (assembly_name);
		AssemblyAttributesHandler (assembly);
		
		Type [] exported_types = assembly.GetTypes ();
		Array.Sort (exported_types, new TypeFullNameComparer ());
			
		foreach (Type t in exported_types) {
			string ns = t.Namespace;
			if (ns == null || !ns.StartsWith ("Mono"))
				continue;
			string fn = GetNativeName (t.FullName);
			ns = GetNativeName (ns);

			TypeHandler (t, ns, fn);
		}
		FileClosers (output);

		return 0;
	}

	private class TypeFullNameComparer : IComparer {
		public int Compare (object o1, object o2)
		{
			Type t1 = o1 as Type;
			Type t2 = o2 as Type;
			if (t1 == t2)
				return 0;
			if (t1 == null)
				return 1;
			if (t2 == null)
				return -1;
			return CultureInfo.InvariantCulture.CompareInfo.Compare (
					t1.FullName, t2.FullName, CompareOptions.Ordinal);
		}
	}

	private class _MemberNameComparer : IComparer {
		public int Compare (object o1, object o2)
		{
			MemberInfo m1 = o1 as MemberInfo;
			MemberInfo m2 = o2 as MemberInfo;
			if (m1 == m2)
				return 0;
			if (m1 == null)
				return 1;
			if (m2 == null)
				return -1;
			return CultureInfo.InvariantCulture.CompareInfo.Compare (
					m1.Name, m2.Name, CompareOptions.Ordinal);
		}
	}

	private class _OrdinalStringComparer : IComparer {
		public int Compare (object o1, object o2)
		{
			string s1 = o1 as string;
			string s2 = o2 as string;
			if (object.ReferenceEquals (s1, s2))
				return 0;
			if (s1 == null)
				return 1;
			if (s2 == null)
				return -1;
			return CultureInfo.InvariantCulture.CompareInfo.Compare (s1, s2, 
					CompareOptions.Ordinal);
		}
	}

	internal static IComparer MemberNameComparer = new _MemberNameComparer ();
	internal static IComparer OrdinalStringComparer = new _OrdinalStringComparer ();

	internal static string GetNativeName (string fn)
	{
		fn = fn.Replace ('.', '_');
		if (fn.StartsWith ("Mono_Unix_Native"))
			return fn.Replace ("Mono_Unix_Native", "Mono_Posix");
		return fn.Replace ("Mono_Unix", "Mono_Posix");
	}
}

abstract class FileGenerator {
	public abstract void CreateFile (string assembly_name, string file_prefix);

	public virtual void WriteAssemblyAttributes (Assembly assembly)
	{
	}

	public abstract void WriteType (Type t, string ns, string fn);
	public abstract void CloseFile (string file_prefix);

	protected static void WriteHeader (StreamWriter s, string assembly)
	{
		WriteHeader (s, assembly, false);
	}

	protected static void WriteHeader (StreamWriter s, string assembly, bool noConfig)
	{
		s.WriteLine (
			"/*\n" +
			" * This file was automatically generated by make-map from {0}.\n" +
			" *\n" +
			" * DO NOT MODIFY.\n" +
			" */",
			assembly);
		if (!noConfig) {
			s.WriteLine ("#include <config.h>");
		}
		s.WriteLine ();
	}

	protected static bool CanMapType (Type t, out bool bits)
	{
		object [] attributes = t.GetCustomAttributes (false);
		bool map = false;
		bits = false;
		
		foreach (object attr in attributes) {
			if (attr.GetType ().Name == "MapAttribute")
				map = true;
			if (attr.GetType ().Name == "FlagsAttribute")
				bits = true;
		}
		return map;
	}

	protected static string GetNativeType (Type t)
	{
		string ut = t.Name;
		if (t.IsEnum)
			ut = Enum.GetUnderlyingType (t).Name;
		Type et = t.GetElementType ();
		if (et != null && et.IsEnum)
			ut = Enum.GetUnderlyingType (et).Name;

		string type = null;

		switch (ut) {
			case "Boolean":       type = "int";             break;
			case "Byte":          type = "unsigned char";   break;
			case "SByte":         type = "signed char";     break;
			case "Int16":         type = "short";           break;
			case "UInt16":        type = "unsigned short";  break;
			case "Int32":         type = "int";             break;
			case "UInt32":        type = "unsigned int";    break;
			case "UInt32[]":      type = "unsigned int*";   break;
			case "Int64":         type = "gint64";          break;
			case "UInt64":        type = "guint64";         break;
			case "IntPtr":        type = "void*";           break;
			case "Byte[]":        type = "void*";           break;
			case "String":        type = "const char*";     break;
			case "StringBuilder": type = "char*";           break;
			case "Void":          type = "void";            break;
			case "HandleRef":     type = "void*";           break;
		}
		if (type != null)
			return string.Format ("{0}{1}", type,
					t.IsByRef ? "*" : "");
		return GetTypeName (t);
	}

	private static string GetTypeName (Type t)
	{
		if (t.Namespace.StartsWith ("System"))
			return "int /* warning: unknown mapping for type: " + t.Name + " */";
		string ts = "struct " +
			MakeMap.GetNativeName (t.FullName).Replace ("+", "_").Replace ("&", "*");
		return ts;
	}
}

class HeaderFileGenerator : FileGenerator {
	StreamWriter sh;

	public override void CreateFile (string assembly_name, string file_prefix)
	{
		sh = File.CreateText (file_prefix + ".h");
		WriteHeader (sh, assembly_name);
		sh.WriteLine ("#ifndef INC_Mono_Posix_" + file_prefix + "_H");
		sh.WriteLine ("#define INC_Mono_Posix_" + file_prefix + "_H\n");
		sh.WriteLine ("#include <glib/gtypes.h>\n");
		sh.WriteLine ("G_BEGIN_DECLS\n");
	}

	public override void WriteType (Type t, string ns, string fn)
	{
		bool bits;
		if (!CanMapType (t, out bits))
			return;
		string etype = GetNativeType (t);

		WriteLiteralValues (sh, t, fn);
		sh.WriteLine ("int {1}_From{2} ({0} x, {0} *r);", etype, ns, t.Name);
		sh.WriteLine ("int {1}_To{2} ({0} x, {0} *r);", etype, ns, t.Name);
		sh.WriteLine ();
	}

	static void WriteLiteralValues (StreamWriter sh, Type t, string n)
	{
		object inst = Activator.CreateInstance (t);
		FieldInfo[] fields = t.GetFields ();
		Array.Sort (fields, MakeMap.MemberNameComparer);
		foreach (FieldInfo fi in fields) {
			if (!fi.IsLiteral)
				continue;
			sh.WriteLine ("#define {0}_{1} 0x{2:x}", n, fi.Name, fi.GetValue (inst));
		}
	}

	public override void CloseFile (string file_prefix)
	{
		sh.WriteLine ("G_END_DECLS\n");
		sh.WriteLine ("#endif /* ndef INC_Mono_Posix_" + file_prefix + "_H */\n");
		sh.Close ();
	}
}

class SourceFileGenerator : FileGenerator {
	StreamWriter sc;

	public override void CreateFile (string assembly_name, string file_prefix)
	{
		sc = File.CreateText (file_prefix + ".c");
		WriteHeader (sc, assembly_name);

		if (file_prefix.IndexOf ("/") != -1)
			file_prefix = file_prefix.Substring (file_prefix.IndexOf ("/") + 1);
		sc.WriteLine ("#include \"{0}.h\"", file_prefix);
		sc.WriteLine ();
	}

	public override void WriteAssemblyAttributes (Assembly assembly)
	{
		object [] x = assembly.GetCustomAttributes (false);
		Console.WriteLine ("Got: " + x.Length);
		foreach (object aattr in assembly.GetCustomAttributes (false)) {
			Console.WriteLine ("Got: " + aattr.GetType ().Name);
			if (aattr.GetType ().Name == "HeaderAttribute"){
				WriteDefines (sc, aattr);
				WriteIncludes (sc, aattr);
			}
		}
	}

	static void WriteDefines (TextWriter writer, object o)
	{
		PropertyInfo prop = o.GetType ().GetProperty ("Defines");
		if (prop == null)
			throw new Exception ("Cannot find 'Defines' property");

		MethodInfo method = prop.GetGetMethod ();
		string [] defines = method.Invoke (o, null).ToString ().Split (',');
		foreach (string def in defines) {
			writer.WriteLine ("#ifndef {0}", def);
			writer.WriteLine ("#define {0}", def);
			writer.WriteLine ("#endif /* ndef {0} */", def);
		}
	}

	static void WriteIncludes (TextWriter writer, object o)
	{
		PropertyInfo prop = o.GetType ().GetProperty ("Includes");
		if (prop == null)
			throw new Exception ("Cannot find 'Includes' property");

		MethodInfo method = prop.GetGetMethod ();
		string [] includes = method.Invoke (o, null).ToString ().Split (',');;
		foreach (string inc in includes){
			if (inc.Length > 3 && 
					string.CompareOrdinal (inc, 0, "ah:", 0, 3) == 0) {
				string i = inc.Substring (3);
				writer.WriteLine ("#ifdef HAVE_" + (i.ToUpper ().Replace ("/", "_").Replace (".", "_")));
				writer.WriteLine ("#include <{0}>", i);
				writer.WriteLine ("#endif");
			} else 
				writer.WriteLine ("#include <{0}>", inc);
		}
		writer.WriteLine ();
	}

	public override void WriteType (Type t, string ns, string fn)
	{
		bool bits;
		if (!CanMapType (t, out bits))
			return;
		string etype = GetNativeType (t);

		WriteFromManagedType (t, ns, fn, etype, bits);
		WriteToManagedType (t, ns, fn, etype, bits);
	}

	private void WriteFromManagedType (Type t, string ns, string fn, string etype, bool bits)
	{
		sc.WriteLine ("int {1}_From{2} ({0} x, {0} *r)", etype, ns, t.Name);
		sc.WriteLine ("{");
		sc.WriteLine ("\t*r = 0;");
		// For many values, 0 is a valid value, but doesn't have it's own symbol.
		// Examples: Error (0 means "no error"), WaitOptions (0 means "no options").
		// Make 0 valid for all conversions.
		sc.WriteLine ("\tif (x == 0)\n\t\treturn 0;");
		FieldInfo[] fields = t.GetFields ();
		Array.Sort (fields, MakeMap.MemberNameComparer);
		foreach (FieldInfo fi in fields) {
			if (!fi.IsLiteral)
				continue;
			if (Attribute.GetCustomAttribute (fi, 
				typeof(ObsoleteAttribute), false) != null) {
				sc.WriteLine ("\t/* {0}_{1} is obsolete; ignoring */", fn, fi.Name);
				continue;
			}
			if (bits)
				// properly handle case where [Flags] enumeration has helper
				// synonyms.  e.g. DEFFILEMODE and ACCESSPERMS for mode_t.
				sc.WriteLine ("\tif ((x & {0}_{1}) == {0}_{1})", fn, fi.Name);
			else
				sc.WriteLine ("\tif (x == {0}_{1})", fn, fi.Name);
			sc.WriteLine ("#ifdef {0}", fi.Name);
			if (bits)
				sc.WriteLine ("\t\t*r |= {1};", fn, fi.Name);
			else
				sc.WriteLine ("\t\t{{*r = {1}; return 0;}}", fn, fi.Name);
			sc.WriteLine ("#else /* def {0} */\n\t\t{{errno = EINVAL; return -1;}}", fi.Name);
			sc.WriteLine ("#endif /* ndef {0} */", fi.Name);
		}
		if (bits)
			sc.WriteLine ("\treturn 0;");
		else
			sc.WriteLine ("\terrno = EINVAL; return -1;"); // return error if not matched
		sc.WriteLine ("}\n");
	}

	private void WriteToManagedType (Type t, string ns, string fn, string etype, bool bits)
	{
		sc.WriteLine ("int {1}_To{2} ({0} x, {0} *r)", etype, ns, t.Name);
		sc.WriteLine ("{");
		sc.WriteLine ("\t*r = 0;", etype);
		// For many values, 0 is a valid value, but doesn't have it's own symbol.
		// Examples: Error (0 means "no error"), WaitOptions (0 means "no options").
		// Make 0 valid for all conversions.
		sc.WriteLine ("\tif (x == 0)\n\t\treturn 0;");
		FieldInfo[] fields = t.GetFields ();
		Array.Sort (fields, MakeMap.MemberNameComparer);
		foreach (FieldInfo fi in fields) {
			if (!fi.IsLiteral)
				continue;
			sc.WriteLine ("#ifdef {0}", fi.Name);
			if (bits)
				// properly handle case where [Flags] enumeration has helper
				// synonyms.  e.g. DEFFILEMODE and ACCESSPERMS for mode_t.
				sc.WriteLine ("\tif ((x & {1}) == {1})\n\t\t*r |= {0}_{1};", fn, fi.Name);
			else
				sc.WriteLine ("\tif (x == {1})\n\t\t{{*r = {0}_{1}; return 0;}}", fn, fi.Name);
			sc.WriteLine ("#endif /* ndef {0} */", fi.Name);
		}
		if (bits)
			sc.WriteLine ("\treturn 0;");
		else
			sc.WriteLine ("\terrno = EINVAL; return -1;");
		sc.WriteLine ("}\n");
	}

	public override void CloseFile (string file_prefix)
	{
		sc.Close ();
	}
}

class ConvertFileGenerator : FileGenerator {
	StreamWriter scs;

	public override void CreateFile (string assembly_name, string file_prefix)
	{
		scs = File.CreateText (file_prefix + ".cs");
		WriteHeader (scs, assembly_name, true);
		scs.WriteLine ("using System;");
		scs.WriteLine ("using System.Runtime.InteropServices;");
		scs.WriteLine ("using Mono.Unix.Native;\n");
		scs.WriteLine ("namespace Mono.Unix.Native {\n");
		scs.WriteLine ("\t[CLSCompliant (false)]");
		scs.WriteLine ("\tpublic sealed /* static */ partial class NativeConvert");
		scs.WriteLine ("\t{");
		scs.WriteLine ("\t\tprivate NativeConvert () {}\n");
		scs.WriteLine ("\t\tprivate const string LIB = \"MonoPosixHelper\";\n");
		scs.WriteLine ("\t\tprivate static void ThrowArgumentException (object value)");
		scs.WriteLine ("\t\t{");
		scs.WriteLine ("\t\t\tthrow new ArgumentOutOfRangeException (\"value\", value,");
		scs.WriteLine ("\t\t\t\tLocale.GetText (\"Current platform doesn't support this value.\"));");
		scs.WriteLine ("\t\t}\n");
	}

	public override void WriteType (Type t, string ns, string fn)
	{
		bool bits;
		if (!CanMapType (t, out bits))
			return;

		string mtype = Enum.GetUnderlyingType(t).Name;
		ObsoleteAttribute oa = (ObsoleteAttribute) Attribute.GetCustomAttribute (t, 
					typeof(ObsoleteAttribute), false);
		string obsolete = "";
		if (oa != null) {
			obsolete = "[Obsolete (\"" + oa.Message + "\")]\n\t\t";
		}
		scs.WriteLine ("\t\t[DllImport (LIB, " + 
			"EntryPoint=\"{0}_From{1}\")]\n" +
			"\t\tprivate static extern int From{1} ({1} value, out {2} rval);\n",
			ns, t.Name, mtype);
		scs.WriteLine ("\t\t{3}public static bool TryFrom{1} ({1} value, out {2} rval)\n" +
			"\t\t{{\n" +
			"\t\t\treturn From{1} (value, out rval) == 0;\n" +
			"\t\t}}\n", ns, t.Name, mtype, obsolete);
		scs.WriteLine ("\t\t{2}public static {0} From{1} ({1} value)", mtype, t.Name, obsolete);
		scs.WriteLine ("\t\t{");
		scs.WriteLine ("\t\t\t{0} rval;", mtype);
		scs.WriteLine ("\t\t\tif (From{0} (value, out rval) == -1)\n" + 
				"\t\t\t\tThrowArgumentException (value);", t.Name);
		scs.WriteLine ("\t\t\treturn rval;");
		scs.WriteLine ("\t\t}\n");
		scs.WriteLine ("\t\t[DllImport (LIB, " + 
			"EntryPoint=\"{0}_To{1}\")]\n" +
			"\t\tprivate static extern int To{1} ({2} value, out {1} rval);\n",
			ns, t.Name, mtype);
		scs.WriteLine ("\t\t{2}public static bool TryTo{1} ({0} value, out {1} rval)\n" +
			"\t\t{{\n" +
			"\t\t\treturn To{1} (value, out rval) == 0;\n" +
			"\t\t}}\n", mtype, t.Name, obsolete);
		scs.WriteLine ("\t\t{2}public static {1} To{1} ({0} value)", mtype, t.Name, obsolete);
		scs.WriteLine ("\t\t{");
		scs.WriteLine ("\t\t\t{0} rval;", t.Name);
		scs.WriteLine ("\t\t\tif (To{0} (value, out rval) == -1)\n" + 
				"\t\t\t\tThrowArgumentException (value);", t.Name);
		scs.WriteLine ("\t\t\treturn rval;");
		scs.WriteLine ("\t\t}\n");
	}

	public override void CloseFile (string file_prefix)
	{
		scs.WriteLine ("\t}");
		scs.WriteLine ("}\n");
		scs.Close ();
	}
}

class ConvertDocFileGenerator : FileGenerator {
	StreamWriter scs;

	public override void CreateFile (string assembly_name, string file_prefix)
	{
		scs = File.CreateText (file_prefix + ".xml");
		scs.WriteLine ("    <!-- BEGIN GENERATED CONTENT");
		WriteHeader (scs, assembly_name, true);
		scs.WriteLine ("      -->");
	}

	public override void WriteType (Type t, string ns, string fn)
	{
		bool bits;
		if (!CanMapType (t, out bits))
			return;

		string type = GetCSharpType (t);
		string mtype = Enum.GetUnderlyingType(t).FullName;
		string member = t.Name;
		string ftype = t.FullName;

		string to_returns = "";
		string to_remarks = "";
		string to_exception = "";

		if (bits) {
			to_returns = "<returns>An approximation of the equivalent managed value.</returns>";
			to_remarks = @"<para>The current conversion functions are unable to determine
        if a value in a <c>[Flags]</c>-marked enumeration <i>does not</i> 
        exist on the current platform.  As such, if <paramref name=""value"" /> 
        contains a flag value which the current platform doesn't support, it 
        will not be present in the managed value returned.</para>
        <para>This should only be a problem if <paramref name=""value"" /> 
        <i>was not</i> previously returned by 
        <see cref=""M:Mono.Unix.Native.NativeConvert.From" + member + "\" />.</para>\n";
		}
		else {
			to_returns = "<returns>The equivalent managed value.</returns>";
			to_exception = @"
        <exception cref=""T:System.ArgumentOutOfRangeException"">
          <paramref name=""value"" /> has no equivalent managed value.
        </exception>
";
		}
		scs.WriteLine (@"
    <Member MemberName=""TryFrom{1}"">
      <MemberSignature Language=""C#"" Value=""public static bool TryFrom{1} ({0} value, out {2} rval);"" />
      <MemberType>Method</MemberType>
      <ReturnValue>
        <ReturnType>System.Boolean</ReturnType>
      </ReturnValue>
      <Parameters>
        <Parameter Name=""value"" Type=""{0}"" />
        <Parameter Name=""rval"" Type=""{3}&amp;"" RefType=""out"" />
      </Parameters>
      <Docs>
        <param name=""value"">The managed value to convert.</param>
        <param name=""rval"">The OS-specific equivalent value.</param>
        <summary>Converts a <see cref=""T:{0}"" /> 
          enumeration value to an OS-specific value.</summary>
        <returns><see langword=""true"" /> if the conversion was successful; 
        otherwise, <see langword=""false"" />.</returns>
        <remarks><para>This is an exception-safe alternative to 
        <see cref=""M:Mono.Unix.Native.NativeConvert.From{1}"" />.</para>
        <para>If successful, this method stores the OS-specific equivalent
        value of <paramref name=""value"" /> into <paramref name=""rval"" />.
        Otherwise, <paramref name=""rval"" /> will contain <c>0</c>.</para>
        </remarks>
        <altmember cref=""M:Mono.Unix.Native.NativeConvert.From{1}"" />
        <altmember cref=""M:Mono.Unix.Native.NativeConvert.To{1}"" />
        <altmember cref=""M:Mono.Unix.Native.NativeConvert.TryTo{1}"" />
      </Docs>
    </Member>
    <Member MemberName=""From{1}"">
      <MemberSignature Language=""C#"" Value=""public static {2} From{1} ({0} value);"" />
      <MemberType>Method</MemberType>
      <ReturnValue>
        <ReturnType>{3}</ReturnType>
      </ReturnValue>
      <Parameters>
        <Parameter Name=""value"" Type=""{0}"" />
      </Parameters>
      <Docs>
        <param name=""value"">The managed value to convert.</param>
        <summary>Converts a <see cref=""T:{0}"" /> 
          to an OS-specific value.</summary>
        <returns>The equivalent OS-specific value.</returns>
        <exception cref=""T:System.ArgumentOutOfRangeException"">
          <paramref name=""value"" /> has no equivalent OS-specific value.
        </exception>
        <remarks></remarks>
        <altmember cref=""M:Mono.Unix.Native.NativeConvert.To{1}"" />
        <altmember cref=""M:Mono.Unix.Native.NativeConvert.TryFrom{1}"" />
        <altmember cref=""M:Mono.Unix.Native.NativeConvert.TryTo{1}"" />
      </Docs>
    </Member>
    <Member MemberName=""TryTo{1}"">
      <MemberSignature Language=""C#"" Value=""public static bool TryTo{1} ({2} value, out {0} rval);"" />
      <MemberType>Method</MemberType>
      <ReturnValue>
        <ReturnType>System.Boolean</ReturnType>
      </ReturnValue>
      <Parameters>
        <Parameter Name=""value"" Type=""{3}"" />
        <Parameter Name=""rval"" Type=""{0}&amp;"" RefType=""out"" />
      </Parameters>
      <Docs>
        <param name=""value"">The OS-specific value to convert.</param>
        <param name=""rval"">The managed equivalent value</param>
        <summary>Converts an OS-specific value to a 
          <see cref=""T:{0}"" />.</summary>
        <returns><see langword=""true"" /> if the conversion was successful; 
        otherwise, <see langword=""false"" />.</returns>
        <remarks><para>This is an exception-safe alternative to 
        <see cref=""M:Mono.Unix.Native.NativeConvert.To{1}"" />.</para>
        <para>If successful, this method stores the managed equivalent
        value of <paramref name=""value"" /> into <paramref name=""rval"" />.
        Otherwise, <paramref name=""rval"" /> will contain a <c>0</c>
        cast to a <see cref=""T:{0}"" />.</para>
        " + to_remarks + 
@"        </remarks>
        <altmember cref=""M:Mono.Unix.Native.NativeConvert.From{1}"" />
        <altmember cref=""M:Mono.Unix.Native.NativeConvert.To{1}"" />
        <altmember cref=""M:Mono.Unix.Native.NativeConvert.TryFrom{1}"" />
      </Docs>
    </Member>
    <Member MemberName=""To{1}"">
      <MemberSignature Language=""C#"" Value=""public static {0} To{1} ({2} value);"" />
      <MemberType>Method</MemberType>
      <ReturnValue>
        <ReturnType>{0}</ReturnType>
      </ReturnValue>
      <Parameters>
        <Parameter Name=""value"" Type=""{3}"" />
      </Parameters>
      <Docs>
        <param name=""value"">The OS-specific value to convert.</param>
        <summary>Converts an OS-specific value to a 
          <see cref=""T:{0}"" />.</summary>
					" + to_returns + "\n" + 
			to_exception + 
@"        <remarks>
        " + to_remarks + @"
        </remarks>
        <altmember cref=""M:Mono.Unix.Native.NativeConvert.From{1}"" />
        <altmember cref=""M:Mono.Unix.Native.NativeConvert.TryFrom{1}"" />
        <altmember cref=""M:Mono.Unix.Native.NativeConvert.TryTo{1}"" />
      </Docs>
    </Member>
", ftype, member, type, mtype
		);
	}

	private string GetCSharpType (Type t)
	{
		string ut = t.Name;
		if (t.IsEnum)
			ut = Enum.GetUnderlyingType (t).Name;
		Type et = t.GetElementType ();
		if (et != null && et.IsEnum)
			ut = Enum.GetUnderlyingType (et).Name;

		string type = null;

		switch (ut) {
			case "Boolean":       type = "bool";    break;
			case "Byte":          type = "byte";    break;
			case "SByte":         type = "sbyte";   break;
			case "Int16":         type = "short";   break;
			case "UInt16":        type = "ushort";  break;
			case "Int32":         type = "int";     break;
			case "UInt32":        type = "uint";    break;
			case "Int64":         type = "long";    break;
			case "UInt64":        type = "ulong";   break;
		}

		return type;
	}

	public override void CloseFile (string file_prefix)
	{
		scs.WriteLine ("    <!-- END GENERATED CONTENT -->");
		scs.Close ();
	}
}

class MphPrototypeFileGenerator : FileGenerator {
	StreamWriter icall;
	Hashtable methods = new Hashtable ();
	Hashtable structs = new Hashtable ();

	public override void CreateFile (string assembly_name, string file_prefix)
	{
		icall = File.CreateText (file_prefix + "-icalls.h");
		WriteHeader (icall, assembly_name);
		icall.WriteLine ("#ifndef INC_Mono_Posix_" + file_prefix + "_ICALLS_H");
		icall.WriteLine ("#define INC_Mono_Posix_" + file_prefix + "_ICALLS_H\n");
		icall.WriteLine ("#include <glib/gtypes.h>\n");
		icall.WriteLine ("G_BEGIN_DECLS\n");

		// Kill warning about unused method
		DumpTypeInfo (null);
	}

	public override void WriteType (Type t, string ns, string fn)
	{
		BindingFlags bf = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		foreach (MethodInfo m in t.GetMethods (bf)) {
			if ((m.Attributes & MethodAttributes.PinvokeImpl) == 0)
				continue;
			DllImportAttribute dia = GetDllImportInfo (m);
			if (dia == null) {
				Console.WriteLine ("Unable to emit native prototype for P/Invoke " + 
						"method: {0}", m);
				continue;
			}
			// we shouldn't declare prototypes for POSIX, etc. functions.
			if (dia.Value != "MonoPosixHelper" || IsOnExcludeList (dia.EntryPoint))
				continue;
			methods [dia.EntryPoint] = m;
			RecordStructs (m);
		}
	}

	private static DllImportAttribute GetDllImportInfo (MethodInfo method)
	{
		// .NET 2.0 synthesizes pseudo-attributes such as DllImport
		DllImportAttribute dia = (DllImportAttribute) Attribute.GetCustomAttribute (method, 
					typeof(DllImportAttribute), false);
		if (dia != null)
			return dia;

		// We're not on .NET 2.0; assume we're on Mono and use some internal
		// methods...
		Type MonoMethod = Type.GetType ("System.Reflection.MonoMethod", false);
		if (MonoMethod == null) {
			Console.WriteLine ("cannot find MonoMethod");
			return null;
		}
		MethodInfo GetDllImportAttribute = 
			MonoMethod.GetMethod ("GetDllImportAttribute", 
					BindingFlags.Static | BindingFlags.NonPublic);
		if (GetDllImportAttribute == null) {
			Console.WriteLine ("cannot find GetDllImportAttribute");
			return null;
		}
		IntPtr mhandle = method.MethodHandle.Value;
		return (DllImportAttribute) GetDllImportAttribute.Invoke (null, 
				new object[]{mhandle});
	}

	private static string[] ExcludeList = new string[]{
		"Mono_Posix_Stdlib_snprintf",
	};

	private bool IsOnExcludeList (string method)
	{
		int idx = Array.BinarySearch (ExcludeList, method);
		return (idx >= 0 && idx < ExcludeList.Length) ? true : false;
	}

	private void RecordStructs (MethodInfo method)
	{
		ParameterInfo[] parameters = method.GetParameters ();
		foreach (ParameterInfo pi in parameters) {
			string s = GetNativeType (pi.ParameterType);
			if (s.StartsWith ("struct"))
				structs [s] = s;
		}
	}

	public override void CloseFile (string file_prefix)
	{
		icall.WriteLine ("/*\n * Structure Declarations\n */");
		foreach (string s in Sort (structs.Keys))
			icall.WriteLine ("{0};", s.Replace ("*", ""));

		icall.WriteLine ();

		icall.WriteLine ("/*\n * Function Declarations\n */");
		foreach (string method in Sort (methods.Keys)) {
			WriteMethodDeclaration ((MethodInfo) methods [method], method);
		}

		icall.WriteLine ("\nG_END_DECLS\n");
		icall.WriteLine ("#endif /* ndef INC_Mono_Posix_" + file_prefix + "_ICALLS_H */\n");
		icall.Close ();
	}

	private static IEnumerable Sort (ICollection c)
	{
		ArrayList al = new ArrayList (c);
		al.Sort (MakeMap.OrdinalStringComparer);
		return al;
	}

	private void WriteMethodDeclaration (MethodInfo method, string entryPoint)
	{
		icall.Write ("{0} ", GetNativeType (method.ReturnType));
		icall.Write ("{0} ", entryPoint);
		ParameterInfo[] parameters = method.GetParameters();
		if (parameters.Length == 0) {
			icall.WriteLine ("(void);");
			return;
		}
		if (parameters.Length > 0) {
			icall.Write ("(");
			WriteParameterDeclaration (parameters [0]);
		}
		for (int i = 1; i < parameters.Length; ++i) {
			icall.Write (", ");
			WriteParameterDeclaration (parameters [i]);
		}
		icall.WriteLine (");");
	}

	private void DumpTypeInfo (Type t)
	{
		if (t == null)
			return;

		icall.WriteLine ("\t\t/* Type Info for " + t.FullName + ":");
		foreach (MemberInfo mi in typeof(Type).GetMembers()) {
			icall.WriteLine ("\t\t\t{0}={1}", mi.Name, GetMemberValue (mi, t));
		}
		icall.WriteLine ("\t\t */");
	}

	private static string GetMemberValue (MemberInfo mi, Type t)
	{
		try {
		switch (mi.MemberType) {
			case MemberTypes.Constructor:
			case MemberTypes.Method: {
				MethodBase b = (MethodBase) mi;
				if (b.GetParameters().Length == 0)
					return b.Invoke (t, new object[]{}).ToString();
				return "<<cannot invoke>>";
			}
			case MemberTypes.Field:
				return ((FieldInfo) mi).GetValue (t).ToString ();
			case MemberTypes.Property: {
				PropertyInfo pi = (PropertyInfo) mi;
				if (!pi.CanRead)
					return "<<cannot read>>";
				return pi.GetValue (t, null).ToString ();
			}
			default:
				return "<<unknown value>>";
		}
		}
		catch (Exception e) {
			return "<<exception reading member: " + e.Message + ">>";
		}
	}

	private void WriteParameterDeclaration (ParameterInfo pi)
	{
		// DumpTypeInfo (pi.ParameterType);
		icall.Write ("{0} {1}", GetNativeType (pi.ParameterType), pi.Name);
	}
}

// vim: noexpandtab
