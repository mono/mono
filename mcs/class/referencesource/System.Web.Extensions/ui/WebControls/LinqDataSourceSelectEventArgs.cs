//------------------------------------------------------------------------------
// <copyright file="LinqDataSourceSelectEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    public class LinqDataSourceSelectEventArgs : CancelEventArgs {

        private DataSourceSelectArguments _arguments;
        private IDictionary<string, object> _groupByParameters;
        private IOrderedDictionary _orderByParameters;
        private IDictionary<string, object> _orderGroupsByParameters;
        private object _result;
        private IDictionary<string, object> _selectParameters;
        private IDictionary<string, object> _whereParameters;

        public LinqDataSourceSelectEventArgs(DataSourceSelectArguments arguments,
                IDictionary<string, object> whereParameters, IOrderedDictionary orderByParameters,
                IDictionary<string, object> groupByParameters, IDictionary<string, object> orderGroupsByParameters,
                IDictionary<string, object> selectParameters) {
            _arguments = arguments;
            _groupByParameters = groupByParameters;
            _orderByParameters = orderByParameters;
            _orderGroupsByParameters = orderGroupsByParameters;
            _selectParameters = selectParameters;
            _whereParameters = whereParameters;
        }

        public DataSourceSelectArguments Arguments {
            get {
                return _arguments;
            }
        }

        public IDictionary<string, object> GroupByParameters {
            get {
                return _groupByParameters;
            }
        }

        public IOrderedDictionary OrderByParameters {
            get {
                return _orderByParameters;
            }
        }

        public IDictionary<string, object> OrderGroupsByParameters {
            get {
                return _orderGroupsByParameters;
            }
        }

        public object Result {
            get {
                return _result;
            }
            set {
                _result = value;
            }
        }

        public IDictionary<string, object> SelectParameters {
            get {
                return _selectParameters;
            }
        }

        public IDictionary<string, object> WhereParameters {
            get {
                return _whereParameters;
            }
        }

    }
}

