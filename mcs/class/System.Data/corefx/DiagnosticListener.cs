// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

internal class DiagnosticListener 
{
	internal static bool DiagnosticListenerEnabled = false;
	internal DiagnosticListener(string s) {}
	internal bool IsEnabled(string s) => DiagnosticListenerEnabled;
	internal void Write(string s1, object s2) { System.Console.WriteLine($"|| {s1},  {s2}");}
}
