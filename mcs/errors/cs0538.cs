// cs0538.cs: 'BaseClass' in explicit interface declaration is not an interface
// Line: 9

class BaseClass {
        public void Foo() {}
}

class InstanceClass: BaseClass {
        void BaseClass.Foo() {
        }
}



