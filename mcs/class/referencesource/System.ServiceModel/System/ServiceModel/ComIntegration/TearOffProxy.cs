//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;
    using System.ServiceModel;

    class TearOffProxy : RealProxy, IDisposable
    {
        ICreateServiceChannel serviceChannelCreator;
        Dictionary<MethodBase, MethodBase> baseTypeToInterfaceMethod;
        internal TearOffProxy(ICreateServiceChannel serviceChannelCreator, Type proxiedType)
            : base(proxiedType)
        {
            if (serviceChannelCreator == null)
            {
                throw Fx.AssertAndThrow("ServiceChannelCreator cannot be null");
            }
            this.serviceChannelCreator = serviceChannelCreator;
            baseTypeToInterfaceMethod = new Dictionary<MethodBase, MethodBase>();
        }

        public override IMessage Invoke(IMessage message)
        {

            RealProxy delegatingProxy = null;
            IMethodCallMessage msg = message as IMethodCallMessage;
            try
            {
                delegatingProxy = serviceChannelCreator.CreateChannel();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;

                return new ReturnMessage(DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(e.GetBaseException().Message, Marshal.GetHRForException(e.GetBaseException()))), msg);
            }

            MethodBase typeMethod = msg.MethodBase;
            IRemotingTypeInfo typeInfo = delegatingProxy as IRemotingTypeInfo;
            if (typeInfo == null)
            {
                throw Fx.AssertAndThrow("Type Info cannot be null");
            }
            if (typeInfo.CanCastTo(typeMethod.DeclaringType, null))
            {
                IMessage msgReturned = delegatingProxy.Invoke(message);
                ReturnMessage returnMsg = msgReturned as ReturnMessage;
                if ((returnMsg == null) || (returnMsg.Exception == null))
                    return msgReturned;
                else
                    return new ReturnMessage(DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(returnMsg.Exception.GetBaseException().Message, Marshal.GetHRForException(returnMsg.Exception.GetBaseException()))), msg);
            }
            else
            {
                return new ReturnMessage(DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(SR.GetString(SR.OperationNotFound, typeMethod.Name), HR.DISP_E_UNKNOWNNAME)), msg);
            }
        }

        void IDisposable.Dispose()
        {
            serviceChannelCreator = null;
        }
    }
}
