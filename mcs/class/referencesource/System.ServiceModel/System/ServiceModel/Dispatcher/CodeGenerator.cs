//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

// ***NOTE*** If this code is changed, make corresponding changes in System.Runtime.Serialization.CodeGenerator also

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime;
    using System.Security;
    using System.ServiceModel.Diagnostics;

    [Fx.Tag.SecurityNote(Critical = "Generates IL into an ILGenerator that was created under an Assert."
        + "Generated IL must be correct and must not subvert the type system.")]
#pragma warning disable 618 // have not moved to the v4 security model yet
    [SecurityCritical(SecurityCriticalScope.Everything)]
#pragma warning restore 618
    internal class CodeGenerator
    {
        static MethodInfo getTypeFromHandle;
        static MethodInfo stringConcat2;
        static MethodInfo objectToString;
        static MethodInfo boxPointer;
        static MethodInfo unboxPointer;

#if USE_REFEMIT
        AssemblyBuilder assemblyBuilder;
        ModuleBuilder moduleBuilder;
        TypeBuilder typeBuilder;
        static int typeCounter;
        MethodBuilder methodBuilder;
#else
        static Module SerializationModule = typeof(CodeGenerator).Module;   // Can be replaced by different assembly with SkipVerification set to false
        DynamicMethod dynamicMethod;

#if DEBUG
        bool allowPrivateMemberAccess;
#endif
#endif

        Type delegateType;
        ILGenerator ilGen;
        ArrayList argList;
        Stack blockStack;
        Label methodEndLabel;

        Hashtable localNames;
        int lineNo = 1;
        enum CodeGenTrace { None, Save, Tron };
        CodeGenTrace codeGenTrace;

        internal CodeGenerator()
        {
            SourceSwitch codeGenSwitch = OperationInvokerTrace.CodeGenerationSwitch;
            if ((codeGenSwitch.Level & SourceLevels.Verbose) == SourceLevels.Verbose)
                codeGenTrace = CodeGenTrace.Tron;
            else if ((codeGenSwitch.Level & SourceLevels.Information) == SourceLevels.Information)
                codeGenTrace = CodeGenTrace.Save;
            else
                codeGenTrace = CodeGenTrace.None;
        }

        static MethodInfo GetTypeFromHandle
        {
            get
            {
                if (getTypeFromHandle == null)
                    getTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle");
                return getTypeFromHandle;
            }
        }

        static MethodInfo StringConcat2
        {
            get
            {
                if (stringConcat2 == null)
                    stringConcat2 = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });
                return stringConcat2;
            }
        }

        static MethodInfo ObjectToString
        {
            get
            {
                if (objectToString == null)
                    objectToString = typeof(object).GetMethod("ToString", new Type[0]);
                return objectToString;
            }
        }

        static MethodInfo BoxPointer
        {
            get
            {
                if (boxPointer == null)
                    boxPointer = typeof(Pointer).GetMethod("Box");
                return boxPointer;
            }
        }

        static MethodInfo UnboxPointer
        {
            get
            {
                if (unboxPointer == null)
                    unboxPointer = typeof(Pointer).GetMethod("Unbox");
                return unboxPointer;
            }
        }

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
#if DEBUG
            this.allowPrivateMemberAccess = allowPrivateMemberAccess;
#endif
#endif

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
            if (codeGenTrace != CodeGenTrace.None)
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

        internal void If()
        {
            InternalIf(false);
        }

        internal void IfNot()
        {
            InternalIf(true);
        }

        internal void Else()
        {
            IfState ifState = PopIfState();
            Br(ifState.EndIf);
            MarkLabel(ifState.ElseBegin);

            ifState.ElseBegin = ifState.EndIf;
            blockStack.Push(ifState);
        }

        internal void EndIf()
        {
            IfState ifState = PopIfState();
            if (!ifState.ElseBegin.Equals(ifState.EndIf))
                MarkLabel(ifState.ElseBegin);
            MarkLabel(ifState.EndIf);
        }

        internal void Call(MethodInfo methodInfo)
        {
            if (methodInfo.IsVirtual)
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

        internal void New(ConstructorInfo constructor)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Newobj " + constructor.ToString() + " on type " + constructor.DeclaringType.ToString());
            ilGen.Emit(OpCodes.Newobj, constructor);
        }

        internal void InitObj(Type valueType)
        {
            if (codeGenTrace != CodeGenTrace.None)
                EmitSourceInstruction("Initobj " + valueType);
            ilGen.Emit(OpCodes.Initobj, valueType);
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
            Type objType = GetVariableType(obj).GetElementType();
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

        static bool IsStruct(Type objType)
        {
            return objType.IsValueType && !objType.IsPrimitive;
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
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxCodeGenCanOnlyStoreIntoArgOrLocGot0, var.GetType().FullName)));
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
                        Ldc((int)(char)o);
                        break;
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
                    case TypeCode.String:
                        Ldstr((string)o);
                        break;
                    case TypeCode.UInt64:
                    case TypeCode.Int64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Object:
                    case TypeCode.Decimal:
                    case TypeCode.DateTime:
                    case TypeCode.Empty:
                    case TypeCode.DBNull:
                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxCodeGenUnknownConstantType, valueType.FullName)));
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
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxCodeGenArrayTypeIsNotSupported, arrayElementType.FullName)));
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

        internal void Stelem(Type arrayElementType)
        {
            if (arrayElementType.IsEnum)
                Stelem(Enum.GetUnderlyingType(arrayElementType));
            else
            {
                OpCode opCode = GetStelemOpCode(Type.GetTypeCode(arrayElementType));
                if (opCode.Equals(OpCodes.Nop))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxCodeGenArrayTypeIsNotSupported, arrayElementType.FullName)));
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
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxCodeGenNoConversionPossibleTo, target.FullName)));
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
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxCodeGenIsNotAssignableFrom, target.FullName, source.FullName)));
            }
            else if (target.IsPointer)
            {
                Call(UnboxPointer);
            }
            else if (source.IsPointer)
            {
                Load(source);
                Call(BoxPointer);
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
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxCodeGenIsNotAssignableFrom, target.FullName, source.FullName)));
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
            //if (assemblyBuilder == null) {
            AssemblyName name = new AssemblyName();
            name.Name = "Microsoft.GeneratedCode."+methodName;
            bool saveAssembly = false;

            if (codeGenTrace != CodeGenTrace.None)
                saveAssembly = true;

            if (saveAssembly)
            {
                assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);
                moduleBuilder = assemblyBuilder.DefineDynamicModule(name.Name, name.Name + ".dll", false);
            }
            else
            {
                assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
                moduleBuilder = assemblyBuilder.DefineDynamicModule(name.Name, false);
            }
            //}
        }
#endif

        void ThrowMismatchException(object expected)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.SFxCodeGenExpectingEnd, expected.ToString())));
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

        OpCode GetLdelemOpCode(TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.Object:
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

        OpCode GetStelemOpCode(TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.Object:
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
                OperationInvokerTrace.WriteInstruction(lineNo++, line);
            if (ilGen != null && codeGenTrace == CodeGenTrace.Tron)
            {
                ilGen.Emit(OpCodes.Ldstr, string.Format(CultureInfo.InvariantCulture, "{0:00000}: {1}", lineNo - 1, line));
                ilGen.Emit(OpCodes.Call, OperationInvokerTrace.TraceInstructionMethod);
            }
        }

        internal void EmitStackTop(Type stackTopType)
        {
            if (codeGenTrace != CodeGenTrace.Tron)
                return;
            codeGenTrace = CodeGenTrace.None;
            Dup();
            ToString(stackTopType);
            LocalBuilder topValue = DeclareLocal(typeof(string), "topValue");
            Store(topValue);
            Load("//value = ");
            Load(topValue);
            Concat2();
            Call(OperationInvokerTrace.TraceInstructionMethod);
            codeGenTrace = CodeGenTrace.Tron;
        }

        internal void ToString(Type type)
        {
            if (type.IsValueType)
            {
                Box(type);
                Call(ObjectToString);
            }
            else
            {
                Dup();
                IfNot();
                Pop();
                Load("<null>");
                Else();
                Call(ObjectToString);
                EndIf();
            }
        }

        internal void Concat2()
        {
            Call(StringConcat2);
        }

        internal void LoadZeroValueIntoLocal(Type type, LocalBuilder local)
        {
            if (type.IsValueType)
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                    case TypeCode.Char:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                        ilGen.Emit(OpCodes.Ldc_I4_0);
                        Store(local);
                        break;
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        ilGen.Emit(OpCodes.Ldc_I4_0);
                        ilGen.Emit(OpCodes.Conv_I8);
                        Store(local);
                        break;
                    case TypeCode.Single:
                        ilGen.Emit(OpCodes.Ldc_R4, 0.0F);
                        Store(local);
                        break;
                    case TypeCode.Double:
                        ilGen.Emit(OpCodes.Ldc_R8, 0.0);
                        Store(local);
                        break;
                    case TypeCode.Decimal:
                    case TypeCode.DateTime:
                    default:
                        LoadAddress(local);
                        InitObj(type);
                        break;
                }
            }
            else
            {
                Load(null);
                Store(local);
            }
        }
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

}


