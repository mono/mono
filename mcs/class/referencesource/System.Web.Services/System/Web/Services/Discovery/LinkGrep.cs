//------------------------------------------------------------------------------
// <copyright file="LinkGrep.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Discovery {

    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Web.Services.Protocols;
    using System.Globalization;
    
    internal class LinkGrep {
        private LinkGrep() { }

        private static string ReadEntireStream(TextReader input) {
            char[] buffer = new char[4096];
            int bufferpos = 0;
            int delta;
    
            // 
            for (;;) {
                delta = input.Read(buffer, bufferpos, buffer.Length - bufferpos);
                
                if (delta == 0)
                    break;
    
                bufferpos += delta;
    
                if (bufferpos == buffer.Length) {
                    char[] newbuf = new char[buffer.Length * 2];
                    System.Array.Copy(buffer, 0, newbuf, 0, buffer.Length);
                    buffer = newbuf;
                }
            }
    
            return new string(buffer, 0, bufferpos);
        }

        internal static string SearchForLink(Stream stream) {
            string text = null;
            text = ReadEntireStream(new StreamReader(stream));

            int textpos = 0;

            Match match;

            if ((match = doctypeDirectiveRegex.Match(text, textpos)).Success) {
                textpos += match.Length;
            }

            bool oneMatch;
            for (;;) {
                
                // Reset match flag
                oneMatch = false;
                
                // 1: scan for text up to the next tag.
    
                // First case: check for whitespace going all the way to the next tag
                
                if ((match = whitespaceRegex.Match(text, textpos)).Success) {
                    oneMatch = true;
                }
                
                // Second case: there may be some nonwhitespace; scan it
                
                else if ((match = textRegex.Match(text, textpos)).Success) {
                    oneMatch = true;
                }
    
                // we might be done now
                
                textpos += match.Length;
                if (textpos == text.Length)
                    break;
               
                // 2: handle constructs that start with <
    
                // First, check to see if it's a tag
    
                if ((match = tagRegex.Match(text, textpos)).Success)
                {
                    oneMatch = true;
                    string tag = match.Groups["tagname"].Value;

                    if (String.Compare(tag, "link", StringComparison.OrdinalIgnoreCase) == 0) {
                        CaptureCollection attrnames = match.Groups["attrname"].Captures;
                        CaptureCollection attrvalues = match.Groups["attrval"].Captures;

                        int count = attrnames.Count;
                        bool rightType = false;
                        bool rightRel = false;
                        string href = null;
                        for (int i = 0; i < count; i++) {
                            string attrName = attrnames[i].ToString();
                            string attrValue = attrvalues[i].ToString();
                            if (String.Compare(attrName, "type", StringComparison.OrdinalIgnoreCase) == 0 &&
                                ContentType.MatchesBase(attrValue, ContentType.TextXml)) {
                                rightType = true;
                            }
                            else if (String.Compare(attrName, "rel", StringComparison.OrdinalIgnoreCase) == 0 &&
                                String.Compare(attrValue, "alternate", StringComparison.OrdinalIgnoreCase) == 0) {
                                rightRel = true;
                            }
                            else if (String.Compare(attrName, "href", StringComparison.OrdinalIgnoreCase) == 0) {
                                href = attrValue;
                            }

                            if (rightType && rightRel && href != null) {
                                // Got a link to a disco file!
                                return href;
                            }
                        }
                    }
                    else if (tag == "body") {
                        // If body begins, get out, since link tags should only be defined in the head
                        break;
                    }

                }
    
                // check to see if it's an end tag
                
                else if ((match = endtagRegex.Match(text, textpos)).Success) {
                    oneMatch = true;
                }
    
                // check to see if it's a comment
    
                else if ((match = commentRegex.Match(text, textpos)).Success) {
                    oneMatch = true;
                }
               
                // we might be done now
                
                textpos += match.Length;

                if (textpos == text.Length)
                    break;

                // If we couldn't get one single match, it means that it's probably not HTML, so bail
                if (!oneMatch) {
                    break;
                }
            }

            return null;
        }
        
        private readonly static Regex tagRegex = new Regex
        (
            "\\G<" +                                    // leading <
            "(?<prefix>[\\w:.-]+(?=:)|):?" +            // optional prefix:
            "(?<tagname>[\\w.-]+)" +                    // tagname
    
            "(?:\\s+" +                                 // zero or more attributes
                "(?<attrprefix>[\\w:.-]+(?=:)|):?" +    //     optional attrprefix:
                "(?<attrname>[\\w.-]+)" +               //     attrname
                "\\s*=\\s*" +                           //     required equals
                "(?:" +                                 //     quoted value
                    "\"(?<attrval>[^\"]*)\"" +          //          double quoted attrval
                    "|'(?<attrval>[^\']*)'" +           //          single quoted attrval
                    "|(?<attrval>[a-zA-Z0-9\\-._:]+)" + //          attrval with no quotes (SGML-approved chars)
                ")" +                                   //     end quoted value
            ")*" +                                      // end attribute
            
            "\\s*(?<empty>/)?>"                         // optional trailing /, and trailing >
        );
            
        private readonly static Regex doctypeDirectiveRegex = new Regex
        (
            @"\G<!doctype\b(([\s\w]+)|("".*""))*>", 
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace
        );

        private readonly static Regex endtagRegex = new Regex
        (
            "\\G</" +                                   // leading </
            "(?<prefix>[\\w:-]+(?=:)|):?" +             // optional prefix:
            "(?<tagname>[\\w-]+)\\s*>"                 // tagname
        );
        
        private readonly static Regex commentRegex = new Regex
        (
            "\\G<!--" +                                 // leading <!--
            "(?>[^-]*-)+?" +                            // one or more chunks of text ending with -, minimal
            "->"                                        // trailing ->
    
        );
        
        private readonly static Regex whitespaceRegex = new Regex
        (
            "\\G\\s+" +                                 // at least one char of whitespace
            "(?=<|\\Z)"                                 // ending with either '<' or the end of the string
        );
        
        private readonly static Regex textRegex = new Regex
        (
            "\\G[^<]+"                                  // at least one char on non-'<', maximal
        );
    }
}
