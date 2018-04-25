//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Runtime;

    public class NetworkInterfaceMessageProperty
    {
        const string PropertyName = "NetworkInterfaceMessageProperty";

        public NetworkInterfaceMessageProperty(int interfaceIndex)
        {
            this.InterfaceIndex = interfaceIndex;
        }

        NetworkInterfaceMessageProperty(NetworkInterfaceMessageProperty other)
        {
            this.InterfaceIndex = other.InterfaceIndex;
        }

        public static string Name
        {
            get { return PropertyName; }
        }

        public int InterfaceIndex
        {
            get;
            private set;
        }

        public static bool TryGet(Message message, out NetworkInterfaceMessageProperty property)
        {
            if (message == null)
            {
                throw FxTrace.Exception.ArgumentNull("message");
            }

            return TryGet(message.Properties, out property);
        }

        public static bool TryGet(MessageProperties properties, out NetworkInterfaceMessageProperty property)
        {
            if (properties == null)
            {
                throw FxTrace.Exception.ArgumentNull("properties");
            }

            object value = null;
            if (properties.TryGetValue(PropertyName, out value))
            {
                property = value as NetworkInterfaceMessageProperty;
            }
            else
            {
                property = null;
            }
            return property != null;
        }

        public void AddTo(Message message)
        {
            if (message == null)
            {
                throw FxTrace.Exception.ArgumentNull("message");
            }

            AddTo(message.Properties);
        }

        public void AddTo(MessageProperties properties)
        {
            if (properties == null)
            {
                throw FxTrace.Exception.ArgumentNull("properties");
            }

            properties.Add(NetworkInterfaceMessageProperty.Name, this);
        }
    }
}
