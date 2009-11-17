//
// System.Web.Compilation.BaseCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) Copyright 2002,2003 Ximian, Inc (http://www.ximian.com)
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
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.Configuration;
using System.IO;

namespace System.Web.Compilation
{
	abstract class BaseCompiler
	{
		const string DEFAULT_NAMESPACE = "ASP";

#if NET_2_0
		internal static Guid HashMD5 = new Guid(0x406ea660, 0x64cf, 0x4c82, 0xb6, 0xf0, 0x42, 0xd4, 0x81, 0x72, 0xa7, 0x99);
		static BindingFlags replaceableFlags = BindingFlags.Public | BindingFlags.NonPublic |
						  BindingFlags.Instance;
#endif

		TemplateParser parser;
		CodeDomProvider provider;
		ICodeCompiler compiler;
		CodeCompileUnit unit;
		CodeNamespace mainNS;
		CompilerParameters compilerParameters;
#if NET_2_0
		bool isRebuilding = false;
		protected Hashtable partialNameOverride = new Hashtable();
		protected CodeTypeDeclaration partialClass;
		protected CodeTypeReferenceExpression partialClassExpr;
#endif
		protected CodeTypeDeclaration mainClass;
		protected CodeTypeReferenceExpression mainClassExpr;
		protected static CodeThisReferenceExpression thisRef = new CodeThisReferenceExpression ();

#if NET_2_0
		VirtualPath inputVirtualPath;
		
		public VirtualPath InputVirtualPath {
			get {
				if (inputVirtualPath == null)
					inputVirtualPath = new VirtualPath (VirtualPathUtility.Combine (parser.BaseVirtualDir, Path.GetFileName (parser.InputFile)));

				return inputVirtualPath;
			}
		}
#endif
		
		protected BaseCompiler (TemplateParser parser)
		{
			this.parser = parser;
		}

		protected void AddReferencedAssembly (Assembly asm)
		{
			if (unit == null || asm == null)
				return;

			StringCollection refAsm = unit.ReferencedAssemblies;
			string asmLocation = asm.Location;
			if (!refAsm.Contains (asmLocation))
				refAsm.Add (asmLocation);
		}
		
		internal CodeStatement AddLinePragma (CodeExpression expression, ControlBuilder builder)
		{
			return AddLinePragma (new CodeExpressionStatement (expression), builder);
		}
		
		internal CodeStatement AddLinePragma (CodeStatement statement, ControlBuilder builder)
		{
			if (builder == null || statement == null)
				return statement;

			ILocation location = null;

			if (!(builder is CodeRenderBuilder))
				location = builder.Location;
			
			if (location != null)
				return AddLinePragma (statement, location);
			else
				return AddLinePragma (statement, builder.Line, builder.FileName);
		}

		internal CodeStatement AddLinePragma (CodeStatement statement, ILocation location)
		{
			if (location == null || statement == null)
				return statement;
			
			return AddLinePragma (statement, location.BeginLine, location.Filename);
		}

		bool IgnoreFile (string fileName)
		{
			if (parser != null && !parser.LinePragmasOn)
				return true;
			
			return String.Compare (fileName, "@@inner_string@@",
#if NET_2_0
					    StringComparison.OrdinalIgnoreCase
#else
					    true
#endif
			) == 0;
		}
		
		internal CodeStatement AddLinePragma (CodeStatement statement, int line, string fileName)
		{
			if (statement == null || IgnoreFile (fileName))
				return statement;
			
			statement.LinePragma = new CodeLinePragma (fileName, line);
			return statement;			
		}

		internal CodeTypeMember AddLinePragma (CodeTypeMember member, ControlBuilder builder)
		{
			if (builder == null || member == null)
				return member;

			ILocation location = builder.Location;
			
			if (location != null)
				return AddLinePragma (member, location);
			else
				return AddLinePragma (member, builder.Line, builder.FileName);
		}
		
		internal CodeTypeMember AddLinePragma (CodeTypeMember member, ILocation location)
		{
			if (location == null || member == null)
				return member;

			return AddLinePragma (member, location.BeginLine, location.Filename);
		}
		
		internal CodeTypeMember AddLinePragma (CodeTypeMember member, int line, string fileName)
		{
			if (member == null || IgnoreFile (fileName))
				return member;
			
			member.LinePragma = new CodeLinePragma (fileName, line);
			return member;
		}
		
		internal void ConstructType ()
		{
			unit = new CodeCompileUnit ();

#if NET_2_0
			byte[] md5checksum = parser.MD5Checksum;

			if (md5checksum != null) {
				CodeChecksumPragma pragma = new CodeChecksumPragma ();
				pragma.FileName = parser.InputFile;
				pragma.ChecksumAlgorithmId = HashMD5;
				pragma.ChecksumData = md5checksum;

				unit.StartDirectives.Add (pragma);
			}

			if (parser.IsPartial) {
				string partialns = null;
				string partialclasstype = parser.PartialClassName;

				int partialdot = partialclasstype.LastIndexOf ('.');
				if (partialdot != -1) {
					partialns = partialclasstype.Substring (0, partialdot);
					partialclasstype = partialclasstype.Substring (partialdot + 1);
				}
				
				CodeNamespace partialNS = new CodeNamespace (partialns);
				partialClass = new CodeTypeDeclaration (partialclasstype);
				partialClass.IsPartial = true;
				partialClassExpr = new CodeTypeReferenceExpression (parser.PartialClassName);
				
				unit.Namespaces.Add (partialNS);
				partialClass.TypeAttributes = TypeAttributes.Public;
				partialNS.Types.Add (partialClass);
			}
#endif

			string mainclasstype = parser.ClassName;
			string mainns = DEFAULT_NAMESPACE;

#if NET_2_0
			int maindot = mainclasstype.LastIndexOf ('.');
			if (maindot != -1) {
				mainns = mainclasstype.Substring (0, maindot);
				mainclasstype = mainclasstype.Substring (maindot + 1);
			}
#endif

			mainNS = new CodeNamespace (mainns);
			mainClass = new CodeTypeDeclaration (mainclasstype);
			CodeTypeReference baseTypeRef;
#if NET_2_0
			if (partialClass != null) {
				baseTypeRef = new CodeTypeReference (parser.PartialClassName);
				baseTypeRef.Options |= CodeTypeReferenceOptions.GlobalReference;
			} else {
				baseTypeRef = new CodeTypeReference (parser.BaseType.FullName);
				if (parser.BaseTypeIsGlobal)
					baseTypeRef.Options |= CodeTypeReferenceOptions.GlobalReference;
			}
#else
			baseTypeRef = new CodeTypeReference (parser.BaseType.FullName);
#endif
			mainClass.BaseTypes.Add (baseTypeRef);

			mainClassExpr = new CodeTypeReferenceExpression (mainns + "." + mainclasstype);

			unit.Namespaces.Add (mainNS);
			mainClass.TypeAttributes = TypeAttributes.Public;
			mainNS.Types.Add (mainClass);

			foreach (object o in parser.Imports.Keys) {
				if (o is string)
					mainNS.Imports.Add (new CodeNamespaceImport ((string) o));
			}

			// StringCollection.Contains has O(n) complexity, but
			// considering the number of comparisons we make on
			// average and the fact that using an intermediate array
			// would be even more costly, this is fine here.
			StringCollection refAsm = unit.ReferencedAssemblies;
			string asmName;
			if (parser.Assemblies != null) {
				foreach (object o in parser.Assemblies) {
					asmName = o as string;
					if (asmName != null && !refAsm.Contains (asmName))
						refAsm.Add (asmName);
				}
			}

#if NET_2_0
			ArrayList al = WebConfigurationManager.ExtraAssemblies;
			if (al != null && al.Count > 0) {
				foreach (object o in al) {
					asmName = o as string;
					if (asmName != null && !refAsm.Contains (asmName))
						refAsm.Add (asmName);
				}
			}

			IList list = BuildManager.CodeAssemblies;
			if (list != null && list.Count > 0) {
				Assembly asm;
				foreach (object o in list) {
					asm = o as Assembly;
					if (o == null)
						continue;
					asmName = asm.Location;
					if (asmName != null && !refAsm.Contains (asmName))
						refAsm.Add (asmName);
				}
			}
#endif
			// Late-bound generators specifics (as for MonoBASIC/VB.NET)
			unit.UserData["RequireVariableDeclaration"] = parser.ExplicitOn;
			unit.UserData["AllowLateBound"] = !parser.StrictOn;

			InitializeType ();
			AddInterfaces ();
			AddClassAttributes ();
			CreateStaticFields ();
			AddApplicationAndSessionObjects ();
			AddScripts ();
			CreateMethods ();
			CreateConstructor (null, null);
		}

		internal CodeFieldReferenceExpression GetMainClassFieldReferenceExpression (string fieldName)
		{
			CodeTypeReference mainClassTypeRef;
			mainClassTypeRef = new CodeTypeReference (mainNS.Name + "." + mainClass.Name);

#if NET_2_0
			mainClassTypeRef.Options |= CodeTypeReferenceOptions.GlobalReference;
#endif
			return new CodeFieldReferenceExpression (
				new CodeTypeReferenceExpression (mainClassTypeRef), fieldName);
		}

		protected virtual void InitializeType ()
		{}
		
		protected virtual void CreateStaticFields ()
		{
			CodeMemberField fld = new CodeMemberField (typeof (bool), "__initialized");
			fld.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			fld.InitExpression = new CodePrimitiveExpression (false);
			mainClass.Members.Add (fld);
		}

#if NET_2_0
		void AssignAppRelativeVirtualPath (CodeConstructor ctor)
		{
			if (String.IsNullOrEmpty (parser.InputFile))
				return;
			
			Type baseType = parser.CodeFileBaseClassType;
			if (baseType == null)
				baseType = parser.BaseType;
			if (baseType == null)
				return;
			if (!baseType.IsSubclassOf (typeof (System.Web.UI.TemplateControl)))
				return;
			
			CodeTypeReference baseTypeRef = new CodeTypeReference (baseType.FullName);
			if (parser.BaseTypeIsGlobal)
				baseTypeRef.Options |= CodeTypeReferenceOptions.GlobalReference;
			
			CodeExpression cast = new CodeCastExpression (baseTypeRef, new CodeThisReferenceExpression ());
			CodePropertyReferenceExpression arvpProp = new CodePropertyReferenceExpression (cast, "AppRelativeVirtualPath");
			CodeAssignStatement arvpAssign = new CodeAssignStatement ();
			arvpAssign.Left = arvpProp;
			arvpAssign.Right = new CodePrimitiveExpression (VirtualPathUtility.RemoveTrailingSlash (InputVirtualPath.AppRelative));
			ctor.Statements.Add (arvpAssign);
		}
#endif
		
		protected virtual void CreateConstructor (CodeStatementCollection localVars,
							  CodeStatementCollection trueStmt)
		{
			CodeConstructor ctor = new CodeConstructor ();
			ctor.Attributes = MemberAttributes.Public;
			mainClass.Members.Add (ctor);

			if (localVars != null)
				ctor.Statements.AddRange (localVars);

#if NET_2_0
			AssignAppRelativeVirtualPath (ctor);
#endif

			CodeFieldReferenceExpression initialized = GetMainClassFieldReferenceExpression ("__initialized");
			
			CodeBinaryOperatorExpression bin;
			bin = new CodeBinaryOperatorExpression (initialized,
								CodeBinaryOperatorType.ValueEquality,
								new CodePrimitiveExpression (false));

			CodeAssignStatement assign = new CodeAssignStatement (initialized,
									      new CodePrimitiveExpression (true));

			CodeConditionStatement cond = new CodeConditionStatement ();
			cond.Condition = bin;
			
			if (trueStmt != null)
				cond.TrueStatements.AddRange (trueStmt);
			cond.TrueStatements.Add (assign);
			ctor.Statements.Add (cond);
			AddStatementsToConstructor (ctor);
		}

		protected virtual void AddStatementsToConstructor (CodeConstructor ctor)
		{
		}
		
		void AddScripts ()
		{
			if (parser.Scripts == null || parser.Scripts.Count == 0)
				return;

			ServerSideScript sss;
			
			foreach (object o in parser.Scripts) {
				sss = o as ServerSideScript;

				if (sss == null)
					continue;
				
				mainClass.Members.Add (AddLinePragma (new CodeSnippetTypeMember (sss.Script), sss.Location));
			}
		}
		
		protected internal virtual void CreateMethods ()
		{
		}

#if NET_2_0
		void InternalCreatePageProperty (string retType, string name, string contextProperty)
		{
			CodeMemberProperty property = new CodeMemberProperty ();
			property.Name = name;
			property.Type = new CodeTypeReference (retType);
			property.Attributes = MemberAttributes.Family | MemberAttributes.Final;

			CodeMethodReturnStatement ret = new CodeMethodReturnStatement ();
			CodeCastExpression cast = new CodeCastExpression ();
			ret.Expression = cast;
			
			CodePropertyReferenceExpression refexp = new CodePropertyReferenceExpression ();
			refexp.TargetObject = new CodePropertyReferenceExpression (new CodeThisReferenceExpression (), "Context");
			refexp.PropertyName = contextProperty;
			
			cast.TargetType = new CodeTypeReference (retType);
			cast.Expression = refexp;
			
			property.GetStatements.Add (ret);
			if (partialClass == null)
				mainClass.Members.Add (property);
			else
				partialClass.Members.Add (property);
		}
		
		protected void CreateProfileProperty ()
		{
			string retType;
			if (AppCodeCompiler.HaveCustomProfile (WebConfigurationManager.GetWebApplicationSection ("system.web/profile") as ProfileSection))
				retType = "ProfileCommon";
			else
				retType = "System.Web.Profile.DefaultProfile";
			InternalCreatePageProperty (retType, "Profile", "Profile");
		}
#endif
		
		protected virtual void AddInterfaces ()
		{
			if (parser.Interfaces == null)
				return;

			foreach (object o in parser.Interfaces) {
				if (o is string)
					mainClass.BaseTypes.Add (new CodeTypeReference ((string) o));
			}
		}

		protected virtual void AddClassAttributes ()
		{
		}
		
		protected virtual void AddApplicationAndSessionObjects ()
		{
		}

		/* Utility methods for <object> stuff */
		protected void CreateApplicationOrSessionPropertyForObject (Type type,
									    string propName,
									    bool isApplication,
									    bool isPublic)
		{
			/* if isApplication this generates (the 'cachedapp' field is created earlier):
			private MyNS.MyClass app {
				get {
					if ((this.cachedapp == null)) {
						this.cachedapp = ((MyNS.MyClass)
							(this.Application.StaticObjects.GetObject("app")));
					}
					return this.cachedapp;
				}
			}

			else, this is for Session:
			private MyNS.MyClass ses {
				get {
					return ((MyNS.MyClass) (this.Session.StaticObjects.GetObject("ses")));
				}
			}

			*/

			CodeExpression result = null;

			CodeMemberProperty prop = new CodeMemberProperty ();
			prop.Type = new CodeTypeReference (type);
			prop.Name = propName;
			if (isPublic)
				prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			else
				prop.Attributes = MemberAttributes.Private | MemberAttributes.Final;

			CodePropertyReferenceExpression p1;
			if (isApplication)
				p1 = new CodePropertyReferenceExpression (thisRef, "Application");
			else
				p1 = new CodePropertyReferenceExpression (thisRef, "Session");

			CodePropertyReferenceExpression p2;
			p2 = new CodePropertyReferenceExpression (p1, "StaticObjects");

			CodeMethodReferenceExpression getobject;
			getobject = new CodeMethodReferenceExpression (p2, "GetObject");

			CodeMethodInvokeExpression invoker;
			invoker = new CodeMethodInvokeExpression (getobject,
						new CodePrimitiveExpression (propName));

			CodeCastExpression cast = new CodeCastExpression (prop.Type, invoker);

			if (isApplication) {
				CodeFieldReferenceExpression field;
				field = new CodeFieldReferenceExpression (thisRef, "cached" + propName);

				CodeConditionStatement stmt = new CodeConditionStatement();
				stmt.Condition = new CodeBinaryOperatorExpression (field,
							CodeBinaryOperatorType.IdentityEquality,
							new CodePrimitiveExpression (null));

				CodeAssignStatement assign = new CodeAssignStatement ();
				assign.Left = field;
				assign.Right = cast;
				stmt.TrueStatements.Add (assign);
				prop.GetStatements.Add (stmt);
				result = field;
			} else {
				result = cast;
			}
						
			prop.GetStatements.Add (new CodeMethodReturnStatement (result));
			mainClass.Members.Add (prop);
		}

		protected string CreateFieldForObject (Type type, string name)
		{
			string fieldName = "cached" + name;
			CodeMemberField f = new CodeMemberField (type, fieldName);
			f.Attributes = MemberAttributes.Private;
			mainClass.Members.Add (f);
			return fieldName;
		}

		protected void CreatePropertyForObject (Type type, string propName, string fieldName, bool isPublic)
		{
			CodeFieldReferenceExpression field = new CodeFieldReferenceExpression (thisRef, fieldName);
			CodeMemberProperty prop = new CodeMemberProperty ();
			prop.Type = new CodeTypeReference (type);
			prop.Name = propName;
			if (isPublic)
				prop.Attributes = MemberAttributes.Public | MemberAttributes.Final;
			else
				prop.Attributes = MemberAttributes.Private | MemberAttributes.Final;

			CodeConditionStatement stmt = new CodeConditionStatement();
			stmt.Condition = new CodeBinaryOperatorExpression (field,
						CodeBinaryOperatorType.IdentityEquality,
						new CodePrimitiveExpression (null));

			CodeObjectCreateExpression create = new CodeObjectCreateExpression (prop.Type);	
			stmt.TrueStatements.Add (new CodeAssignStatement (field, create));
			prop.GetStatements.Add (stmt);
			prop.GetStatements.Add (new CodeMethodReturnStatement (field));

			mainClass.Members.Add (prop);
		}
		/******/

		void CheckCompilerErrors (CompilerResults results)
		{
			if (results.NativeCompilerReturnValue == 0)
				return;

			string fileText = null;
			CompilerErrorCollection errors = results.Errors;
			CompilerError ce = (errors != null && errors.Count > 0) ? errors [0] : null;
			string inFile = (ce != null) ? ce.FileName : null;
			
			if (inFile != null && File.Exists (inFile)) {
				using (StreamReader sr = File.OpenText (inFile)) {
					fileText = sr.ReadToEnd ();
				}
			} else {
				StringWriter writer = new StringWriter();
				provider.CreateGenerator().GenerateCodeFromCompileUnit (unit, writer, null);
				fileText = writer.ToString ();
			}
			throw new CompilationException (parser.InputFile, errors, fileText);
		}

		protected string DynamicDir ()
		{
			return AppDomain.CurrentDomain.SetupInformation.DynamicBase;
		}

		internal static CodeDomProvider CreateProvider (string lang)
		{
			CompilerParameters par;
			string tempdir;
			
			return CreateProvider (HttpContext.Current, lang, out par, out tempdir);
		}
		
		internal static CodeDomProvider CreateProvider (string lang, out string compilerOptions, out int warningLevel, out string tempdir)
		{
			return CreateProvider (HttpContext.Current, lang, out compilerOptions, out warningLevel, out tempdir);
		}
		
		internal static CodeDomProvider CreateProvider (HttpContext context, string lang, out string compilerOptions, out int warningLevel, out string tempdir)
		{
			CodeDomProvider ret;
			CompilerParameters par;

			ret = CreateProvider (context, lang, out par, out tempdir);
			if (par != null){
				warningLevel = par.WarningLevel;
				compilerOptions = par.CompilerOptions;
			} else {
				warningLevel = 2;
				compilerOptions = String.Empty;
			}

			return ret;
		}

		internal static CodeDomProvider CreateProvider (HttpContext context, string lang, out CompilerParameters par, out string tempdir)
		{
			CodeDomProvider ret = null;
			par = null;
			
#if NET_2_0
			CompilationSection config = (CompilationSection) WebConfigurationManager.GetWebApplicationSection ("system.web/compilation");
			Compiler comp = config.Compilers[lang];
			
			if (comp == null) {
				CompilerInfo info = CodeDomProvider.GetCompilerInfo (lang);
				if (info != null && info.IsCodeDomProviderTypeValid) {
					ret = info.CreateProvider ();
					par = info.CreateDefaultCompilerParameters ();
				}
			} else {
				Type t = HttpApplication.LoadType (comp.Type, true);
				ret = Activator.CreateInstance (t) as CodeDomProvider;

				par = new CompilerParameters ();
				par.CompilerOptions = comp.CompilerOptions;
				par.WarningLevel = comp.WarningLevel;
			}
#else
			CompilationConfiguration config;

			config = CompilationConfiguration.GetInstance (context);
			ret = config.GetProvider (lang);

			par = new CompilerParameters ();
			par.CompilerOptions = config.GetCompilerOptions (lang);
			par.WarningLevel = config.GetWarningLevel (lang);
#endif
			tempdir = config.TempDirectory;

			return ret;
		}
		
		[MonoTODO ("find out how to extract the warningLevel and compilerOptions in the <system.codedom> case")]
		public virtual Type GetCompiledType () 
		{
			Type type = CachingCompiler.GetTypeFromCache (parser.InputFile);
			if (type != null)
				return type;

			ConstructType ();
			string lang = parser.Language;
			string tempdir;
			string compilerOptions;
			int warningLevel;

			Provider = CreateProvider (parser.Context, lang, out compilerOptions, out warningLevel, out tempdir);
			if (Provider == null)
				throw new HttpException ("Configuration error. Language not supported: " +
							  lang, 500);

#if !NET_2_0
			compiler = provider.CreateCompiler ();
#endif

			CompilerParameters parameters = CompilerParameters;
			parameters.IncludeDebugInformation = parser.Debug;
			parameters.CompilerOptions = compilerOptions + " " + parser.CompilerOptions;
			parameters.WarningLevel = warningLevel;
			
			bool keepFiles = (Environment.GetEnvironmentVariable ("MONO_ASPNET_NODELETE") != null);

			if (tempdir == null || tempdir == "")
				tempdir = DynamicDir ();
				
			TempFileCollection tempcoll = new TempFileCollection (tempdir, keepFiles);
			parameters.TempFiles = tempcoll;
			string dllfilename = Path.GetFileName (tempcoll.AddExtension ("dll", true));
			parameters.OutputAssembly = Path.Combine (DynamicDir (), dllfilename);

			CompilerResults results = CachingCompiler.Compile (this);
			CheckCompilerErrors (results);
			Assembly assembly = results.CompiledAssembly;
			if (assembly == null) {
				if (!File.Exists (parameters.OutputAssembly)) {
					results.TempFiles.Delete ();
					throw new CompilationException (parser.InputFile, results.Errors,
						"No assembly returned after compilation!?");
				}

				assembly = Assembly.LoadFrom (parameters.OutputAssembly);
			}

			results.TempFiles.Delete ();
			Type mainClassType = assembly.GetType (MainClassType, true);

#if NET_2_0
			if (parser.IsPartial) {
				// With the partial classes, we need to make sure we
				// don't have any methods that should have not been
				// created (because they are accessible from the base
				// types). We cannot do this normally because the
				// codebehind file is actually a partial class and we
				// have no way of identifying the partial class' base
				// type until now.
				if (!isRebuilding && CheckPartialBaseType (mainClassType)) {
					isRebuilding = true;
					parser.RootBuilder.ResetState ();
					return GetCompiledType ();
				}
			}
#endif

			return mainClassType;
		}

		internal string MainClassType {
			get {
				if (mainClassExpr == null)
					return null;

				return mainClassExpr.Type.BaseType;
			}
		}
		
#if NET_2_0
		internal bool IsRebuildingPartial
		{
			get { return isRebuilding; }
		}

		internal bool CheckPartialBaseType (Type type)
		{
			// Get the base type. If we don't have any (bad thing), we
			// don't need to replace ourselves. Also check for the
			// core file, since that won't have any either.
			Type baseType = type.BaseType;
			if (baseType == null || baseType == typeof(System.Web.UI.Page))
				return false;

			bool rebuild = false;

			if (CheckPartialBaseFields (type, baseType))
				rebuild = true;

			if (CheckPartialBaseProperties (type, baseType))
				rebuild = true;

			return rebuild;
		}

		internal bool CheckPartialBaseFields (Type type, Type baseType)
		{
			bool rebuild = false;

			foreach (FieldInfo baseInfo in baseType.GetFields (replaceableFlags)) {
				if (baseInfo.IsPrivate)
					continue;

				FieldInfo typeInfo = type.GetField (baseInfo.Name, replaceableFlags);

				if (typeInfo != null && typeInfo.DeclaringType == type) {
					partialNameOverride [typeInfo.Name] = true;
					rebuild = true;
				}
			}

			return rebuild;
		}

		internal bool CheckPartialBaseProperties (Type type, Type baseType)
		{
			bool rebuild = false;

			foreach (PropertyInfo baseInfo in baseType.GetProperties ()) {
				PropertyInfo typeInfo = type.GetProperty (baseInfo.Name);

				if (typeInfo != null && typeInfo.DeclaringType == type) {
					partialNameOverride [typeInfo.Name] = true;
					rebuild = true;
				}
			}

			return rebuild;
		}
#endif

		internal CodeDomProvider Provider {
			get { return provider; }
			set { provider = value; }
		}

		internal ICodeCompiler Compiler {
			get { return compiler; }
			set { compiler = value; }
		}		

		internal CompilerParameters CompilerParameters {
			get {
				if (compilerParameters == null)
					compilerParameters = new CompilerParameters ();
				
				return compilerParameters;
			}
			
			set { compilerParameters = value; }
		}

		internal CodeCompileUnit CompileUnit {
			get { return unit; }
		}

#if NET_2_0
		internal CodeTypeDeclaration DerivedType {
			get { return mainClass; }
		}

		internal CodeTypeDeclaration BaseType {
			get {
				if (partialClass == null)
					return DerivedType;
				return partialClass;
			}
		}
#endif

		internal TemplateParser Parser {
			get { return parser; }
		}
	}
}

