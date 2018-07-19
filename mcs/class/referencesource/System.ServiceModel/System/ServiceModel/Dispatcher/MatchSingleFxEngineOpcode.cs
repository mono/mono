//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.XPath;
    using System.Diagnostics;
    
    /// <summary>
    /// Opcode that evaluates a single xpath query using the framework engine
    /// </summary>
    internal abstract class SingleFxEngineResultOpcode : ResultOpcode
    {
        protected XPathExpression xpath;
        protected object item;

        internal SingleFxEngineResultOpcode(OpcodeID id)
            : base(id)
        {
            this.flags |= OpcodeFlags.Fx;
        }
        
        internal object Item
        {
            set
            {
                this.item = value;
            }
        }
                
        internal XPathExpression XPath
        {
            set
            {
                this.xpath = value;
            }
        }

        internal override void CollectXPathFilters(ICollection<MessageFilter> filters)
        {
            MessageFilter filter = this.item as MessageFilter;

            if (filter != null)
            {
                // 
                filters.Add(filter);
            }
        }
        
        internal override bool Equals(Opcode op)
        {
            return false;
        }

        protected object Evaluate(XPathNavigator nav)
        {
            SeekableMessageNavigator messageNav = nav as SeekableMessageNavigator;

            if (null != messageNav)
            {
                // If operating on messages, we would have avoided atomizing unless we were using the Framework
                // This ensures that atomization has taken place
                messageNav.Atomize();
            }

            object evalResult;
            if (XPathResultType.NodeSet == xpath.ReturnType)
            {
                evalResult = nav.Select(xpath);
            }
            else
            {
                evalResult = nav.Evaluate(xpath);
            }

            return evalResult;
        }
    }

    internal class MatchSingleFxEngineResultOpcode : SingleFxEngineResultOpcode
    {
        internal MatchSingleFxEngineResultOpcode() : base(OpcodeID.MatchSingleFx) { }

        internal override Opcode Eval(ProcessingContext context)
        {
            SeekableXPathNavigator targetNav = context.Processor.ContextNode;

            bool result = this.Match(targetNav);
            context.Processor.Result = result;

            if (result && null != this.item && null != context.Processor.MatchSet)
            {
                context.Processor.MatchSet.Add((MessageFilter)this.item);
            }                                   

            return this.next;
        }

        internal bool Match(XPathNavigator nav)
        {
            bool retVal;
            object evalResult = this.Evaluate(nav);
            switch (xpath.ReturnType)
            {
                default:
                    retVal = false;
                    break;

                case XPathResultType.Any:
                    retVal = (null != evalResult);
                    break;

                case XPathResultType.Boolean:
                    retVal = (bool)evalResult;
                    break;

                case XPathResultType.NodeSet:
                    XPathNodeIterator iterator = (XPathNodeIterator) evalResult;
                    retVal = (null != iterator && iterator.Count > 0);
                    break;
                    
                case XPathResultType.Number:
                    retVal = (((double)evalResult) != 0.0);
                    break;
                
                case XPathResultType.String:
                    string strVal = (string)evalResult;
                    retVal = (null != strVal && strVal.Length > 0); // see XPath 1.0
                    break;
            }

            return retVal;
        }
    }

    internal class QuerySingleFxEngineResultOpcode : SingleFxEngineResultOpcode
    {
        internal QuerySingleFxEngineResultOpcode() : base(OpcodeID.QuerySingleFx) { }

        internal override Opcode Eval(ProcessingContext context)
        {
            SeekableXPathNavigator targetNav = context.Processor.ContextNode;
            XPathResult result = this.Select(targetNav);
            if (context.Processor.ResultSet == null)
            {
                context.Processor.QueryResult = result;
            }
            else
            {
                context.Processor.ResultSet.Add(new KeyValuePair<MessageQuery, XPathResult>((MessageQuery)this.item, result));
            }

            return this.next;
        }

        internal XPathResult Select(XPathNavigator nav)
        {
            XPathResult result;            
            object evalResult = this.Evaluate(nav);
            switch (xpath.ReturnType)
            {
                default:
                    result = new XPathResult(string.Empty);
                    break;

                case XPathResultType.Boolean:
                    result = new XPathResult((bool)evalResult);                    
                    break;

                case XPathResultType.NodeSet:                    
                    result = new XPathResult((XPathNodeIterator)evalResult);                        
                    break;
                    
                case XPathResultType.Number:
                    result = new XPathResult((double)evalResult);               
                    break;
                
                case XPathResultType.String:
                    result = new XPathResult((string)evalResult);
                    break;
            }

            return result;
        }        
    }
}
