//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = true)]
    public sealed class KnownTypeAttribute : Attribute
    {
        string methodName;
        Type type;

        private KnownTypeAttribute()
        {
            // Disallow default constructor
        }

        public KnownTypeAttribute(Type type)
        {
            this.type = type;
        }

        public KnownTypeAttribute(string methodName)
        {
            this.methodName = methodName;
        }

        public string MethodName
        {
            get { return methodName; }
            //set { methodName = value; }
        }

        public Type Type
        {
            get { return type; }
            //set { type = value; }
        }
    }
}

