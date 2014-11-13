//------------------------------------------------------------------------------
// <copyright file="SimpleParser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.RegularExpressions;
using System.Web.Util;
using System.Web.UI.MobileControls;

namespace System.Web.UI.Design.MobileControls.Util
{
    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class SimpleParser : BaseParser
    {
        private const int _stackInitialSize = 100;
        private static Regex _unclosedTagRegex = null;

        private const RegexOptions _options = 
            RegexOptions.Singleline | RegexOptions.Multiline;

        private const String _pattern = 
            @"\G<(?<tagname>[\w:\.]+)" +
            @"(" +
            @"\s+(?<attrname>\w[-\w:]*)(" +                     // Attribute name
            @"\s*=\s*""(?<attrval>[^""]*)""|" +                 // ="bar" attribute value
            @"\s*=\s*'(?<attrval>[^']*)'|" +                    // ='bar' attribute value
            @"\s*=\s*(?<attrval><%#.*?%>)|" +                   // =<%#expr%> attribute value
            @"\s*=\s*(?!'|"")(?<attrval>[^\s=/>]*)(?!'|"")|" +  // =bar attribute value
            @"(?<attrval>\s*?)" +                               // no attrib value (with no '=')
            @")" +
            @")*" +
            @"\s*(?<empty>)?>";
            //@"\s*(?<empty>/)?>";

        private static ElementTable _endTagOptionalElement = null;

        private readonly static Regex _tagRegex = new TagRegex();
        private readonly static Regex _directiveRegex = new DirectiveRegex();
        private readonly static Regex _endtagRegex = new EndTagRegex();
        private readonly static Regex _aspCodeRegex = new AspCodeRegex();
        private readonly static Regex _aspExprRegex = new AspExprRegex();
        private readonly static Regex _databindExprRegex = new DatabindExprRegex();
        private readonly static Regex _commentRegex = new CommentRegex();
        private readonly static Regex _includeRegex = new IncludeRegex();
        private readonly static Regex _textRegex = new TextRegex();

        // Regexes used in DetectSpecialServerTagError
        private readonly static Regex _gtRegex = new GTRegex();
        private readonly static Regex _ltRegex = new LTRegex();
        private readonly static Regex _serverTagsRegex = new ServerTagsRegex();
        private readonly static Regex _runatServerRegex = new RunatServerRegex();

/* Regex patterns
        AspCodeRegex : \G<%(?!@)(?<code>.*?)%>
        AspExprRegex : \G<%\s*?=(?<code>.*?)?%>
        CommentRegex : \G<%--(([^-]*)-)*?-%>
        DataBindExprRegex : \G<%#(?<code>.*?)?%>
        DirectiveRegex : \G<%\s*@(\s*(?<attrname>\w+(?=\W))(\s*(?<equal>=)\s*"(?<attrval>[^"]*)"|\s*(?<equal>=)\s*'(?<attrval>[^']*)'|\s*(?<equal>=)\s*(?<attrval>[^\s%>]*)|(?<equal>)(?<attrval>\s*?)))*\s*?%>
        EndTagRegex : \G</(?<tagname>[\w:\.]+)\s*>
        GTRegex : [^%]>
        IncludeRegex : \G<!--\s*#(?i:include)\s*(?<pathtype>[\w]+)\s*=\s*["']?(?<filename>[^\"']*?)["']?\s*-->
        LTRegex : <
        RunATServerRegex : runat\W*server
        ServerTagsRegex : <%(?!#)(([^%]*)%)*?>
        TagRegex : \G<(?<tagname>[\w:\.]+)(\s+(?<attrname>[-\w]+)(\s*=\s*"(?<attrval>[^"]*)"|\s*=\s*'(?<attrval>[^']*)'|\s*=\s*(?<attrval><%#.*?%>)|\s*=\s*(?<attrval>[^\s=/>]*)|(?<attrval>\s*?)))*\s*(?<empty>/)?>
        TextRegex : \G[^<]+
                    
        //SimpleDirectiveRegex simpleDirectiveRegex = new SimpleDirectiveRegex();
*/
        // static helper type should not be instantiated.
        private SimpleParser() {
        }

        static SimpleParser()
        {
            _unclosedTagRegex = new Regex(_pattern, _options);
            _endTagOptionalElement = new ElementTable();

            /* following defnitions from MSDN Online WorkShop
                http://msdn.microsoft.com/workshop/c-frame.htm#/workshop/author/default.asp
            */
            _endTagOptionalElement.AddRange(
                new String[] {
                                 "area", "base", "basefront", "bgsound", "br",
                                 "col", "colgroup", "dd", "dt", "embed", "frame",
                                 "hr", "img", "input", "isindex", "li", "link",
                                 "meta", "option", "p", "param", "rt"
                             });
                             
        }

        /// <summary>
        ///     Simple parsing to check if input fragment is well-formed,
        ///     HTML elements that do not required end tags (i.e. <BR>)
        ///     will be ignored by this parser.
        /// </summary>
        /// <param name="text">
        ///     text being parsed
        /// </param>
        internal static bool IsWellFormed(String text)
        {
            int textPos = 0;
            TagStack stack = new TagStack();

            for (;;) 
            {
                Match match = null;

                // 1: scan for text up to the next tag.
                if ((match = _textRegex.Match(text, textPos)).Success)
                {
                    textPos = match.Index + match.Length;
                }

                // we might be done now
                if (textPos == text.Length)
                {
                    while (!stack.IsEmpty())
                    {
                        if (!IsEndTagOptional(stack.Pop()))
                        {
                            return false;
                        }
                    }
                    return true;
                }

                // First check if it's a unclosed tag (i.e. <mobile:Form >)
                if ((match = _unclosedTagRegex.Match(text, textPos)).Success)
                {
                    String startTag = match.Groups["tagname"].Value;
                    stack.Push(startTag);
                }

                // Check to see if it's a tag
                else if ((match = _tagRegex.Match(text, textPos)).Success)
                {
                    // skip
                }

                // Check to see if it's an end tag
                else if ((match = _endtagRegex.Match(text, textPos)).Success)
                {
                    String endTag = match.Groups["tagname"].Value;
                    bool matched = false;

                    while (!stack.IsEmpty())
                    {
                        String startTag = stack.Pop();

                        if (String.Compare(endTag, startTag, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            if (IsEndTagOptional(startTag))
                            {
                                continue;
                            }

                            // no match against start tag that requires an end tag
                            return false;
                        }

                        // we found a match here.
                        matched = true;
                        break;
                    }

                    if (!matched && stack.IsEmpty())
                    {
                        return false;
                    }
                }

                // Check to see if it's a directive (i.e. <%@ %> block)
                else if ((match = _directiveRegex.Match(text, textPos)).Success)
                {
                    // skip
                }

                // Check to see if it's a server side include
                // e.g. <!-- #include file="foo.inc" -->
                else if ((match = _includeRegex.Match(text, textPos)).Success)
                {
                    // skip it
                }

                // Check to see if it's a comment (<%-- --%> block
                // e.g. <!-- Blah! -->
                else if ((match = _commentRegex.Match(text, textPos)).Success)
                {
                    // skip
                }

                // Check to see if it's an asp expression block (i.e. <%= %> block)
                else if ((match = _aspExprRegex.Match(text, textPos)).Success)
                {
                    // skip
                }

                // Check to see if it's a databinding expression block (i.e. <%# %> block)
                // This does not include <%# %> blocks used as values for
                // attributes of server tags.
                else if ((match = _databindExprRegex.Match(text, textPos)).Success)
                {
                    // skip
                }

                // Check to see if it's an asp code block
                else if ((match = _aspCodeRegex.Match(text, textPos)).Success)
                {
                    // skip
                }

                // Did we process the block that started with a '<'?
                if (match == null || !match.Success) 
                {
                    // Skip the '<'
                    textPos++;
                }
                else 
                {
                    textPos = match.Index + match.Length;
                }

                // we might be done now
                if (textPos == text.Length)
                {
                    while (!stack.IsEmpty())
                    {
                        if (!IsEndTagOptional(stack.Pop()))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
        }

        private static bool IsEndTagOptional(String element)
        {
            return (_endTagOptionalElement.Contains(element));
        }
    
        /// <summary>
        ///     Private class used to store lowercase tags in a stack
        ///     return String.Empty if stack is empty 
        /// </summary>
        [
            System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
            Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
        ]
        private class TagStack
        {
            private Stack _tagStack = null;

            internal TagStack() : this(_stackInitialSize) 
            {
            }

            internal TagStack(int initialCapacity)
            {
                _tagStack = new Stack(initialCapacity);
            }

            internal void Push(String tagName)
            {
                _tagStack.Push(tagName.ToLower(CultureInfo.InvariantCulture));
            }

            internal String Pop()
            {
                if (IsEmpty())
                {
                    return String.Empty;
                }
                return (String)_tagStack.Pop();
            }

            internal bool IsEmpty()
            {
                return (_tagStack.Count == 0);
            }
        }

        /// <summary>
        ///     Private class used to store recognizable lowercase elements
        ///     return true if element is in the list, otherwise false 
        /// </summary>
        [
            System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
            Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
        ]
        private class ElementTable
        {
            private Hashtable _table = null;

            internal ElementTable() : this(_stackInitialSize) 
            {}

            internal ElementTable(int initialCapacity)
            {
                _table = new Hashtable(initialCapacity, StringComparer.OrdinalIgnoreCase);
            }

            internal void Add(String key)
            {
                _table.Add(key, true);
            }

            internal bool Contains(String key)
            {
                return (_table.Contains(key));
            }

            internal void AddRange(String[] keysCollection)
            {
                foreach (String key in keysCollection)
                {
                    this.Add(key);
                }
            }
        }
    }
}
