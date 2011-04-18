// CS0118: `bar.foo' is a `field' but a `type' was expected
// Line: 6
// This is from bug #70758

struct bar {
        foo foo;
}

