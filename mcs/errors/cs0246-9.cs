// error CS0246: Cannot find type 'foo'
// This is from bug #70758
struct bar {
        foo foo;
}
