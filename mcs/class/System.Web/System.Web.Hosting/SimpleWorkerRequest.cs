//
// System.Web.Hosting.SimpleWorkerRequest.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

using System;

namespace System.Web.Hosting
{
        public class SimpleWorkerRequest : HttpWorkerRequest
        {
                public SimpleWorkerRequest(string page, string query, TextWriter output);
                public SimpleWorkerRequest(string appVirtualDir, string appPhysicalDir, string page, string query, TextWriter output);
                public override string MachineConfigPath {get;}
                public override void EndOfRequest();
                public override void FlushResponse(bool finalFlush);
                public override string GetAppPath();
                public override string GetAppPathTranslated();
                public override string GetFilePath();
                public override string GetFilePathTranslated();
                public override string GetHttpVerbName();
                public override string GetHttpVersion();
                public override string GetLocalAddress();
                public override int GetLocalPort();
                public override string GetPathInfo();
                public override string GetQueryString();
                public override string GetRawUrl();
                public override string GetRemoteAddress();
                public override int GetRemotePort();
                public override string GetServerVariable(string name);
                public override string GetUriPath();
                public override IntPtr GetUserToken();
                public override string MapPath(string path);
                public override void SendKnownResponseHeader(int index, string value);
                public override void SendResponseFromFile(IntPtr handle, long offset, long length);
                public override void SendResponseFromFile(string filename, long offset, long length);
                public override void SendResponseFromMemory(byte[] data, int length);
                public virtual void SendResponseFromMemory(IntPtr data, int length);
                public override void SendStatus(int statusCode, string statusDescription);
                public override void SendUnknownResponseHeader(string name, string value);
        }
}
