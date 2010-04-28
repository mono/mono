//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.


namespace System.Data.Services.Client
{
    #region Namespaces.

    using System.Collections.Generic;
    using System.Linq;

    #endregion Namespaces.

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710", Justification = "required for this feature")]
    public sealed class QueryOperationResponse<T> : QueryOperationResponse, IEnumerable<T>
    {
        #region Constructors.

        internal QueryOperationResponse(Dictionary<string, string> headers, DataServiceRequest query, MaterializeAtom results)
            : base(headers, query, results)
        {
        }

        #endregion Constructors.

        #region Public properties.

        public override long TotalCount
        {
            get
            {
                if (this.Results != null && !this.Results.IsEmptyResults)
                {
                    return this.Results.CountValue();
                }
                else
                {
                    throw new InvalidOperationException(Strings.MaterializeFromAtom_CountNotPresent);
                }
            }
        }

        #endregion Public properties.

        #region Public methods.

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "required for this feature")]
        public new DataServiceQueryContinuation<T> GetContinuation()
        {
            return (DataServiceQueryContinuation<T>)base.GetContinuation();
        }

        public new IEnumerator<T> GetEnumerator()
        {
            return this.Results.Cast<T>().GetEnumerator();
        }

        #endregion Public methods.
    }
}
