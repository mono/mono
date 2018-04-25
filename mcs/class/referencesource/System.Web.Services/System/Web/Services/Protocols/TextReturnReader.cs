namespace System.Web.Services.Protocols {
    using System.IO;
    using System;
    using System.Web.Services;
    using System.Text;
    using System.Net;
    using System.Security.Permissions;

    /// <include file='doc\TextReturnReader.uex' path='docs/doc[@for="TextReturnReader"]/*' />
    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public class TextReturnReader : MimeReturnReader {
        PatternMatcher matcher;

        /// <include file='doc\TextReturnReader.uex' path='docs/doc[@for="TextReturnReader.Initialize"]/*' />
        public override void Initialize(object o) {
            matcher = (PatternMatcher)o;
        }

        /// <include file='doc\TextReturnReader.uex' path='docs/doc[@for="TextReturnReader.GetInitializer"]/*' />
        public override object GetInitializer(LogicalMethodInfo methodInfo) {
            return new PatternMatcher(methodInfo.ReturnType);
        }

        /// <include file='doc\TextReturnReader.uex' path='docs/doc[@for="TextReturnReader.Read"]/*' />
        public override object Read(WebResponse response, Stream responseStream) {
            try {
                string decodedString = RequestResponseUtils.ReadResponse(response);
                return matcher.Match(decodedString);
            }
            finally {
                response.Close();
            }
        }
    }
}
