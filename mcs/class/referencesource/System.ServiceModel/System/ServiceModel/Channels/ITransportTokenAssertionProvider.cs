//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Xml;
    using System.ServiceModel.Description;

    public interface ITransportTokenAssertionProvider
    {
        XmlElement GetTransportTokenAssertion();
    }
}
