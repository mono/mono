// gcs0633-4.cs: The argument to the `System.Diagnostics.ConditionalAttribute' attribute must be a valid identifier
// Line: 6

using System.Diagnostics;

[Conditional("DEBUG+2")]
public class Test: System.Attribute {}
