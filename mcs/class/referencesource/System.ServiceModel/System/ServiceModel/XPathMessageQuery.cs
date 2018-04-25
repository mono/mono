//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Windows.Markup;
    using System.Xml;
    using System.Xml.Xsl;
    using System.ServiceModel.Dispatcher;

    [ContentProperty("Expression")]
    public class XPathMessageQuery : MessageQuery
    {
        string expression;
        XPathQueryMatcher matcher;
        XmlNamespaceManager namespaces;
        bool needCompile;
        object thisLock;

        public XPathMessageQuery() :
            this(string.Empty, (XmlNamespaceManager)new XPathMessageContext())
        {
        }

        public XPathMessageQuery(string expression)
            : this(expression, (XmlNamespaceManager)new XPathMessageContext())
        {
        }

        public XPathMessageQuery(string expression, XsltContext context)
            : this(expression, (XmlNamespaceManager)context)
        {
        }

        public XPathMessageQuery(string expression, XmlNamespaceManager namespaces)
        {
            if (expression == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("expression");
            }

            this.expression = expression;
            this.namespaces = namespaces;
            this.needCompile = true;
            this.thisLock = new Object();
        }

        [DefaultValue("")]
        public string Expression
        {
            get
            {
                return this.expression;
            }

            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                this.expression = value;
                this.needCompile = true;
            }
        }

        [DefaultValue(null)]
        public XmlNamespaceManager Namespaces
        {
            get
            {
                return this.namespaces;
            }

            set
            {
                this.namespaces = value;
                this.needCompile = true;
            }
        }

        public override MessageQueryCollection CreateMessageQueryCollection()
        {
            return new XPathMessageQueryCollection();
        }

        public override TResult Evaluate<TResult>(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            if (typeof(TResult) == typeof(XPathResult) ||
                typeof(TResult) == typeof(string) ||
                typeof(TResult) == typeof(bool) ||
                typeof(TResult) == typeof(object))
            {
                this.EnsureCompile();

                QueryResult<TResult> queryResult = this.matcher.Evaluate<TResult>(message, false);
                return queryResult.GetSingleResult();
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TResult",
                    SR.GetString(SR.UnsupportedMessageQueryResultType, typeof(TResult)));
            }
        }

        public override TResult Evaluate<TResult>(MessageBuffer buffer)
        {
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
            }

            this.EnsureCompile();

            if (typeof(TResult) == typeof(XPathResult) ||
                typeof(TResult) == typeof(string) ||
                typeof(TResult) == typeof(bool) ||
                typeof(TResult) == typeof(object))
            {
                this.EnsureCompile();

                QueryResult<TResult> queryResult = this.matcher.Evaluate<TResult>(buffer);
                return queryResult.GetSingleResult();
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TResult",
                    SR.GetString(SR.UnsupportedMessageQueryResultType, typeof(TResult)));
            }
        }

        void EnsureCompile()
        {
            if (this.needCompile)
            {
                lock (thisLock)
                {
                    if (this.needCompile)
                    {
                        this.matcher = new XPathQueryMatcher(false);
                        this.matcher.Compile(this.expression, this.namespaces);
                        this.needCompile = false;
                    }
                }
            }
        }
    }
}
