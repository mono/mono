using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace System.Net
{
    internal class ServerCertValidationCallback
    {
        readonly RemoteCertificateValidationCallback m_ValidationCallback;
        readonly ExecutionContext                    m_Context;

        internal ServerCertValidationCallback(RemoteCertificateValidationCallback validationCallback)
        {
            m_ValidationCallback = validationCallback;
            m_Context = ExecutionContext.Capture();
        }

        internal RemoteCertificateValidationCallback ValidationCallback {
            get { return m_ValidationCallback;}
        }

        internal void Callback(object state)
        {
            CallbackContext context = (CallbackContext) state;
            context.result = m_ValidationCallback(context.request,
                                                  context.certificate,
                                                  context.chain,
                                                  context.sslPolicyErrors);
        }

        internal bool Invoke(object request,
                             X509Certificate certificate,
                             X509Chain chain,
                             SslPolicyErrors sslPolicyErrors)
        {
            if (m_Context == null)
            {
                return m_ValidationCallback(request, certificate, chain, sslPolicyErrors);
            }
            else
            {
                ExecutionContext execContext = m_Context.CreateCopy();
                CallbackContext callbackContext = new CallbackContext(request,
                                                                      certificate,
                                                                      chain,
                                                                      sslPolicyErrors);
                ExecutionContext.Run(execContext, Callback, callbackContext);
                return callbackContext.result;
            }
        }

        private class CallbackContext
        {
            internal readonly Object request;
            internal readonly X509Certificate certificate;
            internal readonly X509Chain chain;
            internal readonly SslPolicyErrors sslPolicyErrors;

            internal bool result;

            internal CallbackContext(Object request,
                                     X509Certificate certificate,
                                     X509Chain chain,
                                     SslPolicyErrors sslPolicyErrors)
            {
                this.request = request;
                this.certificate = certificate;
                this.chain = chain;
                this.sslPolicyErrors = sslPolicyErrors;
            }
        }
    }
}