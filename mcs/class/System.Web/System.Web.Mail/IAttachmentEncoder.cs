// Per Arneng <pt99par@student.bth.se>
using System;
using System.IO;

namespace System.Web.Mail {

    interface IAttachmentEncoder {
	void EncodeStream(  Stream ins , Stream outs );	
    }
    
}
