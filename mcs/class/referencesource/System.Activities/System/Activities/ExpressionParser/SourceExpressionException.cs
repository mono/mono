//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.ExpressionParser
{
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    [Serializable]
    public class SourceExpressionException : Exception, ISerializable
    {
        CompilerError[] errors;

        public SourceExpressionException()
            : base(SR.CompilerError)
        {
        }

        public SourceExpressionException(string message)
            : base(message)
        {
        }

        public SourceExpressionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public SourceExpressionException(string message, CompilerErrorCollection errors)
            : base(message)
        {
            this.errors = new CompilerError[errors.Count];
            errors.CopyTo(this.errors, 0);
        }

        protected SourceExpressionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null)
            {
                throw FxTrace.Exception.ArgumentNull("info");
            }
            int length = info.GetInt32("count");
            this.errors = new CompilerError[length];
            for (int i = 0; i < length; ++i)
            {
                string index = i.ToString(CultureInfo.InvariantCulture);
                string fileName = info.GetString("file" + index);
                int line = info.GetInt32("line" + index);
                int column = info.GetInt32("column" + index);
                string errorNumber = info.GetString("number" + index);
                string errorText = info.GetString("text" + index);
                this.errors[i] = new CompilerError(fileName, line, column, errorNumber, errorText);
            }
        }

        public IEnumerable<CompilerError> Errors
        {
            get
            {
                if (this.errors == null)
                {
                    this.errors = new CompilerError[0];
                }
                return this.errors;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Critical because we are overriding a critical method in the base class.")]
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw FxTrace.Exception.ArgumentNull("info");
            }
            if (this.errors == null)
            {
                info.AddValue("count", 0);
            }
            else
            {
                info.AddValue("count", this.errors.Length);
                for (int i = 0; i < this.errors.Length; ++i)
                {
                    CompilerError error = this.errors[i];
                    string index = i.ToString(CultureInfo.InvariantCulture);
                    info.AddValue("file" + index, error.FileName);
                    info.AddValue("line" + index, error.Line);
                    info.AddValue("column" + index, error.Column);
                    info.AddValue("number" + index, error.ErrorNumber);
                    info.AddValue("text" + index, error.ErrorText);
                }
            }
            base.GetObjectData(info, context);
        }
    }
}
