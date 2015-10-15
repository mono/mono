//------------------------------------------------------------------------------
// <copyright file="SqlError.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient {

    using System;
    using System.Diagnostics;
    using System.Globalization;

    [Serializable]
    public sealed class SqlError {

        // 

        private string source = TdsEnums.SQL_PROVIDER_NAME;
        private int    number;
        private byte   state;
        private byte   errorClass;
        [System.Runtime.Serialization.OptionalFieldAttribute(VersionAdded=2)]
        private string server;
        private string message;
        private string procedure;
        private int    lineNumber;
        [System.Runtime.Serialization.OptionalFieldAttribute(VersionAdded=4)]
        private int win32ErrorCode;

        internal SqlError(int infoNumber, byte errorState, byte errorClass, string server, string errorMessage, string procedure, int lineNumber, uint win32ErrorCode)
            : this(infoNumber, errorState, errorClass, server, errorMessage, procedure, lineNumber)
        {
            this.win32ErrorCode = (int)win32ErrorCode;
        }

        internal SqlError(int infoNumber, byte errorState, byte errorClass, string server, string errorMessage, string procedure, int lineNumber) {
            this.number = infoNumber;
            this.state = errorState;
            this.errorClass = errorClass;
            this.server = server;
            this.message = errorMessage;
            this.procedure = procedure;
            this.lineNumber = lineNumber;
            if (errorClass != 0) {
                Bid.Trace("<sc.SqlError.SqlError|ERR> infoNumber=%d, errorState=%d, errorClass=%d, errorMessage='%ls', procedure='%ls', lineNumber=%d\n" ,
                    infoNumber, (int)errorState, (int)errorClass,  errorMessage,
                    procedure == null ? "None" : procedure, (int)lineNumber);
            }
            this.win32ErrorCode = 0;
        }

        // 



        public override string ToString() {
            //return this.GetType().ToString() + ": " + this.message;
            return typeof(SqlError).ToString() + ": " + this.message; // since this is sealed so we can change GetType to typeof
        }

        // 

        public string Source {
            get { return this.source;}
        }

        public int Number {
            get { return this.number;}
        }

        public byte State {
            get { return this.state;}
        }

        public byte Class {
            get { return this.errorClass;}
        }

        public string Server {
            get { return this.server;}
        }

        public string Message {
            get { return this.message;}
        }

        public string Procedure {
            get { return this.procedure;}
        }

        public int LineNumber {
            get { return this.lineNumber;}
        }

        internal int Win32ErrorCode {
            get { return this.win32ErrorCode; }
        }
    }
}
