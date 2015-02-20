namespace System.Activities.Presentation
{

    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Activities.Presentation;

    /// <summary>
    /// OrderToken is a generic class used to identify the sort
    /// order of hierarchical items.  OrderTokens can be used
    /// to define priority that is based on some predefined defaults or
    /// based on other OrderTokens.
    /// </summary>
    abstract class OrderToken : IComparable<OrderToken>
    {

        private readonly OrderToken _reference;
        private readonly OrderTokenPrecedence _precedence;
        private readonly OrderTokenConflictResolution _conflictResolution;

        private readonly int _depth;
        private readonly int _index;
        private int _nextChildIndex;

        /// <summary>
        /// Creates a new OrderToken instance based on the specified
        /// referenced OrderToken, precedence, and conflict resolution
        /// semantics.
        /// </summary>
        /// <param name="precedence">Precedence of this token based on the
        /// referenced token.</param>
        /// <param name="reference">Referenced token.  May be null for the
        /// root token case (token that's not dependent on anything else).</param>
        /// <param name="conflictResolution">Conflict resolution semantics.
        /// Winning ConflictResultion semantic should only be used
        /// on predefined, default OrderToken instances to ensure
        /// their correct placement in more complex chain of order
        /// dependencies.</param>
        protected OrderToken(
            OrderTokenPrecedence precedence,
            OrderToken reference,
            OrderTokenConflictResolution conflictResolution)
        {

            if (!EnumValidator.IsValid(precedence)) throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException("precedence"));
            if (!EnumValidator.IsValid(conflictResolution)) throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException("conflictResolution"));

            _reference = reference;
            _precedence = precedence;
            _conflictResolution = conflictResolution;
            _depth = reference == null ? 0 : reference._depth + 1;
            _index = reference == null ? -1 : reference._nextChildIndex++;
        }

        /// <summary>
        /// Compares this order token with the specified order token.
        /// The comparsion for OrderTokens that don't belong to the same
        /// chain of OrderTokens will be resolved non-deterministically.
        /// </summary>
        /// <param name="other">The token to compare this token to.</param>
        /// <returns>0 when the tokens have an equal order priority,
        /// -1 if this order comes before the specified order,
        /// 1 otherwise.</returns>
        /// <exception cref="ArgumentNullException">When other is null</exception>
        public virtual int CompareTo(OrderToken other)
        {

            if (other == null)
                throw FxTrace.Exception.ArgumentNull("other");

            if (other == this)
                return 0;

            OrderToken thisOrder = this;

            // Find a common parent
            while (thisOrder._reference != other._reference)
            {

                if (thisOrder._depth == other._depth)
                {
                    thisOrder = thisOrder._reference;
                    other = other._reference;
                }
                else
                {
                    if (thisOrder._depth > other._depth)
                    {
                        if (thisOrder._reference == other) return thisOrder._precedence == OrderTokenPrecedence.After ? 1 : -1;
                        thisOrder = thisOrder._reference;
                    }
                    else
                    {
                        if (other._reference == thisOrder) return other._precedence == OrderTokenPrecedence.After ? -1 : 1;
                        other = other._reference;
                    }
                }
            }

            // One order "before", one order "after"?  Easy, return the
            // "before" order.
            if (thisOrder._precedence != other._precedence)
                return thisOrder._precedence == OrderTokenPrecedence.Before ? -1 : 1;

            // Both orders "before" the parent?  Roots win, otherwise call ResolveConflict().
            if (thisOrder._precedence == OrderTokenPrecedence.Before)
            {
                if (thisOrder._conflictResolution == OrderTokenConflictResolution.Win)
                    return -1;
                else if (other._conflictResolution == OrderTokenConflictResolution.Win)
                    return 1;
                return ResolveConflict(thisOrder, other);
            }

            // Both orders "after" the parent?  Roots win, otherwise call ResolveConflict().
            if (thisOrder._conflictResolution == OrderTokenConflictResolution.Win)
                return 1;
            else if (other._conflictResolution == OrderTokenConflictResolution.Win)
                return -1;
            return ResolveConflict(thisOrder, other);
        }

        /// <summary>
        /// This method is called by CompareTo()'s default implementation when two OrderTokens
        /// appear to be equivalent.  The base functionality of this method uses the instantiation
        /// order of the two tokens as a tie-breaker.  Override this method to
        /// implement custom algorithms.  Note that if this method ever returns 0
        /// (indicating that the two tokens are equivalent) and if these tokens
        /// belong to a list that gets sorted multiple times, the relative order in
        /// which they appear in the list will not be guaranteed.  This side-effect
        /// may or may not be a problem depending on the application.
        /// </summary>
        /// <param name="left">Left OrderToken</param>
        /// <param name="right">Right OrderToken</param>
        /// <returns>0, if the two are equal, -1, if left comes before right, 
        /// 1 otherwise.</returns>
        protected virtual int ResolveConflict(OrderToken left, OrderToken right)
        {
            return left._index.CompareTo(right._index);
        }
    }
}
