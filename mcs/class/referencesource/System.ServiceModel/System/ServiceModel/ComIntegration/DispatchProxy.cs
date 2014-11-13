//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;

    class DispatchProxy : IPseudoDispatch, IDisposable
    {
        ContractDescription contract;
        IProvideChannelBuilderSettings channelBuilderSettings;
        Dictionary<UInt32, string> dispToName = new Dictionary<UInt32, string>();
        Dictionary<string, UInt32> nameToDisp = new Dictionary<string, UInt32>();
        Dictionary<UInt32, MethodInfo> dispToOperationDescription = new Dictionary<UInt32, MethodInfo>();

        private DispatchProxy(ContractDescription contract, IProvideChannelBuilderSettings channelBuilderSettings)
        {
            if (channelBuilderSettings == null)
            {
                throw Fx.AssertAndThrow("channelBuilderSettings cannot be null cannot be null");
            }
            if (contract == null)
            {
                throw Fx.AssertAndThrow("contract cannot be null");
            }
            this.channelBuilderSettings = channelBuilderSettings;
            this.contract = contract;
            ProcessContractDescription();
            ComPlusDispatchMethodTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationDispatchMethod,
                    SR.TraceCodeComIntegrationDispatchMethod, dispToOperationDescription);
        }

        internal static ComProxy Create(IntPtr outer, ContractDescription contract, IProvideChannelBuilderSettings channelBuilderSettings)
        {
            DispatchProxy proxy = null;
            IntPtr inner = IntPtr.Zero;
            ComProxy comProxy = null;
            try
            {
                proxy = new DispatchProxy(contract, channelBuilderSettings);
                inner = OuterProxyWrapper.CreateDispatchProxy(outer, proxy);
                comProxy = new ComProxy(inner, proxy);
                return comProxy;

            }
            finally
            {
                if (comProxy == null)
                {
                    if (proxy != null)
                    {
                        ((IDisposable)proxy).Dispose();
                    }
                    if (inner != IntPtr.Zero)
                    {
                        Marshal.Release(inner);
                    }
                }

            }


        }
        [Serializable]
        internal class ParamInfo
        {
            public int inIndex;
            public int outIndex;
            public string name;
            public Type type;
            public ParamInfo()
            {
                inIndex = -1;
                outIndex = -1;
            }
        }


        internal class MethodInfo
        {
            public OperationDescription opDesc;
            public List<ParamInfo> paramList;
            public Dictionary<uint, ParamInfo> dispIdToParamInfo;
            public ParamInfo ReturnVal = null;
            public MethodInfo(OperationDescription opDesc)
            {
                this.opDesc = opDesc;
                paramList = new List<ParamInfo>();
                dispIdToParamInfo = new Dictionary<uint, ParamInfo>();
            }
        }

        void ProcessContractDescription()
        {
            UInt32 dispIndex = 10;
            Dictionary<string, ParamInfo> paramDictionary = null;

            foreach (OperationDescription opDesc in contract.Operations)
            {
                dispToName[dispIndex] = opDesc.Name;
                nameToDisp[opDesc.Name] = dispIndex;
                MethodInfo methodInfo = null;
                methodInfo = new MethodInfo(opDesc);
                dispToOperationDescription[dispIndex++] = methodInfo;
                paramDictionary = new Dictionary<string, ParamInfo>();
                bool inVars = true;
                inVars = true;
                int paramCount = 0;

                foreach (MessageDescription msgDesc in opDesc.Messages)
                {
                    paramCount = 0;


                    if (msgDesc.Body.ReturnValue != null)
                    {
                        if (string.IsNullOrEmpty(msgDesc.Body.ReturnValue.BaseType))
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(SR.GetString(SR.CannotResolveTypeForParamInMessageDescription, "ReturnValue", msgDesc.Body.WrapperName, msgDesc.Body.WrapperNamespace), HR.DISP_E_MEMBERNOTFOUND));

                        msgDesc.Body.ReturnValue.Type = Type.GetType(msgDesc.Body.ReturnValue.BaseType);
                    }
                    foreach (MessagePartDescription param in msgDesc.Body.Parts)
                    {

                        UInt32 dispID = 0;
                        ParamInfo paramInfo = null;
                        paramInfo = null;
                        if (!nameToDisp.TryGetValue(param.Name, out dispID))
                        {

                            dispToName[dispIndex] = param.Name;
                            nameToDisp[param.Name] = dispIndex;
                            dispID = dispIndex;
                            dispIndex++;
                        }
                        if (!paramDictionary.TryGetValue(param.Name, out paramInfo))
                        {

                            paramInfo = new ParamInfo();
                            methodInfo.paramList.Add(paramInfo);
                            methodInfo.dispIdToParamInfo[dispID] = paramInfo;
                            if (string.IsNullOrEmpty(param.BaseType))
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(SR.GetString(SR.CannotResolveTypeForParamInMessageDescription, param.Name, msgDesc.Body.WrapperName, msgDesc.Body.WrapperNamespace), HR.DISP_E_MEMBERNOTFOUND));
                            paramInfo.type = Type.GetType(param.BaseType, true);
                            paramInfo.name = param.Name;
                            paramDictionary[param.Name] = paramInfo;
                            param.Index = paramCount;


                        }
                        param.Type = paramInfo.type;
                        if (inVars)
                        {
                            paramInfo.inIndex = paramCount;
                        }
                        else
                        {
                            paramInfo.outIndex = paramCount;

                        }

                        paramCount++;


                    }

                    inVars = false;

                }

            }

        }

        void IPseudoDispatch.GetIDsOfNames(UInt32 cNames, // size_is param for rgszNames
                    [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 0)] string[] rgszNames,
                     IntPtr pDispID)
        {
            for (int index = 0; index < cNames; index++)
            {

                UInt32 dispID;
                if (!nameToDisp.TryGetValue(rgszNames[index], out dispID))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(SR.GetString(SR.OperationNotFound, rgszNames[index]), HR.DISP_E_UNKNOWNNAME));

                Marshal.WriteInt32(pDispID, index * sizeof(int), (int)dispID);
            }
        }

        int IPseudoDispatch.Invoke(
                    UInt32 dispIdMember,
                    UInt32 cArgs,
                    UInt32 cNamedArgs,
                    IntPtr rgvarg,
                    [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] UInt32[] rgdispidNamedArgs,
                    IntPtr pVarResult,
                    IntPtr pExcepInfo,
                    out UInt32 pArgErr
                )
        {
            pArgErr = 0;
            try
            {
                if (cNamedArgs > 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(SR.GetString(SR.NamedArgsNotSupported), HR.DISP_E_BADPARAMCOUNT));
                MethodInfo mInfo = null;
                if (!dispToOperationDescription.TryGetValue(dispIdMember, out mInfo))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(SR.GetString(SR.BadDispID, dispIdMember), HR.DISP_E_MEMBERNOTFOUND));
                object[] ins = null;
                object[] outs = null;
                string action = null;

                if (mInfo.paramList.Count != cArgs)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(SR.GetString(SR.BadDispID, dispIdMember), HR.DISP_E_BADPARAMCOUNT));
                ins = new object[mInfo.opDesc.Messages[0].Body.Parts.Count];
                outs = new object[mInfo.opDesc.Messages[1].Body.Parts.Count];
                if (cArgs > 0)
                {
                    if (mInfo.opDesc.Messages[0].Body.Parts.Count > 0)
                    {
                        for (int index = 0; index < mInfo.opDesc.Messages[0].Body.Parts.Count; index++)
                            ins[index] = null;
                    }
                    if (!mInfo.opDesc.IsOneWay && (mInfo.opDesc.Messages[1].Body.Parts.Count > 0))
                    {
                        for (int index = 0; index < mInfo.opDesc.Messages[1].Body.Parts.Count; index++)
                            outs[index] = null;

                    }
                }
                action = mInfo.opDesc.Messages[0].Action;

                // First we take care of positional arguments
                int inCount = 0;
                for (int index = 0; index < cArgs; index++)
                {
                    if (mInfo.paramList[index].inIndex != -1)
                    {
                        try
                        {

                            object val = null;
                            if (!mInfo.paramList[index].type.IsArray)
                                val = FetchVariant(rgvarg, (int)(cArgs - index - 1), mInfo.paramList[index].type);
                            else
                                val = FetchVariants(rgvarg, (int)(cArgs - index - 1), mInfo.paramList[index].type);
                            ins[mInfo.paramList[index].inIndex] = val;
                            inCount++;
                        }
                        catch (ArgumentNullException)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(SR.GetString(SR.VariantArrayNull, cArgs - index - 1));
                        }

                    }
                }

                if (inCount != ins.Length)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(SR.GetString(SR.BadParamCount), HR.DISP_E_BADPARAMCOUNT));


                object result = null;
                try
                {
                    result = SendMessage(mInfo.opDesc, action, ins, outs);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                        throw;

                    if (pExcepInfo != IntPtr.Zero)
                    {
                        System.Runtime.InteropServices.ComTypes.EXCEPINFO exceptionInfo = new System.Runtime.InteropServices.ComTypes.EXCEPINFO();
                        e = e.GetBaseException();
                        exceptionInfo.bstrDescription = e.Message;
                        exceptionInfo.bstrSource = e.Source;
                        exceptionInfo.scode = Marshal.GetHRForException(e);
                        Marshal.StructureToPtr(exceptionInfo, pExcepInfo, false);
                    }
                    return HR.DISP_E_EXCEPTION;
                }



                if (!mInfo.opDesc.IsOneWay)
                {
                    if (outs != null)
                    {
                        bool[] filled = new bool[outs.Length];
                        for (UInt32 index = 0; index < filled.Length; index++)
                            filled[index] = false;
                        for (int index = 0; index < cArgs; index++)
                        {
                            if (mInfo.paramList[index].outIndex != -1)
                            {
                                try
                                {
                                    if (IsByRef(rgvarg, (int)(cArgs - index - 1)))
                                    {
                                        PopulateByRef(rgvarg, (int)(cArgs - index - 1), outs[mInfo.paramList[index].outIndex]);
                                    }
                                }
                                catch (ArgumentNullException)
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull(SR.GetString(SR.VariantArrayNull, cArgs - index - 1));
                                }

                                filled[mInfo.paramList[index].outIndex] = true;
                            }
                        }
                    }
                    if ((result != null) && (pVarResult != IntPtr.Zero))
                    {
                        if (!result.GetType().IsArray)
                            Marshal.GetNativeVariantForObject(result, pVarResult);
                        else
                        {
                            Array arr = result as Array;
                            Array arrDest = Array.CreateInstance(typeof(object), arr.Length);
                            arr.CopyTo(arrDest, 0);
                            Marshal.GetNativeVariantForObject(arrDest, pVarResult);
                        }
                    }
                }
                return HR.S_OK;
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                e = e.GetBaseException();
                return Marshal.GetHRForException(e);
            }

        }

        object SendMessage(OperationDescription opDesc, string action, object[] ins, object[] outs)
        {
            ProxyOperationRuntime operationRuntime = channelBuilderSettings.ServiceChannel.ClientRuntime.GetRuntime().GetOperationByName(opDesc.Name);
            if (operationRuntime == null)
            {
                throw Fx.AssertAndThrow("Operation runtime should not be null");
            }
            return channelBuilderSettings.ServiceChannel.Call(action, opDesc.IsOneWay, operationRuntime, ins, outs);
        }

        object FetchVariant(IntPtr baseArray, int index, Type type)
        {
            if (baseArray == IntPtr.Zero)
            {
                Fx.Assert("baseArray should not be null");

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("baseArrray");
            }
            uint displacement = (uint)(index * Marshal.SizeOf(typeof(TagVariant)));

            object ret = Marshal.GetObjectForNativeVariant(GetDisp(baseArray, displacement));

            // this is neccessary because unfortunately the CLR is not very forthcomming when it comes
            // to dynamically converting integer types to other integer types due to boxing
            // the same goes for the array case
            if (type == typeof(Int32))
            {
                if (ret.GetType() == typeof(Int16))
                    ret = (Int32)((Int16)ret);
                else if (ret.GetType() != typeof(Int32))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(SR.GetString(SR.UnsupportedConversion, ret.GetType(), type.GetElementType()), HR.DISP_E_TYPEMISMATCH));
            }
            else if (type == typeof(Int64))
            {
                if (ret.GetType() == typeof(Int16))
                    ret = (Int64)((Int16)ret);
                else if (ret.GetType() == typeof(Int32))
                    ret = (Int64)((Int32)ret);
                else if (ret.GetType() != typeof(Int64))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(SR.GetString(SR.UnsupportedConversion, ret.GetType(), type), HR.DISP_E_TYPEMISMATCH));

            }

            return ret;
        }

        object FetchVariants(IntPtr baseArray, int index, Type type)
        {
            if (baseArray == IntPtr.Zero)
            {
                Fx.Assert("baseArray should not be null");

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("baseArrray");
            }
            uint displacement = (uint)(index * Marshal.SizeOf(typeof(TagVariant)));

            TagVariant varBase = (TagVariant)Marshal.PtrToStructure(GetDisp(baseArray, displacement), typeof(TagVariant));
            if ((varBase.vt & (ushort)(VarEnum.VT_VARIANT | VarEnum.VT_BYREF)) == 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(SR.GetString(SR.OnlyVariantAllowedByRef), HR.DISP_E_TYPEMISMATCH));
            TagVariant varActualVariant = (TagVariant)Marshal.PtrToStructure(varBase.ptr, typeof(TagVariant));

            if ((varActualVariant.vt & (ushort)(VarEnum.VT_VARIANT | VarEnum.VT_BYREF | VarEnum.VT_ARRAY)) == 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(SR.GetString(SR.OnlyByRefVariantSafeArraysAllowed), HR.DISP_E_TYPEMISMATCH));

            IntPtr ppArray = varActualVariant.ptr;
            IntPtr pSafeArray = (IntPtr)Marshal.PtrToStructure(ppArray, typeof(IntPtr));

            int dimensionsOfSafeArray = SafeNativeMethods.SafeArrayGetDim(pSafeArray);
            if (dimensionsOfSafeArray != 1)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(SR.GetString(SR.OnlyOneDimensionalSafeArraysAllowed), HR.DISP_E_TYPEMISMATCH));

            int sizeofElement = SafeNativeMethods.SafeArrayGetElemsize(pSafeArray);
            if (sizeofElement != Marshal.SizeOf(typeof(TagVariant)))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(SR.GetString(SR.OnlyVariantTypeElementsAllowed), HR.DISP_E_TYPEMISMATCH));

            int lBound = SafeNativeMethods.SafeArrayGetLBound(pSafeArray, 1);
            if (lBound > 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(SR.GetString(SR.OnlyZeroLBoundAllowed), HR.DISP_E_TYPEMISMATCH));

            int uBound = SafeNativeMethods.SafeArrayGetUBound(pSafeArray, 1);

            IntPtr pRawData = SafeNativeMethods.SafeArrayAccessData(pSafeArray);
            try
            {
                object[] objects = Marshal.GetObjectsForNativeVariants(pRawData, uBound + 1);

                Array arr = Array.CreateInstance(type.GetElementType(), objects.Length);

                if (objects.Length == 0)
                    return arr;

                if (type.GetElementType() != typeof(Int32) && type.GetElementType() != typeof(Int64))
                {
                    try
                    {
                        objects.CopyTo(arr, 0);
                    }
                    catch (Exception e)
                    {
                        if (Fx.IsFatal(e))
                            throw;

                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(SR.GetString(SR.UnsupportedConversion, objects[0].GetType(), type.GetElementType()), HR.DISP_E_TYPEMISMATCH));
                    }
                }
                else
                {
                    if (type.GetElementType() == typeof(Int32))
                    {
                        for (int i = 0; i < objects.Length; i++)
                        {
                            if (objects[i].GetType() == typeof(Int16))
                                arr.SetValue((Int32)((Int16)objects[i]), i);
                            else if (objects[i].GetType() == typeof(Int32))
                                arr.SetValue(objects[i], i);
                            else
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(SR.GetString(SR.UnsupportedConversion, objects[i].GetType(), type.GetElementType()), HR.DISP_E_TYPEMISMATCH));
                        }
                    }
                    else
                    {
                        for (int i = 0; i < objects.Length; i++)
                        {
                            if (objects[i].GetType() == typeof(Int16))
                                arr.SetValue((Int64)((Int16)objects[i]), i);
                            else if (objects[i].GetType() == typeof(Int32))
                                arr.SetValue((Int64)((Int32)objects[i]), i);
                            else if (objects[i].GetType() == typeof(Int64))
                                arr.SetValue(objects[i], i);
                            else
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(SR.GetString(SR.UnsupportedConversion, objects[i].GetType(), type.GetElementType()), HR.DISP_E_TYPEMISMATCH));
                        }
                    }
                }

                return arr;
            }
            finally
            {
                SafeNativeMethods.SafeArrayUnaccessData(pSafeArray);
            }
        }


        IntPtr GetDisp(IntPtr baseAddress, uint disp)
        {
            long address = (long)baseAddress;
            address += disp;
            return (IntPtr)address;
        }

        void PopulateByRef(IntPtr baseArray, int index, object val)
        {
            if (val != null)
            {
                if (baseArray == IntPtr.Zero)
                {
                    Fx.Assert("baseArray should not be null");

                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("baseArrray");
                }
                uint displacement = (uint)(index * Marshal.SizeOf(typeof(TagVariant)));
                TagVariant var = (TagVariant)Marshal.PtrToStructure(GetDisp(baseArray, displacement), typeof(TagVariant));
                if ((var.vt & (ushort)VarEnum.VT_VARIANT) == 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(SR.GetString(SR.OnlyVariantAllowedByRef), HR.DISP_E_TYPEMISMATCH));
                if (!val.GetType().IsArray)
                    Marshal.GetNativeVariantForObject(val, var.ptr);
                else
                {
                    Array arr = val as Array;
                    Array arrDest = Array.CreateInstance(typeof(object), arr.Length);
                    arr.CopyTo(arrDest, 0);
                    Marshal.GetNativeVariantForObject(arrDest, var.ptr);
                }

            }

        }

        bool IsByRef(IntPtr baseArray, int index)
        {


            if (baseArray == IntPtr.Zero)
            {
                Fx.Assert("baseArray should not be null");

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("baseArrray");
            }
            uint displacement = (uint)(index * Marshal.SizeOf(typeof(TagVariant)));
            ushort vt = (ushort)Marshal.ReadInt16(GetDisp(baseArray, displacement));
            if (0 != (vt & (ushort)(VarEnum.VT_BYREF)))
                return true;
            else
                return false;

        }

        void IDisposable.Dispose()
        {

            dispToName.Clear();
            nameToDisp.Clear();
            dispToOperationDescription.Clear();

        }


    }
}

