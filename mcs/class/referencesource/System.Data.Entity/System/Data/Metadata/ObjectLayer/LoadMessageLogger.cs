//---------------------------------------------------------------------
// <copyright file="ObjectItemLoadingSessionData.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
namespace System.Data.Metadata.Edm
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Diagnostics;
    using System.Text;

    internal class LoadMessageLogger
    {
        private Action<String> _logLoadMessage;
        private Dictionary<EdmType, StringBuilder> _messages = new Dictionary<EdmType, StringBuilder>();

        internal LoadMessageLogger(Action<String> logLoadMessage)
        {
            this._logLoadMessage = logLoadMessage;
        }

        internal void LogLoadMessage(string message, EdmType relatedType)
        {
            if (_logLoadMessage != null)
            {
                _logLoadMessage(message);
            }

            LogMessagesWithTypeInfo(message, relatedType);
        }

        internal string CreateErrorMessageWithTypeSpecificLoadLogs(string errorMessage, EdmType relatedType)
        {
                return new StringBuilder(errorMessage)
                    .AppendLine(this.GetTypeRelatedLogMessage(relatedType)).ToString();
        }

        private string GetTypeRelatedLogMessage(EdmType relatedType)
        {
            Debug.Assert(relatedType != null, "have to pass in a type to get the message");

            if (this._messages.ContainsKey(relatedType))
            {
                return new StringBuilder()
                    .AppendLine()
                    .AppendLine(Strings.ExtraInfo)
                    .AppendLine(this._messages[relatedType].ToString()).ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        private void LogMessagesWithTypeInfo(string message, EdmType relatedType)
        {
            Debug.Assert(relatedType != null, "have to have a type with this message");

            if (this._messages.ContainsKey(relatedType))
            {
                // if this type already contains loading message, append the new message to the end
                this._messages[relatedType].AppendLine(message);
            }
            else
            {
                this._messages.Add(relatedType, new StringBuilder(message));
            }
        }
    }
}
