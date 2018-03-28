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
    /// from an <see cref="HttpResponseMessage"/> instance.
    /// </summary>
    public static class HttpResponseMessageExtensionMethods
    {
        /// <summary>
        /// An extension method for getting a <see cref="Message"/> instance
        /// from an <see cref="HttpResponseMessage"/> instance.
        /// </summary>
        /// <remarks>
        /// The <see cref="Message"/> instance can be read, written and copied 
        /// just as a traditional <see cref="ByteStreamMessage"/> instance. The
        /// <see cref="Message"/> instance can also "read" to retrieve the original
        /// <see cref="HttpResponseMessage"/> instance by calling the 
        /// <see cref="MessageExtensionMethods.ToHttpResponseMessage">
        /// Message.ToHttpResponseMessage()</see> extension method.
        /// </remarks>
        /// <param name="httpResponseMessage">The <see cref="HttpResponseMessage"/>
        /// from which to create the <see cref="Message"/> instance.</param>
        /// <returns>The new <see cref="Message"/> instance.</returns>
        public static Message ToMessage(this HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage == null)
            {
                throw FxTrace.Exception.ArgumentNull("httpResponseMessage");
            }

            Message message = ByteStreamMessage.CreateMessage(httpResponseMessage, null);
            message.ConfigureAsHttpMessage(httpResponseMessage);
            
            return message;
        }
    }
}
