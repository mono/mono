namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Collections;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel.Serialization;


    // IWorkflowCompilerError is registered from Project System
    [Guid("AEA0CDAE-ADB5-46c6-A5ED-DBD516B3E0C1"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(false), ComImport]
    internal interface IWorkflowCompilerError
    {
        String Document { get; }
        bool IsWarning { get; }
        String Text { get; }
        String ErrorNumber { get; }
        int LineNumber { get; }
        int ColumnNumber { get; }
    }
    [Guid("A5367E37-D7AF-4372-8079-D1D6726AEDC8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(false), ComImport]
    internal interface IWorkflowCompilerErrorLogger
    {
        void LogError(IWorkflowCompilerError error);
        void LogMessage(string message);
    }

    [Serializable]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class WorkflowCompilerError : CompilerError, IWorkflowCompilerError
    {
        private Hashtable userData = null;
        private bool incrementLineAndColumn = false;
        private string propertyName = null;

        public WorkflowCompilerError()
        {
        }

        public WorkflowCompilerError(string fileName, int line, int column, string errorNumber, string errorText) :
            base(fileName, line, column, errorNumber, errorText)
        {
        }

        public WorkflowCompilerError(string fileName, WorkflowMarkupSerializationException exception)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");
            this.FileName = fileName;
            this.Line = exception.LineNumber - 1;
            this.Column = exception.LinePosition - 1;
            this.ErrorText = exception.Message;
            this.ErrorNumber = ErrorNumbers.Error_SerializationError.ToString(CultureInfo.InvariantCulture);
            this.incrementLineAndColumn = true;
        }

        internal WorkflowCompilerError(CompilerError error)
        {
            if (error == null)
                throw new ArgumentNullException("error");
            this.Column = error.Column - 1;
            this.ErrorNumber = error.ErrorNumber;
            this.ErrorText = error.ErrorText;
            this.FileName = error.FileName;
            this.IsWarning = error.IsWarning;
            this.Line = error.Line - 1;
            this.incrementLineAndColumn = true;
        }

        public string PropertyName
        {
            get
            {
                return this.propertyName;
            }
            set
            {
                this.propertyName = value;
            }
        }

        public IDictionary UserData
        {
            get
            {
                if (this.userData == null)
                    this.userData = new Hashtable();
                return this.userData;
            }
        }
        public override string ToString()
        {
            if (FileName.Length > 0)
            {
                if (Line <= 0 || Column <= 0)
                    return string.Format(CultureInfo.CurrentCulture, "{0} : {1} {2}: {3}", new object[] { FileName, IsWarning ? "warning" : "error", ErrorNumber, ErrorText });
                else
                    return string.Format(CultureInfo.CurrentCulture, "{0}({1},{2}) : {3} {4}: {5}", new object[] { FileName, this.incrementLineAndColumn ? Line + 1 : Line, this.incrementLineAndColumn ? Column + 1 : Column, IsWarning ? "warning" : "error", ErrorNumber, ErrorText });
            }
            else
            {
                return string.Format(CultureInfo.CurrentCulture, "{0} {1}: {2}", IsWarning ? "warning" : "error", ErrorNumber, ErrorText);
            }
        }


        #region IWorkflowCompilerError Members

        string IWorkflowCompilerError.Document
        {
            get
            {
                return this.FileName;
            }
        }
        bool IWorkflowCompilerError.IsWarning
        {
            get
            {
                return this.IsWarning;
            }
        }
        string IWorkflowCompilerError.Text
        {
            get
            {
                return this.ErrorText;
            }
        }

        string IWorkflowCompilerError.ErrorNumber
        {
            get
            {
                return this.ErrorNumber;
            }
        }

        int IWorkflowCompilerError.LineNumber
        {
            get
            {
                return this.Line;
            }
        }
        int IWorkflowCompilerError.ColumnNumber
        {
            get
            {
                return this.Column;
            }
        }

        #endregion
    }
}
