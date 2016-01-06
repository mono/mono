// ***********************************************************************
// Copyright (c) 2007 Charlie Poole
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
// ***********************************************************************

using System;
using System.IO;
using System.Text;
using System.Collections;
#if CLR_2_0 || CLR_4_0
using System.Collections.Generic;
#endif
using NUnit.Framework;

namespace NUnitLite.Runner
{
    /// <summary>
    /// The CommandLineOptions class parses and holds the values of
    /// any options entered at the command line.
    /// </summary>
    public class CommandLineOptions
    {
        private string optionChars;
        private static string NL = NUnit.Env.NewLine;

        private bool wait = false;
        private bool noheader = false;
        private bool help = false;
        private bool full = false;
        private bool explore = false;
        private bool labelTestsInOutput = false;

        private string exploreFile;
        private string resultFile;
        private string resultFormat;
        private string outFile;
        private string includeCategory;
        private string excludeCategory;

        private bool error = false;

        private StringList tests = new StringList();
        private StringList invalidOptions = new StringList();
        private StringList parameters = new StringList();

        private int randomSeed = -1;

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the 'wait' option was used.
        /// </summary>
        public bool Wait
        {
            get { return wait; }
        }

        /// <summary>
        /// Gets a value indicating whether the 'nologo' option was used.
        /// </summary>
        public bool NoHeader
        {
            get { return noheader; }
        }

        /// <summary>
        /// Gets a value indicating whether the 'help' option was used.
        /// </summary>
        public bool ShowHelp
        {
            get { return help; }
        }

        /// <summary>
        /// Gets a list of all tests specified on the command line
        /// </summary>
        public string[] Tests
        {
            get { return (string[])tests.ToArray(); }
        }

        /// <summary>
        /// Gets a value indicating whether a full report should be displayed
        /// </summary>
        public bool Full
        {
            get { return full; }
        }

        /// <summary>
        /// Gets a value indicating whether tests should be listed
        /// rather than run.
        /// </summary>
        public bool Explore
        {
            get { return explore; }
        }

        /// <summary>
        /// Gets the name of the file to be used for listing tests
        /// </summary>
        public string ExploreFile
        {
            get { return exploreFile; }
        }

        /// <summary>
        /// Gets the name of the file to be used for test results
        /// </summary>
        public string ResultFile
        {
            get { return resultFile; }
        }

        /// <summary>
        /// Gets the format to be used for test results
        /// </summary>
        public string ResultFormat
        {
            get { return resultFormat; }
        }

        /// <summary>
        /// Gets the full path of the file to be used for output
        /// </summary>
        public string OutFile
        {
            get 
            {
                return outFile;
            }
        }

        /// <summary>
        /// Gets the list of categories to include
        /// </summary>
        public string Include
        {
            get
            {
                return includeCategory;
            }
        }

        /// <summary>
        /// Gets the list of categories to exclude
        /// </summary>
        public string Exclude
        {
            get
            {
                return excludeCategory;
            }
        }

        /// <summary>
        /// Gets a flag indicating whether each test should
        /// be labeled in the output.
        /// </summary>
        public bool LabelTestsInOutput
        {
            get { return labelTestsInOutput; }
        }

        private string ExpandToFullPath(string path)
        {
            if (path == null) return null;

#if NETCF
            return Path.Combine(NUnit.Env.DocumentFolder, path);
#else
            return Path.GetFullPath(path); 
#endif
        }

        /// <summary>
        /// Gets the test count
        /// </summary>
        public int TestCount
        {
            get { return tests.Count; }
        }

        /// <summary>
        /// Gets the seed to be used for generating random values
        /// </summary>
        public int InitialSeed
        {
            get 
            {
                if (randomSeed < 0)
                    randomSeed = new Random().Next();

                return randomSeed; 
            }
        }

        #endregion

        /// <summary>
        /// Construct a CommandLineOptions object using default option chars
        /// </summary>
        public CommandLineOptions()
        {
            this.optionChars = System.IO.Path.DirectorySeparatorChar == '/' ? "-" : "/-";
        }

        /// <summary>
        /// Construct a CommandLineOptions object using specified option chars
        /// </summary>
        /// <param name="optionChars"></param>
        public CommandLineOptions(string optionChars)
        {
            this.optionChars = optionChars;
        }

        /// <summary>
        /// Parse command arguments and initialize option settings accordingly
        /// </summary>
        /// <param name="args">The argument list</param>
        public void Parse(params string[] args)
        {
            foreach( string arg in args )
            {
                if (optionChars.IndexOf(arg[0]) >= 0 )
                    ProcessOption(arg);
                else
                    ProcessParameter(arg);
            }
        }

        /// <summary>
        ///  Gets the parameters provided on the commandline
        /// </summary>
        public string[] Parameters
        {
            get { return (string[])parameters.ToArray(); }
        }

        private void ProcessOption(string option)
        {
            string opt = option;
            int pos = opt.IndexOfAny( new char[] { ':', '=' } );
            string val = string.Empty;

            if (pos >= 0)
            {
                val = opt.Substring(pos + 1);
                opt = opt.Substring(0, pos);
            }

            switch (opt.Substring(1))
            {
                case "wait":
                    wait = true;
                    break;
                case "noheader":
                case "noh":
                    noheader = true;
                    break;
                case "help":
                case "h":
                    help = true;
                    break;
                case "test":
                    tests.Add(val);
                    break;
                case "full":
                    full = true;
                    break;
                case "explore":
                    explore = true;
                    if (val == null || val.Length == 0)
                        val = "tests.xml";
                    try
                    {
                        exploreFile = ExpandToFullPath(val);
                    }
                    catch
                    {
                        InvalidOption(option);
                    }
                    break;
                case "result":
                    if (val == null || val.Length == 0)
                        val = "TestResult.xml";
                    try
                    {
                        resultFile = ExpandToFullPath(val);
                    }
                    catch
                    {
                        InvalidOption(option);
                    }
                    break;
                case "format":
                    resultFormat = val;
                    if (resultFormat != "nunit3" && resultFormat != "nunit2")
                        InvalidOption(option);
                    break;
                case "out":
                    try
                    {
                        outFile = ExpandToFullPath(val);
                    }
                    catch
                    {
                        InvalidOption(option);
                    }
                    break;
                case "labels":
                    labelTestsInOutput = true;
                    break;
                case "include":
                    includeCategory = val;
                    break;
                case "exclude":
                    excludeCategory = val;
                    break;
                case "seed":
                    try
                    {
                        randomSeed = int.Parse(val);
                    }
                    catch
                    {
                        InvalidOption(option);
                    }
                    break;
                default:
                    InvalidOption(option);
                    break;
            }
        }

        private void InvalidOption(string option)
        {
            error = true;
            invalidOptions.Add(option);
        }

        private void ProcessParameter(string param)
        {
            parameters.Add(param);
        }

        /// <summary>
        /// Gets a value indicating whether there was an error in parsing the options.
        /// </summary>
        /// <value><c>true</c> if error; otherwise, <c>false</c>.</value>
        public bool Error
        {
            get { return error; }
        }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <value>The error message.</value>
        public string ErrorMessage
        {
            get 
            {
                StringBuilder sb = new StringBuilder();
                foreach (string opt in invalidOptions)
                    sb.Append( "Invalid option: " + opt + NL );
                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets the help text.
        /// </summary>
        /// <value>The help text.</value>
        public string HelpText
        {
            get
            {
                StringBuilder sb = new StringBuilder();

#if PocketPC || WindowsCE || NETCF || SILVERLIGHT
                string name = "NUnitLite";
#else
                string name = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
#endif

                sb.Append("Usage: " + name + " [assemblies] [options]" + NL + NL);
                sb.Append("Runs a set of NUnitLite tests from the console." + NL + NL);
                sb.Append("You may specify one or more test assemblies by name, without a path or" + NL);
                sb.Append("extension. They must be in the same in the same directory as the exe" + NL);
                sb.Append("or on the probing path. If no assemblies are provided, tests in the" + NL);
                sb.Append("executing assembly itself are run." + NL + NL);
                sb.Append("Options:" + NL);
                sb.Append("  -test:testname  Provides the name of a test to run. This option may be" + NL);
                sb.Append("                  repeated. If no test names are given, all tests are run." + NL + NL);
                sb.Append("  -out:FILE       File to which output is redirected. If this option is not" + NL);
                sb.Append("                  used, output is to the Console, which means it is lost" + NL);
                sb.Append("                  on devices without a Console." + NL + NL);
                sb.Append("  -full           Prints full report of all test results." + NL + NL);
                sb.Append("  -result:FILE    File to which the xml test result is written." + NL + NL);
                sb.Append("  -format:FORMAT  Format in which the result is to be written. FORMAT must be" + NL);
                sb.Append("                  either nunit3 or nunit2. The default is nunit3." + NL + NL);
                sb.Append("  -explore:FILE  If provided, this option indicates that the tests" + NL);
                sb.Append("                  should be listed rather than executed. They are listed" + NL);
                sb.Append("                  to the specified file in XML format." + NL);
                sb.Append("  -help,-h        Displays this help" + NL + NL);
                sb.Append("  -noheader,-noh  Suppresses display of the initial message" + NL + NL);
                sb.Append("  -labels         Displays the name of each test when it starts" + NL + NL);
                sb.Append("  -seed:SEED      If provided, this option allows you to set the seed for the" + NL + NL);
                sb.Append("                  random generator in the test context." + NL + NL);
                sb.Append("  -include:CAT    List of categories to include" + NL + NL);
                sb.Append("  -exclude:CAT    List of categories to exclude" + NL + NL);
                sb.Append("  -wait           Waits for a key press before exiting" + NL + NL);

                sb.Append("Notes:" + NL);
                sb.Append(" * File names may be listed by themselves, with a relative path or " + NL);
                sb.Append("   using an absolute path. Any relative path is based on the current " + NL);
                sb.Append("   directory or on the Documents folder if running on a under the " +NL);
                sb.Append("   compact framework." + NL + NL);
                if (System.IO.Path.DirectorySeparatorChar != '/')
                    sb.Append(" * On Windows, options may be prefixed by a '/' character if desired" + NL + NL);
                sb.Append(" * Options that take values may use an equal sign or a colon" + NL);
                sb.Append("   to separate the option from its value." + NL + NL);

                return sb.ToString();
            }
        }

#if CLR_2_0 || CLR_4_0
        class StringList : List<string> { }
#else
        class StringList : ArrayList 
        {
            public new string[] ToArray()
            {
                return (string[])ToArray(typeof(string));
            }
        }
#endif
    }
}
