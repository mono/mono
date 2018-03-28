//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Threading;

    public sealed class PropertyReference<TOperand, TResult> : CodeActivity<Location<TResult>>
    {
        PropertyInfo propertyInfo;
        Func<object, object[], object> getFunc;
        Func<object, object[], object> setFunc;
        MethodInfo getMethod;
        MethodInfo setMethod;

        static MruCache<MethodInfo, Func<object, object[], object>> funcCache =
            new MruCache<MethodInfo, Func<object, object[], object>>(MethodCallExpressionHelper.FuncCacheCapacity);
        static ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        [DefaultValue(null)]
        public string PropertyName
        {
            get;
            set;
        }

        public InArgument<TOperand> Operand
        {
            get;
            set;
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            MethodInfo oldGetMethod = this.getMethod;
            MethodInfo oldSetMethod = this.setMethod;

            bool isRequired = false;
            if (typeof(TOperand).IsEnum)
            {
                metadata.AddValidationError(SR.TargetTypeCannotBeEnum(this.GetType().Name, this.DisplayName));
            }
            else if (typeof(TOperand).IsValueType)
            {
                metadata.AddValidationError(SR.TargetTypeIsValueType(this.GetType().Name, this.DisplayName));
            }

            if (string.IsNullOrEmpty(this.PropertyName))
            {
                metadata.AddValidationError(SR.ActivityPropertyMustBeSet("PropertyName", this.DisplayName));
            }
            else
            {
                Type operandType = typeof(TOperand);
                this.propertyInfo = operandType.GetProperty(this.PropertyName);

                if (this.propertyInfo == null)
                {
                    metadata.AddValidationError(SR.MemberNotFound(PropertyName, typeof(TOperand).Name));
                }
                else
                {
                    getMethod = this.propertyInfo.GetGetMethod();
                    setMethod = this.propertyInfo.GetSetMethod();

                    // Only allow access to public properties, EXCEPT that Locations are top-level variables 
                    // from the other's perspective, not internal properties, so they're okay as a special case.
                    // E.g. "[N]" from the user's perspective is not accessing a nonpublic property, even though
                    // at an implementation level it is.
                    if (setMethod == null && TypeHelper.AreTypesCompatible(this.propertyInfo.DeclaringType, typeof(Location)) == false)
                    {
                        metadata.AddValidationError(SR.ReadonlyPropertyCannotBeSet(this.propertyInfo.DeclaringType, this.propertyInfo.Name));
                    }

                    if ((getMethod != null && !getMethod.IsStatic) || (setMethod != null && !setMethod.IsStatic))
                    {
                        isRequired = true;
                    }
                }
            }
            MemberExpressionHelper.AddOperandArgument(metadata, this.Operand, isRequired);
            if (propertyInfo != null)
            {
                if (MethodCallExpressionHelper.NeedRetrieve(this.getMethod, oldGetMethod, this.getFunc))
                {
                    this.getFunc = MethodCallExpressionHelper.GetFunc(metadata, this.getMethod, funcCache, locker);
                }
                if (MethodCallExpressionHelper.NeedRetrieve(this.setMethod, oldSetMethod, this.setFunc))
                {
                    this.setFunc = MethodCallExpressionHelper.GetFunc(metadata, this.setMethod, funcCache, locker);
                }
            }
        }
        protected override Location<TResult> Execute(CodeActivityContext context)
        {
            Fx.Assert(this.propertyInfo != null, "propertyInfo must not be null");
            return new PropertyLocation<TResult>(this.propertyInfo, this.getFunc, this.setFunc, this.Operand.Get(context));
        }

        [DataContract]
        internal class PropertyLocation<T> : Location<T>
        {
            object owner;

            PropertyInfo propertyInfo;

            Func<object, object[], object> getFunc;
            Func<object, object[], object> setFunc;

            public PropertyLocation(PropertyInfo propertyInfo, Func<object, object[], object> getFunc,
                Func<object, object[], object> setFunc, object owner)
                : base()
            {
                this.propertyInfo = propertyInfo;
                this.owner = owner;
                this.getFunc = getFunc;
                this.setFunc = setFunc;
            }

            public override T Value
            {
                get
                {                    
                    // Only allow access to public properties, EXCEPT that Locations are top-level variables 
                    // from the other's perspective, not internal properties, so they're okay as a special case.
                    // E.g. "[N]" from the user's perspective is not accessing a nonpublic property, even though
                    // at an implementation level it is.
                    if (this.getFunc != null)
                    {
                        if (!this.propertyInfo.GetGetMethod().IsStatic && this.owner == null)
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(SR.NullReferencedMemberAccess(this.propertyInfo.DeclaringType.Name, this.propertyInfo.Name)));
                        }

                        return (T)this.getFunc(this.owner, new object[0]);
                    }
                    if (this.propertyInfo.GetGetMethod() == null && TypeHelper.AreTypesCompatible(this.propertyInfo.DeclaringType, typeof(Location)) == false)
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(SR.WriteonlyPropertyCannotBeRead(this.propertyInfo.DeclaringType, this.propertyInfo.Name)));
                    }

                    return (T)this.propertyInfo.GetValue(this.owner, null);
                }
                set
                {
                    if (this.setFunc != null)
                    {
                        if (!this.propertyInfo.GetSetMethod().IsStatic && this.owner == null)
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(SR.NullReferencedMemberAccess(this.propertyInfo.DeclaringType.Name, this.propertyInfo.Name)));
                        }

                        this.setFunc(this.owner, new object[] { value });
                    }
                    else
                    {
                        this.propertyInfo.SetValue(this.owner, value, null);
                    }
                }
            }

            [DataMember(EmitDefaultValue = false, Name = "owner")]
            internal object SerializedOwner
            {
                get { return this.owner; }
                set { this.owner = value; }
            }

            [DataMember(Name = "propertyInfo")]
            internal PropertyInfo SerializedPropertyInfo
            {
                get { return this.propertyInfo; }
                set { this.propertyInfo = value; }
            }
        }
    }
}
