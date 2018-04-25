// ---------------------------------------------------------------------------
// Copyright (C) 2006 Microsoft Corporation All Rights Reserved
// ---------------------------------------------------------------------------

#define CODE_ANALYSIS
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Workflow.Activities.Rules
{
    // RuleBuilderSyntaxException contains syntax error information in cases where the RuleBuilder
    // failed to parse the expression.
    [Serializable]
    [SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic")]
    internal class RuleSyntaxException : SystemException
    {
        private int position;
        private int errorNumber;

        #region Constructors
        internal RuleSyntaxException()
        {
        }

        internal RuleSyntaxException(int errorNumber, string message, int position)
            : base(message)
        {
            this.errorNumber = errorNumber;
            this.position = position;
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        private RuleSyntaxException(SerializationInfo serializeInfo, StreamingContext context)
            : base(serializeInfo, context)
        {
        }
        #endregion

        internal int Position
        {
            get { return position; }
        }

        internal int ErrorNumber
        {
            get { return errorNumber; }
        }
    }
}
