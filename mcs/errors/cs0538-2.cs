// cs0538.cs: 'BaseClass' in explicit interface declaration is not an interface
// Line: 11

class BaseClass {
        public void Foo() {}
}

public enum E {}

class InstanceClass: E {
        void BaseClass.Foo() {
        }
}
