// cilc -- a CIL-to-C binding generator
// Copyright (C) 2003 Alp Toker <alp@atoker.com>
// Licensed under the terms of the GNU GPL

using System;
using System.IO;
using System.Reflection;

class cilc
{
	public static CodeWriter C, H, Cindex, Hindex;
	static string ns, dllname;
	static string cur_class;

	public static void Main(string[] args)
	{
		ns = "Unnamed";
		Generate (args[0]);
	}

	static void Generate (string assembly)
	{
		Assembly a = Assembly.LoadFrom (assembly);
		dllname = Path.GetFileName (assembly);
		AssemblyGen (a);
	}

	static void AssemblyGen (Assembly a)
	{
		Type[] types = a.GetTypes ();

		ns = types[0].Namespace;
		Hindex = new CodeWriter (ns.ToLower () + ".h");
		Cindex = new CodeWriter (ns.ToLower () + ".c");

		Cindex.WriteLine ("#include <glib.h>");
		Cindex.WriteLine ("#include <mono/metadata/object.h>");
		Cindex.WriteLine ("#include <mono/metadata/debug-helpers.h>");
		//Cindex.WriteLine ("#include \"" + CamelToC (ns) + ".h\"");
		Cindex.LineBreak ();

		Cindex.WriteLine ("MonoDomain *" + CamelToC (ns) + "_get_mono_domain (void)");
		Cindex.WriteLine ("{");
		Cindex.Indent ();
		Cindex.WriteLine ("static MonoDomain *domain = NULL;");
		Cindex.WriteLine ("if (domain != NULL) return domain;");
		Cindex.WriteLine ("domain = mono_jit_init (\"cilc\");");
		Cindex.LineBreak ();
		
		Cindex.WriteLine ("return domain;");
		Cindex.Outdent ();
		Cindex.WriteLine ("}");
		Cindex.LineBreak ();

		Cindex.WriteLine ("MonoAssembly *" + CamelToC (ns) + "_get_mono_assembly (void)");
		Cindex.WriteLine ("{");
		Cindex.Indent ();
		Cindex.WriteLine ("static MonoAssembly *assembly = NULL;");
		Cindex.WriteLine ("assembly = mono_domain_assembly_open (" + CamelToC (ns) + "_get_mono_domain (), \"" + dllname + "\");");
		Cindex.LineBreak ();
		
		Cindex.WriteLine ("return assembly;");
		Cindex.Outdent ();
		Cindex.WriteLine ("}");

		foreach (Type t in types) TypeGen (t);
		Cindex.Close ();
		Hindex.Close ();
	}

	static void TypeGen (Type t)
	{
		//ns = t.Namespace;
		string fname = ns.ToLower () + t.Name.ToLower ();
		C = new CodeWriter (fname + ".c");
		H = new CodeWriter (fname + ".h");
		Hindex.WriteLine ("#include <" + fname + ".h" + ">");


		H.WriteComment ("begin header, " + ns + " " + t.Name);
		H.LineBreak ();
		H.WriteLine ("#include <glib.h>");
		H.WriteLine ("#include <mono/metadata/object.h>");
		H.WriteLine ("#include <mono/metadata/debug-helpers.h>");
		H.LineBreak ();
		H.WriteLine ("#ifdef __cplusplus");
		H.WriteLine ("extern \"C\" {");
		H.WriteLine ("#endif /* __cplusplus */");
		H.LineBreak ();
		H.WriteLine ("typedef MonoObject " + ns + t.Name + ";");
		H.LineBreak ();

		C.WriteComment ("begin file " + ns + CamelToC (t.Name));
		C.LineBreak ();
		C.WriteLine ("#include \"" + fname + ".h" + "\"");
		C.LineBreak ();

		cur_class = CamelToC (ns) + "_" + CamelToC (t.Name);

		C.WriteLine ("static MonoClass *" + cur_class + "_get_mono_class (void)");
		C.WriteLine ("{");
		C.Indent ();
		C.WriteLine ("MonoAssembly *assembly;");
		C.WriteLine ("static MonoClass *class;");
		C.WriteLine ("if (class != NULL) return class;");
		
		C.WriteLine ("assembly = " + CamelToC (ns) + "_get_mono_assembly ();");
		C.WriteLine ("class = mono_class_from_name (assembly->image" + ", \"" + ns + "\", \"" + t.Name + "\");");

		C.WriteLine ("mono_class_init (class);");
		C.LineBreak ();
		
		C.WriteLine ("return class;");
		C.Outdent ();
		C.WriteLine ("}");

		ConstructorInfo[] constructors;
		constructors = t.GetConstructors ();
		foreach (ConstructorInfo c in constructors) ConstructorGen (c, t);

		MethodInfo[] methods;
		methods = t.GetMethods (BindingFlags.Public|BindingFlags.Static|BindingFlags.DeclaredOnly);
		foreach (MethodInfo m in methods) MethodGen (m, t);

		methods = t.GetMethods (BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly);
		foreach (MethodInfo m in methods) MethodGen (m, t);

		C.Close ();

		H.LineBreak ();
		H.WriteLine ("#ifdef __cplusplus");
		H.WriteLine ("}");
		H.WriteLine ("#endif /* __cplusplus */");
		H.Close ();
	}


	static string CsTypeToC (string p)
	{
		string ptype = "MonoClass *";

		switch (p)
		{
			case "System.String":
				ptype = "const gchar *";
			break;

			case "System.Int32":
				ptype = "gint ";
			break;
		}

		return ptype;
	}

	static void ConstructorGen (ConstructorInfo c, Type t)
	{
		ParameterInfo[] parameters = c.GetParameters ();
		FunctionGen (parameters, (MethodBase) c, t, true);
	}

	static void MethodGen (MethodInfo m, Type t)
	{
		ParameterInfo[] parameters = m.GetParameters ();
		FunctionGen (parameters, (MethodBase) m, t, false);
	}

	static void FunctionGen (ParameterInfo[] parameters, MethodBase m, Type t, bool ctor)
	{
		string myargs = "";

		bool has_return = false;
		bool stat = false;
		bool inst = false;

		if (ctor) has_return = true;
		else {
			stat = m.IsStatic;
			inst = !stat;
		}

		string mytype;

		mytype = ns + t.Name + " *";

		//TODO: also check, !static
		if (inst) {
			myargs = mytype + CamelToC (t.Name);
			if (parameters.Length > 0) myargs += ", ";
		}

		string myname;

		myname = cur_class + "_";
		if (ctor) myname += "new";
		else myname += CamelToC (m.Name);

		//construct args
		for (int i = 0 ; i < parameters.Length ; i++) {
			ParameterInfo p = parameters[i];
			myargs += CsTypeToC (p.ParameterType.ToString ()) + p.Name;
			if (i != parameters.Length - 1) myargs += ", ";
		}

		if (myargs == "") myargs = "void";


		string myproto;
		if (has_return) myproto = mytype + myname + " (" + myargs + ")";
		else myproto = "void " + myname + " (" + myargs + ")";

		H.WriteLine (myproto + ";");

		C.LineBreak ();
		C.WriteLine (myproto);
		C.WriteLine ("{");
		C.Indent ();

		C.WriteLine ("static MonoMethod *method = NULL;");
		C.WriteLine ("void **params = g_new (void *, 1);");
		if (ctor) C.WriteLine ("MonoObject *" + CamelToC (t.Name) + " = NULL;");
		C.LineBreak ();
		
		C.WriteLine ("if (method == NULL) {");
		C.Indent ();
		
		if (ctor) C.WriteLine ("MonoMethodDesc *desc = mono_method_desc_new (\":.ctor()\", 0);");
		else {
			string csproto = m.ToString ();
			int i = csproto.IndexOf(' ');
			string rettype = csproto.Substring (0, i);
			//TODO: if there's a return type, use it
			string meth_id = csproto.Substring (i + 1, csproto.Length - i - 1);
			C.WriteLine ("MonoMethodDesc *desc = mono_method_desc_new (\":" + meth_id + "\", 0);");
		}

		C.WriteLine ("method = mono_method_desc_search_in_class (desc, " + cur_class + "_get_mono_class ());");

		C.Outdent ();
		C.WriteLine ("}");
		C.LineBreak ();
		
		C.WriteLine ("//params[0] = msg;");
		
		if (ctor) C.WriteLine (CamelToC (t.Name) + " = mono_object_new (" + CamelToC (ns) + "_get_mono_domain ()" + ", " + cur_class + "_get_mono_class ());");
		
		//FIXME: TODO: pass arguments
		if (stat) C.WriteLine ("mono_runtime_invoke (method, 0, 0, 0);");
		else
			C.WriteLine ("mono_runtime_invoke (method, " + CamelToC (t.Name) + ", 0, 0);");

		if (ctor) C.WriteLine ("return " + CamelToC (t.Name) + ";");

		C.Outdent ();
		C.WriteLine ("}");
	}

	static string CamelToC (string s)
	{
		//convert camel case to c-style

		string o = "";

		bool prev_is_cap = true;

		foreach  (char c in s) {
			char cl = c.ToString ().ToLower ()[0];
			bool is_cap = c != cl;

			if (!prev_is_cap && is_cap) {
				o += "_";
			}

			o += cl;
			prev_is_cap = is_cap;
			if (c == '_') prev_is_cap = true;
		}

		return o;
	}
}

class CodeWriter
{
	private StreamWriter w;

	public CodeWriter (string fname)
	{
		FileStream fs = new FileStream ("generated/" + fname, FileMode.OpenOrCreate, FileAccess.Write);
		w = new StreamWriter (fs);
	}

	public string Indenter = "  ";
	string cur_indent = "";
	int level = 0;

	public void Indent ()
	{
		level++;
		cur_indent = "";
		for (int i = 0; i != level ; i++) cur_indent += Indenter;
	}

	public void Outdent ()
	{
		level--;
		cur_indent = "";
		for (int i = 0; i != level ; i++) cur_indent += Indenter;
	}

	public void Write (string text)
	{
		w.Write (text);
	}

	public void WriteLine (string text)
	{
		w.Write (cur_indent);
		w.WriteLine (text);
	}

	public void WriteComment (string text)
	{
		w.WriteLine ("/* " + text + " */");
	}

	public void LineBreak ()
	{
		w.WriteLine ("");
	}

	public void Close ()
	{
		w.Close ();
	}
}
