
namespace System.Reflection.Emit {
	public struct Label {
		public int label;

		public override bool Equals (object obj) {
			return false;
		}

		public override int GetHashCode () {
			return label;
		}
	}
}
