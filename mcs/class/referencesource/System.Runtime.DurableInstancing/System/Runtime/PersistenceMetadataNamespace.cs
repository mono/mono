//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime
{
    using System.Xml.Linq;

    [Fx.Tag.XamlVisible(false)]
    static class PersistenceMetadataNamespace
    {
        const string baseNamespace = "urn:schemas-microsoft-com:System.Runtime.DurableInstancing/4.0/metadata";
        static readonly XNamespace persistenceMetadataNamespace = XNamespace.Get(baseNamespace);

        static XName instanceType;
        static XName activationType;

        public static XName InstanceType
        {
            get
            {
                if (instanceType == null)
                {
                    instanceType = persistenceMetadataNamespace.GetName("InstanceType");
                }

                return instanceType;
            }
        }

        public static XName ActivationType
        {
            get
            {
                if (activationType == null)
                {
                    activationType = persistenceMetadataNamespace.GetName("ActivationType");
                }

                return activationType;
            }
        }

        public static class ActivationTypes
        {
            const string baseNamespace = "urn:schemas-microsoft-com:System.ServiceModel.Activation";
            static readonly XNamespace activationNamespace = XNamespace.Get(baseNamespace);

            static XName was;

            public static XName WAS
            {
                get
                {
                    if (was == null)
                    {
                        was = activationNamespace.GetName("WindowsProcessActivationService");
                    }

                    return was;
                }
            }
        }
    }
}
