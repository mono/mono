//
// System.Web.Mail.SmtpException.cs
//
// Author(s):
//   Per Arneng <pt99par@student.bth.se>
//
//
using System.IO;

namespace System.Web.Mail {

    // an exception thrown when an smtp exception occurs
    internal class SmtpException : IOException {
	public SmtpException( string message ) : base( message ) {}
    }

}
