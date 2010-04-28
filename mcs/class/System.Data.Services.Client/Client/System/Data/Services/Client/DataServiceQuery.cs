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
    using System.Collections;
    using System.Linq;
    using System.Linq.Expressions;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1010", Justification = "required for this feature")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710", Justification = "required for this feature")]
    public abstract class DataServiceQuery : DataServiceRequest, IQueryable
    {
        internal DataServiceQuery()
        {
        }

        public abstract Expression Expression
        {
            get;
        }

        public abstract IQueryProvider Provider
        {
            get;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033", Justification = "required for this feature")]
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw Error.NotImplemented();
        }

#if !ASTORIA_LIGHT
        public IEnumerable Execute()
        {
            return this.ExecuteInternal();
        }
#endif

        public IAsyncResult BeginExecute(AsyncCallback callback, object state)
        {
            return this.BeginExecuteInternal(callback, state);
        }

        public IEnumerable EndExecute(IAsyncResult asyncResult)
        {
            return this.EndExecuteInternal(asyncResult);
        }

#if !ASTORIA_LIGHT
        internal abstract IEnumerable ExecuteInternal();
#endif

        internal abstract IAsyncResult BeginExecuteInternal(AsyncCallback callback, object state);

        internal abstract IEnumerable EndExecuteInternal(IAsyncResult asyncResult);
    }
}
