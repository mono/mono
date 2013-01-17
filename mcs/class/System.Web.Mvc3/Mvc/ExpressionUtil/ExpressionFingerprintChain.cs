namespace System.Web.Mvc.ExpressionUtil {
    using System;
    using System.Collections.Generic;

    // Expression fingerprint chain class
    // Contains information used for generalizing, comparing, and recreating Expression instances
    //
    // Since Expression objects are immutable and are recreated for every invocation of an expression
    // helper method, they can't be compared directly. Fingerprinting Expression objects allows
    // information about them to be abstracted away, and the fingerprints can be directly compared.
    // Consider the process of fingerprinting that all values (parameters, constants, etc.) are hoisted
    // and replaced with dummies. What remains can be decomposed into a sequence of operations on specific
    // types and specific inputs.
    //
    // Some sample fingerprints chains:
    //
    // 2 + 4 -> OP_ADD, CONST:int, NULL, CONST:int
    // 2 + 8 -> OP_ADD, CONST:int, NULL, CONST:int
    // 2.0 + 4.0 -> OP_ADD, CONST:double, NULL, CONST:double
    //
    // 2 + 4 and 2 + 8 have the same fingerprint, but 2.0 + 4.0 has a different fingerprint since its
    // underlying types differ. Note that this looks a bit like prefix notation and is a side effect
    // of how the ExpressionVisitor class recurses into expressions. (Occasionally there will be a NULL
    // in the fingerprint chain, which depending on context can denote a static member, a null Conversion
    // in a BinaryExpression, and so forth.)
    //
    // "Hello " + "world" -> OP_ADD, CONST:string, NULL, CONST:string
    // "Hello " + {model} -> OP_ADD, CONST:string, NULL, PARAM_0:string
    //
    // These string concatenations have different fingerprints since the inputs are provided differently:
    // one is a constant, the other is a parameter.
    //
    // ({model} ?? "sample").Length -> MEMBER_ACCESS(String.Length), OP_COALESCE, PARAM_0:string, NULL, CONST:string
    // ({model} ?? "other sample").Length -> MEMBER_ACCESS(String.Length), OP_COALESCE, PARAM_0:string, NULL, CONST:string
    //
    // These expressions have the same fingerprint since all constants of the same underlying type are
    // treated equally.
    //
    // It's also important that the fingerprints don't reference the actual Expression objects that were
    // used to generate them, as the fingerprints will be cached, and caching a fingerprint that references
    // an Expression will root the Expression (and any objects it references).

    internal sealed class ExpressionFingerprintChain : IEquatable<ExpressionFingerprintChain> {

        public readonly List<ExpressionFingerprint> Elements = new List<ExpressionFingerprint>();

        public bool Equals(ExpressionFingerprintChain other) {
            // Two chains are considered equal if two elements appearing in the same index in
            // each chain are equal (value equality, not referential equality).

            if (other == null) {
                return false;
            }

            if (this.Elements.Count != other.Elements.Count) {
                return false;
            }

            for (int i = 0; i < this.Elements.Count; i++) {
                if (!Object.Equals(this.Elements[i], other.Elements[i])) {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj) {
            return Equals(obj as ExpressionFingerprintChain);
        }

        public override int GetHashCode() {
            HashCodeCombiner combiner = new HashCodeCombiner();
            Elements.ForEach(combiner.AddFingerprint);
            return combiner.CombinedHash;
        }

    }
}
