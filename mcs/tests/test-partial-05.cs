// Compiler options: -langversion:default

class B {
}

interface iface {
}

partial class A : B {
}

partial class A : iface {
}


partial class A2 : System.Object {
}

partial class A2 {
}

class D { public static void Main () {} }
