//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Compilation;
    using System.Web.RegularExpressions;
    using System.ServiceModel.Activation.Diagnostics;
    using System.Security;
    using System.Runtime.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    /// <summary>
    /// This class will parse the .svc file and maintains a list of useful information that the build
    /// provider needs in order to compile the file. The parser creates a list of dependent assemblies,
    /// understands the compiler that we need to use, fully parses all the supported directives etc.
    /// </summary>
    /// <remarks>
    /// The class is not thread-safe.
    /// </remarks>
#pragma warning disable 618 // have not moved to the v4 security model yet
    [SecurityCritical(SecurityCriticalScope.Everything)]
#pragma warning restore 618
    class ServiceParser
    {
        // the delimiter for the compiled custom string
        const string Delimiter = ServiceHostingEnvironment.ServiceParserDelimiter;

        // attribute names
        const string DefaultDirectiveName = "ServiceHost";
        const string FactoryAttributeName = "Factory";
        const string ServiceAttributeName = "Service";

        // regular exression for the directive
        readonly static SimpleDirectiveRegex directiveRegex;

        // the build provider we will work with 
        ServiceBuildProvider buildProvider;

        // text for the file
        string serviceText;

        // the class attribute value
        string factoryAttributeValue = string.Empty;

        // the constructorstring
        string serviceAttributeValue = string.Empty;

        // the line number in file currently being parsed
        int lineNumber;

        // the column number in file currently being parsed
        int startColumn;

        // the main directive was found or not
        bool foundMainDirective;

        // the type of the compiler (i.e C#)
        CompilerType compilerType;

        // the string containing the code to be compiled,
        // it will be null when all the code is "behind"
        string sourceString;

        // assemblies to be linked with, we need a unique list
        // of them and we maintain a Dictionary for it.
        HybridDictionary linkedAssemblies;

        // the set of assemblies that the build system is 
        // telling us we will be linked with. There is no unique
        // requirement for them.
        ICollection referencedAssemblies;

        // used to figure out where the new lines start
        static char[] newlineChars = new char[] { '\r', '\n' };

        // source file dependencies
        HybridDictionary sourceDependencies;

        // virtual path for the file that we are parsing
        string virtualPath;

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.AptcaMethodsShouldOnlyCallAptcaMethods, Justification = "Users cannot pass arbitrary data to this code.")]
        static ServiceParser()
        {
            directiveRegex = new SimpleDirectiveRegex();
        }

        /// <summary>
        /// The Contructor needs the path to the file that it will parse and a reference to
        /// the build provider that we are using. This is necessary because there are things that 
        /// need to be set on the build provider directly as we are parsing...
        /// </summary>
        internal ServiceParser(string virtualPath, ServiceBuildProvider buildProvider)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, TraceCode.WebHostCompilation, SR.TraceCodeWebHostCompilation,
                    new StringTraceRecord("VirtualPath", virtualPath), this, (Exception)null);
            }

            this.virtualPath = virtualPath;
            this.buildProvider = buildProvider;
        }

        /// <summary>
        /// Constructor that is used when the whole svc file content is provided. This is the case
        /// when the COM+ Admin tool calls into it.
        /// </summary>
        ServiceParser(string serviceText)
        {
            this.serviceText = serviceText;
            this.buildProvider = new ServiceBuildProvider();
        }

        /// <summary>
        /// Parsing the content of the service file and retrieve the serviceAttributeValue attribute for ComPlus.
        /// </summary>
        /// <param name="serviceText">The content of the service file.</param>
        /// <returns>The "serviceAttributeValue" attribute of the Service directive. </returns>
        /// <exception cref="System.Web.HttpParseException"/>
        internal static IDictionary<string, string> ParseServiceDirective(string serviceText)
        {
            ServiceParser parser = new ServiceParser(serviceText);
            parser.ParseString();

            // the list of valid attributes for ComPlus for Service Directive
            IDictionary<string, string> attributeTable = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);

            if (!string.IsNullOrEmpty(parser.factoryAttributeValue))
                attributeTable.Add(FactoryAttributeName, parser.factoryAttributeValue);

            if (!string.IsNullOrEmpty(parser.serviceAttributeValue))
                attributeTable.Add(ServiceAttributeName, parser.serviceAttributeValue);

            return attributeTable;
        }

        /// <summary>
        /// </summary>

        // various getters for private objects that the build
        // provider will need
        //
        internal CompilerType CompilerType
        {
            get
            {
                return compilerType;
            }
        }

        internal ICollection AssemblyDependencies
        {
            get
            {
                if (linkedAssemblies == null)
                {
                    return null;
                }

                return linkedAssemblies.Keys;
            }
        }

        internal ICollection SourceDependencies
        {
            get
            {
                if (sourceDependencies == null)
                {
                    return null;
                }

                return sourceDependencies.Keys;
            }
        }

        internal bool HasInlineCode
        {
            get
            {
                return (sourceString != null);
            }
        }

        /// <summary>
        /// Parses the code file appropriately. This method is used by the
        /// build provider.
        /// </summary>
        internal void Parse(ICollection referencedAssemblies)
        {
            if (referencedAssemblies == null)
            {
                throw FxTrace.Exception.ArgumentNull("referencedAssemblies");
            }

            this.referencedAssemblies = referencedAssemblies;
            AddSourceDependency(virtualPath);

            using (TextReader reader = buildProvider.OpenReaderInternal())
            {
                this.serviceText = reader.ReadToEnd();
                ParseString();
            }
        }

        /// <summary>
        /// This method returns a code compile unit that will be added
        /// to the other depdnecies in order to compile
        /// </summary>
        internal CodeCompileUnit GetCodeModel()
        {
            // Do we have something to compile?
            //
            if (sourceString == null || sourceString.Length == 0)
                return null;

            CodeSnippetCompileUnit snippetCompileUnit = new CodeSnippetCompileUnit(sourceString);

            // Put in some context so that the file can be debugged.
            //
            string pragmaFile = HostingEnvironmentWrapper.MapPath(virtualPath);
            snippetCompileUnit.LinePragma = new CodeLinePragma(pragmaFile, lineNumber);

            return snippetCompileUnit;
        }

        Exception CreateParseException(string message, string sourceCode)
        {
            return CreateParseException(message, null, sourceCode);
        }

        Exception CreateParseException(Exception innerException, string sourceCode)
        {
            return CreateParseException(innerException.Message, innerException, sourceCode);
        }

        Exception CreateParseException(string message, Exception innerException, string sourceCode)
        {
            return new HttpParseException(message, innerException, this.virtualPath, sourceCode, this.lineNumber);
        }

        /// <summary>
        /// This method returns the custom string that is to be passed to ServiceHostingEnvironment from BuildManager.
        /// </summary>
        /// <param name="compiledAssembly">The full name of the built assembly for inline code.</param>
        internal string CreateParseString(Assembly compiledAssembly)
        {
            Type typeToPreserve = this.GetCompiledType(compiledAssembly);
            string typeToPreserveName = string.Empty;
            if (typeToPreserve != null)
                typeToPreserveName = typeToPreserve.AssemblyQualifiedName;

            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            if (compiledAssembly != null)
            {
                builder.Append(Delimiter);
                builder.Append(compiledAssembly.FullName);
            }

            if (this.referencedAssemblies != null)
            {
                // CSDMain #192135
                // Minimize code change by doing 2 passes to have assembly containing type at the top of the list.
                // As a result, this assembly will get loaded first in ServiceHostFactory.CreateServiceHost.
                // In the multi-targetting scenario this prevents the runtime from trying to load a newer CLR assembly
                // and failing.  In the happy case, duplicate assembly references may occur (no effect on runtime).
                // Note that if the service type is contained in a framework assembly, this does not fix the problem.
                // Future improvement is to write fully qualified type name and let CLR handle load/search.
                if (!string.IsNullOrEmpty(serviceAttributeValue))
                {
                    foreach (Assembly assembly in this.referencedAssemblies)
                    {
                        Type serviceType;
                        try
                        {
                            serviceType = assembly.GetType(serviceAttributeValue, false);
                        }
                        catch (Exception e)
                        {
                            if (System.Runtime.Fx.IsFatal(e))
                            {
                                throw;
                            }

                            // log exception, but do not rethrow
                            DiagnosticUtility.TraceHandledException(e, TraceEventType.Warning);

                            break;
                        }

                        if (serviceType != null)
                        {
                            builder.Append(Delimiter);
                            builder.Append(assembly.FullName);
                            break;
                        }
                    }
                }

                foreach (Assembly assembly in this.referencedAssemblies)
                {
                    builder.Append(Delimiter);
                    builder.Append(assembly.FullName);
                }
            }

            if (this.AssemblyDependencies != null)
            {
                foreach (Assembly assembly in this.AssemblyDependencies)
                {
                    builder.Append(Delimiter);
                    builder.Append(assembly.FullName);
                }
            }
            // use application relative virtualpath instead of the absolute path 
            // so that the compliedcustomstring is applicationame independent
            return string.Concat(VirtualPathUtility.ToAppRelative(virtualPath), Delimiter,
                typeToPreserveName, Delimiter,
                serviceAttributeValue, builder.ToString());
        }

        void AddSourceDependency(string fileName)
        {
            if (sourceDependencies == null)
                sourceDependencies = new HybridDictionary(true);

            sourceDependencies.Add(fileName, fileName);
        }

        Type GetCompiledType(Assembly compiledAssembly)
        {
            if (string.IsNullOrEmpty(factoryAttributeValue))
            {
                return null;
            }

            Type type = null;

            // First, try to get the type from the assembly that has been built (if any)
            if (this.HasInlineCode && (compiledAssembly != null))
            {
                type = compiledAssembly.GetType(factoryAttributeValue);
            }

            // If not, try to get it from other assemblies
            if (type == null)
            {
                type = GetType(factoryAttributeValue);
            }

            return type;
        }

        internal IDictionary GetLinePragmasTable()
        {
            LinePragmaCodeInfo info = new LinePragmaCodeInfo(this.lineNumber, this.startColumn, 1, -1, false);
            IDictionary dictionary = new Hashtable();
            dictionary[this.lineNumber] = info;
            return dictionary;
        }

        /// <summary>
        /// Parses the content of the svc file for each directive line
        /// </summary>
        void ParseString()
        {
            try
            {
                int textPos = 0;
                Match match;
                lineNumber = 1;

                // Check for ending bracket first, MB 45013.
                if (this.serviceText.IndexOf('>') == -1)
                {
                    throw FxTrace.Exception.AsError(new HttpException(SR.Hosting_BuildProviderDirectiveEndBracketMissing(ServiceParser.DefaultDirectiveName)));
                }

                // First, parse all the <%@ ... %> directives
                //
                for (;;)
                {
                    match = directiveRegex.Match(this.serviceText, textPos);

                    // Done with the directives?
                    //
                    if (!match.Success)
                        break;

                    lineNumber += ServiceParserUtilities.LineCount(this.serviceText, textPos, match.Index);
                    textPos = match.Index;

                    // Get all the directives into a bag
                    //
                    IDictionary directive = CollectionsUtil.CreateCaseInsensitiveSortedList();
                    string directiveName = ProcessAttributes(match, directive);

                    // Understand the directive
                    //
                    ProcessDirective(directiveName, directive);
                    lineNumber += ServiceParserUtilities.LineCount(this.serviceText, textPos, match.Index + match.Length);
                    textPos = match.Index + match.Length;

                    // Fixup line and column numbers to have meaninglful errors
                    //
                    int newlineIndex = this.serviceText.LastIndexOfAny(newlineChars, textPos - 1);
                    startColumn = textPos - newlineIndex;
                }

                if (!foundMainDirective)
                {
                    throw FxTrace.Exception.AsError(new HttpException(SR.Hosting_BuildProviderDirectiveMissing(ServiceParser.DefaultDirectiveName)));
                }

                // skip the directives chunk
                //
                string remainingText = this.serviceText.Substring(textPos);

                // If there is something else in the file, it needs to be compiled
                //
                if (!ServiceParserUtilities.IsWhiteSpaceString(remainingText))
                {
                    sourceString = remainingText;
                }
            }
            catch (HttpException e)
            {
                // the string is set in the internal exception, no need to set it again.
                //
                Exception parseException = CreateParseException(e, this.serviceText);
                throw FxTrace.Exception.AsError(
                    new HttpCompileException(parseException.Message, parseException));
            }
        }

        /// <summary>
        /// Return the directive if it exists or an empty string
        /// </summary>
        string ProcessAttributes(Match match, IDictionary attribs)
        {
            // creates 3 parallel capture collections
            // for the attribute names, the attribute values and the
            // equal signs
            //
            string ret = String.Empty;

            CaptureCollection attrnames = match.Groups["attrname"].Captures;
            CaptureCollection attrvalues = match.Groups["attrval"].Captures;
            CaptureCollection equalsign = match.Groups["equal"].Captures;

            // Iterate through all of them and add then to 
            // the dictionary of attributes
            //
            for (int i = 0; i < attrnames.Count; i++)
            {
                string attribName = attrnames[i].ToString();
                string attribValue = attrvalues[i].ToString();

                // Check if there is an equal sign.
                //
                bool fHasEqual = (equalsign[i].ToString().Length > 0);

                if (attribName != null)
                {
                    // A <%@ %> block can have two formats:
                    // <%@ directive foo=1 bar=hello %>
                    // <%@ foo=1 bar=hello %>
                    // Check if we have the first format
                    //
                    if (!fHasEqual && i == 0)
                    {
                        // return the main directive
                        //
                        ret = attribName;
                        continue;
                    }

                    try
                    {
                        if (attribs != null)
                            attribs.Add(attribName, attribValue);
                    }
                    catch (ArgumentException)
                    {
                        throw FxTrace.Exception.AsError(new HttpException(SR.Hosting_BuildProviderDuplicateAttribute(attribName)));
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// This method understands the compilation parameters if any ...
        /// </summary>
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotIndirectlyExposeMethodsWithLinkDemands, Justification = "This method doesn't allow callers to access sensitive information, operations, or resources that can be used in a destructive manner.")]
        void ProcessCompilationParams(IDictionary directive, CompilerParameters compilParams)
        {
            bool debug = false;
            if (ServiceParserUtilities.GetAndRemoveBooleanAttribute(directive, "debug", ref debug))
            {
                compilParams.IncludeDebugInformation = debug;
            }

            int warningLevel = 0;
            if (ServiceParserUtilities.GetAndRemoveNonNegativeIntegerAttribute(directive, "warninglevel", ref warningLevel))
            {
                compilParams.WarningLevel = warningLevel;
                if (warningLevel > 0)
                    compilParams.TreatWarningsAsErrors = true;
            }

            string compilerOptions = ServiceParserUtilities.GetAndRemoveNonEmptyAttribute(directive, "compileroptions");
            if (compilerOptions != null)
            {
                compilParams.CompilerOptions = compilerOptions;
            }
        }


        /// <summary>
        /// Processes a directive block
        /// </summary>
        void ProcessDirective(string directiveName, IDictionary directive)
        {
            // Throw on empy, no directive specified
            //
            if (directiveName.Length == 0)
            {
                throw FxTrace.Exception.AsError(new HttpException(SR.Hosting_BuildProviderDirectiveNameMissing));
            }

            // Check for the main directive
            //
            if (string.Compare(directiveName, ServiceParser.DefaultDirectiveName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                // Make sure the main directive was not already specified
                //
                if (foundMainDirective)
                {
                    throw FxTrace.Exception.AsError(new HttpException(SR.Hosting_BuildProviderDuplicateDirective(ServiceParser.DefaultDirectiveName)));
                }

                foundMainDirective = true;

                // Ignore 'codebehind' attribute (ASURT 4591)
                //
                directive.Remove("codebehind");

                string language = ServiceParserUtilities.GetAndRemoveNonEmptyAttribute(directive, "language");

                // Get the compiler for the specified language (if any)
                // or get the one from config
                //
                if (language != null)
                {
                    compilerType = buildProvider.GetDefaultCompilerTypeForLanguageInternal(language);
                }
                else
                {
                    compilerType = buildProvider.GetDefaultCompilerTypeInternal();
                }


                if (directive.Contains(FactoryAttributeName))
                {
                    factoryAttributeValue = ServiceParserUtilities.GetAndRemoveNonEmptyAttribute(directive, FactoryAttributeName);
                    serviceAttributeValue = ServiceParserUtilities.GetAndRemoveNonEmptyAttribute(directive, ServiceAttributeName);
                }
                else if (directive.Contains(ServiceAttributeName))
                {
                    serviceAttributeValue = ServiceParserUtilities.GetAndRemoveNonEmptyAttribute(directive, ServiceAttributeName);
                }
                else
                {
                    throw FxTrace.Exception.AsError(new HttpException(SR.Hosting_BuildProviderMainAttributeMissing));
                }
                // parse the parameters that are related to the compiler
                //
                ProcessCompilationParams(directive, compilerType.CompilerParameters);
            }
            else if (string.Compare(directiveName, "assembly", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (directive.Contains("name") && directive.Contains("src"))
                {
                    throw FxTrace.Exception.AsError(new HttpException(SR.Hosting_BuildProviderMutualExclusiveAttributes("src", "name")));
                }
                else if (directive.Contains("name"))
                {
                    string assemblyName = ServiceParserUtilities.GetAndRemoveNonEmptyAttribute(directive, "name");
                    if (assemblyName != null)
                    {
                        AddAssemblyDependency(assemblyName);
                    }
                    else
                        throw FxTrace.Exception.AsError(new HttpException(SR.Hosting_BuildProviderAttributeEmpty("name")));
                }
                else if (directive.Contains("src"))
                {
                    string srcPath = ServiceParserUtilities.GetAndRemoveNonEmptyAttribute(directive, "src");
                    if (srcPath != null)
                    {
                        ImportSourceFile(srcPath);
                    }
                    else
                        throw FxTrace.Exception.AsError(new HttpException(SR.Hosting_BuildProviderAttributeEmpty("src")));
                }
                else
                { // if (!directive.Contains("name") && !directive.Contains("src"))
                    throw FxTrace.Exception.AsError(new HttpException(SR.Hosting_BuildProviderRequiredAttributesMissing("src", "name")));
                }
            }
            else
            {
                throw FxTrace.Exception.AsError(new HttpException(SR.Hosting_BuildProviderUnknownDirective(directiveName)));
            }

            // check if there are any directives that you did not process 
            //
            if (directive.Count > 0)
                throw FxTrace.Exception.AsError(new HttpException(SR.Hosting_BuildProviderUnknownAttribute(ServiceParserUtilities.FirstDictionaryKey(directive))));
        }

        void ImportSourceFile(string path)
        {
            // Get a full path to the source file, compile it to an assembly
            // add the depedency to the assembly
            //
            string baseVirtualDir = VirtualPathUtility.GetDirectory(virtualPath);
            string fullVirtualPath = VirtualPathUtility.Combine(baseVirtualDir, path);

            AddSourceDependency(fullVirtualPath);
            Assembly a = BuildManager.GetCompiledAssembly(fullVirtualPath);
            AddAssemblyDependency(a);
        }

        void AddAssemblyDependency(string assemblyName)
        {
            // Load and keep track of the assembly
            //
            Assembly a = Assembly.Load(assemblyName);
            AddAssemblyDependency(a);
        }

        void AddAssemblyDependency(Assembly assembly)
        {
            if (linkedAssemblies == null)
                linkedAssemblies = new HybridDictionary(false);

            linkedAssemblies.Add(assembly, null);
        }

        /// <summary>
        /// Look for a type by name in the assemblies available to this page
        /// </summary>
        Type GetType(string typeName)
        {
            Type type;

            // If it contains an assembly name, just call Type.GetType (ASURT 53589)
            //
            if (ServiceParserUtilities.TypeNameIncludesAssembly(typeName))
            {
                try
                {
                    type = Type.GetType(typeName, true);
                }
                catch (ArgumentException e)
                {
                    Exception parseException = CreateParseException(e, this.sourceString);
                    throw FxTrace.Exception.AsError(
                        new HttpCompileException(parseException.Message, parseException));
                }
                catch (TargetInvocationException e)
                {
                    Exception parseException = CreateParseException(e, this.sourceString);
                    throw FxTrace.Exception.AsError(
                        new HttpCompileException(parseException.Message, parseException));
                }
                catch (TypeLoadException e)
                {
                    Exception parseException = CreateParseException(SR.Hosting_BuildProviderCouldNotCreateType(typeName), e, this.sourceString);
                    throw FxTrace.Exception.AsError(
                        new HttpCompileException(parseException.Message, parseException));
                }

                return type;
            }

            try
            {
                type = ServiceParserUtilities.GetTypeFromAssemblies(referencedAssemblies, typeName, false /*ignoreCase*/);
                if (type != null)
                    return type;

                type = ServiceParserUtilities.GetTypeFromAssemblies(AssemblyDependencies, typeName, false /*ignoreCase*/);
                if (type != null)
                    return type;
            }
            catch (HttpException e)
            {
                Exception parseException = CreateParseException(SR.Hosting_BuildProviderCouldNotCreateType(typeName), e, this.sourceString);
                throw FxTrace.Exception.AsError(
                        new HttpCompileException(parseException.Message, parseException));
            }

            Exception exception = CreateParseException(SR.Hosting_BuildProviderCouldNotCreateType(typeName), this.sourceString);
            throw FxTrace.Exception.AsError(
                        new HttpCompileException(exception.Message, exception));
        }

        /// <summary>
        /// This class contains static methods that are necessary to manipulate the 
        /// structures that contain the directives. The logic assumes that the parser will
        /// create a dictionary that contains all the directives and we can pull certain directives as
        /// necessary while processing/compiling the page. The directives are strings.
        /// 
        /// </summary>
        static class ServiceParserUtilities
        {
            /// <summary>
            /// Return the first key of the dictionary as a string.  Throws if it's
            /// empty or if the key is not a string.
            /// </summary>
            internal static string FirstDictionaryKey(IDictionary dictionary)
            {
                // assume that the caller has checked the dictionary before calling
                //
                IDictionaryEnumerator e = dictionary.GetEnumerator();
                e.MoveNext();
                return (string)e.Key;
            }

            /// <summary>
            /// Get a string value from a dictionary, and remove 
            /// it from the dictionary of attributes if it exists.
            /// </summary>
            /// <remarks>Returns null if the value was not there ...</remarks>
            static string GetAndRemove(IDictionary dictionary, string key)
            {
                string val = (string)dictionary[key];

                if (val != null)
                {
                    dictionary.Remove(key);
                    val = val.Trim();
                }
                else
                    return string.Empty;

                return val;
            }

            /// <summary>
            /// Get a value from a dictionary, and remove it from the dictionary if
            /// it exists.  Throw an exception if the value is a whitespace string.
            /// However, don't complain about null, which simply means the value is not
            /// in the dictionary.
            /// </summary>
            internal static string GetAndRemoveNonEmptyAttribute(IDictionary directives, string key, bool required)
            {
                string val = ServiceParserUtilities.GetAndRemove(directives, key);

                if (val.Length == 0)
                {
                    if (required)
                    {
                        throw FxTrace.Exception.AsError(new HttpException(SR.Hosting_BuildProviderAttributeMissing(key)));
                    }
                    return null;
                }

                return val;
            }


            internal static string GetAndRemoveNonEmptyAttribute(IDictionary directives, string key)
            {
                return GetAndRemoveNonEmptyAttribute(directives, key, false /*required*/);
            }

            /// <summary>
            /// Get a string value from a dictionary, and convert it to bool.  Throw an
            /// exception if it's not a valid bool string.
            /// However, don't complain about null, which simply means the value is not
            /// in the dictionary.
            /// The value is returned through a REF param (unchanged if null)
            /// </summary>
            /// <returns>True if attrib exists, false otherwise</returns>
            internal static bool GetAndRemoveBooleanAttribute(IDictionary directives, string key, ref bool val)
            {
                string s = ServiceParserUtilities.GetAndRemove(directives, key);

                if (s.Length == 0)
                    return false;

                try
                {
                    val = bool.Parse(s);
                }
                catch (FormatException)
                {
                    throw FxTrace.Exception.AsError(new HttpException(SR.Hosting_BuildProviderInvalidValueForBooleanAttribute(s, key)));
                }

                return true;
            }

            /// <summary>
            /// Get a string value from a dictionary, and convert it to integer.  Throw an
            /// exception if it's not a valid positive integer string.
            /// However, don't complain about null, which simply means the value is not
            /// in the dictionary.
            /// The value is returned through a REF param (unchanged if null)
            /// </summary>
            /// <returns>True if attrib exists, false otherwise</returns>
            internal static bool GetAndRemoveNonNegativeIntegerAttribute(IDictionary directives, string key, ref int val)
            {
                string s = ServiceParserUtilities.GetAndRemove(directives, key);

                if (s.Length == 0)
                    return false;

                val = GetNonNegativeIntegerAttribute(key, s);
                return true;
            }

            /// <summary>
            /// Parse a string attribute into a non-negative integer
            /// </summary>
            /// <param name="name">Name of the attribute, used only for the error messages</param>
            /// <param name="value">Value to convert to int</param>
            static int GetNonNegativeIntegerAttribute(string name, string value)
            {
                int ret;

                try
                {
                    ret = int.Parse(value, CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {
                    throw FxTrace.Exception.AsError(new HttpException(SR.Hosting_BuildProviderInvalidValueForNonNegativeIntegerAttribute(value, name)));
                }

                // Make sure it's not negative
                //
                if (ret < 0)
                {
                    throw FxTrace.Exception.AsError(new HttpException(SR.Hosting_BuildProviderInvalidValueForNonNegativeIntegerAttribute(value, name)));
                }

                return ret;
            }

            internal static bool IsWhiteSpaceString(string s)
            {
                return (s.Trim().Length == 0);
            }

            /// <summary>
            /// This method takes the code that will be compiled as a string and it 
            /// will count how many lines exist between the given offset and the final
            /// offset. 
            /// </summary>
            /// <param name="text">The text that contains the source code</param>
            /// <param name="offset">Starting offset for lookup</param>
            /// <param name="newoffset">Ending offset</param>
            /// <returns>The number of lines</returns>
            internal static int LineCount(string text, int offset, int newoffset)
            {
                int linecount = 0;

                while (offset < newoffset)
                {
                    if (text[offset] == '\r' || (text[offset] == '\n' && (offset == 0 || text[offset - 1] != '\r')))
                        linecount++;

                    offset++;
                }

                return linecount;
            }

            /// <summary>
            /// Parses a string that contains a type trying to figure out if the assembly info is there.
            /// </summary>
            /// <param name="typeName">The string to search</param>
            internal static bool TypeNameIncludesAssembly(string typeName)
            {
                return (typeName.IndexOf(",", StringComparison.Ordinal) >= 0);
            }

            /// <summary>
            /// Loops through a list of assemblies that are already collected by the parser/provider and 
            /// looks for the specified type.
            /// </summary>
            /// <param name="assemblies">The collection of assemblies</param>
            /// <param name="typeName">The type name</param>
            /// <param name="ignoreCase">Case sensitivity knob</param>
            /// <returns></returns>
            internal static Type GetTypeFromAssemblies(ICollection assemblies, string typeName, bool ignoreCase)
            {
                if (assemblies == null)
                    return null;

                Type type = null;

                foreach (Assembly assembly in assemblies)
                {
                    Type t = assembly.GetType(typeName, false /*throwOnError*/, ignoreCase);

                    if (t == null)
                        continue;

                    // If we had already found a different one, it's an ambiguous type reference
                    //
                    if (type != null && t != type)
                    {
                        throw FxTrace.Exception.AsError(new HttpException(
                            SR.Hosting_BuildProviderAmbiguousType(typeName, type.Assembly.FullName, t.Assembly.FullName)));
                    }

                    // Keep track of it
                    //
                    type = t;
                }

                return type;
            }
        }
    }
}
