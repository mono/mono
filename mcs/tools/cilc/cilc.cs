// cilc -- a CIL-to-C binding generator
// Copyright (C) 2003, 2004 Alp Toker <alp@atoker.com>
// Licensed under the terms of the GNU GPL

using System;
using System.IO;
using System.Reflection;
using System.Collections;

public class cilc
{
	private cilc () {}

	static CodeWriter C, H, Cindex, Hindex, Hdecls;
	static string ns, dllname;
	static string cur_type, CurType;
	static string target_dir;

	static ArrayList funcs_done = new ArrayList ();

	public static int Main (string[] args)
	{
		if (args.Length < 1 || args.Length > 2) {
			Console.WriteLine ("Mono CIL-to-C binding generator");
			Console.WriteLine ("Usage: cilc [options] assembly target");
			return 1;
		}

		ns = "Unnamed";

		if (args.Length == 1)
			Generate (args[0], Path.GetFileNameWithoutExtension (args[0]));
		else
			Generate (args[0], args[1]);

		return 0;
	}

	public static void Generate (string assembly, string target)
	{
		target_dir = target + Path.DirectorySeparatorChar;

		if (Directory.Exists (target_dir)) {
			Console.WriteLine ("Error: Target directory " + target_dir + " already exists.");
			return;
		}

		Directory.CreateDirectory (target_dir);

		//TODO: parse given .h files here and register the type names

		Assembly a = Assembly.LoadFrom (assembly);
		dllname = Path.GetFileName (assembly);
		AssemblyGen (a);

		string soname = NsToFlat (Path.GetFileNameWithoutExtension (assembly)).ToLower ();

		//create a makefile
		CodeWriter makefile = new CodeWriter (target_dir + "Makefile");
		makefile.Indenter = "\t";
		makefile.WriteLine (@"OBJS = $(shell ls *.c | sed -e 's/\.c/.o/')");
		makefile.WriteLine (@"CFLAGS = -static -fpic $(shell pkg-config --cflags glib-2.0 gobject-2.0 mono) -I.");
		makefile.WriteLine ();
		makefile.WriteLine ("all: lib" + soname + ".so");
		makefile.WriteLine ();
		makefile.WriteLine ("lib" + soname + ".so: $(OBJS)");
		makefile.Indent ();
		makefile.WriteLine ("gcc -Wall -fpic -shared `pkg-config --libs glib-2.0 gobject-2.0 mono` -lpthread $(OBJS) -o lib" + soname + ".so");
		makefile.Outdent ();
		makefile.WriteLine ();
		makefile.WriteLine ("clean:");
		makefile.Indent ();
		makefile.WriteLine ("rm -rf core *~ *.o *.so");
		makefile.Outdent ();
		makefile.Close ();

		Console.WriteLine ();
		Console.WriteLine ("Type registry missed hits (by namespace):");
		MakeReport (registry_hits);
	}

	static void MakeReport (Hashtable ctable)
	{
		string[] reg_keys = (string[]) (new ArrayList (ctable.Keys)).ToArray (typeof (string));
		int[] reg_vals = (int[]) (new ArrayList (ctable.Values)).ToArray (typeof (int));
		Array.Sort (reg_vals, reg_keys);

		Array.Reverse (reg_vals);
		Array.Reverse (reg_keys);

		for (int i = 0 ; i != reg_keys.Length ; i++) {
			Console.WriteLine ("  " + reg_keys[i] + ": " + reg_vals[i]);
		}
	}

	static void AssemblyGen (Assembly a)
	{
		Type[] types = a.GetTypes ();
		Hashtable ns_types = new Hashtable ();

		foreach (Type t in types) {
			if (t.IsNotPublic) {
				Console.WriteLine ("Ignoring non-public type: " + t.Name);
				continue;
			}

			if (!t.IsClass) {
				Console.WriteLine ("Ignoring unrecognised type: " + t.Name);
				continue;
			}

			RegisterCsType (t);

			if (!ns_types.Contains (t.Namespace))
				ns_types[t.Namespace] = new ArrayList ();

			((ArrayList) ns_types[t.Namespace]).Add (t);
		}

		namespaces = (string[]) (new ArrayList (ns_types.Keys)).ToArray (typeof (string));

		foreach (DictionaryEntry de in ns_types)
			NamespaceGen ((string) de.Key, (Type[]) ((ArrayList) de.Value).ToArray (typeof (Type)));
	}

	static string[] namespaces;

	static void NamespaceGen (string given_ns, Type[] types)
	{
		//ns = types[0].Namespace;
		ns = given_ns;
		
		Hindex = new CodeWriter (target_dir + NsToFlat (ns).ToLower () + ".h");
		Hdecls = new CodeWriter (target_dir + NsToFlat (ns).ToLower () + "-decls.h");
		Cindex = new CodeWriter (target_dir + NsToFlat (ns).ToLower () + ".c");

		string Hindex_id = "__" + NsToFlat (ns).ToUpper () + "_H__";
		Hindex.WriteLine ("#ifndef " + Hindex_id);
		Hindex.WriteLine ("#define " + Hindex_id);
		Hindex.WriteLine ();

		string Hdecls_id = "__" + NsToFlat (ns).ToUpper () + "_DECLS_H__";
		Hdecls.WriteLine ("#ifndef " + Hdecls_id);
		Hdecls.WriteLine ("#define " + Hdecls_id);
		Hdecls.WriteLine ();

		Cindex.WriteLine ("#include <glib.h>");
		Cindex.WriteLine ("#include <mono/jit/jit.h>");
		Cindex.WriteLine ();

		Cindex.WriteLine ("MonoDomain *" + NsToC (ns) + "_get_mono_domain (void)");
		Cindex.WriteLine ("{");
		Cindex.WriteLine ("static MonoDomain *domain = NULL;");
		Cindex.WriteLine ("if (domain != NULL) return domain;");
		Cindex.WriteLine ("domain = mono_jit_init (\"cilc\");");
		Cindex.WriteLine ();

		Cindex.WriteLine ("return domain;");
		Cindex.WriteLine ("}");
		Cindex.WriteLine ();

		Cindex.WriteLine ("MonoAssembly *" + NsToC (ns) + "_get_mono_assembly (void)");
		Cindex.WriteLine ("{");
		Cindex.WriteLine ("static MonoAssembly *assembly = NULL;");
		Cindex.WriteLine ("assembly = mono_domain_assembly_open (" + NsToC (ns) + "_get_mono_domain (), \"" + dllname + "\");");
		Cindex.WriteLine ();

		Cindex.WriteLine ("return assembly;");
		Cindex.WriteLine ("}");


		foreach (Type t in types)
			TypeGen (t);

		Hindex.WriteLine ();
		Hindex.WriteLine ("#endif /* " + Hindex_id + " */");
		
		Hdecls.WriteLine ();
		Hdecls.WriteLine ("#endif /* " + Hdecls_id + " */");

		Cindex.Close ();
		Hindex.Close ();
		Hdecls.Close ();
	}

	static void TypeGen (Type t)
	{
		//TODO: we only handle ordinary classes for now
		/*
		  else if (t.IsSubclassOf (typeof (Delegate))) {
			Console.WriteLine ("Ignoring delegate: " + t.Name);
			return;
		}
		*/

		cur_type = NsToC (ns) + "_" + CamelToC (t.Name);
		CurType = NsToFlat (ns) + t.Name;

		//ns = t.Namespace;
		string fname = NsToFlat (ns).ToLower () + t.Name.ToLower ();
		C = new CodeWriter (target_dir + fname + ".c");
		H = new CodeWriter (target_dir + fname + ".h");
		Hindex.WriteLine ("#include <" + fname + ".h" + ">");


		string H_id = "__" + NsToFlat (ns).ToUpper () + "_" + t.Name.ToUpper () + "_H__";
		H.WriteLine ("#ifndef " + H_id);
		H.WriteLine ("#define " + H_id);
		H.WriteLine ();

		H.WriteLine ("#include <glib.h>");
		H.WriteLine ("#include <glib-object.h>");
		H.WriteLine ("#include <mono/metadata/object.h>");
		H.WriteLine ("#include <mono/metadata/debug-helpers.h>");
		H.WriteLine ("#include <mono/metadata/appdomain.h>");
		H.WriteLine ();
		
		if (IsRegistered (t.BaseType))
			H.WriteLine ("#include \"" + NsToFlat (t.BaseType.Namespace).ToLower () + t.BaseType.Name.ToLower () + ".h\"");

		//H.WriteLine ("#include \"" + NsToFlat (t.Namespace).ToLower () + "-decls.h\"");
		foreach (string ext_ns in namespaces)
			H.WriteLine ("#include \"" + NsToFlat (ext_ns).ToLower () + "-decls.h\"");

		H.WriteLine ();

		H.WriteLine ("#ifdef __cplusplus");
		H.WriteLine ("extern \"C\" {");
		H.WriteLine ("#endif /* __cplusplus */");
		H.WriteLine ();

		C.WriteLine ("#include \"" + fname + ".h" + "\"");
		C.WriteLine ();

		if (t.IsClass)
			ClassGen (t);
		//else if (t.IsEnum)
		//	EnumGen (t);

		H.WriteLine ();
		H.WriteLine ("#ifdef __cplusplus");
		H.WriteLine ("}");
		H.WriteLine ("#endif /* __cplusplus */");
		H.WriteLine ();

		H.WriteLine ("#endif /* " + H_id + " */");

		C.Close ();
		H.Close ();
	}

	static void EnumGen (Type t)
	{
		//TODO: we needn't split out each enum into its own file
		//TODO: just use glib-mkenums

		C.WriteLine ("GType " + cur_type + "_get_type (void)");
		C.WriteLine ("{");
		C.WriteLine ("static GType etype = 0;");
		C.WriteLine ("etype = g_enum_register_static (\"" + "\", NULL);");
		C.WriteLine ("return etype;");
		C.WriteLine ("}");
	}

	static void ClassGen (Type t)
	{
		//TODO: what flags do we want for GetEvents and GetConstructors?

		//events as signals
		EventInfo[] events;
		events = t.GetEvents (BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly);


		H.WriteLine ("G_BEGIN_DECLS");
		H.WriteLine ();
		
		{
			string NS = NsToC (ns).ToUpper ();
			string T = CamelToC (t.Name).ToUpper ();
			string NST = NS + "_" + T;
			string NSTT = NS + "_TYPE_" + T;

			H.WriteLine ("#define " + NSTT + " (" + cur_type + "_get_type ())");
			H.WriteLine ("#define " + NST + "(object) (G_TYPE_CHECK_INSTANCE_CAST ((object), " + NSTT + ", " + CurType + "))");
			H.WriteLine ("#define " + NST + "_CLASS(klass) (G_TYPE_CHECK_CLASS_CAST ((klass), " + NSTT + ", " + CurType + "Class))");
			H.WriteLine ("#define " + NS + "_IS_" + T + "(object) (G_TYPE_CHECK_INSTANCE_TYPE ((object), " + NSTT + "))");
			H.WriteLine ("#define " + NS + "_IS_" + T + "_CLASS(klass) (G_TYPE_CHECK_CLASS_TYPE ((klass), " + NSTT + "))");
			H.WriteLine ("#define " + NST + "_GET_CLASS(obj) (G_TYPE_INSTANCE_GET_CLASS ((obj), " + NSTT + ", " + CurType + "Class))");
		}
		
		if (!C.IsDuplicate) {
			Hdecls.WriteLine ("typedef struct _" + CurType + " " + CurType + ";");
			Hdecls.WriteLine ("typedef struct _" + CurType + "Class " + CurType + "Class;");
			Hdecls.WriteLine ();
		}

		H.WriteLine ();
		//H.WriteLine ("typedef struct _" + CurType + " " + CurType + ";");
		
		//H.WriteLine ();
		//H.WriteLine ("typedef struct _" + CurType + "Class " + CurType + "Class;");
		H.WriteLine ("typedef struct _" + CurType + "Private " + CurType + "Private;");
		H.WriteLine ();
		H.WriteLine ("struct _" + CurType);
		H.WriteLine ("{");

		string ParentName;
	 	if (IsRegistered (t.BaseType))
			ParentName = NsToFlat (t.BaseType.Namespace) + t.BaseType.Name;
		else
			ParentName = "GObject";

		//H.WriteLine ("GObject parent_instance;");
		H.WriteLine (ParentName + " parent_instance;");
		H.WriteLine (CurType + "Private *priv;");
		H.WriteLine ("};");
		H.WriteLine ();
		H.WriteLine ("struct _" + CurType + "Class");
		H.WriteLine ("{");
		H.WriteLine (ParentName + "Class parent_class;" + " /* inherits " + t.BaseType.Namespace + " " + t.BaseType.Name + " */");
		H.WriteLine ();
		
		//TODO: event arguments
		foreach (EventInfo ei in events)
			H.WriteLine ("void (* " + CamelToC (ei.Name) + ") (" + CurType + " *thiz" + ");");
		
		H.WriteLine ("};");
		H.WriteLine ();

		//generate c file

		//private struct
		C.WriteLine ("struct _" + CurType + "Private");
		C.WriteLine ("{");
		C.WriteLine ("MonoObject *mono_object;");
		C.WriteLine ("};");

		C.WriteLine ();

		//events
		if (events.Length != 0) {
			C.WriteLine ("enum {");

			foreach (EventInfo ei in events)
				C.WriteLine (CamelToC (ei.Name).ToUpper () + ",");

			C.WriteLine ("LAST_SIGNAL");
			C.WriteLine ("};");
			C.WriteLine ();
		}


		//TODO: if the class inherits a known GLib Object, use its raw handle
		C.WriteLine ("static gpointer parent_class;");
		
		if (events.Length == 0)
			C.WriteLine ("static guint signals[0];");
		else
			C.WriteLine ("static guint signals[LAST_SIGNAL] = { 0 };");
		C.WriteLine ();

		C.WriteLine ("static MonoClass *" + cur_type + "_get_mono_class (void)");
		C.WriteLine ("{");
		C.WriteLine ("MonoAssembly *assembly;");
		C.WriteLine ("static MonoClass *class = NULL;");
		C.WriteLine ("if (class != NULL) return class;");

		C.WriteLine ("assembly = (MonoAssembly*) " + NsToC (ns) + "_get_mono_assembly ();");
		C.WriteLine ("class = (MonoClass*) mono_class_from_name ((MonoImage*) mono_assembly_get_image (assembly)" + ", \"" + ns + "\", \"" + t.Name + "\");");

		C.WriteLine ("mono_class_init (class);");
		C.WriteLine ();

		C.WriteLine ("return class;");
		C.WriteLine ("}");

		C.WriteLine ();

		//generate the GObject init function
		C.WriteLine ("static void " + cur_type + "_init (" + CurType + " *thiz" + ")");
		C.WriteLine ("{");
		C.WriteLine ("thiz->priv = g_new0 (" + CurType + "Private, 1);");
		C.WriteLine ("}");

		C.WriteLine ();
	
		
		//generate the GObject class init function
		C.WriteLine ("static void " + cur_type + "_class_init (" + CurType + "Class *klass" + ")");
		C.WriteLine ("{");

		C.WriteLine ("GObjectClass *object_class = G_OBJECT_CLASS (klass);");
		C.WriteLine ("parent_class = g_type_class_peek_parent (klass);");
		//C.WriteLine ("object_class->finalize = _finalize;");

		foreach (EventInfo ei in events)
			EventGen (ei, t);
		
		C.WriteLine ("}");
		
		C.WriteLine ();


		//generate the GObject get_type function
		C.WriteLine ("GType " + cur_type + "_get_type (void)", H, ";");
		C.WriteLine ("{");
		C.WriteLine ("static GType object_type = 0;");
		C.WriteLine ("g_type_init ();");
		C.WriteLine ();
		C.WriteLine ("if (object_type) return object_type;");
		C.WriteLine ();
		C.WriteLine ("static const GTypeInfo object_info =");
		C.WriteLine ("{");
		C.WriteLine ("sizeof (" + CurType + "Class),");
		C.WriteLine ("(GBaseInitFunc) NULL,");
		C.WriteLine ("(GBaseFinalizeFunc) NULL,");
		C.WriteLine ("(GClassInitFunc) " + cur_type + "_class_init,");
		C.WriteLine ("NULL, /* class_finalize */");
		C.WriteLine ("NULL, /* class_data */");
		C.WriteLine ("sizeof (" + CurType + "),");
		C.WriteLine ("0, /* n_preallocs */");
		C.WriteLine ("(GInstanceInitFunc) " + cur_type + "_init,");
		C.WriteLine ("};");
		C.WriteLine ();
		
		string parent_type = "G_TYPE_OBJECT";
	 	if (IsRegistered (t.BaseType))
			parent_type = NsToC (t.BaseType.Namespace).ToUpper () + "_TYPE_" + CamelToC (t.BaseType.Name).ToUpper ();

		C.WriteLine ("object_type = g_type_register_static (" + parent_type + ", \"" + CurType + "\", &object_info, 0);");
		C.WriteLine ();
		C.WriteLine ("return object_type;");
		C.WriteLine ("}");


		//generate constructors
		ConstructorInfo[] constructors;
		constructors = t.GetConstructors ();
		foreach (ConstructorInfo c in constructors)
			ConstructorGen (c, t);

		//generate static methods
		MethodInfo[] methods;
		methods = t.GetMethods (BindingFlags.Public|BindingFlags.Static|BindingFlags.DeclaredOnly);
		foreach (MethodInfo m in methods)
			MethodGen (m, t);

		//generate instance methods
		methods = t.GetMethods (BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly);
		foreach (MethodInfo m in methods)
			MethodGen (m, t);
	
		H.WriteLine ();
		H.WriteLine ("G_END_DECLS");
	}

	//FIXME: this won't work in the general case. arraylist should contain just type names, not Types
	static ArrayList registered_types = new ArrayList ();
	static Hashtable registry_hits = new Hashtable ();

	static bool IsRegistered (Type t)
	{
		bool isreg = registered_types.Contains (t);

		//TODO: use our list of supported primitive types instead
		if (!isreg && !t.IsPrimitive) {
			//Console.WriteLine ("hit: " + t.Namespace + " " + t.Name + " : " + t.FullName);
			string tns = t.Namespace;
			if (tns == null || tns == "")
				tns = t.FullName;

			if (!registry_hits.Contains (tns)) {
				int count = 0;
				registry_hits[tns] = count;
			}

			registry_hits[tns] = (int) registry_hits[tns] + 1;
		}

		return isreg;
	}
	
	static void RegisterCsType (Type t)
	{
		if (IsRegistered (t))
			return;

		registered_types.Add (t);
	}

	static string CsTypeToC (Type t)
	{
		//TODO: use this method everywhere

		//if (t.Namespace == ns)
		if (IsRegistered (t))
			return NsToFlat (t.Namespace) + t.Name + " *";
		
		string ptype = "MonoClass *";

		switch (t.FullName)
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

	static void EventGen (EventInfo ei, Type t)
	{
		//Console.WriteLine ("TODO: event: " + ei.Name);
		//Console.WriteLine ("\t" + CamelToC (ei.Name));
		string name = CamelToC (ei.Name);

		C.WriteLine ();
		C.WriteLine ("signals[" + name.ToUpper () + "] = g_signal_new (");
		C.WriteLine ("\"" + name + "\",");
		C.WriteLine ("G_OBJECT_CLASS_TYPE (object_class),");
		C.WriteLine ("G_SIGNAL_RUN_LAST,");
		C.WriteLine ("G_STRUCT_OFFSET (" + CurType + "Class" + ", " + name + "),");
		C.WriteLine ("NULL, NULL,");
		C.WriteLine ("g_cclosure_marshal_VOID__VOID,");
		C.WriteLine ("G_TYPE_NONE, 0");
		C.WriteLine (");");
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

	static string ToValidFuncName (string name)
	{
		//avoid generated function name conflicts with internal functions

		switch (name.ToLower ()) {
			case "init":
				return "initialize";
			case "class_init":
				return "class_initialize";
			case "get_type":
				return "retrieve_type";
			default:
				return name;
		}
	}

	static void FunctionGen (ParameterInfo[] parameters, MethodBase m, Type t, Type ret_type, bool ctor)
	{
		string myargs = "";

		bool has_return = false;
		bool stat = false;
		bool inst = false;

		if (ctor) {
			has_return = true;
			stat = true;
		}
		else {
			stat = m.IsStatic;
		}
		
		inst = !stat;

		string mytype;
		mytype = CurType + " *";

		/*
		   Console.WriteLine (ret_type);
		   if (ret_type != null && ret_type != typeof (Void)) {
		   has_return = true;
		//TODO: return simple gint or gchar if possible
		mytype = "MonoObject *";
		}
		 */

		string params_arg = "NULL";
		if (parameters.Length != 0)
			params_arg = "_mono_params";

		string instance = "thiz";
		string instance_arg = instance + "->priv->mono_object";

		//TODO: also check, !static
		if (inst) {
			myargs = mytype + instance;
			if (parameters.Length > 0) myargs += ", ";
		}

		string myname;

		myname = cur_type + "_";
		if (ctor)
			myname += "new";
		else
			myname += ToValidFuncName (CamelToC (m.Name));

		//handle overloaded methods
		//TODO: generate an alias function for the default ctor etc.
		
		//TODO: how do we choose the default ctor/method overload? perhaps the
		//first/shortest, but we need scope for this
		//perhaps use DefaultMemberAttribute, Type.GetDefaultMembers

		if (funcs_done.Contains (myname)) {
			for (int i = 0 ; i < parameters.Length ; i++) {
				ParameterInfo p = parameters[i];

				if (i == 0)
					myname += "_with_";
				else if (i != parameters.Length - 1)
					myname += "_and_";

				myname += p.Name;
			}
		}

		if (funcs_done.Contains (myname))
			return;

		funcs_done.Add (myname);
		
		//handle the parameters
		string param_assign = "";
		string mycsargs = "";

		for (int i = 0 ; i < parameters.Length ; i++) {
			ParameterInfo p = parameters[i];
			mycsargs += GetMonoType (Type.GetTypeCode (p.ParameterType));
			myargs += CsTypeToC (p.ParameterType) + p.Name;
			if (i != parameters.Length - 1) {
				mycsargs += ",";
				myargs += ", ";
			}
		}

		if (myargs == "") myargs = "void";

		C.WriteLine ();

		if (has_return)
			C.WriteLine (mytype + myname + " (" + myargs + ")", H, ";");
		else
			C.WriteLine ("void " + myname + " (" + myargs + ")", H, ";");

		C.WriteLine ("{");

		C.WriteLine ("static MonoMethod *_mono_method = NULL;");
		if (parameters.Length != 0) C.WriteLine ("gpointer " + params_arg + "[" + parameters.Length + "];");
		if (ctor) {
			C.WriteLine (CurType + " *" + instance + ";");
			C.WriteLine ();
			C.WriteLine (instance + " = g_object_new (" + NsToC (ns).ToUpper () + "_TYPE_" + CamelToC (t.Name).ToUpper () + ", NULL);");
		}

		C.WriteLine ();

		C.WriteLine ("if (_mono_method == NULL) {");

		//if (ctor) C.WriteLine ("MonoMethodDesc *_mono_method_desc = mono_method_desc_new (\":.ctor()\", FALSE);");
		if (ctor) C.WriteLine ("MonoMethodDesc *_mono_method_desc = mono_method_desc_new (\":.ctor(" + mycsargs + ")\", FALSE);");
		else {
			C.WriteLine ("MonoMethodDesc *_mono_method_desc = mono_method_desc_new (\":" + m.Name + "(" + mycsargs + ")" + "\", FALSE);");
		}

		C.WriteLine ("_mono_method = mono_method_desc_search_in_class (_mono_method_desc, " + cur_type + "_get_mono_class ());");

		C.WriteLine ("}");
		C.WriteLine ();

		//assign the parameters
		for (int i = 0 ; i < parameters.Length ; i++) {
			ParameterInfo p = parameters[i];
			C.WriteLine  (params_arg + "[" + i + "] = " + GetMonoVal (p.Name, p.ParameterType.ToString ()) + ";");
		}
		if (parameters.Length != 0) C.WriteLine ();

		if (ctor) C.WriteLine (instance_arg + " = (MonoObject*) mono_object_new ((MonoDomain*) " + NsToC (ns) + "_get_mono_domain ()" + ", " + cur_type + "_get_mono_class ());");

		//invoke the method
		
		if (ctor || inst)
			C.WriteLine ("mono_runtime_invoke (_mono_method, " + instance_arg + ", " + params_arg + ", NULL);");
		else
			C.WriteLine ("mono_runtime_invoke (_mono_method, " + "NULL" + ", " + params_arg + ", NULL);");

		if (ctor) C.WriteLine ("return " + instance + ";");

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

	static string NsToC (string s)
	{
		s = s.Replace ('.', '_');
		return CamelToC (s);
	}

	static string NsToFlat (string s)
	{
		s = s.Replace (".", "");
		return s;
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

			if (c == '_')
				prev_is_cap = true;
		}

		return o;
	}
}

class CodeWriter
{
	private StreamWriter w;

	public CodeWriter (string fname)
	{
		Init (fname);
	}

	public bool IsDuplicate = false;

	void Init (string fname)
	{
		if (File.Exists (fname)) {
			string newfname = fname + ".x";
			Console.WriteLine ("Warning: File " + fname + " already exists, using " + newfname);
			IsDuplicate = true;
			Init (newfname);
			return;
		}

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
		char[] opentags = {'{', '('};
		char[] closetags = {'}', ')'};

		if (text.TrimStart (closetags) != text)
			Outdent ();

		w.Write (cur_indent);
		w.WriteLine (text);

		if (text.TrimEnd (opentags) != text)
			Indent ();
	}

	public void WriteLine (string text, CodeWriter cc)
	{
		WriteLine (text, "", cc, "");
	}

	public void WriteLine (string text, CodeWriter cc, string suffix)
	{
		WriteLine (text, "", cc, suffix);
	}

	public void WriteLine (string text, string prefix, CodeWriter cc)
	{
		WriteLine (text, prefix, cc, "");
	}

	public void WriteLine (string text, string prefix, CodeWriter cc, string suffix)
	{
		WriteLine (text);
		cc.WriteLine (prefix + text + suffix);
	}

	public void WriteComment (string text)
	{
		w.WriteLine ("/* " + text + " */");
	}

	public void WriteLine ()
	{
		w.WriteLine ("");
	}

	public void Close ()
	{
		w.Flush ();
		w.Close ();
	}
}
