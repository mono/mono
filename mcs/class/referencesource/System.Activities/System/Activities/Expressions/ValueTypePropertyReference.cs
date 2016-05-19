//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Activities.Statements;
    using System.Threading;

    public sealed class ValueTypePropertyReference<TOperand, TResult> : CodeActivity<Location<TResult>>
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

        [DefaultValue(null)]
        public InOutArgument<TOperand> OperandLocation
        {
            get;
            set;
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            MethodInfo oldGetMethod = this.getMethod;
            MethodInfo oldSetMethod = this.setMethod;

            if (!typeof(TOperand).IsValueType)
            {
                metadata.AddValidationError(SR.TypeMustbeValueType(typeof(TOperand).Name));
            }

            if (typeof(TOperand).IsEnum)
            {
                metadata.AddValidationError(SR.TargetTypeCannotBeEnum(this.GetType().Name, this.DisplayName));
            }
            else if (String.IsNullOrEmpty(this.PropertyName))
            {
                metadata.AddValidationError(SR.ActivityPropertyMustBeSet("PropertyName", this.DisplayName));
            }
            else
            {
                this.propertyInfo = typeof(TOperand).GetProperty(this.PropertyName);
                if (this.propertyInfo == null)
                {
                    metadata.AddValidationError(SR.MemberNotFound(PropertyName, typeof(TOperand).Name));
                }
            }

            bool isRequired = false;
            if (this.propertyInfo != null)
            {
                this.setMethod = this.propertyInfo.GetSetMethod();
                this.getMethod = this.propertyInfo.GetGetMethod();

                if (setMethod == null)
                {
                    metadata.AddValidationError(SR.MemberIsReadOnly(propertyInfo.Name, typeof(TOperand)));
                }
                if (setMethod != null && !setMethod.IsStatic)
                {
                    isRequired = true;
                }
            }
            MemberExpressionHelper.AddOperandLocationArgument<TOperand>(metadata, this.OperandLocation, isRequired);

            if (this.propertyInfo != null)
            {
                if (MethodCallExpressionHelper.NeedRetrieve(this.getMethod, oldGetMethod, this.getFunc))
                {
                    this.getFunc = MethodCallExpressionHelper.GetFunc(metadata, this.getMethod, funcCache, locker);
                }
                if (MethodCallExpressionHelper.NeedRetrieve(this.setMethod, oldSetMethod, this.setFunc))
                {
                    this.setFunc = MethodCallExpressionHelper.GetFunc(metadata, this.setMethod, funcCache, locker, true);
                }
            }
        }

        protected override Location<TResult> Execute(CodeActivityContext context)
        {
            Location<TOperand> operandLocationValue = this.OperandLocation.GetLocation(context);
            Fx.Assert(operandLocationValue != null, "OperandLocation must not be null");
            Fx.Assert(this.propertyInfo != null, "propertyInfo must not be null");
            return new PropertyLocation(this.propertyInfo, this.getFunc, this.setFunc, operandLocationValue);
        }

        [DataContract]
        internal class PropertyLocation : Location<TResult>
        {
            Location<TOperand> ownerLocation;

            PropertyInfo propertyInfo;

            Func<object, object[], object> getFunc;
            Func<object, object[], object> setFunc;

            public PropertyLocation(PropertyInfo propertyInfo, Func<object, object[], object> getFunc,
                Func<object, object[], object> setFunc, Location<TOperand> ownerLocation)
                : base()
            {
                this.propertyInfo = propertyInfo;
                this.ownerLocation = ownerLocation;

                this.getFunc = getFunc;
                this.setFunc = setFunc;
            }

            public override TResult Value
            {
                get
                {
                    // Only allow access to public properties, EXCEPT that Locations are top-level variables 
                    // from the other's perspective, not internal properties, so they're okay as a special case.
                    // E.g. "[N]" from the user's perspective is not accessing a nonpublic property, even though
                    // at an implementation level it is.
                    if (this.getFunc != null)
                    {
                        return (TResult)this.getFunc(this.ownerLocation.Value, new object[0]);
                    }
                    if (this.propertyInfo.GetGetMethod() == null && TypeHelper.AreTypesCompatible(this.propertyInfo.DeclaringType, typeof(Location)) == false)
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException(SR.WriteonlyPropertyCannotBeRead(this.propertyInfo.DeclaringType, this.propertyInfo.Name)));
                    }

                    return (TResult)this.propertyInfo.GetValue(this.ownerLocation.Value, null);
                }
                set
                {
                    object copy = this.ownerLocation.Value;
                    if (this.getFunc != null)
                    {
                        copy = this.setFunc(copy, new object[] { value });
                    }
                    else
                    {
                        this.propertyInfo.SetValue(copy, value, null);
                    }
                    if (copy != null)
                    {
                        this.ownerLocation.Value = (TOperand)copy;
                    }
                }
            }

            [DataMember(EmitDefaultValue = false, Name = "ownerLocation")]
            internal Location<TOperand> SerializedOwnerLocation
            {
                get { return this.ownerLocation; }
                set { this.ownerLocation = value; }
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
