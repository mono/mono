//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Activities.Dispatcher
{
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    class OptionalMessageQuery : MessageQuery
    {
        public OptionalMessageQuery()
            : base()
        {
        }

        public MessageQuery Query
        {
            get;
            set;
        }

        public override TResult Evaluate<TResult>(MessageBuffer buffer)
        {
            return this.Query.Evaluate<TResult>(buffer);
        }

        public override TResult Evaluate<TResult>(Message message)
        {
            return this.Query.Evaluate<TResult>(message);
        }
    }
}
