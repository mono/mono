namespace System.Activities.Presentation.Internal {

    using System.Diagnostics;
    using System.Runtime;

    //
    // An EqualityArray is an array of objects whose
    // .Equals method runs against all items in the 
    // array.  It is assumed that the data input
    // into the array is constant.  We use this in attributes
    // to offer a quick and accurate TypeId property.
    //
    internal class EqualityArray {
        private object[] _values;

        internal EqualityArray(params object[] values) {
            _values = values;
            Fx.Assert(_values != null && _values.Length > 0, "EqualityArray expects at least one value");
        }

        public override bool Equals(object other) {
            EqualityArray otherArray = other as EqualityArray;
            if (otherArray == null) return false;
            if (otherArray._values.Length != _values.Length) return false;
            for (int idx = 0; idx < _values.Length; idx++) {
                if (_values[idx] != otherArray._values[idx]) return false;
            }
            return true;
        }

        public override int GetHashCode() {
            return _values[0].GetHashCode();
        }
    }
}

