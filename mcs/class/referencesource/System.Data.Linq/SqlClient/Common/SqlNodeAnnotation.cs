using System;
using System.Collections.Generic;
using System.Text;

namespace System.Data.Linq.SqlClient {

    /// <summary>
    /// Annotation about a particular SqlNode.
    /// </summary>
    internal abstract class SqlNodeAnnotation {
        string message;
        internal SqlNodeAnnotation(string message) {
            this.message = message;
        }
        internal string Message {
            get {return this.message;}
        }
    }
}
