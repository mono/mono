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
        /// SQLMetal compatible
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// user password for database access
        /// SQLMetal compatible
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// server host name
        /// SQLMetal compatible
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// database name
        /// SQLMetal compatible
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// This connection string if present overrides User, Password, Server.
        /// Database is always used to generate the specific DataContext name
        /// SQLMetal compatible
        /// </summary>
        public string Conn { get; set; }

        /// <summary>
        /// the namespace to put our classes into
        /// SQLMetal compatible
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// the language to generate classes for
        /// SQLMetal compatible
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// If present, write out C# code
        /// SQLMetal compatible
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// If present, write out DBML XML representing the DB
        /// SQLMetal compatible
        /// </summary>
        public string Dbml { get; set; }

        /// <summary>
        /// when true, we will call Singularize()/Pluralize() functions.
        /// SQLMetal compatible
        /// </summary>
        public bool Pluralize { get; set; }

        public string Culture { get; set; }

        /// <summary>
        /// Load object renamings from an xml file
        /// DbLinq specific
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
        /// SQLMetal compatible
        /// </summary>
        public string EntityBase { get; set; }

        /// <summary>
        /// Interfaces to be implemented
        /// </summary>
        public string[] EntityInterfaces { get; set; }

        /// <summary>
        /// Extra attributes to be implemented by class
        /// </summary>
        public string EntityAttributes { get; set; }
        public string[] EntityExposedAttributes { get { return GetArray(EntityAttributes); } }

        /// <summary>
        /// Extra attributes to be implemented by class
        /// </summary>
        public string MemberAttributes { get; set; }
        public string[] MemberExposedAttributes { get { return GetArray(MemberAttributes); } }

        /// <summary>
        /// base class from which all generated entities will inherit
        /// SQLMetal compatible
        /// </summary>
        public bool GenerateEqualsAndHash { get; set; }

        /// <summary>
        /// export stored procedures
        /// SQLMetal compatible
        /// </summary>
        public bool Sprocs { get; set; }

        /// <summary>
        /// preserve case of database names
        /// DbLinq specific
        /// </summary>
        public string Case { get; set; }

        bool useDomainTypes = true;

        /// <summary>
        /// if true, and PostgreSql database contains DOMAINS (typedefs), 
        /// we will generate code DbType='DerivedType'.
        /// if false, generate code DbType='BaseType'.
        /// DbLinq specific
        /// </summary>
        public bool UseDomainTypes
        {
            get { return useDomainTypes; }
            set { useDomainTypes = value; }
        }

        /// <summary>
        /// force a Console.ReadKey at end of program.
        /// Useful when running from Studio, so the output window does not disappear
        /// picrap comment: you may use the tool to write output to Visual Studio output window instead of a console window
        /// DbLinq specific
        /// </summary>
        public bool ReadLineAtExit { get; set; }

        /// <summary>
        /// specifies a provider (which here is a pair or ISchemaLoader and IDbConnection implementors)
        /// SQLMetal compatible
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// For fine tuning, we allow to specifiy an ISchemaLoader
        /// DbLinq specific
        /// </summary>
        public string DbLinqSchemaLoaderProvider { get; set; }

        /// <summary>
        /// For fine tuning, we allow to specifiy an IDbConnection
        /// DbLinq specific
        /// </summary>
        public string DatabaseConnectionProvider { get; set; }

        public string SqlDialectType { get; set; }

        public IList<string> GenerateTypes { get; set; }

        public bool GenerateTimestamps { get; set; }

        public bool Help { get; set; }

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
            GenerateTimestamps = true;
            EntityInterfaces = new []{ "INotifyPropertyChanging", "INotifyPropertyChanged" };
        }

        /// <summary>
        /// Converts a list separated by a comma to a string array
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public string[] GetArray(string list)
        {
            if (string.IsNullOrEmpty(list))
                return new string[0];
            return (from entityInterface in list.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    select entityInterface.Trim()).ToArray();
        }

        public void Parse(IList<string> args)
        {
            Options = new OptionSet() {
                { "c|conn=",
                  "Database {CONNECTION STRING}. Cannot be used with /server, "
                  +"/user or /password options.",
                  conn => Conn = conn },
                { "u|user=",
                  "Login user {NAME}.",
                  name => User = name },
                { "p|password=",
                  "Login {PASSWORD}.",
                  password => Password = password },
                { "s|server=",
                  "Database server {NAME}.",
                  name => Server = name },
                { "d|database=",
                  "Database catalog {NAME} on server.",
                  name => Database = name },
                { "provider=",
                  "Specify {PROVIDER}. May be Ingres, MySql, Oracle, OracleODP, PostgreSql or Sqlite.",
                  provider => Provider = provider },
                { "dbLinqSchemaLoaderProvider=",
                  "Specify a custom ISchemaLoader implementation {TYPE}.",
                  type => DbLinqSchemaLoaderProvider = type },
                { "databaseConnectionProvider=",
                  "Specify a custom IDbConnection implementation {TYPE}.",
                  type => DatabaseConnectionProvider = type },
                { "sqlDialectType=",
                  "The IVendor implementation {TYPE}.",
                  type => SqlDialectType = type },
                { "code=",
                  "Output as source code to {FILE}. Cannot be used with /dbml option.",
                  file => Code = file },
                { "dbml=",
                  "Output as dbml to {FILE}. Cannot be used with /map option.",
                  file => Dbml = file },
                { "language=",
                  "Language {NAME} for source code: C#, C#2 or VB "
                  +"(default: derived from extension on code file name).",
                  name => Language = name },
                { "aliases|renamesFile=",
                  "Use mapping {FILE}.",
                  file => Aliases = file },
                { "schema",
                  "Generate schema in code files (default='true').",
                  v => Schema = v != null},
                { "namespace=",
                  "Namespace {NAME} of generated code (default: no namespace).",
                  name => Namespace = name },
                { "entityBase=",
                  "Base {TYPE} of entity classes in the generated code "
                  +"(default: entities have no base class).",
                  type => EntityBase = type },
                { "entityAttributes=",
                  "Comma separated {ATTRIBUTE(S)} of entity classes in the generated code.",
                  attributes => EntityAttributes = attributes },
                { "memberAttributes=",
                  "Comma separated {ATTRIBUTE(S)} of entity members in the generated code.",
                  attributes => MemberAttributes = attributes },
                { "generate-type=",
                  "Generate only the {TYPE} selected, can be specified multiple times "
                  +"and does not prevent references from being generated (default: "
                  +"generate a DataContex subclass and all the entities in the schema).",
                  type => GenerateTypes.Add(type) },
                { "generateEqualsAndHash",
                  "Generates overrides for Equals() and GetHashCode() methods.",
                  v => GenerateEqualsAndHash = v != null},
                { "sprocs",
                  "Extract stored procedures.",
                  v => Sprocs = v != null},
                { "pluralize",
                  "Automatically pluralize or singularize class and member names "
                  +"using specified culture rules.",
                  v => Pluralize = v != null},
                { "culture=",
                  "Specify {CULTURE} for word recognition and pluralization (default=\"en\").",
                  culture => Culture = culture },
                { "case=",
                  "Transform names with the indicated {STYLE} "
                  +"(default: net; may be: leave, pascal, camel, net).",
                  style => Case = style },
                { "generate-timestamps",
                  "Generate timestampes in the generated code. True by default.",
                  v => GenerateTimestamps = v != null },
                { "readlineAtExit",
                  "Wait for a key to be pressed after processing.",
                  v => ReadLineAtExit = v != null },
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
