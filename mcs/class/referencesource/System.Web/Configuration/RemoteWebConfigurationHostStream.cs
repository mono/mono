//------------------------------------------------------------------------------
// <copyright file="RemoteWebConfigurationHostStream.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System.Collections;
    using System.Configuration;
    using System.Web;
    using System.Web.Util;
    using System.Security;
    using System.Security.Principal;
    using System.IO;
    using System.Web.Hosting;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
    internal class RemoteWebConfigurationHostStream : Stream
    {
        private string                      _FileName;
        private string                      _TemplateFileName;
        private string                      _Server;
        private MemoryStream                _MemoryStream;
        private bool                        _IsDirty = false;
        private long                        _ReadTime = 0;
        private WindowsIdentity             _Identity;
        private string                      _Username;
        private string                      _Domain;
        private string                      _Password;
        private bool                        _streamForWrite;

        internal RemoteWebConfigurationHostStream(bool streamForWrite, string serverName, string streamName, string templateStreamName, string username, string domain, string password, WindowsIdentity identity) {
            _Server = serverName;
            _FileName = streamName;
            _TemplateFileName = templateStreamName;
            _Username = username;
            _Domain = domain;
            _Password = password;
            _Identity = identity;
            _streamForWrite = streamForWrite;
        }

        private void Init()
        {
#if !FEATURE_PAL // FEATURE_PAL does not enable COM
            if (_MemoryStream != null)
                return;

            byte[]                            buf     = null;
            WindowsImpersonationContext       wiContext = null;

            try
            {
                ////////////////////////////////////////////////////////////
                // Step 1: Set the impersonation if required
                if (_Identity != null)
                {
                    wiContext = _Identity.Impersonate();
                }

                try
                {
                    IRemoteWebConfigurationHostServer remoteSrv = RemoteWebConfigurationHost.CreateRemoteObject(_Server, _Username, _Domain, _Password);
                    try
                    {
                        // If we open the stream for writing, we only need to get the _ReadTime because
                        // we will create an empty memory stream for write.
                        buf = remoteSrv.GetData(_FileName, _streamForWrite, out _ReadTime);
                    }
                    finally
                    {
                        while (Marshal.ReleaseComObject(remoteSrv) > 0)
                        {
                        }
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    if (wiContext != null)
                    {
                        wiContext.Undo(); // revert impersonation
                    }
                }
            }
            catch
            {
                throw;
            }

            if (buf == null || buf.Length < 1)
            {
                _MemoryStream = new MemoryStream();
            }
            else
            {
                _MemoryStream = new MemoryStream(buf.Length);
                _MemoryStream.Write(buf, 0, buf.Length);
                _MemoryStream.Position = 0;
            }
#else // !FEATURE_PAL
            throw new NotSupportedException();
#endif // !FEATURE_PAL
        }

        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return true; } }
        public override bool CanWrite { get { return true; } }
        public override long Length
        {
            get
            {
                Init();
                return _MemoryStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                Init();
                return _MemoryStream.Position;
            }
            set
            {
                Init();
                _MemoryStream.Position = value;
            }
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            Init();
            return _MemoryStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            _IsDirty = true;
            Init();
            if (offset + count > _MemoryStream.Length)
                _MemoryStream.SetLength(offset + count);
            return _MemoryStream.BeginWrite(buffer, offset, count, callback, state);
        }

        protected override void Dispose(bool disposing)
        {
            try {
                if (disposing && _MemoryStream != null) {
                    Flush();
                    _MemoryStream.Close();
                }
            }
            finally {
                base.Dispose(disposing);
            }
        }

        public override System.Runtime.Remoting.ObjRef CreateObjRef(Type requestedType)
        {
            throw new System.Runtime.Remoting.RemotingException();
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            Init();
            return _MemoryStream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            Init();
            _MemoryStream.EndWrite(asyncResult);
        }

        public override void Flush() 
        {
            // It's a memory stream.  Don't need to flush anything.
        }

        internal void FlushForWriteCompleted()
        {
#if !FEATURE_PAL // FEATURE_PAL does not enable COM
            if (_IsDirty && _MemoryStream != null)
            {
                WindowsImpersonationContext wiContext = null;

                try
                {
                    ////////////////////////////////////////////////////////////
                    // Step 1: Set the impersonation if required
                    if (_Identity != null)
                    {
                        wiContext = _Identity.Impersonate();
                    }

                    try
                    {
                        IRemoteWebConfigurationHostServer remoteSrv = RemoteWebConfigurationHost.CreateRemoteObject(_Server, _Username, _Domain, _Password);
                        try
                        {
                            remoteSrv.WriteData(_FileName, _TemplateFileName, _MemoryStream.ToArray(), ref _ReadTime);
                        }
                        catch
                        {
                            throw;
                        }
                        finally
                        {
                            while (Marshal.ReleaseComObject(remoteSrv) > 0)
                            {
                            }
                        }
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        if (wiContext != null)
                        {
                            wiContext.Undo();
                        }
                    }
                }
                catch
                {
                    throw;
                }

                _MemoryStream.Flush();
                _IsDirty = false;
            }
#else // !FEATURE_PAL
            throw new NotSupportedException();
#endif // !FEATURE_PAL
        }
        public override object InitializeLifetimeService()
        {
            Init();
            return _MemoryStream.InitializeLifetimeService();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            Init();
            return _MemoryStream.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            Init();
            return _MemoryStream.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            Init();
            return _MemoryStream.Seek(offset, origin);
        }

        public override void SetLength(long val)
        {
            _IsDirty = true;
            Init();
            _MemoryStream.SetLength(val);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _IsDirty = true;
            Init();
            if (offset + count > _MemoryStream.Length)
                _MemoryStream.SetLength(offset + count);
            _MemoryStream.Write(buffer, offset, count);

        }

        public override void WriteByte(byte val)
        {
            _IsDirty = true;
            Init();
            _MemoryStream.WriteByte(val);
        }
    }
}
