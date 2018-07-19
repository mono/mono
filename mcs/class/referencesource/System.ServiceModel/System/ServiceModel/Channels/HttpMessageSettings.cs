// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System.Net.Http;

    /// <summary>
    /// A binding parameter that can be used with the HTTP Transport to 
    /// specify the setting for HttpMessage support.
    /// </summary>
    public sealed class HttpMessageSettings : IEquatable<HttpMessageSettings>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the HTTP transport should
        /// support <see cref="HttpRequestMessage"/> and <see cref="HttpResponseMessage"/>
        /// instances.
        /// </summary>
        public bool HttpMessagesSupported { get; set; }

        public bool Equals(HttpMessageSettings other)
        {
            return other == null ?
                false :
                other.HttpMessagesSupported == this.HttpMessagesSupported;
        }
    } 
}
