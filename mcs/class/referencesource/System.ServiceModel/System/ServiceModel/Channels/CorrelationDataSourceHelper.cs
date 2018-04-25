//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    class CorrelationDataSourceHelper : ICorrelationDataSource
    {
        ICollection<CorrelationDataDescription> dataSources;

        public CorrelationDataSourceHelper(ICollection<CorrelationDataDescription> dataSources)
        {
            if (dataSources.IsReadOnly)
            {
                this.dataSources = dataSources;
            }
            else
            {
                this.dataSources = new ReadOnlyCollection<CorrelationDataDescription>(new List<CorrelationDataDescription>(dataSources));
            }
        }

        CorrelationDataSourceHelper(ICollection<CorrelationDataDescription> dataSource1, ICollection<CorrelationDataDescription> dataSource2)
        {
            List<CorrelationDataDescription> newData = new List<CorrelationDataDescription>(dataSource1);

            foreach (CorrelationDataDescription correlationData in dataSource2)
            {
                newData.Add(correlationData);
            }

            this.dataSources = new ReadOnlyCollection<CorrelationDataDescription>(newData);
        }

        public static ICorrelationDataSource Combine(ICorrelationDataSource dataSource1, ICorrelationDataSource dataSource2)
        {
            if (dataSource1 == null)
            {
                return dataSource2;
            }

            if (dataSource2 == null)
            {
                return dataSource1;
            }

            return new CorrelationDataSourceHelper(dataSource1.DataSources, dataSource2.DataSources);
        }

        ICollection<CorrelationDataDescription> ICorrelationDataSource.DataSources
        {
            get { return this.dataSources; }
        }
    }
}
