//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
// Enable this to dump the contents of all connections to the disk
//#define CONNECTIONDUMP
#if CONNECTIONDUMP

namespace System.ServiceModel.Channels
{
    using System.Text;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security;
    using System.IO;

    class ConnectionDumpInitiator : IConnectionInitiator
    {
        IConnectionInitiator connectionInitiator;
        string outputDirectory;

        public ConnectionDumpInitiator(IConnectionInitiator connectionInitiator)
        {
            this.connectionInitiator = connectionInitiator;
            this.outputDirectory = Environment.GetEnvironmentVariable("ConnectionDump");
        }

        public IConnection Connect(Uri uri, TimeSpan timeout)
        {
            return CreateDumpingConnection(connectionInitiator.Connect(uri, timeout));
        }

        public IAsyncResult BeginConnect(Uri uri, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return connectionInitiator.BeginConnect(uri, timeout, callback, state);
        }

        public IConnection EndConnect(IAsyncResult result)
        {
            IConnection connection = connectionInitiator.EndConnect(result);
            return CreateDumpingConnection(connection);
        }

        IConnection CreateDumpingConnection(IConnection connection)
        {
            if (this.outputDirectory != null)
            {
                return new DumpingConnection(connection, this.outputDirectory);
            }
            else
            {
                return connection;
            }
        }

        class DumpingConnection : DelegatingConnection
        {
            Stream outputStream;
            Stream inputStream;

            public DumpingConnection(IConnection innerConnection, string outputDirectory)
                : base(innerConnection)
            {
                string basePath = Path.Combine(outputDirectory, ModuleName.GetThisModuleName());
                for (int index = 0; ; index++)
                {
                    string inFilePath = basePath + "." + index.ToString() + ".in.mf";
                    if (File.Exists(inFilePath))
                    {
                        continue;
                    }
                    string outFilePath = basePath + "." + index.ToString() + ".out.mf";
                    if (File.Exists(outFilePath))
                    {
                        continue;
                    }

                    outputStream = File.Create(outFilePath);
                    inputStream = File.Create(inFilePath);
                    break;
                }
            }

            public override void Abort()
            {
                OnDone();
                base.Abort();
            }

            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout, AsyncCallback callback, object state)
            {
                IAsyncResult result = base.BeginWrite(buffer, offset, size, immediate, timeout, callback, state);
                OnWrite(buffer, offset, size);
                return result;
            }

            public override void Close(TimeSpan timeout)
            {
                OnDone();
                base.Close(timeout);
            }

            public override int EndRead()
            {
                int bytesRead = base.EndRead();
                OnRead(this.AsyncReadBuffer, 0, bytesRead);
                return bytesRead;
            }

            public void OnRead(byte[] buffer, int offset, int size)
            {
                inputStream.Write(buffer, offset, size);
            }

            public void OnWrite(byte[] buffer, int offset, int size)
            {
                outputStream.Write(buffer, offset, size);
            }

            public void OnDone()
            {
                inputStream.Close();
                outputStream.Close();
            }

            public override int Read(byte[] buffer, int offset, int size, TimeSpan timeout)
            {
                int bytesRead = base.Read(buffer, offset, size, timeout);
                OnRead(buffer, offset, bytesRead);
                return bytesRead;
            }

            public override void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout)
            {
                base.Write(buffer, offset, size, immediate, timeout);
                OnWrite(buffer, offset, size);
            }

            public override void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout, BufferManager bufferManager)
            {
                base.Write(buffer, offset, size, immediate, timeout, bufferManager);
                OnWrite(buffer, offset, size);
            }

            static class ModuleName
            {
                [SuppressUnmanagedCodeSecurity]
                [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
                [ResourceExposure(ResourceScope.None)]
                static extern int GetModuleFileName(IntPtr module, StringBuilder fileName, int count);

                public static string GetThisModulePath()
                {
                    StringBuilder modulePath = new StringBuilder(256);
                    GetModuleFileName(IntPtr.Zero, modulePath, 256);
                    return modulePath.ToString();
                }

                public static string GetThisModuleName()
                {
                    return Path.GetFileNameWithoutExtension(GetThisModulePath());
                }
            }
        }
    }
}

#endif
