//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Xml;
    using System.Xml.XPath;

    internal enum QueryProcessingError
    {
        None,
        Unexpected,
        TypeMismatch,
        UnsupportedXmlNodeType,
        NodeCountMaxExceeded,
        InvalidXmlAttributes,
        InvalidNavigatorPosition,
        NotAtomized,
        NotSupported,
        InvalidBodyAccess,
        InvalidNamespacePrefix
    }

    internal class QueryProcessingException : XPathException
    {
        QueryProcessingError error;

        internal QueryProcessingException(QueryProcessingError error, string message) : base(message, null)
        {
            this.error = error;
        }

        internal QueryProcessingException(QueryProcessingError error) : this(error, null)
        {
            this.error = error;
        }

        public override string ToString()
        {
            return this.error.ToString();
        }
    }

    internal enum QueryCompileError
    {
        None,
        General,
        CouldNotParseExpression,
        UnexpectedToken,
        UnsupportedOperator,
        UnsupportedAxis,
        UnsupportedFunction,
        UnsupportedNodeTest,
        UnsupportedExpression,
        AbsolutePathRequired,
        InvalidNCName,
        InvalidVariable,
        InvalidNumber,
        InvalidLiteral,
        InvalidOperatorName,
        InvalidNodeType,
        InvalidExpression,
        InvalidFunction,
        InvalidLocationPath,
        InvalidLocationStep,
        InvalidAxisSpecifier,
        InvalidNodeTest,
        InvalidPredicate,
        InvalidComparison,
        InvalidOrdinal,
        InvalidType,
        InvalidTypeConversion,
        NoNamespaceForPrefix,
        MismatchedParen,
        DuplicateOpcode,
        OpcodeExists,
        OpcodeNotFound,
        PredicateNestingTooDeep
    }

    internal class QueryCompileException : XPathException
    {
        QueryCompileError error;

        internal QueryCompileException(QueryCompileError error, string message) : base(message, null)
        {
            this.error = error;
        }

        internal QueryCompileException(QueryCompileError error) : this(error, null)
        {
            this.error = error;
        }

        public override string ToString()
        {
            return this.error.ToString();
        }
    }
}
