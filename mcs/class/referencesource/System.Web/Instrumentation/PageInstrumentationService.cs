using System.Collections.Generic;

namespace System.Web.Instrumentation
{
    public sealed class PageInstrumentationService {
        /// <summary>
        /// Gets or sets a boolean indicating if instrumentation is active for the entire application
        /// </summary>
        /// <remarks>
        /// Page Instrumentation is a per-request service, and may not be active for a particular request, however this flag MUST be true
        /// for any request to use Page Instrumentation.
        /// </remarks>
        public static bool IsEnabled { get; set; }

        private IList<PageExecutionListener> _executionListeners = new List<PageExecutionListener>();

        /// <summary>
        /// Gets a list of listeners which are subscribed to the page execution process.
        /// </summary>
        /// <remarks>
        /// Guaranteed not to be null, but MAY be an empty list.
        /// </remarks>
        public IList<PageExecutionListener> ExecutionListeners { get { return _executionListeners; } }
    }
}
