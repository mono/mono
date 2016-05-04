//------------------------------------------------------------------------------
// <copyright file="DataSourceSelectArguments.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.UI {

    using System.Diagnostics;
    using System.Security.Permissions;

    public sealed class DataSourceSelectArguments {
        private DataSourceCapabilities _requestedCapabilities;
        private DataSourceCapabilities _supportedCapabilities;

        private int _maximumRows;
        private bool _retrieveTotalRowCount;
        private string _sortExpression;
        private int _startRowIndex;
        private int _totalRowCount = -1;
        

        public DataSourceSelectArguments() : this(String.Empty, 0, 0) {
        }

        public DataSourceSelectArguments(string sortExpression) : this(sortExpression, 0, 0) {
        }


        public DataSourceSelectArguments(int startRowIndex, int maximumRows) : this(String.Empty, startRowIndex, maximumRows) {
        }


        public DataSourceSelectArguments(string sortExpression, int startRowIndex, int maximumRows) {
            SortExpression = sortExpression;
            StartRowIndex = startRowIndex;
            MaximumRows = maximumRows;
        }

        // Empty cannot be a static readonly field because we want each requester to get their own copy.
        // This is because DataSourceViews need to call AddSupportedCapabilities on the DataSourceSelectArguments,
        // changing it to be suited to the DataSourceView's needs.  If another DataSourceView used the same instance,
        // the supported capabilities would be wrong.  This member stays as a property for programming ease and 
        // returns a new instance each time.  If the user wants to change this instance, they're free to.
        public static DataSourceSelectArguments Empty {
            get {
                return new DataSourceSelectArguments();
            }
        }

        /// <devdoc>
        /// The maximum number of rows requested for a paged data request.
        /// Use 0 to indicate all rows.
        /// </devdoc>
        public int MaximumRows { 
            get {
                return _maximumRows;
            }
            set {
                if (value == 0) {
                    if (_startRowIndex == 0) {
                        _requestedCapabilities &= ~DataSourceCapabilities.Page;
                    }
                }
                else {
                    _requestedCapabilities |= DataSourceCapabilities.Page;
                }
                _maximumRows = value;
            }
        }


        /// <devdoc>
        /// Indicates whether the total row count is requested
        /// </devdoc>
        public bool RetrieveTotalRowCount {
            get {
                return _retrieveTotalRowCount;
            }
            set {
                if (value) {
                    _requestedCapabilities |= DataSourceCapabilities.RetrieveTotalRowCount;
                }
                else {
                    _requestedCapabilities &= ~DataSourceCapabilities.RetrieveTotalRowCount;
                }
                _retrieveTotalRowCount = value;
            }
        }


        /// <devdoc>
        /// The expression used to sort the data.
        /// </devdoc>
        public string SortExpression {
            get {
                if (_sortExpression == null)
                    _sortExpression = String.Empty;
                return _sortExpression;
            }
            set {
                if (String.IsNullOrEmpty(value)) {
                    _requestedCapabilities &= ~DataSourceCapabilities.Sort;
                }
                else {
                    _requestedCapabilities |= DataSourceCapabilities.Sort;
                }
                _sortExpression = value;
            }
        }


        /// <devdoc>
        /// The index of the first row requested for a paged data request
        /// </devdoc>
        public int StartRowIndex {
            get {
                return _startRowIndex;
            }
            set {
                if (value == 0) {
                    if (_maximumRows == 0) {
                        _requestedCapabilities &= ~DataSourceCapabilities.Page;
                    }
                }
                else {
                    _requestedCapabilities |= DataSourceCapabilities.Page;
                }
                _startRowIndex = value;
            }
        }


        /// <devdoc>
        /// The number of rows returned by the query that counts the number of rows.  Typically
        /// set by the DataSource.
        /// </devdoc>
        public int TotalRowCount {
            get {
                return _totalRowCount;
            }
            set {
                _totalRowCount = value;
            }
        }
     

        /// <devdoc>
        /// DataSource controls would call this for each capability that it handled.  
        /// It would do the bitwise operations to handle determining what capabilities were left
        /// over at the end for RaiseUnsupportedCapabilitiesError to handle.
        /// </devdoc>
        public void AddSupportedCapabilities(DataSourceCapabilities capabilities) {
            _supportedCapabilities |= capabilities;
        }


        /// <devdoc>
        /// Prevents a compiler error because Equals was overridden
        /// </devdoc>
        public override int GetHashCode() {
            return System.Web.Util.HashCodeCombiner.CombineHashCodes(_maximumRows.GetHashCode(),
                                                                     _retrieveTotalRowCount.GetHashCode(),
                                                                     _sortExpression.GetHashCode(),
                                                                     _startRowIndex.GetHashCode(),
                                                                     _totalRowCount.GetHashCode());
        }


        public override bool Equals(object obj) {
            DataSourceSelectArguments arguments = obj as DataSourceSelectArguments;
            if (arguments != null) {
                return ((arguments.MaximumRows == _maximumRows) &&
                        (arguments.RetrieveTotalRowCount == _retrieveTotalRowCount) &&
                        (arguments.SortExpression == _sortExpression) &&
                        (arguments.StartRowIndex == _startRowIndex) &&
                        (arguments.TotalRowCount == _totalRowCount));
            }

            return false;
        }
     

        /// <devdoc>
        /// Select implementations would call this method to raise errors on unsupported capabilities.
        /// </devdoc>
        public void RaiseUnsupportedCapabilitiesError(DataSourceView view) {
            DataSourceCapabilities unsupportedCapabilities;
            unsupportedCapabilities = _requestedCapabilities & ~_supportedCapabilities;
            
            if ((unsupportedCapabilities & DataSourceCapabilities.Sort) != 0) {
                view.RaiseUnsupportedCapabilityError(DataSourceCapabilities.Sort);
            }

            if ((unsupportedCapabilities & DataSourceCapabilities.Page) != 0) {
                view.RaiseUnsupportedCapabilityError(DataSourceCapabilities.Page);
            }

            if ((unsupportedCapabilities & DataSourceCapabilities.RetrieveTotalRowCount) != 0) {
                view.RaiseUnsupportedCapabilityError(DataSourceCapabilities.RetrieveTotalRowCount);
            }

            Debug.Assert(unsupportedCapabilities == 0, "unknown capability not supported");
        }
    }
}
