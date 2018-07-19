// ---------------------------------------------------------------------------
// Copyright (C) 2006 Microsoft Corporation All Rights Reserved
// ---------------------------------------------------------------------------

#define CODE_ANALYSIS
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.Activities.Common;

namespace System.Workflow.Activities.Rules
{
    #region ExpressionInfo

    // Public base class (which just holds the Type of the expression).
    public class RuleExpressionInfo
    {
        private Type expressionType;

        public RuleExpressionInfo(Type expressionType)
        {
            this.expressionType = expressionType;
        }

        public Type ExpressionType
        {
            get { return expressionType; }
        }
    }

    // Internal derivation for CodeMethodInvokeExpression
    internal class RuleMethodInvokeExpressionInfo : RuleExpressionInfo
    {
        private MethodInfo methodInfo;
        private bool needsParamsExpansion;

        internal RuleMethodInvokeExpressionInfo(MethodInfo mi, bool needsParamsExpansion)
            : base(mi.ReturnType)
        {
            this.methodInfo = mi;
            this.needsParamsExpansion = needsParamsExpansion;
        }

        internal MethodInfo MethodInfo
        {
            get { return methodInfo; }
        }

        internal bool NeedsParamsExpansion
        {
            get { return needsParamsExpansion; }
        }
    }

    // Internal derivation for CodeBinaryExpression
    internal class RuleBinaryExpressionInfo : RuleExpressionInfo
    {
        private Type leftType;
        private Type rightType;
        private MethodInfo methodInfo;

        // no overridden method needed
        internal RuleBinaryExpressionInfo(Type lhsType, Type rhsType, Type resultType)
            : base(resultType)
        {
            this.leftType = lhsType;
            this.rightType = rhsType;
        }

        // overridden method found
        internal RuleBinaryExpressionInfo(Type lhsType, Type rhsType, MethodInfo mi)
            : base(mi.ReturnType)
        {
            this.leftType = lhsType;
            this.rightType = rhsType;
            this.methodInfo = mi;
        }

        internal Type LeftType
        {
            get { return leftType; }
        }

        internal Type RightType
        {
            get { return rightType; }
        }

        internal MethodInfo MethodInfo
        {
            get { return methodInfo; }
        }
    }

    // Internal derivation for CodeFieldReferenceExpression
    internal class RuleFieldExpressionInfo : RuleExpressionInfo
    {
        private FieldInfo fieldInfo;

        internal RuleFieldExpressionInfo(FieldInfo fi)
            : base(fi.FieldType)
        {
            fieldInfo = fi;
        }

        internal FieldInfo FieldInfo
        {
            get { return fieldInfo; }
        }
    }

    // Internal derivation for CodePropertyReferenceExpression
    internal class RulePropertyExpressionInfo : RuleExpressionInfo
    {
        private PropertyInfo propertyInfo;
        private bool needsParamsExpansion;

        // Note that the type pi.PropertyType may differ from the "exprType" argument if this
        // property is a Bind.
        internal RulePropertyExpressionInfo(PropertyInfo pi, Type exprType, bool needsParamsExpansion)
            : base(exprType)
        {
            this.propertyInfo = pi;
            this.needsParamsExpansion = needsParamsExpansion;
        }

        internal PropertyInfo PropertyInfo
        {
            get { return propertyInfo; }
        }

        internal bool NeedsParamsExpansion
        {
            get { return needsParamsExpansion; }
        }
    }

    // Internal derivation for CodeMethodInvokeExpression
    internal class RuleConstructorExpressionInfo : RuleExpressionInfo
    {
        private ConstructorInfo constructorInfo;
        private bool needsParamsExpansion;

        internal RuleConstructorExpressionInfo(ConstructorInfo ci, bool needsParamsExpansion)
            : base(ci.DeclaringType)
        {
            this.constructorInfo = ci;
            this.needsParamsExpansion = needsParamsExpansion;
        }

        internal ConstructorInfo ConstructorInfo
        {
            get { return constructorInfo; }
        }

        internal bool NeedsParamsExpansion
        {
            get { return needsParamsExpansion; }
        }
    }

    internal class ExtensionMethodInfo : MethodInfo
    {
        MethodInfo actualMethod;
        int actualParameterLength;
        ParameterInfo[] expectedParameters;
        Type assumedDeclaringType;
        bool hasOutOrRefParameters = false;

        public ExtensionMethodInfo(MethodInfo method, ParameterInfo[] actualParameters)
            : base()
        {
            Debug.Assert(method.IsStatic, "Expected static method as an extension method");

            actualMethod = method;
            // modify parameters
            actualParameterLength = actualParameters.Length;
            if (actualParameterLength < 2)
                expectedParameters = new ParameterInfo[0];
            else
            {
                expectedParameters = new ParameterInfo[actualParameterLength - 1];
                Array.Copy(actualParameters, 1, expectedParameters, 0, actualParameterLength - 1);
                foreach (ParameterInfo pi in expectedParameters)
                {
                    if (pi.ParameterType.IsByRef)
                        hasOutOrRefParameters = true;
                }
            }
            // get the type we pretend this method is on (which happens to be the first actual parameter)
            assumedDeclaringType = actualParameters[0].ParameterType;
        }

        public override MethodInfo GetBaseDefinition()
        {
            return actualMethod.GetBaseDefinition();
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes
        {
            get { return actualMethod.ReturnTypeCustomAttributes; }
        }

        public override MethodAttributes Attributes
        {
            get { return actualMethod.Attributes & ~MethodAttributes.Static; }
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return actualMethod.GetMethodImplementationFlags();
        }

        public override ParameterInfo[] GetParameters()
        {
            return expectedParameters;
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            object[] actualParameters = new object[actualParameterLength];
            if (actualParameterLength > 1)
                Array.Copy(parameters, 0, actualParameters, 1, actualParameterLength - 1);
            if (obj == null)
                actualParameters[0] = null;
            else
                actualParameters[0] = Executor.AdjustType(obj.GetType(), obj, assumedDeclaringType);
            object result = actualMethod.Invoke(null, invokeAttr, binder, actualParameters, culture);
            // may be out/ref parameters, so copy back the results
            if (hasOutOrRefParameters)
                Array.Copy(actualParameters, 1, parameters, 0, actualParameterLength - 1);
            return result;
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get { return actualMethod.MethodHandle; }
        }

        public override Type DeclaringType
        {
            get { return actualMethod.DeclaringType; }
        }

        public Type AssumedDeclaringType
        {
            get { return assumedDeclaringType; }
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return actualMethod.GetCustomAttributes(attributeType, inherit);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return actualMethod.GetCustomAttributes(inherit);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return actualMethod.IsDefined(attributeType, inherit);
        }

        public override string Name
        {
            get { return actualMethod.Name; }
        }

        public override Type ReflectedType
        {
            get { return actualMethod.ReflectedType; }
        }

        public override Type ReturnType
        {
            get { return actualMethod.ReturnType; }
        }
    }

    internal class SimpleParameterInfo : ParameterInfo
    {
        // only thing we look at is ParameterType, so no need to override anything else
        Type parameterType;

        public SimpleParameterInfo(ParameterInfo parameter)
            : base()
        {
            parameterType = typeof(Nullable<>).MakeGenericType(parameter.ParameterType);
        }

        public SimpleParameterInfo(Type parameter)
            : base()
        {
            parameterType = parameter;
        }

        public override Type ParameterType
        {
            get
            {
                return parameterType;
            }
        }
    }

    internal abstract class BaseMethodInfo : MethodInfo
    {
        protected MethodInfo actualMethod;
        protected ParameterInfo[] expectedParameters;
        protected Type resultType;

        public BaseMethodInfo(MethodInfo method)
            : base()
        {
            Debug.Assert(method.IsStatic, "Expected static method as an lifted method");
            actualMethod = method;
            resultType = method.ReturnType;
            expectedParameters = method.GetParameters();
        }

        public override MethodInfo GetBaseDefinition()
        {
            return actualMethod.GetBaseDefinition();
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes
        {
            get { return actualMethod.ReturnTypeCustomAttributes; }
        }

        public override MethodAttributes Attributes
        {
            get { return actualMethod.Attributes & ~MethodAttributes.Static; }
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return actualMethod.GetMethodImplementationFlags();
        }

        public override ParameterInfo[] GetParameters()
        {
            return expectedParameters;
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get { return actualMethod.MethodHandle; }
        }

        public override Type DeclaringType
        {
            get { return actualMethod.DeclaringType; }
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return actualMethod.GetCustomAttributes(attributeType, inherit);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return actualMethod.GetCustomAttributes(inherit);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return actualMethod.IsDefined(attributeType, inherit);
        }

        public override string Name
        {
            get { return actualMethod.Name; }
        }

        public override Type ReflectedType
        {
            get { return actualMethod.ReflectedType; }
        }

        public override Type ReturnType
        {
            get { return resultType; }
        }

        public override bool Equals(object obj)
        {
            BaseMethodInfo other = obj as BaseMethodInfo;
            if ((other == null)
                || (actualMethod != other.actualMethod)
                || (resultType != other.resultType)
                || (expectedParameters.Length != other.expectedParameters.Length))
                return false;
            for (int i = 0; i < expectedParameters.Length; ++i)
                if (expectedParameters[i].ParameterType != other.expectedParameters[i].ParameterType)
                    return false;
            return true;
        }

        public override int GetHashCode()
        {
            int result = actualMethod.GetHashCode() ^ resultType.GetHashCode();
            for (int i = 0; i < expectedParameters.Length; ++i)
                result ^= expectedParameters[i].GetHashCode();
            return result;
        }
    }

    internal class LiftedConversionMethodInfo : BaseMethodInfo
    {
        public LiftedConversionMethodInfo(MethodInfo method)
            : base(method)
        {
            Debug.Assert(expectedParameters.Length == 1, "not 1 parameters");

            // modify result
            resultType = typeof(Nullable<>).MakeGenericType(method.ReturnType);

            // modify parameter (exactly 1)
            ParameterInfo[] actualParameters = method.GetParameters();
            expectedParameters = new ParameterInfo[1];
            expectedParameters[0] = new SimpleParameterInfo(actualParameters[0]);
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            // null in, then result is null
            if (parameters[0] == null)
                return Activator.CreateInstance(resultType);

            // invoke the conversion from S -> T
            object result = actualMethod.Invoke(null, invokeAttr, binder, parameters, culture);
            // return a T?
            return Executor.AdjustType(actualMethod.ReturnType, result, resultType);
        }
    }

    internal class LiftedArithmeticOperatorMethodInfo : BaseMethodInfo
    {
        public LiftedArithmeticOperatorMethodInfo(MethodInfo method)
            : base(method)
        {
            Debug.Assert(expectedParameters.Length == 2, "not 2 parameters");

            // modify parameters (exactly 2, both need to be lifted)
            ParameterInfo[] actualParameters = method.GetParameters();
            expectedParameters = new ParameterInfo[2];
            expectedParameters[0] = new SimpleParameterInfo(actualParameters[0]);
            expectedParameters[1] = new SimpleParameterInfo(actualParameters[1]);

            // modify result
            resultType = typeof(Nullable<>).MakeGenericType(method.ReturnType);
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            // null in, then result is null
            if (parameters[0] == null)
                return null;
            if (parameters[1] == null)
                return null;

            // apply the underlying operator
            object result = actualMethod.Invoke(null, invokeAttr, binder, parameters, culture);
            // return a T?
            return Executor.AdjustType(actualMethod.ReturnType, result, resultType);
        }
    }

    internal class LiftedEqualityOperatorMethodInfo : BaseMethodInfo
    {
        public LiftedEqualityOperatorMethodInfo(MethodInfo method)
            : base(method)
        {
            Debug.Assert(method.ReturnType == typeof(bool), "not a bool result");
            Debug.Assert(expectedParameters.Length == 2, "not 2 parameters");

            // modify parameters (exactly 2, both need to be lifted)
            ParameterInfo[] actualParameters = method.GetParameters();
            expectedParameters = new ParameterInfo[2];
            expectedParameters[0] = new SimpleParameterInfo(actualParameters[0]);
            expectedParameters[1] = new SimpleParameterInfo(actualParameters[1]);

            // set the result type
            resultType = typeof(bool);
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            // null == null is true, null == something else is false, else call method
            if (parameters[0] == null)
                return (parameters[1] == null);
            else if (parameters[1] == null)
                return false;

            // invoke the actual comparison (parameters are unwrapped)
            return actualMethod.Invoke(null, invokeAttr, binder, parameters, culture);
        }
    }

    internal class LiftedRelationalOperatorMethodInfo : BaseMethodInfo
    {
        public LiftedRelationalOperatorMethodInfo(MethodInfo method)
            : base(method)
        {
            Debug.Assert(method.ReturnType == typeof(bool), "not a bool result");
            Debug.Assert(expectedParameters.Length == 2, "not 2 parameters");

            // modify parameters (exactly 2, both need to be lifted)
            ParameterInfo[] actualParameters = method.GetParameters();
            expectedParameters = new ParameterInfo[2];
            expectedParameters[0] = new SimpleParameterInfo(actualParameters[0]);
            expectedParameters[1] = new SimpleParameterInfo(actualParameters[1]);

            // set the result type
            resultType = typeof(bool);
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            // if either parameter is null, then result is false
            if (parameters[0] == null)
                return false;
            if (parameters[1] == null)
                return false;

            // invoke the actual comparison (parameters are unwrapped)
            return actualMethod.Invoke(null, invokeAttr, binder, parameters, culture);
        }
    }

    internal class EnumOperationMethodInfo : MethodInfo
    {
        CodeBinaryOperatorType op;
        ParameterInfo[] expectedParameters;
        Type resultType;        // may be nullable, enum, or value type
        bool resultIsNullable;  // true if resultType is nullable

        Type lhsBaseType;       // non-Nullable, may be enum
        Type rhsBaseType;
        Type resultBaseType;

        Type lhsRootType;       // underlying type (int, long, ushort, etc)
        Type rhsRootType;
        Type resultRootType;

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public EnumOperationMethodInfo(Type lhs, CodeBinaryOperatorType operation, Type rhs, bool isZero)
        {
            // only 5 arithmetic cases (U = underlying type of E):
            //    E = E + U
            //    E = U + E
            //    U = E - E
            //    E = E - U
            //    E = U - E
            // plus 5 comparison cases
            //    E == E
            //    E < E
            //    E <= E
            //    E > E
            //    E >= E
            // either E can be nullable

            op = operation;

            // parameters are easy -- they are the same as the type passed in
            expectedParameters = new ParameterInfo[2];
            expectedParameters[0] = new SimpleParameterInfo(lhs);
            expectedParameters[1] = new SimpleParameterInfo(rhs);

            // compute return type (depends on type of operation)
            // start by getting the types without Nullable<>
            bool lhsNullable = ConditionHelper.IsNullableValueType(lhs);
            bool rhsNullable = ConditionHelper.IsNullableValueType(rhs);
            lhsBaseType = (lhsNullable) ? Nullable.GetUnderlyingType(lhs) : lhs;
            rhsBaseType = (rhsNullable) ? Nullable.GetUnderlyingType(rhs) : rhs;
            // determine the underlying types for both sides
            if (lhsBaseType.IsEnum)
                lhsRootType = EnumHelper.GetUnderlyingType(lhsBaseType);
            else
                lhsRootType = lhsBaseType;

            if (rhsBaseType.IsEnum)
                rhsRootType = EnumHelper.GetUnderlyingType(rhsBaseType);
            else
                rhsRootType = rhsBaseType;

            switch (op)
            {
                case CodeBinaryOperatorType.Add:
                    // add always produces an enum, except enum + enum
                    if ((lhsBaseType.IsEnum) && (rhs.IsEnum))
                        resultBaseType = lhsRootType;
                    else if (lhsBaseType.IsEnum)
                        resultBaseType = lhsBaseType;
                    else
                        resultBaseType = rhsBaseType;
                    // if either side is nullable, result is nullable
                    resultIsNullable = (lhsNullable || rhsNullable);
                    resultType = (resultIsNullable) ? typeof(Nullable<>).MakeGenericType(resultBaseType) : resultBaseType;
                    break;
                case CodeBinaryOperatorType.Subtract:
                    // subtract can be an enum or the underlying type
                    if (rhsBaseType.IsEnum && lhsBaseType.IsEnum)
                    {
                        resultRootType = rhsRootType;
                        resultBaseType = rhsRootType;
                    }
                    else if (lhsBaseType.IsEnum)
                    {
                        // special case for E - 0
                        // if 0 is the underlying type, then use E - U
                        // if not the underlying type, then 0 becomes E, use E - E
                        resultRootType = lhsRootType;
                        if (isZero && rhsBaseType != lhsRootType)
                            resultBaseType = lhsRootType;
                        else
                            resultBaseType = lhsBaseType;
                    }
                    else    // rhsType.IsEnum
                    {
                        // special case for 0 - E
                        // in all cases 0 becomes E, use E - E
                        resultRootType = rhsRootType;
                        if (isZero)
                            resultBaseType = rhsRootType;
                        else
                            resultBaseType = rhsBaseType;
                    }
                    resultIsNullable = (lhsNullable || rhsNullable);
                    resultType = (resultIsNullable) ? typeof(Nullable<>).MakeGenericType(resultBaseType) : resultBaseType;
                    break;
                case CodeBinaryOperatorType.ValueEquality:
                case CodeBinaryOperatorType.LessThan:
                case CodeBinaryOperatorType.LessThanOrEqual:
                case CodeBinaryOperatorType.GreaterThan:
                case CodeBinaryOperatorType.GreaterThanOrEqual:
                    resultType = typeof(bool);
                    break;
            }
        }

        public override MethodInfo GetBaseDefinition()
        {
            return null;
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes
        {
            get { return null; }
        }

        public override MethodAttributes Attributes
        {
            get { return MethodAttributes.Static; }
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return MethodImplAttributes.Runtime;
        }

        public override ParameterInfo[] GetParameters()
        {
            return expectedParameters;
        }

        [SuppressMessage("Microsoft.Performance", "CA1803:AvoidCostlyCallsWherePossible")]
        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            // we should get passed in 2 values that correspond to the parameter types

            object result;
            ArithmeticLiteral leftArithmetic, rightArithmetic;
            Literal leftLiteral, rightLiteral;

            // for design-time types we couldn't find the underlying type, so do it now
            if (lhsRootType == null)
                lhsRootType = Enum.GetUnderlyingType(lhsBaseType);
            if (rhsRootType == null)
                rhsRootType = Enum.GetUnderlyingType(rhsBaseType);

            switch (op)
            {
                case CodeBinaryOperatorType.Add:
                    // if either is null, then the result is null
                    if ((parameters[0] == null) || (parameters[1] == null))
                        return null;
                    leftArithmetic = ArithmeticLiteral.MakeLiteral(lhsRootType, parameters[0]);
                    rightArithmetic = ArithmeticLiteral.MakeLiteral(rhsRootType, parameters[1]);
                    result = leftArithmetic.Add(rightArithmetic);
                    result = Executor.AdjustType(result.GetType(), result, resultBaseType);
                    if (resultIsNullable)
                        result = Activator.CreateInstance(resultType, result);
                    return result;
                case CodeBinaryOperatorType.Subtract:
                    // if either is null, then the result is null
                    if ((parameters[0] == null) || (parameters[1] == null))
                        return null;
                    leftArithmetic = ArithmeticLiteral.MakeLiteral(resultRootType,
                        Executor.AdjustType(lhsRootType, parameters[0], resultRootType));
                    rightArithmetic = ArithmeticLiteral.MakeLiteral(resultRootType,
                        Executor.AdjustType(rhsRootType, parameters[1], resultRootType));
                    result = leftArithmetic.Subtract(rightArithmetic);
                    result = Executor.AdjustType(result.GetType(), result, resultBaseType);
                    if (resultIsNullable)
                        result = Activator.CreateInstance(resultType, result);
                    return result;

                case CodeBinaryOperatorType.ValueEquality:
                    leftLiteral = Literal.MakeLiteral(lhsRootType, parameters[0]);
                    rightLiteral = Literal.MakeLiteral(rhsRootType, parameters[1]);
                    return leftLiteral.Equal(rightLiteral);
                case CodeBinaryOperatorType.LessThan:
                    leftLiteral = Literal.MakeLiteral(lhsRootType, parameters[0]);
                    rightLiteral = Literal.MakeLiteral(rhsRootType, parameters[1]);
                    return leftLiteral.LessThan(rightLiteral);
                case CodeBinaryOperatorType.LessThanOrEqual:
                    leftLiteral = Literal.MakeLiteral(lhsRootType, parameters[0]);
                    rightLiteral = Literal.MakeLiteral(rhsRootType, parameters[1]);
                    return leftLiteral.LessThanOrEqual(rightLiteral);
                case CodeBinaryOperatorType.GreaterThan:
                    leftLiteral = Literal.MakeLiteral(lhsRootType, parameters[0]);
                    rightLiteral = Literal.MakeLiteral(rhsRootType, parameters[1]);
                    return leftLiteral.GreaterThan(rightLiteral);
                case CodeBinaryOperatorType.GreaterThanOrEqual:
                    leftLiteral = Literal.MakeLiteral(lhsRootType, parameters[0]);
                    rightLiteral = Literal.MakeLiteral(rhsRootType, parameters[1]);
                    return leftLiteral.GreaterThanOrEqual(rightLiteral);
            }
            string message = string.Format(CultureInfo.CurrentCulture, Messages.BinaryOpNotSupported, op.ToString());
            throw new RuleEvaluationException(message);
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get { return new RuntimeMethodHandle(); }
        }

        public override Type DeclaringType
        {
            get { return typeof(Enum); }
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return new object[0];
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return new object[0];
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return true;
        }

        public override string Name
        {
            get { return "op_Enum"; }
        }

        public override Type ReflectedType
        {
            get { return resultType; }
        }

        public override Type ReturnType
        {
            get { return resultType; }
        }
    }
    #endregion

    #region SimpleRunTimeTypeProvider

    internal class SimpleRunTimeTypeProvider : ITypeProvider
    {
        private Assembly root;
        private List<Assembly> references;

        internal SimpleRunTimeTypeProvider(Assembly startingAssembly)
        {
            root = startingAssembly;
        }

        public Type GetType(string name)
        {
            return GetType(name, false);
        }

        public Type GetType(string name, bool throwOnError)
        {
            // is the type available in the main workflow assembly?
            Type type = root.GetType(name, throwOnError, false);
            if (type != null)
                return type;

            // now try mscorlib or this assembly
            // (or if the name is an assembly qualified name)
            type = Type.GetType(name, throwOnError, false);
            if (type != null)
                return type;

            // no luck so far, so try all referenced assemblies
            foreach (Assembly a in ReferencedAssemblies)
            {
                type = a.GetType(name, throwOnError, false);
                if (type != null)
                    return type;
            }

            // keep going by trying all loaded assemblies
            Assembly[] loaded = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < loaded.Length; ++i)
            {
                type = loaded[i].GetType(name, throwOnError, false);
                if (type != null)
                    return type;
            }
            return null;
        }

        public Type[] GetTypes()
        {
            List<Type> types = new List<Type>();
            try
            {
                types.AddRange(root.GetTypes());
            }
            catch (ReflectionTypeLoadException e)
            {
                // problems loading all the types, take what we can get
                foreach (Type type in e.Types)
                    if (type != null)
                        types.Add(type);
            }
            foreach (Assembly a in ReferencedAssemblies)
            {
                try
                {
                    types.AddRange(a.GetTypes());
                }
                catch (ReflectionTypeLoadException e)
                {
                    // problems loading all the types, take what we can get
                    foreach (Type type in e.Types)
                        if (type != null)
                            types.Add(type);
                }
            }
            return types.ToArray();
        }

        public Assembly LocalAssembly
        {
            get { return root; }
        }

        public ICollection<Assembly> ReferencedAssemblies
        {
            get
            {
                // references is created on demand, does not include root
                if (references == null)
                {
                    List<Assembly> list = new List<Assembly>();
                    foreach (AssemblyName a in root.GetReferencedAssemblies())
                    {
                        list.Add(Assembly.Load(a));
                    }
                    references = list;
                }
                return references;
            }
        }

        public IDictionary<object, Exception> TypeLoadErrors
        {
            get
            {
                // we never use this method, so add use of EventHandlers to keep compiler happy
                TypesChanged.Invoke(this, null);
                TypeLoadErrorsChanged.Invoke(this, null);
                return null;
            }
        }

        public event EventHandler TypesChanged;

        public event EventHandler TypeLoadErrorsChanged;
    }
    #endregion

    #region RuleValidation

    public class RuleValidation
    {
        private Type thisType;
        private ITypeProvider typeProvider;
        private ValidationErrorCollection errors = new ValidationErrorCollection();
        private Dictionary<string, Type> typesUsed = new Dictionary<string, Type>(16);
        private Dictionary<string, Type> typesUsedAuthorized;
        private Stack<CodeExpression> activeParentNodes = new Stack<CodeExpression>();
        private Dictionary<CodeExpression, RuleExpressionInfo> expressionInfoMap = new Dictionary<CodeExpression, RuleExpressionInfo>();
        private Dictionary<CodeTypeReference, Type> typeRefMap = new Dictionary<CodeTypeReference, Type>();
        private bool checkStaticType;
        private IList<AuthorizedType> authorizedTypes;
        private static readonly Type voidType = typeof(void);
        private static string voidTypeName = voidType.AssemblyQualifiedName;

        #region Constructors

        // Validate at design time.
        public RuleValidation(Activity activity, ITypeProvider typeProvider, bool checkStaticType)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");
            if (typeProvider == null)
                throw new ArgumentNullException("typeProvider");

            this.thisType = ConditionHelper.GetContextType(typeProvider, activity);
            this.typeProvider = typeProvider;
            this.checkStaticType = checkStaticType;
            if (checkStaticType)
            {
                Debug.Assert(WorkflowCompilationContext.Current != null, "Can't have checkTypes set to true without a context in scope");
                this.authorizedTypes = WorkflowCompilationContext.Current.GetAuthorizedTypes();
                this.typesUsedAuthorized = new Dictionary<string, Type>();
                this.typesUsedAuthorized.Add(voidTypeName, voidType);
            }
        }

        // Validate at runtime when we have the actual subject instance.  This is
        // mostly for conditions used in activities like IfElse.
        internal RuleValidation(object thisObject)
        {
            if (thisObject == null)
                throw new ArgumentNullException("thisObject");

            this.thisType = thisObject.GetType();
            this.typeProvider = new SimpleRunTimeTypeProvider(this.thisType.Assembly);
        }

        // Validate at runtime when we have just the type.  This is mostly for rules.
        public RuleValidation(Type thisType, ITypeProvider typeProvider)
        {
            if (thisType == null)
                throw new ArgumentNullException("thisType");

            this.thisType = thisType;
            this.typeProvider = (typeProvider != null) ? typeProvider : new SimpleRunTimeTypeProvider(this.thisType.Assembly);
        }

        #endregion

        #region Internal validation methods

        internal bool ValidateConditionExpression(CodeExpression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            // Run the validation pass.
            RuleExpressionInfo exprInfo = RuleExpressionWalker.Validate(this, expression, false);
            if (exprInfo == null)
                return false;

            Type resultType = exprInfo.ExpressionType;
            if (!IsValidBooleanResult(resultType))
            {
                // not a boolean, so complain unless another error may have caused this problem
                if (resultType != null || Errors.Count == 0)
                {
                    string message = Messages.ConditionMustBeBoolean;
                    ValidationError error = new ValidationError(message, ErrorNumbers.Error_ConditionMustBeBoolean);
                    error.UserData[RuleUserDataKeys.ErrorObject] = expression;
                    Errors.Add(error);
                }
            }

            return Errors.Count == 0;
        }

        internal static bool IsValidBooleanResult(Type type)
        {
            return ((type == typeof(bool))
                || (type == typeof(bool?))
                || (ImplicitConversion(type, typeof(bool))));
        }

        internal static bool IsPrivate(MethodInfo methodInfo)
        {
            return methodInfo.IsPrivate
                || methodInfo.IsFamily
                || methodInfo.IsFamilyOrAssembly
                || methodInfo.IsFamilyAndAssembly;
        }

        internal static bool IsPrivate(FieldInfo fieldInfo)
        {
            return fieldInfo.IsPrivate
                || fieldInfo.IsFamily
                || fieldInfo.IsFamilyOrAssembly
                || fieldInfo.IsFamilyAndAssembly;
        }

        internal static bool IsInternal(MethodInfo methodInfo)
        {
            return methodInfo.IsAssembly
                || methodInfo.IsFamilyAndAssembly;
        }

        internal static bool IsInternal(FieldInfo fieldInfo)
        {
            return fieldInfo.IsAssembly
                || fieldInfo.IsFamilyAndAssembly;
        }

        #endregion

        #region Miscellaneous public properties & methods

        public Type ThisType
        {
            get { return thisType; }
        }

        internal ITypeProvider GetTypeProvider()
        {
            return typeProvider;
        }

        public ValidationErrorCollection Errors
        {
            get { return errors; }
        }

        internal bool AllowInternalMembers(Type type)
        {
            return type.Assembly == thisType.Assembly;
        }

        internal void AddError(ValidationError error)
        {
            this.Errors.Add(error);
        }

        public bool PushParentExpression(CodeExpression newParent)
        {
            if (newParent == null)
                throw new ArgumentNullException("newParent");

            if (activeParentNodes.Contains(newParent))
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.CyclicalExpression);
                ValidationError error = new ValidationError(message, ErrorNumbers.Error_CyclicalExpression);
                error.UserData[RuleUserDataKeys.ErrorObject] = newParent;
                Errors.Add(error);
                return false;
            }

            activeParentNodes.Push(newParent);
            return true;
        }

        public void PopParentExpression()
        {
            activeParentNodes.Pop();
        }

        // Get the ExpressionInfo associated with a given CodeExpression
        public RuleExpressionInfo ExpressionInfo(CodeExpression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            RuleExpressionInfo exprInfo = null;
            expressionInfoMap.TryGetValue(expression, out exprInfo);

            return exprInfo;
        }

        #endregion

        #region CodeDom Expression Validation methods

        internal RuleExpressionInfo ValidateSubexpression(CodeExpression expr, RuleExpressionInternal ruleExpr, bool isWritten)
        {
            Debug.Assert(ruleExpr != null, "Validation::ValidateSubexpression - IRuleExpression is null");
            Debug.Assert(expr != null, "Validation::ValidateSubexpression - CodeExpression is null");

            RuleExpressionInfo exprInfo = ruleExpr.Validate(expr, this, isWritten);

            if (exprInfo != null)
            {
                // Add the CodeExpression object to the info map.  We don't want to add the IRuleExpression guy
                // as the key, since it might likely be just a tearoff wrapper.
                expressionInfoMap[expr] = exprInfo;
            }

            return exprInfo;
        }

        internal static bool TypesAreAssignable(Type rhsType, Type lhsType, CodeExpression rhsExpression, out ValidationError error)
        {
            // determine if rhsType can be implicitly converted to lhsType,
            // following the rules in C# specification section 6.1, 
            // plus support for Nullable<T>

            // all but 6.1.7 handled as a standard implicit conversion
            if (StandardImplicitConversion(rhsType, lhsType, rhsExpression, out error))
                return true;
            if (error != null)
                return false;

            // no standard implicit conversion works, see if user specified one
            // from section 6.4.3, start by determining what types to check
            // as we find each type, add the list of implicit conversions available
            if (FindImplicitConversion(rhsType, lhsType, out error) == null)
                return false;
            return true;
        }

        internal static bool ExplicitConversionSpecified(Type fromType, Type toType, out ValidationError error)
        {
            // determine if fromType can be implicitly converted to toType,
            // following the rules in C# specification section 6.2

            // start by seeing if there is a standard implicit conversion
            if (StandardImplicitConversion(fromType, toType, null, out error))
                return true;
            if (error != null)
                return false;

            // explicit numeric conversions
            // also handles Enum conversions, since GetTypeCode returns the underlying type
            if (fromType.IsValueType && toType.IsValueType && IsExplicitNumericConversion(fromType, toType))
                return true;

            // explicit reference conversions
            // this looks like the inverse of implicit conversions
            ValidationError dummyError; // so we don't return an error
            if (StandardImplicitConversion(toType, fromType, null, out dummyError))
                return true;
            // include interface checks
            if (toType.IsInterface)
            {
                // from any class-type S to any interface-type T, provided S is not sealed and provided S does not implement T.
                // latter part should be handled by implicit conversion, so we are ok as long as class is not sealed
                if ((fromType.IsClass) && (!fromType.IsSealed))
                    return true;
                // from any interface-type S to any interface-type T, provided S is not derived from T.
                // again, if S derived from T, handled by implicit conversion above
                if (fromType.IsInterface)
                    return true;
            }
            if (fromType.IsInterface)
            {
                // from any interface-type S to any class-type T, provided T is not sealed or provided T implements S.
                if ((toType.IsClass) && ((!toType.IsSealed) || (InterfaceMatch(toType.GetInterfaces(), fromType))))
                    return true;
            }

            // no look for user-defined conversions
            // from section 6.4.4, start by determining what types to check
            // as we find each type, add the list of implicit conversions available
            if (FindExplicitConversion(fromType, toType, out error) == null)
                return false;
            return true;
        }

        private static bool InterfaceMatch(Type[] types, Type fromType)
        {
            foreach (Type t in types)
            {
                if (t == fromType)
                    return true;
            }
            return false;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static MethodInfo FindImplicitConversion(Type fromType, Type toType, out ValidationError error)
        {
            List<MethodInfo> candidates = new List<MethodInfo>();

            bool fromIsNullable = ConditionHelper.IsNullableValueType(fromType);
            bool toIsNullable = ConditionHelper.IsNullableValueType(toType);
            Type fromType0 = (fromIsNullable) ? Nullable.GetUnderlyingType(fromType) : fromType;
            Type toType0 = (toIsNullable) ? Nullable.GetUnderlyingType(toType) : toType;

            if (fromType0.IsClass)
            {
                AddImplicitConversions(fromType0, fromType, toType, candidates);
                Type baseType = fromType0.BaseType;
                while ((baseType != null) && (baseType != typeof(object)))
                {
                    AddImplicitConversions(baseType, fromType, toType, candidates);
                    baseType = baseType.BaseType;
                }
            }
            else if (IsStruct(fromType0))
            {
                AddImplicitConversions(fromType0, fromType, toType, candidates);
            }
            if ((toType0.IsClass) || (IsStruct(toType0)))
            {
                AddImplicitConversions(toType0, fromType, toType, candidates);
            }

            // if both types are nullable, add the lifted operators
            if (fromIsNullable && toIsNullable)
            {
                // start by finding all the conversion operators from S0 -> T0
                List<MethodInfo> liftedCandidates = new List<MethodInfo>();
                if (fromType0.IsClass)
                {
                    AddImplicitConversions(fromType0, fromType0, toType0, liftedCandidates);
                    Type baseType = fromType0.BaseType;
                    while ((baseType != null) && (baseType != typeof(object)))
                    {
                        AddImplicitConversions(baseType, fromType0, toType0, liftedCandidates);
                        baseType = baseType.BaseType;
                    }
                }
                else if (IsStruct(fromType0))
                {
                    AddImplicitConversions(fromType0, fromType0, toType0, liftedCandidates);
                }
                if ((toType0.IsClass) || (IsStruct(toType0)))
                {
                    AddImplicitConversions(toType0, fromType0, toType0, liftedCandidates);
                }

                // add them all to the candidates list as lifted methods (which wraps them appropriately)
                foreach (MethodInfo mi in liftedCandidates)
                {
                    // only lift candidates that convert from a non-nullable value type
                    // to a non-nullable value type
                    ParameterInfo[] parameters = mi.GetParameters();
                    if (ConditionHelper.IsNonNullableValueType(mi.ReturnType) && ConditionHelper.IsNonNullableValueType(parameters[0].ParameterType))
                        candidates.Add(new LiftedConversionMethodInfo(mi));
                }
            }

            if (candidates.Count == 0)
            {
                // no overrides, so must be false
                string message = string.Format(CultureInfo.CurrentCulture,
                    Messages.NoConversion,
                    RuleDecompiler.DecompileType(fromType),
                    RuleDecompiler.DecompileType(toType));
                error = new ValidationError(message, ErrorNumbers.Error_OperandTypesIncompatible);
                return null;
            }

            // find the most specific source type
            ValidationError dummyError; // so we don't return an error
            Type sx = candidates[0].GetParameters()[0].ParameterType;
            if (sx != fromType)
            {
                for (int i = 1; i < candidates.Count; ++i)
                {
                    Type testType = candidates[i].GetParameters()[0].ParameterType;
                    if (testType == fromType)
                    {
                        // we have a match with the source type, so that's the correct answer
                        sx = fromType;
                        break;
                    }
                    if (StandardImplicitConversion(testType, sx, null, out dummyError))
                        sx = testType;
                }
            }

            // find the most specific target type
            Type tx = candidates[0].ReturnType;
            if (tx != toType)
            {
                for (int i = 1; i < candidates.Count; ++i)
                {
                    Type testType = candidates[i].ReturnType;
                    if (testType == toType)
                    {
                        // we have a match with the target type, so that's the correct answer
                        tx = toType;
                        break;
                    }
                    if (StandardImplicitConversion(tx, testType, null, out dummyError))
                        tx = testType;
                }
            }

            // see how many candidates convert from sx to tx, ignoring lifted methods
            int numMatches = 0;
            int position = 0;
            for (int i = 0; i < candidates.Count; ++i)
            {
                if ((candidates[i].ReturnType == tx) &&
                    (candidates[i].GetParameters()[0].ParameterType == sx) &&
                    (!(candidates[i] is LiftedConversionMethodInfo)))
                {
                    position = i;
                    ++numMatches;
                }
            }
            if (numMatches == 1)
            {
                // found what we are looking for
                error = null;
                return candidates[position];
            }

            // now check for lifted conversions
            if ((toIsNullable) && (numMatches == 0))
            {
                if (fromIsNullable)
                {
                    for (int i = 0; i < candidates.Count; ++i)
                    {
                        if ((candidates[i].ReturnType == tx) &&
                            (candidates[i].GetParameters()[0].ParameterType == sx) &&
                            (candidates[i] is LiftedConversionMethodInfo))
                        {
                            position = i;
                            ++numMatches;
                        }
                    }
                    if (numMatches == 1)
                    {
                        // found what we are looking for
                        error = null;
                        return candidates[position];
                    }
                }
                else
                {
                    // we are doing a conversion T? = S, so a conversion from S -> T is valid
                    MethodInfo result = FindImplicitConversion(fromType, toType0, out error);
                    if (result != null)
                    {
                        error = null;
                        // return it as a lifted method so the wrapping to T? is done
                        return new LiftedConversionMethodInfo(result);
                    }
                }
            }

            // no exact matches, so it's an error
            string message2 = string.Format(CultureInfo.CurrentCulture,
                Messages.AmbiguousConversion,
                RuleDecompiler.DecompileType(fromType),
                RuleDecompiler.DecompileType(toType));
            error = new ValidationError(message2, ErrorNumbers.Error_OperandTypesIncompatible);
            return null;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static MethodInfo FindExplicitConversion(Type fromType, Type toType, out ValidationError error)
        {
            List<MethodInfo> candidates = new List<MethodInfo>();
            ValidationError dummyError; // don't return transient errors

            bool fromIsNullable = ConditionHelper.IsNullableValueType(fromType);
            bool toIsNullable = ConditionHelper.IsNullableValueType(toType);
            Type fromType0 = (fromIsNullable) ? Nullable.GetUnderlyingType(fromType) : fromType;
            Type toType0 = (toIsNullable) ? Nullable.GetUnderlyingType(toType) : toType;

            if (fromType0.IsClass)
            {
                AddExplicitConversions(fromType0, fromType, toType, candidates);
                Type baseType = fromType0.BaseType;
                while ((baseType != null) && (baseType != typeof(object)))
                {
                    AddExplicitConversions(baseType, fromType, toType, candidates);
                    baseType = baseType.BaseType;
                }
            }
            else if (IsStruct(fromType0))
            {
                AddExplicitConversions(fromType0, fromType, toType, candidates);
            }
            if (toType0.IsClass)
            {
                AddExplicitConversions(toType0, fromType, toType, candidates);
                Type baseType = toType0.BaseType;
                while ((baseType != null) && (baseType != typeof(object)))
                {
                    AddExplicitConversions(baseType, fromType, toType, candidates);
                    baseType = baseType.BaseType;
                }
            }
            else if (IsStruct(toType0))
            {
                AddExplicitConversions(toType0, fromType, toType, candidates);
            }

            // if both types are nullable, add the lifted operators
            if (fromIsNullable && toIsNullable)
            {
                // start by finding all the conversion operators from S0 -> T0
                List<MethodInfo> liftedCandidates = new List<MethodInfo>();
                if (fromType0.IsClass)
                {
                    AddExplicitConversions(fromType0, fromType0, toType0, liftedCandidates);
                    Type baseType = fromType0.BaseType;
                    while ((baseType != null) && (baseType != typeof(object)))
                    {
                        AddExplicitConversions(baseType, fromType0, toType0, liftedCandidates);
                        baseType = baseType.BaseType;
                    }
                }
                else if (IsStruct(fromType0))
                {
                    AddExplicitConversions(fromType0, fromType0, toType0, liftedCandidates);
                }
                if (toType0.IsClass)
                {
                    AddExplicitConversions(toType0, fromType0, toType0, liftedCandidates);
                    Type baseType = toType0.BaseType;
                    while ((baseType != null) && (baseType != typeof(object)))
                    {
                        AddExplicitConversions(baseType, fromType0, toType0, liftedCandidates);
                        baseType = baseType.BaseType;
                    }
                }
                else if (IsStruct(toType0))
                {
                    AddExplicitConversions(toType0, fromType0, toType0, liftedCandidates);
                }

                // add them all to the candidates list as lifted methods (which wraps them appropriately)
                foreach (MethodInfo mi in liftedCandidates)
                    candidates.Add(new LiftedConversionMethodInfo(mi));
            }

            if (candidates.Count == 0)
            {
                // no overrides, so must be false
                string message = string.Format(CultureInfo.CurrentCulture,
                    Messages.NoConversion,
                    RuleDecompiler.DecompileType(fromType),
                    RuleDecompiler.DecompileType(toType));
                error = new ValidationError(message, ErrorNumbers.Error_OperandTypesIncompatible);
                return null;
            }

            // find the most specific source type
            // if any are s, s is the answer
            Type sx = null;
            for (int i = 0; i < candidates.Count; ++i)
            {
                Type testType = candidates[i].GetParameters()[0].ParameterType;
                if (testType == fromType)
                {
                    // we have a match with the source type, so that's the correct answer
                    sx = fromType;
                    break;
                }
            }
            // if no match, find the most encompassed type if the type encompasses s
            if (sx == null)
            {
                for (int i = 0; i < candidates.Count; ++i)
                {
                    Type testType = candidates[i].GetParameters()[0].ParameterType;
                    if (StandardImplicitConversion(fromType, testType, null, out dummyError))
                    {
                        if (sx == null)
                            sx = testType;
                        else if (StandardImplicitConversion(testType, sx, null, out dummyError))
                            sx = testType;
                    }
                }
            }
            // still no match, find most encompassing type
            if (sx == null)
            {
                for (int i = 0; i < candidates.Count; ++i)
                {
                    Type testType = candidates[i].GetParameters()[0].ParameterType;
                    if (StandardImplicitConversion(testType, fromType, null, out dummyError))
                    {
                        if (sx == null)
                            sx = testType;
                        else if (StandardImplicitConversion(sx, testType, null, out dummyError))
                            sx = testType;
                    }
                }
            }

            // find the most specific target type
            // if any are t, t is the answer
            Type tx = null;
            for (int i = 0; i < candidates.Count; ++i)
            {
                Type testType = candidates[i].ReturnType;
                if (testType == toType)
                {
                    // we have a match with the target type, so that's the correct answer
                    tx = toType;
                    break;
                }
            }
            // if no match, find the most encompassed type if the type encompasses s
            if (tx == null)
            {
                for (int i = 0; i < candidates.Count; ++i)
                {
                    Type testType = candidates[i].ReturnType;
                    if (StandardImplicitConversion(testType, toType, null, out dummyError))
                    {
                        if (tx == null)
                            tx = testType;
                        else if (StandardImplicitConversion(tx, testType, null, out dummyError))
                            tx = testType;
                    }
                }
            }
            // still no match, find most encompassing type
            if (tx == null)
            {
                for (int i = 0; i < candidates.Count; ++i)
                {
                    Type testType = candidates[i].ReturnType;
                    if (StandardImplicitConversion(toType, testType, null, out dummyError))
                    {
                        if (tx == null)
                            tx = testType;
                        else if (StandardImplicitConversion(testType, tx, null, out dummyError))
                            tx = testType;
                    }
                }
            }

            // see how many candidates convert from sx to tx, ignoring lifted methods
            int numMatches = 0;
            int position = 0;
            for (int i = 0; i < candidates.Count; ++i)
            {
                if ((candidates[i].ReturnType == tx) &&
                        (candidates[i].GetParameters()[0].ParameterType == sx) &&
                        (!(candidates[i] is LiftedConversionMethodInfo)))
                {
                    position = i;
                    ++numMatches;
                }
            }
            if (numMatches == 1)
            {
                // found what we are looking for
                error = null;
                return candidates[position];
            }

            // now check for lifted conversions
            if ((toIsNullable) && (numMatches == 0))
            {
                if (fromIsNullable)
                {
                    for (int i = 0; i < candidates.Count; ++i)
                    {
                        if ((candidates[i].ReturnType == tx) &&
                            (candidates[i].GetParameters()[0].ParameterType == sx) &&
                            (candidates[i] is LiftedConversionMethodInfo))
                        {
                            position = i;
                            ++numMatches;
                        }
                    }
                    if (numMatches == 1)
                    {
                        // found what we are looking for
                        error = null;
                        return candidates[position];
                    }
                }
                else
                {
                    // we are doing a conversion T? = S, so a conversion from S -> T is valid
                    MethodInfo result = FindExplicitConversion(fromType, toType0, out error);
                    if (result != null)
                    {
                        error = null;
                        // return it as a lifted method so the wrapping to T? is done
                        return new LiftedConversionMethodInfo(result);
                    }
                }
            }

            // no exact matches, so it's an error
            string message2 = string.Format(CultureInfo.CurrentCulture,
                Messages.AmbiguousConversion,
                RuleDecompiler.DecompileType(fromType),
                RuleDecompiler.DecompileType(toType));
            error = new ValidationError(message2, ErrorNumbers.Error_OperandTypesIncompatible);
            return null;
        }

        private static bool IsStruct(Type type)
        {
            return ((type.IsValueType) && (!type.IsPrimitive));
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static bool IsExplicitNumericConversion(Type sourceType, Type testType)
        {
            // includes the implicit conversions as well

            // unwrap nullables
            TypeCode sourceTypeCode = (ConditionHelper.IsNullableValueType(sourceType))
                ? Type.GetTypeCode(sourceType.GetGenericArguments()[0])
                : Type.GetTypeCode(sourceType);
            TypeCode testTypeCode = (ConditionHelper.IsNullableValueType(testType))
                ? Type.GetTypeCode(testType.GetGenericArguments()[0])
                : Type.GetTypeCode(testType);

            switch (sourceTypeCode)
            {
                case TypeCode.SByte:
                    switch (testTypeCode)
                    {
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.Char:

                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.Byte:
                    switch (testTypeCode)
                    {
                        case TypeCode.Byte:
                        case TypeCode.SByte:
                        case TypeCode.Char:

                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.Int16:
                    switch (testTypeCode)
                    {
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.Char:

                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.UInt16:
                    switch (testTypeCode)
                    {
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Char:

                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.Int32:
                    switch (testTypeCode)
                    {
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.Char:

                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.UInt32:
                    switch (testTypeCode)
                    {
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Char:

                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.Int64:
                    switch (testTypeCode)
                    {
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Char:

                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.UInt64:
                    switch (testTypeCode)
                    {
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Char:

                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.Char:
                    switch (testTypeCode)
                    {
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:

                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.Single:
                    switch (testTypeCode)
                    {
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Char:
                        case TypeCode.Single:
                        case TypeCode.Decimal:

                        case TypeCode.Double:
                            return true;
                    }
                    return false;

                case TypeCode.Double:
                    switch (testTypeCode)
                    {
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Char:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;

                case TypeCode.Decimal:
                    switch (testTypeCode)
                    {
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Char:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
            }
            return false;
        }


        internal static bool ImplicitConversion(Type fromType, Type toType)
        {
            ValidationError error;

            // is there a standard conversion we can use
            if (StandardImplicitConversion(fromType, toType, null, out error))
                return true;

            // no standard one, did the user provide one?
            return (FindImplicitConversion(fromType, toType, out error) != null);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static bool StandardImplicitConversion(Type rhsType, Type lhsType, CodeExpression rhsExpression, out ValidationError error)
        {
            error = null;

            // 6.1.1 identity conversion
            if (rhsType == lhsType)
            {
                // Easy special case... they're the same type.
                return true;
            }

            // 6.1.4 (h) from the null type to any reference-type
            if (rhsType == typeof(NullLiteral))
            {
                // Special case if the RHS is 'null'; just make sure the LHS type can be assigned a null value.
                if (ConditionHelper.IsNonNullableValueType(lhsType))
                {
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.AssignNotAllowed, Messages.NullValue, RuleDecompiler.DecompileType(lhsType));
                    error = new ValidationError(message, ErrorNumbers.Error_OperandTypesIncompatible);
                    return false;
                }
                return true;
            }

            // check for nullables
            bool lhsIsNullable = ConditionHelper.IsNullableValueType(lhsType);
            bool rhsIsNullable = ConditionHelper.IsNullableValueType(rhsType);
            if (rhsIsNullable)
            {
                if (!lhsIsNullable)
                {
                    // We had T1 = T2?, which is not valid for any T1 or T2, unless T1 is object
                    return (lhsType == typeof(object));
                }

                rhsType = Nullable.GetUnderlyingType(rhsType);
            }

            if (lhsIsNullable)
                lhsType = Nullable.GetUnderlyingType(lhsType);

            if (lhsType == rhsType)
            {
                // We had T? = T, which is valid.
                return true;
            }

            // handle rest of 6.1.4
            if (TypeProvider.IsAssignable(lhsType, rhsType))
            {
                // They are assignable, which will handle inheritance and trivial up-casting.
                return true;
            }

            // 6.1.3 implicit enumeration conversions
            if (lhsType.IsEnum)
            {
                // right-hand side can be decimal-integer-literal 0
                CodePrimitiveExpression primitive = rhsExpression as CodePrimitiveExpression;
                if ((primitive == null) || (primitive.Value == null))
                {
                    // not a constant
                    return false;
                }
                switch (Type.GetTypeCode(primitive.Value.GetType()))
                {
                    case TypeCode.SByte:
                        return ((sbyte)primitive.Value == 0);
                    case TypeCode.Byte:
                        return ((byte)primitive.Value == 0);
                    case TypeCode.Int16:
                        return ((short)primitive.Value == 0);
                    case TypeCode.UInt16:
                        return ((ushort)primitive.Value == 0);
                    case TypeCode.Int32:
                        return ((int)primitive.Value == 0);
                    case TypeCode.UInt32:
                        return ((uint)primitive.Value == 0);
                    case TypeCode.Int64:
                        return ((long)primitive.Value == 0);
                    case TypeCode.UInt64:
                        return ((ulong)primitive.Value == 0);
                    case TypeCode.Char:
                        return ((char)primitive.Value == 0);
                }
                return false;
            }
            if (rhsType.IsEnum)
            {
                // don't treat enums as numbers
                return false;
            }

            // 6.1.2 implicit numeric conversions
            // 6.1.6 implicit constant expression conversions
            // not assignable, but the assignment might still be valid for
            // value types if a free conversion is available.
            TypeCode lhsTypeCode = Type.GetTypeCode(lhsType);
            TypeCode rhsTypeCode = Type.GetTypeCode(rhsType);

            switch (lhsTypeCode)
            {
                case TypeCode.Decimal:
                    switch (rhsTypeCode)
                    {
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Decimal:
                        case TypeCode.Char:
                            return true;
                    }
                    return false;

                case TypeCode.Double:
                    switch (rhsTypeCode)
                    {
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Char:
                            return true;
                    }
                    return false;

                case TypeCode.Single:
                    switch (rhsTypeCode)
                    {
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Char:
                            return true;
                    }
                    return false;

                case TypeCode.Char:
                    switch (rhsTypeCode)
                    {
                        case TypeCode.Char:
                            return true;
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                            // Maybe, if the value is in range.
                            return CheckValueRange(rhsExpression, lhsType, out error);
                    }
                    return false;

                case TypeCode.SByte:
                    switch (rhsTypeCode)
                    {
                        case TypeCode.SByte:
                            return true;
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Char:
                            // Maybe, if the value is in range.
                            return CheckValueRange(rhsExpression, lhsType, out error);
                    }
                    return false;

                case TypeCode.Byte:
                    switch (rhsTypeCode)
                    {
                        case TypeCode.Byte:
                            return true;
                        case TypeCode.SByte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Char:
                            // Maybe, if the value is in range.
                            return CheckValueRange(rhsExpression, lhsType, out error);
                    }
                    return false;

                case TypeCode.Int16:
                    switch (rhsTypeCode)
                    {
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                            return true;
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Char:
                            // Maybe, if the value is in range.
                            return CheckValueRange(rhsExpression, lhsType, out error);
                    }
                    return false;

                case TypeCode.Int32:
                    switch (rhsTypeCode)
                    {
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.Char:
                            return true;
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                            // Maybe, if the value is in range.
                            return CheckValueRange(rhsExpression, lhsType, out error);
                    }
                    return false;

                case TypeCode.Int64:
                    switch (rhsTypeCode)
                    {
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.Char:
                            return true;
                        case TypeCode.UInt64:
                            // Maybe, if the value is in range.
                            return CheckValueRange(rhsExpression, lhsType, out error);
                    }
                    return false;

                case TypeCode.UInt16:
                    switch (rhsTypeCode)
                    {
                        case TypeCode.Byte:
                        case TypeCode.UInt16:
                        case TypeCode.Char:
                            return true;
                        case TypeCode.SByte:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                            // Maybe, if the value is in range.
                            return CheckValueRange(rhsExpression, lhsType, out error);
                    }
                    return false;

                case TypeCode.UInt32:
                    switch (rhsTypeCode)
                    {
                        case TypeCode.Byte:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.Char:
                            return true;
                        case TypeCode.SByte:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                            // Maybe, if the value is in range.
                            return CheckValueRange(rhsExpression, lhsType, out error);
                    }
                    return false;

                case TypeCode.UInt64:
                    switch (rhsTypeCode)
                    {
                        case TypeCode.Byte:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.Char:
                            return true;
                        case TypeCode.SByte:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                            // Maybe, if the value is in range.
                            return CheckValueRange(rhsExpression, lhsType, out error);
                    }
                    return false;

                default:
                    // It wasn't a numeric type, it was some other kind of value type (e.g., bool,
                    // DateTime, etc).  There will be no conversions.
                    return false;
            }
        }

        private static void AddImplicitConversions(Type t, Type source, Type target, List<MethodInfo> methods)
        {
            // append the list of methods that match the name specified
            // s is the source type, so the parameter must encompass it
            // t is the target type, so it must encompass the result
            MethodInfo[] possible = t.GetMethods(BindingFlags.Static | BindingFlags.Public);
            foreach (MethodInfo mi in possible)
            {
                if ((mi.Name == "op_Implicit") && (mi.GetParameters().Length == 1))
                {
                    Type sourceType = mi.GetParameters()[0].ParameterType;
                    Type targetType = mi.ReturnType;
                    ValidationError error;
                    if (StandardImplicitConversion(source, sourceType, null, out error) &&
                        StandardImplicitConversion(targetType, target, null, out error))
                    {
                        if (!methods.Contains(mi))
                            methods.Add(mi);
                    }
                }
            }
        }

        private static void AddExplicitConversions(Type t, Type source, Type target, List<MethodInfo> methods)
        {
            // append the list of methods that match the name specified
            // s is the source type, so the parameter must encompass it
            // t is the target type, so it must encompass the result
            MethodInfo[] possible = t.GetMethods(BindingFlags.Static | BindingFlags.Public);
            foreach (MethodInfo mi in possible)
            {
                if (((mi.Name == "op_Implicit") || (mi.Name == "op_Explicit")) && (mi.GetParameters().Length == 1))
                {
                    Type sourceType = mi.GetParameters()[0].ParameterType;
                    Type targetType = mi.ReturnType;
                    ValidationError error;
                    if ((StandardImplicitConversion(source, sourceType, null, out error) || StandardImplicitConversion(sourceType, source, null, out error))
                     && (StandardImplicitConversion(target, targetType, null, out error) || StandardImplicitConversion(targetType, target, null, out error)))
                    {
                        if (!methods.Contains(mi))
                            methods.Add(mi);
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static bool CheckValueRange(CodeExpression rhsExpression, Type lhsType, out ValidationError error)
        {
            error = null;

            CodePrimitiveExpression primitive = rhsExpression as CodePrimitiveExpression;
            if (primitive != null)
            {
                try
                {
                    System.Convert.ChangeType(primitive.Value, lhsType, CultureInfo.CurrentCulture);
                    // If we get here without throwing, it's valid.
                    return true;
                }
                catch (Exception e)
                {
                    error = new ValidationError(e.Message, ErrorNumbers.Error_OperandTypesIncompatible);
                    return false;
                }
            }

            return false;
        }

        internal bool ValidateMemberAccess(
            CodeExpression targetExpression, Type targetType, FieldInfo accessorMethod, string memberName, CodeExpression parentExpr)
        {
            return this.ValidateMemberAccess(
                targetExpression, targetType, memberName, parentExpr,
                accessorMethod.DeclaringType.Assembly, RuleValidation.IsPrivate(accessorMethod), RuleValidation.IsInternal(accessorMethod), accessorMethod.IsStatic);
        }

        internal bool ValidateMemberAccess(
            CodeExpression targetExpression, Type targetType, MethodInfo accessorMethod, string memberName, CodeExpression parentExpr)
        {
            return this.ValidateMemberAccess(
                targetExpression, targetType, memberName, parentExpr,
                accessorMethod.DeclaringType.Assembly, RuleValidation.IsPrivate(accessorMethod), RuleValidation.IsInternal(accessorMethod), accessorMethod.IsStatic);
        }

        private bool ValidateMemberAccess(
            CodeExpression targetExpression, Type targetType, string memberName, CodeExpression parentExpr,
            Assembly methodAssembly, bool isPrivate, bool isInternal, bool isStatic)
        {
            string message;

            if (isStatic != (targetExpression is CodeTypeReferenceExpression))
            {
                // If it's static, then the target object must be a type ref, and vice versa.

                int errorNumber;

                if (isStatic)
                {
                    // We have "object.StaticMember"
                    message = string.Format(CultureInfo.CurrentCulture, Messages.StaticMember, memberName);
                    errorNumber = ErrorNumbers.Error_StaticMember;
                }
                else
                {
                    // We have "TypeName.NonStaticMember"
                    message = string.Format(CultureInfo.CurrentCulture, Messages.NonStaticMember, memberName);
                    errorNumber = ErrorNumbers.Error_NonStaticMember;
                }

                ValidationError error = new ValidationError(message, errorNumber);
                error.UserData[RuleUserDataKeys.ErrorObject] = parentExpr;
                Errors.Add(error);

                return false;
            }

            if (isPrivate && targetType != ThisType)
            {
                // Can't access private members except on the subject type.
                message = string.Format(CultureInfo.CurrentCulture, Messages.CannotAccessPrivateMember, memberName, RuleDecompiler.DecompileType(targetType));
                ValidationError error = new ValidationError(message, ErrorNumbers.Error_CannotResolveMember);
                error.UserData[RuleUserDataKeys.ErrorObject] = parentExpr;
                Errors.Add(error);

                return false;
            }

            if (isInternal && ThisType.Assembly != methodAssembly)
            {
                // Can't access internal members except on the subject assembly.
                message = string.Format(CultureInfo.CurrentCulture, Messages.CannotAccessInternalMember, memberName, RuleDecompiler.DecompileType(targetType));
                ValidationError error = new ValidationError(message, ErrorNumbers.Error_CannotResolveMember);
                error.UserData[RuleUserDataKeys.ErrorObject] = parentExpr;
                Errors.Add(error);

                return false;
            }

            return true;
        }

        #region Field and property resolution

        internal MemberInfo ResolveFieldOrProperty(Type targetType, string name)
        {
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;
            if (AllowInternalMembers(targetType))
                bindingFlags |= BindingFlags.NonPublic;

            // Look up a field or property of the given name.
            MemberInfo[] results = targetType.GetMember(name, MemberTypes.Field | MemberTypes.Property, bindingFlags);

            if (results != null)
            {
                int numResults = results.Length;
                if (numResults == 1)
                {
                    // If we found exactly one, we're good.
                    return results[0];
                }
                else if (numResults > 1)
                {
                    // We may have found more than one property if it's overloaded.  If we find one without
                    // any parameters, return that one.
                    for (int i = 0; i < numResults; ++i)
                    {
                        MemberInfo member = results[i];
                        System.Diagnostics.Debug.Assert(member.MemberType == MemberTypes.Property, "only properties can be overloaded");

                        PropertyInfo pi = (PropertyInfo)member;
                        ParameterInfo[] parms = pi.GetIndexParameters();
                        if (parms == null || parms.Length == 0)
                        {
                            if (pi != null)
                            {
                                IsAuthorized(pi.PropertyType);
                            }
                            return pi;
                        }
                    }
                }
            }

            // If we didn't find it, and if the target type is an interface, try resolving a property
            // that may exist in its inheritance chain.  (Fields cannot appear on interfaces.)
            if (targetType.IsInterface)
                return ResolveProperty(targetType, name, bindingFlags);

            // Otherwise, it's no good.
            return null;
        }

        internal PropertyInfo ResolveProperty(Type targetType, string propertyName, BindingFlags bindingFlags)
        {

            PropertyInfo pi = GetProperty(targetType, propertyName, bindingFlags);
            if (pi == null && targetType.IsInterface)
            {
                Type[] parentInterfacesArray = targetType.GetInterfaces();
                List<Type> parentInterfaces = new List<Type>();
                parentInterfaces.AddRange(parentInterfacesArray);

                int index = 0;
                while (index < parentInterfaces.Count)
                {
                    pi = GetProperty(parentInterfaces[index], propertyName, bindingFlags);
                    if (pi != null)
                        break;

                    Type[] pInterfaces = parentInterfaces[index].GetInterfaces();
                    if (pInterfaces.Length > 0)
                        parentInterfaces.AddRange(pInterfaces);
                    ++index;
                }
            }

            if (pi != null)
            {
                IsAuthorized(pi.PropertyType);
            }
            return pi;
        }

        private static PropertyInfo GetProperty(Type targetType, string propertyName, BindingFlags bindingFlags)
        {
            // Properties may be overloaded (in VB), so we have to ---- out those that we can support,
            // i.e., those that have no parameters.

            MemberInfo[] members = targetType.GetMember(propertyName, MemberTypes.Property, bindingFlags);
            for (int m = 0; m < members.Length; ++m)
            {
                PropertyInfo pi = (PropertyInfo)members[m];

                ParameterInfo[] parms = pi.GetIndexParameters();
                if (parms == null || parms.Length == 0)
                    return pi;
            }

            return null;
        }

        #endregion

        #region Method resolution

        private class Argument
        {
            internal CodeExpression expression;
            internal FieldDirection direction;
            internal Type type;

            internal Argument(CodeExpression expr, RuleValidation validation)
            {
                this.expression = expr;

                this.direction = FieldDirection.In;
                CodeDirectionExpression directionExpr = expr as CodeDirectionExpression;
                if (directionExpr != null)
                    this.direction = directionExpr.Direction;

                this.type = validation.ExpressionInfo(expr).ExpressionType;
            }

            internal Argument(Type type)
            {
                this.direction = FieldDirection.In;
                this.type = type;
            }
        }

        private class CandidateParameter
        {
            private Type type;
            private FieldDirection direction;

            internal CandidateParameter(Type type)
            {
                this.type = type;
                this.direction = FieldDirection.In;
            }

            internal CandidateParameter(ParameterInfo paramInfo)
            {
                this.direction = FieldDirection.In;
                if (paramInfo.IsOut)
                    this.direction = FieldDirection.Out;
                else if (paramInfo.ParameterType.IsByRef)
                    this.direction = FieldDirection.Ref;

                this.type = paramInfo.ParameterType;
            }

            internal bool Match(Argument argument, string methodName, int argPosition, out ValidationError error)
            {
                string message;

                // If we don't agree on the argument direction, this method is not a candidate.
                if (this.direction != argument.direction)
                {
                    string dirString = "";
                    switch (this.direction)
                    {
                        case FieldDirection.In:
                            dirString = "in"; // No localization required, this is a keyword.
                            break;
                        case FieldDirection.Out:
                            dirString = "out"; // No localization required, this is a keyword.
                            break;
                        case FieldDirection.Ref:
                            dirString = "ref"; // No localization required, this is a keyword.
                            break;
                    }

                    message = string.Format(CultureInfo.CurrentCulture, Messages.MethodDirectionMismatch, argPosition, methodName, dirString);
                    error = new ValidationError(message, ErrorNumbers.Error_MethodDirectionMismatch);

                    return false;
                }

                if (this.type.IsByRef && this.type != argument.type)
                {
                    // If the parameter is "ref" or "out", then the types must match exactly.
                    // If not, this method can't be a candidate.

                    message = string.Format(CultureInfo.CurrentCulture, Messages.MethodArgumentTypeMismatch, argPosition, methodName, RuleDecompiler.DecompileType(argument.type), RuleDecompiler.DecompileType(this.type));
                    error = new ValidationError(message, ErrorNumbers.Error_MethodArgumentTypeMismatch);

                    return false;
                }

                // If the argument type is not assignable to the corresponding parameter type,
                // this method can't be a candidate.
                if (!RuleValidation.TypesAreAssignable(argument.type, this.type, argument.expression, out error))
                {
                    if (error == null)
                    {
                        message = string.Format(CultureInfo.CurrentCulture, Messages.MethodArgumentTypeMismatch, argPosition, methodName, RuleDecompiler.DecompileType(argument.type), RuleDecompiler.DecompileType(this.type));
                        error = new ValidationError(message, ErrorNumbers.Error_MethodArgumentTypeMismatch);
                    }
                    return false;
                }

                // If we passed the above checks, this argument is a candidate match for the parameter.
                return true;
            }

            public override bool Equals(object obj)
            {
                CandidateParameter otherParam = obj as CandidateParameter;
                if (otherParam == null)
                    return false;

                return this.direction == otherParam.direction && this.type == otherParam.type;
            }

            public override int GetHashCode()
            {
                return this.direction.GetHashCode() ^ this.type.GetHashCode();
            }

            internal int CompareConversion(CandidateParameter otherParam, Argument argument)
            {
                // Return 0 if they are equal; 1 if this is better; -1 if this is worse.
                // This follows the C# specification 7.4.2.3
                int better = 1;
                int worse = -1;
                int equal = 0;

                // If the two candidate parameters have the same type, neither one is better.
                if (this.type == otherParam.type)
                    return equal;

                // If the argument type is the same as one of the parameter types, that parameter is better.
                if (argument.type == this.type)
                    return better;
                if (argument.type == otherParam.type)
                    return worse;

                // If this parameter can be converted to the other parameter, and not vice versa, then
                // this is a better conversion.  (And in the reverse situation, it's a worse conversion.)
                ValidationError dummy;
                bool thisConvertsToOther = RuleValidation.TypesAreAssignable(this.type, otherParam.type, null, out dummy);
                bool otherConvertsToThis = RuleValidation.TypesAreAssignable(otherParam.type, this.type, null, out dummy);
                if (thisConvertsToOther && !otherConvertsToThis)
                    return better;
                if (otherConvertsToThis && !thisConvertsToOther)
                    return worse;

                // See if one is a better sign-preserving conversion than the other.
                if (BetterSignedConversion(this.type, otherParam.type))
                    return better;
                if (BetterSignedConversion(otherParam.type, this.type))
                    return worse;

                // Otherwise, neither conversion is better.
                return equal;
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
            private static bool BetterSignedConversion(Type t1, Type t2)
            {
                TypeCode tc1 = Type.GetTypeCode(t1);
                TypeCode tc2 = Type.GetTypeCode(t2);

                switch (tc1)
                {
                    case TypeCode.SByte:
                        switch (tc2)
                        {
                            case TypeCode.Byte:
                            case TypeCode.UInt16:
                            case TypeCode.UInt32:
                            case TypeCode.UInt64:
                                // A conversion to sbyte is better than a conversion to an unsigned type.
                                return true;
                        }
                        break;

                    case TypeCode.Int16:
                        switch (tc2)
                        {
                            case TypeCode.UInt16:
                            case TypeCode.UInt32:
                            case TypeCode.UInt64:
                                // A conversion to short is better than a conversion to an unsigned type.
                                return true;
                        }
                        break;

                    case TypeCode.Int32:
                        if (tc2 == TypeCode.UInt32 || tc2 == TypeCode.UInt64)
                        {
                            // A conversion to int is better than a conversion to an unsigned type.
                            return true;
                        }
                        break;

                    case TypeCode.Int64:
                        if (tc2 == TypeCode.UInt64)
                        {
                            // A conversion to long is better than a conversion to an unsigned type.
                            return true;
                        }
                        break;

                    case TypeCode.Object:
                        // it is possible that the types are nullable
                        if (ConditionHelper.IsNullableValueType(t1))
                        {
                            t1 = t1.GetGenericArguments()[0];
                            // t2 may already be a value type
                            if (ConditionHelper.IsNullableValueType(t2))
                                t2 = t2.GetGenericArguments()[0];
                            return BetterSignedConversion(t1, t2);
                        }
                        return false;
                }

                return false;
            }
        }

        private class CandidateMember
        {
            internal enum Form
            {
                Normal,     // no "params" expansion
                Expanded    // matched only after "params" expansion
            }

            internal MemberInfo Member;
            private ParameterInfo[] memberParameters;
            private List<CandidateParameter> signature;
            private Form form;
            private static ParameterInfo[] noParameters = new ParameterInfo[0];
            private static List<CandidateParameter> noSignature = new List<CandidateParameter>();

            // Constructor for candidate methods with parameters.
            internal CandidateMember(MemberInfo member, ParameterInfo[] parameters, List<CandidateParameter> signature, Form form)
            {
                this.Member = member;
                this.memberParameters = parameters;
                this.signature = signature;
                this.form = form;
            }

            // Constructor for a candidate method that has no parameters.
            internal CandidateMember(MemberInfo member)
                : this(member, noParameters, noSignature, Form.Normal)
            {
            }

            internal bool IsExpanded
            {
                get { return form == Form.Expanded; }
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
            internal int CompareMember(Type targetType, CandidateMember other, List<Argument> arguments, RuleValidation validator)
            {
                int better = 1;
                int worse = -1;
                int equal = 0;

                // Methods in a base class are not candidates if any method in a derived class
                // is applicable.
                Type thisDeclaringType = this.Member.DeclaringType;
                Type otherDeclaringType = other.Member.DeclaringType;
                if (thisDeclaringType != otherDeclaringType)
                {
                    if (TypeProvider.IsAssignable(otherDeclaringType, thisDeclaringType))
                    {
                        // This declaring type can be converted to the other declaring type,
                        // which means this one is more derived.
                        return better;
                    }
                    else if (TypeProvider.IsAssignable(thisDeclaringType, otherDeclaringType))
                    {
                        // The other declaring type can be converted to this declaring type,
                        // which means the other one is more derived.
                        return worse;
                    }
                }

                System.Diagnostics.Debug.Assert(arguments.Count == this.signature.Count);
                System.Diagnostics.Debug.Assert(arguments.Count == other.signature.Count);

                bool hasAtLeastOneBetterConversion = false;
                bool hasAtLeastOneWorseConversion = false;
                bool signaturesAreIdentical = true;

                // pick non-extension methods over extension methods
                // if both are extension methods, then pick the one in the namespace closest to "this"
                ExtensionMethodInfo thisExtension = this.Member as ExtensionMethodInfo;
                ExtensionMethodInfo otherExtension = other.Member as ExtensionMethodInfo;
                if ((thisExtension == null) && (otherExtension != null))
                    return better;
                else if ((thisExtension != null) && (otherExtension == null))
                    return worse;
                else if ((thisExtension != null) && (otherExtension != null))
                {
                    // we have 2 extension methods, which one is better
                    string[] thisNameSpace = thisExtension.DeclaringType.FullName.Split('.');
                    string[] otherNameSpace = otherExtension.DeclaringType.FullName.Split('.');
                    string[] bestNameSpace = validator.thisType.FullName.Split('.');
                    int thisMatch = MatchNameSpace(thisNameSpace, bestNameSpace);
                    int otherMatch = MatchNameSpace(otherNameSpace, bestNameSpace);
                    if (thisMatch > otherMatch)
                        return better;
                    else if (thisMatch < otherMatch)
                        return worse;

                    // compare arguments, including the "this" argument
                    CandidateParameter thisDeclaringParam = new CandidateParameter(thisExtension.AssumedDeclaringType);
                    CandidateParameter otherDeclaringParam = new CandidateParameter(otherExtension.AssumedDeclaringType);
                    if (!thisDeclaringParam.Equals(otherDeclaringParam))
                    {
                        signaturesAreIdentical = false;
                        int conversionResult = thisDeclaringParam.CompareConversion(otherDeclaringParam, new Argument(targetType));
                        if (conversionResult < 0)
                        {
                            // A conversion was found that was worse, so this candidate is not better.
                            hasAtLeastOneWorseConversion = true;
                        }
                        else if (conversionResult > 0)
                        {
                            // This candidate had at least one conversion that was better.  (But
                            // we have to keep looking in case there's one that's worse.)
                            hasAtLeastOneBetterConversion = true;
                        }
                    }

                    // this check compares parameter lists correctly (see below)
                    for (int p = 0; p < arguments.Count; ++p)
                    {
                        CandidateParameter thisParam = this.signature[p];
                        CandidateParameter otherParam = other.signature[p];

                        if (!thisParam.Equals(otherParam))
                            signaturesAreIdentical = false;

                        int conversionResult = thisParam.CompareConversion(otherParam, arguments[p]);
                        if (conversionResult < 0)
                        {
                            // A conversion was found that was worse, so this candidate is not better.
                            hasAtLeastOneWorseConversion = true;
                        }
                        else if (conversionResult > 0)
                        {
                            // This candidate had at least one conversion that was better.  (But
                            // we have to keep looking in case there's one that's worse.)
                            hasAtLeastOneBetterConversion = true;
                        }
                    }
                    if (hasAtLeastOneBetterConversion && !hasAtLeastOneWorseConversion)
                    {
                        // At least one conversion was better than the "other" candidate
                        // and no other arguments were worse, so this one is better.
                        return better;
                    }
                    else if (!hasAtLeastOneBetterConversion && hasAtLeastOneWorseConversion)
                    {
                        // At least one conversion was worse than the "other" candidate
                        // and no other arguments were better, so this one is worse.
                        return worse;
                    }
                }
                else
                {
                    // NOTE: this is the original v1 code
                    // It doesn't check for worse parameters correctly.
                    // However, for backwards compatability, we can't change it
                    for (int p = 0; p < arguments.Count; ++p)
                    {
                        CandidateParameter thisParam = this.signature[p];
                        CandidateParameter otherParam = other.signature[p];

                        if (!thisParam.Equals(otherParam))
                            signaturesAreIdentical = false;

                        int conversionResult = thisParam.CompareConversion(otherParam, arguments[p]);
                        if (conversionResult < 0)
                        {
                            // A conversion was found that was worse, so this candidate is not better.
                            return worse;
                        }
                        else if (conversionResult > 0)
                        {
                            // This candidate had at least one conversion that was better.  (But
                            // we have to keep looking in case there's one that's worse.)
                            hasAtLeastOneBetterConversion = true;
                        }
                    }

                    if (hasAtLeastOneBetterConversion)
                    {
                        // At least one conversion was better than the "other" candidate, so this one
                        // is better.
                        return better;
                    }
                }

                if (signaturesAreIdentical)
                {
                    // The signatures were "tied".  Try some disambiguating rules for expanded signatures
                    // vs normal signatures.
                    if (this.form == Form.Normal && other.form == Form.Expanded)
                    {
                        // This candidate matched in its normal form, but the other one matched only after
                        // expansion of a params array.  This one is better.
                        return better;
                    }
                    else if (this.form == Form.Expanded && other.form == Form.Normal)
                    {
                        // This candidate matched in its expanded form, but the other one matched in its
                        // normal form.  The other one was better.
                        return worse;
                    }
                    else if (this.form == Form.Expanded && other.form == Form.Expanded)
                    {
                        // Both candidates matched in their expanded forms.  

                        int thisParameterCount = this.memberParameters.Length;
                        int otherParameterCount = other.memberParameters.Length;

                        if (thisParameterCount > otherParameterCount)
                        {
                            // This candidate had more declared parameters, so it is better.
                            return better;
                        }
                        else if (otherParameterCount > thisParameterCount)
                        {
                            // The other candidate had more declared parameters, so it was better.
                            return worse;
                        }
                    }
                }

                // Nothing worked, the two candidates are equally applicable.
                return equal;
            }

            private static int MatchNameSpace(string[] test, string[] reference)
            {
                // returns the number of strings in test that are the same as reference
                int i;
                int len = Math.Min(test.Length, reference.Length);
                for (i = 0; i < len; ++i)
                {
                    if (test[i] != reference[i])
                        break;
                }
                return i;
            }
        }

        // Get the candidate target types, ordered from most-derived to least-derived.
        private static List<Type> GetCandidateTargetTypes(Type targetType)
        {
            List<Type> candidateTypes;

            if (targetType.IsInterface)
            {
                candidateTypes = new List<Type>();
                candidateTypes.Add(targetType);

                // Add all base interfaces in its hierarchy to the candidate list.
                for (int i = 0; i < candidateTypes.Count; ++i)
                {
                    Type currentCandidate = candidateTypes[i];
                    candidateTypes.AddRange(currentCandidate.GetInterfaces());
                }

                // Finally, add "System.Object", since all types intrinsically derive from this.
                candidateTypes.Add(typeof(object));
            }
            else
            {
                // It was a class; just add the one class.
                candidateTypes = new List<Type>(1);
                candidateTypes.Add(targetType);
            }

            return candidateTypes;
        }

        private delegate ValidationError BuildArgCountMismatchError(string name, int numArguments);

        private static void EvaluateCandidate(List<CandidateMember> candidates, MemberInfo candidateMember, ParameterInfo[] parameters, List<Argument> arguments, out ValidationError error, BuildArgCountMismatchError buildArgCountMismatchError)
        {
            error = null;

            int numArguments = arguments.Count;
            string candidateName = candidateMember.Name;

            if (parameters == null || parameters.Length == 0)
            {
                // If there were no arguments supplied, and this method has no parameters,
                // then it's a candidate.  (It should be the only one.)
                if (numArguments == 0)
                {
                    candidates.Add(new CandidateMember(candidateMember));
                }
                else
                {
                    error = buildArgCountMismatchError(candidateName, numArguments);
                }
            }
            else
            {
                List<CandidateParameter> signature = new List<CandidateParameter>();

                int parameterCount = parameters.Length;

                int fixedParameterCount = parameterCount;

                // Check to see if the last parameter is (1) an array and (2) has a ParamArrayAttribute
                // (i.e., it is a "params" array).
                ParameterInfo lastParam = parameters[parameterCount - 1];
                if (lastParam.ParameterType.IsArray)
                {
                    object[] attrs = lastParam.GetCustomAttributes(typeof(ParamArrayAttribute), false);
                    if (attrs != null && attrs.Length > 0)
                        fixedParameterCount -= 1;
                }

                if (numArguments < fixedParameterCount)
                {
                    // Not enough arguments were passed for this to be a candidate.
                    error = buildArgCountMismatchError(candidateName, numArguments);

                    return;
                }
                else if (fixedParameterCount == parameterCount && numArguments != parameterCount)
                {
                    // Too many arguments were passed for this to be a candidate.
                    error = buildArgCountMismatchError(candidateName, numArguments);

                    return;
                }

                // For the fixed part of the method signature, make sure each argument can
                // be implicitly converted to the corresponding parameter.
                int p = 0;
                for (; p < fixedParameterCount; ++p)
                {
                    CandidateParameter candidateParam = new CandidateParameter(parameters[p]);
                    if (!candidateParam.Match(arguments[p], candidateName, p + 1, out error))
                        break; // argument #p didn't match

                    // If we get here, then so far so good.
                    signature.Add(candidateParam);
                }

                if (p != fixedParameterCount)
                {
                    // We didn't match all of the fixed part.  This method is not a candidate.
                    return;
                }

                if (fixedParameterCount < parameterCount)
                {
                    // The last parameter was a "params" array.  As long as zero or more arguments
                    // are assignable, it's a valid candidate in the expanded form.

                    CandidateMember candidateMethod = null;

                    if (numArguments == fixedParameterCount)
                    {
                        // Zero arguments were passed as the params array.  The method is a candidate
                        // in its expanded form.
                        candidateMethod = new CandidateMember(candidateMember, parameters, signature, CandidateMember.Form.Expanded);
                    }
                    else if (numArguments == parameterCount)
                    {
                        // Special case:  one argument was passed as the params array.
                        CandidateParameter candidateParam = new CandidateParameter(lastParam);
                        if (candidateParam.Match(arguments[p], candidateName, p + 1, out error))
                        {
                            // It was the same array type as the params array, so the candidate 
                            // matched in its normal form.
                            signature.Add(candidateParam);
                            candidateMethod = new CandidateMember(candidateMember, parameters, signature, CandidateMember.Form.Normal);
                        }
                    }

                    if (candidateMethod == null)
                    {
                        // One or more arguments were passed as the params array.  As long
                        // as they match the element type, this method is a candidate.
                        CandidateParameter candidateParam = new CandidateParameter(lastParam.ParameterType.GetElementType());

                        for (; p < numArguments; ++p)
                        {
                            if (!candidateParam.Match(arguments[p], candidateName, p + 1, out error))
                            {
                                // Not all of the trailing arguments matched the params array's element type;
                                // this cannot be a candidate.
                                return;
                            }

                            // If we get here, then so far so good.
                            signature.Add(candidateParam);
                        }

                        // All the trailing arguments matched, so this is a candidate in the expanded form.
                        candidateMethod = new CandidateMember(candidateMember, parameters, signature, CandidateMember.Form.Expanded);
                    }

                    candidates.Add(candidateMethod);
                }
                else
                {
                    // The last parameter wasn't "params".  This candidate matched in its normal form.
                    candidates.Add(new CandidateMember(candidateMember, parameters, signature, CandidateMember.Form.Normal));
                }
            }
        }

        private CandidateMember FindBestCandidate(Type targetType, List<CandidateMember> candidates, List<Argument> arguments)
        {
            int numCandidates = candidates.Count;
            Debug.Assert(numCandidates > 0, "expected at least one candidate");

            // Start by assuming the first candidate is the best one.
            List<CandidateMember> bestCandidates = new List<CandidateMember>(1);
            bestCandidates.Add(candidates[0]);

            // Go through the rest of the candidates and try to find a better one.  (If
            // there are no more candidates, then there was only one, and that's the right
            // one.)
            for (int i = 1; i < numCandidates; ++i)
            {
                CandidateMember newCandidate = candidates[i];

                // Compare this new candidate one if the current "best" ones.  (If there
                // is currently more than one best candidate, then so far its ambiguous, which 
                // means all the best ones are equally good.  Thus if this new candidate
                // is better than one, it's better than all.
                CandidateMember bestCandidate = bestCandidates[0];

                int comparison = newCandidate.CompareMember(targetType, bestCandidate, arguments, this);
                if (comparison > 0)
                {
                    // The new one was better than at least one of the best ones.  It
                    // becomes the new best one.
                    bestCandidates.Clear();
                    bestCandidates.Add(newCandidate);
                }
                else if (comparison == 0)
                {
                    // The new one was no better, so add it to the list of current best.
                    // (Unless we find a better one, it's ambiguous so far.)
                    bestCandidates.Add(newCandidate);
                }
            }

            if (bestCandidates.Count == 1)
            {
                // Good, there was exactly one best match.
                return bestCandidates[0];
            }

            // Otherwise, it must have been ambiguous.
            return null;
        }

        internal MethodInfo FindBestCandidate(Type targetType, List<MethodInfo> methods, params Type[] types)
        {
            List<Argument> arguments = new List<Argument>();
            foreach (Type t in types)
                arguments.Add(new Argument(t));

            List<CandidateMember> candidates = new List<CandidateMember>(methods.Count);
            foreach (MethodInfo method in methods)
            {
                ValidationError tempError = null;
                EvaluateCandidate(candidates, method, method.GetParameters(), arguments, out tempError,
                                  delegate(string name, int numArguments)
                                  {
                                      string message = string.Format(CultureInfo.CurrentCulture, Messages.MethodArgCountMismatch, name, numArguments);
                                      return new ValidationError(message, ErrorNumbers.Error_MethodArgCountMismatch);
                                  });
            }
            if (candidates.Count == 0)
            {
                // nothing looks useful
                return null;
            }
            CandidateMember result = FindBestCandidate(targetType, candidates, arguments);
            return (result != null) ? (MethodInfo)result.Member : null;
        }

        internal RuleConstructorExpressionInfo ResolveConstructor(Type targetType, BindingFlags constructorBindingFlags, List<CodeExpression> argumentExprs, out ValidationError error)
        {
            string message;

            List<Argument> arguments = new List<Argument>(argumentExprs.Count);
            foreach (CodeExpression argumentExpr in argumentExprs)
                arguments.Add(new Argument(argumentExpr, this));

            // Get the candidate types and all candidate methods contained in them.
            List<Type> candidateTypes = GetCandidateTargetTypes(targetType);
            // Get all methods by this name...
            List<ConstructorInfo> constructors = GetConstructors(candidateTypes, constructorBindingFlags);
            if (constructors.Count == 0)
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.UnknownConstructor, RuleDecompiler.DecompileType(targetType));
                error = new ValidationError(message, ErrorNumbers.Error_MethodNotExists);
                return null;
            }

            // Cull the list of methods to those which match the supplied arguments.
            List<CandidateMember> candidateConstructors = GetCandidateConstructors(constructors, arguments, out error);

            // If the list is null, then no candidates matched.
            if (candidateConstructors == null)
                return null;

            // We found candidate methods in this type.
            CandidateMember bestCandidate = FindBestCandidate(targetType, candidateConstructors, arguments);

            if (bestCandidate == null)
            {
                // It was ambiguous.
                message = string.Format(CultureInfo.CurrentCulture, Messages.AmbiguousConstructor, RuleDecompiler.DecompileType(targetType));
                error = new ValidationError(message, ErrorNumbers.Error_CannotResolveMember);
                return null;
            }

            // We found the best match.
            return new RuleConstructorExpressionInfo((ConstructorInfo)bestCandidate.Member, bestCandidate.IsExpanded);
        }

        internal RuleMethodInvokeExpressionInfo ResolveMethod(Type targetType, string methodName, BindingFlags methodBindingFlags, List<CodeExpression> argumentExprs, out ValidationError error)
        {
            string message;

            List<Argument> arguments = new List<Argument>(argumentExprs.Count);
            foreach (CodeExpression argumentExpr in argumentExprs)
                arguments.Add(new Argument(argumentExpr, this));

            // Get the candidate types and all candidate methods contained in them.
            List<Type> candidateTypes = GetCandidateTargetTypes(targetType);
            // Get all methods by this name...
            List<MethodInfo> methods = GetNamedMethods(candidateTypes, methodName, methodBindingFlags);
            if (methods.Count == 0)
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.UnknownMethod, methodName, RuleDecompiler.DecompileType(targetType));
                error = new ValidationError(message, ErrorNumbers.Error_MethodNotExists);
                return null;
            }

            // Cull the list of methods to those which match the supplied arguments.
            List<CandidateMember> candidateMethods = GetCandidateMethods(methodName, methods, arguments, out error);

            // If the list is null, then no candidates matched.
            if (candidateMethods == null)
                return null;

            // We found candidate methods in this type.
            CandidateMember bestCandidate = FindBestCandidate(targetType, candidateMethods, arguments);

            if (bestCandidate == null)
            {
                // It was ambiguous.
                message = string.Format(CultureInfo.CurrentCulture, Messages.AmbiguousMatch, methodName);
                error = new ValidationError(message, ErrorNumbers.Error_CannotResolveMember);

                return null;
            }

            // We found the best match.
            MethodInfo theMethod = (MethodInfo)bestCandidate.Member;
            if (theMethod != null)
            {
                IsAuthorized(theMethod.ReturnType);
            }
            return new RuleMethodInvokeExpressionInfo(theMethod, bestCandidate.IsExpanded);
        }

        internal static List<ConstructorInfo> GetConstructors(List<Type> targetTypes, BindingFlags constructorBindingFlags)
        {
            List<ConstructorInfo> methods = new List<ConstructorInfo>();

            for (int t = 0; t < targetTypes.Count; ++t)
            {
                Type targetType = targetTypes[t];

                // Go through all the constructors on the target type
                ConstructorInfo[] members = targetType.GetConstructors(constructorBindingFlags);
                for (int m = 0; m < members.Length; ++m)
                {
                    ConstructorInfo constructor = members[m];
                    if (constructor.IsGenericMethod) // skip generic constructors
                        continue;
                    if (constructor.IsStatic) // skip static constructors
                        continue;
                    if (constructor.IsPrivate) // skip private constructors
                        continue;
                    if (constructor.IsFamily) // skip internal constructors
                        continue;
                    methods.Add(constructor);
                }
            }
            return methods;
        }

        private List<MethodInfo> GetNamedMethods(List<Type> targetTypes, string methodName, BindingFlags methodBindingFlags)
        {
            List<MethodInfo> methods = new List<MethodInfo>();
            List<ExtensionMethodInfo> currentExtensionMethods = ExtensionMethods;
            for (int t = 0; t < targetTypes.Count; ++t)
            {
                Type targetType = targetTypes[t];

                // Go through all the methods on the target type that have matching names.
                MemberInfo[] members = targetType.GetMember(methodName, MemberTypes.Method, methodBindingFlags);
                for (int m = 0; m < members.Length; ++m)
                {
                    MethodInfo method = (MethodInfo)members[m];
                    if (!method.IsGenericMethod) // skip generic methods
                        methods.Add(method);
                }

                // add in any extension methods that match
                foreach (ExtensionMethodInfo extension in currentExtensionMethods)
                {
                    // does it have the right name and is the type compatible
                    ValidationError error;
                    if ((extension.Name == methodName) &&
                        TypesAreAssignable(targetType, extension.AssumedDeclaringType, null, out error))
                    {
                        // possible match
                        methods.Add(extension);
                    }
                }
            }

            return methods;
        }

        private List<ExtensionMethodInfo> extensionMethods;
        private List<Assembly> seenAssemblies;
        private const string ExtensionAttributeFullName = "System.Runtime.CompilerServices.ExtensionAttribute, " + AssemblyRef.SystemCore;
        private Type extensionAttribute;

        private static Type defaultExtensionAttribute = GetDefaultExtensionAttribute();

        private static Type GetDefaultExtensionAttribute()
        {
            return Type.GetType(ExtensionAttributeFullName, false);
        }

        // The extensionAttributeType may still be null after calling this method
        // if, for example, we are in a 3.0 SP2 environment.
        private void SetExtensionAttribute()
        {
            // use the TypeProvider first
            extensionAttribute = typeProvider.GetType(ExtensionAttributeFullName, false);
            if (extensionAttribute == null)
            {
                extensionAttribute = defaultExtensionAttribute;
            }
        }

        internal List<ExtensionMethodInfo> ExtensionMethods
        {
            get
            {
                if (extensionMethods == null)
                    DetermineExtensionMethods();

                return extensionMethods;
            }
        }

        private void DetermineExtensionMethods()
        {
            extensionMethods = new List<ExtensionMethodInfo>();

            SetExtensionAttribute();
            if (extensionAttribute != null)
            {
                seenAssemblies = new List<Assembly>();
                Assembly localAssembly = typeProvider.LocalAssembly;
                if (localAssembly != null)
                {
                    DetermineExtensionMethods(localAssembly);
                    foreach (Assembly a in typeProvider.ReferencedAssemblies)
                        DetermineExtensionMethods(a);
                }
                else
                {
                    // probably at design-time, nothing compiled yet
                    // go through all types it knows about
                    DetermineExtensionMethods(typeProvider.GetTypes());
                }
            }
        }

        internal void DetermineExtensionMethods(Assembly assembly)
        {
            // when this method is called outside of this class, we must have tried 
            // getting ExtensionMethods. So we must have tried setting extensionAttributeType.

            if (extensionAttribute != null)
            {
                if ((assembly != null) && (!seenAssemblies.Contains(assembly)))
                {
                    seenAssemblies.Add(assembly);
                    if (IsMarkedExtension(assembly))
                    {
                        Type[] types;
                        try
                        {
                            types = assembly.GetTypes();
                        }
                        catch (ReflectionTypeLoadException e)
                        {
                            // problems loading all the types, take what we can get
                            // some types will be null
                            types = e.Types;
                        }
                        DetermineExtensionMethods(types);
                    }
                }
            }
        }

        private void DetermineExtensionMethods(Type[] types)
        {
            foreach (Type type in types)
            {
                // static classes are defined as "abstract sealed"
                // Note: VB doesn't support static classes, so the modules are only defined as "sealed"
                if ((type != null) && (type.IsPublic || type.IsNestedPublic) && (type.IsSealed) && (IsMarkedExtension(type)))
                {
                    // looks like a class containing extension methods, let's find them
                    MethodInfo[] staticMethods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
                    foreach (MethodInfo mi in staticMethods)
                    {
                        // skip generic methods
                        if ((mi.IsStatic) && !(mi.IsGenericMethod) && (IsMarkedExtension(mi)))
                        {
                            ParameterInfo[] parms = mi.GetParameters();
                            if (parms.Length > 0 && parms[0].ParameterType != null)
                            {
                                extensionMethods.Add(new ExtensionMethodInfo(mi, parms));
                            }
                        }
                    }
                }
            }
        }

        private bool IsMarkedExtension(Assembly assembly)
        {
            if (extensionAttribute != null)
            {
                object[] objAttrs = assembly.GetCustomAttributes(extensionAttribute, false);
                if (objAttrs != null && objAttrs.Length > 0)
                    return true;
            }

            return false;
        }

        private bool IsMarkedExtension(Type type)
        {
            if (extensionAttribute != null)
            {
                object[] objAttrs = type.GetCustomAttributes(extensionAttribute, false);
                if (objAttrs != null && objAttrs.Length > 0)
                    return true;
            }

            return false;
        }

        private bool IsMarkedExtension(MethodInfo mi)
        {
            if (extensionAttribute != null)
            {
                object[] objAttrs = mi.GetCustomAttributes(extensionAttribute, false);
                if (objAttrs != null && objAttrs.Length > 0)
                    return true;
            }

            return false;
        }

        static List<CandidateMember> GetCandidateMethods(string methodName, List<MethodInfo> methods, List<Argument> arguments, out ValidationError error)
        {
            List<CandidateMember> candidates = new List<CandidateMember>();

            error = null;

            int errorCount = 0;
            foreach (MethodInfo method in methods)
            {
                ValidationError tempError = null;
                EvaluateCandidate(candidates, method, method.GetParameters(), arguments, out tempError,
                                  delegate(string name, int numArguments)
                                  {
                                      string message = string.Format(CultureInfo.CurrentCulture, Messages.MethodArgCountMismatch, name, numArguments);
                                      return new ValidationError(message, ErrorNumbers.Error_MethodArgCountMismatch);
                                  });

                error = tempError;
                if (tempError != null)
                    ++errorCount;
            }

            if (candidates.Count == 0)
            {
                // No candidates were found.

                if (errorCount > 1)
                {
                    // If multiple candidates generated errors, then use a more generic error that says
                    // we couldn't find a matching overload.
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.MethodOverloadNotFound, methodName);
                    error = new ValidationError(message, ErrorNumbers.Error_MethodOverloadNotFound);
                }

                return null;
            }
            else
            {
                // If there are any candidates, then wipe out any errors left over from any mismatches.
                error = null;
            }

            return candidates;
        }

        static List<CandidateMember> GetCandidateConstructors(List<ConstructorInfo> constructors, List<Argument> arguments, out ValidationError error)
        {
            List<CandidateMember> candidates = new List<CandidateMember>();

            error = null;

            int errorCount = 0;
            foreach (ConstructorInfo method in constructors)
            {
                ValidationError tempError = null;
                EvaluateCandidate(candidates, method, method.GetParameters(), arguments, out tempError,
                                  delegate(string name, int numArguments)
                                  {
                                      string message = string.Format(CultureInfo.CurrentCulture, Messages.MethodArgCountMismatch, name, numArguments);
                                      return new ValidationError(message, ErrorNumbers.Error_MethodArgCountMismatch);
                                  });

                error = tempError;
                if (tempError != null)
                    ++errorCount;
            }

            if (candidates.Count == 0)
            {
                // No candidates were found.

                if (errorCount > 1)
                {
                    // If multiple candidates generated errors, then use a more generic error that says
                    // we couldn't find a matching overload.
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.ConstructorOverloadNotFound);
                    error = new ValidationError(message, ErrorNumbers.Error_MethodOverloadNotFound);
                }

                return null;
            }
            else
            {
                // If there are any candidates, then wipe out any errors left over from any mismatches.
                error = null;
            }

            return candidates;
        }

        internal RulePropertyExpressionInfo ResolveIndexerProperty(Type targetType, BindingFlags bindingFlags, List<CodeExpression> argumentExprs, out ValidationError error)
        {
            string message;

            int numArgs = argumentExprs.Count;

            if (numArgs < 1)
            {
                // Must have at least one indexer!
                message = string.Format(CultureInfo.CurrentCulture, Messages.IndexerCountMismatch, numArgs);
                error = new ValidationError(message, ErrorNumbers.Error_IndexerCountMismatch);
                return null;
            }

            List<Argument> arguments = new List<Argument>(numArgs);
            foreach (CodeExpression argumentExpr in argumentExprs)
                arguments.Add(new Argument(argumentExpr, this));

            // Get the candidate types and all the candidate indexer properties contained in them.
            List<Type> candidateTypes = GetCandidateTargetTypes(targetType);
            List<PropertyInfo> indexerProperties = GetIndexerProperties(candidateTypes, bindingFlags);
            if (indexerProperties.Count == 0)
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.IndexerNotFound, RuleDecompiler.DecompileType(targetType));
                error = new ValidationError(message, ErrorNumbers.Error_IndexerNotFound);
                return null;
            }

            List<CandidateMember> candidateIndexers = GetCandidateIndexers(indexerProperties, arguments, out error);

            // If the list is null, then no candidates matched.
            if (candidateIndexers == null)
                return null;

            // We found candidate methods in this type.
            CandidateMember bestCandidate = FindBestCandidate(targetType, candidateIndexers, arguments);

            if (bestCandidate == null)
            {
                // It was ambiguous.
                message = string.Format(CultureInfo.CurrentCulture, Messages.AmbiguousIndexerMatch);
                error = new ValidationError(message, ErrorNumbers.Error_CannotResolveMember);

                return null;
            }

            // We found the best match.
            PropertyInfo pi = (PropertyInfo)bestCandidate.Member;
            if (pi != null)
            {
                IsAuthorized(pi.PropertyType);
            }
            return new RulePropertyExpressionInfo(pi, pi.PropertyType, bestCandidate.IsExpanded);
        }

        private static List<PropertyInfo> GetIndexerProperties(List<Type> candidateTypes, BindingFlags bindingFlags)
        {
            List<PropertyInfo> indexerProperties = new List<PropertyInfo>();

            foreach (Type targetType in candidateTypes)
            {
                object[] attrs = targetType.GetCustomAttributes(typeof(DefaultMemberAttribute), true);
                if (attrs == null || attrs.Length == 0)
                    continue;

                DefaultMemberAttribute[] defaultMemberAttrs = (DefaultMemberAttribute[])attrs;

                PropertyInfo[] properties = targetType.GetProperties(bindingFlags);
                for (int p = 0; p < properties.Length; ++p)
                {
                    PropertyInfo pi = properties[p];

                    // Select only those properties whose name matches the default name.
                    bool matchedName = false;
                    for (int dm = 0; dm < defaultMemberAttrs.Length; ++dm)
                    {
                        if (defaultMemberAttrs[dm].MemberName == pi.Name)
                        {
                            matchedName = true;
                            break;
                        }
                    }

                    if (matchedName)
                    {
                        // We matched the name...
                        ParameterInfo[] indexerParameters = pi.GetIndexParameters();
                        if (indexerParameters.Length > 0)
                        {
                            // ... and have indexer parameters; therefore, this is
                            // an interesting property.
                            indexerProperties.Add(pi);
                        }
                    }
                }
            }

            return indexerProperties;
        }

        private static List<CandidateMember> GetCandidateIndexers(List<PropertyInfo> indexerProperties, List<Argument> arguments, out ValidationError error)
        {
            List<CandidateMember> candidates = new List<CandidateMember>();

            error = null;

            int errorCount = 0;
            foreach (PropertyInfo indexerProp in indexerProperties)
            {
                ValidationError tempError = null;
                EvaluateCandidate(candidates, indexerProp, indexerProp.GetIndexParameters(), arguments, out tempError,
                                  delegate(string propName, int numArguments)
                                  {
                                      string message = string.Format(CultureInfo.CurrentCulture, Messages.IndexerCountMismatch, numArguments);
                                      return new ValidationError(message, ErrorNumbers.Error_IndexerCountMismatch);
                                  });

                error = tempError;
                if (tempError != null)
                    ++errorCount;
            }

            if (candidates.Count == 0)
            {
                // No candidates were found.

                if (errorCount > 1)
                {
                    // If multiple candidates generated errors, then use a more generic error that says
                    // we couldn't find a matching overload.
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.IndexerOverloadNotFound);
                    error = new ValidationError(message, ErrorNumbers.Error_IndexerOverloadNotFound);
                }

                return null;
            }
            else
            {
                // If there are any candidates, then wipe out any errors left over from any mismatches.
                error = null;
            }

            return candidates;
        }

        #endregion

        #region Type resolution

        internal void AddTypeReference(CodeTypeReference typeRef, Type type)
        {
            typeRefMap[typeRef] = type;
        }

        internal Type ResolveType(CodeTypeReference typeRef)
        {
            Type resultType = null;

            if (!typeRefMap.TryGetValue(typeRef, out resultType))
            {
                string message;

                resultType = FindType(typeRef.BaseType);

                if (resultType == null)
                {
                    // check if we have a qualifiedname saved, and if we do, use it
                    string qualifiedName = typeRef.UserData[RuleUserDataKeys.QualifiedName] as string;
                    resultType = ResolveType(qualifiedName);
                    if (resultType != null)
                    {
                        // qualified name returned the complete type, save it and we're done
                        typeRefMap.Add(typeRef, resultType);
                        return resultType;
                    }
                    message = string.Format(CultureInfo.CurrentCulture, Messages.UnknownType, typeRef.BaseType);
                    ValidationError error = new ValidationError(message, ErrorNumbers.Error_UnableToResolveType);
                    error.UserData[RuleUserDataKeys.ErrorObject] = typeRef;
                    Errors.Add(error);
                    return null;
                }

                // Handle generic type arguments.
                if (typeRef.TypeArguments.Count > 0)
                {
                    Type[] typeArguments = new Type[typeRef.TypeArguments.Count];
                    for (int i = 0; i < typeRef.TypeArguments.Count; ++i)
                    {
                        // design-time types don't have fully-qualified names, so when they are
                        // used in a generic CodeTypeReference constructor leaves them with []
                        // surrounding them. Remove the [] if possible
                        CodeTypeReference arg = typeRef.TypeArguments[i];
                        if (arg.BaseType.StartsWith("[", StringComparison.Ordinal))
                            arg.BaseType = arg.BaseType.Substring(1, arg.BaseType.Length - 2);

                        typeArguments[i] = ResolveType(arg);
                        if (typeArguments[i] == null)
                            return null;
                    }

                    resultType = resultType.MakeGenericType(typeArguments);
                    if (resultType == null)
                    {
                        StringBuilder sb = new StringBuilder(typeRef.BaseType);
                        string prefix = "<";
                        foreach (Type t in typeArguments)
                        {
                            sb.Append(prefix);
                            prefix = ",";
                            sb.Append(RuleDecompiler.DecompileType(t));
                        }
                        sb.Append(">");
                        message = string.Format(CultureInfo.CurrentCulture, Messages.UnknownGenericType, sb.ToString());
                        ValidationError error = new ValidationError(message, ErrorNumbers.Error_UnableToResolveType);
                        error.UserData[RuleUserDataKeys.ErrorObject] = typeRef;
                        Errors.Add(error);
                        return null;
                    }
                }


                if (resultType != null)
                {
                    CodeTypeReference arrayTypeRef = typeRef;
                    if (arrayTypeRef.ArrayRank > 0)
                    {
                        do
                        {
                            resultType = (arrayTypeRef.ArrayRank == 1) ? resultType.MakeArrayType() : resultType.MakeArrayType(arrayTypeRef.ArrayRank);

                            arrayTypeRef = arrayTypeRef.ArrayElementType;
                        } while (arrayTypeRef.ArrayRank > 0);
                    }
                }

                if (resultType != null)
                {
                    typeRefMap.Add(typeRef, resultType);

                    // at runtime we may not have the assembly loaded, so keep the fully qualified name around
                    typeRef.UserData[RuleUserDataKeys.QualifiedName] = resultType.AssemblyQualifiedName;
                }
            }

            return resultType;
        }

        internal Type ResolveType(string qualifiedName)
        {
            Type resultType = null;
            if (qualifiedName != null)
            {
                resultType = typeProvider.GetType(qualifiedName, false);

                // if the Typeprovider can't find it, use the framework, 
                // since it should be an AssemblyQualifiedName
                if (resultType == null)
                    resultType = Type.GetType(qualifiedName, false);

            }
            return resultType;
        }

        private Type FindType(string typeName)
        {
            if (typeName == null)
                throw new ArgumentNullException("typeName");

            Type type = null;

            // do we know about this type
            if (!typesUsed.TryGetValue(typeName, out type))
            {
                type = typeProvider.GetType(typeName, false);

                if (type != null)
                {
                    typesUsed.Add(typeName, type);

                    IsAuthorized(type);
                }
            }

            return type;
        }

        internal void IsAuthorized(Type type)
        {
            Debug.Assert(!type.IsPointer && !type.IsByRef,
            "IsAuthorized should not be called for a type that is a pointer or passed by reference : " + type.AssemblyQualifiedName);
            if (checkStaticType)
            {
                if (authorizedTypes == null)
                {
                    ValidationError error = new ValidationError(Messages.Error_ConfigFileMissingOrInvalid, ErrorNumbers.Error_ConfigFileMissingOrInvalid);
                    Errors.Add(error);
                }
                else
                {
                    while (type.IsArray)
                    {
                        type = type.GetElementType();
                    }
                    if (type.IsGenericType)
                    {
                        IsAuthorizedSimpleType(type.GetGenericTypeDefinition());
                        Type[] typeArguments = type.GetGenericArguments();
                        foreach (Type t in typeArguments)
                        {
                            IsAuthorized(t);
                        }
                    }
                    else
                    {
                        IsAuthorizedSimpleType(type);
                    }
                }
            }
        }

        void IsAuthorizedSimpleType(Type type)
        {
            Debug.Assert((!type.IsGenericType || type.IsGenericTypeDefinition) && !type.HasElementType,
                "IsAuthorizedSimpleType should not be called for a partially specialized generic type or a type that encompasses or refers to another type : " +
                type.AssemblyQualifiedName);

            string qualifiedName = type.AssemblyQualifiedName;

            if (!typesUsedAuthorized.ContainsKey(qualifiedName))
            {
                bool authorized = false;
                foreach (AuthorizedType authorizedType in authorizedTypes)
                {
                    if (authorizedType.RegularExpression.IsMatch(qualifiedName))
                    {
                        authorized = (String.Compare(bool.TrueString, authorizedType.Authorized, StringComparison.OrdinalIgnoreCase) == 0);
                        if (!authorized)
                            break;
                    }
                }
                if (!authorized)
                {
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.Error_TypeNotAuthorized, type.FullName);
                    ValidationError error = new ValidationError(message, ErrorNumbers.Error_TypeNotAuthorized);
                    error.UserData[RuleUserDataKeys.ErrorObject] = type;
                    Errors.Add(error);
                }
                else
                {
                    typesUsedAuthorized.Add(qualifiedName, type);
                }
            }
        }

        #endregion

        #endregion
    }
    #endregion RuleValidator
}
