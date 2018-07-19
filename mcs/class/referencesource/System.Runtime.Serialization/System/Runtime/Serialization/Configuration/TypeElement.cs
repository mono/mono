//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime.Serialization.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.ServiceModel.Diagnostics;
    using System.Security;

    public sealed partial class TypeElement : ConfigurationElement
    {
        public TypeElement()
        {
        }

        public TypeElement(string typeName)
            : this()
        {
            if (String.IsNullOrEmpty(typeName))
            {
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("typeName");
            }
            this.Type = typeName;
        }

        internal string Key
        {
            get { return this.key; }
        }

        [ConfigurationProperty(ConfigurationStrings.DefaultCollectionName, DefaultValue = null, Options = ConfigurationPropertyOptions.IsDefaultCollection)]
        public ParameterElementCollection Parameters
        {
            get { return (ParameterElementCollection)base[ConfigurationStrings.DefaultCollectionName]; }
        }

        protected override void Reset(ConfigurationElement parentElement)
        {
            TypeElement parent = (TypeElement)parentElement;
            this.key = parent.key;
            base.Reset(parentElement);
        }

        [ConfigurationProperty(ConfigurationStrings.Type, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string Type
        {
            get { return (string)base[ConfigurationStrings.Type]; }
            set { base[ConfigurationStrings.Type] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.Index, DefaultValue = 0)]
        [IntegerValidator(MinValue = 0)]
        public int Index
        {
            get { return (int)base[ConfigurationStrings.Index]; }
            set { base[ConfigurationStrings.Index] = value; }
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - Loads type given name in configuration."
            + " Since this information is used to determine whether a particular type is included as a known type,"
            + " changes to the logic should be reviewed.")]
        internal Type GetType(string rootType, Type[] typeArgs)
        {
            return GetType(rootType, typeArgs, this.Type, this.Index, this.Parameters);
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - Loads type given name in configuration."
            + " Since this information is used to determine whether a particular type is included as a known type,"
            + " changes to the logic should be reviewed.")]
        internal static Type GetType(string rootType, Type[] typeArgs, string type, int index, ParameterElementCollection parameters)
        {
            if (String.IsNullOrEmpty(type))
            {
                if (typeArgs == null || index >= typeArgs.Length)
                {
                    int typeArgsCount = typeArgs == null ? 0 : typeArgs.Length;
                    if (typeArgsCount == 0)
                    {
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.KnownTypeConfigIndexOutOfBoundsZero,
                            rootType,
                            typeArgsCount,
                            index));
                    }
                    else
                    {
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.KnownTypeConfigIndexOutOfBounds,
                            rootType,
                            typeArgsCount,
                            index));
                    }
                }

                return typeArgs[index];
            }

            Type t = System.Type.GetType(type, true);
            if (t.IsGenericTypeDefinition)
            {
                if (parameters.Count != t.GetGenericArguments().Length)
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.KnownTypeConfigGenericParamMismatch,
                        type,
                        t.GetGenericArguments().Length,
                        parameters.Count));

                Type[] types = new Type[parameters.Count];
                for (int i = 0; i < types.Length; ++i)
                {
                    types[i] = parameters[i].GetType(rootType, typeArgs);
                }
                t = t.MakeGenericType(types);
            }
            return t;
        }

        string key = Guid.NewGuid().ToString();
    }
}
