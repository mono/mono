#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using DbLinq.Util;

using Mono.Options;

namespace DbMetal
{
    [DebuggerDisplay("Parameters from {Provider}, server={Server}")]
    public class Parameters
    {
        /// <summary>
        /// user name for database access
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// user password for database access
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// server host name
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// database name
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// This connection string if present overrides User, Password, Server.
        /// Database is always used to generate the specific DataContext name
        /// </summary>
        public string Conn { get; set; }

        /// <summary>
        /// the namespace to put our classes into
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// the language to generate classes for
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// If present, write out C# code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// if present, write out DBML XML representing the DB
        /// </summary>
        public string Dbml { get; set; }

        /// <summary>
        /// when true, we will call Singularize()/Pluralize() functions.
        /// </summary>
        public bool Pluralize { get; set; }

        /// <summary>
        /// the culture used for word recognition and pluralization
        /// </summary>
        public string Culture { get; set; }

        /// <summary>
        /// load object renamings from an xml file
        /// </summary>
        public string Aliases { get; set; }

        /// <summary>
        /// this is the "input file" parameter
        /// </summary>
        public string SchemaXmlFile
        {
            get
            {
                return Extra.Count > 0 ? Extra[0] : null;
            }
        }

        public bool Schema { get; set; }

        /// <summary>
        /// base class from which all generated entities will inherit
        /// </summary>
        public string EntityBase { get; set; }

        /// <summary>
        /// interfaces to be implemented
        /// </summary>
        public string[] EntityInterfaces { get; set; }

        /// <summary>
        /// extra attributes to be implemented by class members
        /// </summary>
        public IList<string> MemberAttributes { get; set; }

        /// <summary>
        /// generate Equals() and GetHashCode()
        /// </summary>
        public bool GenerateEqualsHash { get; set; }

        /// <summary>
        /// export stored procedures
        /// </summary>
        public bool Sprocs { get; set; }

        /// <summary>
        /// preserve case of database names
        /// </summary>
        public string Case { get; set; }

        /// <summary>
        /// force a Console.ReadKey at end of program.
        /// Useful when running from Studio, so the output window does not disappear
        /// picrap comment: you may use the tool to write output to Visual Studio output window instead of a console window
        /// </summary>
        public bool Readline { get; set; }

        /// <summary>
        /// specifies a provider (which here is a pair or ISchemaLoader and IDbConnection implementors)
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// for fine tuning, we allow to specifiy an ISchemaLoader
        /// </summary>
        public string DbLinqSchemaLoaderProvider { get; set; }

        /// <summary>
        /// for fine tuning, we allow to specifiy an IDbConnection
        /// </summary>
        public string DatabaseConnectionProvider { get; set; }

        /// <summary>
        /// the SQL dialect used by the database
        /// </summary>
        public string SqlDialectType { get; set; }

        /// <summary>
        /// the types to be generated
        /// </summary>
        public IList<string> GenerateTypes { get; set; }

        /// <summary>
        /// if true, put a timestamp comment before the generated code
        /// </summary>
        public bool GenerateTimestamps { get; set; }

        /// <summary>
        /// show help
        /// </summary>
        public bool Help { get; set; }

        /// <summary>
        /// Show stack traces in error messages, etc., instead of just the message.
        /// </summary>
        public bool Debug { get; set; }

        /// <summary>
        /// non-option parameters
        /// </summary>
        public IList<string> Extra = new List<string>();

        TextWriter log;
        public TextWriter Log
        {
            get { return log ?? Console.Out; }
            set { log = value; }
        }


        protected OptionSet Options;

        public Parameters()
        {
            Schema = true;
            Culture = "en";
            GenerateTypes = new List<string>();
            MemberAttributes = new List<string>();
            GenerateTimestamps = true;
            EntityInterfaces = new []{ "INotifyPropertyChanging", "INotifyPropertyChanged" };
        }

        public void Parse(IList<string> args)
        {
            Options = new OptionSet() {
                 // SQLMetal compatible
                { "c|conn=",
                  "Database {CONNECTION STRING}. Cannot be used with /server, "
                  +"/user or /password options.",
                  conn => Conn = conn },
                 // SQLMetal compatible
                { "u|user=",
                  "Login user {NAME}.",
                  name => User = name },
                 // SQLMetal compatible
                { "p|password=",
                  "Login {PASSWORD}.",
                  password => Password = password },
                 // SQLMetal compatible
                { "s|server=",
                  "Database server {NAME}.",
                  name => Server = name },
                 // SQLMetal compatible
                { "d|database=",
                  "Database catalog {NAME} on server.",
                  name => Database = name },
                { "provider=",
                  "Specify {PROVIDER}. May be Ingres, MySql, Oracle, OracleODP, PostgreSql or Sqlite.",
                  provider => Provider = provider },
                { "with-schema-loader=",
                  "ISchemaLoader implementation {TYPE}.",
                  type => DbLinqSchemaLoaderProvider = type },
                { "with-dbconnection=",
                  "IDbConnection implementation {TYPE}.",
                  type => DatabaseConnectionProvider = type },
                { "with-sql-dialect=",
                  "IVendor implementation {TYPE}.",
                  type => SqlDialectType = type },
                 // SQLMetal compatible
                { "code=",
                  "Output as source code to {FILE}. Cannot be used with /dbml option.",
                  file => Code = file },
                 // SQLMetal compatible
                { "dbml=",
                  "Output as dbml to {FILE}. Cannot be used with /map option.",
                  file => Dbml = file },
                 // SQLMetal compatible
                { "language=",
                  "Language {NAME} for source code: C#, C#2 or VB "
                  +"(default: derived from extension on code file name).",
                  name => Language = name },
                { "aliases=",
                  "Use mapping {FILE}.",
                  file => Aliases = file },
                { "schema",
                  "Generate schema in code files (default: enabled).",
                  v => Schema = v != null },
                 // SQLMetal compatible
                { "namespace=",
                  "Namespace {NAME} of generated code (default: no namespace).",
                  name => Namespace = name },
                 // SQLMetal compatible
                { "entitybase=",
                  "Base {TYPE} of entity classes in the generated code "
                  +"(default: entities have no base class).",
                  type => EntityBase = type },
                { "member-attribute=",
                  "{ATTRIBUTE} for entity members in the generated code, "
                  +"can be specified multiple times.",
                  attribute => MemberAttributes.Add(attribute) },
                { "generate-type=",
                  "Generate only the {TYPE} selected, can be specified multiple times "
                  +"and does not prevent references from being generated (default: "
                  +"generate a DataContex subclass and all the entities in the schema).",
                  type => GenerateTypes.Add(type) },
                { "generate-equals-hash",
                  "Generates overrides for Equals() and GetHashCode() methods.",
                  v => GenerateEqualsHash = v != null },
                 // SQLMetal compatible
                { "sprocs",
                  "Extract stored procedures.",
                  v => Sprocs = v != null},
                 // SQLMetal compatible
                { "pluralize",
                  "Automatically pluralize or singularize class and member names "
                  +"using specified culture rules.",
                  v => Pluralize = v != null},
                { "culture=",
                  "Specify {CULTURE} for word recognition and pluralization (default: \"en\").",
                  culture => Culture = culture },
                { "case=",
                  "Transform names with the indicated {STYLE} "
                  +"(default: net; may be: leave, pascal, camel, net).",
                  style => Case = style },
                { "generate-timestamps",
                  "Generate timestampes in the generated code (default: enabled).",
                  v => GenerateTimestamps = v != null },
                { "readline",
                  "Wait for a key to be pressed after processing.",
                  v => Readline = v != null },
                { "debug",
                  "Enables additional information to help with debugging, " + 
                  "such as full stack traces in error messages.",
                  v => Debug = v != null },
                { "h|?|help",
                  "Show this help",
                  v => Help = v != null }
            };

            Extra = Options.Parse(args);
        }

        #region Help

        public void WriteHelp()
        {
            WriteHeader(); // includes a WriteLine()
            WriteSyntax();
            WriteLine();
            WriteSummary();
            WriteLine();
            Options.WriteOptionDescriptions(Log);
            WriteLine();
            WriteExamples();
        }

        bool headerWritten;

        /// <summary>
        /// Writes the application header
        /// </summary>
        public void WriteHeader()
        {
            if (!headerWritten)
            {
                WriteHeaderContents();
                WriteLine();
                headerWritten = true;
            }
        }

        protected void WriteHeaderContents()
        {
            var version = ApplicationVersion;
            Write("DbLinq Database mapping generator 2008 version {0}.{1}", version.Major, version.Minor);
            Write("for Microsoft (R) .NET Framework version 3.5");
            Write("Distributed under the MIT licence (http://linq.to/db/license)");
        }

        /// <summary>
        /// Writes a small summary
        /// </summary>
        public void WriteSummary()
        {
            Write("  Generates code and mapping for DbLinq. SqlMetal can:");
            Write("  - Generate source code and mapping attributes or a mapping file from a database.");
            Write("  - Generate an intermediate dbml file for customization from the database.");
            Write("  - Generate code and mapping attributes or mapping file from a dbml file.");
        }

        public void WriteSyntax()
        {
            var syntax = new StringBuilder();
            syntax.AppendFormat("{0} [OPTIONS] [<DBML INPUT FILE>]", ApplicationName);
            Write(syntax.ToString());
        }

        /// <summary>
        /// Writes examples
        /// </summary>
        public void WriteExamples()
        {
        }

        /// <summary>
        /// Outputs a formatted string to the console.
        /// We're not using the ILogger here, since we want console output.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Write(string format, params object[] args)
        {
            Output.WriteLine(Log, OutputLevel.Information, format, args);
        }

        /// <summary>
        /// Outputs an empty line
        /// </summary>
        public void WriteLine()
        {
            Output.WriteLine(Log, OutputLevel.Information, string.Empty);
        }

        /// <summary>
        /// Returns the application (assembly) name (without extension)
        /// </summary>
        protected static string ApplicationName
        {
            get
            {
                return Assembly.GetEntryAssembly().GetName().Name;
            }
        }

        /// <summary>
        /// Returns the application (assembly) version
        /// </summary>
        protected static Version ApplicationVersion
        {
            get
            {
                // Assembly.GetEntryAssembly() is null when loading from the
                // non-default AppDomain.
                var a = Assembly.GetEntryAssembly();
                return a != null ? a.GetName().Version : new Version();
            }
        }

        #endregion
    }
}
