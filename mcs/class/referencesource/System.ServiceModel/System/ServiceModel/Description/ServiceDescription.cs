//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Reflection;
    using System.Diagnostics;
    using System.Security;
    using System.Security.Permissions;

    [DebuggerDisplay("ServiceType={serviceType}")]
    public class ServiceDescription
    {
        KeyedByTypeCollection<IServiceBehavior> behaviors = new KeyedByTypeCollection<IServiceBehavior>();
        string configurationName;
        ServiceEndpointCollection endpoints = new ServiceEndpointCollection();
        Type serviceType;
        XmlName serviceName;
        string serviceNamespace = NamingHelper.DefaultNamespace;


        public ServiceDescription()
        {
        }

        internal ServiceDescription(String serviceName)
        {
            if (String.IsNullOrEmpty(serviceName))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceName");

            this.Name = serviceName;
        }

        public ServiceDescription(IEnumerable<ServiceEndpoint> endpoints)
            : this()
        {
            if (endpoints == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoints");

            foreach (ServiceEndpoint endpoint in endpoints)
                this.endpoints.Add(endpoint);
        }

        public string Name
        {
            get
            {
                if (serviceName != null)
                    return serviceName.EncodedName;
                else if (ServiceType != null)
                    return NamingHelper.XmlName(ServiceType.Name);
                else
                    return NamingHelper.DefaultServiceName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    serviceName = null;
                }
                else
                {
                    // the XmlName ctor validate the value
                    serviceName = new XmlName(value, true /*isEncoded*/);
                }
            }
        }

        public string Namespace
        {
            get
            {
                return serviceNamespace;
            }
            set
            {
                serviceNamespace = value;
            }
        }


        public KeyedByTypeCollection<IServiceBehavior> Behaviors
        {
            get { return this.behaviors; }
        }

        public string ConfigurationName
        {
            get { return this.configurationName; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.configurationName = value;
            }
        }

        public ServiceEndpointCollection Endpoints
        {
            get { return this.endpoints; }
        }

        public Type ServiceType
        {
            get { return this.serviceType; }
            set { this.serviceType = value; }
        }

        static void AddBehaviors(ServiceDescription serviceDescription)
        {
            Type type = serviceDescription.ServiceType;

            System.ServiceModel.Description.TypeLoader.ApplyServiceInheritance<IServiceBehavior, KeyedByTypeCollection<IServiceBehavior>>(
                type, serviceDescription.Behaviors, ServiceDescription.GetIServiceBehaviorAttributes);

            ServiceBehaviorAttribute serviceBehavior = EnsureBehaviorAttribute(serviceDescription);

            if (serviceBehavior.Name != null)
                serviceDescription.Name = new XmlName(serviceBehavior.Name).EncodedName;
            if (serviceBehavior.Namespace != null)
                serviceDescription.Namespace = serviceBehavior.Namespace;

            if (String.IsNullOrEmpty(serviceBehavior.ConfigurationName))
            {
                serviceDescription.ConfigurationName = type.FullName;
            }
            else
            {
                serviceDescription.ConfigurationName = serviceBehavior.ConfigurationName;
            }

            AspNetEnvironment.Current.EnsureCompatibilityRequirements(serviceDescription);
        }

        internal static object CreateImplementation(Type serviceType)
        {
            ConstructorInfo constructor = serviceType.GetConstructor(
                TypeLoader.DefaultBindingFlags, null, Type.EmptyTypes, null);
            if (constructor == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR.GetString(SR.SFxNoDefaultConstructor)));
            }

            // Stop the partially trusted callers to use the ServiceDescription.GetService(Type) method to
            // instantiate types in this assembly that are not public or have a non-public default constructor.
            if ((!PartialTrustHelpers.AppDomainFullyTrusted) &&
                (serviceType.IsNotPublic || (!constructor.IsPublic)) &&
                (serviceType.Assembly == typeof(ServiceDescription).Assembly))
            {
                PartialTrustHelpers.DemandForFullTrust();
            }

            try
            {
                object implementation = constructor.Invoke(
                    TypeLoader.DefaultBindingFlags, null, null, System.Globalization.CultureInfo.InvariantCulture);
                return implementation;
            }
            catch (MethodAccessException methodAccessException)
            {
                SecurityException securityException = methodAccessException.InnerException as SecurityException;
                if (securityException != null && securityException.PermissionType.Equals(typeof(ReflectionPermission)))
                {
                    DiagnosticUtility.TraceHandledException(methodAccessException, TraceEventType.Warning);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new SecurityException(SR.GetString(
                                SR.PartialTrustServiceCtorNotVisible,
                                serviceType.FullName)));
                }
                else
                {
                    throw;
                }
            }
        }

        static ServiceBehaviorAttribute EnsureBehaviorAttribute(ServiceDescription description)
        {
            ServiceBehaviorAttribute attr = description.Behaviors.Find<ServiceBehaviorAttribute>();

            if (attr == null)
            {
                attr = new ServiceBehaviorAttribute();
                description.Behaviors.Insert(0, attr);
            }

            return attr;
        }

        // This method ensures that the description object graph is structurally sound and that none
        // of the fundamental SFx framework assumptions have been violated.
        internal void EnsureInvariants()
        {
            for (int i = 0; i < this.Endpoints.Count; i++)
            {
                ServiceEndpoint endpoint = this.Endpoints[i];
                if (endpoint == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.AChannelServiceEndpointIsNull0)));
                }
                endpoint.EnsureInvariants();
            }
        }

        static void GetIServiceBehaviorAttributes(Type currentServiceType, KeyedByTypeCollection<IServiceBehavior> behaviors)
        {
            foreach (IServiceBehavior behaviorAttribute in ServiceReflector.GetCustomAttributes(currentServiceType, typeof(IServiceBehavior)))
            {
                behaviors.Add(behaviorAttribute);
            }
        }

        public static ServiceDescription GetService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceType");
            }

            if (!serviceType.IsClass)
            {
                throw new ArgumentException(SR.GetString(SR.SFxServiceHostNeedsClass));
            }

            ServiceDescription description = new ServiceDescription();
            description.ServiceType = serviceType;

            AddBehaviors(description);
            SetupSingleton(description, null, false);
            return description;
        }

        public static ServiceDescription GetService(object serviceImplementation)
        {
            if (serviceImplementation == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceImplementation");
            }

            Type serviceType = serviceImplementation.GetType();
            ServiceDescription description = new ServiceDescription();
            description.ServiceType = serviceType;

            if (serviceImplementation is IServiceBehavior)
            {
                description.Behaviors.Add((IServiceBehavior)serviceImplementation);
            }

            AddBehaviors(description);
            SetupSingleton(description, serviceImplementation, true);
            return description;
        }

        static void SetupSingleton(ServiceDescription serviceDescription, object implementation, bool isWellKnown)
        {
            ServiceBehaviorAttribute serviceBehavior = EnsureBehaviorAttribute(serviceDescription);
            Type type = serviceDescription.ServiceType;
            if ((implementation == null) && (serviceBehavior.InstanceContextMode == InstanceContextMode.Single))
            {
                implementation = CreateImplementation(type);
            }

            if (isWellKnown)
            {
                serviceBehavior.SetWellKnownSingleton(implementation);
            }
            else if ((implementation != null) && (serviceBehavior.InstanceContextMode == InstanceContextMode.Single))
            {
                serviceBehavior.SetHiddenSingleton(implementation);
            }
        }
    }
}

