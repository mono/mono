//
// System.Web.HttpWorkerRequest.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

using System;

namespace System.Web
{
        public abstract class HttpWorkerRequest
        {
                public const int HeaderAccept = 20;
                public const int HeaderAcceptCharset = 21;
                public const int HeaderAcceptEncoding = 22;
                public const int HeaderAcceptLanguage = 23;
                public const int HeaderAcceptRanges = 20;
                public const int HeaderAge = 21;
                public const int HeaderAllow = 10;
                public const int HeaderAuthorization = 24;
                public const int HeaderCacheControl = 0;
                public const int HeaderConnection = 1;
                public const int HeaderContentEncoding = 13;
                public const int HeaderContentLanguage = 14;
                public const int HeaderContentLength = 11;
                public const int HeaderContentLocation = 15;
                public const int HeaderContentMd5 = 16;
                public const int HeaderContentRange = 17;
                public const int HeaderContentType = 12;
                public const int HeaderCookie = 25;
                public const int HeaderDate = 2;
                public const int HeaderEtag = 22;
                public const int HeaderExpect = 26;
                public const int HeaderExpires = 18;
                public const int HeaderFrom = 27;
                public const int HeaderHost = 28;
                public const int HeaderIfMatch = 29;
                public const int HeaderIfModifiedSince = 30;
                public const int HeaderIfNoneMatch = 31;
                public const int HeaderIfRange = 32;
                public const int HeaderIfUnmodifiedSince = 33;
                public const int HeaderKeepAlive = 3;
                public const int HeaderLastModified = 19;
                public const int HeaderLocation = 23;
                public const int HeaderMaxForwards = 34;
                public const int HeaderPragma = 4;
                public const int HeaderProxyAuthenticate = 24;
                public const int HeaderProxyAuthorization = 35;
                public const int HeaderRange = 37;
                public const int HeaderReferer = 36;
                public const int HeaderRetryAfter = 25;
                public const int HeaderServer = 26;
                public const int HeaderSetCookie = 27;
                public const int HeaderTe = 38;
                public const int HeaderTrailer = 5;
                public const int HeaderTransferEncoding = 6;
                public const int HeaderUpgrade = 7;
                public const int HeaderUserAgent = 39;
                public const int HeaderVary = 28;
                public const int HeaderVia = 8;
                public const int HeaderWarning = 9;
                public const int HeaderWwwAuthenticate = 29;
                public const int ReasonCachePolicy = 2;
                public const int ReasonCacheSecurity = 3;
                public const int ReasonClientDisconnect = 4;
                public const int ReasonDefault = 0;
                public const int ReasonFileHandleCacheMiss = 1;
                public const int ReasonResponseCacheMiss = 0;
                public const int RequestHeaderMaximum = 40;
                public const int ResponseHeaderMaximum = 30;

public static int GetKnownRequestHeaderIndex(string header);
public static string GetKnownRequestHeaderName(int index);
public static int GetKnownResponseHeaderIndex(string header);
public static string GetKnownResponseHeaderName(int index);
public static string GetStatusDescription(int code);
public virtual string MachineConfigPath {get;}
public virtual string MachineInstallDirectory {get;}

                protected HttpWorkerRequest() {}

public virtual void CloseConnection();
public abstract void EndOfRequest();
public abstract void FlushResponse(bool finalFlush);
public virtual string GetAppPath();
public virtual string GetAppPathTranslated();
public virtual string GetAppPoolID();
public virtual long GetBytesRead();
public virtual byte[] GetClientCertificate();
public virtual byte[] GetClientCertificateBinaryIssuer();
public virtual int GetClientCertificateEncoding();
public virtual byte[] GetClientCertificatePublicKey();
public virtual DateTime GetClientCertificateValidFrom();
public virtual DateTime GetClientCertificateValidUntil();
public virtual long GetConnectionID();
public virtual string GetFilePath();
public virtual string GetFilePathTranslated();
public abstract string GetHttpVerbName();
public abstract string GetHttpVersion();
public virtual string GetKnownRequestHeader(int index);
public abstract string GetLocalAddress();
public abstract int GetLocalPort();
public virtual string GetPathInfo();
public virtual byte[] GetPreloadedEntityBody();
public virtual string GetProtocol();
public abstract string GetQueryString();
public virtual byte[] GetQueryStringRawBytes();
public abstract string GetRawUrl();
public abstract string GetRemoteAddress();
public virtual string GetRemoteName();
public abstract int GetRemotePort();
public virtual int GetRequestReason();
public virtual string GetServerName();
public virtual string GetServerVariable(string name);
public virtual string GetUnknownRequestHeader(string name);
public virtual string[][] GetUnknownRequestHeaders();
public abstract string GetUriPath();
public virtual long GetUrlContextID();
public virtual IntPtr GetUserToken();
public virtual IntPtr GetVirtualPathToken();
public bool HasEntityBody();
public virtual bool HeadersSent();
public virtual bool IsClientConnected();
public virtual bool IsEntireEntityBodyIsPreloaded();
public virtual bool IsSecure();
public virtual string MapPath(string virtualPath);
public virtual int ReadEntityBody(byte[] buffer, int size);
public virtual void SendCalculatedContentLength(int contentLength);
public abstract void SendKnownResponseHeader(int index, string value);
public abstract void SendResponseFromFile(IntPtr handle, long offset, long length);
public abstract void SendResponseFromFile(string filename, long offset, long length);
public abstract void SendResponseFromMemory(byte[] data, int length);
public virtual void SendResponseFromMemory(IntPtr data, int length);
public abstract void SendStatus(int statusCode, string statusDescription);
public abstract void SendUnknownResponseHeader(string name, string value);
public virtual void SetEndOfSendNotification(HttpWorkerRequest.EndOfSendNotification callback, object extraData);
        }
}
