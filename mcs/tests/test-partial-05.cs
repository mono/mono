// Compiler options: -langversion:default

class B {
}

interface iface {
}

partial class A : B {
}

partial class A : iface {
}

class D { static void Main () {} }
