//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel
{
    using System.ServiceModel.Channels;

    public enum WSMessageEncoding
    {
        Text = 0,
        Mtom,
    }

    static class WSMessageEncodingHelper
    {
        internal static bool IsDefined(WSMessageEncoding value)
        {
            return
                value == WSMessageEncoding.Text
                || value == WSMessageEncoding.Mtom;
        }
        internal static void SyncUpEncodingBindingElementProperties(TextMessageEncodingBindingElement textEncoding, MtomMessageEncodingBindingElement mtomEncoding)
        {
            // textEncoding provides the backing store for ReaderQuotas and WriteEncoding,
            // we must ensure same values propogate to mtomEncoding
            textEncoding.ReaderQuotas.CopyTo(mtomEncoding.ReaderQuotas);
            mtomEncoding.WriteEncoding = textEncoding.WriteEncoding;
        }
    }
}
