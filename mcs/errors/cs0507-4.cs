// CS0507: `DerivedClass.Message.set': cannot change access modifiers when overriding `public' inherited member `BaseClass.Message.set'
// Line: 19

class BaseClass {
        public virtual string Message {
		get {
			return "";
		}
		set {}
	}
}

class DerivedClass : BaseClass {
		public override string Message {
		get {
			return "";
		}
		
		private set {}
	}
}

