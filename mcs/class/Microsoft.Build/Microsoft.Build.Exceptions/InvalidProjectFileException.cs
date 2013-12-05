//
// InvalidProjectFileException.cs
//
// Author:
//   Leszek Ciesielski (skolima@gmail.com)
//
// (C) 2011 Leszek Ciesielski
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
//

using System;
using System.Runtime.Serialization;
using Microsoft.Build.Construction;
using Microsoft.Build.Internal.Expressions;

namespace Microsoft.Build.Exceptions
{
        [Serializable]
        public sealed class InvalidProjectFileException : Exception
        {
                public InvalidProjectFileException ()
                {
                }
                public InvalidProjectFileException (string message) : base(message)
                {
                }
                private InvalidProjectFileException (SerializationInfo info, StreamingContext context)
                        : base(info, context)
                {
                        ProjectFile = info.GetString ("projectFile");
                        LineNumber = info.GetInt32 ("lineNumber");
                        ColumnNumber = info.GetInt32 ("columnNumber");
                        EndLineNumber = info.GetInt32 ("endLineNumber");
                        EndColumnNumber = info.GetInt32 ("endColumnNumber");
                        ErrorSubcategory = info.GetString ("errorSubcategory");
                        ErrorCode = info.GetString ("errorCode");
                        HelpKeyword = info.GetString ("helpKeyword");
                        HasBeenLogged = info.GetBoolean ("hasBeenLogged");
                }
                public InvalidProjectFileException (string message, Exception innerException)
                        : base(message, innerException)
                {
                }
                internal InvalidProjectFileException (ILocation start, string message,
                                                      string errorSubcategory = null, string errorCode = null, string helpKeyword = null)
                        : this (start != null ? start.File : null, 0, start != null ? start.Column : 0, 0, 0, message, errorSubcategory, errorCode, helpKeyword)
                {
                }
                internal InvalidProjectFileException (ElementLocation start, ElementLocation end, string message,
                                                    string errorSubcategory = null, string errorCode = null, string helpKeyword = null)
                        : this (start != null ? start.File : null, start != null ? start.Line : 0, start != null ? start.Column : 0,
                                end != null ? end.Line : 0, end != null ? end.Column : 0,
                                message, errorSubcategory, errorCode, helpKeyword)
                {
                }
                public InvalidProjectFileException (string projectFile, int lineNumber, int columnNumber,
                                                    int endLineNumber, int endColumnNumber, string message,
                                                    string errorSubcategory, string errorCode, string helpKeyword)
                        : base(message)
                {
                        ProjectFile = projectFile;
                        LineNumber = lineNumber;
                        ColumnNumber = columnNumber;
                        EndLineNumber = endLineNumber;
                        EndColumnNumber = endColumnNumber;
                        ErrorSubcategory = errorSubcategory;
                        ErrorCode = errorCode;
                        HelpKeyword = helpKeyword;
                }
                public override void GetObjectData (SerializationInfo info, StreamingContext context)
                {
                        base.GetObjectData (info, context);
                        info.AddValue ("projectFile", ProjectFile);
                        info.AddValue ("lineNumber", LineNumber);
                        info.AddValue ("columnNumber", ColumnNumber);
                        info.AddValue ("endLineNumber", EndLineNumber);
                        info.AddValue ("endColumnNumber", EndColumnNumber);
                        info.AddValue ("errorSubcategory", ErrorSubcategory);
                        info.AddValue ("errorCode", ErrorCode);
                        info.AddValue ("helpKeyword", HelpKeyword);
                        info.AddValue ("hasBeenLogged", HasBeenLogged);
                }
                public string BaseMessage {
                        get { return base.Message; }
                }
                public int ColumnNumber { get; private set; }
                public int EndColumnNumber { get; private set; }
                public int EndLineNumber { get; private set; }
                public string ErrorCode { get; private set; }
                public string ErrorSubcategory { get; private set; }
                public bool HasBeenLogged { get; private set; }
                public string HelpKeyword { get; private set; }
                public int LineNumber { get; private set; }
                public override string Message {
                        get { return ProjectFile == null ? base.Message : base.Message + " " + GetLocation (); }
                }
                public string ProjectFile { get; private set; }

                string GetLocation ()
                {
                        string start = LineNumber == 0 ? string.Empty : ColumnNumber > 0 ? string.Format ("{0},{1}", LineNumber, ColumnNumber) : string.Format ("{0}", LineNumber);
                        string end = EndLineNumber == 0 ? string.Empty : EndColumnNumber > 0 ? string.Format (" - {0},{1}", EndLineNumber, EndColumnNumber) : string.Format (" - {0}", EndLineNumber);
                        return LineNumber == 0 ? ProjectFile : String.Format (" at: {0} ({1}{2})", ProjectFile, start, end);
                }
        }
}
