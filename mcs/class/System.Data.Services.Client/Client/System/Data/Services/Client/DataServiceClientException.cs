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
    using System.Security.Permissions;

#if !ASTORIA_LIGHT
    [Serializable]
#endif
    [System.Diagnostics.DebuggerDisplay("{Message}")]
    public sealed class DataServiceClientException : InvalidOperationException
    {
        private readonly int statusCode;

        #region Constructors.

        public DataServiceClientException()
            : this(Strings.DataServiceException_GeneralError)
        {
        }

        public DataServiceClientException(string message)
            : this(message, null)
        {
        }

        public DataServiceClientException(string message, Exception innerException)
            : this(message, innerException, 500)
        {
        }

        public DataServiceClientException(string message, int statusCode)
            : this(message, null, statusCode)
        {
        }

        public DataServiceClientException(string message, Exception innerException, int statusCode)
            : base(message, innerException)
        {
            this.statusCode = statusCode;
        }

#if !ASTORIA_LIGHT
#pragma warning disable 0628
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1047", Justification = "Follows serialization info pattern.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032", Justification = "Follows serialization info pattern.")]
        protected DataServiceClientException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext context)
            : base(serializationInfo, context)
        {
            if (serializationInfo != null)
            {
                this.statusCode = serializationInfo.GetInt32("statusCode");
            }
        }
#pragma warning restore 0628
#endif

        #endregion Constructors.

        #region Public properties.

        public int StatusCode
        {
            get { return this.statusCode; }
        }

        #endregion Public properties.

        #region Methods.

#if !ASTORIA_LIGHT
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        [System.Security.SecurityCritical]
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            if (info != null)
            {
                info.AddValue("statusCode", this.statusCode);
            }

            base.GetObjectData(info, context);
        }
#endif
        #endregion Methods.
    }
}
