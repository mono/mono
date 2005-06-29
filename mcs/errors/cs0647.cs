// cs0647.cs: Error during emitting `System.Runtime.InteropServices.GuidAttribute' attribute. The reason is `Invalid format for Guid.Guid(string).'
// Line: 5
using System.Runtime.InteropServices;

[Guid ("aaa")]

class X {
static void Main () {}
}
