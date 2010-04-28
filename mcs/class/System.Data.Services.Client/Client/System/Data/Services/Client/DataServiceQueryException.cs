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
    using System;

#if !ASTORIA_LIGHT
    [Serializable]
#endif
    [System.Diagnostics.DebuggerDisplay("{Message}")]
    public sealed class DataServiceQueryException : InvalidOperationException
    {
        #region Private fields.

#if !ASTORIA_LIGHT
        [NonSerialized]
#endif
        private readonly QueryOperationResponse response;

        #endregion Private fields.

        #region Constructors.

        public DataServiceQueryException()
            : base(Strings.DataServiceException_GeneralError)
        {
        }

        public DataServiceQueryException(string message)
            : base(message)
        {
        }

        public DataServiceQueryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public DataServiceQueryException(string message, Exception innerException, QueryOperationResponse response)
            : base(message, innerException)
        {
            this.response = response;
        }

#if !ASTORIA_LIGHT
#pragma warning disable 0628
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1047", Justification = "Follows serialization info pattern.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032", Justification = "Follows serialization info pattern.")]
        protected DataServiceQueryException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
#pragma warning restore 0628
#endif

        #endregion Constructors.

        #region Public properties.

        public QueryOperationResponse Response
        {
            get { return this.response; }
        }

        #endregion Public properties.
    }
}
