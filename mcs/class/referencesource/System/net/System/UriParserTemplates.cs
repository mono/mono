//------------------------------------------------------------------------------
// <copyright file="UriParserTemplates.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*++
Abstract:

    This file contains a set of predefined parseres that a user can derive from
    See also GenericUriParser.cs file for more user choices
    
    Note these parsers are for user to derive from hence they are nor "simple" nor "built-in"

Author:
    Alexei Vopilov    Jul 26 2004

Revision History:
--*/

    //
    // ATTN: The below types must be compile-time registered with UriParser.CheckSetIsSimpleFlag() method
    //       to avoid calling into the user code if there is no one.
    //
namespace System {
    //
    //  The HTTP Uri syntax description
    //  MustHaveAuthority | AllowAnInternetHost | MayHaveUserInfo | MayHavePort | MayHavePath | MayHaveQuery | MayHaveFragment |
    //  | PathIsRooted | ConvertPathSlashes | CompressPath | CanonicalizeAsFilePath | UnEscapeDotsAndSlashes
    //
    public class HttpStyleUriParser: UriParser
    {
        public HttpStyleUriParser():base(UriParser.HttpUri.Flags)
        {
        }
    }
    //
    //  The FTP Uri syntax description
    //  MustHaveAuthority | AllowAnInternetHost | MayHaveUserInfo | MayHavePort | MayHavePath | MayHaveFragment | PathIsRooted
    //  ConvertPathSlashes | CompressPath | CanonicalizeAsFilePath
    //
    public class FtpStyleUriParser: UriParser
    {
        public FtpStyleUriParser():base(UriParser.FtpUri.Flags)
        {
        }
    }
    //
    //  The FILE Uri syntax description
    //  MustHaveAuthority | AllowEmptyHost| AllowAnInternetHost | MayHavePath | MayHaveFragment | PathIsRooted
    //  | FileLikeUri | AllowDOSPath | ConvertPathSlashes | CompressPath | CanonicalizeAsFilePath | UnEscapeDotsAndSlashes
    //
    public class FileStyleUriParser: UriParser
    {
        public FileStyleUriParser():base(UriParser.FileUri.Flags)
        {
        }
    }
    //
    //  The NEWS Uri syntax description
    //  MayHavePath | MayHaveFragment
    //
    public class NewsStyleUriParser: UriParser
    {
        public NewsStyleUriParser():base(UriParser.NewsUri.Flags)
        {
        }
    }
    //
    //  The GOPHER Uri syntax description
    //  MustHaveAuthority | AllowAnInternetHost | MayHaveUserInfo | MayHavePort | MayHavePath | MayHaveFragment | PathIsRooted
    //
    public class GopherStyleUriParser: UriParser
    {
        public GopherStyleUriParser():base(UriParser.GopherUri.Flags)
        {
        }
    }
    //
    //  The LDAP Uri syntax description
    //  MustHaveAuthority | AllowEmptyHost | AllowAnInternetHost | MayHaveUserInfo | MayHavePort | MayHavePath | MayHaveQuery | MayHaveFragment | PathIsRooted
    //
    public class LdapStyleUriParser: UriParser
    {
        public LdapStyleUriParser():base(UriParser.LdapUri.Flags)
        {
        }
    }

    public class NetPipeStyleUriParser: UriParser
    {
        public NetPipeStyleUriParser():base(UriParser.NetPipeUri.Flags)
        {
        }
    }
    
    public class NetTcpStyleUriParser: UriParser
    {
        public NetTcpStyleUriParser():base(UriParser.NetTcpUri.Flags)
        {
        }
    }
}

