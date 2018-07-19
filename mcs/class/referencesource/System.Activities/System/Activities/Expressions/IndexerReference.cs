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
    public sealed class IndexerReference<TOperand, TItem> : CodeActivity<Location<TItem>>
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
        public InArgument<TOperand> Operand
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

            if (typeof(TOperand).IsValueType)
            {
                metadata.AddValidationError(SR.TargetTypeIsValueType(this.GetType().Name, this.DisplayName));
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
           
            RuntimeArgument operandArgument = new RuntimeArgument("Operand", typeof(TOperand), ArgumentDirection.In, true);
            metadata.Bind(this.Operand, operandArgument);
            metadata.AddArgument(operandArgument);
            
            IndexerHelper.OnGetArguments<TItem>(this.Indices, this.Result, metadata);
             if (MethodCallExpressionHelper.NeedRetrieve(this.getMethod, oldGetMethod, this.getFunc))
            {
                this.getFunc = MethodCallExpressionHelper.GetFunc(metadata, this.getMethod, funcCache, locker);
            }
            if (MethodCallExpressionHelper.NeedRetrieve(this.setMethod, oldSetMethod, this.setFunc))
            {
                this.setFunc = MethodCallExpressionHelper.GetFunc(metadata, this.setMethod, funcCache, locker);
            }
        }

        protected override Location<TItem> Execute(CodeActivityContext context)
        {
            object[] indicesValue = new object[this.Indices.Count];

            for (int i = 0; i < this.Indices.Count; i++)
            {
                indicesValue[i] = this.Indices[i].Get(context);
            }

            TOperand operandValue = this.Operand.Get(context);
            if (operandValue == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.MemberCannotBeNull("Operand", this.GetType().Name, this.DisplayName)));
            }

            return new IndexerLocation(operandValue, indicesValue, this.getMethod, this.setMethod, this.getFunc, this.setFunc);
        }

        
        [DataContract]
        internal class IndexerLocation : Location<TItem>
        {
            TOperand operand;

            object[] indices;

            object[] parameters;

            MethodInfo getMethod;

            MethodInfo setMethod;

            Func<object, object[], object> getFunc;
            Func<object, object[], object> setFunc;

            public IndexerLocation(TOperand operand, object[] indices, MethodInfo getMethod, MethodInfo setMethod, 
                Func<object, object[], object> getFunc, Func<object, object[], object> setFunc)
                : base()
            {
                this.operand = operand;
                this.indices = indices;
                this.getMethod = getMethod;
                this.setMethod = setMethod;
                this.getFunc = getFunc;
                this.setFunc = setFunc;
            }

            public override TItem Value
            {
                get
                {
                    Fx.Assert(this.operand != null, "operand must not be null");
                    Fx.Assert(this.indices != null, "indices must not be null");
                    if (this.getFunc != null)
                    {
                        return (TItem)this.getFunc(this.operand, indices);
                    }
                    else if (this.getMethod != null)
                    {
                        return (TItem)this.getMethod.Invoke(this.operand, indices);
                    }
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.SpecialMethodNotFound("get_Item", typeof(TOperand).Name)));
                }
                set
                {
                    Fx.Assert(this.setMethod != null, "setMethod must not be null");
                    Fx.Assert(this.operand != null, "operand must not be null");
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
                    if (this.setFunc != null)
                    {
                        this.setFunc(operand, parameters);
                    }
                    else
                    {
                        this.setMethod.Invoke(operand, parameters);
                    }
                }
            }

            [DataMember(EmitDefaultValue = false, Name = "operand")]
            internal TOperand SerializedOperand
            {
                get { return this.operand; }
                set { this.operand = value; }
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
