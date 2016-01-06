namespace NUnit.Framework.Internal.Filters
{
    /// <summary>
    /// SimpleCategoryFilter parses a basic string representing a
    /// single category or a list of categories separated by commas
    /// </summary>
    public class SimpleCategoryExpression
    {
        private string text;

        private TestFilter filter;

        /// <summary>
        /// Construct category filter from a text string
        /// </summary>
        /// <param name="text">A list of categories to parse</param>
        public SimpleCategoryExpression(string text)
        {
            this.text = text;
        }

        /// <summary>
        /// Gets the TestFilter represented by the expression
        /// </summary>
        public TestFilter Filter
        {
            get
            {
                if (filter == null)
                {
                    filter = GetCategories(); 
                }
                return filter;
            }
        }

        private TestFilter GetCategories()
        {
            string[] categories = text.Split(',');
            return new CategoryFilter(categories);
        }
    }
}
