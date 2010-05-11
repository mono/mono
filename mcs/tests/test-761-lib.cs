// Compiler options: -t:library

using System;

public class DerivedAttribute : BaseAttribute {}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class BaseAttribute : System.Attribute {}