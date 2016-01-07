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
using System.Text;

namespace NUnit
{
    /// <summary>
    /// Env is a static class that provides some of the features of
    /// System.Environment that are not available under all runtimes
    /// </summary>
    public class Env
    {
        // Define NewLine to be used for this system
        // NOTE: Since this is done at compile time for .NET CF,
        // these binaries are not yet currently portable.
        /// <summary>
        /// The newline sequence in the current environmemt.
        /// </summary>
#if PocketPC || WindowsCE || NETCF
        public static readonly string NewLine = "\r\n";
#else
        public static readonly string NewLine = Environment.NewLine;
#endif

        /// <summary>
        /// Path to the 'My Documents' folder
        /// </summary>
#if SILVERLIGHT || PocketPC || WindowsCE || NETCF
        public static string DocumentFolder = @"\My Documents";
#else
        public static string DocumentFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
#endif
    }
}
