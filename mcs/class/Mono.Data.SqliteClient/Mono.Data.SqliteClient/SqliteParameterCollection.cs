// -*- c-basic-offset: 8; inent-tabs-mode: nil -*-
//
//  SqliteParameterCollection.cs
//
//  Author(s): Vladimir Vukicevic  <vladimir@pobox.com>
//
//  Copyright (C) 2002  Vladimir Vukicevic
//

using System;
using System.Data;
using System.Collections;

namespace Mono.Data.SqliteClient
{
        public class SqliteParameterCollection : IDataParameterCollection,
                IList
        {
                ArrayList numeric_param_list = new ArrayList ();
                Hashtable named_param_hash = new Hashtable ();

                public IEnumerator GetEnumerator ()
                {
                        throw new NotImplementedException ();
                }

                public void RemoveAt (string parameterName)
                {
                        if (!named_param_hash.Contains (parameterName))
                                throw new ApplicationException ("Parameter " + parameterName + " not found");

                        numeric_param_list.RemoveAt ((int) named_param_hash[parameterName]);
                        named_param_hash.Remove (parameterName);

                        RecreateNamedHash ();
                }

                public void RemoveAt (SqliteParameter param)
                {
                        RemoveAt (param.ParameterName);
                }

                public void RemoveAt (int index)
                {
                        RemoveAt (((SqliteParameter) numeric_param_list[index]).ParameterName);
                }

                int IList.IndexOf (object o)
                {
                        return IndexOf ((SqliteParameter) o);
                }

                public int IndexOf (string parameterName)
                {
                        return (int) named_param_hash[parameterName];
                }

                public int IndexOf (SqliteParameter param)
                {
                        return IndexOf (param.ParameterName);
                }

                bool IList.Contains (object value)
                {
                        return Contains ((SqliteParameter) value);
                }

                public bool Contains (string parameterName)
                {
                        return named_param_hash.Contains (parameterName);
                }

                public bool Contains (SqliteParameter param)
                {
                        return Contains (param.ParameterName);
                }

                object IList.this[int index] {
                        get {
                                return this[index];
                        }
                        set {
                                CheckSqliteParam (value);
                                this[index] = (SqliteParameter) value;
                        }
                }

                object IDataParameterCollection.this[string parameterName] {
                        get {
                                return this[parameterName];
                        }
                        set {
                                CheckSqliteParam (value);
                                this[parameterName] = (SqliteParameter) value;
                        }
                }

                public SqliteParameter this[string parameterName] {
                        get {
                                return this[(int) named_param_hash[parameterName]];
                        }
                        set {
                                if (this.Contains (parameterName))
                                        numeric_param_list[(int) named_param_hash[parameterName]] = value;
                                else          // uhm, do we add it if it doesn't exist? what does ms do?
                                        Add (value);
                        }
                }

                public SqliteParameter this[int parameterIndex] {
                        get {
                                return (SqliteParameter) numeric_param_list[parameterIndex];
                        }
                        set {
                                numeric_param_list[parameterIndex] = value;
                        }
                }

                public int Add (object value)
                {
                        CheckSqliteParam (value);
                        SqliteParameter sqlp = (SqliteParameter) value;
                        if (named_param_hash.Contains (sqlp.ParameterName))
                                throw new DuplicateNameException ("Parameter collection already contains given value.");

                        named_param_hash[value] = numeric_param_list.Add (value);

                        return (int) named_param_hash[value];
                }

                // IList

                public SqliteParameter Add (SqliteParameter param)
                {
                        Add ((object)param);
                        return param;
                }

                public SqliteParameter Add (string name, object value)
                {
                        return Add (new SqliteParameter (name, value));
                }

                public SqliteParameter Add (string name, DbType type)
                {
                        return Add (new SqliteParameter (name, type));
                }

                public bool IsFixedSize {
                        get {
                                return false;
                        }
                }

                public bool IsReadOnly {
                        get {
                                return false;
                        }
                }

                public void Clear ()
                {
                        numeric_param_list.Clear ();
                        named_param_hash.Clear ();
                }

                public void Insert (int index, object value)
                {
                        CheckSqliteParam (value);
                        if (numeric_param_list.Count == index) {
                                Add (value);
                                return;
                        }

                        numeric_param_list.Insert (index, value);
                        RecreateNamedHash ();
                }

                public void Remove (object value)
                {
                        CheckSqliteParam (value);
                        RemoveAt ((SqliteParameter) value);
                }

                // ICollection

                public int Count {
                        get {
                                return numeric_param_list.Count;
                        }
                }

                public bool IsSynchronized {
                        get {
                                return false;
                        }
                }

                public object SyncRoot {
                        get {
                                return null;
                        }
                }

                public void CopyTo (Array array, int index)
                {
                        throw new NotImplementedException ();
                }

                private void CheckSqliteParam (object value)
                {
                        if (!(value is SqliteParameter))
                                throw new InvalidCastException ("Can only use SqliteParameter objects");
                }

                private void RecreateNamedHash ()
                {
                        for (int i = 0; i < numeric_param_list.Count; i++) {
                                named_param_hash[((SqliteParameter) numeric_param_list[i]).ParameterName] = i;
                        }
                }
        }
}
