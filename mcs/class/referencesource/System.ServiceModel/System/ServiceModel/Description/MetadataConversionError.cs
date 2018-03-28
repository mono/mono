//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System.Collections.ObjectModel;
    using System.ServiceModel.Channels;

    public class MetadataConversionError
    {
        string message;
        bool isWarning;

        public MetadataConversionError(string message) : this(message, false) { }
        public MetadataConversionError(string message, bool isWarning)
        {
            if (message == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            this.message = message;
            this.isWarning = isWarning;
        }

        public string Message { get { return message; } }
        public bool IsWarning { get { return isWarning; } }
        public override bool Equals(object obj)
        {
            MetadataConversionError otherError = obj as MetadataConversionError;
            if (otherError == null)
                return false;
            return otherError.IsWarning == this.IsWarning && otherError.Message == this.Message;
        }

        public override int GetHashCode()
        {
            return message.GetHashCode();
        }
    }

}
