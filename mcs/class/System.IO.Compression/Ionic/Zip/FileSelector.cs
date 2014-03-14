//#define SelectorTrace

// FileSelector.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2008-2011 Dino Chiesa.
// All rights reserved.
//
// This code module is part of DotNetZip, a zipfile class library.
//
// ------------------------------------------------------------------
//
// This code is licensed under the Microsoft Public License.
// See the file License.txt for the license details.
// More info on: http://dotnetzip.codeplex.com
//
// ------------------------------------------------------------------
//
// last saved: <2011-August-05 11:03:11>
//
// ------------------------------------------------------------------
//
// This module implements a "file selector" that finds files based on a
// set of inclusion criteria, including filename, size, file time, and
// potentially file attributes.  The criteria are given in a string with
// a simple expression language. Examples:
//
// find all .txt files:
//     name = *.txt
//
// shorthand for the above
//     *.txt
//
// all files modified after January 1st, 2009
//     mtime > 2009-01-01
//
// All .txt files modified after the first of the year
//     name = *.txt  AND  mtime > 2009-01-01
//
// All .txt files modified after the first of the year, or any file with the archive bit set
//     (name = *.txt  AND  mtime > 2009-01-01) or (attribtues = A)
//
// All .txt files or any file greater than 1mb in size
//     (name = *.txt  or  size > 1mb)
//
// and so on.
// ------------------------------------------------------------------


using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Collections.Generic;
#if SILVERLIGHT
using System.Linq;
#endif

namespace Ionic
{

    /// <summary>
    /// Enumerates the options for a logical conjunction. This enum is intended for use
    /// internally by the FileSelector class.
    /// </summary>
    internal enum LogicalConjunction
    {
        NONE,
        AND,
        OR,
        XOR,
    }

    internal enum WhichTime
    {
        atime,
        mtime,
        ctime,
    }


    internal enum ComparisonOperator
    {
        [Description(">")]
        GreaterThan,
        [Description(">=")]
        GreaterThanOrEqualTo,
        [Description("<")]
        LesserThan,
        [Description("<=")]
        LesserThanOrEqualTo,
        [Description("=")]
        EqualTo,
        [Description("!=")]
        NotEqualTo
    }


    internal abstract partial class SelectionCriterion
    {
        internal virtual bool Verbose
        {
            get;set;
        }
        internal abstract bool Evaluate(string filename);

        [System.Diagnostics.Conditional("SelectorTrace")]
        protected static void CriterionTrace(string format, params object[] args)
        {
            //System.Console.WriteLine("  " + format, args);
        }
    }


    internal partial class SizeCriterion : SelectionCriterion
    {
        internal ComparisonOperator Operator;
        internal Int64 Size;

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("size ").Append(EnumUtil.GetDescription(Operator)).Append(" ").Append(Size.ToString());
            return sb.ToString();
        }

        internal override bool Evaluate(string filename)
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(filename);
            CriterionTrace("SizeCriterion::Evaluate('{0}' [{1}])",
                           filename, this.ToString());
            return _Evaluate(fi.Length);
        }

        private bool _Evaluate(Int64 Length)
        {
            bool result = false;
            switch (Operator)
            {
                case ComparisonOperator.GreaterThanOrEqualTo:
                    result = Length >= Size;
                    break;
                case ComparisonOperator.GreaterThan:
                    result = Length > Size;
                    break;
                case ComparisonOperator.LesserThanOrEqualTo:
                    result = Length <= Size;
                    break;
                case ComparisonOperator.LesserThan:
                    result = Length < Size;
                    break;
                case ComparisonOperator.EqualTo:
                    result = Length == Size;
                    break;
                case ComparisonOperator.NotEqualTo:
                    result = Length != Size;
                    break;
                default:
                    throw new ArgumentException("Operator");
            }
            return result;
        }

    }



    internal partial class TimeCriterion : SelectionCriterion
    {
        internal ComparisonOperator Operator;
        internal WhichTime Which;
        internal DateTime Time;

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Which.ToString()).Append(" ").Append(EnumUtil.GetDescription(Operator)).Append(" ").Append(Time.ToString("yyyy-MM-dd-HH:mm:ss"));
            return sb.ToString();
        }

        internal override bool Evaluate(string filename)
        {
            DateTime x;
            switch (Which)
            {
                case WhichTime.atime:
                    x = System.IO.File.GetLastAccessTime(filename).ToUniversalTime();
                    break;
                case WhichTime.mtime:
                    x = System.IO.File.GetLastWriteTime(filename).ToUniversalTime();
                    break;
                case WhichTime.ctime:
                    x = System.IO.File.GetCreationTime(filename).ToUniversalTime();
                    break;
                default:
                    throw new ArgumentException("Operator");
            }
            CriterionTrace("TimeCriterion({0},{1})= {2}", filename, Which.ToString(), x);
            return _Evaluate(x);
        }


        private bool _Evaluate(DateTime x)
        {
            bool result = false;
            switch (Operator)
            {
                case ComparisonOperator.GreaterThanOrEqualTo:
                    result = (x >= Time);
                    break;
                case ComparisonOperator.GreaterThan:
                    result = (x > Time);
                    break;
                case ComparisonOperator.LesserThanOrEqualTo:
                    result = (x <= Time);
                    break;
                case ComparisonOperator.LesserThan:
                    result = (x < Time);
                    break;
                case ComparisonOperator.EqualTo:
                    result = (x == Time);
                    break;
                case ComparisonOperator.NotEqualTo:
                    result = (x != Time);
                    break;
                default:
                    throw new ArgumentException("Operator");
            }

            CriterionTrace("TimeCriterion: {0}", result);
            return result;
        }
    }



    internal partial class NameCriterion : SelectionCriterion
    {
        private Regex _re;
        private String _regexString;
        internal ComparisonOperator Operator;
        private string _MatchingFileSpec;
        internal virtual string MatchingFileSpec
        {
            set
            {
                // workitem 8245
                if (Directory.Exists(value))
                {
                    _MatchingFileSpec = ".\\" + value + "\\*.*";
                }
                else
                {
                    _MatchingFileSpec = value;
                }

                _regexString = "^" +
                Regex.Escape(_MatchingFileSpec)
                    .Replace(@"\\\*\.\*", @"\\([^\.]+|.*\.[^\\\.]*)")
                    .Replace(@"\.\*", @"\.[^\\\.]*")
                    .Replace(@"\*", @".*")
                    //.Replace(@"\*", @"[^\\\.]*") // ill-conceived
                    .Replace(@"\?", @"[^\\\.]")
                    + "$";

                CriterionTrace("NameCriterion regexString({0})", _regexString);

                _re = new Regex(_regexString, RegexOptions.IgnoreCase);
            }
        }


        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("name ").Append(EnumUtil.GetDescription(Operator))
                .Append(" '")
                .Append(_MatchingFileSpec)
                .Append("'");
            return sb.ToString();
        }


        internal override bool Evaluate(string filename)
        {
            CriterionTrace("NameCriterion::Evaluate('{0}' pattern[{1}])",
                           filename, _MatchingFileSpec);
            return _Evaluate(filename);
        }

        private bool _Evaluate(string fullpath)
        {
            CriterionTrace("NameCriterion::Evaluate({0})", fullpath);
            // No slash in the pattern implicitly means recurse, which means compare to
            // filename only, not full path.
            String f = (_MatchingFileSpec.IndexOf('\\') == -1)
                ? System.IO.Path.GetFileName(fullpath)
                : fullpath; // compare to fullpath

            bool result = _re.IsMatch(f);

            if (Operator != ComparisonOperator.EqualTo)
                result = !result;
            return result;
        }
    }


    internal partial class TypeCriterion : SelectionCriterion
    {
        private char ObjectType;  // 'D' = Directory, 'F' = File
        internal ComparisonOperator Operator;
        internal string AttributeString
        {
            get
            {
                return ObjectType.ToString();
            }
            set
            {
                if (value.Length != 1 ||
                    (value[0]!='D' && value[0]!='F'))
                    throw new ArgumentException("Specify a single character: either D or F");
                ObjectType = value[0];
            }
        }

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("type ").Append(EnumUtil.GetDescription(Operator)).Append(" ").Append(AttributeString);
            return sb.ToString();
        }

        internal override bool Evaluate(string filename)
        {
            CriterionTrace("TypeCriterion::Evaluate({0})", filename);

            bool result = (ObjectType == 'D')
                ? Directory.Exists(filename)
                : File.Exists(filename);

            if (Operator != ComparisonOperator.EqualTo)
                result = !result;
            return result;
        }
    }


#if !SILVERLIGHT
    internal partial class AttributesCriterion : SelectionCriterion
    {
        private FileAttributes _Attributes;
        internal ComparisonOperator Operator;
        internal string AttributeString
        {
            get
            {
                string result = "";
                if ((_Attributes & FileAttributes.Hidden) != 0)
                    result += "H";
                if ((_Attributes & FileAttributes.System) != 0)
                    result += "S";
                if ((_Attributes & FileAttributes.ReadOnly) != 0)
                    result += "R";
                if ((_Attributes & FileAttributes.Archive) != 0)
                    result += "A";
                if ((_Attributes & FileAttributes.ReparsePoint) != 0)
                    result += "L";
                if ((_Attributes & FileAttributes.NotContentIndexed) != 0)
                    result += "I";
                return result;
            }

            set
            {
                _Attributes = FileAttributes.Normal;
                foreach (char c in value.ToUpper())
                {
                    switch (c)
                    {
                        case 'H':
                            if ((_Attributes & FileAttributes.Hidden) != 0)
                                throw new ArgumentException(String.Format("Repeated flag. ({0})", c), "value");
                            _Attributes |= FileAttributes.Hidden;
                            break;

                        case 'R':
                            if ((_Attributes & FileAttributes.ReadOnly) != 0)
                                throw new ArgumentException(String.Format("Repeated flag. ({0})", c), "value");
                            _Attributes |= FileAttributes.ReadOnly;
                            break;

                        case 'S':
                            if ((_Attributes & FileAttributes.System) != 0)
                                throw new ArgumentException(String.Format("Repeated flag. ({0})", c), "value");
                            _Attributes |= FileAttributes.System;
                            break;

                        case 'A':
                            if ((_Attributes & FileAttributes.Archive) != 0)
                                throw new ArgumentException(String.Format("Repeated flag. ({0})", c), "value");
                            _Attributes |= FileAttributes.Archive;
                            break;

                        case 'I':
                            if ((_Attributes & FileAttributes.NotContentIndexed) != 0)
                                throw new ArgumentException(String.Format("Repeated flag. ({0})", c), "value");
                            _Attributes |= FileAttributes.NotContentIndexed;
                            break;

                        case 'L':
                            if ((_Attributes & FileAttributes.ReparsePoint) != 0)
                                throw new ArgumentException(String.Format("Repeated flag. ({0})", c), "value");
                            _Attributes |= FileAttributes.ReparsePoint;
                            break;

                        default:
                            throw new ArgumentException(value);
                    }
                }
            }
        }


        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("attributes ").Append(EnumUtil.GetDescription(Operator)).Append(" ").Append(AttributeString);
            return sb.ToString();
        }

        private bool _EvaluateOne(FileAttributes fileAttrs, FileAttributes criterionAttrs)
        {
            bool result = false;
            if ((_Attributes & criterionAttrs) == criterionAttrs)
                result = ((fileAttrs & criterionAttrs) == criterionAttrs);
            else
                result = true;
            return result;
        }



        internal override bool Evaluate(string filename)
        {
            // workitem 10191
            if (Directory.Exists(filename))
            {
                // Directories don't have file attributes, so the result
                // of an evaluation is always NO. This gets negated if
                // the operator is NotEqualTo.
                return (Operator != ComparisonOperator.EqualTo);
            }
#if NETCF
            FileAttributes fileAttrs = NetCfFile.GetAttributes(filename);
#else
            FileAttributes fileAttrs = System.IO.File.GetAttributes(filename);
#endif

            return _Evaluate(fileAttrs);
        }

        private bool _Evaluate(FileAttributes fileAttrs)
        {
            bool result = _EvaluateOne(fileAttrs, FileAttributes.Hidden);
            if (result)
                result = _EvaluateOne(fileAttrs, FileAttributes.System);
            if (result)
                result = _EvaluateOne(fileAttrs, FileAttributes.ReadOnly);
            if (result)
                result = _EvaluateOne(fileAttrs, FileAttributes.Archive);
            if (result)
                result = _EvaluateOne(fileAttrs, FileAttributes.NotContentIndexed);
            if (result)
                result = _EvaluateOne(fileAttrs, FileAttributes.ReparsePoint);

            if (Operator != ComparisonOperator.EqualTo)
                result = !result;

            return result;
        }
    }
#endif


    internal partial class CompoundCriterion : SelectionCriterion
    {
        internal LogicalConjunction Conjunction;
        internal SelectionCriterion Left;

        private SelectionCriterion _Right;
        internal SelectionCriterion Right
        {
            get { return _Right; }
            set
            {
                _Right = value;
                if (value == null)
                    Conjunction = LogicalConjunction.NONE;
                else if (Conjunction == LogicalConjunction.NONE)
                    Conjunction = LogicalConjunction.AND;
            }
        }


        internal override bool Evaluate(string filename)
        {
            bool result = Left.Evaluate(filename);
            switch (Conjunction)
            {
                case LogicalConjunction.AND:
                    if (result)
                        result = Right.Evaluate(filename);
                    break;
                case LogicalConjunction.OR:
                    if (!result)
                        result = Right.Evaluate(filename);
                    break;
                case LogicalConjunction.XOR:
                    result ^= Right.Evaluate(filename);
                    break;
                default:
                    throw new ArgumentException("Conjunction");
            }
            return result;
        }


        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(")
            .Append((Left != null) ? Left.ToString() : "null")
            .Append(" ")
            .Append(Conjunction.ToString())
            .Append(" ")
            .Append((Right != null) ? Right.ToString() : "null")
            .Append(")");
            return sb.ToString();
        }
    }



    /// <summary>
    ///   FileSelector encapsulates logic that selects files from a source - a zip file
    ///   or the filesystem - based on a set of criteria.  This class is used internally
    ///   by the DotNetZip library, in particular for the AddSelectedFiles() methods.
    ///   This class can also be used independently of the zip capability in DotNetZip.
    /// </summary>
    ///
    /// <remarks>
    ///
    /// <para>
    ///   The FileSelector class is used internally by the ZipFile class for selecting
    ///   files for inclusion into the ZipFile, when the <see
    ///   cref="Ionic.Zip.ZipFile.AddSelectedFiles(String,String)"/> method, or one of
    ///   its overloads, is called.  It's also used for the <see
    ///   cref="Ionic.Zip.ZipFile.ExtractSelectedEntries(String)"/> methods.  Typically, an
    ///   application that creates or manipulates Zip archives will not directly
    ///   interact with the FileSelector class.
    /// </para>
    ///
    /// <para>
    ///   Some applications may wish to use the FileSelector class directly, to
    ///   select files from disk volumes based on a set of criteria, without creating or
    ///   querying Zip archives.  The file selection criteria include: a pattern to
    ///   match the filename; the last modified, created, or last accessed time of the
    ///   file; the size of the file; and the attributes of the file.
    /// </para>
    ///
    /// <para>
    ///   Consult the documentation for <see cref="SelectionCriteria"/>
    ///   for more information on specifying the selection criteria.
    /// </para>
    ///
    /// </remarks>
    internal partial class FileSelector
    {
        internal SelectionCriterion _Criterion;

#if NOTUSED
        /// <summary>
        ///   The default constructor.
        /// </summary>
        /// <remarks>
        ///   Typically, applications won't use this constructor.  Instead they'll
        ///   call the constructor that accepts a selectionCriteria string.  If you
        ///   use this constructor, you'll want to set the SelectionCriteria
        ///   property on the instance before calling SelectFiles().
        /// </remarks>
        protected FileSelector() { }
#endif
        /// <summary>
        ///   Constructor that allows the caller to specify file selection criteria.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   This constructor allows the caller to specify a set of criteria for
        ///   selection of files.
        /// </para>
        ///
        /// <para>
        ///   See <see cref="FileSelector.SelectionCriteria"/> for a description of
        ///   the syntax of the selectionCriteria string.
        /// </para>
        ///
        /// <para>
        ///   By default the FileSelector will traverse NTFS Reparse Points.  To
        ///   change this, use <see cref="FileSelector(String,
        ///   bool)">FileSelector(String, bool)</see>.
        /// </para>
        /// </remarks>
        ///
        /// <param name="selectionCriteria">The criteria for file selection.</param>
        public FileSelector(String selectionCriteria)
        : this(selectionCriteria, true)
        {
        }

        /// <summary>
        ///   Constructor that allows the caller to specify file selection criteria.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   This constructor allows the caller to specify a set of criteria for
        ///   selection of files.
        /// </para>
        ///
        /// <para>
        ///   See <see cref="FileSelector.SelectionCriteria"/> for a description of
        ///   the syntax of the selectionCriteria string.
        /// </para>
        /// </remarks>
        ///
        /// <param name="selectionCriteria">The criteria for file selection.</param>
        /// <param name="traverseDirectoryReparsePoints">
        /// whether to traverse NTFS reparse points (junctions).
        /// </param>
        public FileSelector(String selectionCriteria, bool traverseDirectoryReparsePoints)
        {
            if (!String.IsNullOrEmpty(selectionCriteria))
                _Criterion = _ParseCriterion(selectionCriteria);
            TraverseReparsePoints = traverseDirectoryReparsePoints;
        }



        /// <summary>
        ///   The string specifying which files to include when retrieving.
        /// </summary>
        /// <remarks>
        ///
        /// <para>
        ///   Specify the criteria in statements of 3 elements: a noun, an operator,
        ///   and a value.  Consider the string "name != *.doc" .  The noun is
        ///   "name".  The operator is "!=", implying "Not Equal".  The value is
        ///   "*.doc".  That criterion, in English, says "all files with a name that
        ///   does not end in the .doc extension."
        /// </para>
        ///
        /// <para>
        ///   Supported nouns include "name" (or "filename") for the filename;
        ///   "atime", "mtime", and "ctime" for last access time, last modfied time,
        ///   and created time of the file, respectively; "attributes" (or "attrs")
        ///   for the file attributes; "size" (or "length") for the file length
        ///   (uncompressed); and "type" for the type of object, either a file or a
        ///   directory.  The "attributes", "type", and "name" nouns all support =
        ///   and != as operators.  The "size", "atime", "mtime", and "ctime" nouns
        ///   support = and !=, and &gt;, &gt;=, &lt;, &lt;= as well.  The times are
        ///   taken to be expressed in local time.
        /// </para>
        ///
        /// <para>
        ///   Specify values for the file attributes as a string with one or more of
        ///   the characters H,R,S,A,I,L in any order, implying file attributes of
        ///   Hidden, ReadOnly, System, Archive, NotContextIndexed, and ReparsePoint
        ///   (symbolic link) respectively.
        /// </para>
        ///
        /// <para>
        ///   To specify a time, use YYYY-MM-DD-HH:mm:ss or YYYY/MM/DD-HH:mm:ss as
        ///   the format.  If you omit the HH:mm:ss portion, it is assumed to be
        ///   00:00:00 (midnight).
        /// </para>
        ///
        /// <para>
        ///   The value for a size criterion is expressed in integer quantities of
        ///   bytes, kilobytes (use k or kb after the number), megabytes (m or mb),
        ///   or gigabytes (g or gb).
        /// </para>
        ///
        /// <para>
        ///   The value for a name is a pattern to match against the filename,
        ///   potentially including wildcards.  The pattern follows CMD.exe glob
        ///   rules: * implies one or more of any character, while ?  implies one
        ///   character.  If the name pattern contains any slashes, it is matched to
        ///   the entire filename, including the path; otherwise, it is matched
        ///   against only the filename without the path.  This means a pattern of
        ///   "*\*.*" matches all files one directory level deep, while a pattern of
        ///   "*.*" matches all files in all directories.
        /// </para>
        ///
        /// <para>
        ///   To specify a name pattern that includes spaces, use single quotes
        ///   around the pattern.  A pattern of "'* *.*'" will match all files that
        ///   have spaces in the filename.  The full criteria string for that would
        ///   be "name = '* *.*'" .
        /// </para>
        ///
        /// <para>
        ///   The value for a type criterion is either F (implying a file) or D
        ///   (implying a directory).
        /// </para>
        ///
        /// <para>
        ///   Some examples:
        /// </para>
        ///
        /// <list type="table">
        ///   <listheader>
        ///     <term>criteria</term>
        ///     <description>Files retrieved</description>
        ///   </listheader>
        ///
        ///   <item>
        ///     <term>name != *.xls </term>
        ///     <description>any file with an extension that is not .xls
        ///     </description>
        ///   </item>
        ///
        ///   <item>
        ///     <term>name = *.mp3 </term>
        ///     <description>any file with a .mp3 extension.
        ///     </description>
        ///   </item>
        ///
        ///   <item>
        ///     <term>*.mp3</term>
        ///     <description>(same as above) any file with a .mp3 extension.
        ///     </description>
        ///   </item>
        ///
        ///   <item>
        ///     <term>attributes = A </term>
        ///     <description>all files whose attributes include the Archive bit.
        ///     </description>
        ///   </item>
        ///
        ///   <item>
        ///     <term>attributes != H </term>
        ///     <description>all files whose attributes do not include the Hidden bit.
        ///     </description>
        ///   </item>
        ///
        ///   <item>
        ///     <term>mtime > 2009-01-01</term>
        ///     <description>all files with a last modified time after January 1st, 2009.
        ///     </description>
        ///   </item>
        ///
        ///   <item>
        ///     <term>ctime > 2009/01/01-03:00:00</term>
        ///     <description>all files with a created time after 3am (local time),
        ///     on January 1st, 2009.
        ///     </description>
        ///   </item>
        ///
        ///   <item>
        ///     <term>size > 2gb</term>
        ///     <description>all files whose uncompressed size is greater than 2gb.
        ///     </description>
        ///   </item>
        ///
        ///   <item>
        ///     <term>type = D</term>
        ///     <description>all directories in the filesystem. </description>
        ///   </item>
        ///
        /// </list>
        ///
        /// <para>
        ///   You can combine criteria with the conjunctions AND, OR, and XOR. Using
        ///   a string like "name = *.txt AND size &gt;= 100k" for the
        ///   selectionCriteria retrieves entries whose names end in .txt, and whose
        ///   uncompressed size is greater than or equal to 100 kilobytes.
        /// </para>
        ///
        /// <para>
        ///   For more complex combinations of criteria, you can use parenthesis to
        ///   group clauses in the boolean logic.  Absent parenthesis, the
        ///   precedence of the criterion atoms is determined by order of
        ///   appearance.  Unlike the C# language, the AND conjunction does not take
        ///   precendence over the logical OR.  This is important only in strings
        ///   that contain 3 or more criterion atoms.  In other words, "name = *.txt
        ///   and size &gt; 1000 or attributes = H" implies "((name = *.txt AND size
        ///   &gt; 1000) OR attributes = H)" while "attributes = H OR name = *.txt
        ///   and size &gt; 1000" evaluates to "((attributes = H OR name = *.txt)
        ///   AND size &gt; 1000)".  When in doubt, use parenthesis.
        /// </para>
        ///
        /// <para>
        ///   Using time properties requires some extra care. If you want to
        ///   retrieve all entries that were last updated on 2009 February 14,
        ///   specify "mtime &gt;= 2009-02-14 AND mtime &lt; 2009-02-15".  Read this
        ///   to say: all files updated after 12:00am on February 14th, until
        ///   12:00am on February 15th.  You can use the same bracketing approach to
        ///   specify any time period - a year, a month, a week, and so on.
        /// </para>
        ///
        /// <para>
        ///   The syntax allows one special case: if you provide a string with no
        ///   spaces, it is treated as a pattern to match for the filename.
        ///   Therefore a string like "*.xls" will be equivalent to specifying "name
        ///   = *.xls".  This "shorthand" notation does not work with compound
        ///   criteria.
        /// </para>
        ///
        /// <para>
        ///   There is no logic in this class that insures that the inclusion
        ///   criteria are internally consistent.  For example, it's possible to
        ///   specify criteria that says the file must have a size of less than 100
        ///   bytes, as well as a size that is greater than 1000 bytes.  Obviously
        ///   no file will ever satisfy such criteria, but this class does not check
        ///   for or detect such inconsistencies.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <exception cref="System.Exception">
        ///   Thrown in the setter if the value has an invalid syntax.
        /// </exception>
        public String SelectionCriteria
        {
            get
            {
                if (_Criterion == null) return null;
                return _Criterion.ToString();
            }
            set
            {
                if (value == null) _Criterion = null;
                else if (value.Trim() == "") _Criterion = null;
                else
                    _Criterion = _ParseCriterion(value);
            }
        }

        /// <summary>
        ///  Indicates whether searches will traverse NTFS reparse points, like Junctions.
        /// </summary>
        public bool TraverseReparsePoints
        {
            get; set;
        }


        private enum ParseState
        {
            Start,
            OpenParen,
            CriterionDone,
            ConjunctionPending,
            Whitespace,
        }


        private static class RegexAssertions
        {
            internal static readonly String PrecededByOddNumberOfSingleQuotes = "(?<=(?:[^']*'[^']*')*'[^']*)";
            internal static readonly String FollowedByOddNumberOfSingleQuotesAndLineEnd = "(?=[^']*'(?:[^']*'[^']*')*[^']*$)";

            internal static readonly String PrecededByEvenNumberOfSingleQuotes = "(?<=(?:[^']*'[^']*')*[^']*)";
            internal static readonly String FollowedByEvenNumberOfSingleQuotesAndLineEnd = "(?=(?:[^']*'[^']*')*[^']*$)";
        }


        private static string NormalizeCriteriaExpression(string source)
        {
            // The goal here is to normalize the criterion expression. At output, in
            // the transformed criterion string, every significant syntactic element
            // - a property element, grouping paren for the boolean logic, operator
            // ( = < > != ), conjunction, or property value - will be separated from
            // its neighbors by at least one space. Thus,
            //
            // before                         after
            // -------------------------------------------------------------------
            // name=*.txt                     name = *.txt
            // (size>100)AND(name=*.txt)      ( size > 100 ) AND ( name = *.txt )
            //
            // This is relatively straightforward using regular expression
            // replacement. This method applies a distinct regex pattern and
            // corresponding replacement string for each one of a number of cases:
            // an open paren followed by a word; a word followed by a close-paren; a
            // pair of open parens; a close paren followed by a word (which should
            // then be followed by an open paren). And so on. These patterns and
            // replacements are all stored in prPairs. By applying each of these
            // regex replacements in turn, we get the transformed string. Easy.
            //
            // The resulting "normalized" criterion string, is then used as the
            // subject that gets parsed, by splitting the string into tokens that
            // are separated by spaces.  Here, there's a twist. The spaces within
            // single-quote delimiters do not delimit distinct tokens.  So, this
            // normalization method temporarily replaces those spaces with
            // ASCII 6 (0x06), a control character which is not a legal
            // character in a filename. The parsing logic that happens later will
            // revert that change, restoring the original value of the filename
            // specification.
            //
            // To illustrate, for a "before" string of [(size>100)AND(name='Name
            // (with Parens).txt')] , the "after" string is [( size > 100 ) AND
            // ( name = 'Name\u0006(with\u0006Parens).txt' )].
            //

            string[][] prPairs =
                {
                    // A. opening double parens - insert a space between them
                    new string[] { @"([^']*)\(\(([^']+)", "$1( ($2" },

                    // B. closing double parens - insert a space between
                    new string[] { @"(.)\)\)", "$1) )" },

                    // C. single open paren with a following word - insert a space between
                    new string[] { @"\((\S)", "( $1" },

                    // D. single close paren with a preceding word - insert a space between the two
                    new string[] { @"(\S)\)", "$1 )" },

                    // E. close paren at line start?, insert a space before the close paren
                    // this seems like a degenerate case.  I don't recall why it's here.
                    new string[] { @"^\)", " )" },

                    // F. a word (likely a conjunction) followed by an open paren - insert a space between
                    new string[] { @"(\S)\(", "$1 (" },

                    // G. single close paren followed by word - insert a paren after close paren
                    new string[] { @"\)(\S)", ") $1" },

                    // H. insert space between = and a following single quote
                    //new string[] { @"(=|!=)('[^']*')", "$1 $2" },
                    new string[] { @"(=)('[^']*')", "$1 $2" },

                    // I. insert space between property names and the following operator
                    //new string[] { @"([^ ])([><(?:!=)=])", "$1 $2" },
                    new string[] { @"([^ !><])(>|<|!=|=)", "$1 $2" },

                    // J. insert spaces between operators and the following values
                    //new string[] { @"([><(?:!=)=])([^ ])", "$1 $2" },
                    new string[] { @"(>|<|!=|=)([^ =])", "$1 $2" },

                    // K. replace fwd slash with backslash
                    new string[] { @"/", "\\" },
                };

            string interim = source;

            for (int i=0; i < prPairs.Length; i++)
            {
                //char caseIdx = (char)('A' + i);
                string pattern = RegexAssertions.PrecededByEvenNumberOfSingleQuotes +
                    prPairs[i][0] +
                    RegexAssertions.FollowedByEvenNumberOfSingleQuotesAndLineEnd;

                interim = Regex.Replace(interim, pattern, prPairs[i][1]);
            }

            // match a fwd slash, followed by an odd number of single quotes.
            // This matches fwd slashes only inside a pair of single quote delimiters,
            // eg, a filename.  This must be done as well as the case above, to handle
            // filenames specified inside quotes as well as filenames without quotes.
            var regexPattern = @"/" +
                                RegexAssertions.FollowedByOddNumberOfSingleQuotesAndLineEnd;
            // replace with backslash
            interim = Regex.Replace(interim, regexPattern, "\\");

            // match a space, followed by an odd number of single quotes.
            // This matches spaces only inside a pair of single quote delimiters.
            regexPattern = " " +
                RegexAssertions.FollowedByOddNumberOfSingleQuotesAndLineEnd;

            // Replace all spaces that appear inside single quotes, with
            // ascii 6.  This allows a split on spaces to get tokens in
            // the expression. The split will not split any filename or
            // wildcard that appears within single quotes. After tokenizing, we
            // need to replace ascii 6 with ascii 32 to revert the
            // spaces within quotes.
            return Regex.Replace(interim, regexPattern, "\u0006");
        }


        private static SelectionCriterion _ParseCriterion(String s)
        {
            if (s == null) return null;

            // inject spaces after open paren and before close paren, etc
            s = NormalizeCriteriaExpression(s);

            // no spaces in the criteria is shorthand for filename glob
            if (s.IndexOf(" ") == -1)
                s = "name = " + s;

            // split the expression into tokens
            string[] tokens = s.Trim().Split(' ', '\t');

            if (tokens.Length < 3) throw new ArgumentException(s);

            SelectionCriterion current = null;

            LogicalConjunction pendingConjunction = LogicalConjunction.NONE;

            ParseState state;
            var stateStack = new System.Collections.Generic.Stack<ParseState>();
            var critStack = new System.Collections.Generic.Stack<SelectionCriterion>();
            stateStack.Push(ParseState.Start);

            for (int i = 0; i < tokens.Length; i++)
            {
                string tok1 = tokens[i].ToLower();
                switch (tok1)
                {
                    case "and":
                    case "xor":
                    case "or":
                        state = stateStack.Peek();
                        if (state != ParseState.CriterionDone)
                            throw new ArgumentException(String.Join(" ", tokens, i, tokens.Length - i));

                        if (tokens.Length <= i + 3)
                            throw new ArgumentException(String.Join(" ", tokens, i, tokens.Length - i));

                        pendingConjunction = (LogicalConjunction)Enum.Parse(typeof(LogicalConjunction), tokens[i].ToUpper(), true);
                        current = new CompoundCriterion { Left = current, Right = null, Conjunction = pendingConjunction };
                        stateStack.Push(state);
                        stateStack.Push(ParseState.ConjunctionPending);
                        critStack.Push(current);
                        break;

                    case "(":
                        state = stateStack.Peek();
                        if (state != ParseState.Start && state != ParseState.ConjunctionPending && state != ParseState.OpenParen)
                            throw new ArgumentException(String.Join(" ", tokens, i, tokens.Length - i));

                        if (tokens.Length <= i + 4)
                            throw new ArgumentException(String.Join(" ", tokens, i, tokens.Length - i));

                        stateStack.Push(ParseState.OpenParen);
                        break;

                    case ")":
                        state = stateStack.Pop();
                        if (stateStack.Peek() != ParseState.OpenParen)
                            throw new ArgumentException(String.Join(" ", tokens, i, tokens.Length - i));

                        stateStack.Pop();
                        stateStack.Push(ParseState.CriterionDone);
                        break;

                    case "atime":
                    case "ctime":
                    case "mtime":
                        if (tokens.Length <= i + 2)
                            throw new ArgumentException(String.Join(" ", tokens, i, tokens.Length - i));

                        DateTime t;
                        try
                        {
                            t = DateTime.ParseExact(tokens[i + 2], "yyyy-MM-dd-HH:mm:ss", null);
                        }
                        catch (FormatException)
                        {
                            try
                            {
                                t = DateTime.ParseExact(tokens[i + 2], "yyyy/MM/dd-HH:mm:ss", null);
                            }
                            catch (FormatException)
                            {
                                try
                                {
                                    t = DateTime.ParseExact(tokens[i + 2], "yyyy/MM/dd", null);
                                }
                                catch (FormatException)
                                {
                                    try
                                    {
                                        t = DateTime.ParseExact(tokens[i + 2], "MM/dd/yyyy", null);
                                    }
                                    catch (FormatException)
                                    {
                                        t = DateTime.ParseExact(tokens[i + 2], "yyyy-MM-dd", null);
                                    }
                                }
                            }
                        }
                        t= DateTime.SpecifyKind(t, DateTimeKind.Local).ToUniversalTime();
                        current = new TimeCriterion
                        {
                            Which = (WhichTime)Enum.Parse(typeof(WhichTime), tokens[i], true),
                            Operator = (ComparisonOperator)EnumUtil.Parse(typeof(ComparisonOperator), tokens[i + 1]),
                            Time = t
                        };
                        i += 2;
                        stateStack.Push(ParseState.CriterionDone);
                        break;


                    case "length":
                    case "size":
                        if (tokens.Length <= i + 2)
                            throw new ArgumentException(String.Join(" ", tokens, i, tokens.Length - i));

                        Int64 sz = 0;
                        string v = tokens[i + 2];
                        if (v.ToUpper().EndsWith("K"))
                            sz = Int64.Parse(v.Substring(0, v.Length - 1)) * 1024;
                        else if (v.ToUpper().EndsWith("KB"))
                            sz = Int64.Parse(v.Substring(0, v.Length - 2)) * 1024;
                        else if (v.ToUpper().EndsWith("M"))
                            sz = Int64.Parse(v.Substring(0, v.Length - 1)) * 1024 * 1024;
                        else if (v.ToUpper().EndsWith("MB"))
                            sz = Int64.Parse(v.Substring(0, v.Length - 2)) * 1024 * 1024;
                        else if (v.ToUpper().EndsWith("G"))
                            sz = Int64.Parse(v.Substring(0, v.Length - 1)) * 1024 * 1024 * 1024;
                        else if (v.ToUpper().EndsWith("GB"))
                            sz = Int64.Parse(v.Substring(0, v.Length - 2)) * 1024 * 1024 * 1024;
                        else sz = Int64.Parse(tokens[i + 2]);

                        current = new SizeCriterion
                        {
                            Size = sz,
                            Operator = (ComparisonOperator)EnumUtil.Parse(typeof(ComparisonOperator), tokens[i + 1])
                        };
                        i += 2;
                        stateStack.Push(ParseState.CriterionDone);
                        break;

                    case "filename":
                    case "name":
                        {
                            if (tokens.Length <= i + 2)
                                throw new ArgumentException(String.Join(" ", tokens, i, tokens.Length - i));

                            ComparisonOperator c =
                                (ComparisonOperator)EnumUtil.Parse(typeof(ComparisonOperator), tokens[i + 1]);

                            if (c != ComparisonOperator.NotEqualTo && c != ComparisonOperator.EqualTo)
                                throw new ArgumentException(String.Join(" ", tokens, i, tokens.Length - i));

                            string m = tokens[i + 2];

                            // handle single-quoted filespecs (used to include
                            // spaces in filename patterns)
                            if (m.StartsWith("'") && m.EndsWith("'"))
                            {
                                // trim off leading and trailing single quotes and
                                // revert the control characters to spaces.
                                m = m.Substring(1, m.Length - 2)
                                    .Replace("\u0006", " ");
                            }

                            // if (m.StartsWith("'"))
                            //     m = m.Replace("\u0006", " ");

                            current = new NameCriterion
                            {
                                MatchingFileSpec = m,
                                Operator = c
                            };
                            i += 2;
                            stateStack.Push(ParseState.CriterionDone);
                        }
                        break;

#if !SILVERLIGHT
                    case "attrs":
                    case "attributes":
#endif
                    case "type":
                        {
                            if (tokens.Length <= i + 2)
                                throw new ArgumentException(String.Join(" ", tokens, i, tokens.Length - i));

                            ComparisonOperator c =
                                (ComparisonOperator)EnumUtil.Parse(typeof(ComparisonOperator), tokens[i + 1]);

                            if (c != ComparisonOperator.NotEqualTo && c != ComparisonOperator.EqualTo)
                                throw new ArgumentException(String.Join(" ", tokens, i, tokens.Length - i));

#if SILVERLIGHT
                            current = (SelectionCriterion) new TypeCriterion
                                    {
                                        AttributeString = tokens[i + 2],
                                        Operator = c
                                    };
#else
                            current = (tok1 == "type")
                                ? (SelectionCriterion) new TypeCriterion
                                    {
                                        AttributeString = tokens[i + 2],
                                        Operator = c
                                    }
                                : (SelectionCriterion) new AttributesCriterion
                                    {
                                        AttributeString = tokens[i + 2],
                                        Operator = c
                                    };
#endif
                            i += 2;
                            stateStack.Push(ParseState.CriterionDone);
                        }
                        break;

                    case "":
                        // NOP
                        stateStack.Push(ParseState.Whitespace);
                        break;

                    default:
                        throw new ArgumentException("'" + tokens[i] + "'");
                }

                state = stateStack.Peek();
                if (state == ParseState.CriterionDone)
                {
                    stateStack.Pop();
                    if (stateStack.Peek() == ParseState.ConjunctionPending)
                    {
                        while (stateStack.Peek() == ParseState.ConjunctionPending)
                        {
                            var cc = critStack.Pop() as CompoundCriterion;
                            cc.Right = current;
                            current = cc; // mark the parent as current (walk up the tree)
                            stateStack.Pop();   // the conjunction is no longer pending

                            state = stateStack.Pop();
                            if (state != ParseState.CriterionDone)
                                throw new ArgumentException("??");
                        }
                    }
                    else stateStack.Push(ParseState.CriterionDone);  // not sure?
                }

                if (state == ParseState.Whitespace)
                    stateStack.Pop();
            }

            return current;
        }


        /// <summary>
        /// Returns a string representation of the FileSelector object.
        /// </summary>
        /// <returns>The string representation of the boolean logic statement of the file
        /// selection criteria for this instance. </returns>
        public override String ToString()
        {
            return "FileSelector("+_Criterion.ToString()+")";
        }


        private bool Evaluate(string filename)
        {
            // dinoch - Thu, 11 Feb 2010  18:34
            SelectorTrace("Evaluate({0})", filename);
            bool result = _Criterion.Evaluate(filename);
            return result;
        }

        [System.Diagnostics.Conditional("SelectorTrace")]
        private void SelectorTrace(string format, params object[] args)
        {
            if (_Criterion != null && _Criterion.Verbose)
                System.Console.WriteLine(format, args);
        }

        /// <summary>
        ///   Returns the names of the files in the specified directory
        ///   that fit the selection criteria specified in the FileSelector.
        /// </summary>
        ///
        /// <remarks>
        ///   This is equivalent to calling <see cref="SelectFiles(String, bool)"/>
        ///   with recurseDirectories = false.
        /// </remarks>
        ///
        /// <param name="directory">
        ///   The name of the directory over which to apply the FileSelector
        ///   criteria.
        /// </param>
        ///
        /// <returns>
        ///   A collection of strings containing fully-qualified pathnames of files
        ///   that match the criteria specified in the FileSelector instance.
        /// </returns>
        public System.Collections.Generic.ICollection<String> SelectFiles(String directory)
        {
            return SelectFiles(directory, false);
        }


        /// <summary>
        ///   Returns the names of the files in the specified directory that fit the
        ///   selection criteria specified in the FileSelector, optionally recursing
        ///   through subdirectories.
        /// </summary>
        ///
        /// <remarks>
        ///   This method applies the file selection criteria contained in the
        ///   FileSelector to the files contained in the given directory, and
        ///   returns the names of files that conform to the criteria.
        /// </remarks>
        ///
        /// <param name="directory">
        ///   The name of the directory over which to apply the FileSelector
        ///   criteria.
        /// </param>
        ///
        /// <param name="recurseDirectories">
        ///   Whether to recurse through subdirectories when applying the file
        ///   selection criteria.
        /// </param>
        ///
        /// <returns>
        ///   A collection of strings containing fully-qualified pathnames of files
        ///   that match the criteria specified in the FileSelector instance.
        /// </returns>
        public System.Collections.ObjectModel.ReadOnlyCollection<String>
            SelectFiles(String directory,
                        bool recurseDirectories)
        {
            if (_Criterion == null)
                throw new ArgumentException("SelectionCriteria has not been set");

            var list = new List<String>();
            try
            {
                if (Directory.Exists(directory))
                {
                    String[] filenames = Directory.GetFiles(directory);

                    // add the files:
                    foreach (String filename in filenames)
                    {
                        if (Evaluate(filename))
                            list.Add(filename);
                    }

                    if (recurseDirectories)
                    {
                        // add the subdirectories:
                        String[] dirnames = Directory.GetDirectories(directory);
                        foreach (String dir in dirnames)
                        {
                            if (this.TraverseReparsePoints
#if !SILVERLIGHT
                                || ((File.GetAttributes(dir) & FileAttributes.ReparsePoint) == 0)
#endif
                                )
                            {
                                // workitem 10191
                                if (Evaluate(dir)) list.Add(dir);
                                list.AddRange(this.SelectFiles(dir, recurseDirectories));
                            }
                        }
                    }
                }
            }
            // can get System.UnauthorizedAccessException here
            catch (System.UnauthorizedAccessException)
            {
            }
            catch (System.IO.IOException)
            {
            }

            return list.AsReadOnly();
        }
    }



    /// <summary>
    /// Summary description for EnumUtil.
    /// </summary>
    internal sealed class EnumUtil
    {
        private EnumUtil() { }
        /// <summary>
        ///   Returns the value of the DescriptionAttribute if the specified Enum
        ///   value has one.  If not, returns the ToString() representation of the
        ///   Enum value.
        /// </summary>
        /// <param name="value">The Enum to get the description for</param>
        /// <returns></returns>
        internal static string GetDescription(System.Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        /// <summary>
        ///   Converts the string representation of the name or numeric value of one
        ///   or more enumerated constants to an equivalent enumerated object.
        ///   Note: use the DescriptionAttribute on enum values to enable this.
        /// </summary>
        /// <param name="enumType">The System.Type of the enumeration.</param>
        /// <param name="stringRepresentation">
        ///   A string containing the name or value to convert.
        /// </param>
        /// <returns></returns>
        internal static object Parse(Type enumType, string stringRepresentation)
        {
            return Parse(enumType, stringRepresentation, false);
        }


#if SILVERLIGHT
       internal static System.Enum[] GetEnumValues(Type type)
        {
            if (!type.IsEnum)
                throw new ArgumentException("not an enum");

            return (
              from field in type.GetFields(BindingFlags.Public | BindingFlags.Static)
              where field.IsLiteral
              select (System.Enum)field.GetValue(null)
            ).ToArray();
        }

        internal static string[] GetEnumStrings<T>()
        {
            var type = typeof(T);
            if (!type.IsEnum)
                throw new ArgumentException("not an enum");

            return (
              from field in type.GetFields(BindingFlags.Public | BindingFlags.Static)
              where field.IsLiteral
              select field.Name
            ).ToArray();
        }
#endif

        /// <summary>
        ///   Converts the string representation of the name or numeric value of one
        ///   or more enumerated constants to an equivalent enumerated object.  A
        ///   parameter specified whether the operation is case-sensitive.  Note:
        ///   use the DescriptionAttribute on enum values to enable this.
        /// </summary>
        /// <param name="enumType">The System.Type of the enumeration.</param>
        /// <param name="stringRepresentation">
        ///   A string containing the name or value to convert.
        /// </param>
        /// <param name="ignoreCase">
        ///   Whether the operation is case-sensitive or not.</param>
        /// <returns></returns>
        internal static object Parse(Type enumType, string stringRepresentation, bool ignoreCase)
        {
            if (ignoreCase)
                stringRepresentation = stringRepresentation.ToLower();

#if SILVERLIGHT
            foreach (System.Enum enumVal in GetEnumValues(enumType))
#else
            foreach (System.Enum enumVal in System.Enum.GetValues(enumType))
#endif
            {
                string description = GetDescription(enumVal);
                if (ignoreCase)
                    description = description.ToLower();
                if (description == stringRepresentation)
                    return enumVal;
            }

            return System.Enum.Parse(enumType, stringRepresentation, ignoreCase);
        }
    }


#if DEMO
    internal class DemonstrateFileSelector
    {
        private string _directory;
        private bool _recurse;
        private bool _traverse;
        private bool _verbose;
        private string _selectionCriteria;
        private FileSelector f;

        public DemonstrateFileSelector()
        {
            this._directory = ".";
            this._recurse = true;
        }

        public DemonstrateFileSelector(string[] args) : this()
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch(args[i])
                {
                case"-?":
                    Usage();
                    Environment.Exit(0);
                    break;
                case "-d":
                    i++;
                    if (args.Length <= i)
                        throw new ArgumentException("-directory");
                    this._directory = args[i];
                    break;
                case "-norecurse":
                    this._recurse = false;
                    break;

                case "-j-":
                    this._traverse = false;
                    break;

                case "-j+":
                    this._traverse = true;
                    break;

                case "-v":
                    this._verbose = true;
                    break;

                default:
                    if (this._selectionCriteria != null)
                        throw new ArgumentException(args[i]);
                    this._selectionCriteria = args[i];
                    break;
                }

                if (this._selectionCriteria != null)
                    this.f = new FileSelector(this._selectionCriteria);
            }
        }


        internal static void Main(string[] args)
        {
            try
            {
                Console.WriteLine();
                new DemonstrateFileSelector(args).Run();
            }
            catch (Exception exc1)
            {
                Console.WriteLine("Exception: {0}", exc1.ToString());
                Usage();
            }
        }


        public void Run()
        {
            if (this.f == null)
                this.f = new FileSelector("name = *.jpg AND (size > 1000 OR atime < 2009-02-14-01:00:00)");

            this.f.TraverseReparsePoints = _traverse;
            this.f.Verbose = this._verbose;
            Console.WriteLine();
            Console.WriteLine(new String(':', 88));
            Console.WriteLine("Selecting files:\n" + this.f.ToString());
            var files = this.f.SelectFiles(this._directory, this._recurse);
            if (files.Count == 0)
            {
                Console.WriteLine("no files.");
            }
            else
            {
                Console.WriteLine("files: {0}", files.Count);
                foreach (string file in files)
                {
                    Console.WriteLine("  " + file);
                }
            }
        }

        internal static void Usage()
        {
            Console.WriteLine("FileSelector: select files based on selection criteria.\n");
            Console.WriteLine("Usage:\n  FileSelector <selectionCriteria>  [options]\n" +
                              "\n" +
                              "  -d <dir>   directory to select from (Default .)\n" +
                              " -norecurse  don't recurse into subdirs\n" +
                              " -j-         don't traverse junctions\n" +
                              " -v          verbose output\n");
        }
    }

#endif



}


