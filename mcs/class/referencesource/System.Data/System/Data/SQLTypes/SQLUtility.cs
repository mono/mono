//------------------------------------------------------------------------------
// <copyright file="SQLUtility.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">junfang</owner>
// <owner current="true" primary="false">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

//**************************************************************************
// @File: SQLUtility.cs
//
// Create by:	JunFang
//
// Purpose: Implementation of utilities in COM+ SQL Types Library.
//			Includes interface INullable, exceptions SqlNullValueException
//			and SqlTruncateException, and SQLDebug class.
//
// Notes:
//
// History:
//
//   09/17/99  JunFang	Created and implemented as first drop.
//
// @EndHeader@
//**************************************************************************

using System;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data.SqlTypes {

    internal enum EComparison {
        LT,
        LE,
        EQ,
        GE,
        GT,
        NE
    }

	// This enumeration is used to inquire about internal storage of a SqlBytes or SqlChars instance:
    public enum StorageState {
		Buffer = 0,
		Stream = 1,
		UnmanagedBuffer = 2
	}

    [Serializable]
    public class SqlTypeException : SystemException {

        public SqlTypeException() : this(Res.GetString(Res.SqlMisc_SqlTypeMessage), null) { // MDAC 82931, 84941
        }

        // Creates a new SqlTypeException with its message string set to message.
        public SqlTypeException(String message) : this(message, null) {
        }

        public SqlTypeException(String message, Exception e) : base(message, e) { // MDAC 82931
            HResult = HResults.SqlType; // MDAC 84941
        }

        // runtime will call even if private...
        // <fxcop ignore=SerializableTypesMustHaveMagicConstructorWithAdequateSecurity />
        protected SqlTypeException(SerializationInfo si, StreamingContext sc) : base(SqlTypeExceptionSerialization(si, sc), sc) {
        }

        static private SerializationInfo SqlTypeExceptionSerialization(SerializationInfo si, StreamingContext sc) {
            if ((null != si) && (1 == si.MemberCount)) {
                string message = si.GetString("SqlTypeExceptionMessage"); // WebData 104331
                SqlTypeException fakeValue = new SqlTypeException(message);
                fakeValue.GetObjectData(si, sc);
            }
            return si;
        }
    } // SqlTypeException

    [Serializable]
    public sealed class SqlNullValueException : SqlTypeException {

        // Creates a new SqlNullValueException with its message string set to the common string.
        public SqlNullValueException() : this(SQLResource.NullValueMessage, null) {
        }

        // Creates a new NullValueException with its message string set to message.
        public SqlNullValueException(String message) : this(message, null) {
        }

        public SqlNullValueException(String message, Exception e) : base(message, e) { // MDAC 82931
            HResult = HResults.SqlNullValue; // MDAC 84941
        }

        // runtime will call even if private...
        // <fxcop ignore=SerializableTypesMustHaveMagicConstructorWithAdequateSecurity />
        private SqlNullValueException(SerializationInfo si, StreamingContext sc) : base(SqlNullValueExceptionSerialization(si, sc), sc) {
        }

        static private SerializationInfo SqlNullValueExceptionSerialization(SerializationInfo si, StreamingContext sc) {
            if ((null != si) && (1 == si.MemberCount)) {
                string message = si.GetString("SqlNullValueExceptionMessage"); // WebData 104331
                SqlNullValueException fakeValue = new SqlNullValueException(message);
                fakeValue.GetObjectData(si, sc);
            }
            return si;
        }
    } // NullValueException

    [Serializable]
    public sealed class SqlTruncateException : SqlTypeException {

        // Creates a new SqlTruncateException with its message string set to the empty string.
        public SqlTruncateException() : this(SQLResource.TruncationMessage, null) {
        }

        // Creates a new SqlTruncateException with its message string set to message.
        public SqlTruncateException(String message) : this(message, null) {
        }

        public SqlTruncateException(String message, Exception e) : base(message, e) { // MDAC 82931
            HResult = HResults.SqlTruncate; // MDAC 84941
        }

        // runtime will call even if private...
        // <fxcop ignore=SerializableTypesMustHaveMagicConstructorWithAdequateSecurity />
        private SqlTruncateException(SerializationInfo si, StreamingContext sc) : base(SqlTruncateExceptionSerialization(si, sc), sc) {
        }

        static private SerializationInfo SqlTruncateExceptionSerialization(SerializationInfo si, StreamingContext sc) {
            if ((null != si) && (1 == si.MemberCount)) {
                string message = si.GetString("SqlTruncateExceptionMessage"); // WebData 104331
                SqlTruncateException fakeValue = new SqlTruncateException(message);
                fakeValue.GetObjectData(si, sc);
            }
            return si;
        }
    } // SqlTruncateException

	[Serializable]
    public sealed class SqlNotFilledException : SqlTypeException {
		// 


		// Creates a new SqlNotFilledException with its message string set to the common string.
		public SqlNotFilledException() : this(SQLResource.NotFilledMessage, null) {
		}

		// Creates a new NullValueException with its message string set to message.
		public SqlNotFilledException(String message) : this(message, null) {
        }

		public SqlNotFilledException(String message, Exception e) : base(message, e) { // MDAC 82931
			HResult = HResults.SqlNullValue; // MDAC 84941
		}

		// runtime will call even if private...
		// <fxcop ignore=SerializableTypesMustHaveMagicConstructorWithAdequateSecurity />
		private SqlNotFilledException(SerializationInfo si, StreamingContext sc) : base(si, sc) {
		}
	} // SqlNotFilledException

	[Serializable]
    public sealed class SqlAlreadyFilledException : SqlTypeException {
		// 


		// Creates a new SqlNotFilledException with its message string set to the common string.
		public SqlAlreadyFilledException() : this(SQLResource.AlreadyFilledMessage, null) {
		}

		// Creates a new NullValueException with its message string set to message.
		public SqlAlreadyFilledException(String message) : this(message, null) {
        }

		public SqlAlreadyFilledException(String message, Exception e) : base(message, e) { // MDAC 82931
			HResult = HResults.SqlNullValue; // MDAC 84941
		}

		// runtime will call even if private...
		// <fxcop ignore=SerializableTypesMustHaveMagicConstructorWithAdequateSecurity />
		private SqlAlreadyFilledException(SerializationInfo si, StreamingContext sc) : base(si, sc) {
		}
	} // SqlNotFilledException

	
    internal sealed class SQLDebug {
        private SQLDebug() { /* prevent utility class from being insantiated*/ }

        [System.Diagnostics.Conditional("DEBUG")]
        internal static void Check(bool condition) {
            Debug.Assert(condition, "", "");
        }

        [System.Diagnostics.Conditional("DEBUG")]
        internal static void Check(bool condition, String conditionString, string message) {
            Debug.Assert(condition, conditionString, message);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        internal static void Check(bool condition, String conditionString) {
            Debug.Assert(condition, conditionString, "");
        }

        /*
        [System.Diagnostics.Conditional("DEBUG")]
        internal static void Message(String traceMessage) {
            Debug.WriteLine(SQLResource.MessageString + ": " + traceMessage);
        }
        */

    } // SQLDebug
} // namespace System.Data.SqlTypes
