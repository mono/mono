// cs0544.cs: 'DerivedClass.value' : cannot override; 'BaseClass.value' is not a property
// Line: 9

class BaseClass {
        protected int value;
}

class DerivedClass: BaseClass {
        protected override int value { get {} }
}

