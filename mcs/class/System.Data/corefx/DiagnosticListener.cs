using System;

internal class DiagnosticListener 
{
    internal DiagnosticListener(string s) {}
    internal bool IsEnabled(string s) => false;
    internal void Write(string s1, object s2) { System.Console.WriteLine($"|| {s1},  {s2}");}
}
