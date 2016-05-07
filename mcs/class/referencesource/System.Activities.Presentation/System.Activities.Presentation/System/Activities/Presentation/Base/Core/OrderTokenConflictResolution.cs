namespace System.Activities.Presentation {
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Enum used to specify the winner of ordering ties.
    /// Winning ConflictResultion marker should only be used
    /// on predefined, default OrderToken instances to ensure
    /// their correct placement in more complex chain of order
    /// dependencies.
    /// </summary>
    enum OrderTokenConflictResolution {

        /// <summary>
        /// Indicates that this token should win during conflicts.
        /// </summary>
        Win,

        /// <summary>
        /// Indicates that this token should lose during conflicts.  
        /// If two tokens are compared that are equivalent and both of 
        /// which have their ConflictResolution set to Win, they will
        /// be considered equal.
        /// </summary>
        Lose
    }
}
