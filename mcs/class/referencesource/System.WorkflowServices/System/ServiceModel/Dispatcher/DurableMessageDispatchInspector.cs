//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.ServiceModel.Channels;

    class DurableMessageDispatchInspector : IDispatchMessageInspector
    {
        public const string NewDurableInstanceIdPropertyName = "newDurableInstanceIdProperty";
        const string suppressContextOnReply = "suppressContextOnReply";
        SessionMode sessionMode;

        public DurableMessageDispatchInspector(SessionMode sessionMode)
        {
            this.sessionMode = sessionMode;
        }

        public static void SuppressContextOnReply(OperationContext operationContext)
        {
            if (operationContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("operationContext");
            }
            operationContext.OutgoingMessageProperties[suppressContextOnReply] = true;
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            if (instanceContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("instanceContext");
            }

            if (request == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("request");
            }

            if (sessionMode != SessionMode.NotAllowed)
            {
                object result = null;

                if (request.Properties.TryGetValue(NewDurableInstanceIdPropertyName, out result))
                {
                    return result.ToString();
                }
            }
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            try
            {
                if (reply != null)
                {
                    ContextMessageProperty context = null;

                    if (sessionMode == SessionMode.NotAllowed || reply.Properties.ContainsKey(suppressContextOnReply))
                    {
                        if (ContextMessageProperty.TryGet(reply, out context))
                        {
                            context.Context.Clear();
                        }
                    }
                    else
                    {
                        string newInstanceId = correlationState as string;

                        if (newInstanceId != null)
                        {

                            if (!ContextMessageProperty.TryGet(reply, out context))
                            {
                                context = new ContextMessageProperty();
                                context.Context[WellKnownContextProperties.InstanceId] = newInstanceId;
                                context.AddOrReplaceInMessage(reply);
                            }
                            else
                            {
                                context.Context[WellKnownContextProperties.InstanceId] = newInstanceId;
                            }
                        }
                    }
                }
            }
            finally
            {
                DurableInstance durableInstance = OperationContext.Current.InstanceContext.Extensions.Find<DurableInstance>();

                if (durableInstance == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(
                        SR2.GetString(
                        SR2.RequiredInstanceContextExtensionNotFound,
                        typeof(DurableInstance).Name)));
                }
                //Decrement InstanceActivity Count
                durableInstance.DecrementActivityCount();
            }
        }
    }
}
