//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation.Services
{
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Xaml;

    /// <summary>
    /// The ModelSearchService class is responsible for generate a TextImage and navigate to
    /// the corresponding ModelItem in the workflow designer.
    /// </summary>
    public abstract class ModelSearchService
    {
        /// <summary>
        /// Constructs a new ModelSearchService.
        /// </summary>
        protected ModelSearchService()
        {
        }

        /// <summary>
        /// Generate a searchable text image based on the model item tree.
        /// </summary>
        /// <returns></returns>
        public abstract TextImage GenerateTextImage();

        /// <summary>
        /// Navigate to a modelItem based on the line number in the text image.
        /// </summary>
        /// <param name="location">the line number in text image.</param>
        /// <returns>Is the Navivating succeed.</returns>
        public abstract bool NavigateTo(int location);

        /// <summary>
        /// Navigate to a modelItem based on the source location in the xaml file.
        /// </summary>
        /// <param name="srcLocation">the source location in the xaml file.</param>
        /// <returns>Is the Navivating succeed.</returns>
        public abstract bool NavigateTo(int startLine, int startColumn, int endLine, int endColumn);
    }
}
