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

namespace DbMetal
{
    [DebuggerDisplay("Parameters from {Provider}, server={Server}")]
    public class Parameters : AbstractParameters
    {
        /// <summary>
        /// user name for database access
        /// SQLMetal compatible
        /// </summary>
        [Option("Login user ID.", ValueName = "name", Group = 1)]
        public string User { get; set; }

        /// <summary>
        /// user password for database access
        /// SQLMetal compatible
        /// </summary>
        [Option("Login password.", ValueName = "password", Group = 1)]
        public string Password { get; set; }

        /// <summary>
        /// server host name
        /// SQLMetal compatible
        /// </summary>
        [Option("Database server name.", ValueName = "name", Group = 1)]
        public string Server { get; set; }

        /// <summary>
        /// database name
        /// SQLMetal compatible
        /// </summary>
        [Option("Database catalog on server.", ValueName = "name", Group = 1)]
        public string Database { get; set; }

        /// <summary>
        /// This connection string if present overrides User, Password, Server.
        /// Database is always used to generate the specific DataContext name
        /// SQLMetal compatible
        /// </summary>
        [Option("Database connection string. Cannot be used with /server, /user or /password options.",
            ValueName = "connection string", Group = 1)]
        public string Conn { get; set; }

        /// <summary>
        /// the namespace to put our classes into
        /// SQLMetal compatible
        /// </summary>
        [Option("Namespace of generated code (default: no namespace).", ValueName = "name", Group = 4)]
        public string Namespace { get; set; }

        /// <summary>
        /// the language to generate classes for
        /// SQLMetal compatible
        /// </summary>
        [Option("Language for source code: C#, C#2 or VB (default: derived from extension on code file name).", ValueName = "name", Group = 4)]
        public string Language { get; set; }

        /// <summary>
        /// If present, write out C# code
        /// SQLMetal compatible
        /// </summary>
        [Option("Output as source code. Cannot be used with /dbml option.", ValueName = "file", Group = 3)]
        public string Code { get; set; }

        /// <summary>
        /// If present, write out DBML XML representing the DB
        /// SQLMetal compatible
        /// </summary>
        [Option("Output as dbml. Cannot be used with /map option.", ValueName = "file", Group = 3)]
        public string Dbml { get; set; }

        /// <summary>
        /// when true, we will call Singularize()/Pluralize() functions.
        /// SQLMetal compatible
        /// </summary>
        [Option("Automatically pluralize or singularize class and member names using specified culture rules.", Group = 4)]
        public bool Pluralize { get; set; }

        [Option("Specify culture for word recognition and pluralization (default=\"en\").", Group = 4)]
        public string Culture { get; set; }

        /// <summary>
        /// Load object renamings from an xml file
        /// DbLinq specific
        /// </summary>
        [Option("Use mapping file.", ValueName = "file", Group = 3)]
        [Alternate("renamesFile")]
        public string Aliases { get; set; }

        /// <summary>
        /// this is the "input file" parameter
        /// </summary>
        [File("input file", "DBML input file.")]
        public string SchemaXmlFile
        {
            get
            {
                return Extra.Count > 0 ? Extra[0] : null;
            }
        }

        [Option("Generate schema in code files (default='true').", Group = 4)]
        public bool Schema { get; set; }

        /// <summary>
        /// base class from which all generated entities will inherit
        /// SQLMetal compatible
        /// </summary>
        [Option("Base class of entity classes in the generated code (default: entities have no base class).",
            ValueName = "type", Group = 4)]
        public string EntityBase { get; set; }

        /// <summary>
        /// Interfaces to be implemented
        /// </summary>
        [Option("Comma separated base interfaces of entity classes in the generated code (default: entities implement INotifyPropertyChanged).",
            ValueName = "interface(s)", Group = 4)]
        public string EntityInterfaces { get; set; }
        public string[] EntityImplementedInterfaces { get { return GetArray(EntityInterfaces); } }

        /// <summary>
        /// Extra attributes to be implemented by class
        /// </summary>
        [Option("Comma separated attributes of entity classes in the generated code.",
            ValueName = "attribute(s)", Group = 4)]
        public string EntityAttributes { get; set; }
        public string[] EntityExposedAttributes { get { return GetArray(EntityAttributes); } }

        /// <summary>
        /// Extra attributes to be implemented by class
        /// </summary>
        [Option("Comma separated attributes of entity members in the generated code.",
            ValueName = "attribute(s)", Group = 4)]
        public string MemberAttributes { get; set; }
        public string[] MemberExposedAttributes { get { return GetArray(MemberAttributes); } }

        /// <summary>
        /// base class from which all generated entities will inherit
        /// SQLMetal compatible
        /// </summary>
        [Option("Generates overrides for Equals() and GetHashCode() methods.", Group = 4)]
        public bool GenerateEqualsAndHash { get; set; }

        /// <summary>
        /// export stored procedures
        /// SQLMetal compatible
        /// </summary>
        [Option("Extract stored procedures.", Group = 2)]
        public bool Sprocs { get; set; }

        /// <summary>
        /// ??
        /// DbLinq specific
        /// </summary>
        public bool VerboseForeignKeys { get; set; }

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
        [Option("Wait for a key to be pressed after processing.", Group = 4)]
        public bool ReadLineAtExit { get; set; }

        /// <summary>
        /// specifies a provider (which here is a pair or ISchemaLoader and IDbConnection implementors)
        /// SQLMetal compatible
        /// </summary>
        [Option("Specify provider. May be Ingres, MySql, Oracle, OracleODP, PostgreSql or Sqlite.",
            ValueName = "provider", Group = 1)]
        public string Provider { get; set; }

        /// <summary>
        /// For fine tuning, we allow to specifiy an ISchemaLoader
        /// DbLinq specific
        /// </summary>
        [Option("Specify a custom ISchemaLoader implementation type.", ValueName = "type", Group = 1)]
        public string DbLinqSchemaLoaderProvider { get; set; }

        /// <summary>
        /// For fine tuning, we allow to specifiy an IDbConnection
        /// DbLinq specific
        /// </summary>
        [Option("Specify a custom IDbConnection implementation type.", ValueName = "type", Group = 1)]
        public string DatabaseConnectionProvider { get; set; }

        public Parameters()
        {
            Schema = true;
            Culture = "en";
            EntityInterfaces = "INotifyPropertyChanged";//INotifyPropertyChanging INotifyPropertyChanged IModified
        }

        public IEnumerable<Parameters> GetBatch(IList<string> args)
        {
            return GetParameterBatch<Parameters>(args);
        }

        #region Help

        protected override void WriteHeaderContents()
        {
            var version = ApplicationVersion;
            Write("DbLinq Database mapping generator 2008 version {0}.{1}", version.Major, version.Minor);
            Write("for Microsoft (R) .NET Framework version 3.5");
            Write("Distributed under the MIT licence (http://linq.to/db/license)");
        }

        public override void WriteSummary()
        {
            Write("  Generates code and mapping for DbLinq. SqlMetal can:");
            Write("  - Generate source code and mapping attributes or a mapping file from a database.");
            Write("  - Generate an intermediate dbml file for customization from the database.");
            Write("  - Generate code and mapping attributes or mapping file from a dbml file.");
        }

        #endregion
    }
}
