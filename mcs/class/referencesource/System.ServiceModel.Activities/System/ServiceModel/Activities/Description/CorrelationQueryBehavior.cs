//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Activities.Description
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Activities.Dispatcher;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Xml.Linq;

    class CorrelationQueryBehavior : IEndpointBehavior, IChannelInitializer, IExtension<IContextChannel>
    {        
        const string defaultQueryFormat = "sm:correlation-data('{0}')";
        const string contextCorrelationName = "wsc-instanceId"; 
        const string cookieCorrelationName = "http-cookie";
        static string xPathForCookie = string.Format(CultureInfo.InvariantCulture, defaultQueryFormat, cookieCorrelationName);
        CorrelationKeyCalculator correlationKeyCalculator;        
        ICollection<CorrelationQuery> queries;
        ReadOnlyCollection<string> sendNames;
        ReadOnlyCollection<string> receiveNames;
        bool shouldPreserveMessage;
        
        public CorrelationQueryBehavior(ICollection<CorrelationQuery> queries)
        {
            Fx.AssertAndThrow(queries != null, "queries must not be null");

            foreach (CorrelationQuery query in queries)
            {
                Fx.AssertAndThrow(query.Where != null, "CorrelationQuery.Where must not be null");
            }

            this.queries = queries;
            this.shouldPreserveMessage = true;
        }

        public ICollection<CorrelationQuery> CorrelationQueries
        {
            get { return this.queries; }            
        }

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode,
            Justification = "We will use this")]
        public ICollection<string> ReceiveNames
        {
            get { return this.receiveNames; }
        }

        public ICollection<string> SendNames
        {
            get { return this.sendNames; }
        }

        internal XName ScopeName
        {
            set;
            get;
        }

        public XName ServiceContractName
        {
            get;
            set;
        }

        internal bool IsCookieBasedQueryPresent()
        {
            if (this.queries.Count > 0)
            {
                foreach (CorrelationQuery query in this.queries)
                {
                    // we only need to look at queries for selectAdditional since this query should be always initializing
                    foreach (MessageQuerySet messageQueryset in query.SelectAdditional)
                    {
                        foreach (KeyValuePair<string, MessageQuery> item in messageQueryset)
                        {
                            XPathMessageQuery xPathQuery = item.Value as XPathMessageQuery;
                            if (xPathQuery != null && xPathQuery.Expression.Equals(xPathForCookie))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            ICorrelationDataSource source = endpoint.Binding.GetProperty<ICorrelationDataSource>(new BindingParameterCollection());

            if (source != null)
            {
                this.ConfigureBindingDataNames(source);
                this.ConfigureBindingDefaultQueries(endpoint, source, false);
            }
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            ICorrelationDataSource source = endpoint.Binding.GetProperty<ICorrelationDataSource>(new BindingParameterCollection());

            if (source != null)
            {
                this.ConfigureBindingDataNames(source);
                this.ConfigureBindingDefaultQueries(endpoint, source, true);
            }

            ServiceDescription description = endpointDispatcher.ChannelDispatcher.Host.Description;
            WorkflowServiceHost host = endpointDispatcher.ChannelDispatcher.Host as WorkflowServiceHost;

            if (host == null)
            {
                // setup the scope name to be the Namespace + Name of the ServiceDescription. This will be 
                // either have been explicitly set by WorkflowService.Name or defaulted through the infrastructure
                this.ScopeName = XNamespace.Get(description.Namespace).GetName(description.Name);
            }
            else
            {
                this.ScopeName = host.DurableInstancingOptions.ScopeName;
            }

            endpointDispatcher.ChannelDispatcher.ChannelInitializers.Add(this);
            if (this.shouldPreserveMessage)
            {
                // there could be a query that might be read from the message body
                // let us buffer the message at the dispatcher
                endpointDispatcher.DispatchRuntime.PreserveMessage = true;
            }
        }

        public static bool BindingHasDefaultQueries(Binding binding)
        {
            ICorrelationDataSource source = binding.GetProperty<ICorrelationDataSource>(new BindingParameterCollection());
            bool hasDefaults = false;

            if (source != null)
            {
                foreach (CorrelationDataDescription data in source.DataSources)
                {
                    if (data.IsDefault)
                    {
                        hasDefaults = true;
                        break;
                    }
                }
            }

            return hasDefaults;
        }

        void ConfigureBindingDataNames(ICorrelationDataSource source)
        {           
            List<string> receiveNames = new List<string>();
            List<string> sendNames = new List<string>();         

            foreach (CorrelationDataDescription data in source.DataSources)
            {
                if (data.ReceiveValue)
                {
                    receiveNames.Add(data.Name);
                }
                // we want to optimize the correlation path for Send/SendReply cases,
                // when using httpbinding, we always have 'http-cookie' added by transport, so we 
                // add data.name even when we don't have a query. This results in postponing the correlation key calculation
                // till the channel calls us back.
                if (data.SendValue)
                {
                    // if the data.Name is for cookie, but there is no user added query for the cookie, we will not 
                    // add this to sendNames. 
                    // Note that we only look at user added queries. This is because http-cookie does not have a default query

                    // 

                    if (data.Name == cookieCorrelationName && !this.IsCookieBasedQueryPresent())
                    {
                        continue;
                    }
                    else
                    {
                        sendNames.Add(data.Name);
                    }
                }
            }

            this.receiveNames = new ReadOnlyCollection<string>(receiveNames);
            this.sendNames = new ReadOnlyCollection<string>(sendNames);
        }

        void ConfigureBindingDefaultQueries(ServiceEndpoint endpoint, ICorrelationDataSource source, bool dispatch)
        {            
            if (!CorrelationQuery.IsQueryCollectionSearchable(this.queries))
            {
                return;
            }

            // we should preserve the message if there are any existing queries added by the user
            if (this.queries.Count <= 0)
            {
                this.shouldPreserveMessage = false;
            }

            foreach (OperationDescription operation in endpoint.Contract.Operations)
            {
                string inAction;
                CorrelationQuery inQuery;
                string outAction = null;
                CorrelationQuery outQuery = null;
                CorrelationQuery noActionReplyQuery = null;

                inAction = operation.Messages[0].Action;
                inQuery = CorrelationQuery.FindCorrelationQueryForAction(this.queries, inAction);
                
                if (!operation.IsOneWay)
                {
                    outAction = operation.Messages[1].Action;
                    outQuery = CorrelationQuery.FindCorrelationQueryForAction(this.queries, outAction);
                    if (!dispatch)
                    {
                        noActionReplyQuery = CorrelationQuery.FindCorrelationQueryForAction(this.queries, String.Empty);
                    }
                }

                // we will not add default query if a query already exists for the action
                bool canDefaultIn = inQuery == null;
                bool canDefaultOut = !operation.IsOneWay && outQuery == null;
                
                // if there are no user added queries for receiveReply, we add a NoActionQuery
                bool addNoActionQueryForReply = !operation.IsOneWay && !dispatch && noActionReplyQuery == null; 

                // On the client side we add special filters, SendFilter and ReceiveFilter
                // But on dispatch side, we use ActionFilter and therefore need to verify that for wildcardaction, we 
                // only add a single defaultquery
                if (canDefaultIn && canDefaultOut)
                {
                    //verify if any of them is a wildcardaction, in that case let's just add a single query with a MatchAllFilter
                    if (inAction == MessageHeaders.WildcardAction)
                    {
                        canDefaultOut = false;
                    }
                    else if (outAction == MessageHeaders.WildcardAction)
                    {
                        canDefaultIn = false;
                    }
                    else if (inAction == outAction)
                    {
                        // in this case we will be adding the same query twice, let's just add once
                        // a possible scenario is when we add a contractDescription with wildcardaction for request & reply
                        canDefaultOut = false;
                    }
                }

                if (!canDefaultIn && !canDefaultOut)
                {
                    continue;
                }
                
                foreach (CorrelationDataDescription data in source.DataSources)
                {
                    if (!data.IsDefault)
                    {
                        continue;
                    }                                   
                    
                    if (canDefaultIn &&
                        (dispatch && data.ReceiveValue || data.SendValue))
                    {
                        inQuery = CreateDefaultCorrelationQuery(inQuery, inAction, data, ref shouldPreserveMessage);                        
                    }

                    if (canDefaultOut && 
                        (dispatch && data.SendValue || data.ReceiveValue))
                    {
                        outQuery = CreateDefaultCorrelationQuery(outQuery, outAction, data, ref shouldPreserveMessage);
                    }
                    
                    if (addNoActionQueryForReply)
                    {
                        noActionReplyQuery = CreateDefaultCorrelationQuery(noActionReplyQuery, String.Empty, data, ref shouldPreserveMessage);
                    }
                }

                if (canDefaultIn && inQuery != null)
                {
                    this.queries.Add(inQuery);
                }

                if (canDefaultOut && outQuery != null )
                {
                    this.queries.Add(outQuery);
                }

                if (addNoActionQueryForReply && noActionReplyQuery != null)
                {
                    this.queries.Add(noActionReplyQuery);
                }
               
            }
        }

        static CorrelationQuery CreateDefaultCorrelationQuery(CorrelationQuery query, string action, CorrelationDataDescription data, ref bool shouldPreserveMessage)
        {
            MessageQuery messageQuery = new XPathMessageQuery
            {
                Expression = string.Format(CultureInfo.InvariantCulture, defaultQueryFormat, data.Name),
                Namespaces = new XPathMessageContext()
            };

            if (data.IsOptional)
            {
                messageQuery = new OptionalMessageQuery
                {
                    Query = messageQuery
                };
            }

           
            if (query == null)
            {
                MessageFilter filter;
                // verify if the data name is added by the context channel
                bool isContextQuery = (data.Name == contextCorrelationName);
                
                // if there is a query that is not a context query set it to true since we might read from
                // the message body
                if (!shouldPreserveMessage && !isContextQuery)
                {
                    shouldPreserveMessage = true;
                }
                // this is a server side query, we use an action filter
                if (action == MessageHeaders.WildcardAction)
                {
                    filter = new MatchAllMessageFilter();
                }
                else
                {
                    filter = new ActionMessageFilter(action);
                }
                
                return new CorrelationQuery
                {
                    Where = filter,

                    IsDefaultContextQuery = isContextQuery,

                    Select = new MessageQuerySet
                    {
                        { data.Name, messageQuery }                           
                    }

                };
                
            }
            else
            {
                query.Select[data.Name] = messageQuery;                
                return query;
            }
        }

        public void Validate(ServiceEndpoint endpoint)
        {            
        }

        void IChannelInitializer.Initialize(IClientChannel channel)
        {
            channel.Extensions.Add(this);
        }

        void IExtension<IContextChannel>.Attach(IContextChannel owner)
        {         
        }

        void IExtension<IContextChannel>.Detach(IContextChannel owner)
        {         
        }        

        public CorrelationKeyCalculator GetKeyCalculator()
        {
            if (this.correlationKeyCalculator == null)
            {
                CorrelationKeyCalculator localKeyCalculator = new CorrelationKeyCalculator(this.ScopeName);

                foreach (CorrelationQuery query in this.queries)
                {                   
                    IDictionary<string, MessageQueryTable<string>> dictionary =
                        new Dictionary<string, MessageQueryTable<string>>();

                    // consider changing Dictionary to Collection
                    int count = 0;
                    foreach (MessageQuerySet querySet in query.SelectAdditional)
                    {
                        dictionary.Add("SelectAdditional_item_" + count, querySet.GetMessageQueryTable());
                        count++;
                    }

                    localKeyCalculator.AddQuery(
                        query.Where,
                        query.Select != null ? query.Select.GetMessageQueryTable() : new MessageQueryTable<string>(),
                        dictionary,
                        query.IsDefaultContextQuery);
                }

                this.correlationKeyCalculator = localKeyCalculator;
            }

            return this.correlationKeyCalculator;
        }
    }       
}
