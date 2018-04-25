//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel.Channels;
    using System.Collections.Generic;
    
    public interface IMessageFilterTable<TFilterData> : IDictionary<MessageFilter, TFilterData>
    {
        // return a single match
        bool GetMatchingValue(Message message, out TFilterData value);
        bool GetMatchingValue(MessageBuffer messageBuffer, out TFilterData value);

        // return multiple matches
        bool GetMatchingValues(Message message, ICollection<TFilterData> results);
        bool GetMatchingValues(MessageBuffer messageBuffer, ICollection<TFilterData> results);

        // If you need both the filter and the data, use these functions then look up
        // the data using the filter tables IDictionary methods.
        bool GetMatchingFilter(Message message, out MessageFilter filter);
        bool GetMatchingFilter(MessageBuffer messageBuffer, out MessageFilter filter);
        bool GetMatchingFilters(Message message, ICollection<MessageFilter> results);
        bool GetMatchingFilters(MessageBuffer messageBuffer, ICollection<MessageFilter> results);
    }    
}
