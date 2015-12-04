//---------------------------------------------------------------------
// <copyright file="CommentEmitter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.CodeDom;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.Common.Utils;
using System.Data.EntityModel.SchemaObjectModel;
using System.Globalization;
using System.Data.Entity.Design.Common;
using System.Data.Entity.Design;
using System.Data.Metadata.Edm;
using System.Reflection;
using System.Xml;
using System.IO;


namespace System.Data.EntityModel.Emitters
{
    /// <summary>
    /// static helper class for emitting comments.
    /// </summary>
    internal static class CommentEmitter
    {
        #region Static Fields
        private static readonly Regex LeadingBlanks = new Regex(@"^(?<LeadingBlanks>\s{1,})\S", RegexOptions.Singleline | RegexOptions.Compiled);
        #endregion

        #region Public Methods
        /// <summary>
        /// emit all the documentation comments for an element's documentation child
        /// (if the element does not have a documentation child emit some standard "missing comments" comment
        /// </summary>
        /// <param name="element">the element whose documentation is to be displayed</param>
        /// <param name="commentCollection">the comment collection of the CodeDom object to be commented</param>
        public static void EmitSummaryComments(MetadataItem item, CodeCommentStatementCollection commentCollection)
        {
            Debug.Assert(item != null, "item parameter is null");
            Debug.Assert(commentCollection != null, "commentCollection parameter is null");

            Documentation documentation = GetDocumentation(item);
            string [] summaryComments = null;
            if (documentation != null && !MetadataUtil.IsNullOrEmptyOrWhiteSpace(documentation.Summary)) 
            {
                // we have documentation to emit
                summaryComments = GetFormattedLines(documentation.Summary, true);
            }
            else
            {
                string summaryComment;
                // no summary content, so use a default
                switch (item.BuiltInTypeKind)
                {
                    case BuiltInTypeKind.EdmProperty:
                        summaryComment = Strings.MissingPropertyDocumentation(((EdmProperty)item).Name);
                        break;
                    case BuiltInTypeKind.ComplexType:
                        summaryComment = Strings.MissingComplexTypeDocumentation(((ComplexType)item).FullName);
                        break;
                    default:
                        {
                            PropertyInfo pi = item.GetType().GetProperty("FullName");
                            if (pi == null)
                            {
                                pi = item.GetType().GetProperty("Name");
                            }

                            object value = null;
                            if (pi != null)
                            {
                                value = pi.GetValue(item, null);
                            }


                            if (value != null)
                            {
                                summaryComment = Strings.MissingDocumentation(value.ToString());
                            }
                            else
                            {
                                summaryComment = Strings.MissingDocumentationNoName;
                            }
                        }
                        break;
                }
                summaryComments = new string[] { summaryComment };
            }
            EmitSummaryComments(summaryComments, commentCollection);
            EmitOtherDocumentationComments(documentation, commentCollection);
        }

        private static Documentation GetDocumentation(MetadataItem item)
        {
            if (item is Documentation)
                return (Documentation)item;
            else
                return item.Documentation;
        }

        /// <summary>
        /// Emit summary comments from a string
        /// </summary>
        /// <param name="summaryComments">the summary comments to be emitted</param>
        /// <param name="commentCollection">the comment collection of the CodeDom object to be commented</param>
        public static void EmitSummaryComments(string summaryComments, CodeCommentStatementCollection commentCollection)
        {
            Debug.Assert(commentCollection != null, "commentCollection parameter is null");

            if (string.IsNullOrEmpty(summaryComments) || string.IsNullOrEmpty(summaryComments = summaryComments.TrimEnd()))
                return;

            EmitSummaryComments(SplitIntoLines(summaryComments), commentCollection);
        }

        /// <summary>
        /// Emit some lines of comments
        /// </summary>
        /// <param name="commentLines">the lines of comments to emit</param>
        /// <param name="commentCollection">the comment collection of the CodeDom object to be commented</param>
        /// <param name="docComment">true if the comments are 'documentation' comments</param>
        public static void EmitComments(string[] commentLines, CodeCommentStatementCollection commentCollection, bool docComment)
        {
            Debug.Assert(commentLines != null, "commentLines parameter is null");
            Debug.Assert(commentCollection != null, "commentCollection parameter is null");

            foreach (string comment in commentLines)
            {
                commentCollection.Add(new CodeCommentStatement(comment, docComment));
            }
        }

        /// <summary>
        /// Emit documentation comments for a method parameter
        /// </summary>
        /// <param name="parameter">the parameter being commented</param>
        /// <param name="comment">the comment text</param>
        /// <param name="commentCollection">the comment collection of the CodeDom object to be commented</param>
        public static void EmitParamComments(CodeParameterDeclarationExpression parameter, string comment,
            CodeCommentStatementCollection commentCollection)
        {
            Debug.Assert(parameter != null, "parameter parameter is null");
            Debug.Assert(comment != null, "comment parameter is null");

            string paramComment = string.Format(System.Globalization.CultureInfo.CurrentCulture,
                "<param name=\"{0}\">{1}</param>", parameter.Name, comment);
            commentCollection.Add(new CodeCommentStatement(paramComment, true));
        }

        /// <summary>
        /// 'Format' a string of text into lines: separates in to lines on '\n', removes '\r', and removes common leading blanks.
        /// </summary>
        /// <param name="escapeForXml">if true characters troublesome for xml are converted to entities</param>
        /// <param name="text">the text to be formatted</param>
        /// <returns>the formatted lines</returns>
        public static string[] GetFormattedLines(string text, bool escapeForXml)
        {
#if false
            if ( text.IndexOf("\n") >= 0 )
                Console.WriteLine("GetFormattedText(\""+text.Replace("\n","\\n").Replace("\r","\\r")+"\","+escapeForXml+")");
#endif
            Debug.Assert(!string.IsNullOrEmpty(text));

            // nothing in, almost nothing out.
            if (StringUtil.IsNullOrEmptyOrWhiteSpace(text))
                return new string[] { "" };

            // normalize CRLF and LFCRs to LFs (we just remove all the crs, assuming there are no extraneous ones) and remove trailing spaces
            text = text.Replace("\r", "");

            // remove leading and.or trailing line ends to get single line for:
            // <documentation>
            // text
            // <documentation>
            bool trim = false;
            int start = text.IndexOf('\n');
            if (start >= 0 && MetadataUtil.IsNullOrEmptyOrWhiteSpace(text, 0, start + 1))
            {
                ++start;
                trim = true;
            }
            else
            {
                start = 0;
            }
            int last = text.LastIndexOf('\n');
            if (last > start - 1 && MetadataUtil.IsNullOrEmptyOrWhiteSpace(text, last))
            {
                --last;
                trim = true;
            }
            else
            {
                last = text.Length - 1;
            }
            if (trim)
            {
                Debug.Assert(start <= last);
                text = text.Substring(start, last - start + 1);
            }

            // break into lines (preversing blank lines and preping text for being in xml comments)
            if (escapeForXml)
                text = MetadataUtil.Entityize(text);
            string[] lines = SplitIntoLines(text);

            if (lines.Length == 1)
            {
                lines[0] = lines[0].Trim();
                return lines;
            }

            // find the maximum leading whitespace substring (ignoring blank lines)
            string leadingBlanks = null;
            foreach (string line in lines)
            {
                // is an empty line
                if (MetadataUtil.IsNullOrEmptyOrWhiteSpace(line))
                    continue;

                // find the leading whitespace substring
                Match match = LeadingBlanks.Match(line);
                if (!match.Success)
                {
                    //none, we're done
                    leadingBlanks = "";
                    break;
                }

                if (leadingBlanks == null)
                {
                    // this is first non-empty line
                    leadingBlanks = match.Groups["LeadingBlanks"].Value;
                    continue;
                }

                // use the leadingBlanks if it matched the new one or it is a leading substring of the new one
                string leadingBlanks2 = match.Groups["LeadingBlanks"].Value;
                if (leadingBlanks2 == leadingBlanks || leadingBlanks2.StartsWith(leadingBlanks, StringComparison.Ordinal))
                    continue;

                if (leadingBlanks.StartsWith(leadingBlanks2, StringComparison.OrdinalIgnoreCase))
                {
                    // the current leading whitespace string is a leading substring of leadingBlanks. use the new one
                    leadingBlanks = leadingBlanks2;
                    continue;
                }

                // find longest leading common substring and use that.
                int minLength = Math.Min(leadingBlanks.Length, leadingBlanks2.Length);
                for (int j = 0; j < minLength; ++j)
                {
                    if (leadingBlanks[j] != leadingBlanks2[j])
                    {
                        if (j == 0)
                            leadingBlanks = "";
                        else
                            leadingBlanks = leadingBlanks.Substring(0, j);
                        break;
                    }
                }

                // if we've reduced the leading substring to an empty string, we're done.
                if (string.IsNullOrEmpty(leadingBlanks))
                    break;
            }

            // remove the leading whitespace substring and remove any trailing blanks.
            int numLeadingCharsToRemove = leadingBlanks.Length;
            for (int i = 0; i < lines.Length; ++i)
            {
                if (lines[i].Length >= numLeadingCharsToRemove)
                    lines[i] = lines[i].Substring(numLeadingCharsToRemove);
                lines[i] = lines[i].TrimEnd();
            }
            return lines;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Emit the other (than Summary) documentation comments from a Documentation element
        /// </summary>
        /// <param name="documentation">the schema Docuementation element</param>
        /// <param name="commentCollection">the comment collection of the CodeDom object to be commented</param>
        private static void EmitOtherDocumentationComments(Documentation documentation, CodeCommentStatementCollection commentCollection)
        {
            Debug.Assert(commentCollection != null);
            if (documentation == null)
                return;

            if (!string.IsNullOrEmpty(documentation.LongDescription))
                EmitXmlComments("LongDescription", GetFormattedLines(documentation.LongDescription, true), commentCollection);
        }

        /// <summary>
        /// Emit the summary comments
        /// </summary>
        /// <param name="summaryComments"></param>
        /// <param name="commentCollection">the comment collection of the CodeDom object to be commented</param>
        private static void EmitSummaryComments(string[] summaryComments, CodeCommentStatementCollection commentCollection)
        {
            Debug.Assert(summaryComments != null);
            Debug.Assert(commentCollection != null);

            EmitXmlComments("summary", summaryComments, commentCollection);
        }

        /// <summary>
        /// emit documentation comments between xml open and close tags
        /// </summary>
        /// <param name="tag">the xml tag name</param>
        /// <param name="summaryComments">the lines of comments to emit</param>
        /// <param name="commentCollection">the comment collection of the CodeDom object to be commented</param>
        private static void EmitXmlComments(string tag, string[] summaryComments, CodeCommentStatementCollection commentCollection)
        {
            Debug.Assert(tag != null);
            Debug.Assert(summaryComments != null);
            Debug.Assert(commentCollection != null);

            commentCollection.Add(new CodeCommentStatement(string.Format(CultureInfo.InvariantCulture, "<{0}>", tag), true));
            EmitComments(summaryComments, commentCollection, true);
            commentCollection.Add(new CodeCommentStatement(string.Format(CultureInfo.InvariantCulture, "</{0}>", tag), true));
        }

        /// <summary>
        /// split a string into lines on '\n' chars and remove '\r' chars 
        /// </summary>
        /// <param name="text">the string to split</param>
        /// <returns>the split string</returns>
        private static string[] SplitIntoLines(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new string[] { "" };

            return text.Replace("\r", "").Split('\n');
        }
        #endregion
    }
}
