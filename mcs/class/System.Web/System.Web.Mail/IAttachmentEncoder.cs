//
// System.Web.Mail.IAttachmentEncoder.cs
//
// Author(s):
//   Per Arneng <pt99par@student.bth.se>
//
//
using System;
using System.IO;

namespace System.Web.Mail {
    
    // An interface for attachment encoders ex Base64, UUEncode
    interface IAttachmentEncoder {
	void EncodeStream(  Stream ins , Stream outs );	
    }
    
}
