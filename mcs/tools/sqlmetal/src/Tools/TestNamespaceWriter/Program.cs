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

namespace TestNamespaceWriter
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    class Program
    {
        /// <summary>
        /// Processes the specified directory.
        /// </summary>
        /// <param name="directory">The directory.</param>
        static void ProcessDirectory(string directory)
        {
            foreach (var file in Directory.GetFiles(directory, "*.cs"))
            {
                ProcessFile(file);
            }
            foreach (var subDirectory in Directory.GetDirectories(directory))
            {
                ProcessDirectory(subDirectory);
            }
        }

        /// <summary>
        /// Processes the file.
        /// </summary>
        /// <param name="file">The file.</param>
        private static void ProcessFile(string file)
        {
            string codeText;
            using (var textStream = File.OpenText(file))
            {
                codeText = textStream.ReadToEnd();
            }
            if (IsTest(codeText))
            {
                Console.WriteLine("Processing {0}", Path.GetFileName(file));
                codeText = SetNamespaces(codeText);
                using (var fileStream = File.Create(file))
                using (var textStream = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    textStream.Write(codeText);
                }
            }
        }

        private const string Marker = "// test ns";

        private static readonly Regex HeaderEx = new Regex(Regex.Escape(Marker) + "(?<ns>[^\n\r]*)?" + "(?<holder>.*?){",
                                                           RegexOptions.Compiled | RegexOptions.Singleline);

        /// <summary>
        /// Sets the namespaces.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        private static string SetNamespaces(string text)
        {
            return HeaderEx.Replace(text, delegate(Match match)
                                              {
                                                  var ns = match.Groups["ns"].Value.Trim();
                                                  var newHeader = Marker + " " + ns + "\r\n" + GetNamespaces(ns) + "{";
                                                  return newHeader;
                                              });
        }

        private static string GetNamespaces(string ns)
        {
            var namespacesBuilder = new StringBuilder();
            foreach (string key in ConfigurationManager.AppSettings)
            {
                if (namespacesBuilder.Length == 0)
                    namespacesBuilder.Append("#if ");
                else
                    namespacesBuilder.Append("#elif ");

                var keys = key.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var literalKeys = string.Join(" && ", keys);
                namespacesBuilder.AppendLine(literalKeys);
                namespacesBuilder.AppendFormat("    namespace {0}{1}\r\n", ConfigurationManager.AppSettings[key],
                    string.IsNullOrEmpty(ns) ? "" : "." + ns);
            }
            namespacesBuilder.AppendLine("#endif");
            return namespacesBuilder.ToString();
        }

        /// <summary>
        /// Determines whether the specified code text is test.
        /// </summary>
        /// <param name="codeText">The code text.</param>
        /// <returns>
        /// 	<c>true</c> if the specified code text is test; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsTest(string codeText)
        {
            return codeText.Contains("\r\n" + Marker);
        }

        static void Main(string[] args)
        {
            ProcessDirectory(@"..\..\..\..");
        }
    }
}
