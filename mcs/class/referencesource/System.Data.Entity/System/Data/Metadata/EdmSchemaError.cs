//---------------------------------------------------------------------
// <copyright file="EdmSchemaError.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Metadata.Edm
{
    using System;

    /// <summary>
    /// This class encapsulates the error information for a schema error that was encountered.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
    [Serializable]
    public sealed class EdmSchemaError : EdmError
    {
        #region Instance Fields
        private int _errorCode = 0;
        private EdmSchemaErrorSeverity _severity = EdmSchemaErrorSeverity.Warning;
        private string _schemaLocation = null;
        private int _line = -1;
        private int _column = -1;
        private string _stackTrace = string.Empty;
        #endregion

        #region Public Methods
        /// <summary>
        /// Constructs a EdmSchemaError object.
        /// </summary>
        /// <param name="message">The explanation of the error.</param>
        /// <param name="errorCode">The code associated with this error.</param>
        /// <param name="severity">The severity of the error.</param>
        internal EdmSchemaError(string message, int errorCode, EdmSchemaErrorSeverity severity) :
            this(message, errorCode, severity, null)
        {
        }

        /// <summary>
        /// Constructs a EdmSchemaError object.
        /// </summary>
        /// <param name="message">The explanation of the error.</param>
        /// <param name="errorCode">The code associated with this error.</param>
        /// <param name="severity">The severity of the error.</param>
        /// <param name="exception">The exception that caused the error to be filed.</param>
        internal EdmSchemaError(string message, int errorCode, EdmSchemaErrorSeverity severity, Exception exception)
            : base(message)
        {
            Initialize(errorCode, severity, null, -1, -1, exception);
        }

        /// <summary>
        /// Constructs a EdmSchemaError object.
        /// </summary>
        /// <param name="message">The explanation of the error.</param>
        /// <param name="errorCode">The code associated with this error.</param>
        /// <param name="severity">The severity of the error.</param>
        /// <param name="sourceUri"></param>
        /// <param name="lineNumber"></param>
        /// <param name="sourceColumn"></param>
        internal EdmSchemaError(string message, int errorCode, EdmSchemaErrorSeverity severity, string schemaLocation, int line, int column)
            : this(message, errorCode, severity, schemaLocation, line, column, null)
        {
        }

        /// <summary>
        /// Constructs a EdmSchemaError object.
        /// </summary>
        /// <param name="message">The explanation of the error.</param>
        /// <param name="errorCode">The code associated with this error.</param>
        /// <param name="severity">The severity of the error.</param>
        /// <param name="sourceUri"></param>
        /// <param name="lineNumber"></param>
        /// <param name="sourceColumn"></param>
        /// <param name="exception">The exception that caused the error to be filed.</param>
        internal EdmSchemaError(string message, int errorCode, EdmSchemaErrorSeverity severity, string schemaLocation, int line, int column, Exception exception)
            : base(message)
        {
            if (severity < EdmSchemaErrorSeverity.Warning || severity > EdmSchemaErrorSeverity.Error)
            {
                throw new ArgumentOutOfRangeException("severity", severity, System.Data.Entity.Strings.ArgumentOutOfRange(severity));
            }

            Initialize(errorCode, severity, schemaLocation, line, column, exception);
        }

        private void Initialize(int errorCode, EdmSchemaErrorSeverity severity, string schemaLocation, int line, int column, Exception exception)
        {
            if (errorCode < 0)
            {
                throw new ArgumentOutOfRangeException("errorCode", errorCode, System.Data.Entity.Strings.ArgumentOutOfRangeExpectedPostiveNumber(errorCode));
            }

            _errorCode = errorCode;
            _severity = severity;
            _schemaLocation = schemaLocation;
            _line = line;
            _column = column;
            if (exception != null)
            {
                _stackTrace = exception.StackTrace;
            }
        }

        /// <summary>
        /// Creates a string representation of the error.
        /// </summary>
        public override string ToString()
        {
            string text;
            string severity;

            switch (Severity)
            {
                case EdmSchemaErrorSeverity.Error:
                    severity = System.Data.Entity.Strings.GeneratorErrorSeverityError;
                    break;
                case EdmSchemaErrorSeverity.Warning:
                    severity = System.Data.Entity.Strings.GeneratorErrorSeverityWarning;
                    break;
                default:
                    severity = System.Data.Entity.Strings.GeneratorErrorSeverityUnknown;
                    break;
            }

            if (String.IsNullOrEmpty(SchemaName) && Line < 0 && Column < 0)
            {
                text = String.Format(System.Globalization.CultureInfo.CurrentCulture, "{0} {1:0000}: {2}",
                    severity,
                    ErrorCode,
                    Message);
            }
            else
            {
                text = String.Format(System.Globalization.CultureInfo.CurrentCulture, "{0}({1},{2}) : {3} {4:0000}: {5}",
                    (SchemaName == null) ? System.Data.Entity.Strings.SourceUriUnknown : SchemaName,
                    Line,
                    Column,
                    severity,
                    ErrorCode,
                    Message);
            }

            return text;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets the ErrorCode.
        /// </summary>
        public int ErrorCode
        {
            get
            {
                return _errorCode;
            }
        }

        /// <summary>
        /// Gets the Severity of the error.
        /// </summary>
        public EdmSchemaErrorSeverity Severity
        {
            get
            {
                return _severity;
            }
            set
            {
                _severity = value;
            }
        }

        /// <summary>
        /// Gets the LineNumber that the error occured on.
        /// </summary>
        public int Line
        {
            get
            {
                return _line;
            }
        }

        /// <summary>
        /// Gets the column that the error occured in.
        /// </summary>
        public int Column
        {
            get
            {
                return _column;
            }
        }

        /// <summary>
        /// Gets the of the schema that contains the error.
        /// </summary>
        public string SchemaLocation
        {
            get
            {
                return _schemaLocation;
            }
        }

        /// <summary>
        /// Gets the of the schema that contains the error.
        /// </summary>
        public string SchemaName
        {
            get
            {
                return GetNameFromSchemaLocation(SchemaLocation);
            }
        }

        /// <summary>
        /// Gets the stack trace of when the error occured.
        /// </summary>
        /// <value></value>
        public string StackTrace
        {
            get
            {
                return _stackTrace;
            }
        }
        #endregion

        private static string GetNameFromSchemaLocation(string schemaLocation)
        {
            if (string.IsNullOrEmpty(schemaLocation))
            {
                return schemaLocation;
            }

            int pos = Math.Max(schemaLocation.LastIndexOf('/'), schemaLocation.LastIndexOf('\\'));
            int start = pos + 1;
            if (pos < 0)
            {
                return schemaLocation;
            }
            else if (start >= schemaLocation.Length)
            {
                return string.Empty;
            }

            return schemaLocation.Substring(start);
        }
    }
}
