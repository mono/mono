//
// monop -- a semi-clone of javap
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2004 Ben Maurer
//


using System;
using System.Reflection;
using System.Collections;
using System.CodeDom.Compiler;

class MonoP {
	static string assembly;
	
	// very common namespaces, all in corlib
	static readonly string [] v_common_ns = {
		"System",
		"System.Collections",
		"System.Reflection",
		"System.Text",
		"System.IO"
	};
	
	static readonly string [] common_assemblies = {
		"System.Xml.dll",
		"System.Web.dll",
		"gtk-sharp.dll",
		"glib-sharp.dll"
	};
	
	static readonly string [] common_ns = {
		"System.Xml",
		"System.Web",
		"Gtk",
		"GLib"
	};
	
	static IndentedTextWriter o = new IndentedTextWriter (Console.Out, "    ");

	static Type GetType (string tname, bool ignoreCase)
	{
		Type t;
		if (assembly != null){
			Assembly a = Assembly.Load (assembly);
			t = a.GetType (tname, false, ignoreCase);
		} else
			t = Type.GetType (tname, false, ignoreCase);

		return t;
	}

	static Type GetType (string tname)
	{
		return GetType (tname, false);
	}
	
	static void Main (string [] args)
	{
		if (args.Length < 1) {
			Console.WriteLine ("monop [-r:Assembly] class-name");
			return;
		}
		
		IndentedTextWriter o = new IndentedTextWriter (Console.Out, "    ");

		int i = 0;
		if (args [0].StartsWith ("-r:")){
			i++;
			assembly = args [0].Substring (3);
		}

		if (args.Length < i+1){
			Console.WriteLine ("Usage is: monop [-r:Assembly] class-name");
			return;
		}

		string tname = args [i];
		Type t = GetType (tname);

		if (t == null) {
			// Try some very common ones, dont load anything
			foreach (string ns in v_common_ns) {
				t = GetType (ns + "." + tname, true);
				if (t != null)
					goto found;
			}
		}

		if (assembly != null){
			Console.WriteLine ("Did not find type in assembly");
		}
		
		if (t == null) {
			foreach (string assm in common_assemblies) {
				try {
					Assembly a = Assembly.Load (assm);
					t = a.GetType (tname, false, true);
					if (t != null)
						goto found;
					foreach (string ns in common_ns) {
						t = a.GetType (ns + "." + tname, false, true);
						if (t != null) {
							Console.WriteLine ("(using class from {0})", ns);
							goto found;
						}
					}
				} catch {
				}
			}
		}
		
		if (t == null) {
			Console.WriteLine ("Could not find {0}", tname);
			return;
		}
	found:
		PrintClass (t);
	}
	
	static void PrintClass (Type t)
	{
		o.Write ("public {0} {1}", t.IsValueType ? "struct" : "class", t.Name);
		
		Type [] interfaces = (Type []) Comparer.Sort (t.GetInterfaces ());
		Type parent = t.BaseType;

		if ((parent != null && parent != typeof (object) && parent != typeof (ValueType))|| interfaces.Length != 0) {
			bool first = true;
			o.Write (" : ");
			
			if (parent != null && parent != typeof (object) && parent != typeof (ValueType)) {
				o.Write (PName (parent));
				first = false;
			}
			
			foreach (Type intf in interfaces) {
				if (!first) o.Write (", ");
				first = false;
				
				o.Write (PName (intf));
			}
		}
		
		o.WriteLine (" {"); o.Indent++;
		
		foreach (ConstructorInfo ci in t.GetConstructors ())
			o.WriteLine ("{0} ({1});", t.Name, PPParams (ci.GetParameters ()));
		
		o.WriteLine ();
		
		foreach (MethodInfo m in Comparer.Sort (t.GetMethods ())) {
			if ((m.Attributes & MethodAttributes.SpecialName) != 0)
				continue;
			
			o.WriteLine (PPMethod (m));
		}
		
		o.WriteLine ();
		
		foreach (PropertyInfo pi in Comparer.Sort (t.GetProperties ())) {
			ParameterInfo [] idxp = pi.GetIndexParameters ();
			
			if ((pi.CanRead ? pi.GetGetMethod () : pi.GetSetMethod ()).IsStatic)
				o.Write ("static ");
			
			o.Write (PName (pi.PropertyType));
			o.Write (" ");
			
			if (idxp.Length == 0)
				o.Write (pi.Name);
			else
				o.Write ("this [{0}]", PPParams (idxp));
			
			o.WriteLine (" {");
			o.Indent ++;
			
			if (pi.CanRead) o.WriteLine ("get;");
			if (pi.CanWrite) o.WriteLine ("set;");
			
			o.Indent --;
			o.WriteLine ("}");
		}
		
		
		o.Indent--; o.WriteLine ("}");
	}
	
	public static string PPParams (ParameterInfo [] p) {
	
		string parms = "";
		for (int i = 0; i < p.Length; ++i) {
			if (i > 0)
				parms = parms + ", ";
				
			parms += PName (p[i].ParameterType) + " " + p [i].Name;
		}
		return parms;
	}
	
	public static string PPMethod (MethodInfo mi) {
		
		return (mi.IsStatic ? "static " : "") + PName (mi.ReturnType) + " " + mi.Name + " (" + PPParams (mi.GetParameters ()) + ");";
	}

	
	public static string PName (Type t)
	{
		string type = t.FullName;
		if (!type.StartsWith ("System."))
			return type;
		
		if (t.HasElementType) {
			Type et = t.GetElementType ();
			if (t.IsArray)
				return PName (et) + " []";
			if (t.IsPointer)
				return PName (et) + " *";
			if (t.IsByRef)
				return "ref " + PName (et);
		}

		switch (type) {
		case "System.Byte": return "byte";
		case "System.SByte": return "sbyte";
		case "System.Int16": return "short";
		case "System.Int32": return "int";
		case "System.Int64": return "long";
			
		case "System.UInt16": return "ushort";
		case "System.UInt32": return "uint";
		case "System.UInt64": return "ulong";
			
		case "System.Single":  return "float";
		case "System.Double":  return "double";
		case "System.Decimal": return "decimal";
		case "System.Boolean": return "bool";
		case "System.Char":    return "char";
		case "System.String":  return "string";
			
		case "System.Object":  return "object";
		case "System.Void":  return "void";
		}

		if (type.LastIndexOf(".") == 6)
			return type.Substring(7);
		
		return type;
	}
}

public delegate int ComparerFunc (object a, object b);
	
public class Comparer : IComparer  {
	ComparerFunc cmp;
	
	public Comparer (ComparerFunc f)
	{
		this.cmp = f;
	}
	
	public int Compare (object a, object b)
	{
		return cmp (a, b);
	}
	
	static int CompareMemberInfo (object a, object b)
	{
		return string.Compare (((MemberInfo) a).Name, ((MemberInfo) b).Name);
	}
	
	static Comparer MemberInfoComparer = new Comparer (new ComparerFunc (CompareMemberInfo));
	
	public static MemberInfo [] Sort (MemberInfo [] inf)
	{
		Array.Sort (inf, MemberInfoComparer);
		return inf;
	}
	
	static int CompareMethodBase (object a, object b)
	{
		MethodBase aa = (MethodBase) a, bb = (MethodBase) b;
		
		if (aa.IsStatic == bb.IsStatic)
			return CompareMemberInfo (a, b);
		
		if (aa.IsStatic)
			return -1;
		
		return 1;
	}
	
	static Comparer MethodBaseComparer = new Comparer (new ComparerFunc (CompareMethodBase));
	
	public static MethodBase [] Sort (MethodBase [] inf)
	{
		Array.Sort (inf, MethodBaseComparer);
		return inf;
	}
	
	static int ComparePropertyInfo (object a, object b)
	{
		PropertyInfo aa = (PropertyInfo) a, bb = (PropertyInfo) b;
		
		bool astatic = (aa.CanRead ? aa.GetGetMethod () : aa.GetSetMethod ()).IsStatic;
		bool bstatic = (bb.CanRead ? bb.GetGetMethod () : bb.GetSetMethod ()).IsStatic;
		
		if (astatic == bstatic)
			return CompareMemberInfo (a, b);
		
		if (astatic)
			return -1;
		
		return 1;
	}
	
	static Comparer PropertyInfoComparer = new Comparer (new ComparerFunc (ComparePropertyInfo));
	
	public static PropertyInfo [] Sort (PropertyInfo [] inf)
	{
		Array.Sort (inf, PropertyInfoComparer);
		return inf;
	}
}
