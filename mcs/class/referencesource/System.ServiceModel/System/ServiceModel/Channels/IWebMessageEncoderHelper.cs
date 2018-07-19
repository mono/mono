// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    // In .net 4.0, the WebMessageEncoder uses a raw encoder, different than ByteStreamMessageEncoder. This raw encoder produces Messages for which the 
    // Message.GetReaderAtBodyContents() method returns an XmlDictionaryReader positioned on content (the root element of the xml); that's because MoveToContent() 
    // is called on the reader before it's returned.
    //
    // Starting with .net 4.5, the WebMessageEncoder uses the ByteStreamMessageEncoder. By default, this encoder produces Messages for which the body reader is initially 
    // positioned on None (just before the root element). It does so for compatibility with .net 4.0. So we need the WebMessageEncoder to create a non-default 
    // ByteStreamMessageEncoder that triggers the MoveToContent() call. We don't want to expose a direct public way to create this non-default ByteStreamMessageEncoder
    // and WebMessageEncoder (from System.ServiceModel.Web.dll) doesn't have access to the internals of ByteStreamMessageEncoder (from System.ServiceModel.Channels.dll).
    // So we use this intermediate interface.
    //
    // This is what we want to have:
    // +---------------------+------------------------------------------------------------+--------------------------------------------------------------------------+
    // |                     | WebMessageEncodingBindingElement/WebMessageEncoder is used | ByteStreamMessageEncodingBindingElement/ByteStreamMessageEncoder is used |
    // +=====================+============================================================+==========================================================================+
    // | .net 4.0            | the Message body reader is initially positioned on content | the Message body reader is initially positioned on None                  |
    // +---------------------+------------------------------------------------------------+--------------------------------------------------------------------------+
    // | .net 4.5 (or after) | the Message body reader is initially positioned on content | the Message body reader is initially positioned on None                  |
    // +---------------------+------------------------------------------------------------+--------------------------------------------------------------------------+
    //
    // See 252277 @ CSDMain for other info.
    internal interface IWebMessageEncoderHelper
    {
        void EnableBodyReaderMoveToContent();
    }
}
