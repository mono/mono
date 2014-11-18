//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
// ***NOTE*** If this code is changed, make corresponding changes in System.ServiceModel.CodeGenerator also
namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Security;

    class CodeGenerator
    {
        [Fx.Tag.SecurityNote(Critical = "Static fields are marked SecurityCritical or readonly to prevent"
            + " data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        static MethodInfo getTypeFromHandle;
        static MethodInfo GetTypeFromHandle
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical getTypeFromHandle field.",
                Safe = "Get-only properties only needs to be protected for write; initialized in getter if null.")]
            [SecuritySafeCritical]
            get
            {
                if (getTypeFromHandle == null)
                    getTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle");
                return getTypeFromHandle;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Static fields are marked SecurityCritical or readonly to prevent"
            + " data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        static MethodInfo stringFormat;
        static MethodInfo StringFormat
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical stringFormat field.",
                Safe = "Get-only properties only needs to be protected for write; initialized in getter if null.")]
            [SecuritySafeCritical]
            get
            {
                if (stringFormat == null)
                    stringFormat = typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(object[]) });
                return stringFormat;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Static fields are marked SecurityCritical or readonly to prevent"
            + " data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        static MethodInfo stringConcat2;
        static MethodInfo StringConcat2
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical stringConcat2 field.",
                Safe = "Get-only properties only needs to be protected for write; initialized in getter if null.")]
            [SecuritySafeCritical]
            get
            {
                if (stringConcat2 == null)
                    stringConcat2 = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });
                return stringConcat2;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Static fields are marked SecurityCritical or readonly to prevent"
            + " data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        static MethodInfo stringConcat3;
        static MethodInfo StringConcat3
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical stringConcat3 field.",
                Safe = "Get-only properties only needs to be protected for write; initialized in getter if null.")]
            [SecuritySafeCritical]
            get
            {
                if (stringConcat3 == null)
                    stringConcat3 = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string), typeof(string) });
                return stringConcat3;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Static fields are marked SecurityCritical or readonly to prevent"
            + " data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        static MethodInfo objectToString;
        static MethodInfo ObjectToString
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical objectToString field.",
                Safe = "Get-only properties only needs to be protected for write; initialized in getter if null.")]
            [SecuritySafeCritical]
            get
            {
                if (objectToString == null)
                    objectToString = typeof(object).GetMethod("ToString", new Type[0]);
                return objectToString;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Static fields are marked SecurityCritical or readonly to prevent"
            + " data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        static MethodInfo objectEquals;
        static MethodInfo ObjectEquals
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical objectEquals field.",
                Safe = "Get-only properties only needs to be protected for write; initialized in getter if null.")]
            [SecuritySafeCritical]
            get
            {
                if (objectEquals == null)
                    objectEquals = Globals.TypeOfObject.GetMethod("Equals", BindingFlags.Public | BindingFlags.Static);
                return objectEquals;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Static fields are marked SecurityCritical or readonly to prevent"
            + " data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        static MethodInfo arraySetValue;
        static MethodInfo ArraySetValue
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical arraySetValue field.",
                Safe = "Get-only properties only needs to be protected for write; initialized in getter if null.")]
            [SecuritySafeCritical]
            get
            {
                if (arraySetValue == null)
                    arraySetValue = typeof(Array).GetMethod("SetValue", new Type[] { typeof(object), typeof(int) });
                return arraySetValue;
            }
        }

        Type delegateType;

#if USE_REFEMIT
        AssemblyBuilder assemblyBuilder;
        ModuleBuilder moduleBuilder;
        TypeBuilder typeBuilder;
        static int typeCounter;
        MethodBuilder methodBuilder;
#else
        [Fx.Tag.SecurityNote(Critical = "Static fields are marked SecurityCritical or readonly to prevent"
            + " data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        static Module serializationModule;
        static Module SerializationModule
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical serializationModule field.",
                Safe = "Get-only properties only needs to be protected for write; initialized in getter if null.")]
            [SecuritySafeCritical]
            get
            {
                if (serializationModule == null)
                {
                    serializationModule = typeof(CodeGenerator).Module;   // could to be replaced by different dll that has SkipVerification set to false
                }
                return serializationModule;
            }
        }
        DynamicMethod dynamicMethod;
#endif

        ILGenerator ilGen;
        ArrayList argList;
        Stack blockStack;
        Label methodEndLabel;

        LocalBuilder stringFormatArray;

        Hashtable localNames;
        int lineNo = 1;
        enum CodeGenTrace { None, Save, Tron };
        CodeGenTrace codeGenTrace;

        internal CodeGenerator()
        {
            SourceSwitch codeGenSwitch = SerializationTrace.CodeGenerationSwitch;
            if ((codeGenSwitch.Level & SourceLevels.Verbose) == SourceLevels.Verbose)
                codeGenTrace = CodeGenTrace.Tron;
            else if ((codeGenSwitch.Level & SourceLevels.Information) == SourceLevels.Information)
                codeGenTrace = CodeGenTrace.Save;
            else
                codeGenTrace = CodeGenTrace.None;
        }

#if !USE_REFEMIT
        internal void BeginMethod(DynamicMethod dynamicMethod, Type delegateType, string methodName, Type[] argTypes, bool allowPrivateMemberAccess)
        {
            this.dynamicMethod = dynamicMethod;
            this.ilGen = this.dynamicMethod.GetILGenerator();
            this.delegateType = delegateType;

            InitILGeneration(methodName, argTypes);
        }
#endif

        internal void BeginMethod(string methodName, Type delegateType, bool allowPrivateMemberAccess)
        {
            MethodInfo signature = delegateType.GetMethod("Invoke");
            ParameterInfo[] parameters = signature.GetParameters();
            Type[] paramTypes = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
                paramTypes[i] = parameters[i].ParameterType;
            BeginMethod(signature.ReturnType, methodName, paramTypes, allowPrivateMemberAccess);
            this.delegateType = delegateType;
        }

        void BeginMethod(Type returnType, string methodName, Type[] argTypes, bool allowPrivateMemberAccess)
        {
#if USE_REFEMIT
            string typeName = "Type" + (typeCounter++);
            InitAssemblyBuilder(typeName + "." + methodName);
            this.typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public);
            this.methodBuilder = typeBuilder.DefineMethod(methodName, MethodAttributes.Public|MethodAttributes.Static, returnType, argTypes);
            this.ilGen = this.methodBuilder.GetILGenerator();
#else
            this.dynamicMethod = new DynamicMethod(methodName, returnType, argTypes, SerializationModule, allowPrivateMemberAccess);
            this.ilGen = this.dynamicMethod.GetILGenerator();
#endif

            InitILGeneration(methodName, argTypes);
        }

        void InitILGeneration(string methodName, Type[] argTypes)
        {
            this.methodEndLabel = ilGen.DefineLabel();
            this.blockStack = new Stack();
            this.argList = new ArrayList();
            for (int i = 0; i < argTypes.Length; i++)
                argList.Add(new ArgBuilder(i, argTypes[i]));
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceLabel("Begin method " + methodName + " {");
        }

        internal Delegate EndMethod()
        {
            MarkLabel(methodEndLabel);
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceLabel("} End method");
            Ret();

            Delegate retVal = null;
#if USE_REFEMIT
            Type type = typeBuilder.CreateType();
            assemblyBuilder.Save(assemblyBuilder.GetName().Name+".dll");

            MethodInfo method = type.GetMethod(methodBuilder.Name);
            retVal = Delegate.CreateDelegate(delegateType, method);
            methodBuilder = null;
#else
            retVal = dynamicMethod.CreateDelegate(delegateType);
            dynamicMethod = null;
#endif
            delegateType = null;

            ilGen = null;
            blockStack = null;
            argList = null;
            return retVal;
        }

        internal MethodInfo CurrentMethod
        {
            get
            {
#if USE_REFEMIT
                return methodBuilder;
#else
                return dynamicMethod;
#endif
            }
        }

        internal ArgBuilder GetArg(int index)
        {
            return (ArgBuilder)argList[index];
        }

        internal Type GetVariableType(object var)
        {
            if (var is ArgBuilder)
                return ((ArgBuilder)var).ArgType;
            else if (var is LocalBuilder)
                return ((LocalBuilder)var).LocalType;
            else
                return var.GetType();
        }

        internal LocalBuilder DeclareLocal(Type type, string name, object initialValue)
        {
            LocalBuilder local = DeclareLocal(type, name);
            Load(initialValue);
            Store(local);
            return local;
        }

        internal LocalBuilder DeclareLocal(Type type, string name)
        {
            return DeclareLocal(type, name, false);
        }

        internal LocalBuilder DeclareLocal(Type type, string name, bool isPinned)
        {
            LocalBuilder local = ilGen.DeclareLocal(type, isPinned);
            if (codeGenTrace != CodeGenTrace.None)
            {
                LocalNames[local] = name;
                EmitSourceComment("Declare local '" + name + "' of type " + type);
            }
            return local;
        }

        internal void Set(LocalBuilder local, object value)
        {
            Load(value);
            Store(local);
        }

        internal object For(LocalBuilder local, object start, object end)
        {
            ForState forState = new ForState(local, DefineLabel(), DefineLabel(), end);
            if (forState.Index != null)
            {
                Load(start);
                Stloc(forState.Index);
                Br(forState.TestLabel);
            }
            MarkLabel(forState.BeginLabel);
            blockStack.Push(forState);
            return forState;
        }

        internal void EndFor()
        {
            object stackTop = blockStack.Pop();
            ForState forState = stackTop as ForState;
            if (forState == null)
                ThrowMismatchException(stackTop);

            if (forState.Index != null)
            {
                Ldloc(forState.Index);
                Ldc(1);
                Add();
                Stloc(forState.Index);
                MarkLabel(forState.TestLabel);
                Ldloc(forState.Index);
                Load(forState.End);
                if (GetVariableType(forState.End).IsArray)
                    Ldlen();
                Blt(forState.BeginLabel);
            }
            else
                Br(forState.BeginLabel);
            if (forState.RequiresEndLabel)
                MarkLabel(forState.EndLabel);
        }

        internal void Break(object forState)
        {
            InternalBreakFor(forState, OpCodes.Br);
        }

        internal void IfTrueBreak(object forState)
        {
            InternalBreakFor(forState, OpCodes.Brtrue);
        }

        internal void IfFalseBreak(object forState)
        {
            InternalBreakFor(forState, OpCodes.Brfalse);
        }

        internal void InternalBreakFor(object userForState, OpCode branchInstruction)
        {
            foreach (object block in blockStack)
            {
                ForState forState = block as ForState;
                if (forState != null && (object)forState == userForState)
                {
                    if (!forState.RequiresEndLabel)
                    {
                        forState.EndLabel = DefineLabel();
                        forState.RequiresEndLabel = true;
                    }
                    if (codeGenTrace != CodeGenTrace.None)
                        EmitSourceInstruction(branchInstruction + " " + forState.EndLabel.GetHashCode());
                    ilGen.Emit(branchInstruction, forState.EndLabel);
                    break;
                }
            }
        }

        internal void ForEach(LocalBuilder local, Type elementType, Type enumeratorType,
            LocalBuilder enumerator, MethodInfo getCurrentMethod)
        {
            ForState forState = new ForState(local, DefineLabel(), DefineLabel(), enumerator);

            Br(forState.TestLabel);
            MarkLabel(forState.BeginLabel);

            Call(enumerator, getCurrentMethod);

            ConvertValue(elementType, GetVariableType(local));
            Stloc(local);
            blockStack.Push(forState);
        }

        internal void EndForEach(MethodInfo moveNextMethod)
        {
            object stackTop = blockStack.Pop();
            ForState forState = stackTop as ForState;
            if (forState == null)
                ThrowMismatchException(stackTop);

            MarkLabel(forState.TestLabel);

            object enumerator = forState.End;
            Call(enumerator, moveNextMethod);


            Brtrue(forState.BeginLabel);
            if (forState.RequiresEndLabel)
                MarkLabel(forState.EndLabel);
        }

        internal void IfNotDefaultValue(object value)
        {
            Type type = GetVariableType(value);
            TypeCode typeCode = Type.GetTypeCode(type);
            if ((typeCode == TypeCode.Object && type.IsValueType) ||
                typeCode == TypeCode.DateTime || typeCode == TypeCode.Decimal)
            {
                LoadDefaultValue(type);
                ConvertValue(type, Globals.TypeOfObject);
                Load(value);
                ConvertValue(type, Globals.TypeOfObject);
                Call(ObjectEquals);
                IfNot();
            }
            else
            {
                LoadDefaultValue(type);
                Load(value);
                If(Cmp.NotEqualTo);
            }
        }

        internal void If()
        {
            InternalIf(false);
        }

        internal void IfNot()
        {
            InternalIf(true);
        }

        OpCode GetBranchCode(Cmp cmp)
        {
            switch (cmp)
            {
                case Cmp.LessThan:
                    return OpCodes.Bge;
                case Cmp.EqualTo:
                    return OpCodes.Bne_Un;
                case Cmp.LessThanOrEqualTo:
                    return OpCodes.Bgt;
                case Cmp.GreaterThan:
                    return OpCodes.Ble;
                case Cmp.NotEqualTo:
                    return OpCodes.Beq;
                default:
                    Fx.Assert(cmp == Cmp.GreaterThanOrEqualTo, "Unexpected cmp");
                    return OpCodes.Blt;
            }
        }

        Cmp GetCmpInverse(Cmp cmp)
        {
            switch (cmp)
            {
                case Cmp.LessThan:
                    return Cmp.GreaterThanOrEqualTo;
                case Cmp.EqualTo:
                    return Cmp.NotEqualTo;
                case Cmp.LessThanOrEqualTo:
                    return Cmp.GreaterThan;
                case Cmp.GreaterThan:
                    return Cmp.LessThanOrEqualTo;
                case Cmp.NotEqualTo:
                    return Cmp.EqualTo;
                default:
                    Fx.Assert(cmp == Cmp.GreaterThanOrEqualTo, "Unexpected cmp");
                    return Cmp.LessThan;
            }
        }

        internal void If(Cmp cmpOp)
        {
            IfState ifState = new IfState();
            ifState.EndIf = DefineLabel();
            ifState.ElseBegin = DefineLabel();
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Branch if " + GetCmpInverse(cmpOp).ToString() + " to " + ifState.ElseBegin.GetHashCode().ToString(NumberFormatInfo.InvariantInfo));
            ilGen.Emit(GetBranchCode(cmpOp), ifState.ElseBegin);
            blockStack.Push(ifState);
        }


        internal void If(object value1, Cmp cmpOp, object value2)
        {
            Load(value1);
            Load(value2);
            If(cmpOp);
        }
        internal void Else()
        {
            IfState ifState = PopIfState();
            Br(ifState.EndIf);
            MarkLabel(ifState.ElseBegin);

            ifState.ElseBegin = ifState.EndIf;
            blockStack.Push(ifState);
        }

        internal void ElseIf(object value1, Cmp cmpOp, object value2)
        {
            IfState ifState = (IfState)blockStack.Pop();
            Br(ifState.EndIf);
            MarkLabel(ifState.ElseBegin);

            Load(value1);
            Load(value2);
            ifState.ElseBegin = DefineLabel();

            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Branch if " + GetCmpInverse(cmpOp).ToString() + " to " + ifState.ElseBegin.GetHashCode().ToString(NumberFormatInfo.InvariantInfo));

            ilGen.Emit(GetBranchCode(cmpOp), ifState.ElseBegin);
            blockStack.Push(ifState);
        }


        internal void EndIf()
        {
            IfState ifState = PopIfState();
            if (!ifState.ElseBegin.Equals(ifState.EndIf))
                MarkLabel(ifState.ElseBegin);
            MarkLabel(ifState.EndIf);
        }

        internal void VerifyParameterCount(MethodInfo methodInfo, int expectedCount)
        {
            if (methodInfo.GetParameters().Length != expectedCount)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.ParameterCountMismatch, methodInfo.Name, methodInfo.GetParameters().Length, expectedCount)));
        }

        internal void Call(object thisObj, MethodInfo methodInfo)
        {
            VerifyParameterCount(methodInfo, 0);
            LoadThis(thisObj, methodInfo);
            Call(methodInfo);
        }

        internal void Call(object thisObj, MethodInfo methodInfo, object param1)
        {
            VerifyParameterCount(methodInfo, 1);
            LoadThis(thisObj, methodInfo);
            LoadParam(param1, 1, methodInfo);
            Call(methodInfo);
        }

        internal void Call(object thisObj, MethodInfo methodInfo, object param1, object param2)
        {
            VerifyParameterCount(methodInfo, 2);
            LoadThis(thisObj, methodInfo);
            LoadParam(param1, 1, methodInfo);
            LoadParam(param2, 2, methodInfo);
            Call(methodInfo);
        }

        internal void Call(object thisObj, MethodInfo methodInfo, object param1, object param2, object param3)
        {
            VerifyParameterCount(methodInfo, 3);
            LoadThis(thisObj, methodInfo);
            LoadParam(param1, 1, methodInfo);
            LoadParam(param2, 2, methodInfo);
            LoadParam(param3, 3, methodInfo);
            Call(methodInfo);
        }

        internal void Call(object thisObj, MethodInfo methodInfo, object param1, object param2, object param3, object param4)
        {
            VerifyParameterCount(methodInfo, 4);
            LoadThis(thisObj, methodInfo);
            LoadParam(param1, 1, methodInfo);
            LoadParam(param2, 2, methodInfo);
            LoadParam(param3, 3, methodInfo);
            LoadParam(param4, 4, methodInfo);
            Call(methodInfo);
        }

        internal void Call(object thisObj, MethodInfo methodInfo, object param1, object param2, object param3, object param4, object param5)
        {
            VerifyParameterCount(methodInfo, 5);
            LoadThis(thisObj, methodInfo);
            LoadParam(param1, 1, methodInfo);
            LoadParam(param2, 2, methodInfo);
            LoadParam(param3, 3, methodInfo);
            LoadParam(param4, 4, methodInfo);
            LoadParam(param5, 5, methodInfo);
            Call(methodInfo);
        }

        internal void Call(object thisObj, MethodInfo methodInfo, object param1, object param2, object param3, object param4, object param5, object param6)
        {
            VerifyParameterCount(methodInfo, 6);
            LoadThis(thisObj, methodInfo);
            LoadParam(param1, 1, methodInfo);
            LoadParam(param2, 2, methodInfo);
            LoadParam(param3, 3, methodInfo);
            LoadParam(param4, 4, methodInfo);
            LoadParam(param5, 5, methodInfo);
            LoadParam(param6, 6, methodInfo);
            Call(methodInfo);
        }

        internal void Call(MethodInfo methodInfo)
        {
            if (methodInfo.IsVirtual && !methodInfo.DeclaringType.IsValueType)
            {
                if (codeGenTrace != CodeGenTrace.None)
                    EmitSourceInstruction("Callvirt " + methodInfo.ToString() + " on type " + methodInfo.DeclaringType.ToString());
                ilGen.Emit(OpCodes.Callvirt, methodInfo);
            }
            else if (methodInfo.IsStatic)
            {
                if (codeGenTrace != CodeGenTrace.None)
                    EmitSourceInstruction("Static Call " + methodInfo.ToString() + " on type " + methodInfo.DeclaringType.ToString());
                ilGen.Emit(OpCodes.Call, methodInfo);
            }
            else
            {
                if (codeGenTrace != CodeGenTrace.None)
                    EmitSourceInstruction("Call " + methodInfo.ToString() + " on type " + methodInfo.DeclaringType.ToString());
                ilGen.Emit(OpCodes.Call, methodInfo);
            }
        }

        internal void Call(ConstructorInfo ctor)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Call " + ctor.ToString() + " on type " + ctor.DeclaringType.ToString());
            ilGen.Emit(OpCodes.Call, ctor);
        }

        internal void New(ConstructorInfo constructorInfo)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Newobj " + constructorInfo.ToString() + " on type " + constructorInfo.DeclaringType.ToString());
            ilGen.Emit(OpCodes.Newobj, constructorInfo);
        }

        internal void New(ConstructorInfo constructorInfo, object param1)
        {
            LoadParam(param1, 1, constructorInfo);
            New(constructorInfo);
        }

        internal void InitObj(Type valueType)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Initobj " + valueType);
            ilGen.Emit(OpCodes.Initobj, valueType);
        }

        internal void NewArray(Type elementType, object len)
        {
            Load(len);
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Newarr " + elementType);
            ilGen.Emit(OpCodes.Newarr, elementType);
        }

        internal void IgnoreReturnValue()
        {
            Pop();
        }

        internal void LoadArrayElement(object obj, object arrayIndex)
        {
            Type objType = GetVariableType(obj).GetElementType();
            Load(obj);
            Load(arrayIndex);
            if (IsStruct(objType))
            {
                Ldelema(objType);
                Ldobj(objType);
            }
            else
                Ldelem(objType);
        }

        internal void StoreArrayElement(object obj, object arrayIndex, object value)
        {
            Type arrayType = GetVariableType(obj);
            if (arrayType == Globals.TypeOfArray)
            {
                Call(obj, ArraySetValue, value, arrayIndex);
            }
            else
            {
                Type objType = arrayType.GetElementType();
                Load(obj);
                Load(arrayIndex);
                if (IsStruct(objType))
                    Ldelema(objType);
                Load(value);
                ConvertValue(GetVariableType(value), objType);
                if (IsStruct(objType))
                    Stobj(objType);
                else
                    Stelem(objType);
            }
        }

        static bool IsStruct(Type objType)
        {
            return objType.IsValueType && !objType.IsPrimitive;
        }

        internal Type LoadMember(MemberInfo memberInfo)
        {
            Type memberType = null;
            if (memberInfo.MemberType == MemberTypes.Field)
            {
                FieldInfo fieldInfo = (FieldInfo)memberInfo;
                memberType = fieldInfo.FieldType;
                if (fieldInfo.IsStatic)
                {
                    if (codeGenTrace != CodeGenTrace.None)
                        EmitSourceInstruction("Ldsfld " + fieldInfo + " on type " + fieldInfo.DeclaringType);
                    ilGen.Emit(OpCodes.Ldsfld, fieldInfo);
                }
                else
                {
                    if (codeGenTrace != CodeGenTrace.None)
                        EmitSourceInstruction("Ldfld " + fieldInfo + " on type " + fieldInfo.DeclaringType);
                    ilGen.Emit(OpCodes.Ldfld, fieldInfo);
                }
            }
            else if (memberInfo.MemberType == MemberTypes.Property)
            {
                PropertyInfo property = memberInfo as PropertyInfo;
                memberType = property.PropertyType;
                if (property != null)
                {
                    MethodInfo getMethod = property.GetGetMethod(true);
                    if (getMethod == null)
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.NoGetMethodForProperty, property.DeclaringType, property)));
                    Call(getMethod);
                }
            }
            else if (memberInfo.MemberType == MemberTypes.Method)
            {
                MethodInfo method = (MethodInfo)memberInfo;
                memberType = method.ReturnType;
                Call(method);
            }
            else
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.CannotLoadMemberType, memberInfo.MemberType, memberInfo.DeclaringType, memberInfo.Name)));

            EmitStackTop(memberType);
            return memberType;
        }

        internal void StoreMember(MemberInfo memberInfo)
        {
            if (memberInfo.MemberType == MemberTypes.Field)
            {
                FieldInfo fieldInfo = (FieldInfo)memberInfo;
                if (fieldInfo.IsStatic)
                {
                    if (codeGenTrace != CodeGenTrace.None)
                        EmitSourceInstruction("Stsfld " + fieldInfo + " on type " + fieldInfo.DeclaringType);
                    ilGen.Emit(OpCodes.Stsfld, fieldInfo);
                }
                else
                {
                    if (codeGenTrace != CodeGenTrace.None)
                        EmitSourceInstruction("Stfld " + fieldInfo + " on type " + fieldInfo.DeclaringType);
                    ilGen.Emit(OpCodes.Stfld, fieldInfo);
                }
            }
            else if (memberInfo.MemberType == MemberTypes.Property)
            {
                PropertyInfo property = memberInfo as PropertyInfo;
                if (property != null)
                {
                    MethodInfo setMethod = property.GetSetMethod(true);
                    if (setMethod == null)
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.NoSetMethodForProperty, property.DeclaringType, property)));
                    Call(setMethod);
                }
            }
            else if (memberInfo.MemberType == MemberTypes.Method)
                Call((MethodInfo)memberInfo);
            else
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.CannotLoadMemberType, memberInfo.MemberType)));
        }

        internal void LoadDefaultValue(Type type)
        {
            if (type.IsValueType)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                        Ldc(false);
                        break;
                    case TypeCode.Char:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                        Ldc(0);
                        break;
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        Ldc(0L);
                        break;
                    case TypeCode.Single:
                        Ldc(0.0F);
                        break;
                    case TypeCode.Double:
                        Ldc(0.0);
                        break;
                    case TypeCode.Decimal:
                    case TypeCode.DateTime:
                    default:
                        LocalBuilder zero = DeclareLocal(type, "zero");
                        LoadAddress(zero);
                        InitObj(type);
                        Load(zero);
                        break;
                }
            }
            else
                Load(null);
        }

        internal void Load(object obj)
        {
            if (obj == null)
            {
                if (codeGenTrace != CodeGenTrace.None)
                    EmitSourceInstruction("Ldnull");
                ilGen.Emit(OpCodes.Ldnull);
            }
            else if (obj is ArgBuilder)
                Ldarg((ArgBuilder)obj);
            else if (obj is LocalBuilder)
                Ldloc((LocalBuilder)obj);
            else
                Ldc(obj);
        }

        internal void Store(object var)
        {
            if (var is ArgBuilder)
                Starg((ArgBuilder)var);
            else if (var is LocalBuilder)
                Stloc((LocalBuilder)var);
            else
            {
                Fx.Assert("Data can only be stored into ArgBuilder or LocalBuilder.");
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.CanOnlyStoreIntoArgOrLocGot0, DataContract.GetClrTypeFullName(var.GetType()))));
            }
        }

        internal void Dec(object var)
        {
            Load(var);
            Load(1);
            Subtract();
            Store(var);
        }

        internal void Inc(object var)
        {
            Load(var);
            Load(1);
            Add();
            Store(var);
        }

        internal void LoadAddress(object obj)
        {
            if (obj is ArgBuilder)
                LdargAddress((ArgBuilder)obj);
            else if (obj is LocalBuilder)
                LdlocAddress((LocalBuilder)obj);
            else
                Load(obj);
        }


        internal void ConvertAddress(Type source, Type target)
        {
            InternalConvert(source, target, true);
        }

        internal void ConvertValue(Type source, Type target)
        {
            InternalConvert(source, target, false);
        }


        internal void Castclass(Type target)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Castclass " + target);
            ilGen.Emit(OpCodes.Castclass, target);
        }

        internal void Box(Type type)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Box " + type);
            ilGen.Emit(OpCodes.Box, type);
        }

        internal void Unbox(Type type)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Unbox " + type);
            ilGen.Emit(OpCodes.Unbox, type);
        }

        OpCode GetLdindOpCode(TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return OpCodes.Ldind_I1; // TypeCode.Boolean:
                case TypeCode.Char:
                    return OpCodes.Ldind_I2; // TypeCode.Char:
                case TypeCode.SByte:
                    return OpCodes.Ldind_I1; // TypeCode.SByte:
                case TypeCode.Byte:
                    return OpCodes.Ldind_U1; // TypeCode.Byte:
                case TypeCode.Int16:
                    return OpCodes.Ldind_I2; // TypeCode.Int16:
                case TypeCode.UInt16:
                    return OpCodes.Ldind_U2; // TypeCode.UInt16:
                case TypeCode.Int32:
                    return OpCodes.Ldind_I4; // TypeCode.Int32:
                case TypeCode.UInt32:
                    return OpCodes.Ldind_U4; // TypeCode.UInt32:
                case TypeCode.Int64:
                    return OpCodes.Ldind_I8; // TypeCode.Int64:
                case TypeCode.UInt64:
                    return OpCodes.Ldind_I8; // TypeCode.UInt64:
                case TypeCode.Single:
                    return OpCodes.Ldind_R4; // TypeCode.Single:
                case TypeCode.Double:
                    return OpCodes.Ldind_R8; // TypeCode.Double:
                case TypeCode.String:
                    return OpCodes.Ldind_Ref; // TypeCode.String:
                default:
                    return OpCodes.Nop;
            }
            // 
        }

        internal void Ldobj(Type type)
        {
            OpCode opCode = GetLdindOpCode(Type.GetTypeCode(type));
            if (!opCode.Equals(OpCodes.Nop))
            {
                if (codeGenTrace != CodeGenTrace.None)
                    EmitSourceInstruction(opCode.ToString());
                ilGen.Emit(opCode);
            }
            else
            {
                if (codeGenTrace != CodeGenTrace.None)
                    EmitSourceInstruction("Ldobj " + type);
                ilGen.Emit(OpCodes.Ldobj, type);
            }
        }

        internal void Stobj(Type type)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Stobj " + type);
            ilGen.Emit(OpCodes.Stobj, type);
        }


        internal void Ceq()
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Ceq");
            ilGen.Emit(OpCodes.Ceq);
        }

        internal void Bgt(Label label)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Bgt " + label.GetHashCode());
            ilGen.Emit(OpCodes.Bgt, label);
        }

        internal void Ble(Label label)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Ble " + label.GetHashCode());
            ilGen.Emit(OpCodes.Ble, label);
        }

        internal void Throw()
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Throw");
            ilGen.Emit(OpCodes.Throw);
        }

        internal void Ldtoken(Type t)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Ldtoken " + t);
            ilGen.Emit(OpCodes.Ldtoken, t);
        }

        internal void Ldc(object o)
        {
            Type valueType = o.GetType();
            if (o is Type)
            {
                Ldtoken((Type)o);
                Call(GetTypeFromHandle);
            }
            else if (valueType.IsEnum)
            {
                if (codeGenTrace != CodeGenTrace.None)
                    EmitSourceComment("Ldc " + o.GetType() + "." + o);
                Ldc(((IConvertible)o).ToType(Enum.GetUnderlyingType(valueType), null));
            }
            else
            {
                switch (Type.GetTypeCode(valueType))
                {
                    case TypeCode.Boolean:
                        Ldc((bool)o);
                        break;
                    case TypeCode.Char:
                        Fx.Assert("Char is not a valid schema primitive and should be treated as int in DataContract");
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.CharIsInvalidPrimitive)));
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                        Ldc(((IConvertible)o).ToInt32(CultureInfo.InvariantCulture));
                        break;
                    case TypeCode.Int32:
                        Ldc((int)o);
                        break;
                    case TypeCode.UInt32:
                        Ldc((int)(uint)o);
                        break;
                    case TypeCode.UInt64:
                        Ldc((long)(ulong)o);
                        break;
                    case TypeCode.Int64:
                        Ldc((long)o);
                        break;
                    case TypeCode.Single:
                        Ldc((float)o);
                        break;
                    case TypeCode.Double:
                        Ldc((double)o);
                        break;
                    case TypeCode.String:
                        Ldstr((string)o);
                        break;
                    case TypeCode.Object:
                    case TypeCode.Decimal:
                    case TypeCode.DateTime:
                    case TypeCode.Empty:
                    case TypeCode.DBNull:
                    default:
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.UnknownConstantType, DataContract.GetClrTypeFullName(valueType))));
                }
            }
        }

        internal void Ldc(bool boolVar)
        {
            if (boolVar)
            {
                if (codeGenTrace != CodeGenTrace.None)
                    EmitSourceInstruction("Ldc.i4 1");
                ilGen.Emit(OpCodes.Ldc_I4_1);
            }
            else
            {
                if (codeGenTrace != CodeGenTrace.None)
                    EmitSourceInstruction("Ldc.i4 0");
                ilGen.Emit(OpCodes.Ldc_I4_0);
            }
        }

        internal void Ldc(int intVar)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Ldc.i4 " + intVar);
            switch (intVar)
            {
                case -1:
                    ilGen.Emit(OpCodes.Ldc_I4_M1);
                    break;
                case 0:
                    ilGen.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    ilGen.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    ilGen.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    ilGen.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    ilGen.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    ilGen.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    ilGen.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    ilGen.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    ilGen.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    ilGen.Emit(OpCodes.Ldc_I4, intVar);
                    break;
            }
        }

        internal void Ldc(long l)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Ldc.i8 " + l);
            ilGen.Emit(OpCodes.Ldc_I8, l);
        }

        internal void Ldc(float f)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Ldc.r4 " + f);
            ilGen.Emit(OpCodes.Ldc_R4, f);
        }

        internal void Ldc(double d)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Ldc.r8 " + d);
            ilGen.Emit(OpCodes.Ldc_R8, d);
        }

        internal void Ldstr(string strVar)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Ldstr " + strVar);
            ilGen.Emit(OpCodes.Ldstr, strVar);
        }

        internal void LdlocAddress(LocalBuilder localBuilder)
        {
            if (localBuilder.LocalType.IsValueType)
                Ldloca(localBuilder);
            else
                Ldloc(localBuilder);
        }

        internal void Ldloc(LocalBuilder localBuilder)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Ldloc " + LocalNames[localBuilder]);
            ilGen.Emit(OpCodes.Ldloc, localBuilder);
            EmitStackTop(localBuilder.LocalType);
        }

        internal void Stloc(LocalBuilder local)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Stloc " + LocalNames[local]);
            EmitStackTop(local.LocalType);
            ilGen.Emit(OpCodes.Stloc, local);
        }

        internal void Ldloc(int slot)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Ldloc " + slot);

            switch (slot)
            {
                case 0:
                    ilGen.Emit(OpCodes.Ldloc_0);
                    break;
                case 1:
                    ilGen.Emit(OpCodes.Ldloc_1);
                    break;
                case 2:
                    ilGen.Emit(OpCodes.Ldloc_2);
                    break;
                case 3:
                    ilGen.Emit(OpCodes.Ldloc_3);
                    break;
                default:
                    if (slot <= 255)
                        ilGen.Emit(OpCodes.Ldloc_S, slot);
                    else
                        ilGen.Emit(OpCodes.Ldloc, slot);
                    break;
            }
        }

        internal void Stloc(int slot)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Stloc " + slot);
            switch (slot)
            {
                case 0:
                    ilGen.Emit(OpCodes.Stloc_0);
                    break;
                case 1:
                    ilGen.Emit(OpCodes.Stloc_1);
                    break;
                case 2:
                    ilGen.Emit(OpCodes.Stloc_2);
                    break;
                case 3:
                    ilGen.Emit(OpCodes.Stloc_3);
                    break;
                default:
                    if (slot <= 255)
                        ilGen.Emit(OpCodes.Stloc_S, slot);
                    else
                        ilGen.Emit(OpCodes.Stloc, slot);
                    break;
            }
        }

        internal void Ldloca(LocalBuilder localBuilder)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Ldloca " + LocalNames[localBuilder]);
            ilGen.Emit(OpCodes.Ldloca, localBuilder);
            EmitStackTop(localBuilder.LocalType);
        }

        internal void Ldloca(int slot)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Ldloca " + slot);
            if (slot <= 255)
                ilGen.Emit(OpCodes.Ldloca_S, slot);
            else
                ilGen.Emit(OpCodes.Ldloca, slot);
        }

        internal void LdargAddress(ArgBuilder argBuilder)
        {
            if (argBuilder.ArgType.IsValueType)
                Ldarga(argBuilder);
            else
                Ldarg(argBuilder);
        }

        internal void Ldarg(ArgBuilder arg)
        {
            Ldarg(arg.Index);
        }

        internal void Starg(ArgBuilder arg)
        {
            Starg(arg.Index);
        }

        internal void Ldarg(int slot)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Ldarg " + slot);
            switch (slot)
            {
                case 0:
                    ilGen.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    ilGen.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    ilGen.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    ilGen.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    if (slot <= 255)
                        ilGen.Emit(OpCodes.Ldarg_S, slot);
                    else
                        ilGen.Emit(OpCodes.Ldarg, slot);
                    break;
            }
        }

        internal void Starg(int slot)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Starg " + slot);
            if (slot <= 255)
                ilGen.Emit(OpCodes.Starg_S, slot);
            else
                ilGen.Emit(OpCodes.Starg, slot);
        }

        internal void Ldarga(ArgBuilder argBuilder)
        {
            Ldarga(argBuilder.Index);
        }

        internal void Ldarga(int slot)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Ldarga " + slot);
            if (slot <= 255)
                ilGen.Emit(OpCodes.Ldarga_S, slot);
            else
                ilGen.Emit(OpCodes.Ldarga, slot);
        }

        internal void Ldlen()
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Ldlen");
            ilGen.Emit(OpCodes.Ldlen);
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Conv.i4");
            ilGen.Emit(OpCodes.Conv_I4);
        }

        OpCode GetLdelemOpCode(TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.Object:
                case TypeCode.DBNull:
                    return OpCodes.Ldelem_Ref; // TypeCode.Object:
                case TypeCode.Boolean:
                    return OpCodes.Ldelem_I1; // TypeCode.Boolean:
                case TypeCode.Char:
                    return OpCodes.Ldelem_I2; // TypeCode.Char:
                case TypeCode.SByte:
                    return OpCodes.Ldelem_I1; // TypeCode.SByte:
                case TypeCode.Byte:
                    return OpCodes.Ldelem_U1; // TypeCode.Byte:
                case TypeCode.Int16:
                    return OpCodes.Ldelem_I2; // TypeCode.Int16:
                case TypeCode.UInt16:
                    return OpCodes.Ldelem_U2; // TypeCode.UInt16:
                case TypeCode.Int32:
                    return OpCodes.Ldelem_I4; // TypeCode.Int32:
                case TypeCode.UInt32:
                    return OpCodes.Ldelem_U4; // TypeCode.UInt32:
                case TypeCode.Int64:
                    return OpCodes.Ldelem_I8; // TypeCode.Int64:
                case TypeCode.UInt64:
                    return OpCodes.Ldelem_I8; // TypeCode.UInt64:
                case TypeCode.Single:
                    return OpCodes.Ldelem_R4; // TypeCode.Single:
                case TypeCode.Double:
                    return OpCodes.Ldelem_R8; // TypeCode.Double:
                case TypeCode.String:
                    return OpCodes.Ldelem_Ref; // TypeCode.String:
                default:
                    return OpCodes.Nop;
            }
        }

        internal void Ldelem(Type arrayElementType)
        {
            if (arrayElementType.IsEnum)
            {
                Ldelem(Enum.GetUnderlyingType(arrayElementType));
            }
            else
            {
                OpCode opCode = GetLdelemOpCode(Type.GetTypeCode(arrayElementType));
                if (opCode.Equals(OpCodes.Nop))
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.ArrayTypeIsNotSupported, DataContract.GetClrTypeFullName(arrayElementType))));
                if (codeGenTrace != CodeGenTrace.None)
                    EmitSourceInstruction(opCode.ToString());
                ilGen.Emit(opCode);
                EmitStackTop(arrayElementType);
            }
        }
        internal void Ldelema(Type arrayElementType)
        {
            OpCode opCode = OpCodes.Ldelema;
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction(opCode.ToString());
            ilGen.Emit(opCode, arrayElementType);

            EmitStackTop(arrayElementType);
        }

        OpCode GetStelemOpCode(TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.Object:
                case TypeCode.DBNull:
                    return OpCodes.Stelem_Ref; // TypeCode.Object:
                case TypeCode.Boolean:
                    return OpCodes.Stelem_I1; // TypeCode.Boolean:
                case TypeCode.Char:
                    return OpCodes.Stelem_I2; // TypeCode.Char:
                case TypeCode.SByte:
                    return OpCodes.Stelem_I1; // TypeCode.SByte:
                case TypeCode.Byte:
                    return OpCodes.Stelem_I1; // TypeCode.Byte:
                case TypeCode.Int16:
                    return OpCodes.Stelem_I2; // TypeCode.Int16:
                case TypeCode.UInt16:
                    return OpCodes.Stelem_I2; // TypeCode.UInt16:
                case TypeCode.Int32:
                    return OpCodes.Stelem_I4; // TypeCode.Int32:
                case TypeCode.UInt32:
                    return OpCodes.Stelem_I4; // TypeCode.UInt32:
                case TypeCode.Int64:
                    return OpCodes.Stelem_I8; // TypeCode.Int64:
                case TypeCode.UInt64:
                    return OpCodes.Stelem_I8; // TypeCode.UInt64:
                case TypeCode.Single:
                    return OpCodes.Stelem_R4; // TypeCode.Single:
                case TypeCode.Double:
                    return OpCodes.Stelem_R8; // TypeCode.Double:
                case TypeCode.String:
                    return OpCodes.Stelem_Ref; // TypeCode.String:
                default:
                    return OpCodes.Nop;
            }
        }

        internal void Stelem(Type arrayElementType)
        {
            if (arrayElementType.IsEnum)
                Stelem(Enum.GetUnderlyingType(arrayElementType));
            else
            {
                OpCode opCode = GetStelemOpCode(Type.GetTypeCode(arrayElementType));
                if (opCode.Equals(OpCodes.Nop))
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.ArrayTypeIsNotSupported, DataContract.GetClrTypeFullName(arrayElementType))));
                if (codeGenTrace != CodeGenTrace.None)
                    EmitSourceInstruction(opCode.ToString());
                EmitStackTop(arrayElementType);
                ilGen.Emit(opCode);
            }
        }

        internal Label DefineLabel()
        {
            return ilGen.DefineLabel();
        }

        internal void MarkLabel(Label label)
        {
            ilGen.MarkLabel(label);
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceLabel(label.GetHashCode() + ":");
        }

        internal void Add()
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Add");
            ilGen.Emit(OpCodes.Add);
        }

        internal void Subtract()
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Sub");
            ilGen.Emit(OpCodes.Sub);
        }

        internal void And()
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("And");
            ilGen.Emit(OpCodes.And);
        }
        internal void Or()
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Or");
            ilGen.Emit(OpCodes.Or);
        }

        internal void Not()
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Not");
            ilGen.Emit(OpCodes.Not);
        }

        internal void Ret()
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Ret");
            ilGen.Emit(OpCodes.Ret);
        }

        internal void Br(Label label)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Br " + label.GetHashCode());
            ilGen.Emit(OpCodes.Br, label);
        }

        internal void Blt(Label label)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Blt " + label.GetHashCode());
            ilGen.Emit(OpCodes.Blt, label);
        }

        internal void Brfalse(Label label)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Brfalse " + label.GetHashCode());
            ilGen.Emit(OpCodes.Brfalse, label);
        }

        internal void Brtrue(Label label)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Brtrue " + label.GetHashCode());
            ilGen.Emit(OpCodes.Brtrue, label);
        }



        internal void Pop()
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Pop");
            ilGen.Emit(OpCodes.Pop);
        }

        internal void Dup()
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Dup");
            ilGen.Emit(OpCodes.Dup);
        }

        void LoadThis(object thisObj, MethodInfo methodInfo)
        {
            if (thisObj != null && !methodInfo.IsStatic)
            {
                LoadAddress(thisObj);
                ConvertAddress(GetVariableType(thisObj), methodInfo.DeclaringType);
            }
        }

        void LoadParam(object arg, int oneBasedArgIndex, MethodBase methodInfo)
        {
            Load(arg);
            if (arg != null)
                ConvertValue(GetVariableType(arg), methodInfo.GetParameters()[oneBasedArgIndex - 1].ParameterType);
        }

        void InternalIf(bool negate)
        {
            IfState ifState = new IfState();
            ifState.EndIf = DefineLabel();
            ifState.ElseBegin = DefineLabel();
            if (negate)
                Brtrue(ifState.ElseBegin);
            else
                Brfalse(ifState.ElseBegin);
            blockStack.Push(ifState);
        }

        OpCode GetConvOpCode(TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return OpCodes.Conv_I1; // TypeCode.Boolean:
                case TypeCode.Char:
                    return OpCodes.Conv_I2; // TypeCode.Char:
                case TypeCode.SByte:
                    return OpCodes.Conv_I1; // TypeCode.SByte:
                case TypeCode.Byte:
                    return OpCodes.Conv_U1; // TypeCode.Byte:
                case TypeCode.Int16:
                    return OpCodes.Conv_I2; // TypeCode.Int16:
                case TypeCode.UInt16:
                    return OpCodes.Conv_U2; // TypeCode.UInt16:
                case TypeCode.Int32:
                    return OpCodes.Conv_I4; // TypeCode.Int32:
                case TypeCode.UInt32:
                    return OpCodes.Conv_U4; // TypeCode.UInt32:
                case TypeCode.Int64:
                    return OpCodes.Conv_I8; // TypeCode.Int64:
                case TypeCode.UInt64:
                    return OpCodes.Conv_I8; // TypeCode.UInt64:
                case TypeCode.Single:
                    return OpCodes.Conv_R4; // TypeCode.Single:
                case TypeCode.Double:
                    return OpCodes.Conv_R8; // TypeCode.Double:
                default:
                    return OpCodes.Nop;
            }
        }

        void InternalConvert(Type source, Type target, bool isAddress)
        {
            if (target == source)
                return;
            if (target.IsValueType)
            {
                if (source.IsValueType)
                {
                    OpCode opCode = GetConvOpCode(Type.GetTypeCode(target));
                    if (opCode.Equals(OpCodes.Nop))
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.NoConversionPossibleTo, DataContract.GetClrTypeFullName(target))));
                    else
                    {
                        if (codeGenTrace != CodeGenTrace.None)
                            EmitSourceInstruction(opCode.ToString());
                        ilGen.Emit(opCode);
                    }
                }
                else if (source.IsAssignableFrom(target))
                {
                    Unbox(target);
                    if (!isAddress)
                        Ldobj(target);
                }
                else
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.IsNotAssignableFrom, DataContract.GetClrTypeFullName(target), DataContract.GetClrTypeFullName(source))));
            }
            else if (target.IsAssignableFrom(source))
            {
                if (source.IsValueType)
                {
                    if (isAddress)
                        Ldobj(source);
                    Box(source);
                }
            }
            else if (source.IsAssignableFrom(target))
            {
                //assert(source.IsValueType == false);
                Castclass(target);
            }
            else if (target.IsInterface || source.IsInterface)
            {
                Castclass(target);
            }
            else
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.IsNotAssignableFrom, DataContract.GetClrTypeFullName(target), DataContract.GetClrTypeFullName(source))));
        }

        IfState PopIfState()
        {
            object stackTop = blockStack.Pop();
            IfState ifState = stackTop as IfState;
            if (ifState == null)
                ThrowMismatchException(stackTop);
            return ifState;
        }

#if USE_REFEMIT
        void InitAssemblyBuilder(string methodName)
        {
            AssemblyName name = new AssemblyName();
            name.Name = "Microsoft.GeneratedCode."+methodName;
            assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);
            moduleBuilder = assemblyBuilder.DefineDynamicModule(name.Name, name.Name + ".dll", false);
        }
#endif

        void ThrowMismatchException(object expected)
        {
            throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.ExpectingEnd, expected.ToString())));
        }

        Hashtable LocalNames
        {
            get
            {
                if (localNames == null)
                    localNames = new Hashtable();
                return localNames;
            }
        }

        internal void EmitSourceInstruction(string line)
        {
            EmitSourceLine("    " + line);
        }

        internal void EmitSourceLabel(string line)
        {
            EmitSourceLine(line);
        }

        internal void EmitSourceComment(string comment)
        {
            EmitSourceInstruction("// " + comment);
        }

        internal void EmitSourceLine(string line)
        {
            if (codeGenTrace != CodeGenTrace.None)
                SerializationTrace.WriteInstruction(lineNo++, line);
            if (ilGen != null && codeGenTrace == CodeGenTrace.Tron)
            {
                ilGen.Emit(OpCodes.Ldstr, string.Format(CultureInfo.InvariantCulture, "{0:00000}: {1}", lineNo - 1, line));
                ilGen.Emit(OpCodes.Call, XmlFormatGeneratorStatics.TraceInstructionMethod);
            }
        }

        internal void EmitStackTop(Type stackTopType)
        {
            if (codeGenTrace != CodeGenTrace.Tron)
                return;
            codeGenTrace = CodeGenTrace.None;
            Dup();
            ToDebuggableString(stackTopType);
            LocalBuilder topValue = DeclareLocal(Globals.TypeOfString, "topValue");
            Store(topValue);
            Load("//value = ");
            Load(topValue);
            Concat2();
            Call(XmlFormatGeneratorStatics.TraceInstructionMethod);
            codeGenTrace = CodeGenTrace.Tron;
        }

        internal void ToString(Type type)
        {
            if (type != Globals.TypeOfString)
            {
                if (type.IsValueType)
                {
                    Box(type);
                }
                Call(ObjectToString);
            }
        }

        internal void ToDebuggableString(Type type)
        {
            if (type.IsValueType)
            {
                Box(type);
                Call(ObjectToString);
            }
            else
            {
                Dup();
                Load(null);
                If(Cmp.EqualTo);
                Pop();
                Load("<null>");
                Else();
                if (type.IsArray)
                {
                    LocalBuilder arrayVar = DeclareLocal(type, "arrayVar");
                    Store(arrayVar);
                    Load("{ ");
                    LocalBuilder arrayValueString = DeclareLocal(typeof(string), "arrayValueString");
                    Store(arrayValueString);
                    LocalBuilder i = DeclareLocal(typeof(int), "i");
                    For(i, 0, arrayVar);
                    Load(arrayValueString);
                    LoadArrayElement(arrayVar, i);
                    ToDebuggableString(arrayVar.LocalType.GetElementType());
                    Load(", ");
                    Concat3();
                    Store(arrayValueString);
                    EndFor();
                    Load(arrayValueString);
                    Load("}");
                    Concat2();
                }
                else
                    Call(ObjectToString);
                EndIf();
            }
        }

        internal void Concat2()
        {
            Call(StringConcat2);
        }

        internal void Concat3()
        {
            Call(StringConcat3);
        }

        internal Label[] Switch(int labelCount)
        {
            SwitchState switchState = new SwitchState(DefineLabel(), DefineLabel());
            Label[] caseLabels = new Label[labelCount];
            for (int i = 0; i < caseLabels.Length; i++)
                caseLabels[i] = DefineLabel();

            if (codeGenTrace != CodeGenTrace.None)
            {
                EmitSourceInstruction("switch (");
                foreach (Label l in caseLabels)
                    EmitSourceInstruction("    " + l.GetHashCode());
                EmitSourceInstruction(") {");
            }
            ilGen.Emit(OpCodes.Switch, caseLabels);
            Br(switchState.DefaultLabel);
            blockStack.Push(switchState);
            return caseLabels;
        }
        internal void Case(Label caseLabel1, string caseLabelName)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("case " + caseLabelName + "{");
            MarkLabel(caseLabel1);
        }

        internal void EndCase()
        {
            object stackTop = blockStack.Peek();
            SwitchState switchState = stackTop as SwitchState;
            if (switchState == null)
                ThrowMismatchException(stackTop);
            Br(switchState.EndOfSwitchLabel);
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("} //end case ");
        }

        internal void DefaultCase()
        {
            object stackTop = blockStack.Peek();
            SwitchState switchState = stackTop as SwitchState;
            if (switchState == null)
                ThrowMismatchException(stackTop);
            MarkLabel(switchState.DefaultLabel);
            switchState.DefaultDefined = true;
        }

        internal void EndSwitch()
        {
            object stackTop = blockStack.Pop();
            SwitchState switchState = stackTop as SwitchState;
            if (switchState == null)
                ThrowMismatchException(stackTop);
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("} //end switch");
            if (!switchState.DefaultDefined)
                MarkLabel(switchState.DefaultLabel);
            MarkLabel(switchState.EndOfSwitchLabel);
        }

        internal void CallStringFormat(string msg, params object[] values)
        {
            NewArray(typeof(object), values.Length);
            if (stringFormatArray == null)
                stringFormatArray = DeclareLocal(typeof(object[]), "stringFormatArray");
            Stloc(stringFormatArray);
            for (int i = 0; i < values.Length; i++)
                StoreArrayElement(stringFormatArray, i, values[i]);

            Load(msg);
            Load(stringFormatArray);
            Call(StringFormat);
        }

        static MethodInfo stringLength = typeof(string).GetProperty("Length").GetGetMethod();
        internal void ElseIfIsEmptyString(LocalBuilder strLocal)
        {
            IfState ifState = (IfState)blockStack.Pop();
            Br(ifState.EndIf);
            MarkLabel(ifState.ElseBegin);

            Load(strLocal);
            Call(stringLength);
            Load(0);
            ifState.ElseBegin = DefineLabel();

            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Branch if " + GetCmpInverse(Cmp.EqualTo).ToString() + " to " + ifState.ElseBegin.GetHashCode().ToString(NumberFormatInfo.InvariantInfo));

            ilGen.Emit(GetBranchCode(Cmp.EqualTo), ifState.ElseBegin);
            blockStack.Push(ifState);
        }

        internal void IfNotIsEmptyString(LocalBuilder strLocal)
        {
            Load(strLocal);
            Call(stringLength);
            Load(0);
            If(Cmp.NotEqualTo);
        }

        internal void BeginWhileCondition()
        {
            Label startWhile = DefineLabel();
            MarkLabel(startWhile);
            blockStack.Push(startWhile);
        }

        internal void BeginWhileBody(Cmp cmpOp)
        {
            Label startWhile = (Label)blockStack.Pop();
            If(cmpOp);
            blockStack.Push(startWhile);
        }

        internal void EndWhile()
        {
            Label startWhile = (Label)blockStack.Pop();
            Br(startWhile);
            EndIf();
        }

#if NotUsed
        static MethodInfo stringCompare = typeof(string).GetMethod("Compare", new Type[] { typeof(string), typeof(string) });
        internal void ElseIfString(object s1, Cmp cmpOp, object s2)
        {
            IfState ifState = (IfState)blockStack.Pop();
            Br(ifState.EndIf);
            MarkLabel(ifState.ElseBegin);

            Load(s1);
            Load(s2);
            Call(stringCompare);
            Load(0);
            ifState.ElseBegin = DefineLabel();

            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Branch if " + GetCmpInverse(cmpOp).ToString() + " to " + ifState.ElseBegin.GetHashCode().ToString(NumberFormatInfo.InvariantInfo));

            ilGen.Emit(GetBranchCode(cmpOp), ifState.ElseBegin);
            blockStack.Push(ifState);
        }

        internal void IfString(object s1, Cmp cmpOp, object s2)
        {
            Load(s1);
            Load(s2);
            Call(stringCompare);
            Load(0);
            If(cmpOp);
        }

        static MethodInfo stringEquals = typeof(string).GetMethod("Equals", new Type[]{ typeof(string), typeof(string)});
        static ConstructorInfo permissionSetCtor = typeof(PermissionSet).GetConstructor(new Type[]{ typeof(PermissionState)});
        static MethodInfo permissionSetDemand = typeof(PermissionSet).GetMethod("Demand", new Type[0]);

        internal void AndIf(Cmp cmpOp)
        {
            IfState ifState = (IfState) blockStack.Peek();
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Branch if " + CmpInverse[(int) cmpOp].ToString() + " to " + ifState.ElseBegin.GetHashCode().ToString());
            ilGen.Emit(BranchCode[(int)cmpOp], ifState.ElseBegin);
        }

        internal void AndIf()
        {
            IfState ifState = (IfState) blockStack.Peek();
            Brfalse(ifState.ElseBegin);
        }
        internal void ElseIf(object boolVal)
        {
            InternalElseIf(boolVal, false);
        }
        void InternalElseIf(object boolVal, bool negate)
        {
            IfState ifState = (IfState)blockStack.Pop();
            Br(ifState.EndIf);
            MarkLabel(ifState.ElseBegin);

            Load(boolVal);
            ifState.ElseBegin = DefineLabel();
            if (negate)
                Brtrue(ifState.ElseBegin);
            else
                Brfalse(ifState.ElseBegin);
            blockStack.Push(ifState);
        }
        
        internal void While(object value1, Cmp cmpOp, object value2)
        {
            IfState ifState = new IfState();
            ifState.EndIf = DefineLabel();
            ifState.ElseBegin = DefineLabel();
            ilGen.MarkLabel(ifState.ElseBegin);
            Load(value1);
            Load(value2);
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Branch if " + CmpInverse[(int) cmpOp].ToString() + " to " + ifState.ElseBegin.GetHashCode().ToString());
            ilGen.Emit(BranchCode[(int)cmpOp], ifState.EndIf);
            blockStack.Push(ifState);
        }

        internal void EndWhile()
        {
            IfState ifState = PopIfState();
            Br(ifState.ElseBegin);
            MarkLabel(ifState.EndIf);
        }

        internal void ElseIfNot(object boolVal)
        {
            InternalElseIf(boolVal, true);
        }

        }

        internal void Ldc(long l)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Ldc.i8 " + l);
            ilGen.Emit(OpCodes.Ldc_I8, l);
        }

        internal void Ldc(float f)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Ldc.r4 " + f);
            ilGen.Emit(OpCodes.Ldc_R4, f);
        }

        internal void Ldc(double d)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Ldc.r8 " + d);
            ilGen.Emit(OpCodes.Ldc_R8, d);
        }

        internal void IsInst(Type type)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("IsInst " + DataContract.GetClrTypeFullName(type));
            ilGen.Emit(OpCodes.Isinst, type);
        }

        internal void Clt()
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Clt");
            ilGen.Emit(OpCodes.Clt);
        }

        internal void StringEquals()
        {
            Call(stringEquals);
        }

        internal void StringEquals(object s1, object s2)
        {
            Load(s1);
            ConvertValue(GetVariableType(s1), typeof(string));
            Load(s2);
            ConvertValue(GetVariableType(s2), typeof(string));
            StringEquals();
        }

        internal void Bge(Label label)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Bge " + label.GetHashCode());
            ilGen.Emit(OpCodes.Bge, label);
        }

        internal void Beq(Label label)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Beq " + label.GetHashCode());
            ilGen.Emit(OpCodes.Beq, label);
        }

        internal void Bne(Label label)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Bne " + label.GetHashCode());
            ilGen.Emit(OpCodes.Bne_Un, label);
        }
        internal void ResizeArray(object arrayVar, object sizeVar)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceComment("ResizeArray() {");

            Label doResize = DefineLabel();
            Load(arrayVar);
            Load(null);
            Beq(doResize);
            Ldlen(arrayVar);
            Load(sizeVar);
            If(Cmp.NotEqualTo);
            MarkLabel(doResize);
            Type arrayType = GetVariableType(arrayVar);
            Type elementType = arrayType.GetElementType();
            LocalBuilder tempArray = DeclareLocal(arrayType, "tempArray");
            NewArray(elementType, sizeVar);
            Store(tempArray);
            CopyArray(arrayVar, tempArray, sizeVar);
            Load(tempArray);
            Store(arrayVar);
            EndIf();

            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceComment("} // ResizeArray");
        }

        LocalBuilder resizeLen, resizeCounter;
        internal void EnsureArrayCapacity(object arrayVar, object lastElementVar)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceComment("EnsureArrayCapacity() {");

            Type arrayType = GetVariableType(arrayVar);
            Type elementType = arrayType.GetElementType();
            If(arrayVar, Cmp.EqualTo, null);
            NewArray(elementType, 4);
            Store(arrayVar);
            Else();
            Load(lastElementVar);
            Ldlen(arrayVar);
            If(Cmp.GreaterThanOrEqualTo);
            LocalBuilder tempArray = DeclareLocal(arrayType, "tempArray");
            if (resizeLen == null)
                resizeLen = DeclareLocal(typeof(int), "resizeLen");
            Load(lastElementVar);
            Load(2);
            Mul();
            Store(resizeLen);
            NewArray(elementType, resizeLen);
            Store(tempArray);
            CopyArray(arrayVar, tempArray, arrayVar);
            Load(tempArray);
            Store(arrayVar);
            EndIf();
            EndIf();

            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceComment("} // EnsureArrayCapacity");
        }

        internal void CopyArray(object sourceArray, object destArray, object length)
        {
            If(sourceArray, Cmp.NotEqualTo, null);
            if (resizeCounter == null)
                resizeCounter = DeclareLocal(typeof(int), "resizeCounter");
            For(resizeCounter, 0, length);
            Load(destArray);
            Load(resizeCounter);
            LoadArrayElement(sourceArray, resizeCounter);
            Stelem(GetVariableType(destArray).GetElementType());
            EndFor();
            EndIf();
        }
        internal void GotoMethodEnd()
        {
            Br(this.methodEndLabel);
        }

        internal void AndWhile(Cmp cmpOp)
        {
            object startWhile = blockStack.Pop();
            AndIf(cmpOp);
            blockStack.Push(startWhile);
        }

        internal void BeginWhileBody(Cmp cmpOp)
        {
            Label startWhile = (Label) blockStack.Pop();
            If(cmpOp);
            blockStack.Push(startWhile);
        }

        internal void Cne()
        {
            Ceq();
            Load(0);
            Ceq();
        }

        internal void New(ConstructorInfo constructorInfo, object param1, object param2)
        {
            LoadParam(param1, 1, constructorInfo);
            LoadParam(param2, 2, constructorInfo);
            New(constructorInfo);
        }

        //This code is not tested
        internal void Stind(Type type)
        {
            OpCode opCode = StindOpCodes[(int) Type.GetTypeCode(type)];
            if (!opCode.Equals(OpCodes.Nop))
            {
                if (codeGenTrace != CodeGenTrace.None)
                    EmitSourceInstruction(opCode.ToString());
                ilGen.Emit(opCode);
            }
            else
            {
                if (codeGenTrace != CodeGenTrace.None)
                    EmitSourceInstruction("Stobj " + type);
                ilGen.Emit(OpCodes.Stobj, type);
            }
        }
        //This code is not tested
        internal void StoreOutParam(ArgBuilder arg, object value)
        {
            Type destType = arg.ArgType;
            if (!destType.IsByRef)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.OutParametersMustBeByRefTypeReceived, DataContract.GetClrTypeFullName(destType))));
            destType = destType.GetElementType();

            Type sourceType;
            if (value is ArgBuilder)
                sourceType = ((ArgBuilder) value).ArgType;
            else if (value is LocalBuilder)
                sourceType = ((LocalBuilder) value).LocalType;
            else if (value != null)
                sourceType = value.GetType();
            else
                sourceType = null;

            Load(arg);
            Load(value);
            if (sourceType != null)
                ConvertAddress(sourceType, destType);
            Stind(destType);
        }
        void CheckSecurity(FieldInfo field)
        {
            if (fullTrustDemanded)
                return;
            if (IsProtectedWithSecurity(field))
                DemandFullTrust();
        }

        void CheckSecurity(MethodBase method)
        {
            if (fullTrustDemanded)
                return;
            if (IsProtectedWithSecurity(method))
                DemandFullTrust();
        }

        void CheckSecurity(Type type)
        {
            if (fullTrustDemanded)
                return;
            if (IsProtectedWithSecurity(type))
                DemandFullTrust();
        }

        void CheckSecurity(Assembly assembly)
        {
            if (fullTrustDemanded)
                return;
            if (IsProtectedWithSecurity(assembly))
                DemandFullTrust();
        }

        static bool IsProtectedWithSecurity(FieldInfo field)
        {
            return IsProtectedWithSecurity(field.DeclaringType);
        }

        static bool IsProtectedWithSecurity(Type type)
        {
            return IsProtectedWithSecurity(type.Assembly) || (type.Attributes & TypeAttributes.HasSecurity) != 0;
        }

        static bool IsProtectedWithSecurity(Assembly assembly)
        {
            object[] attrs = assembly.GetCustomAttributes(typeof(AllowPartiallyTrustedCallersAttribute), true);
            bool hasAptca = attrs != null && attrs.Length > 0;
            return !hasAptca;
        }

        void DemandFullTrust()
        {
            fullTrustDemanded = true;
/*
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceComment("DemandFullTrust() {");

            Ldc(PermissionState.Unrestricted);
            New(permissionSetCtor);
            Call(permissionSetDemand);

            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceComment("}");
*/
        }

        static bool IsProtectedWithSecurity(MethodBase method)
        {
            return false;
            //return (method.Attributes & MethodAttributes.HasSecurity) != 0;
        }

#endif

    }


    internal class ArgBuilder
    {
        internal int Index;
        internal Type ArgType;
        internal ArgBuilder(int index, Type argType)
        {
            this.Index = index;
            this.ArgType = argType;
        }
    }

    internal class ForState
    {
        LocalBuilder indexVar;
        Label beginLabel;
        Label testLabel;
        Label endLabel;
        bool requiresEndLabel;
        object end;

        internal ForState(LocalBuilder indexVar, Label beginLabel, Label testLabel, object end)
        {
            this.indexVar = indexVar;
            this.beginLabel = beginLabel;
            this.testLabel = testLabel;
            this.end = end;
        }

        internal LocalBuilder Index
        {
            get
            {
                return indexVar;
            }
        }

        internal Label BeginLabel
        {
            get
            {
                return beginLabel;
            }
        }

        internal Label TestLabel
        {
            get
            {
                return testLabel;
            }
        }

        internal Label EndLabel
        {
            get
            {
                return endLabel;
            }
            set
            {
                endLabel = value;
            }
        }

        internal bool RequiresEndLabel
        {
            get
            {
                return requiresEndLabel;
            }
            set
            {
                requiresEndLabel = value;
            }
        }

        internal object End
        {
            get
            {
                return end;
            }
        }
    }

    internal enum Cmp
    {
        LessThan,
        EqualTo,
        LessThanOrEqualTo,
        GreaterThan,
        NotEqualTo,
        GreaterThanOrEqualTo
    }

    internal class IfState
    {
        Label elseBegin;
        Label endIf;

        internal Label EndIf
        {
            get
            {
                return this.endIf;
            }
            set
            {
                this.endIf = value;
            }
        }

        internal Label ElseBegin
        {
            get
            {
                return this.elseBegin;
            }
            set
            {
                this.elseBegin = value;
            }
        }

    }

#if NotUsed
    internal class BitFlagsGenerator
    {
        LocalBuilder[] locals;
        CodeGenerator ilg;
        int bitCount;
        internal BitFlagsGenerator(int bitCount, CodeGenerator ilg, string localName)
        {
            this.ilg = ilg;
            this.bitCount = bitCount;
            int localCount = (bitCount+7)/8;
            locals = new LocalBuilder[localCount];
            for (int i=0;i<locals.Length;i++)
                locals[i] = ilg.DeclareLocal(Globals.TypeOfByte, localName+i, (byte)0);
        }
        internal void Store(int bitIndex, bool value)
        {
            LocalBuilder local = locals[GetByteIndex(bitIndex)];
            byte bitValue = GetBitValue(bitIndex);
            if (value)
            {
                ilg.Load(local);
                ilg.Load(bitValue);
                ilg.Or();
                ilg.Stloc(local);
            }
            else
            {
                ilg.Load(local);
                ilg.Load(bitValue);
                ilg.Not();
                ilg.And();
                ilg.Stloc(local);
            }
        }

        internal void Load(int bitIndex)
        {
            LocalBuilder local = locals[GetByteIndex(bitIndex)];
            byte bitValue = GetBitValue(bitIndex);
            ilg.Load(local);
            ilg.Load(bitValue);
            ilg.And();
            ilg.Load(bitValue);
            ilg.Ceq();
        }

        internal void LoadArray()
        {
            LocalBuilder localArray = ilg.DeclareLocal(Globals.TypeOfByteArray, "localArray");
            ilg.NewArray(Globals.TypeOfByte, locals.Length);
            ilg.Store(localArray);
            for (int i=0;i<locals.Length;i++)
                ilg.StoreArrayElement(localArray, i, locals[i]);
            ilg.Load(localArray);
        }

        internal int GetLocalCount()
        {
            return locals.Length;
        }

        internal int GetBitCount()
        {
            return bitCount;
        }

        internal LocalBuilder GetLocal(int i)
        {
            return locals[i];
        }

        internal static bool IsBitSet(byte[] bytes, int bitIndex)
        {
            int byteIndex = GetByteIndex(bitIndex);
            byte bitValue = GetBitValue(bitIndex);
            return (bytes[byteIndex] & bitValue) == bitValue;
        }

        internal static void SetBit(byte[] bytes, int bitIndex)
        {
            int byteIndex = GetByteIndex(bitIndex);
            byte bitValue = GetBitValue(bitIndex);
            bytes[byteIndex] |= bitValue;
        }

        static int GetByteIndex(int bitIndex)
        {
            return bitIndex >> 3;
        }

        static byte GetBitValue(int bitIndex)
        {
            return (byte)(1 << (bitIndex & 7));
        }
    }
#endif

    internal class SwitchState
    {
        Label defaultLabel;
        Label endOfSwitchLabel;
        bool defaultDefined;
        internal SwitchState(Label defaultLabel, Label endOfSwitchLabel)
        {
            this.defaultLabel = defaultLabel;
            this.endOfSwitchLabel = endOfSwitchLabel;
            this.defaultDefined = false;
        }
        internal Label DefaultLabel
        {
            get
            {
                return defaultLabel;
            }
        }

        internal Label EndOfSwitchLabel
        {
            get
            {
                return endOfSwitchLabel;
            }
        }
        internal bool DefaultDefined
        {
            get
            {
                return defaultDefined;
            }
            set
            {
                defaultDefined = value;
            }
        }
    }

}


