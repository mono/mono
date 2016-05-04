//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Runtime;

    enum ValueDataType : byte
    {
        None = 0,
        Boolean,
        Double,
        StackFrame,
        Sequence,
        String
    }

    // Value is like Variant. Since a Value is only temporary storage, we use memory to avoid typecasting type
    // casting, which in C# is expensive. Value contains storage for every possible XPath data type.
    // We avoid data copying by being smart about how we pass Value around - values are either accessed via
    // method calls, or the value object itself is passed by ref
    //
    // The filter engine never deals with a single value per se. We only work with Sets of Values. A single value
    // is a ValueSet of size 1
    //
    internal struct Value
    {
        bool boolVal;
        double dblVal;
        StackFrame frame;
        NodeSequence sequence;
        string strVal;
        ValueDataType type;

        internal bool Boolean
        {
            get
            {
                return this.boolVal;
            }
            set
            {
                this.type = ValueDataType.Boolean;
                this.boolVal = value;
            }
        }

        internal double Double
        {
            get
            {
                return this.dblVal;
            }
            set
            {
                this.type = ValueDataType.Double;
                this.dblVal = value;
            }
        }

        internal StackFrame Frame
        {
            get
            {
                return this.frame;
            }
#if NO
            set
            {
                this.type = ValueDataType.StackFrame;
                this.frame = value;
            }
#endif
        }
#if NO 
        internal int FrameCount
        {
            get
            {
                return this.frame.Count;
            }
        }
#endif
        internal int FrameEndPtr
        {
#if NO
            get
            {
                return this.frame.endPtr;
            }
#endif
            set
            {
                Fx.Assert(this.IsType(ValueDataType.StackFrame), "");
                this.frame.EndPtr = value;
            }
        }
#if NO
        internal int FrameBasePtr
        {
            get
            {
                return this.frame.basePtr;
            }            
        }
        
        internal int StackPtr
        {
            get
            {
                return this.frame.basePtr - 1;
            }
        }
#endif
        internal int NodeCount
        {
            get
            {
                return this.sequence.Count;
            }
        }

        internal NodeSequence Sequence
        {
            get
            {
                return this.sequence;
            }
            set
            {
                this.type = ValueDataType.Sequence;
                this.sequence = value;
            }
        }

        internal string String
        {
            get
            {
                return this.strVal;
            }
            set
            {
                this.type = ValueDataType.String;
                this.strVal = value;
            }
        }

        internal ValueDataType Type
        {
            get
            {
                return this.type;
            }
        }

        internal void Add(double val)
        {
            Fx.Assert(ValueDataType.Double == this.type, "");
            this.dblVal += val;
        }

#if NO
        internal void Clear()
        {
            this.type = ValueDataType.None;
            this.sequence = null;
        }
#endif
        internal void Clear(ProcessingContext context)
        {
            if (ValueDataType.Sequence == this.type)
            {
                this.ReleaseSequence(context);
            }
            this.type = ValueDataType.None;
        }

        // Fully general compare
        internal bool CompareTo(ref Value val, RelationOperator op)
        {
            switch (this.type)
            {
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));

                case ValueDataType.Boolean:
                    switch (val.type)
                    {
                        default:
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));
                        case ValueDataType.Boolean:
                            return QueryValueModel.Compare(this.boolVal, val.boolVal, op);
                        case ValueDataType.Double:
                            return QueryValueModel.Compare(this.boolVal, val.dblVal, op);
                        case ValueDataType.Sequence:
                            return QueryValueModel.Compare(this.boolVal, val.sequence, op);
                        case ValueDataType.String:
                            return QueryValueModel.Compare(this.boolVal, val.strVal, op);
                    }

                case ValueDataType.Double:
                    switch (val.type)
                    {
                        default:
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));
                        case ValueDataType.Boolean:
                            return QueryValueModel.Compare(this.dblVal, val.boolVal, op);
                        case ValueDataType.Double:
                            return QueryValueModel.Compare(this.dblVal, val.dblVal, op);
                        case ValueDataType.Sequence:
                            return QueryValueModel.Compare(this.dblVal, val.sequence, op);
                        case ValueDataType.String:
                            return QueryValueModel.Compare(this.dblVal, val.strVal, op);
                    }

                case ValueDataType.Sequence:
                    switch (val.type)
                    {
                        default:
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));
                        case ValueDataType.Boolean:
                            return QueryValueModel.Compare(this.sequence, val.boolVal, op);
                        case ValueDataType.Double:
                            return QueryValueModel.Compare(this.sequence, val.dblVal, op);
                        case ValueDataType.Sequence:
                            return QueryValueModel.Compare(this.sequence, val.sequence, op);
                        case ValueDataType.String:
                            return QueryValueModel.Compare(this.sequence, val.strVal, op);
                    }

                case ValueDataType.String:
                    switch (val.type)
                    {
                        default:
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));
                        case ValueDataType.Boolean:
                            return QueryValueModel.Compare(this.strVal, val.boolVal, op);
                        case ValueDataType.Double:
                            return QueryValueModel.Compare(this.strVal, val.dblVal, op);
                        case ValueDataType.Sequence:
                            return QueryValueModel.Compare(this.strVal, val.sequence, op);
                        case ValueDataType.String:
                            return QueryValueModel.Compare(this.strVal, val.strVal, op);
                    }
            }
        }

        internal bool CompareTo(double val, RelationOperator op)
        {
            switch (this.type)
            {
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));

                case ValueDataType.Boolean:
                    return QueryValueModel.Compare(this.boolVal, val, op);

                case ValueDataType.Double:
                    return QueryValueModel.Compare(this.dblVal, val, op);

                case ValueDataType.Sequence:
                    return QueryValueModel.Compare(this.sequence, val, op);

                case ValueDataType.String:
                    return QueryValueModel.Compare(this.strVal, val, op);
            }
        }

        internal void ConvertTo(ProcessingContext context, ValueDataType newType)
        {
            Fx.Assert(null != context, "");

            if (newType == this.type)
            {
                return;
            }

            switch (newType)
            {
                default:
                    break;

                case ValueDataType.Boolean:
                    this.boolVal = this.ToBoolean();
                    break;

                case ValueDataType.Double:
                    this.dblVal = this.ToDouble();
                    break;

                case ValueDataType.String:
                    this.strVal = this.ToString();
                    break;
            }

            if (ValueDataType.Sequence == this.type)
            {
                this.ReleaseSequence(context);
            }
            this.type = newType;
        }

        internal bool Equals(string val)
        {
            switch (this.type)
            {
                default:
                    Fx.Assert("Invalid Type");
                    return false;

                case ValueDataType.Boolean:
                    return QueryValueModel.Equals(this.boolVal, val);

                case ValueDataType.Double:
                    return QueryValueModel.Equals(this.dblVal, val);

                case ValueDataType.Sequence:
                    return QueryValueModel.Equals(this.sequence, val);

                case ValueDataType.String:
                    return QueryValueModel.Equals(this.strVal, val);
            }
        }

        internal bool Equals(double val)
        {
            switch (this.type)
            {
                default:
                    Fx.Assert("Invalid Type");
                    return false;

                case ValueDataType.Boolean:
                    return QueryValueModel.Equals(this.boolVal, val);

                case ValueDataType.Double:
                    return QueryValueModel.Equals(this.dblVal, val);

                case ValueDataType.Sequence:
                    return QueryValueModel.Equals(this.sequence, val);

                case ValueDataType.String:
                    return QueryValueModel.Equals(val, this.strVal);
            }
        }
#if NO
        internal bool Equals(bool val)
        {
            switch (this.type)
            {
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryProcessingException(QueryProcessingError.TypeMismatch), TraceEventType.Critical);

                case ValueDataType.Boolean:
                    return QueryValueModel.Equals(this.boolVal, val);

                case ValueDataType.Double:
                    return QueryValueModel.Equals(this.dblVal, val);

                case ValueDataType.Sequence:
                    return QueryValueModel.Equals(this.sequence, val);

                case ValueDataType.String:
                    return QueryValueModel.Equals(this.strVal, val);
            }
        }
#endif
        internal bool GetBoolean()
        {
            if (ValueDataType.Boolean != this.type)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));
            }

            return this.boolVal;
        }

        internal double GetDouble()
        {
            if (ValueDataType.Double != this.type)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));
            }

            return this.dblVal;
        }

        internal NodeSequence GetSequence()
        {
            if (ValueDataType.Sequence != this.type)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));
            }

            return this.sequence;
        }

        internal string GetString()
        {
            if (ValueDataType.String != this.type)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));
            }

            return this.strVal;
        }

        internal bool IsType(ValueDataType type)
        {
            return (type == this.type);
        }

        internal void Multiply(double val)
        {
            Fx.Assert(ValueDataType.Double == this.type, "");
            this.dblVal *= val;
        }

        internal void Negate()
        {
            Fx.Assert(this.type == ValueDataType.Double, "");
            this.dblVal = -this.dblVal;
        }

        internal void Not()
        {
            Fx.Assert(this.type == ValueDataType.Boolean, "");
            this.boolVal = !this.boolVal;
        }

        internal void ReleaseSequence(ProcessingContext context)
        {
            Fx.Assert(null != context && this.type == ValueDataType.Sequence && null != this.sequence, "");

            context.ReleaseSequence(this.sequence);
            this.sequence = null;
        }

        internal void StartFrame(int start)
        {
            this.type = ValueDataType.StackFrame;
            this.frame.basePtr = start + 1;
            this.frame.endPtr = start;
        }

#if NO
        internal void Subtract(double dblVal)
        {
            Fx.Assert(ValueDataType.Double == this.type, "");
            this.dblVal -= dblVal;
        }
#endif
        internal bool ToBoolean()
        {
            switch (this.type)
            {
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));

                case ValueDataType.Boolean:
                    return this.boolVal;

                case ValueDataType.Double:
                    return QueryValueModel.Boolean(this.dblVal);

                case ValueDataType.Sequence:
                    return QueryValueModel.Boolean(this.sequence);

                case ValueDataType.String:
                    return QueryValueModel.Boolean(this.strVal);

            }
        }

        internal double ToDouble()
        {
            switch (this.type)
            {
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));

                case ValueDataType.Boolean:
                    return QueryValueModel.Double(this.boolVal);

                case ValueDataType.Double:
                    return this.dblVal;

                case ValueDataType.Sequence:
                    return QueryValueModel.Double(this.sequence);

                case ValueDataType.String:
                    return QueryValueModel.Double(this.strVal);

            }
        }

        public override string ToString()
        {
            switch (this.type)
            {
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.TypeMismatch));

                case ValueDataType.Boolean:
                    return QueryValueModel.String(this.boolVal);

                case ValueDataType.Double:
                    return QueryValueModel.String(this.dblVal);

                case ValueDataType.Sequence:
                    return QueryValueModel.String(this.sequence);

                case ValueDataType.String:
                    return this.strVal;
            }
        }

        internal void Update(ProcessingContext context, bool val)
        {
            if (ValueDataType.Sequence == this.type)
            {
                context.ReleaseSequence(this.sequence);
            }
            this.Boolean = val;
        }

        internal void Update(ProcessingContext context, double val)
        {
            if (ValueDataType.Sequence == this.type)
            {
                context.ReleaseSequence(this.sequence);
            }
            this.Double = val;
        }

        internal void Update(ProcessingContext context, string val)
        {
            if (ValueDataType.Sequence == this.type)
            {
                context.ReleaseSequence(this.sequence);
            }
            this.String = val;
        }

        internal void Update(ProcessingContext context, NodeSequence val)
        {
            if (ValueDataType.Sequence == this.type)
            {
                context.ReleaseSequence(this.sequence);
            }
            this.Sequence = val;
        }

    }
}
