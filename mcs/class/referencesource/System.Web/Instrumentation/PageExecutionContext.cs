using System.IO;

namespace System.Web.Instrumentation {
    /// <summary>
    /// Contains information about the current position with page execution
    /// </summary>
    public class PageExecutionContext {
        /// <summary>
        /// A flag indicating if the block contains literal content which should be deeply scanned for selection mapping purposes
        /// </summary>
        public bool IsLiteral { get; set; }

        /// <summary>
        /// The length of the block in characters
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// The start position of the block, zero-based, from the start of the outputted document
        /// </summary>
        public int StartPosition { get; set; }

        /// <summary>
        /// The TextWriter being used to output the document. Allows consumers to write tokens to the output stream for selection mapping
        /// </summary>
        public TextWriter TextWriter { get; set; }

        /// <summary>
        /// The virtual path to the source file. Used to allow consumers to find the source file to be used for selection mapping.
        /// </summary>
        public string VirtualPath { get; set; }
    }
}
