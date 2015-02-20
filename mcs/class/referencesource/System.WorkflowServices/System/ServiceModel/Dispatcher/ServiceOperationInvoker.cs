//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;

    class ServiceOperationInvoker : IOperationInvoker
    {
        bool canCreateInstance;
        bool completesInstance;
        bool contractCausesSave;
        IOperationInvoker innerInvoker;

        public ServiceOperationInvoker(IOperationInvoker innerInvoker, bool completesInstance, bool canCreateInstance, bool contractCausesSave)
        {
            if (innerInvoker == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("innerInvoker");
            }

            this.innerInvoker = innerInvoker;
            this.completesInstance = completesInstance;
            this.canCreateInstance = canCreateInstance;
            this.contractCausesSave = contractCausesSave;
        }

        public bool IsSynchronous
        {
            get { return this.innerInvoker.IsSynchronous; }
        }

        public object[] AllocateInputs()
        {
            return this.innerInvoker.AllocateInputs();
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            if (instance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("instance");
            }

            ServiceDurableInstance durableInstance = instance as ServiceDurableInstance;

            if (durableInstance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(
                    SR2.GetString(SR2.InvokeCalledWithWrongType, typeof(DurableServiceAttribute).Name)));
            }

            object serviceInstance = durableInstance.StartOperation(this.canCreateInstance);
            Exception operationException = null;

            bool failFast = false;
            try
            {
                return this.innerInvoker.Invoke(serviceInstance, inputs, out outputs);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    failFast = true;
                    throw;
                }

                operationException = e;
                ServiceErrorHandler.MarkException(e);
                throw;
            }
            finally
            {
                if (!failFast)
                {
                    durableInstance.FinishOperation(this.completesInstance, this.contractCausesSave, operationException);
                }
            }
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            if (instance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("instance");
            }

            ServiceDurableInstance durableInstance = instance as ServiceDurableInstance;

            if (durableInstance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(
                    SR2.GetString(SR2.InvokeCalledWithWrongType, typeof(DurableServiceAttribute).Name)));
            }

            return new InvokeAsyncResult(durableInstance, inputs, this, this.canCreateInstance, callback, state);
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            return InvokeAsyncResult.End(out outputs, result);
        }

        public class InvokeAsyncResult : AsyncResult
        {
            static AsyncCallback finishCallback = Fx.ThunkCallback(new AsyncCallback(FinishComplete));
            static AsyncCallback invokeCallback = Fx.ThunkCallback(new AsyncCallback(InvokeComplete));
            static AsyncCallback startCallback = Fx.ThunkCallback(new AsyncCallback(StartComplete));
            Exception completionException;
            ServiceDurableInstance durableInstance;
            object[] inputs;

            ServiceOperationInvoker invoker;
            OperationContext operationContext;
            object[] outputs;
            object returnValue;
            object serviceInstance;

            public InvokeAsyncResult(ServiceDurableInstance instance, object[] inputs, ServiceOperationInvoker invoker, bool canCreateInstance, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.invoker = invoker;
                this.inputs = inputs;
                this.durableInstance = instance;
                this.operationContext = OperationContext.Current;

                IAsyncResult result = this.durableInstance.BeginStartOperation(canCreateInstance, startCallback, this);

                if (result.CompletedSynchronously)
                {
                    this.serviceInstance = this.durableInstance.EndStartOperation(result);
                    if (DoInvoke())
                    {
                        Complete(true, this.completionException);
                    }
                }
            }

            public static object End(out object[] outputs, IAsyncResult result)
            {
                InvokeAsyncResult invokeResult = AsyncResult.End<InvokeAsyncResult>(result);

                outputs = invokeResult.outputs;

                return invokeResult.returnValue;
            }

            // We pass the exception to another thread
            [SuppressMessage("Reliability", "Reliability104")]
            [SuppressMessage("Microsoft.Design", "CA1031")]
            static void FinishComplete(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                InvokeAsyncResult invokeResult = result.AsyncState as InvokeAsyncResult;
                Fx.Assert(invokeResult != null, "Async state should have been of type InvokeAsyncResult.");

                try
                {
                    invokeResult.durableInstance.EndFinishOperation(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    invokeResult.completionException = e;
                }

                invokeResult.Complete(false, invokeResult.completionException);
            }

            // We pass the exception to another thread
            [SuppressMessage("Reliability", "Reliability104")]
            [SuppressMessage("Microsoft.Design", "CA1031")]
            static void InvokeComplete(IAsyncResult resultParameter)
            {
                if (resultParameter.CompletedSynchronously)
                {
                    return;
                }

                InvokeAsyncResult invokeResult = resultParameter.AsyncState as InvokeAsyncResult;

                Fx.Assert(invokeResult != null,
                    "Async state should have been of type InvokeAsyncResult.");

                try
                {
                    invokeResult.returnValue = invokeResult.invoker.innerInvoker.InvokeEnd(invokeResult.serviceInstance, out invokeResult.outputs, resultParameter);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    ServiceErrorHandler.MarkException(e);
                    invokeResult.completionException = e;
                }
                finally
                {
                    if (invokeResult.DoFinish())
                    {
                        invokeResult.Complete(false, invokeResult.completionException);
                    }
                }
            }

            // We pass the exception to another thread
            [SuppressMessage("Reliability", "Reliability104")]
            [SuppressMessage("Microsoft.Design", "CA1031")]
            static void StartComplete(IAsyncResult resultParameter)
            {
                if (resultParameter.CompletedSynchronously)
                {
                    return;
                }

                InvokeAsyncResult invokeResult = resultParameter.AsyncState as InvokeAsyncResult;

                Fx.Assert(invokeResult != null,
                    "Async state should have been of type InvokeAsyncResult.");

                try
                {
                    invokeResult.serviceInstance = invokeResult.durableInstance.EndStartOperation(resultParameter);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    invokeResult.Complete(false, e);
                    return;
                }

                if (invokeResult.DoInvoke())
                {
                    invokeResult.Complete(false, invokeResult.completionException);
                }
            }

            // We pass the exception to another thread
            [SuppressMessage("Reliability", "Reliability104")]
            [SuppressMessage("Microsoft.Design", "CA1031")]
            bool DoFinish()
            {
                try
                {
                    IAsyncResult result = this.durableInstance.BeginFinishOperation(this.invoker.completesInstance, this.invoker.contractCausesSave, this.completionException, finishCallback, this);

                    if (result.CompletedSynchronously)
                    {
                        this.durableInstance.EndFinishOperation(result);
                        return true;
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    this.completionException = e;
                    return true;
                }

                return false;
            }

            // We pass the exception to another thread
            [SuppressMessage("Reliability", "Reliability104")]
            [SuppressMessage("Microsoft.Design", "CA1031")]
            bool DoInvoke()
            {
                bool finishNow = false;

                try
                {
                    IAsyncResult result = null;

                    using (OperationContextScope operationScope = new OperationContextScope(this.operationContext))
                    {
                        result = this.invoker.innerInvoker.InvokeBegin(this.serviceInstance, this.inputs, invokeCallback, this);
                    }

                    if (result.CompletedSynchronously)
                    {
                        this.returnValue = this.invoker.innerInvoker.InvokeEnd(this.serviceInstance, out this.outputs, result);
                        finishNow = true;
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    ServiceErrorHandler.MarkException(e);
                    this.completionException = e;
                    finishNow = true;
                }

                if (finishNow)
                {
                    if (DoFinish())
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
