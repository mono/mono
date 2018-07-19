//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.ServiceModel.Channels;

    class DurableInstanceProvider : IInstanceProvider
    {
        DurableInstanceContextProvider durableInstanceContextProvider;

        public DurableInstanceProvider(DurableInstanceContextProvider instanceContextProvider)
        {
            this.durableInstanceContextProvider = instanceContextProvider;
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return ((IInstanceProvider) this).GetInstance(instanceContext, null);
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            if (instanceContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("instanceContext");
            }

            return instanceContext.Extensions.Find<DurableInstance>();
        }

        public virtual void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            if (instanceContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("instanceContext");
            }

            if (instance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("instance");
            }

            DurableInstance durableInstance = (DurableInstance) instance;

            if (instanceContext.State == CommunicationState.Faulted || instanceContext.Aborted)
            {
                durableInstance.Abort();
                this.durableInstanceContextProvider.UnbindAbortedInstance(instanceContext, durableInstance.InstanceId);
            }
            else if (instanceContext.State == CommunicationState.Closed)
            {
                durableInstance.Close();
            }
        }
    }
}
