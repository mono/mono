/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if CLR2
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
#if SILVERLIGHT
using System.Core;
#endif

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic.Utils;

namespace System.Dynamic {

    /// <summary>
    /// Describes arguments in the dynamic binding process.
    /// </summary>
    /// <remarks>
    /// ArgumentCount - all inclusive number of arguments.
    /// ArgumentNames - names for those arguments that are named.
    ///
    /// Argument names match to the argument values in left to right order 
    /// and last name corresponds to the last argument.
    /// 
    /// Example:
    ///   Foo(arg1, arg2, arg3, name1 = arg4, name2 = arg5, name3 = arg6)
    ///
    ///   will correspond to:
    ///    ArgumentCount: 6
    ///    ArgumentNames: {"name1", "name2", "name3"}
    /// </remarks>
    public sealed class CallInfo {
        private readonly int _argCount;
        private readonly ReadOnlyCollection<string> _argNames;

        /// <summary>
        /// Creates a new PositionalArgumentInfo.
        /// </summary>
        /// <param name="argCount">The number of arguments.</param>
        /// <param name="argNames">The argument names.</param>
        /// <returns>The new CallInfo</returns>
        public CallInfo(int argCount, params string[] argNames)
            : this(argCount, (IEnumerable<string>)argNames) {
        }

        /// <summary>
        /// Creates a new CallInfo that represents arguments in the dynamic binding process.
        /// </summary>
        /// <param name="argCount">The number of arguments.</param>
        /// <param name="argNames">The argument names.</param>
        /// <returns>The new CallInfo</returns>
        public CallInfo(int argCount, IEnumerable<string> argNames) {
            ContractUtils.RequiresNotNull(argNames, "argNames");

            var argNameCol = argNames.ToReadOnly();

            if (argCount < argNameCol.Count) throw Error.ArgCntMustBeGreaterThanNameCnt();
            ContractUtils.RequiresNotNullItems(argNameCol, "argNames");

            _argCount = argCount;
            _argNames = argNameCol;
        }

        /// <summary>
        /// The number of arguments.
        /// </summary>
        public int ArgumentCount {
            get { return _argCount; }
        }

        /// <summary>
        /// The argument names.
        /// </summary>
        public ReadOnlyCollection<string> ArgumentNames {
            get { return _argNames; }
        }

        /// <summary>
        /// Serves as a hash function for the current CallInfo.
        /// </summary>
        /// <returns>A hash code for the current CallInfo.</returns>
        public override int GetHashCode() {
            return _argCount ^ _argNames.ListHashCode();
        }

        /// <summary>
        /// Determines whether the specified CallInfo instance is considered equal to the current.
        /// </summary>
        /// <param name="obj">The instance of CallInfo to compare with the current instance.</param>
        /// <returns>true if the specified instance is equal to the current one otherwise, false.</returns>
        public override bool Equals(object obj) {
            var other = obj as CallInfo;
            return _argCount == other._argCount && _argNames.ListEquals(other._argNames);
        }
    }
}
