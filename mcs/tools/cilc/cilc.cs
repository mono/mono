// cilc -- a CIL-to-C binding generator
// Copyright (C) 2003 Alp Toker <alp@atoker.com>
// Licensed under the terms of the GNU GPL

using System;
using System.IO;
using System.Reflection;
using System.Collections;

class cilc
{
	public static CodeWriter C, H, Cindex, Hindex;
	static string ns, dllname;
	static string cur_class;
	static string target_dir;

	static ArrayList funcs_done;

	public static int Main(string[] args)
	{
		if (args.Length != 2) {
			Console.WriteLine ("Mono CIL-to-C binding generator");
			Console.WriteLine ("Usage: cilc [options] assembly target");
			return 1;
		}

		ns = "Unnamed";
		Generate (args[0], args[1]);

		return 0;
	}

	static void Generate (string assembly, string target)
	{
		target_dir = target + Path.DirectorySeparatorChar;
		if (!Directory.Exists (target_dir)) Directory.CreateDirectory (target_dir);

		Assembly a = Assembly.LoadFrom (assembly);
		dllname = Path.GetFileName (assembly);
		AssemblyGen (a);

		//create a makefile
		CodeWriter makefile = new CodeWriter (target_dir + "Makefile");
		makefile.Indenter = "\t";
		makefile.WriteLine (@"OBJS = $(shell ls *.c | sed -e 's/\.c/.o/')");
		makefile.WriteLine (@"CFLAGS = -static -fpic $(shell pkg-config --cflags glib-2.0 mono)");
		makefile.LineBreak ();
		makefile.WriteLine ("all: lib" + ns.ToLower () + ".so");
		makefile.LineBreak ();
		makefile.WriteLine ("lib" + ns.ToLower () + ".so: $(OBJS)");
		makefile.Indent ();
		makefile.WriteLine ("gcc -Wall -fpic -shared `pkg-config --cflags --libs glib-2.0 mono` -lpthread *.c -o lib" + ns.ToLower () + ".so");
		makefile.Outdent ();
		makefile.LineBreak ();
		makefile.WriteLine ("clean:");
		makefile.Indent ();
		makefile.WriteLine ("rm -rf core *~ *.o *.so");
		makefile.Outdent ();
		makefile.Close ();
	}

	static void AssemblyGen (Assembly a)
	{
		Type[] types = a.GetTypes ();

		ns = types[0].Namespace;
		Hindex = new CodeWriter (target_dir + ns.ToLower () + ".h");
		Cindex = new CodeWriter (target_dir + ns.ToLower () + ".c");

		string Hindex_id = "__" + ns.ToUpper () + "_H__";
		Hindex.WriteLine ("#ifndef " + Hindex_id);
		Hindex.WriteLine ("#define " + Hindex_id);
		Hindex.LineBreak ();

		Cindex.WriteLine ("#include <glib.h>");
		Cindex.WriteLine ("#include <mono/jit/jit.h>");
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

		Hindex.LineBreak ();
		Hindex.WriteLine ("#endif /* " + Hindex_id + " */");

		Cindex.Close ();
		Hindex.Close ();
	}

	static void TypeGen (Type t)
	{
		//TODO: we only handle ordinary classes for now
		if (!t.IsClass) return;

		//ns = t.Namespace;
		string fname = ns.ToLower () + t.Name.ToLower ();
		C = new CodeWriter (target_dir + fname + ".c");
		H = new CodeWriter (target_dir + fname + ".h");
		Hindex.WriteLine ("#include <" + fname + ".h" + ">");


		string H_id = "__" + ns.ToUpper () + "_" + t.Name.ToUpper () + "_H__";
		H.WriteLine ("#ifndef " + H_id);
		H.WriteLine ("#define " + H_id);
		H.LineBreak ();

		H.WriteLine ("#include <glib.h>");
		H.WriteLine ("#include <mono/metadata/object.h>");
		H.WriteLine ("#include <mono/metadata/debug-helpers.h>");
		H.WriteLine ("#include <mono/metadata/appdomain.h>");
		H.LineBreak ();

		H.WriteLine ("#ifdef __cplusplus");
		H.WriteLine ("extern \"C\" {");
		H.WriteLine ("#endif /* __cplusplus */");
		H.LineBreak ();

		H.WriteLine ("typedef MonoObject " + ns + t.Name + ";");
		H.LineBreak ();


		C.WriteLine ("#include \"" + fname + ".h" + "\"");
		C.LineBreak ();

		cur_class = CamelToC (ns) + "_" + CamelToC (t.Name);
		C.WriteLine ("static MonoClass *" + cur_class + "_get_mono_class (void)");
		C.WriteLine ("{");
		C.Indent ();
		C.WriteLine ("MonoAssembly *assembly;");
		C.WriteLine ("static MonoClass *class = NULL;");
		C.WriteLine ("if (class != NULL) return class;");

		C.WriteLine ("assembly = (MonoAssembly*) " + CamelToC (ns) + "_get_mono_assembly ();");
		C.WriteLine ("class = (MonoClass*) mono_class_from_name (mono_assembly_get_image (assembly)" + ", \"" + ns + "\", \"" + t.Name + "\");");

		C.WriteLine ("mono_class_init (class);");
		C.LineBreak ();

		C.WriteLine ("return class;");
		C.Outdent ();
		C.WriteLine ("}");

		funcs_done = new ArrayList ();

		ConstructorInfo[] constructors;
		constructors = t.GetConstructors ();
		foreach (ConstructorInfo c in constructors) ConstructorGen (c, t);

		MethodInfo[] methods;
		methods = t.GetMethods (BindingFlags.Public|BindingFlags.Static|BindingFlags.DeclaredOnly);
		foreach (MethodInfo m in methods) MethodGen (m, t);

		methods = t.GetMethods (BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly);
		foreach (MethodInfo m in methods) MethodGen (m, t);

		H.LineBreak ();
		H.WriteLine ("#ifdef __cplusplus");
		H.WriteLine ("}");
		H.WriteLine ("#endif /* __cplusplus */");
		H.LineBreak ();

		H.WriteLine ("#endif /* " + H_id + " */");

		C.Close ();
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
		FunctionGen (parameters, (MethodBase) c, t, null, true);
	}

	static void MethodGen (MethodInfo m, Type t)
	{
		ParameterInfo[] parameters = m.GetParameters ();
		FunctionGen (parameters, (MethodBase) m, t, m.ReturnType, false);
	}

	static void FunctionGen (ParameterInfo[] parameters, MethodBase m, Type t, Type ret_type, bool ctor)
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

		/*
		   Console.WriteLine (ret_type);
		   if (ret_type != null && ret_type != typeof (Void)) {
		   has_return = true;
		//TODO: return simple gint or gchar if possible
		mytype = "MonoObject *";
		}
		 */

		//TODO: also check, !static
		if (inst) {
			myargs = mytype + CamelToC (t.Name);
			if (parameters.Length > 0) myargs += ", ";
		}

		string myname;

		myname = cur_class + "_";
		if (ctor) myname += "new";
		else myname += CamelToC (m.Name);

		//TODO: handle polymorphism / method overloading. this workaround ignores it. often we miss out on the useful form of a method
		if (funcs_done.Contains (myname)) return;
		funcs_done.Add (myname);

		//handle the parameters
		string param_assign = "";
		string mycsargs = "";

		for (int i = 0 ; i < parameters.Length ; i++) {
			ParameterInfo p = parameters[i];
			mycsargs += GetMonoType (Type.GetTypeCode (p.ParameterType));
			myargs += CsTypeToC (p.ParameterType.ToString ()) + p.Name;
			if (i != parameters.Length - 1) {
				mycsargs += ",";
				myargs += ", ";
			}
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

		C.WriteLine ("static MonoMethod *_mono_method = NULL;");
		if (parameters.Length != 0) C.WriteLine ("gpointer params[" + parameters.Length + "];");
		if (ctor) C.WriteLine ("MonoObject *" + CamelToC (t.Name) + ";");
		C.LineBreak ();

		C.WriteLine ("if (_mono_method == NULL) {");
		C.Indent ();

		if (ctor) C.WriteLine ("MonoMethodDesc *_mono_method_desc = mono_method_desc_new (\":.ctor()\", FALSE);");
		else {
			C.WriteLine ("MonoMethodDesc *_mono_method_desc = mono_method_desc_new (\":" + m.Name + "(" + mycsargs + ")" + "\", FALSE);");
		}

		C.WriteLine ("_mono_method = mono_method_desc_search_in_class (_mono_method_desc, " + cur_class + "_get_mono_class ());");

		C.Outdent ();
		C.WriteLine ("}");
		C.LineBreak ();

		//assign the parameters
		for (int i = 0 ; i < parameters.Length ; i++) {
			ParameterInfo p = parameters[i];
			C.WriteLine  ("params[" + i + "] = " + GetMonoVal (p.Name, p.ParameterType.ToString ()) + ";");
		}
		if (parameters.Length != 0) C.LineBreak ();

		if (ctor) C.WriteLine (CamelToC (t.Name) + " = (MonoObject*) mono_object_new ((MonoDomain*) " + CamelToC (ns) + "_get_mono_domain ()" + ", " + cur_class + "_get_mono_class ());");

		//invoke the method
		string params_arg = "NULL";
		if (parameters.Length != 0) params_arg = "params";

		string instance_arg = "NULL";
		if (!stat) instance_arg = CamelToC (t.Name);

		C.WriteLine ("mono_runtime_invoke (_mono_method, " + instance_arg + ", " + params_arg + ", NULL);");

		if (ctor) C.WriteLine ("return " + CamelToC (t.Name) + ";");

		C.Outdent ();
		C.WriteLine ("}");
	}

	static string GetMonoType (TypeCode tc)
	{
		//see mcs/class/corlib/System/TypeCode.cs
		//see mono/mono/dis/get.c

		switch (tc)
		{
			case TypeCode.Int32:
				return "int";

			case TypeCode.String:
				return "string";

			default:
				return tc.ToString ();
		}
	}

	static string GetMonoVal (string name, string type)
	{
		switch (type) {
			case "System.String":
				return "(gpointer*) mono_string_new ((MonoDomain*) mono_domain_get (), " + name + ")";

			case "System.Int32":
				return "&" + name;

			default:
			return "&" + name;
		}
	}

	static string CamelToC (string s)
	{
		//converts camel case to c-style

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
		FileStream fs = new FileStream (fname, FileMode.OpenOrCreate, FileAccess.Write);
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
		w.Flush ();
		w.Close ();
	}
}
