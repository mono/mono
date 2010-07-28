// CS0647: Error during emitting `System.Runtime.InteropServices.GuidAttribute' attribute. The reason is `Invalid Guid format: aaa'
// Line: 6

using System.Runtime.InteropServices;

[Guid ("aaa")]
class X {
static void Main () {}
}
