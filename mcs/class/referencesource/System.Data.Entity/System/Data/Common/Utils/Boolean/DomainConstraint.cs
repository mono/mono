//---------------------------------------------------------------------
// <copyright file="DomainConstraint.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace System.Data.Common.Utils.Boolean
{
    /// <summary>
    /// Represents a variable with finite domain, e.g., c in {1, 2, 3}
    /// </summary>
    /// <typeparam name="T_Element">Type of domain variables (int in the above example).</typeparam>
    /// <typeparam name="T_Variable">Type of the identifier (c above -- it need not be int).</typeparam>
    internal class DomainVariable<T_Variable, T_Element>
    {
        private readonly T_Variable _identifier;
        private readonly Set<T_Element> _domain;
        private readonly int _hashCode;
        private readonly IEqualityComparer<T_Variable> _identifierComparer;

        /// <summary>
        /// Constructs a new domain variable.
        /// </summary>
        /// <param name="identifier">Identifier </param>
        /// <param name="domain">Domain of variable.</param>
        /// <param name="identifierComparer">Comparer of identifier</param>
        internal DomainVariable(T_Variable identifier, Set<T_Element> domain, IEqualityComparer<T_Variable> identifierComparer)
        {
            Debug.Assert(null != identifier && null != domain);
            _identifier = identifier;
            _domain = domain.AsReadOnly();
            _identifierComparer = identifierComparer ?? EqualityComparer<T_Variable>.Default;
            int domainHashCode = _domain.GetElementsHashCode();
            int identifierHashCode = _identifierComparer.GetHashCode(_identifier);
            _hashCode = domainHashCode ^ identifierHashCode;
        }
        internal DomainVariable(T_Variable identifier, Set<T_Element> domain) : this(identifier, domain, null) { }

        /// <summary>
        /// Gets the variable.
        /// </summary>
        internal T_Variable Identifier { get { return _identifier; } }

        /// <summary>
        /// Gets the domain of this variable.
        /// </summary>
        internal Set<T_Element> Domain { get { return _domain; } }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj)) { return true; }
            DomainVariable<T_Variable, T_Element> other = obj as DomainVariable<T_Variable, T_Element>;
            if (null == other) { return false; }
            if (_hashCode != other._hashCode) { return false; }
            return (_identifierComparer.Equals(_identifier, other._identifier) && _domain.SetEquals(other._domain));
        }

        public override string ToString()
        {
            return StringUtil.FormatInvariant("{0}{{{1}}}",
                _identifier.ToString(), _domain);
        }
    }

    /// <summary>
    /// Represents a constraint of the form:
    /// 
    ///     Var1 in Range
    /// </summary>
    /// <typeparam name="T_Element">Type of range elements.</typeparam>
    /// <typeparam name="T_Variable">Type of the variable.</typeparam>
    internal class DomainConstraint<T_Variable, T_Element>
    {
        private readonly DomainVariable<T_Variable, T_Element> _variable;
        private readonly Set<T_Element> _range;
        private readonly int _hashCode;

        /// <summary>
        /// Constructs a new constraint for the given variable and range.
        /// </summary>
        /// <param name="variable">Variable in constraint.</param>
        /// <param name="range">Range of constraint.</param>
        internal DomainConstraint(DomainVariable<T_Variable, T_Element> variable, Set<T_Element> range)
        {
            Debug.Assert(null != variable && null != range);
            _variable = variable;
            _range = range.AsReadOnly();
            _hashCode = _variable.GetHashCode() ^ _range.GetElementsHashCode();
        }

        /// <summary>
        /// Constructor supporting a singleton range domain constraint
        /// </summary>
        internal DomainConstraint(DomainVariable<T_Variable, T_Element> variable, T_Element element)
            : this(variable, new Set<T_Element>(new T_Element[] { element }).MakeReadOnly())
        {
        }

        /// <summary>
        /// Gets the variable for this constraint.
        /// </summary>
        internal DomainVariable<T_Variable, T_Element> Variable { get { return _variable; } }

        /// <summary>
        /// Get the range for this constraint.
        /// </summary>
        internal Set<T_Element> Range { get { return _range; } }

        /// <summary>
        /// Inverts this constraint (this iff. !result)
        /// !(Var in Range) iff. Var in (Var.Domain - Range)
        /// </summary>
        /// <returns></returns>
        internal DomainConstraint<T_Variable, T_Element> InvertDomainConstraint()
        {
            return new DomainConstraint<T_Variable, T_Element>(_variable,
                _variable.Domain.Difference(_range).AsReadOnly());
        }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj)) { return true; }
            DomainConstraint<T_Variable, T_Element> other = obj as DomainConstraint<T_Variable, T_Element>;
            if (null == other) { return false; }
            if (_hashCode != other._hashCode) { return false; }
            return (_range.SetEquals(other._range) && _variable.Equals(other._variable));
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override string ToString()
        {
            return StringUtil.FormatInvariant("{0} in [{1}]",
                _variable, _range);
        }
    }
}
