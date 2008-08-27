//
// repl.cs: Support for using the compiler in interactive mode (read-eval-print loop)
//
// Authors:
//   Miguel de Icaza (miguel@gnome.org)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2001, 2002, 2003 Ximian, Inc (http://www.ximian.com)
// Copyright 2004, 2005, 2006, 2007, 2008 Novell, Inc
//
using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
 
namespace Mono.CSharp {

	public static class InteractiveShell {
		static int class_number;
		static bool isatty = true;
		static bool dumb;
		static public ArrayList using_alias_list = new ArrayList ();
		static public ArrayList using_list = new ArrayList ();
		public static Hashtable fields = new Hashtable ();
		
		static int count;
		static string current_debug_name;

#if NET_2_0 && !SMCS_SOURCE
		static Mono.Terminal.LineEditor editor;

		static void SetupEditor ()
		{
			editor = new Mono.Terminal.LineEditor ("ics");
		}

		static string GetLine (bool primary)
		{
			string prompt = primary ? InteractiveBase.Prompt : InteractiveBase.ContinuationPrompt;

			if (dumb){
				if (isatty)
					Console.Write (prompt);

				return Console.ReadLine ();
			} else {
				return editor.Edit (prompt, "");
			}
		}
#else
		static void SetupEditor ()
		{
			dumb = true;
		}

		static string GetLine (bool primary)
		{
			string prompt = primary ? InteractiveBase.Prompt : InteractiveBase.ContinuationPrompt;

			if (isatty)
				Console.Write (prompt);

			return Console.ReadLine ();
		}
#endif

		static void Reset ()
		{
			CompilerCallableEntryPoint.Reset (false);
			Driver.LoadReferences ();

			if (!TypeManager.InitCoreTypes ())
				throw new Exception ("Failed to InitCoreTypes");
			TypeManager.InitOptionalCoreTypes ();
			
			Location.AddFile ("<interactive>");
			Location.Initialize ();

			current_debug_name = "interactive" + (count++) + ".dll";
			if (Environment.GetEnvironmentVariable ("SAVE") != null){
				CodeGen.Init (current_debug_name, current_debug_name, false);
			} else
				CodeGen.InitDynamic (current_debug_name);
		}

		static void InitializeUsing ()
		{
			Evaluate ("using System; using System.Linq; using System.Collections.Generic; using System.Collections;");
		}

		static void InitTerminal ()
		{
			isatty = UnixUtils.isatty (0) && UnixUtils.isatty (1);
			string term = Environment.GetEnvironmentVariable ("TERM");
			dumb = term == "dumb" || term == null || isatty == false;

			SetupEditor ();

			if (isatty)
				Console.WriteLine ("Mono C# Shell, type \"Help();\" for help\n\nEnter statements below.");
		}
		
		static public int ReadEvalPrintLoop ()
		{
			InitTerminal ();
			RootContext.EvalMode = true;

			InitializeUsing ();
			
			string expr = null;
			while (true){
				string input = GetLine (expr == null);
				if (input == null)
					return 0;

				expr = expr == null ? input : expr + "\n" + input;
				
				expr = Evaluate (expr);
			} 
		}

		
		static string Evaluate (string input)
		{
			if (input == null)
				return null;

			bool partial_input;
			CSharpParser parser = ParseString (true, input, Tokenizer.InteractiveParserCharacter, out partial_input);
			if (parser == null){
				if (partial_input)
					return input;
				
				if (parser == null){
					ParseString (false, input, Tokenizer.InteractiveParserCharacter, out partial_input);
					return null;
				}
			}
			
			// 
			// The parser.InteractiveResult will eventually be multiple
			// different things.  Currently they are statements, but
			// we will add support for copy-pasting entire blocks of
			// code, so we will allow namespaces, types, etc
			//
			object result = parser.InteractiveResult;
			
			if (result is Class){
				try { 
					object rval = ExecuteBlock ((Class) result);
					
					//
					// We use a reference to a compiler type, in this case
					// Driver as a flag to indicate that this was a statement
					//
					if (rval != typeof (NoValueSet)){
						PrettyPrint (rval);
						Console.WriteLine ();
					}
				} catch (Exception e){
					Console.WriteLine (e);
				}
			} else if (result is NamespaceEntry){
				((NamespaceEntry)result).Extract (using_alias_list, using_list);
			} else if (result == null){
				// Just a happy parse.
			} else {
				Console.Error.WriteLine ("Support for {0} is not available on the shell", parser.InteractiveResult);
			}
			return null;
		}

		//
		// Parses the string @input and returns a CSharpParser if succeeful.
		//
		// if @silent is set to true then no errors are
		// reported to the user.  This is used to do various calls to the
		// parser and check if the expression is parsable.
		//
		// @parser_init_code determines which token is injected in the token
		// stream.   This is used to driver the parser between statement
		// parsing, or full compilation-unit-like parsing.
		//
		// @partial_input: if @silent is true, then it returns whether the
		// parsed expression was partial, and more data is needed
		//
		static CSharpParser ParseString (bool silent, string input, int parser_init_code, out bool partial_input)
		{
			partial_input = false;
			Reset ();
			queued_fields.Clear ();
			
			Stream s = new MemoryStream (Encoding.Default.GetBytes (input));
			SeekableStreamReader seekable = new SeekableStreamReader (s, Encoding.Default);
			CSharpParser parser = new CSharpParser (seekable, Location.SourceFiles [0]);
			parser.ErrorOutput = Report.Stderr;

			parser.Lexer.putback_char = parser_init_code;
			if (silent)
				Report.DisableReporting ();
			try {
				parser.parse ();
			} finally {
				if (Report.Errors != 0){
					if (silent && parser.UnexpectedEOF)
						partial_input = true;
					parser = null;
				}

				if (silent)
					Report.EnableReporting ();
			}
			
			return parser;
		}

		static void p (string s)
		{
			Console.Write (s);
		}

		static string EscapeString (string s)
		{
			return s.Replace ("\"", "\\\"");
		}
		
		static void PrettyPrint (object result)
		{
			if (result == null){
				p ("null");
				return;
			}
			
			if (result is Array){
				Array a = (Array) result;
				
				p ("{ ");
				int top = a.GetUpperBound (0);
				for (int i = a.GetLowerBound (0); i <= top; i++){
					PrettyPrint (a.GetValue (i));
					if (i != top)
						p (", ");
				}
				p (" }");
			} else if (result is string){
				p (String.Format ("\"{0}\"", EscapeString ((string)result)));
			} else {
				p (result.ToString ());
			}
		}
		
		delegate void HostSignature (ref object retvalue);

		//
		// Queue all the fields that we use, as we need to then go from FieldBuilder to FieldInfo
		// or reflection gets confused (it basically gets confused, and variables override each
		// other).
		//
		static ArrayList queued_fields = new ArrayList ();
		
		static ArrayList types = new ArrayList ();
		
		static object ExecuteBlock (Class host)
		{
			RootContext.ResolveTree ();
			RootContext.PopulateTypes ();
			RootContext.DefineTypes ();

			if (Report.Errors != 0)
				return typeof (NoValueSet);

			TypeBuilder tb = host.TypeBuilder;
			types.Add (tb);
			MethodBuilder mb = null;
			foreach (MemberCore member in host.Methods){
				if (member.Name != "Host")
					continue;

				MethodOrOperator method = (MethodOrOperator) member;
				mb = method.MethodBuilder;
				break;
			}

			if (mb == null)
				throw new Exception ("Internal error: did not find the method builder for the generated method");
			
			RootContext.EmitCode ();
			if (Report.Errors != 0)
				return typeof (NoValueSet);
			
			RootContext.CloseTypes ();

			if (Environment.GetEnvironmentVariable ("SAVE") != null)
				CodeGen.Save (current_debug_name, false);

			//
			// Unlike Mono, .NET requires that the MethodInfo is fetched, it cant
			// work from MethodBuilders.   Retarded, I know.
			//
			Type tt = CodeGen.Assembly.Builder.GetType (tb.Name);
			MethodInfo mi = tt.GetMethod (mb.Name);

			// Pull the FieldInfos from the type, and keep track of them
			foreach (Field field in queued_fields){
				FieldInfo fi = tt.GetField (field.Name);

				FieldInfo old = (FieldInfo) fields [field.Name];

				// If a previous value was set, nullify it, so that we do
				// not leak memory
				if (old != null){
					if (!old.FieldType.IsValueType){
						try {
							old.SetValue (null, null);
						} catch {
						}
					}
				}
				
				fields [field.Name] = fi;
			}
			queued_fields.Clear ();
			
			HostSignature invoker = (HostSignature) System.Delegate.CreateDelegate (typeof (HostSignature), mi);
			object retval = typeof (NoValueSet);
			invoker (ref retval);

			// d.DynamicInvoke  (new object [] { retval });

			return retval;
		}

		static public void LoadAliases (NamespaceEntry ns)
		{
			ns.Populate (using_alias_list, using_list);
		}
		
		//
		// Just a placeholder class, used as a sentinel to determine that the
		// generated code did not set a value
		class NoValueSet {
		}

		static public FieldInfo LookupField (string name)
		{
			FieldInfo fi =  (FieldInfo) fields [name];

			return fi;
		}

		//
		// Puts the FieldBuilder into a queue of names that will be
		// registered.   We can not register FieldBuilders directly
		// we need to fetch the FieldInfo after Reflection cooks the
		// types, or bad things happen (bad means: FieldBuilders behave
		// incorrectly across multiple assemblies, causing assignments to
		// invalid areas
		//
		// This also serves for the parser to register Field classes
		// that should be exposed as global variables
		//
		static public void QueueField (Field f)
		{
			queued_fields.Add (f);
		}

		static public void ShowUsing ()
		{
			foreach (object x in using_alias_list)
				Console.WriteLine ("using {0};", x);

			foreach (object x in using_list)
				Console.WriteLine ("using {0};", x);
		}
	}

	//
	// A local variable reference that will create a Field in a
	// Class with the resolved type.  This is necessary so we can
	// support "var" as a field type in a class declaration.
	//
	// We allow LocalVariableReferece to do the heavy lifting, and
	// then we insert the field with the resolved type
	//
	public class LocalVariableReferenceWithClassSideEffect : LocalVariableReference {
		TypeContainer container;
		string name;
		
		public LocalVariableReferenceWithClassSideEffect (TypeContainer container, string name, Block current_block, string local_variable_id, Location loc)
			: base (current_block, local_variable_id, loc)
		{
			this.container = container;
			this.name = name;
		}

		public override bool Equals (object obj)
		{
			LocalVariableReferenceWithClassSideEffect lvr = obj as LocalVariableReferenceWithClassSideEffect;
			if (lvr == null)
				return false;

			if (lvr.name != name || lvr.container != container)
				return false;

			return base.Equals (obj);
		}

		public override int GetHashCode ()
		{
			return name.GetHashCode ();
		}
		
		override public Expression DoResolveLValue (EmitContext ec, Expression right_side)
		{
			Expression ret = base.DoResolveLValue (ec, right_side);
			if (ret == null)
				return null;

			Field f = new Field (container, new TypeExpression (ret.Type, Location),
					     Modifiers.NEW | Modifiers.PUBLIC | Modifiers.STATIC,
					     name, null, Location);
			container.AddField (f);
			if (f.Define ())
				InteractiveShell.QueueField (f);
			
			return ret;
		}
	}

	/// <summary>
	///    A class used to assign values if the source expression is not void
	///
	///    Used by the interactive shell to allow it to call this code to set
	///    the return value for an invocation.
	/// </summary>
	class OptionalAssign : SimpleAssign {
		public OptionalAssign (Expression t, Expression s, Location loc)
			: base (t, s, loc)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			CloneContext cc = new CloneContext ();
			Expression clone = source.Clone (cc);
			
			clone = clone.Resolve (ec);
			if (clone == null)
				return null;

			// This means its really a statement.
			if (clone.Type == TypeManager.void_type){
				source = source.Resolve (ec);
				target = null;
				type = TypeManager.void_type;
				eclass = ExprClass.Value;
				return this;
			}

			return base.DoResolve (ec);
		}

		public override void Emit (EmitContext ec)
		{
			if (target == null)
				source.Emit (ec);
			else
				base.Emit (ec);
		}

		public override void EmitStatement (EmitContext ec)
		{
			if (target == null)
				source.Emit (ec);
			else
				base.EmitStatement (ec);
		}
	}

	/// <summary>
	///   The base class for every interaction line
	/// </summary>
	public class InteractiveBase {
		public static string Prompt             = "csharp> ";
		public static string ContinuationPrompt = "      > ";

		static string Quote (string s)
		{
			if (s.IndexOf ('"') != -1)
				s = s.Replace ("\"", "\\\"");
			
			return "\"" + s + "\"";
		}
		
		static public void ShowVars ()
		{
			foreach (DictionaryEntry de in InteractiveShell.fields){
				FieldInfo fi = InteractiveShell.LookupField ((string) de.Key);
				object value = null;
				bool error = false;
				
				try {
					if (value == null)
						value = "null";
					value = fi.GetValue (null);
					if (value is string)
						value = Quote ((string)value);
				} catch {
					error = true;
				}

				if (error)
					Console.WriteLine ("{0} {1} <error reading value>", TypeManager.CSharpName(fi.FieldType), de.Key);
				else
					Console.WriteLine ("{0} {1} = {2}", TypeManager.CSharpName(fi.FieldType), de.Key, value);
			}
		}

		static public void ShowUsing ()
		{
			InteractiveShell.ShowUsing ();
		}

#if !SMCS_SOURCE
		static public void LoadPackage (string pkg)
		{
			if (pkg == null){
				Console.Error.WriteLine ("Invalid package specified");
				return;
			}

			string pkgout = Driver.GetPackageFlags (pkg, false);
			if (pkgout == null)
				return;

			string [] xargs = pkgout.Trim (new Char [] {' ', '\n', '\r', '\t'}).
				Split (new Char [] { ' ', '\t'});

			foreach (string s in xargs){
				if (s.StartsWith ("-r:") || s.StartsWith ("/r:") || s.StartsWith ("/reference:")){
					string lib = s.Substring (s.IndexOf (':')+1);

					//Console.WriteLine ("Loading: {0}", lib);
					Driver.LoadAssembly (lib, true);
					continue;
				}
			}
		}
#endif

		static public void Help ()
		{
			Console.WriteLine ("Static methods:\n"+
					   "  LoadPackage (pkg); - Loads the given Package (like -pkg:FILE)\n" +
					   "  ShowVars ();       - Shows defined local variables.\n" +
					   "  ShowUsing ();      - Show active using decltions.\n" + 
					   "  Help ();\n"
				);
		}
		
	}
}
