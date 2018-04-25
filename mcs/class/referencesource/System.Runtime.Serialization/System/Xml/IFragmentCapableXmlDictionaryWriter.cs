//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Xml
{
    using System.IO;

    public interface IFragmentCapableXmlDictionaryWriter
    {
        bool CanFragment { get; }

        void StartFragment(Stream stream, bool generateSelfContainedTextFragment);

        void EndFragment();

        void WriteFragment(byte[] buffer, int offset, int count);
    }
}
