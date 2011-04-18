// CS1715: `DerivedClass.Prop': type must be `bool' to match overridden member `BaseClass.Prop'
// Line: 9

class BaseClass {
        protected virtual bool Prop { set {} }
}

class DerivedClass: BaseClass {
        protected override int Prop { set {} }
}



