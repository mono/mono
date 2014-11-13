namespace System.Web.Instrumentation
{
    public abstract class PageExecutionListener {
        /// <summary>
        /// Called by a view engine BEFORE it renders the output for the specified context.
        /// </summary>
        public abstract void BeginContext(PageExecutionContext context);

        /// <summary>
        /// Called by a view engine AFTER it renders the output for the specified context.
        /// </summary>
        public abstract void EndContext(PageExecutionContext context);
    }
}
