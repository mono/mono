//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Runtime.Collections;
    using System.Windows.Markup;

    [ContentProperty("Bounds")]
    public sealed class NewArray<TResult> : CodeActivity<TResult>
    {
        Collection<Argument> bounds;
        ConstructorInfo constructorInfo;

        public Collection<Argument> Bounds
        {
            get
            {
                if (this.bounds == null)
                {
                    this.bounds = new ValidatingCollection<Argument>
                    {
                        // disallow null values
                        OnAddValidationCallback = item =>
                        {
                            if (item == null)
                            {
                                throw FxTrace.Exception.ArgumentNull("item");
                            }
                        }
                    };
                }
                return this.bounds;
            }
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (!typeof(TResult).IsArray)
            {
                metadata.AddValidationError(SR.NewArrayRequiresArrayTypeAsResultType);

                // We shortcut any further processing in this case.
                return;
            }

            bool foundError = false;

            // Loop through each argument, validate it, and if validation
            // passed expose it to the metadata
            Type[] types = new Type[this.Bounds.Count];
            for (int i = 0; i < this.Bounds.Count; i++)
            {
                Argument argument = this.Bounds[i];
                if (argument == null || argument.IsEmpty)
                {
                    metadata.AddValidationError(SR.ArgumentRequired("Bounds", typeof(NewArray<TResult>)));
                    foundError = true;
                }
                else
                {
                    if (!isIntegralType(argument.ArgumentType))
                    {
                        metadata.AddValidationError(SR.NewArrayBoundsRequiresIntegralArguments);
                        foundError = true;
                    }
                    else
                    {
                        RuntimeArgument runtimeArgument = new RuntimeArgument("Argument" + i, this.Bounds[i].ArgumentType, this.bounds[i].Direction, true);
                        metadata.Bind(this.Bounds[i], runtimeArgument);
                        metadata.AddArgument(runtimeArgument);

                        types[i] = argument.ArgumentType;
                    }
                }
            }

            // If we didn't find any errors in the arguments then
            // we can look for an appropriate constructor.
            if (!foundError)
            {
                this.constructorInfo = typeof(TResult).GetConstructor(types);
                if (this.constructorInfo == null)
                {
                    metadata.AddValidationError(SR.ConstructorInfoNotFound(typeof(TResult).Name));
                }
            }
        } 

        protected override TResult Execute(CodeActivityContext context)
        {
            object[] objects = new object[this.Bounds.Count];
            int i = 0;
            foreach (Argument argument in this.Bounds)
            {
                objects[i] = argument.Get(context);
                i++;
            }
            TResult result = (TResult)this.constructorInfo.Invoke(objects);
            return result;
        }

        bool isIntegralType(Type type)
        {
            if (type == typeof(sbyte) || type == typeof(byte) || type == typeof(char) || type == typeof(short) || 
                type == typeof(ushort) || type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
