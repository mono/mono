//------------------------------------------------------------------------------
// <copyright file="XslException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace System.Xml.Xsl {
    using Res = System.Xml.Utils.Res;

    [Serializable]
    internal class XslTransformException : XsltException {

        protected XslTransformException(SerializationInfo info, StreamingContext context)
            : base(info, context) {}

        public XslTransformException(Exception inner, string res, params string[] args)
            : base(CreateMessage(res, args), inner) {}

        public XslTransformException(string message)
            : base(CreateMessage(message, null), null) {}

        internal XslTransformException(string res, params string[] args)
            : this(null, res, args) {}

        internal static string CreateMessage(string res, params string[] args) {
            string message = null;

            try {
                message = Res.GetString(res, args);
            }
            catch (MissingManifestResourceException) {
            }

            if (message != null) {
                return message;
            }

            StringBuilder sb = new StringBuilder(res);
            if (args != null && args.Length > 0) {
                Debug.Fail("Resource string '" + res + "' was not found");
                sb.Append('(');
                sb.Append(args[0]);
                for (int idx = 1; idx < args.Length; idx++) {
                    sb.Append(", ");
                    sb.Append(args[idx]);
                }
                sb.Append(')');
            }
            return sb.ToString();
        }

        internal virtual string FormatDetailedMessage() {
            return Message;
        }

        public override string ToString() {
            string result = this.GetType().FullName;
            string info = FormatDetailedMessage();
            if (info != null && info.Length > 0) {
                result += ": " + info;
            }
            if (InnerException != null) {
                result += " ---> " + InnerException.ToString() + Environment.NewLine + "   " + CreateMessage(Res.Xml_EndOfInnerExceptionStack);
            }
            if (StackTrace != null) {
                result += Environment.NewLine + StackTrace;
            }
            return result;
        }
    }

    [Serializable]
    internal class XslLoadException : XslTransformException {
        ISourceLineInfo lineInfo;

        protected XslLoadException (SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            bool hasLineInfo = (bool) info.GetValue("hasLineInfo", typeof(bool));

            if (hasLineInfo) {
                string  uriString;
                int     startLine, startPos, endLine, endPos;

                uriString   = (string)   info.GetValue("Uri"        , typeof(string ));
                startLine   = (int)      info.GetValue("StartLine"  , typeof(int    ));
                startPos    = (int)      info.GetValue("StartPos"   , typeof(int    ));
                endLine     = (int)      info.GetValue("EndLine"    , typeof(int    ));
                endPos      = (int)      info.GetValue("EndPos"     , typeof(int    ));

                lineInfo = new SourceLineInfo(uriString, startLine, startPos, endLine, endPos);
            }
        }

        [SecurityPermissionAttribute(SecurityAction.LinkDemand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);
            info.AddValue("hasLineInfo"  , lineInfo != null);

            if (lineInfo != null) {
                info.AddValue("Uri"      , lineInfo.Uri);
                info.AddValue("StartLine", lineInfo.Start.Line);
                info.AddValue("StartPos" , lineInfo.Start.Pos );
                info.AddValue("EndLine"  , lineInfo.End.Line);
                info.AddValue("EndPos"   , lineInfo.End.Pos );
            }
        }

        internal XslLoadException(string res, params string[] args)
            : base(null, res, args) {}

        internal XslLoadException(Exception inner, ISourceLineInfo lineInfo)
            : base(inner, Res.Xslt_CompileError2, null)
        {
            SetSourceLineInfo(lineInfo);
        }

        internal XslLoadException(CompilerError error)
            : base(Res.Xml_UserException, new string[] { error.ErrorText })
        {
            int errorLine = error.Line;
            int errorColumn = error.Column;

            if (errorLine == 0) {
                // If the compiler reported error on Line 0 - ignore columns, 
                //   0 means it doesn't know where the error was and our SourceLineInfo
                //   expects either all zeroes or all non-zeroes
                errorColumn = 0;
            }
            else {
                if (errorColumn == 0) {
                    // In this situation the compiler returned for example Line 10, Column 0.
                    // This means that the compiler knows the line of the error, but it doesn't
                    //   know (or support) the column part of the location.
                    // Since we don't allow column 0 (as it's invalid), let's turn it into 1 (the begining of the line)
                    errorColumn = 1;
                }
            }

            SetSourceLineInfo(new SourceLineInfo(error.FileName, errorLine, errorColumn, errorLine, errorColumn));
        }

        internal void SetSourceLineInfo(ISourceLineInfo lineInfo) {
            Debug.Assert(lineInfo == null || lineInfo.Uri != null);
            this.lineInfo = lineInfo;
        }

        public override string SourceUri {
            get { return lineInfo != null ? lineInfo.Uri : null; }
        }

        public override int LineNumber {
            get { return lineInfo != null ? lineInfo.Start.Line : 0; }
        }

        public override int LinePosition {
            get { return lineInfo != null ? lineInfo.Start.Pos : 0; }
        }

        private static string AppendLineInfoMessage(string message, ISourceLineInfo lineInfo) {
            if (lineInfo != null) {
                string fileName = SourceLineInfo.GetFileName(lineInfo.Uri);
                string lineInfoMessage = CreateMessage(Res.Xml_ErrorFilePosition, fileName, lineInfo.Start.Line.ToString(CultureInfo.InvariantCulture), lineInfo.Start.Pos.ToString(CultureInfo.InvariantCulture));
                if (lineInfoMessage != null && lineInfoMessage.Length > 0) {
                    if (message.Length > 0 && !XmlCharType.Instance.IsWhiteSpace(message[message.Length - 1])) {
                        message += " ";
                    }
                    message += lineInfoMessage;
                }
            }
            return message;
        }

        internal static string CreateMessage(ISourceLineInfo lineInfo, string res, params string[] args) {
            return AppendLineInfoMessage(CreateMessage(res, args), lineInfo);
        }

        internal override string FormatDetailedMessage() {
            return AppendLineInfoMessage(Message, lineInfo);
        }
    }
}
