//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.WorkflowServices, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    [Serializable]
    public class ContextMessageProperty : IMessageProperty
    {
        internal const string InstanceIdKey = "instanceId";
        const string PropertyName = "ContextMessageProperty";
        static ContextMessageProperty empty;
        IDictionary<string, string> contextStore;

        public ContextMessageProperty()
        {
            this.contextStore = new ContextDictionary();
        }

        public ContextMessageProperty(IDictionary<string, string> context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            this.contextStore = new ContextDictionary(context);
        }

        public static string Name
        {
            get
            {
                return PropertyName;
            }
        }

        public IDictionary<string, string> Context
        {
            get
            {
                return this.contextStore;
            }
        }

        internal static ContextMessageProperty Empty
        {
            get
            {
                if (empty == null)
                {
                    ContextMessageProperty context = new ContextMessageProperty();
                    context.contextStore = ContextDictionary.Empty;
                    empty = context;
                }
                return empty;
            }
        }

        public static bool TryCreateFromHttpCookieHeader(string httpCookieHeader, out ContextMessageProperty context)
        {
            return ContextProtocol.HttpCookieToolbox.TryCreateFromHttpCookieHeader(httpCookieHeader, out context);
        }

        public static bool TryGet(Message message, out ContextMessageProperty contextMessageProperty)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            return TryGet(message.Properties, out contextMessageProperty);
        }

        public static bool TryGet(MessageProperties properties, out ContextMessageProperty contextMessageProperty)
        {
            if (properties == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("properties");
            }

            object value = null;
            if (properties.TryGetValue(PropertyName, out value))
            {
                contextMessageProperty = value as ContextMessageProperty;
            }
            else
            {
                contextMessageProperty = null;
            }

            return contextMessageProperty != null;
        }

        public void AddOrReplaceInMessage(Message message)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            this.AddOrReplaceInMessageProperties(message.Properties);
        }

        public void AddOrReplaceInMessageProperties(MessageProperties properties)
        {
            if (properties == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("properties");
            }

            properties[PropertyName] = this;
        }


        public IMessageProperty CreateCopy()
        {
            return new ContextMessageProperty(this.Context);
        }
    }
}
