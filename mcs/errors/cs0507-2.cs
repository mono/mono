// cs0506.cs: 'DerivedClass.Message.set()' : cannot change access modifiers when overriding inherited member 'BaseClass.Message.set'
// Line: 15

class BaseClass {
        public virtual string Message {
		get {
			return "";
		}
		protected set {
		}
	}
}

class DerivedClass : BaseClass {
        public override string Message {
		get {
			return "";
		}
		set {
		}
	}
}

