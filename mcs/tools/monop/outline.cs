//
// outline -- support for rendering in monop
// Some code stolen from updater.cs in monodoc.
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
using System.IO;
	
public class Outline {
	
	IndentedTextWriter o;
	Type t;
	
	public Outline (Type t, TextWriter output)
	{
		this.t = t;
		this.o = new IndentedTextWriter (output, "    ");
	}
	
	public void OutlineType ()
        {
		o.Write (GetTypeVisibility (t));
		o.Write (" ");
		o.Write (t.IsValueType ? "struct" : "class");
		o.Write (" ");
		o.Write (t.Name);
		
		Type [] interfaces = (Type []) Comparer.Sort (t.GetInterfaces ());
		Type parent = t.BaseType;

		if ((parent != null && parent != typeof (object) && parent != typeof (ValueType)) || interfaces.Length != 0) {
			bool first = true;
			o.Write (" : ");
			
			if (parent != null && parent != typeof (object) && parent != typeof (ValueType)) {
				o.Write (FormatType (parent));
				first = false;
			}
			
			foreach (Type intf in interfaces) {
				if (!first) o.Write (", ");
				first = false;
				
				o.Write (FormatType (intf));
			}
		}
		
		o.WriteLine (" {");
		o.Indent++;
		
		foreach (ConstructorInfo ci in t.GetConstructors ()) {
			OutlineConstructor (ci);
			
			o.WriteLine ();
		}
		
		o.WriteLine ();
		
		foreach (MethodInfo m in Comparer.Sort (t.GetMethods ())) {
			if ((m.Attributes & MethodAttributes.SpecialName) != 0)
				continue;
			
			OutlineMethod (m);
			
			o.WriteLine ();
		}
		
		o.WriteLine ();
		
		foreach (PropertyInfo pi in Comparer.Sort (t.GetProperties ())) {
			OutlineProperty (pi);
			
			o.WriteLine ();
		}
		
		
		o.Indent--; o.WriteLine ("}");
	}
	
	void OutlineConstructor (ConstructorInfo ci)
	{
		o.Write (GetMethodVisibility (ci));
		o.Write (t.Name);
		o.Write (" (");
		OutlineParams (ci.GetParameters ());
		o.Write (");");
	}
	
	
	void OutlineProperty (PropertyInfo pi)
	{
		ParameterInfo [] idxp = pi.GetIndexParameters ();
		MethodBase accessor = pi.CanRead ? pi.GetGetMethod () : pi.GetSetMethod ();
		
		o.Write (GetMethodVisibility (accessor));
		o.Write (GetMethodModifiers  (accessor));
		o.Write (FormatType (pi.PropertyType));
		o.Write (" ");
		
		if (idxp.Length == 0)
			o.Write (pi.Name);
		else {
			o.Write ("this [");
			OutlineParams (idxp);
			o.Write ("]");
		}
		
		o.WriteLine (" {");
		o.Indent ++;
		
		if (pi.CanRead)  o.WriteLine ("get;");
		if (pi.CanWrite) o.WriteLine ("set;");
		
		o.Indent --;
		o.Write ("}");
	}
	
	void OutlineMethod (MethodInfo mi)
	{
		o.Write (GetMethodVisibility (mi));
		o.Write (GetMethodModifiers  (mi));
		o.Write (FormatType (mi.ReturnType));
		o.Write (" ");
		o.Write (mi.Name);
		o.Write (" (");
		OutlineParams (mi.GetParameters ());
		o.Write (");");
	}
	
	void OutlineParams (ParameterInfo [] pi)
	{
		int i = 0;
		foreach (ParameterInfo p in pi) {
			bool isPointer = false;
			if (p.ParameterType.IsByRef) {
				o.Write (p.IsOut ? "out " : "ref ");
				o.Write (FormatType (p.ParameterType.GetElementType ()));
			} else
				o.Write (FormatType (p.ParameterType));
			
			o.Write (" ");
			o.Write (p.Name);
			if (i + 1 < pi.Length)
				o.Write (", ");
			i++;
		}
	}
	
	static string GetMethodVisibility (MethodBase m)
	{
		if (m.IsPublic)   return "public ";
		if (m.IsFamily)   return "protected ";
		if (m.IsPrivate)  return "private ";
		if (m.IsAssembly) return "internal ";
			
		return null;
	}
	
	static string GetMethodModifiers (MethodBase method)
	{
		if (method.IsStatic)
			return "static ";
	
		if (method.IsVirtual)
			return ((method.Attributes & MethodAttributes.NewSlot) != 0) ?
				"virtual " :
				"override ";
		
		return null;
	}
	
	static string GetTypeVisibility (Type t)
	{
                switch (t.Attributes & TypeAttributes.VisibilityMask){
                case TypeAttributes.Public:
                case TypeAttributes.NestedPublic:
                        return "public";

                case TypeAttributes.NestedFamily:
                case TypeAttributes.NestedFamANDAssem:
                case TypeAttributes.NestedFamORAssem:
                        return "protected";

                default:
                        return "internal";
                }
	}
	
	static string FormatType (Type t)
	{
		string type = t.FullName;
		if (!type.StartsWith ("System."))
			return type;
		
		if (t.HasElementType) {
			Type et = t.GetElementType ();
			if (t.IsArray)
				return FormatType (et) + " []";
			if (t.IsPointer)
				return FormatType (et) + " *";
			if (t.IsByRef)
				return "ref " + FormatType (et);
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

public class Comparer : IComparer  {
	delegate int ComparerFunc (object a, object b);
	
	ComparerFunc cmp;
	
	Comparer (ComparerFunc f)
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
