using System.IO;

namespace System.Web.Mail {

    internal class SmtpException : IOException {
	public SmtpException( string message ) : base( message ) {}
    }

}
