// ***********************************************************************
// Copyright (c) 2009 Charlie Poole
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
using NUnitLite.Runner;
using NUnit.Framework.Internal;

namespace NUnitLite.Tests
{
    public class Program
    {
        // The main program executes the tests. Output may be routed to
        // various locations, depending on the arguments passed.
        //
        // Arguments:
        //
        //  Arguments may be names of assemblies or options prefixed with '/'
        //  or '-'. Normally, no assemblies are passed and the calling
        //  assembly (the one containing this Main) is used. The following
        //  options are accepted:
        //
        //    -test:<testname>  Provides the name of a test to be exected.
        //                      May be repeated. If this option is not used,
        //                      all tests are run.
        //
        //    -out:PATH         Path to a file to which output is written.
        //                      If omitted, Console is used, which means the
        //                      output is lost on a platform with no Console.
        //
        //    -full             Print full report of all tests.
        //
        //    -result:PATH      Path to a file to which the XML test result is written.
        //
        //    -explore[:Path]   If specified, list tests rather than executing them. If a
        //                      path is given, an XML file representing the tests is written
        //                      to that location. If not, output is written to tests.xml.
        //
        //    -noheader,noh     Suppress display of the initial message.
        //
        //    -wait             Wait for a keypress before exiting.
        //
        //    -include:categorylist 
        //             If specified, nunitlite will only run the tests with a category 
        //             that is in the comma separated list of category names. 
        //             Example usage: -include:category1,category2 this command can be used
        //             in combination with the -exclude option also note that exlude takes priority
        //             over all includes.
        //
        //    -exclude:categorylist 
        //             If specified, nunitlite will not run any of the tests with a category 
        //             that is in the comma separated list of category names. 
        //             Example usage: -exclude:category1,category2 this command can be used
        //             in combination with the -include option also note that exclude takes priority
        //             over all includes
        public static void Main(string[] args)
        {
            new TextUI().Execute(args);
        }
    }
}