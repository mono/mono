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

	public void OutlineType (BindingFlags flags)
        {
		bool first;
		
		OutlineAttributes ();
		o.Write (GetTypeVisibility (t));
		
		if (t.IsClass && !t.IsSubclassOf (typeof (System.MulticastDelegate))) {
			if (t.IsSealed)
				o.Write (t.IsAbstract ? " static" : " sealed");
			else if (t.IsAbstract)
				o.Write (" abstract");
		}
		
		o.Write (" ");
		o.Write (GetTypeKind (t));
		o.Write (" ");
		
		Type [] interfaces = (Type []) Comparer.Sort (t.GetInterfaces ());
		Type parent = t.BaseType;

		if (t.IsSubclassOf (typeof (System.MulticastDelegate))) {
			MethodInfo method;

			method = t.GetMethod ("Invoke");

			o.Write (FormatType (method.ReturnType));
			o.Write (" ");
			o.Write (t.Name);
			o.Write (" (");
			OutlineParams (method.GetParameters ());
			o.WriteLine (");");

			return;
		}
		
		o.Write (t.Name);
		if (((parent != null && parent != typeof (object) && parent != typeof (ValueType)) || interfaces.Length != 0) && ! t.IsEnum) {
			first = true;
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

		if (t.IsEnum) {
			bool is_first = true;
			foreach (FieldInfo fi in t.GetFields (BindingFlags.Public | BindingFlags.Static)) {
				
				if (! is_first)
					o.WriteLine (",");
				is_first = false;
				o.Write (fi.Name);
			}
			o.WriteLine ();
			o.Indent--; o.WriteLine ("}");
			return;
		}
		
		first = true;
		
		foreach (ConstructorInfo ci in t.GetConstructors (flags)) {
			if (first)
				o.WriteLine ();
			first = false;
			
			OutlineConstructor (ci);
			
			o.WriteLine ();
		}
		

		first = true;
		
		foreach (MethodInfo m in Comparer.Sort (t.GetMethods (flags))) {
			if ((m.Attributes & MethodAttributes.SpecialName) != 0)
				continue;
			
			if (first)
				o.WriteLine ();
			first = false;
			
			OutlineMethod (m);
			
			o.WriteLine ();
		}
		
		first = true;
		
		foreach (PropertyInfo pi in Comparer.Sort (t.GetProperties (flags))) {
			
			if (first)
				o.WriteLine ();
			first = false;
			
			OutlineProperty (pi);
			
			o.WriteLine ();
		}
		
		first = true;

		foreach (FieldInfo fi in t.GetFields (flags)) {
			
			if (first)
				o.WriteLine ();
			first = false;
			
			OutlineField (fi);
			
			o.WriteLine ();
		}

		first = true;
		
		foreach (EventInfo ei in Comparer.Sort (t.GetEvents (flags))) {
			
			if (first)
				o.WriteLine ();
			first = false;
			
			OutlineEvent (ei);
			
			o.WriteLine ();
		}

		first = true;

		foreach (Type ntype in Comparer.Sort (t.GetNestedTypes (flags))) {
			
			if (first)
				o.WriteLine ();
			first = false;
			
			new Outline (ntype, o).OutlineType (flags);
		}
		
		o.Indent--; o.WriteLine ("}");
	}

	// FIXME: add other interesting attributes?
	void OutlineAttributes ()
	{
		if (t.IsSerializable)
			o.WriteLine ("[Serializable]");

		if (t.IsDefined (typeof (System.FlagsAttribute), true))
			o.WriteLine ("[Flags]");

		if (t.IsDefined (typeof (System.ObsoleteAttribute), true))
			o.WriteLine ("[Obsolete]");
	}

	void OutlineEvent (EventInfo ei)
	{
		MethodBase accessor = ei.GetAddMethod ();
		
		o.Write (GetMethodVisibility (accessor));
		o.Write ("event ");
		o.Write (FormatType (ei.EventHandlerType));
		o.Write (" ");
		o.Write (ei.Name);
		o.Write (";");
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
		MethodBase accessor = pi.CanRead ? pi.GetGetMethod (true) : pi.GetSetMethod (true);
		
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
			if (p.ParameterType.IsByRef) {
				o.Write (p.IsOut ? "out " : "ref ");
				o.Write (FormatType (p.ParameterType.GetElementType ()));
			} else if (p.IsDefined (typeof (ParamArrayAttribute), false)) {
				o.Write ("params ");
				o.Write (FormatType (p.ParameterType));
			} else {
				o.Write (FormatType (p.ParameterType));
			}
			
			o.Write (" ");
			o.Write (p.Name);
			if (i + 1 < pi.Length)
				o.Write (", ");
			i++;
		}
	}

	void OutlineField (FieldInfo fi)
	{
		if (fi.IsPublic)   o.Write ("public ");
		if (fi.IsFamily)   o.Write ("protected ");
		if (fi.IsPrivate)  o.Write ("private ");
		if (fi.IsAssembly) o.Write ("internal ");
		if (fi.IsLiteral) o.Write ("const ");
		o.Write (FormatType (fi.FieldType));
		o.Write (" ");
		o.Write (fi.Name);
		if (fi.IsLiteral)
		{
			o.Write (" = ");
			o.Write (fi.GetValue (this));
		}
		o.Write (";");
	}

	static string GetMethodVisibility (MethodBase m)
	{
		// itnerfaces have no modifiers here
		if (m.DeclaringType.IsInterface)
			return "";
		
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
	
		// all interface methods are "virtual" but we don't say that in c#
		if (method.IsVirtual && !method.DeclaringType.IsInterface)
			return ((method.Attributes & MethodAttributes.NewSlot) != 0) ?
				"virtual " :
				"override ";
		
		return null;
	}

	static string GetTypeKind (Type t)
	{
		if (t.IsEnum)
			return "enum";
		if (t.IsClass) {
			if (t.IsSubclassOf (typeof (System.MulticastDelegate)))
				return "delegate";
			else
				return "class";
		}
		if (t.IsInterface)
			return "interface";
		if (t.IsValueType)
			return "struct";
		return "class";
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
	
	string FormatType (Type t)
	{
		string type = t.FullName;
		
		if (t.Namespace == this.t.Namespace)
			return t.Name;
		
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

	static int CompareType (object a, object b)
	{
		Type type1 = (Type) a;
		Type type2 = (Type) b;

		if (type1.IsSubclassOf (typeof (System.MulticastDelegate)) != type2.IsSubclassOf (typeof (System.MulticastDelegate)))
				return (type1.IsSubclassOf (typeof (System.MulticastDelegate)))? -1:1;
		return string.Compare (type1.Name, type2.Name);
			
	}

	static Comparer TypeComparer = new Comparer (new ComparerFunc (CompareType));

	static Type [] Sort (Type [] types)
	{
		Array.Sort (types, TypeComparer);
		return types;
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
		
		bool astatic = (aa.CanRead ? aa.GetGetMethod (true) : aa.GetSetMethod (true)).IsStatic;
		bool bstatic = (bb.CanRead ? bb.GetGetMethod (true) : bb.GetSetMethod (true)).IsStatic;
		
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
	
	static int CompareEventInfo (object a, object b)
	{
		EventInfo aa = (EventInfo) a, bb = (EventInfo) b;
		
		bool astatic = aa.GetAddMethod (true).IsStatic;
		bool bstatic = bb.GetAddMethod (true).IsStatic;
		
		if (astatic == bstatic)
			return CompareMemberInfo (a, b);
		
		if (astatic)
			return -1;
		
		return 1;
	}
	
	static Comparer EventInfoComparer = new Comparer (new ComparerFunc (CompareEventInfo));
	
	public static EventInfo [] Sort (EventInfo [] inf)
	{
		Array.Sort (inf, EventInfoComparer);
		return inf;
	}
}
