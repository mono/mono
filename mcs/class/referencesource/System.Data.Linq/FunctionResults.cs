using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq {
    /// <summary>
    /// The result of executing a query.
    /// </summary>
    public interface IExecuteResult :  IDisposable {
        /// <summary>
        /// The return value or result of the executed query. This object has the same type as the
        /// query expression's Type property.
        /// </summary>
        object ReturnValue { get; }

        /// <summary>
        /// Retrieves the nth output parameter.  This method is normally used when the query is a mapped
        /// function with output parameters.
        /// </summary>
        /// <param name="parameterIndex"></param>
        /// <returns></returns>
        object GetParameterValue(int parameterIndex);
    }

    /// <summary>
    /// Interface providing access to a function return value.
    /// </summary>
    public interface IFunctionResult {
        /// <summary>
        /// The value.
        /// </summary>
        object ReturnValue { get; }
    }

    /// <summary>
    /// An interface for representing the result of a mapped function with a single return sequence.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification="tadam: Meant to represent a database table which is delayed loaded and doesn't provide collection semantics.")]
    public interface ISingleResult<T> : IEnumerable<T>, IFunctionResult, IDisposable { }

    /// <summary>
    /// An interface for representing results of mapped functions or queries with variable return sequences.
    /// </summary>
    public interface IMultipleResults : IFunctionResult, IDisposable {
        /// <summary>
        /// Retrieves the next result as a sequence of Type 'TElement'.
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "tadam: Generic parameters are required for strong-typing of the return type.")]
        IEnumerable<TElement> GetResult<TElement>();
    }
}