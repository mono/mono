//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Collections;
    using System.Runtime.Serialization;
    using System.Windows.Markup;
    using System.Threading;

    [ContentProperty("Indices")]
    public sealed class ValueTypeIndexerReference<TOperand, TItem> : CodeActivity<Location<TItem>>
    {
        Collection<InArgument> indices;
        MethodInfo getMethod;
        MethodInfo setMethod;

        Func<object, object[], object> getFunc;
        Func<object, object[], object> setFunc;

        static MruCache<MethodInfo, Func<object, object[], object>> funcCache =
            new MruCache<MethodInfo, Func<object, object[], object>>(MethodCallExpressionHelper.FuncCacheCapacity);
        static ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        [RequiredArgument]
        [DefaultValue(null)]
        public InOutArgument<TOperand> OperandLocation
        {
            get;
            set;
        }

        [RequiredArgument]
        [DefaultValue(null)]
        public Collection<InArgument> Indices
        {
            get
            {
                if (this.indices == null)
                {
                    this.indices = new ValidatingCollection<InArgument>
                    {
                        // disallow null values
                        OnAddValidationCallback = item =>
                        {
                            if (item == null)
                            {
                                throw FxTrace.Exception.ArgumentNull("item");
                            }
                        },
                    };
                }
                return this.indices;
            }
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            MethodInfo oldGetMethod = this.getMethod;
            MethodInfo oldSetMethod = this.setMethod;
            if (!typeof(TOperand).IsValueType)
            {
                metadata.AddValidationError(SR.TypeMustbeValueType(typeof(TOperand).Name));
            }
            if (this.Indices.Count == 0)
            {
                metadata.AddValidationError(SR.IndicesAreNeeded(this.GetType().Name, this.DisplayName));
            }
            else
            {
                IndexerHelper.CacheMethod<TOperand, TItem>(this.Indices, ref this.getMethod, ref this.setMethod);
                if (this.setMethod == null)
                {
                    metadata.AddValidationError(SR.SpecialMethodNotFound("set_Item", typeof(TOperand).Name));
                }
            }

            RuntimeArgument operandArgument = new RuntimeArgument("OperandLocation", typeof(TOperand), ArgumentDirection.InOut, true);
            metadata.Bind(this.OperandLocation, operandArgument);
            metadata.AddArgument(operandArgument);

            IndexerHelper.OnGetArguments<TItem>(this.Indices, this.Result, metadata);

            if (MethodCallExpressionHelper.NeedRetrieve(this.getMethod, oldGetMethod, this.getFunc))
            {
                this.getFunc = MethodCallExpressionHelper.GetFunc(metadata, this.getMethod, funcCache, locker);
            }
            if (MethodCallExpressionHelper.NeedRetrieve(this.setMethod, oldSetMethod, this.setFunc))
            {
                this.setFunc = MethodCallExpressionHelper.GetFunc(metadata, this.setMethod, funcCache, locker, true);
            }
        }

        protected override Location<TItem> Execute(CodeActivityContext context)
        {
            object[] indicesValue = new object[this.Indices.Count];
            for (int i = 0; i < this.Indices.Count; i++)
            {
                indicesValue[i] = this.Indices[i].Get(context);
            }
            Location<TOperand> operandLocationValue = this.OperandLocation.GetLocation(context);
            Fx.Assert(operandLocationValue != null, "OperandLocation must not be null");
            return new IndexerLocation(operandLocationValue, indicesValue, getMethod, setMethod, this.getFunc, this.setFunc);
        }

        [DataContract]
        internal class IndexerLocation : Location<TItem>
        {
            Location<TOperand> operandLocation;

            object[] indices;

            object[] parameters;

            MethodInfo getMethod;

            MethodInfo setMethod;

            Func<object, object[], object> getFunc;
            Func<object, object[], object> setFunc;

            public IndexerLocation(Location<TOperand> operandLocation, object[] indices, MethodInfo getMethod, MethodInfo setMethod, 
                Func<object, object[], object> getFunc, Func<object, object[], object> setFunc)
                : base()
            {
                this.operandLocation = operandLocation;
                this.indices = indices;
                this.getMethod = getMethod;
                this.setMethod = setMethod;
                this.setFunc = setFunc;
                this.getFunc = getFunc;
            }

            public override TItem Value
            {
                get
                {
                    Fx.Assert(this.operandLocation != null, "operandLocation must not be null");
                    Fx.Assert(this.indices != null, "indices must not be null");
                    if (this.getFunc != null)
                    {
                        return (TItem)this.getFunc(this.operandLocation.Value, indices);
                    }
                    else if (this.getMethod != null)
                    {
                        return (TItem)this.getMethod.Invoke(this.operandLocation.Value, indices);    
                    }
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.SpecialMethodNotFound("get_Item", typeof(TOperand).Name)));
                }
                set
                {
                    Fx.Assert(this.setMethod != null, "setMethod must not be null");
                    Fx.Assert(this.operandLocation != null, "operandLocation must not be null");
                    Fx.Assert(this.indices != null, "indices must not be null");

                    if (this.parameters == null)
                    {
                        this.parameters = new object[this.indices.Length + 1];
                        for (int i = 0; i < this.indices.Length; i++)
                        {
                            parameters[i] = this.indices[i];
                        }
                        parameters[parameters.Length - 1] = value;
                    }
                    object copy = this.operandLocation.Value;
                    if (this.setFunc != null)
                    {
                        copy = this.setFunc(copy, parameters);
                    }
                    else
                    {
                        this.setMethod.Invoke(copy, parameters);
                    }
                    if (copy != null)
                    {
                        this.operandLocation.Value = (TOperand)copy;
                    }
                }
            }

            [DataMember(EmitDefaultValue = false, Name = "operandLocation")]
            internal Location<TOperand> SerializedOperandLocation
            {
                get { return this.operandLocation; }
                set { this.operandLocation = value; }
            }

            [DataMember(EmitDefaultValue = false, Name = "indices")]
            internal object[] SerializedIndices
            {
                get { return this.indices; }
                set { this.indices = value; }
            }

            [DataMember(EmitDefaultValue = false, Name = "parameters")]
            internal object[] SerializedParameters
            {
                get { return this.parameters; }
                set { this.parameters = value; }
            }

            [DataMember(EmitDefaultValue = false, Name = "getMethod")]
            internal MethodInfo SerializedGetMethod
            {
                get { return this.getMethod; }
                set { this.getMethod = value; }
            }

            [DataMember(EmitDefaultValue = false, Name = "setMethod")]
            internal MethodInfo SerializedSetMethod
            {
                get { return this.setMethod; }
                set { this.setMethod = value; }
            }
        }
    }
}
