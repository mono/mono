#if WINDOWS && UNMANAGED

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Npgsql
{
    /// <summary>
    /// A class to handle everything associated with SSPI authentication
    /// </summary>
    internal class SSPIHandler : IDisposable
    {
        #region constants and structs

        private const int SECBUFFER_VERSION = 0;
        private const int SECBUFFER_TOKEN = 2;
        private const int SEC_E_OK = 0x00000000;
        private const int SEC_I_CONTINUE_NEEDED = 0x00090312;
        private const int ISC_REQ_ALLOCATE_MEMORY=0x00000100;
        private const int SECURITY_NETWORK_DREP=0x00000000;
        private const int SECPKG_CRED_OUTBOUND=0x00000002;

        [StructLayout(LayoutKind.Sequential)]
        private struct SecHandle
        {
            public int dwLower;
            public int dwUpper;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SecBuffer
        {
            public int cbBuffer;
            public int BufferType;
            public IntPtr pvBuffer;
        }

        /// <summary>
        /// Simplified SecBufferDesc struct with only one SecBuffer
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct SecBufferDesc
        {
            public int ulVersion;
            public int cBuffers;
            public IntPtr pBuffer;
        }

        #endregion

        #region p/invoke methods

        [DllImport("Secur32.dll")]
        private extern static int AcquireCredentialsHandle(
            string pszPrincipal,
            string pszPackage,
            int fCredentialUse,
            IntPtr pvLogonID,
            IntPtr pAuthData,
            IntPtr pGetKeyFn,
            IntPtr pvGetKeyArgument,
            ref SecHandle phCredential,
            out SecHandle ptsExpiry
        );

        [DllImport("secur32", CharSet=CharSet.Auto, SetLastError=true)]
        static extern int InitializeSecurityContext(
            ref SecHandle phCredential,
            ref SecHandle phContext,
            string pszTargetName,
            int fContextReq,
            int Reserved1,
            int TargetDataRep,
            ref SecBufferDesc pInput,
            int Reserved2,
            out SecHandle phNewContext,
            out SecBufferDesc pOutput,
            out int pfContextAttr,
            out SecHandle ptsExpiry);

        [DllImport("secur32", CharSet=CharSet.Auto, SetLastError=true)]
        static extern int InitializeSecurityContext(
            ref SecHandle phCredential,
            IntPtr phContext,
            string pszTargetName,
            int fContextReq,
            int Reserved1,
            int TargetDataRep,
            IntPtr pInput,
            int Reserved2,
            out SecHandle phNewContext,
            out SecBufferDesc pOutput,
            out int pfContextAttr,
            out SecHandle ptsExpiry);

        [DllImport("Secur32.dll")]
        private extern static int FreeContextBuffer(
            IntPtr pvContextBuffer
        );

        [DllImport("Secur32.dll")]
        private extern static int FreeCredentialsHandle(
            ref SecHandle phCredential
        );

        [DllImport("Secur32.dll")]
        private extern static int DeleteSecurityContext(
            ref SecHandle phContext
        );

        #endregion

        private bool disposed;
        private string sspitarget;
        private SecHandle sspicred;
        private SecHandle sspictx;
        private bool sspictx_set;

        public SSPIHandler(string pghost, string krbsrvname)
        {
            if (pghost == null)
                throw new ArgumentNullException("pghost");
            if (krbsrvname == null)
                krbsrvname = String.Empty;
            sspitarget = String.Format("{0}/{1}", krbsrvname, pghost);

            SecHandle expire;
            int status = AcquireCredentialsHandle(
                "",
                "negotiate",
                SECPKG_CRED_OUTBOUND,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero,
                ref sspicred,
                out expire
            );
            if (status != SEC_E_OK)
            {
                // This will automaticcaly fill in the message of the last Win32 error
                throw new Win32Exception();
            }
        }

        public string Continue(byte[] authData)
        {
            if (authData == null && sspictx_set)
                throw new InvalidOperationException("The authData parameter con only be null at the first call to continue!");


            int status;

            SecBuffer OutBuffer;
            SecBuffer InBuffer;
	        SecBufferDesc inbuf;
	        SecBufferDesc outbuf;
            SecHandle newContext;
            SecHandle expire;
            int contextAttr;

            OutBuffer.pvBuffer = IntPtr.Zero;
            OutBuffer.BufferType = SECBUFFER_TOKEN;
            OutBuffer.cbBuffer = 0;
            outbuf.cBuffers = 1;
            outbuf.ulVersion = SECBUFFER_VERSION;
            outbuf.pBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(OutBuffer));

            try
            {
                Marshal.StructureToPtr(OutBuffer, outbuf.pBuffer, false);
                if (sspictx_set)
                {
                    inbuf.pBuffer = IntPtr.Zero;
                    InBuffer.pvBuffer = Marshal.AllocHGlobal(authData.Length);
                    try
                    {
                    Marshal.Copy(authData, 0, InBuffer.pvBuffer, authData.Length);
                    InBuffer.cbBuffer = authData.Length;
                    InBuffer.BufferType = SECBUFFER_TOKEN;
                    inbuf.ulVersion = SECBUFFER_VERSION;
                    inbuf.cBuffers = 1;
                    inbuf.pBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(InBuffer));
                    Marshal.StructureToPtr(InBuffer, inbuf.pBuffer, false);
                        status = InitializeSecurityContext(
                            ref sspicred,
                            ref sspictx,
                            sspitarget,
                            ISC_REQ_ALLOCATE_MEMORY,
                            0,
                            SECURITY_NETWORK_DREP,
                            ref inbuf,
                            0,
                            out newContext,
                            out outbuf,
                            out contextAttr,
                            out expire
                        );
                    }
                    finally
                    {
                        if (InBuffer.pvBuffer != IntPtr.Zero)
                            Marshal.FreeHGlobal(InBuffer.pvBuffer);
                        if (inbuf.pBuffer != IntPtr.Zero)
                            Marshal.FreeHGlobal(inbuf.pBuffer);
                    }
                }
                else
                {
                    status = InitializeSecurityContext(
                        ref sspicred,
                        IntPtr.Zero,
                        sspitarget,
                        ISC_REQ_ALLOCATE_MEMORY,
                        0,
                        SECURITY_NETWORK_DREP,
                        IntPtr.Zero,
                        0,
                        out newContext,
                        out outbuf,
                        out contextAttr,
                        out expire
                    );
                }

                if (status != SEC_E_OK && status != SEC_I_CONTINUE_NEEDED)
                {
                    // This will automaticcaly fill in the message of the last Win32 error
                    throw new Win32Exception();
                }
                if (!sspictx_set)
                {
                    sspictx.dwUpper = newContext.dwUpper;
                    sspictx.dwLower = newContext.dwLower;
                    sspictx_set = true;
                }


                if (outbuf.cBuffers > 0)
                {
                    if (outbuf.cBuffers != 1)
                    {
                        throw new InvalidOperationException("SSPI returned invalid number of output buffers");
                    }
                    // attention: OutBuffer is still our initially created struct but outbuf.pBuffer doesn't point to
                    // it but to the copy of it we created on the unmanaged heap and passed to InitializeSecurityContext() 
                    // we have to marshal it back to see the content change
                    OutBuffer = (SecBuffer)Marshal.PtrToStructure(outbuf.pBuffer, typeof(SecBuffer));
                    if (OutBuffer.cbBuffer > 0)
                    {
                        // we need the buffer with a terminating 0 so we
                        // make it one byte bigger
                        byte[] buffer = new byte[OutBuffer.cbBuffer];
                        Marshal.Copy(OutBuffer.pvBuffer, buffer, 0, buffer.Length);
                        // The SSPI authentication data must be sent as password message

                        return System.Text.Encoding.ASCII.GetString(buffer);
                        //stream.WriteByte((byte)'p');
                        //PGUtil.WriteInt32(stream, buffer.Length + 5);
                        //stream.Write(buffer, 0, buffer.Length);
                        //stream.Flush();
                    }
                }
                return String.Empty;
            }
            finally
            {
                if (OutBuffer.pvBuffer != IntPtr.Zero)
                    FreeContextBuffer(OutBuffer.pvBuffer);
                if (outbuf.pBuffer != IntPtr.Zero)
                    Marshal.FreeHGlobal(outbuf.pBuffer);
            }
        }


        #region resource cleanup

        private void FreeHandles()
        {
            if (sspictx_set)
            {
                FreeCredentialsHandle(ref sspicred);
                DeleteSecurityContext(ref sspictx);
            }
        }

        ~SSPIHandler()
        {
            FreeHandles();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    FreeHandles();
                }
                disposed = true;
            }
        }

        #endregion
    }
}

#endif