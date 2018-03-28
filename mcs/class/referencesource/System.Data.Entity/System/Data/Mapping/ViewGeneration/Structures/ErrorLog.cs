//---------------------------------------------------------------------
// <copyright file="ErrorLog.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Mapping.ViewGeneration.Structures
{
    using System.Collections.Generic;
    using System.Data.Common.Utils;
    using System.Data.Entity;
    using System.Data.Mapping.ViewGeneration.Utils;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    internal class ErrorLog : InternalBase
    {

        #region Constructors
        internal ErrorLog()
        {
            m_log = new List<Record>();
        }
        #endregion

        #region Fields
        private List<Record> m_log;
        #endregion

        #region Properties
        internal int Count
        {
            get { return m_log.Count; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")] // referenced (indirectly) by System.Data.Entity.Design.dll
        internal IEnumerable<EdmSchemaError> Errors
        {
            get
            {
                foreach (Record record in m_log)
                {
                    yield return record.Error;
                }
            }
        }
        #endregion

        #region Methods
        internal void AddEntry(Record record)
        {
            EntityUtil.CheckArgumentNull(record, "record");
            m_log.Add(record);
        }

        internal void Merge(ErrorLog log)
        {
            foreach (Record record in log.m_log)
            {
                m_log.Add(record);
            }
        }

        internal void PrintTrace()
        {
            StringBuilder builder = new StringBuilder();
            ToCompactString(builder);
            Helpers.StringTraceLine(builder.ToString());
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            foreach (Record record in m_log)
            {
                record.ToCompactString(builder);
            }
        }

        internal string ToUserString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (Record record in m_log)
            {
                string recordString = record.ToUserString();
                builder.AppendLine(recordString);
            }
            return builder.ToString();
        }
        #endregion

        #region Nested classes/struct
        internal class Record : InternalBase
        {
            #region Constructor
            // effects: Creates an error record for wrappers, a debug message
            // and an error message given by "message". Note: wrappers cannot
            // be null
            internal Record(bool isError, ViewGenErrorCode errorCode, string message,
                            IEnumerable<LeftCellWrapper> wrappers, string debugMessage)
            {
                Debug.Assert(wrappers != null);
                IEnumerable<Cell> cells = LeftCellWrapper.GetInputCellsForWrappers(wrappers);
                Init(isError, errorCode, message, cells, debugMessage);
            }

            internal Record(bool isError, ViewGenErrorCode errorCode, string message, Cell sourceCell, string debugMessage)
            {
                Init(isError, errorCode, message, new Cell[] { sourceCell }, debugMessage);
            }

            internal Record(bool isError, ViewGenErrorCode errorCode, string message, IEnumerable<Cell> sourceCells,
                            string debugMessage)
            {
                Init(isError, errorCode, message, sourceCells, debugMessage);
            }

            //There are cases when we want to create a ViewGen error that is not specific to any mapping fragment
            //In this case, it is better to just create the EdmSchemaError directly and hold on to it.
            internal Record(EdmSchemaError error)
            {
                m_debugMessage = error.ToString();
                m_mappingError = error;
            }


            private void Init(bool isError, ViewGenErrorCode errorCode, string message,
                              IEnumerable<Cell> sourceCells, string debugMessage)
            {
                m_sourceCells = new List<Cell>(sourceCells);

                Debug.Assert(m_sourceCells.Count > 0, "Error record must have at least one cell");

                // For certain foreign key messages, we may need the SSDL line numbers and file names
                CellLabel label = m_sourceCells[0].CellLabel;
                string sourceLocation = label.SourceLocation;
                int lineNumber = label.StartLineNumber;
                int columnNumber = label.StartLinePosition;

                string userMessage = InternalToString(message, debugMessage, m_sourceCells, sourceLocation, errorCode, isError, false);
                m_debugMessage = InternalToString(message, debugMessage, m_sourceCells, sourceLocation, errorCode, isError, true);
                m_mappingError = new EdmSchemaError(userMessage, (int)errorCode, EdmSchemaErrorSeverity.Error, sourceLocation,
                                                      lineNumber, columnNumber);
            }
            #endregion

            #region Fields
            private EdmSchemaError m_mappingError;
            private List<Cell> m_sourceCells;
            private string m_debugMessage;
            #endregion

            #region Properties
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")] // referenced (indirectly) by System.Data.Entity.Design.dll
            internal EdmSchemaError Error
            {
                get { return m_mappingError; }
            }
            #endregion

            #region Methods
            internal override void ToCompactString(StringBuilder builder)
            {
                builder.Append(m_debugMessage);
            }

            // effects: adds a comma-separated list of line numbers to the string builder
            private static void GetUserLinesFromCells(IEnumerable<Cell> sourceCells, StringBuilder lineBuilder, bool isInvariant)
            {
                var orderedCells = sourceCells.OrderBy<Cell, int>(cell => cell.CellLabel.StartLineNumber, Comparer<int>.Default);

                bool isFirst = true;
                // Get the line numbers
                foreach (Cell cell in orderedCells)
                {
                    if (isFirst == false)
                    {
                        lineBuilder.Append(isInvariant ? EntityRes.GetString(EntityRes.ViewGen_CommaBlank) : ", ");
                    }
                    isFirst = false;
                    lineBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}", cell.CellLabel.StartLineNumber);
                }
                Debug.Assert(isFirst == false, "No cells");
            }

            // effects: Converts the message/debugMessage to a user-readable
            // message using resources (if isInvariant is false) or a test
            // message (if isInvariant is true)
            static private string InternalToString(string message, string debugMessage,
                                                   List<Cell> sourceCells, string sourceLocation, ViewGenErrorCode errorCode,
                                                   bool isError, bool isInvariant)
            {
                StringBuilder builder = new StringBuilder();

                if (isInvariant)
                {
                    builder.AppendLine(debugMessage);

                    builder.Append(isInvariant ? "ERROR" : System.Data.Entity.Strings.ViewGen_Error);
                    StringUtil.FormatStringBuilder(builder, " ({0}): ", (int)errorCode);
                }

                StringBuilder lineBuilder = new StringBuilder();
                GetUserLinesFromCells(sourceCells, lineBuilder, isInvariant);

                if (isInvariant)
                {
                    if (sourceCells.Count > 1)
                    {
                        StringUtil.FormatStringBuilder(builder, "Problem in Mapping Fragments starting at lines {0}: ", lineBuilder.ToString());
                    }
                    else
                    {
                        StringUtil.FormatStringBuilder(builder, "Problem in Mapping Fragment starting at line {0}: ", lineBuilder.ToString());
                    }
                }
                else
                {
                    if (sourceCells.Count > 1)
                    {
                        builder.Append(Strings.ViewGen_ErrorLog2(lineBuilder.ToString()));
                    }
                    else
                    {
                        builder.Append(Strings.ViewGen_ErrorLog(lineBuilder.ToString()));
                    }
                }
                builder.AppendLine(message);
                return builder.ToString();
            }

            internal string ToUserString()
            {
                return m_mappingError.ToString();
            }
            #endregion
        }
        #endregion
    }
}
