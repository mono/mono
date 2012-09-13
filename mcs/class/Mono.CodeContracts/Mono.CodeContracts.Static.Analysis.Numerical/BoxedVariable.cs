// 
// BoxedVariable.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2012 Alexander Chebaturkin
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//  
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        class BoxedVariable<TVar> : IEquatable<BoxedVariable<TVar>> {
                readonly InnerVariable inner_variable;
                readonly VariableKind kind;
                readonly TVar variable;

                public BoxedVariable (TVar variable)
                {
                        if (variable != null) {
                                this.variable = variable;
                                inner_variable = null;
                                kind = VariableKind.Normal;
                        }
                        else {
                                this.variable = default(TVar);
                                inner_variable = new InnerVariable ();
                                kind = VariableKind.Slack;
                        }
                }

                BoxedVariable ()
                {
                        variable = default (TVar);
                        inner_variable = new InnerVariable ();
                        kind = VariableKind.Slack;
                }

                public bool Equals (BoxedVariable<TVar> that)
                {
                        if (ReferenceEquals (this, that))
                                return true;

                        if (ReferenceEquals (that, null))
                                return false;

                        if (inner_variable != null)
                                return inner_variable.Equals (that.inner_variable);

                        return variable != null && variable.Equals (that.variable);
                }

                public override bool Equals (object obj)
                {
                        if (ReferenceEquals (this, obj))
                                return true;

                        if (obj is TVar && inner_variable == null)
                                return variable.Equals (obj);

                        var that = obj as BoxedVariable<TVar>;
                        if (that != null) {
                                if (inner_variable != null)
                                        return inner_variable.Equals (that.inner_variable);

                                if (variable != null)
                                        return variable.Equals (that.variable);
                        }

                        return false;
                }

                public override int GetHashCode ()
                {
                        return inner_variable != null
                                       ? inner_variable.GetHashCode ()
                                       : variable.GetHashCode ();
                }

                public override string ToString ()
                {
                        if (kind == VariableKind.Slack)
                                return "s" + inner_variable;

                        if (inner_variable != null)
                                return inner_variable.ToString ();

                        return variable.ToString ();
                }

                public bool TryUnpackVariable (out TVar value)
                {
                        if (inner_variable != null)
                                return false.Without (out value);

                        return true.With (variable, out value);
                }

                public static BoxedVariable<TVar> SlackVariable ()
                {
                        return new BoxedVariable<TVar> ();
                }

                public static void ResetFreshVariableCounter ()
                {
                        InnerVariable.ResetFreshVariableCounter ();
                }

                #region Nested type: InnerVariable

                class InnerVariable {
                        static int count;
                        static int start_count;

                        readonly int id;
                        readonly int start_id;

                        public InnerVariable ()
                        {
                                id = count++;
                                start_id = start_count++;
                        }

                        public override bool Equals (object obj)
                        {
                                if (ReferenceEquals (this, obj))
                                        return true;

                                var innerVariable = obj as InnerVariable;
                                return innerVariable != null && innerVariable.id == id;
                        }

                        public override int GetHashCode ()
                        {
                                return id;
                        }

                        public override string ToString ()
                        {
                                return string.Format ("iv{0}", id - start_id);
                        }

                        public static void ResetFreshVariableCounter ()
                        {
                                start_count = count;
                        }
                }

                #endregion

                #region Nested type: VariableKind

                enum VariableKind {
                        Normal,
                        Slack
                }

                #endregion
        }
}