//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    [AttributeUsage(ServiceModelAttributeTargets.ServiceContract | ServiceModelAttributeTargets.OperationContract, Inherited = true, AllowMultiple = true)]
    public sealed class ServiceKnownTypeAttribute : Attribute
    {
        Type declaringType;
        string methodName;
        Type type;

        private ServiceKnownTypeAttribute()
        {
            // Disallow default constructor
        }

        public ServiceKnownTypeAttribute(Type type)
        {
            this.type = type;
        }

        public ServiceKnownTypeAttribute(string methodName)
        {
            this.methodName = methodName;
        }

        public ServiceKnownTypeAttribute(string methodName, Type declaringType)
        {
            this.methodName = methodName;
            this.declaringType = declaringType;
        }

        public Type DeclaringType
        {
            get { return declaringType; }
        }

        public string MethodName
        {
            get { return methodName; }
        }

        public Type Type
        {
            get { return type; }
        }
    }
}

