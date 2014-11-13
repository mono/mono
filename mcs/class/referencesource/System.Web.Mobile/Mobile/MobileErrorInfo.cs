//------------------------------------------------------------------------------
// <copyright file="MobileErrorInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Security.Permissions;

namespace System.Web.Mobile
{
    /*
     * Mobile Error Info
     * Contains information about an error that occurs in a mobile application.
     * This information can be used to format the error for the target device.
     *
     * 




*/

    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class MobileErrorInfo
    {
        /// <include file='doc\MobileErrorInfo.uex' path='docs/doc[@for="MobileErrorInfo.ContextKey"]/*' />
        public static readonly String ContextKey = "MobileErrorInfo";
        private static object _lockObject = new object();

        private const String _errorType = "Type";
        private const String _errorDescription = "Description";
        private const String _errorMiscTitle = "MiscTitle";
        private const String _errorMiscText = "MiscText";
        private const String _errorFile = "File";
        private const String _errorLineNumber = "LineNumber";
        private static Regex[] _searchExpressions = null;
        private static bool _searchExpressionsBuilt = false;
        private const int _expressionCount = 3;

        private StringDictionary _dictionary = new StringDictionary();

        internal MobileErrorInfo(Exception e)
        {
            // Don't want any failure to escape here...
            try
            {
                // For some reason, the compile exception lives in the
                // InnerException. 
                HttpCompileException compileException =
                    e.InnerException as HttpCompileException;

                if (compileException != null)
                {
                    this.Type = SR.GetString(SR.MobileErrorInfo_CompilationErrorType);
                    this.Description = SR.GetString(SR.MobileErrorInfo_CompilationErrorDescription);                
                    this.MiscTitle = SR.GetString(SR.MobileErrorInfo_CompilationErrorMiscTitle);
                    
                    CompilerErrorCollection errors = compileException.Results.Errors;
                
                    if (errors != null && errors.Count >= 1)
                    {
                        CompilerError error = errors[0];
                        this.LineNumber = error.Line.ToString(CultureInfo.InvariantCulture);
                        this.File = error.FileName;
                        this.MiscText = error.ErrorNumber + ":" + error.ErrorText;
                    }
                    else
                    {
                        this.LineNumber = SR.GetString(SR.MobileErrorInfo_Unknown);
                        this.File = SR.GetString(SR.MobileErrorInfo_Unknown);
                        this.MiscText = SR.GetString(SR.MobileErrorInfo_Unknown);
                    }

                    return;
                }

                HttpParseException parseException = e as HttpParseException; 
                if (parseException != null)
                {
                    this.Type = SR.GetString(SR.MobileErrorInfo_ParserErrorType);
                    this.Description = SR.GetString(SR.MobileErrorInfo_ParserErrorDescription);                
                    this.MiscTitle = SR.GetString(SR.MobileErrorInfo_ParserErrorMiscTitle);
                    this.LineNumber = parseException.Line.ToString(CultureInfo.InvariantCulture);
                    this.File = parseException.FileName;
                    this.MiscText = parseException.Message;
                    return;
                }

                // We try to use the hacky way of parsing an HttpException of an
                // unknown subclass.
                HttpException httpException = e as HttpException;
                if (httpException != null && ParseHttpException(httpException))
                {
                    return;
                }
                
            }
            catch
            {
                // Don't need to do anything here, just continue to base case
                // below. 
            }
            
            // Default to the most basic if none of the above succeed.
            this.Type = e.GetType().FullName;
            this.Description = e.Message;
            this.MiscTitle = SR.GetString(SR.MobileErrorInfo_SourceObject);
            String s = e.StackTrace;
            if(s != null) {
                int i = s.IndexOf('\r');
                if (i != -1)
                {
                    s = s.Substring(0, i);
                }
                this.MiscText = s;
            }
        }


        /// <include file='doc\MobileErrorInfo.uex' path='docs/doc[@for="MobileErrorInfo.this"]/*' />
        public String this[String key]
        {
            get
            {
                String s = _dictionary[key];
                return (s == null) ? String.Empty : s;
            }
            set
            {
                _dictionary[key] = value;
            }
        }

        /// <include file='doc\MobileErrorInfo.uex' path='docs/doc[@for="MobileErrorInfo.Type"]/*' />
        public String Type
        {
            get
            {
                return this[_errorType];
            }
            set
            {
                this[_errorType] = value;
            }
        }

        /// <include file='doc\MobileErrorInfo.uex' path='docs/doc[@for="MobileErrorInfo.Description"]/*' />
        public String Description
        {
            get
            {
                return this[_errorDescription];
            }
            set
            {
                this[_errorDescription] = value;
            }
        }

        /// <include file='doc\MobileErrorInfo.uex' path='docs/doc[@for="MobileErrorInfo.MiscTitle"]/*' />
        public String MiscTitle
        {
            get
            {
                return this[_errorMiscTitle];
            }
            set
            {
                this[_errorMiscTitle] = value;
            }
        }

        /// <include file='doc\MobileErrorInfo.uex' path='docs/doc[@for="MobileErrorInfo.MiscText"]/*' />
        public String MiscText
        {
            get
            {
                return this[_errorMiscText];
            }
            set
            {                  
                this[_errorMiscText] = value;
            }
        }

        /// <include file='doc\MobileErrorInfo.uex' path='docs/doc[@for="MobileErrorInfo.File"]/*' />
        public String File
        {
            get
            {
                return this[_errorFile];
            }
            set
            {
                this[_errorFile] = value;
            }
        }

        /// <include file='doc\MobileErrorInfo.uex' path='docs/doc[@for="MobileErrorInfo.LineNumber"]/*' />
        public String LineNumber
        {
            get
            {
                return this[_errorLineNumber];
            }
            set
            {
                this[_errorLineNumber] = value;
            }
        }

        // Return true if we succeed
        private bool ParseHttpException(HttpException e)
        {
            int i;
            Match match = null;

            String errorMessage = e.GetHtmlErrorMessage();
            if (errorMessage == null)
            {
                return false;
            }

            // Use regular expressions to scrape the message output
            // for meaningful data. One problem: Some parts of the 
            // output are optional, and any regular expression that
            // uses the ()? syntax doesn't pick it up. So, we have
            // to have all the different combinations of expressions,
            // and use each one in order.

            EnsureSearchExpressions();
            for (i = 0; i < _expressionCount; i++)
            {
                match = _searchExpressions[i].Match(errorMessage);
                if (match.Success)
                {
                    break;
                }
            }

            if (i == _expressionCount)
            {
                return false;
            }

            this.Type        = TrimAndClean(match.Result("${title}"));
            this.Description = TrimAndClean(match.Result("${description}"));
            if (i <= 1)
            {
                // These expressions were able to match the miscellaneous
                // title/text section.
                this.MiscTitle = TrimAndClean(match.Result("${misctitle}"));
                this.MiscText  = TrimAndClean(match.Result("${misctext}"));
            }
            if (i == 0)
            {
                // This expression was able to match the file/line # 
                // section.
                this.File        = TrimAndClean(match.Result("${file}"));
                this.LineNumber  = TrimAndClean(match.Result("${linenumber}"));
            }

            return true;
        }

        private static void EnsureSearchExpressions()
        {
            // Create precompiled search expressions. They're here
            // rather than in static variables, so that we can load
            // them from resources on demand. But once they're loaded,
            // they're compiled and always available.

            if (!_searchExpressionsBuilt)
            {
                lock(_lockObject)
                {
                    if (!_searchExpressionsBuilt)
                    {
                        // 






                        // Why three similar expressions? See ParseHttpException above.

                        _searchExpressions = new Regex[_expressionCount];

                        _searchExpressions[0] = new Regex(
                            "<title>(?'title'.*?)</title>.*?" +
                                ": </b>(?'description'.*?)<br>.*?" + 
                                "(<b>(?'misctitle'.*?): </b>(?'misctext'.*?)<br)+.*?" +
                                "(Source File:</b>(?'file'.*?)&nbsp;&nbsp; <b>Line:</b>(?'linenumber'.*?)<br)+",
                            RegexOptions.Singleline | 
                                RegexOptions.IgnoreCase | 
                                RegexOptions.CultureInvariant |
                                RegexOptions.Compiled);

                        _searchExpressions[1] = new Regex(
                            "<title>(?'title'.*?)</title>.*?" +
                                ": </b>(?'description'.*?)<br>.*?" + 
                                "(<b>(?'misctitle'.*?): </b>(?'misctext'.*?)<br)+.*?",
                            RegexOptions.Singleline | 
                                RegexOptions.IgnoreCase | 
                                RegexOptions.CultureInvariant |
                                RegexOptions.Compiled);

                        _searchExpressions[2] = new Regex(
                            "<title>(?'title'.*?)</title>.*?: </b>(?'description'.*?)<br>",
                            RegexOptions.Singleline | 
                            RegexOptions.IgnoreCase | 
                            RegexOptions.CultureInvariant |
                                RegexOptions.Compiled);

                        _searchExpressionsBuilt = true;
                    }
                }
            }
        }

        private static String TrimAndClean(String s)
        {
            return s.Replace("\r\n", " ").Trim();
        }
    }
}
    

