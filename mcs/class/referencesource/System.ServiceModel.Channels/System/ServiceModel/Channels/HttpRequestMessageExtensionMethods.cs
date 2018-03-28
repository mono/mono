// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System;
    using System.Net.Http;
    using System.Runtime;

    /// <summary>
    /// A static extension methods class for getting a <see cref="Message"/> instance
    /// from an <see cref="HttpRequestMessage"/> instance.
    /// </summary>
    public static class HttpRequestMessageExtensionMethods
    {
        /// <summary>
        /// An extension method for getting a <see cref="Message"/> instance
        /// from an <see cref="HttpRequestMessage"/> instance.
        /// </summary>
        /// <remarks>
        /// The <see cref="Message"/> instance can be read, written and copied 
        /// just as a traditional <see cref="ByteStreamMessage"/> instance. The
        /// <see cref="Message"/> instance can also "read" to retrieve the original
        /// <see cref="HttpRequestMessage"/> instance by calling the 
        /// <see cref="MessageExtensionMethods.ToHttpRequestMessage">
        /// Message.ToHttpRequestMessage()</see> extension method.
        /// </remarks>
        /// <param name="httpRequestMessage">The <see cref="HttpRequestMessage"/>
        /// from which to create the <see cref="Message"/> instance.</param>
        /// <returns>The new <see cref="Message"/> instance.</returns>
        public static Message ToMessage(this HttpRequestMessage httpRequestMessage)
        {
            if (httpRequestMessage == null)
            {
                throw FxTrace.Exception.ArgumentNull("httpRequestMessage");
            }

            Message message = ByteStreamMessage.CreateMessage(httpRequestMessage, null);
            message.ConfigureAsHttpMessage(httpRequestMessage);

            return message;
        }
    }
}
