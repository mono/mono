// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace Microsoft.Activities.Presentation.Xaml
{
    using System;
    using System.Collections;
    using System.ComponentModel.Composition;
    using System.Reflection;

    class ImportAttributeInfo : AttributeInfo<ImportAttribute>
    {
        static ConstructorInfo nameConstructor;
        static ConstructorInfo typeConstructor;
        static ConstructorInfo nameAndTypeConstructor;

        public override bool IsComplete
        {
            get { return false; }
        }

        public override ICollection GetConstructorArguments(ImportAttribute attribute, ref ConstructorInfo constructor)
        {
            if (attribute.ContractName != null)
            {
                if (attribute.ContractType != null)
                {
                    constructor = NameAndTypeConstructor;
                    return new object[] { attribute.ContractName, attribute.ContractType };
                }
                else
                {
                    constructor = NameConstructor;
                    return new object[] { attribute.ContractName };
                }
            }
            else if (attribute.ContractType != null)
            {
                constructor = TypeConstructor;
                return new object[] { attribute.ContractType };
            }
            else
            {
                return new object[] { };
            }
        }

        public override ConstructorInfo GetConstructor()
        {
            return typeof(ImportAttribute).GetConstructor(Type.EmptyTypes);
        }

        static ConstructorInfo NameConstructor
        {
            get
            {
                if (nameConstructor == null)
                {
                    nameConstructor = typeof(ImportAttribute).GetConstructor(new Type[] { typeof(string) });
                }
                return nameConstructor;
            }
        }

        static ConstructorInfo NameAndTypeConstructor
        {
            get
            {
                if (nameAndTypeConstructor == null)
                {
                    nameAndTypeConstructor = typeof(ImportAttribute).GetConstructor(new Type[] { typeof(string), typeof(Type) });
                }
                return nameAndTypeConstructor;
            }
        }

        static ConstructorInfo TypeConstructor
        {
            get
            {
                if (typeConstructor == null)
                {
                    typeConstructor = typeof(ImportAttribute).GetConstructor(new Type[] { typeof(Type) });
                }
                return typeConstructor;
            }
        }
    }
}
