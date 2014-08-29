//
// eval.cs: Evaluation and Hosting API for the C# compiler
//
// Authors:
//   Miguel de Icaza (miguel@gnome.org)
//   Marek Safar (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2001, 2002, 2003 Ximian, Inc (http://www.ximian.com)
// Copyright 2004-2011 Novell, Inc
// Copyright 2011 Xamarin Inc
//

using System;
using System.Threading;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using System.Text;
using System.Linq;

namespace Mono.CSharp
{

	/// <summary>
	/// Experimental!
	/// </summary>
	public delegate void ValueModificationHandler (string variableName, int row, int column, object value);

	/// <summary>
	///   Evaluator: provides an API to evaluate C# statements and
	///   expressions dynamically.
	/// </summary>
	/// <remarks>
	///   This class exposes static methods to evaluate expressions in the
	///   current program.
	///
	///   To initialize the evaluator with a number of compiler
	///   options call the Init(string[]args) method with a set of
	///   command line options that the compiler recognizes.
	///
	///   To interrupt execution of a statement, you can invoke the
	///   Evaluator.Interrupt method.
	/// </remarks>
	public class Evaluator {

		enum ParseMode {
			// Parse silently, do not output any error messages
			Silent,

			// Report errors during parse
			ReportErrors,

			// Auto-complete, means that the tokenizer will start producing
			// GETCOMPLETIONS tokens when it reaches a certain point.
			GetCompletions
		}

		static object evaluator_lock = new object ();
		static volatile bool invoking;
		
#if !STATIC
		static int count;
#endif
		static Thread invoke_thread;

		readonly Dictionary<string, Tuple<FieldSpec, FieldInfo>> fields;

		Type base_class;
		bool inited;
		int startup_files;

		readonly CompilerContext ctx;
		readonly ModuleContainer module;
		readonly ReflectionImporter importer;
		readonly CompilationSourceFile source_file;

		int? listener_id;
		
		public Evaluator (CompilerContext ctx)
		{
			this.ctx = ctx;

			module = new ModuleContainer (ctx);
			module.Evaluator = this;

			source_file = new CompilationSourceFile (module, null);
			module.AddTypeContainer (source_file);

			startup_files = ctx.SourceFiles.Count;

			// FIXME: Importer needs this assembly for internalsvisibleto
			module.SetDeclaringAssembly (new AssemblyDefinitionDynamic (module, "evaluator"));
			importer = new ReflectionImporter (module, ctx.BuiltinTypes);

			InteractiveBaseClass = typeof (InteractiveBase);
			fields = new Dictionary<string, Tuple<FieldSpec, FieldInfo>> ();
		}

		void Init ()
		{
			var loader = new DynamicLoader (importer, ctx);

			CompilerCallableEntryPoint.Reset ();
			RootContext.ToplevelTypes = module;

			//var startup_files = new List<string> ();
			//foreach (CompilationUnit file in Location.SourceFiles)
			//    startup_files.Add (file.Path);

			loader.LoadReferences (module);
			ctx.BuiltinTypes.CheckDefinitions (module);
			module.InitializePredefinedTypes ();

			inited = true;
		}

		void ParseStartupFiles ()
		{
			Driver d = new Driver (ctx);

			Location.Initialize (ctx.SourceFiles);

			var parser_session = new ParserSession ();
			for (int i = 0; i < startup_files; ++i) {
				var sf = ctx.SourceFiles [i];
				d.Parse (sf, module, parser_session, ctx.Report);
			}
		}

		void Reset ()
		{
			CompilerCallableEntryPoint.PartialReset ();
			
			Location.Reset ();
			Location.Initialize (ctx.SourceFiles);
		}

		/// <summary>
		/// When set evaluator will automatically wait on Task of async methods. When not
		/// set it's called responsibility to handle Task execution
		/// </summary>
		public bool WaitOnTask { get; set; }

		/// <summary>
		///   If true, turns type expressions into valid expressions
		///   and calls the describe method on it
		/// </summary>
		public bool DescribeTypeExpressions;

		/// <summary>
		///   Whether the evaluator will use terse syntax, and the semicolons at the end are optional
		/// </summary>
		public bool Terse = true;

		/// <summary>
		///   The base class for the classes that host the user generated code
		/// </summary>
		/// <remarks>
		///
		///   This is the base class that will host the code
		///   executed by the Evaluator.  By default
		///   this is the Mono.CSharp.InteractiveBase class
		///   which is useful for interactive use.
		///
		///   By changing this property you can control the
		///   base class and the static members that are
		///   available to your evaluated code.
		/// </remarks>
		public Type InteractiveBaseClass {
			get {
				return base_class;
			}
			set {
				base_class = value;

				if (value != null && typeof (InteractiveBase).IsAssignableFrom (value))
					InteractiveBase.Evaluator = this;
			}
		}

		/// <summary>
		///   Interrupts the evaluation of an expression executing in Evaluate.
		/// </summary>
		/// <remarks>
		///   Use this method to interrupt long-running invocations.
		/// </remarks>
		public void Interrupt ()
		{
			if (!inited || !invoking)
				return;
			
			if (invoke_thread != null)
				invoke_thread.Abort ();
		}

		/// <summary>
		///   Compiles the input string and returns a delegate that represents the compiled code.
		/// </summary>
		/// <remarks>
		///
		///   Compiles the input string as a C# expression or
		///   statement, unlike the Evaluate method, the
		///   resulting delegate can be invoked multiple times
		///   without incurring in the compilation overhead.
		///
		///   If the return value of this function is null,
		///   this indicates that the parsing was complete.
		///   If the return value is a string it indicates
		///   that the input string was partial and that the
		///   invoking code should provide more code before
		///   the code can be successfully compiled.
		///
		///   If you know that you will always get full expressions or
		///   statements and do not care about partial input, you can use
		///   the other Compile overload. 
		///
		///   On success, in addition to returning null, the
		///   compiled parameter will be set to the delegate
		///   that can be invoked to execute the code.
		///
		/// </remarks>
		public string Compile (string input, out CompiledMethod compiled)
		{
			if (input == null || input.Length == 0){
				compiled = null;
				return null;
			}

			lock (evaluator_lock){
				if (!inited) {
					Init ();
					ParseStartupFiles ();
				} else {
					ctx.Report.Printer.Reset ();
				}

				bool partial_input;
				CSharpParser parser = ParseString (ParseMode.Silent, input, out partial_input);

				// Terse mode, try to provide the trailing semicolon automatically.
				if (parser == null && Terse && partial_input){
					bool ignore;

					// check if the source would compile with a block, if so, we should not
					// add the semicolon.
					var needs_block = ParseString (ParseMode.Silent, input + "{}", out ignore) != null;
					if (!needs_block)
						parser = ParseString (ParseMode.Silent, input + ";", out ignore);
				}
				if (parser == null){
					compiled = null;
					if (partial_input)
						return input;
					
					ParseString (ParseMode.ReportErrors, input, out partial_input);
					return null;
				}
				
				Class parser_result = parser.InteractiveResult;
				compiled = CompileBlock (parser_result, parser.undo, ctx.Report);
				return null;
			}
		}

		/// <summary>
		///   Compiles the input string and returns a delegate that represents the compiled code.
		/// </summary>
		/// <remarks>
		///
		///   Compiles the input string as a C# expression or
		///   statement, unlike the Evaluate method, the
		///   resulting delegate can be invoked multiple times
		///   without incurring in the compilation overhead.
		///
		///   This method can only deal with fully formed input
		///   strings and does not provide a completion mechanism.
		///   If you must deal with partial input (for example for
		///   interactive use) use the other overload. 
		///
		///   On success, a delegate is returned that can be used
		///   to invoke the method.
		///
		/// </remarks>
		public CompiledMethod Compile (string input)
		{
			CompiledMethod compiled;

			// Ignore partial inputs
			if (Compile (input, out compiled) != null){
				// Error, the input was partial.
				return null;
			}

			// Either null (on error) or the compiled method.
			return compiled;
		}

		static MethodInfo listener_proxy_value;
		internal void EmitValueChangedCallback (EmitContext ec, string name, TypeSpec type, Location loc)
		{
			if (listener_id == null)
				listener_id = ListenerProxy.Register (ModificationListener);

			if (listener_proxy_value == null)
				listener_proxy_value = typeof (ListenerProxy).GetMethod ("ValueChanged");

#if STATIC
			throw new NotSupportedException ();
#else
			// object value, int row, int col, string name, int listenerId
			if (type.IsStructOrEnum)
				ec.Emit (OpCodes.Box, type);

			Console.WriteLine ("EMIT: {0} => {1},{2}", name, loc.Row, loc.Column);

			ec.EmitInt (loc.Row);
			ec.EmitInt (loc.Column);
			ec.Emit (OpCodes.Ldstr, name);
			ec.EmitInt (listener_id.Value);
			ec.Emit (OpCodes.Call, listener_proxy_value);
#endif
		}

		/// <summary>
		///   Evaluates and expression or statement and returns any result values.
		/// </summary>
		/// <remarks>
		///   Evaluates the input string as a C# expression or
		///   statement.  If the input string is an expression
		///   the result will be stored in the result variable
		///   and the result_set variable will be set to true.
		///
		///   It is necessary to use the result/result_set
		///   pair to identify when a result was set (for
		///   example, execution of user-provided input can be
		///   an expression, a statement or others, and
		///   result_set would only be set if the input was an
		///   expression.
		///
		///   If the return value of this function is null,
		///   this indicates that the parsing was complete.
		///   If the return value is a string, it indicates
		///   that the input is partial and that the user
		///   should provide an updated string.
		/// </remarks>
		public string Evaluate (string input, out object result, out bool result_set)
		{
			CompiledMethod compiled;

			result_set = false;
			result = null;

			input = Compile (input, out compiled);
			if (input != null)
				return input;
			
			if (compiled == null)
				return null;
				
			//
			// The code execution does not need to keep the compiler lock
			//
			object retval = typeof (QuitValue);

			try {
				invoke_thread = System.Threading.Thread.CurrentThread;
				invoking = true;
				compiled (ref retval);
			} catch (ThreadAbortException e){
				Thread.ResetAbort ();
				Console.WriteLine ("Interrupted!\n{0}", e);
			} finally {
				invoking = false;

				if (listener_id != null) {
					ListenerProxy.Unregister (listener_id.Value);
					listener_id = null;
				}
			}

			//
			// We use a reference to a compiler type, in this case
			// Driver as a flag to indicate that this was a statement
			//
			if (!ReferenceEquals (retval, typeof (QuitValue))) {
				result_set = true;
				result = retval; 
			}

			return null;
		}

		public string [] GetCompletions (string input, out string prefix)
		{
			prefix = "";
			if (input == null || input.Length == 0)
				return null;
			
			lock (evaluator_lock){
				if (!inited)
					Init ();
				
				bool partial_input;
				CSharpParser parser = ParseString (ParseMode.GetCompletions, input, out partial_input);
				if (parser == null){
					return null;
				}

				Class host = parser.InteractiveResult;

				var base_class_imported = importer.ImportType (base_class);
				var baseclass_list = new List<FullNamedExpression> (1) {
					new TypeExpression (base_class_imported, host.Location)
				};
				host.SetBaseTypes (baseclass_list);

#if NET_4_0
				var access = AssemblyBuilderAccess.RunAndCollect;
#else
				var access = AssemblyBuilderAccess.Run;
#endif
				var a = new AssemblyDefinitionDynamic (module, "completions");
				a.Create (AppDomain.CurrentDomain, access);
				module.SetDeclaringAssembly (a);

				// Need to setup MemberCache
				host.CreateContainer ();
				// Need to setup base type
				host.DefineContainer ();

				var method = host.Members[0] as Method;
				BlockContext bc = new BlockContext (method, method.Block, ctx.BuiltinTypes.Void);

				try {
					method.Block.Resolve (bc, method);
				} catch (CompletionResult cr) {
					prefix = cr.BaseText;
					return cr.Result;
				} 
			}
			return null;
		}

		/// <summary>
		///   Executes the given expression or statement.
		/// </summary>
		/// <remarks>
		///    Executes the provided statement, returns true
		///    on success, false on parsing errors.  Exceptions
		///    might be thrown by the called code.
		/// </remarks>
		public bool Run (string statement)
		{
			object result;
			bool result_set;

			return Evaluate (statement, out result, out result_set) == null;
		}

		/// <summary>
		///   Evaluates and expression or statement and returns the result.
		/// </summary>
		/// <remarks>
		///   Evaluates the input string as a C# expression or
		///   statement and returns the value.   
		///
		///   This method will throw an exception if there is a syntax error,
		///   of if the provided input is not an expression but a statement.
		/// </remarks>
		public object Evaluate (string input)
		{
			object result;
			bool result_set;
			
			string r = Evaluate (input, out result, out result_set);

			if (r != null)
				throw new ArgumentException ("Syntax error on input: partial input");
			
			if (result_set == false)
				throw new ArgumentException ("The expression failed to resolve");

			return result;
		}

		/// <summary>
		/// Experimental!
		/// </summary>
		public ValueModificationHandler ModificationListener { get; set; }

		enum InputKind {
			EOF,
			StatementOrExpression,
			CompilationUnit,
			Error
		}

		//
		// Deambiguates the input string to determine if we
		// want to process a statement or if we want to
		// process a compilation unit.
		//
		// This is done using a top-down predictive parser,
		// since the yacc/jay parser can not deambiguage this
		// without more than one lookahead token.   There are very
		// few ambiguities.
		//
		InputKind ToplevelOrStatement (SeekableStreamReader seekable)
		{
			Tokenizer tokenizer = new Tokenizer (seekable, source_file, new ParserSession (), ctx.Report);
			
			// Prefer contextual block keywords over identifiers
			tokenizer.parsing_block++;

			int t = tokenizer.token ();
			switch (t){
			case Token.EOF:
				return InputKind.EOF;
				
			// These are toplevels
			case Token.EXTERN:
			case Token.OPEN_BRACKET:
			case Token.ABSTRACT:
			case Token.CLASS:
			case Token.ENUM:
			case Token.INTERFACE:
			case Token.INTERNAL:
			case Token.NAMESPACE:
			case Token.PRIVATE:
			case Token.PROTECTED:
			case Token.PUBLIC:
			case Token.SEALED:
			case Token.STATIC:
			case Token.STRUCT:
				return InputKind.CompilationUnit;
				
			// Definitely expression
			case Token.FIXED:
			case Token.BOOL:
			case Token.BYTE:
			case Token.CHAR:
			case Token.DECIMAL:
			case Token.DOUBLE:
			case Token.FLOAT:
			case Token.INT:
			case Token.LONG:
			case Token.NEW:
			case Token.OBJECT:
			case Token.SBYTE:
			case Token.SHORT:
			case Token.STRING:
			case Token.UINT:
			case Token.ULONG:
				return InputKind.StatementOrExpression;

			// These need deambiguation help
			case Token.USING:
				t = tokenizer.token ();
				if (t == Token.EOF)
					return InputKind.EOF;

				if (t == Token.IDENTIFIER)
					return InputKind.CompilationUnit;
				return InputKind.StatementOrExpression;


			// Distinguish between:
			//    delegate opt_anonymous_method_signature block
			//    delegate type 
			case Token.DELEGATE:
				t = tokenizer.token ();
				if (t == Token.EOF)
					return InputKind.EOF;
				if (t == Token.OPEN_PARENS || t == Token.OPEN_BRACE)
					return InputKind.StatementOrExpression;
				return InputKind.CompilationUnit;

			// Distinguih between:
			//    unsafe block
			//    unsafe as modifier of a type declaration
			case Token.UNSAFE:
				t = tokenizer.token ();
				if (t == Token.EOF)
					return InputKind.EOF;
				if (t == Token.OPEN_PARENS)
					return InputKind.StatementOrExpression;
				return InputKind.CompilationUnit;
				
		        // These are errors: we list explicitly what we had
			// from the grammar, ERROR and then everything else

			case Token.READONLY:
			case Token.OVERRIDE:
			case Token.ERROR:
				return InputKind.Error;

			// This catches everything else allowed by
			// expressions.  We could add one-by-one use cases
			// if needed.
			default:
				return InputKind.StatementOrExpression;
			}
		}
		
		//
		// Parses the string @input and returns a CSharpParser if succeeful.
		//
		// if @silent is set to true then no errors are
		// reported to the user.  This is used to do various calls to the
		// parser and check if the expression is parsable.
		//
		// @partial_input: if @silent is true, then it returns whether the
		// parsed expression was partial, and more data is needed
		//
		CSharpParser ParseString (ParseMode mode, string input, out bool partial_input)
		{
			partial_input = false;
			Reset ();

			var enc = ctx.Settings.Encoding;
			var s = new MemoryStream (enc.GetBytes (input));
			SeekableStreamReader seekable = new SeekableStreamReader (s, enc);

			InputKind kind = ToplevelOrStatement (seekable);
			if (kind == InputKind.Error){
				if (mode == ParseMode.ReportErrors)
					ctx.Report.Error (-25, "Detection Parsing Error");
				partial_input = false;
				return null;
			}

			if (kind == InputKind.EOF){
				if (mode == ParseMode.ReportErrors)
					Console.Error.WriteLine ("Internal error: EOF condition should have been detected in a previous call with silent=true");
				partial_input = true;
				return null;
				
			}
			seekable.Position = 0;

			source_file.DeclarationFound = false;
			CSharpParser parser = new CSharpParser (seekable, source_file, new ParserSession ());

			if (kind == InputKind.StatementOrExpression){
				parser.Lexer.putback_char = Tokenizer.EvalStatementParserCharacter;
				parser.Lexer.parsing_block++;
				ctx.Settings.StatementMode = true;
			} else {
				parser.Lexer.putback_char = Tokenizer.EvalCompilationUnitParserCharacter;
				ctx.Settings.StatementMode = false;
			}

			if (mode == ParseMode.GetCompletions)
				parser.Lexer.CompleteOnEOF = true;

			ReportPrinter old_printer = null;
			if ((mode == ParseMode.Silent || mode == ParseMode.GetCompletions))
				old_printer = ctx.Report.SetPrinter (new StreamReportPrinter (TextWriter.Null));

			try {
				parser.parse ();
			} finally {
				if (ctx.Report.Errors != 0){
					if (mode != ParseMode.ReportErrors  && parser.UnexpectedEOF)
						partial_input = true;

					if (parser.undo != null)
						parser.undo.ExecuteUndo ();

					parser = null;
				}

				if (old_printer != null)
					ctx.Report.SetPrinter (old_printer);
			}
			return parser;
		}

		CompiledMethod CompileBlock (Class host, Undo undo, Report Report)
		{
#if STATIC
			throw new NotSupportedException ();
#else
			string current_debug_name = "eval-" + count + ".dll";
			++count;

			AssemblyDefinitionDynamic assembly;
			AssemblyBuilderAccess access;

			if (Environment.GetEnvironmentVariable ("SAVE") != null) {
				access = AssemblyBuilderAccess.RunAndSave;
				assembly = new AssemblyDefinitionDynamic (module, current_debug_name, current_debug_name);
				assembly.Importer = importer;
			} else {
#if NET_4_0
				access = AssemblyBuilderAccess.RunAndCollect;
#else
				access = AssemblyBuilderAccess.Run;
#endif
				assembly = new AssemblyDefinitionDynamic (module, current_debug_name);
			}

			assembly.Create (AppDomain.CurrentDomain, access);

			Method expression_method;
			if (host != null) {
				var base_class_imported = importer.ImportType (base_class);
				var baseclass_list = new List<FullNamedExpression> (1) {
					new TypeExpression (base_class_imported, host.Location)
				};

				host.SetBaseTypes (baseclass_list);

				expression_method = (Method) host.Members[0];

				if ((expression_method.ModFlags & Modifiers.ASYNC) != 0) {
					//
					// Host method is async. When WaitOnTask is set we wrap it with wait
					//
					// void AsyncWait (ref object $retval) {
					//	$retval = Host();
					//	((Task)$retval).Wait();  // When WaitOnTask is set
					// }
					//
					var p = new ParametersCompiled (
						new Parameter (new TypeExpression (module.Compiler.BuiltinTypes.Object, Location.Null), "$retval", Parameter.Modifier.REF, null, Location.Null)
					);

					var method = new Method(host, new TypeExpression(module.Compiler.BuiltinTypes.Void, Location.Null),
						Modifiers.PUBLIC | Modifiers.STATIC, new MemberName("AsyncWait"), p, null);

					method.Block = new ToplevelBlock(method.Compiler, p, Location.Null);
					method.Block.AddStatement(new StatementExpression (new SimpleAssign(
						new SimpleName(p [0].Name, Location.Null),
						new Invocation(new SimpleName(expression_method.MemberName.Name, Location.Null), new Arguments(0)),
						Location.Null), Location.Null));

					if (WaitOnTask) {
						var task = new Cast (expression_method.TypeExpression, new SimpleName (p [0].Name, Location.Null), Location.Null);

						method.Block.AddStatement (new StatementExpression (new Invocation (
								new MemberAccess (task, "Wait", Location.Null),
							new Arguments (0)), Location.Null));
					}

					host.AddMember(method);

					expression_method = method;
				}

				host.CreateContainer();
				host.DefineContainer();
				host.Define();

			} else {
				expression_method = null;
			}

			module.CreateContainer ();

			// Disable module and source file re-definition checks
			module.EnableRedefinition ();
			source_file.EnableRedefinition ();

			module.Define ();

			if (Report.Errors != 0){
				if (undo != null)
					undo.ExecuteUndo ();

				return null;
			}

			if (host != null){
				host.PrepareEmit ();
				host.EmitContainer ();
			}
			
			module.EmitContainer ();

			if (Report.Errors != 0){
				if (undo != null)
					undo.ExecuteUndo ();
				return null;
			}

			module.CloseContainer ();
			if (host != null)
				host.CloseContainer ();

			if (access == AssemblyBuilderAccess.RunAndSave)
				assembly.Save ();

			if (host == null)
				return null;
			
			//
			// Unlike Mono, .NET requires that the MethodInfo is fetched, it cant
			// work from MethodBuilders.   Retarded, I know.
			//
			var tt = assembly.Builder.GetType (host.TypeBuilder.Name);
			var mi = tt.GetMethod (expression_method.MemberName.Name);

			//
			// We need to then go from FieldBuilder to FieldInfo
			// or reflection gets confused (it basically gets confused, and variables override each
			// other).
			//
			foreach (var member in host.Members) {
				var field = member as Field;
				if (field == null)
					continue;

				var fi = tt.GetField (field.Name);

				Tuple<FieldSpec, FieldInfo> old;

				// If a previous value was set, nullify it, so that we do
				// not leak memory
				if (fields.TryGetValue (field.Name, out old)) {
					if (old.Item1.MemberType.IsStruct) {
						//
						// TODO: Clear fields for structs
						//
					} else {
						try {
							old.Item2.SetValue (null, null);
						} catch {
						}
					}
				}

				fields[field.Name] = Tuple.Create (field.Spec, fi);
			}
			
			return (CompiledMethod) System.Delegate.CreateDelegate (typeof (CompiledMethod), mi);
#endif
		}

		/// <summary>
		///   A sentinel value used to indicate that no value was
		///   was set by the compiled function.   This is used to
		///   differentiate between a function not returning a
		///   value and null.
		/// </summary>
		internal static class QuitValue { }

		internal Tuple<FieldSpec, FieldInfo> LookupField (string name)
		{
			Tuple<FieldSpec, FieldInfo> fi;
			fields.TryGetValue (name, out fi);
			return fi;
		}

		static string Quote (string s)
		{
			if (s.IndexOf ('"') != -1)
				s = s.Replace ("\"", "\\\"");
			
			return "\"" + s + "\"";
		}

		public string GetUsing ()
		{
			StringBuilder sb = new StringBuilder ();
			// TODO:
			//foreach (object x in ns.using_alias_list)
			//    sb.AppendFormat ("using {0};\n", x);

			foreach (var ue in source_file.Usings) {
				sb.AppendFormat ("using {0};", ue.ToString ());
				sb.Append (Environment.NewLine);
			}

			return sb.ToString ();
		}

		internal List<string> GetUsingList ()
		{
			var res = new List<string> ();

			foreach (var ue in source_file.Usings) {
				if (ue.Alias != null || ue.ResolvedExpression == null)
					continue;

				res.Add (ue.NamespaceExpression.Name);
			}

			return res;
		}
		
		internal string [] GetVarNames ()
		{
			lock (evaluator_lock){
				return new List<string> (fields.Keys).ToArray ();
			}
		}
		
		public string GetVars ()
		{
			lock (evaluator_lock){
				StringBuilder sb = new StringBuilder ();
				
				foreach (var de in fields){
					var fi = LookupField (de.Key);
					object value;
					try {
						value = fi.Item2.GetValue (null);
						if (value is string)
							value = Quote ((string)value);
					} catch {
						value = "<error reading value>";
					}

					sb.AppendFormat ("{0} {1} = {2}", fi.Item1.MemberType.GetSignatureForError (), de.Key, value);
					sb.AppendLine ();
				}
				
				return sb.ToString ();
			}
		}

		/// <summary>
		///    Loads the given assembly and exposes the API to the user.
		/// </summary>
		public void LoadAssembly (string file)
		{
			var loader = new DynamicLoader (importer, ctx);
			var assembly = loader.LoadAssemblyFile (file, false);
			if (assembly == null)
				return;

			lock (evaluator_lock){
				importer.ImportAssembly (assembly, module.GlobalRootNamespace);
			}
		}

		/// <summary>
		///    Exposes the API of the given assembly to the Evaluator
		/// </summary>
		public void ReferenceAssembly (Assembly a)
		{
			lock (evaluator_lock){
				importer.ImportAssembly (a, module.GlobalRootNamespace);
			}
		}
	}

	
	/// <summary>
	///   A delegate that can be used to invoke the
	///   compiled expression or statement.
	/// </summary>
	/// <remarks>
	///   Since the Compile methods will compile
	///   statements and expressions into the same
	///   delegate, you can tell if a value was returned
	///   by checking whether the returned value is of type
	///   NoValueSet.   
	/// </remarks>
	
	public delegate void CompiledMethod (ref object retvalue);

	/// <summary>
	///   The default base class for every interaction line
	/// </summary>
	/// <remarks>
	///   The expressions and statements behave as if they were
	///   a static method of this class.   The InteractiveBase class
	///   contains a number of useful methods, but can be overwritten
	///   by setting the InteractiveBaseType property in the Evaluator
	/// </remarks>
	public class InteractiveBase {
		/// <summary>
		///   Determines where the standard output of methods in this class will go. 
		/// </summary>
		public static TextWriter Output = Console.Out;

		/// <summary>
		///   Determines where the standard error of methods in this class will go. 
		/// </summary>
		public static TextWriter Error = Console.Error;

		/// <summary>
		///   The primary prompt used for interactive use.
		/// </summary>
		public static string Prompt             = "csharp> ";

		/// <summary>
		///   The secondary prompt used for interactive use (used when
		///   an expression is incomplete).
		/// </summary>
		public static string ContinuationPrompt = "      > ";

		/// <summary>
		///   Used to signal that the user has invoked the  `quit' statement.
		/// </summary>
		public static bool QuitRequested;

		public static Evaluator Evaluator;
		
		/// <summary>
		///   Shows all the variables defined so far.
		/// </summary>
		static public void ShowVars ()
		{
			Output.Write (Evaluator.GetVars ());
			Output.Flush ();
		}

		/// <summary>
		///   Displays the using statements in effect at this point. 
		/// </summary>
		static public void ShowUsing ()
		{
			Output.Write (Evaluator.GetUsing ());
			Output.Flush ();
		}
	
		/// <summary>
		///   Times the execution of the given delegate
		/// </summary>
		static public TimeSpan Time (Action a)
		{
			DateTime start = DateTime.Now;
			a ();
			return DateTime.Now - start;
		}
		
		/// <summary>
		///   Loads the assemblies from a package
		/// </summary>
		/// <remarks>
		///   Loads the assemblies from a package.   This is equivalent
		///   to passing the -pkg: command line flag to the C# compiler
		///   on the command line. 
		/// </remarks>
		static public void LoadPackage (string pkg)
		{
			if (pkg == null){
				Error.WriteLine ("Invalid package specified");
				return;
			}

			string pkgout = Driver.GetPackageFlags (pkg, null);

			string [] xargs = pkgout.Trim (new Char [] {' ', '\n', '\r', '\t'}).
				Split (new Char [] { ' ', '\t'});

			foreach (string s in xargs){
				if (s.StartsWith ("-r:") || s.StartsWith ("/r:") || s.StartsWith ("/reference:")){
					string lib = s.Substring (s.IndexOf (':')+1);

					Evaluator.LoadAssembly (lib);
					continue;
				}
			}
		}

		/// <summary>
		///   Loads the assembly
		/// </summary>
		/// <remarks>
		///   Loads the specified assembly and makes its types
		///   available to the evaluator.  This is equivalent
		///   to passing the -pkg: command line flag to the C#
		///   compiler on the command line.
		/// </remarks>
		static public void LoadAssembly (string assembly)
		{
			Evaluator.LoadAssembly (assembly);
		}

		static public void print (object obj)
		{
			Output.WriteLine (obj);
		}

		static public void print (string fmt, params object [] args)
		{
			Output.WriteLine (fmt, args);
		}
		
		/// <summary>
		///   Returns a list of available static methods. 
		/// </summary>
		static public string help {
			get {
				return "Static methods:\n" +
					"  Describe (object);       - Describes the object's type\n" +
					"  LoadPackage (package);   - Loads the given Package (like -pkg:FILE)\n" +
					"  LoadAssembly (assembly); - Loads the given assembly (like -r:ASSEMBLY)\n" +
					"  ShowVars ();             - Shows defined local variables.\n" +
					"  ShowUsing ();            - Show active using declarations.\n" +
					"  Prompt                   - The prompt used by the C# shell\n" +
					"  ContinuationPrompt       - The prompt for partial input\n" +
					"  Time (() => { });        - Times the specified code\n" +
					"  print (obj);             - Shorthand for Console.WriteLine\n" +
					"  quit;                    - You'll never believe it - this quits the repl!\n" +
					"  help;                    - This help text\n";
			}
		}

		/// <summary>
		///   Indicates to the read-eval-print-loop that the interaction should be finished. 
		/// </summary>
		static public object quit {
			get {
				QuitRequested = true;

				// To avoid print null at the exit
				return typeof (Evaluator.QuitValue);
			}
		}

		/// <summary>
		///   Same as quit - useful in script scenerios
		/// </summary>
		static public void Quit () {
			QuitRequested = true;
		}

#if !NET_2_1
		/// <summary>
		///   Describes an object or a type.
		/// </summary>
		/// <remarks>
		///   This method will show a textual representation
		///   of the object's type.  If the object is a
		///   System.Type it renders the type directly,
		///   otherwise it renders the type returned by
		///   invoking GetType on the object.
		/// </remarks>
		static public string Describe (object x)
		{
			if (x == null)
				return "<null>";

			var type = x as Type ?? x.GetType ();

			StringWriter sw = new StringWriter ();
			new Outline (type, sw, true, false, false).OutlineType ();
			return sw.ToString ();
		}
#endif
	}

	class InteractiveMethod : Method
	{
		public InteractiveMethod(TypeDefinition parent, FullNamedExpression returnType, Modifiers mod, ParametersCompiled parameters)
			: base(parent, returnType, mod, new MemberName("Host"), parameters, null)
		{
		}

		public void ChangeToAsync ()
		{
			ModFlags |= Modifiers.ASYNC;
			ModFlags &= ~Modifiers.UNSAFE;
			type_expr = new TypeExpression(Module.PredefinedTypes.Task.TypeSpec, Location);
			parameters = ParametersCompiled.EmptyReadOnlyParameters;
		}

		public override string GetSignatureForError()
		{
			return "InteractiveHost";
		}
	}

	class HoistedEvaluatorVariable : HoistedVariable
	{
		public HoistedEvaluatorVariable (Field field)
			: base (null, field)
		{
		}

		protected override FieldExpr GetFieldExpression (EmitContext ec)
		{
			return new FieldExpr (field, field.Location);
		}
	}

	/// <summary>
	///    A class used to assign values if the source expression is not void
	///
	///    Used by the interactive shell to allow it to call this code to set
	///    the return value for an invocation.
	/// </summary>
	class OptionalAssign : SimpleAssign {
		public OptionalAssign (Expression s, Location loc)
			: base (null, s, loc)
		{
		}

		public override Location StartLocation {
			get {
				return Location.Null;
			}
		}

		protected override Expression DoResolve (ResolveContext ec)
		{
			Expression clone = source.Clone (new CloneContext ());

			clone = clone.Resolve (ec);
			if (clone == null)
				return null;

			//
			// A useful feature for the REPL: if we can resolve the expression
			// as a type, Describe the type;
			//
			if (ec.Module.Evaluator.DescribeTypeExpressions && !(ec.CurrentAnonymousMethod is AsyncInitializer)) {
				var old_printer = ec.Report.SetPrinter (new SessionReportPrinter ());
				Expression tclone;
				try {
					// Note: clone context cannot be shared otherwise block mapping would leak
					tclone = source.Clone (new CloneContext ());
					tclone = tclone.Resolve (ec, ResolveFlags.Type);
					if (ec.Report.Errors > 0)
						tclone = null;
				} finally {
					ec.Report.SetPrinter (old_printer);
				}

				if (tclone is TypeExpr) {
					Arguments args = new Arguments (1);
					args.Add (new Argument (new TypeOf ((TypeExpr) clone, Location)));
					return new Invocation (new SimpleName ("Describe", Location), args).Resolve (ec);
				}
			}

			// This means its really a statement.
			if (clone.Type.Kind == MemberKind.Void || clone is DynamicInvocation || clone is Assign) {
				return clone;
			}

			source = clone;

			var host = (Method) ec.MemberContext.CurrentMemberDefinition;

			if (host.ParameterInfo.IsEmpty) {
				eclass = ExprClass.Value;
				type = InternalType.FakeInternalType;
				return this;
			}

			target = new SimpleName (host.ParameterInfo[0].Name, Location);

			return base.DoResolve (ec);
		}

		public override void EmitStatement(EmitContext ec)
		{
			if (target == null) {
				source.Emit (ec);
				return;
			}

			base.EmitStatement(ec);
		}
	}

	public class Undo
	{
		List<Action> undo_actions;

		public void AddTypeContainer (TypeContainer current_container, TypeDefinition tc)
		{
			if (current_container == tc){
				Console.Error.WriteLine ("Internal error: inserting container into itself");
				return;
			}

			if (undo_actions == null)
				undo_actions = new List<Action> ();

			if (current_container.Containers != null)
			{
				var existing = current_container.Containers.FirstOrDefault (l => l.Basename == tc.Basename);
				if (existing != null) {
					current_container.RemoveContainer (existing);
					undo_actions.Add (() => current_container.AddTypeContainer (existing));
				}
			}

			undo_actions.Add (() => current_container.RemoveContainer (tc));
		}

		public void ExecuteUndo ()
		{
			if (undo_actions == null)
				return;

			foreach (var p in undo_actions){
				p ();
			}

			undo_actions = null;
		}
	}

	static class ListenerProxy
	{
		static readonly Dictionary<int, ValueModificationHandler> listeners = new Dictionary<int, ValueModificationHandler> ();

		static int counter;

		public static int Register (ValueModificationHandler listener)
		{
			lock (listeners) {
				var id = counter++;
				listeners.Add (id, listener);
				return id;
			}
		}

		public static void Unregister (int listenerId)
		{
			lock (listeners) {
				listeners.Remove (listenerId);
			}
		}

		public static void ValueChanged (object value, int row, int col, string name, int listenerId)
		{
			ValueModificationHandler action;
			lock (listeners) {
				if (!listeners.TryGetValue (listenerId, out action))
					return;
			}

			action (name, row, col, value);
		}
	}
}
