// cs0508.cs: 'DerivedClass.Prop' : cannot change return type when overriding inherited member 'BaseClass.Prop'
// Line: 9

class BaseClass {
        protected virtual bool Prop { set {} }
}

class DerivedClass: BaseClass {
        protected override int Prop { set {} }
}



