namespace System.Activities.Presentation {
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Precedence specifies the precedence of order tokens.
    /// </summary>
    enum OrderTokenPrecedence {

        /// <summary>
        /// Indicates that this token comes before
        /// </summary>
        Before,

        /// <summary>
        /// Indicates that this token comes after
        /// </summary>
        After
    }
}
