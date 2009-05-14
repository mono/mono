using System;
using System.Linq.Expressions;

#if MONO_STRICT
namespace System.Data.Linq.Sugar.Expressions
#else
namespace DbLinq.Data.Linq.Sugar.Expressions
#endif
{
    /// <summary>
    ///  Permits translation of expressions for specific database vendors
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   Not all databases are equal, and some databases have SQL constraints 
    ///   that are not shared by others.  Some of these SQL dialect constraints
    ///   can be handled by <see cref="DbLinq.Vendor.ISqlProvider" />, but
    ///   in some circumstances that's too late in the game.
    ///  </para>
    ///  <para>
    ///   Case in point: for 
    ///   <c>(from p in people orderby p.LastName select p).Count()</c>
    ///   is translated into e.g. 
    ///   <c>SELECT COUNT(*) FROM People ORDER BY LastName</c>.  However, this 
    ///   is invalid for Microsoft SQL Server (the <c>ORDER BY</c> cannot be
    ///   present), and by the time <c>ISqlProvider</c> is being used, there's
    ///   no easy way to remove the OrderBy sequence.
    ///  </para>
    ///  <para>
    ///   The <c>ExpressionTranslator</c> type allows vendor code to manipulate
    ///   the expression tree <i>before</i> SQL generation, thus allowing 
    ///   otherwise invalid expressions to be removed prior to the generation 
    ///   phase.
    ///  </para>
    /// </remarks>
#if !MONO_STRICT
    public
#endif
    class ExpressionTranslator
    {
        /// <summary>
        ///  Translate the entire (outermost) expression.
        /// </summary>
        /// <param name="e">
        ///  A <see cref="SelectExpression" /> containing the expression to 
        ///  translate.
        /// </param>
        /// <returns>
        ///  The <see cref="SelectExpression" /> to use for SQL generation.
        /// </returns>
        /// <remarks>
        ///  <para>
        ///   Derived classes can override this method to manipulate the 
        ///   entire expression prior to SQL generation.
        ///  </para>
        ///  <para>
        ///   The default implementation returns <c>e</c> unchanged.
        ///  </para>
        /// </remarks>
        public virtual SelectExpression OuterExpression(SelectExpression e)
        {
            return e;
        }
    }
}
