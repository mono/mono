//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Runtime;
    using System.Xml.XPath;

    public sealed class XPathResult : IDisposable
    {
        bool boolResult;
        SafeNodeSequenceIterator internalIterator;
        XPathNodeIterator nodeSetResult;
        double numberResult;
        XPathResultType resultType;
        string stringResult;

        internal XPathResult(XPathNodeIterator nodeSetResult)
            : this()
        {
            this.nodeSetResult = nodeSetResult;
            this.internalIterator = nodeSetResult as SafeNodeSequenceIterator;
            this.resultType = XPathResultType.NodeSet;
        }

        internal XPathResult(string stringResult) : this()
        {
            this.stringResult = stringResult;
            this.resultType = XPathResultType.String;
        }

        internal XPathResult(bool boolResult) : this()
        {
            this.boolResult = boolResult;
            this.resultType = XPathResultType.Boolean;
        }

        internal XPathResult(double numberResult) : this()
        {
            this.numberResult = numberResult;
            this.resultType = XPathResultType.Number;
        }

        XPathResult()
        {
        }

        public XPathResultType ResultType
        {
            get
            {
                return this.resultType;
            }
        }

        public void Dispose()
        {
            if (this.internalIterator != null)
            {
                this.internalIterator.Dispose();
            }
        }

        public bool GetResultAsBoolean()
        {
            switch (this.resultType)
            {
                case XPathResultType.Boolean:
                    return this.boolResult;

                case XPathResultType.NodeSet:
                    return QueryValueModel.Boolean(this.nodeSetResult);

                case XPathResultType.Number:
                    return QueryValueModel.Boolean(this.numberResult);

                case XPathResultType.String:
                    return QueryValueModel.Boolean(this.stringResult);

                default:
                    throw Fx.AssertAndThrow("Unexpected result type.");
            }
        }

        public XPathNodeIterator GetResultAsNodeset()
        {
            if (this.resultType != XPathResultType.NodeSet)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CannotRepresentResultAsNodeset)));
            }

            return this.nodeSetResult;
        }

        public double GetResultAsNumber()
        {
            switch (this.resultType)
            {
                case XPathResultType.Boolean:
                    return QueryValueModel.Double(this.boolResult);

                case XPathResultType.NodeSet:
                    return QueryValueModel.Double(this.nodeSetResult);

                case XPathResultType.Number:
                    return this.numberResult;

                case XPathResultType.String:
                    return QueryValueModel.Double(this.stringResult);

                default:
                    throw Fx.AssertAndThrow("Unexpected result type.");
            }
        }

        public string GetResultAsString()
        {
            switch (this.resultType)
            {
                case XPathResultType.Boolean:
                    return QueryValueModel.String(this.boolResult);

                case XPathResultType.NodeSet:
                    return QueryValueModel.String(this.nodeSetResult);

                case XPathResultType.Number:
                    return QueryValueModel.String(this.numberResult);

                case XPathResultType.String:
                    return this.stringResult;

                default:
                    throw Fx.AssertAndThrow("Unexpected result type.");
            }
        }

        internal XPathResult Copy()
        {
            XPathResult result = new XPathResult();

            result.resultType = this.resultType;
            switch (this.resultType)
            {
                case XPathResultType.Boolean:
                    result.boolResult = this.boolResult;
                    break;
                case XPathResultType.NodeSet:
                    result.nodeSetResult = this.nodeSetResult.Clone();
                    break;
                case XPathResultType.Number:
                    result.numberResult = this.numberResult;
                    break;
                case XPathResultType.String:
                    result.stringResult = this.stringResult;
                    break;
                default:
                    throw Fx.AssertAndThrow("Unexpected result type.");
            }

            return result;
        }
    }
}
