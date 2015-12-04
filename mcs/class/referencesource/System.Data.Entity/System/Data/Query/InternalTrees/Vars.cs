//---------------------------------------------------------------------
// <copyright file="Vars.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace System.Data.Query.InternalTrees
{
    /// <summary>
    /// Types of variable
    /// </summary>
    internal enum VarType
    {
        /// <summary>
        /// a parameter
        /// </summary>
        Parameter,

        /// <summary>
        /// Column of a table
        /// </summary>
        Column,

        /// <summary>
        /// A Computed var
        /// </summary>
        Computed,

        /// <summary>
        /// Var for SetOps (Union, Intersect, Except)
        /// </summary>
        SetOp,

        /// <summary>
        /// NotValid
        /// </summary>
        NotValid
    }

    /// <summary>
    /// Same as a ValRef in SqlServer. I just like changing names :-)
    /// </summary>
    internal abstract class Var
    {
        int m_id;
        VarType m_varType;
        TypeUsage m_type;

        internal Var(int id, VarType varType, TypeUsage type)
        {
            m_id = id;
            m_varType = varType;
            m_type = type;
        }

        /// <summary>
        /// Id of this var
        /// </summary>
        internal int Id { get { return m_id; } }

        /// <summary>
        /// Kind of Var
        /// </summary>
        internal VarType VarType { get { return m_varType; } }

        /// <summary>
        /// Datatype of this Var
        /// </summary>
        internal TypeUsage Type { get { return m_type; } }

        /// <summary>
        /// Try to get the name of this Var. 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal virtual bool TryGetName(out string name)
        { 
            name = null;
            return false;
        }

        /// <summary>
        /// Debugging support
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "{0}", this.Id); ;
        }
    }

    /// <summary>
    /// Describes a query parameter
    /// </summary>
    internal sealed class ParameterVar : Var
    {
        string m_paramName;

        internal ParameterVar(int id, TypeUsage type, string paramName)
            : base(id, VarType.Parameter, type)
        {
            m_paramName = paramName;
        }

        /// <summary>
        /// Name of the parameter
        /// </summary>
        internal string ParameterName { get { return m_paramName; } }

        /// <summary>
        /// Get the name of this Var
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal override bool TryGetName(out string name)
        {
            name = this.ParameterName;
            return true;
        }
    }

    /// <summary>
    /// Describes a column of a table
    /// </summary>
    internal sealed class ColumnVar : Var
    {
        ColumnMD m_columnMetadata;
        Table m_table;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="table"></param>
        /// <param name="columnMetadata"></param>
        internal ColumnVar(int id, Table table, ColumnMD columnMetadata)
            : base(id, VarType.Column, columnMetadata.Type)
        {
            m_table = table;
            m_columnMetadata = columnMetadata;
        }

        /// <summary>
        /// The table instance containing this column reference
        /// </summary>
        internal Table Table { get { return m_table; } }

        /// <summary>
        /// The column metadata for this column
        /// </summary>
        internal ColumnMD ColumnMetadata { get { return m_columnMetadata; } }

        /// <summary>
        /// Get the name of this column var
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal override bool TryGetName(out string name)
        {
            name = m_columnMetadata.Name;
            return true;
        }
    }

    /// <summary>
    /// A computed expression. Defined by a VarDefOp
    /// </summary>
    internal sealed class ComputedVar : Var
    {
        internal ComputedVar(int id, TypeUsage type) : base(id, VarType.Computed, type)
        {
        }
    }

    /// <summary>
    /// A SetOp Var - used as the output var for set operations (Union, Intersect, Except)
    /// </summary>
    internal sealed class SetOpVar : Var
    {
        internal SetOpVar(int id, TypeUsage type) : base(id, VarType.SetOp, type) { }
    }

    //

    /// <summary>
    /// A VarVec is a compressed representation of a set of variables - with no duplicates
    /// and no ordering
    ///
    /// A VarVec should be used in many places where we expect a number of vars to be
    /// passed around; and we don't care particularly about the ordering of the vars
    ///
    /// This is obviously not suitable for representing sort keys, but is still
    /// reasonable for representing group by keys, and a variety of others.
    ///
    /// </summary>
    internal class VarVec : IEnumerable<Var>
    {
        #region Nested Classes
        /// <summary>
        /// A VarVec enumerator is a specialized enumerator for a VarVec.
        /// </summary>
        internal class VarVecEnumerator : IEnumerator<Var>, IDisposable
        {
            #region private state
            private int m_position;
            private Command m_command;
            private BitArray m_bitArray;
            #endregion

            #region Constructors
            /// <summary>
            /// Constructs a new enumerator for the specified Vec
            /// </summary>
            /// <param name="vec"></param>
            internal VarVecEnumerator(VarVec vec)
            {
                Init(vec);
            }
            #endregion

            #region public surface
            /// <summary>
            /// Initialize the enumerator to enumerate over the supplied Vec
            /// </summary>
            /// <param name="vec"></param>
            internal void Init(VarVec vec)
            {
                m_position = -1;
                m_command = vec.m_command;
                m_bitArray = vec.m_bitVector;
            }
            #endregion

            #region IEnumerator<Var> Members
            /// <summary>
            /// Get the Var at the current position
            /// </summary>
            public Var Current
            {
                get { return (m_position >= 0 && m_position < m_bitArray.Count) ? m_command.GetVar(m_position) : (Var)null; }
            }
            #endregion

            #region IEnumerator Members
            object IEnumerator.Current
            {
                get { return Current;}
            }

            /// <summary>
            /// Move to the next position
            /// </summary>
            /// <returns></returns>
            public bool MoveNext()
            {
                m_position++;
                for (; m_position < m_bitArray.Count; m_position++)
                {
                    if (m_bitArray[m_position])
                    {
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// Reset enumerator to start off again
            /// </summary>
            public void Reset()
            {
                m_position = -1;
            }
            #endregion

            #region IDisposable Members
            /// <summary>
            /// Dispose of the current enumerator - return it to the Command
            /// </summary>
            public void Dispose()
            {
                // Technically, calling GC.SuppressFinalize is not required because the class does not
                // have a finalizer, but it does no harm, protects against the case where a finalizer is added
                // in the future, and prevents an FxCop warning.
                GC.SuppressFinalize(this);
                m_bitArray = null;
                m_command.ReleaseVarVecEnumerator(this);
            }

            #endregion
        }
        #endregion

        #region public methods
        internal void Clear()
        {
            m_bitVector.Length = 0;
        }

        internal void And(VarVec other)
        {
            Align(other);
            m_bitVector.And(other.m_bitVector);
        }

        internal void Or(VarVec other)
        {
            Align(other);
            m_bitVector.Or(other.m_bitVector);
        }
        
        /// <summary>
        /// Computes (this Minus other) by performing (this And (Not(other)))
        /// A temp VarVec is used and released at the end of the operation
        /// </summary>
        /// <param name="other"></param>
        internal void Minus(VarVec other)
        {
            VarVec tmp = m_command.CreateVarVec(other);
            tmp.m_bitVector.Length = m_bitVector.Length;
            tmp.m_bitVector.Not();
            this.And(tmp);
            m_command.ReleaseVarVec(tmp);
        }

        /// <summary>
        /// Does this have a non-zero overlap with the other vec
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        internal bool Overlaps(VarVec other)
        {
            VarVec otherCopy = m_command.CreateVarVec(other);
            otherCopy.And(this);
            bool overlaps = !otherCopy.IsEmpty;
            m_command.ReleaseVarVec(otherCopy);
            return overlaps;
        }

        /// <summary>
        /// Does this Vec include every var in the other vec?
        /// Written this way deliberately under the assumption that "other"
        /// is a relatively small vec
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        internal bool Subsumes(VarVec other)
        {
            for (int i = 0; i < other.m_bitVector.Count; i++)
            {
                if (other.m_bitVector[i] && 
                    ((i >= this.m_bitVector.Count) || !this.m_bitVector[i]))
                {
                    return false;
                }
            }
            return true;
        }

        internal void InitFrom(VarVec other)
        {
            this.Clear();
            this.m_bitVector.Length = other.m_bitVector.Length;
            this.m_bitVector.Or(other.m_bitVector);
        }

        internal void InitFrom(IEnumerable<Var> other)
        {
            InitFrom(other, false);
        }

        internal void InitFrom(IEnumerable<Var> other, bool ignoreParameters)
        {
            this.Clear();
            foreach (Var v in other)
            {
                if (!ignoreParameters || (v.VarType != VarType.Parameter))
                {
                    this.Set(v);
                }
            }
        }

        /// <summary>
        /// The enumerator pattern
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Var> GetEnumerator()
        {
            return m_command.GetVarVecEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Number of vars in this set
        /// </summary>
        internal int Count
        {
            get
            {
                int count = 0;
                foreach (Var v in this)
                    count++;
                return count;
            }
        }

        internal bool IsSet(Var v)
        {
            Align(v.Id);
            return m_bitVector.Get(v.Id);
        }
        internal void Set(Var v)
        {
            Align(v.Id);
            m_bitVector.Set(v.Id, true);
        }
        internal void Clear(Var v)
        {
            Align(v.Id);
            m_bitVector.Set(v.Id, false);
        }

        /// <summary>
        /// Is this Vec empty?
        /// </summary>
        internal bool IsEmpty
        {
            get { return this.First == null;}
        }

        /// <summary>
        /// Get me the first var that is set
        /// </summary>
        internal Var First
        {
            get
            {
                foreach (Var v in this)
                {
                    return v;
                }
                return null;
            }
        }

        /// <summary>
        /// Walk through the input varVec, replace any vars that have been "renamed" based
        /// on the input varMap, and return the new VarVec
        /// </summary>
        /// <param name="varMap">dictionary of renamed vars</param>
        /// <returns>a new VarVec</returns>
        internal VarVec Remap(Dictionary<Var, Var> varMap)
        {
            VarVec newVec = m_command.CreateVarVec();
            foreach (Var v in this)
            {
                Var newVar;
                if (!varMap.TryGetValue(v, out newVar))
                {
                    newVar = v;
                }
                newVec.Set(newVar);
            }
            return newVec;
        }

        #endregion

        #region constructors
        internal VarVec(Command command)
        {
            m_bitVector = new BitArray(64);
            m_command = command;
        }
        #endregion

        #region private methods
        private void Align(VarVec other)
        {
            if (other.m_bitVector.Count == this.m_bitVector.Count)
                return;
            if (other.m_bitVector.Count > this.m_bitVector.Count)
            {
                this.m_bitVector.Length = other.m_bitVector.Count;
            }
            else
            {
                other.m_bitVector.Length = this.m_bitVector.Count;
            }
        }
        private void Align(int idx)
        {
            if (idx >= m_bitVector.Count)
            {
                m_bitVector.Length = idx + 1;
            }
        }

        /// <summary>
        /// Debugging support
        /// provide a string representation for debugging.
        /// <returns></returns>
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string separator = String.Empty;

            foreach (Var v in this)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}", separator, v.Id);
                separator = ",";
            }
            return sb.ToString();
        }
        #endregion

        #region private state
        private BitArray m_bitVector;
        private Command m_command;
        #endregion

        #region Clone
        /// <summary>
        /// Create a clone of this vec
        /// </summary>
        /// <returns></returns>
        public VarVec Clone()
        {
            VarVec newVec = m_command.CreateVarVec();
            newVec.InitFrom(this);
            return newVec;
        }

        #endregion
    }

    /// <summary>
    /// An ordered list of Vars. Use this when you need an ordering.
    /// </summary>
    [DebuggerDisplay("{{{ToString()}}}")]
    internal class VarList : List<Var>
    {
        #region constructors
        /// <summary>
        /// Trivial constructor
        /// </summary>
        internal VarList() : base() { }

        /// <summary>
        /// Not so trivial constructor
        /// </summary>
        /// <param name="vars"></param>
        internal VarList(IEnumerable<Var> vars) : base(vars) { }
        #endregion

        #region public methods

        /// <summary>
        /// Debugging support
        /// provide a string representation for debugging.
        /// <returns></returns>
        /// </summary>
        public override string ToString() 
        {
            StringBuilder sb = new StringBuilder();
            string separator = String.Empty;

            foreach (Var v in this) 
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}", separator, v.Id);
                separator = ",";
            }
            return sb.ToString();
        }

        #endregion
    }


    #region VarMap
    /// <summary>
    /// Helps map one variable to the next.
    /// </summary>
    internal class VarMap: Dictionary<Var, Var>
    {
        #region public surfaces

        internal VarMap GetReverseMap()
        {
            VarMap reverseMap = new VarMap();
            foreach (KeyValuePair<Var, Var> kv in this)
            {
                Var x;
                // On the odd chance that a var is in the varMap more than once, the first one
                // is going to be the one we want to use, because it might be the discriminator
                // var;
                if (!reverseMap.TryGetValue(kv.Value, out x))
                {
                    reverseMap[kv.Value] = kv.Key;
                }
            }
            return reverseMap;
        }
        
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string separator = string.Empty;

            foreach (Var v in this.Keys)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}({1},{2})", separator, v.Id, this[v].Id);
                separator = ",";
            }
            return sb.ToString();
        }

        #endregion

        #region constructors
        internal VarMap() : base() { }
        #endregion
    }
    #endregion
}
