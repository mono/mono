namespace System.Activities.Presentation.PropertyEditing {
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Activities.Presentation;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// This class is used to set the order in which properties show up within a category, 
    /// or within a list of sub-properties.  3rd parties may choose to derive from this class
    /// and create their own custom order tokens, which can both guarantee property order as
    /// well as property grouping.
    /// </summary>
    [Fx.Tag.XamlVisible(false)]
    sealed class PropertyOrder : OrderToken
    {
        private static PropertyOrder _default;

        /// <summary>
        /// Creates a PropertyOrder.  
        /// </summary>
        /// <param name="precedence">Precedence of this token based on the
        /// referenced token.</param>
        /// <param name="reference">Referenced token.</param>
        /// <param name="conflictResolution">Conflict resolution semantics.
        /// Winning ConflictResultion semantic should only be used
        /// on predefined, default OrderToken instances to ensure
        /// their correct placement in more complex chain of order
        /// dependencies.</param>
        private PropertyOrder(OrderTokenPrecedence precedence, OrderToken reference, OrderTokenConflictResolution conflictResolution)
            : base(precedence, reference, conflictResolution) {
        }

        /// <summary>
        /// Creates a PropertyOrder that comes after the passed in token.
        /// </summary>
        /// <param name="reference">The reference token</param>
        /// <returns>The new PropertyOrder</returns>
        /// <exception cref="ArgumentNullException">When reference is null</exception>
        public static PropertyOrder CreateAfter(OrderToken reference)
        {
            if (reference == null)
                throw FxTrace.Exception.ArgumentNull("reference");

            return new PropertyOrder(OrderTokenPrecedence.After, reference, OrderTokenConflictResolution.Lose);
        }

        /// <summary>
        /// Treat equal orders as equal
        /// </summary>
        /// <param name="left">Left token</param>
        /// <param name="right">Right token</param>
        /// <returns>0</returns>
        protected override int ResolveConflict(OrderToken left, OrderToken right) {
            return 0;
        }

        /// <summary>
        /// Gets the system defined Default order position.
        /// </summary>
        public static PropertyOrder Default {
            get {
                if (_default == null) {
                    _default = new PropertyOrder(OrderTokenPrecedence.After, null, OrderTokenConflictResolution.Win);
                }
                return _default;
            }
        }
    }
}
