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
	static void Main (string [] args)
	{
		if (args.Length != 1) {
			Console.WriteLine ("monop <class name>");
			return;
		}
		
		IndentedTextWriter o = new IndentedTextWriter (Console.Out, "    ");
		
		string tname = args [0];
		Type t = Type.GetType (tname);
		
		o.WriteLine ("public class {0} {{", t.Name); o.Indent++;
		
		foreach (ConstructorInfo ci in t.GetConstructors ())
			o.WriteLine ("{0} ({1});", t.Name, PPParams (ci.GetParameters ()));
		
		o.WriteLine ();
		
		foreach (MethodInfo m in t.GetMethods ()) {
			if ((m.Attributes & MethodAttributes.HideBySig) != 0)
				continue;
			
			o.WriteLine (PPMethod (m));
		}
		
		o.WriteLine ();
		
		foreach (PropertyInfo pi in t.GetProperties ()) {
			ParameterInfo [] idxp = pi.GetIndexParameters ();
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
		
		return PName (mi.ReturnType) + " " + mi.Name + " (" + PPParams (mi.GetParameters ()) + ");";
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
