// ***********************************************************************
// Copyright (c) 2008 Charlie Poole
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
using NUnit.Framework.Internal;

namespace NUnit.Framework.Constraints
{
    /// <summary>
    /// PathConstraint serves as the abstract base of constraints
    /// that operate on paths and provides several helper methods.
    /// </summary>
    public abstract class PathConstraint : Constraint
    {
        private static readonly char[] DirectorySeparatorChars = new char[] { '\\', '/' };

        /// <summary>
        /// The expected path used in the constraint
        /// </summary>
        protected string expectedPath;

        /// <summary>
        /// Flag indicating whether a caseInsensitive comparison should be made
        /// </summary>
        protected bool caseInsensitive = Path.DirectorySeparatorChar == '\\';

        /// <summary>
        /// Construct a PathConstraint for a give expected path
        /// </summary>
        /// <param name="expectedPath">The expected path</param>
        protected PathConstraint(string expectedPath) : base(expectedPath)
        {
            this.expectedPath = expectedPath;
        }

        /// <summary>
        /// Modifies the current instance to be case-insensitve
        /// and returns it.
        /// </summary>
        public PathConstraint IgnoreCase
        {
            get { caseInsensitive = true; return this; }
        }

        /// <summary>
        /// Modifies the current instance to be case-sensitve
        /// and returns it.
        /// </summary>
        public PathConstraint RespectCase
        {
            get { caseInsensitive = false; return this; }
        }

        /// <summary>
        /// Test whether the constraint is satisfied by a given value
        /// </summary>
        /// <param name="actual">The value to be tested</param>
        /// <returns>True for success, false for failure</returns>
        public override bool Matches(object actual)
        {
            this.actual = actual;
            string actualPath = actual as string;

            return actualPath != null && IsMatch(expectedPath, actualPath);
        }

        /// <summary>
        /// Returns true if the expected path and actual path match
        /// </summary>
        protected abstract bool IsMatch(string expectedPath, string actualPath);

        /// <summary>
        /// Returns the string representation of this constraint
        /// </summary>
        protected override string GetStringRepresentation()
        {
            return string.Format("<{0} \"{1}\" {2}>", DisplayName, expectedPath, caseInsensitive ? "ignorecase" : "respectcase");
        }

        #region Static Helper Methods

        /// <summary>
        /// Transform the provided path to its canonical form so that it 
        /// may be more easily be compared with other paths.
        /// </summary>
        /// <param name="path">The original path</param>
        /// <returns>The path in canonical form</returns>
        protected static string Canonicalize(string path)
        {
            if (Path.DirectorySeparatorChar != Path.AltDirectorySeparatorChar)
                path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            string leadingSeparators = "";
            foreach (char c in path)
            {
                if (c == Path.DirectorySeparatorChar)
                    leadingSeparators += c;
                else break;
            }

#if (CLR_2_0 || CLR_4_0) && !NETCF
            string[] parts = path.Split(DirectorySeparatorChars, StringSplitOptions.RemoveEmptyEntries);
#else
            string[] parts = path.Split(DirectorySeparatorChars);
#endif
            int count = 0;
            bool shifting = false;
            foreach (string part in parts)
            {
                switch (part)
                {
                    case "":
                    case ".":
                        shifting = true;
                        break;

                    case "..":
                        shifting = true;
                        if (count > 0)
                            --count;
                        break;

                    default:
                        if (shifting)
                            parts[count] = part;
                        ++count;
                        break;
                }
            }

            return leadingSeparators + String.Join(Path.DirectorySeparatorChar.ToString(), parts, 0, count);
        }

        /// <summary>
        /// Test whether one path in canonical form is under another.
        /// </summary>
        /// <param name="path1">The first path - supposed to be the parent path</param>
        /// <param name="path2">The second path - supposed to be the child path</param>
        /// <param name="ignoreCase">Indicates whether case should be ignored</param>
        /// <returns></returns>
        protected static bool IsSubPath(string path1, string path2, bool ignoreCase)
        {
            int length1 = path1.Length;
            int length2 = path2.Length;

            // if path1 is longer or equal, then path2 can't be under it
            if (length1 >= length2)
                return false;

            // path 2 is longer than path 1: see if initial parts match
            if (!StringUtil.StringsEqual(path1, path2.Substring(0, length1), ignoreCase))
                return false;

            // must match through or up to a directory separator boundary
            return path2[length1 - 1] == Path.DirectorySeparatorChar ||
                length2 > length1 && path2[length1] == Path.DirectorySeparatorChar;
        }

        #endregion
    }
}