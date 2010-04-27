// CS0507: `DerivedClass.Message': cannot change access modifiers when overriding `public' inherited member `BaseClass.Message'
// Line: 12

class BaseClass {
	public virtual string Message {
		set {
		}
	}
}

class DerivedClass : BaseClass {
	protected override string Message {
		set {
		}
	}
}

