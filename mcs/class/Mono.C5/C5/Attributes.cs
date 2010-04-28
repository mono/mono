/*
 Copyright (c) 2003-2006 Niels Kokholm and Peter Sestoft
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
*/
using System;
using System.Diagnostics;
using SCG = System.Collections.Generic;
namespace C5
{
  /// <summary>
  /// A custom attribute to mark methods and properties as being tested 
  /// sufficiently in the regression test suite.
  /// </summary>
  [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
  public sealed class TestedAttribute : Attribute
  {

    /// <summary>
    /// Optional reference to test case
    /// </summary>
    [Tested]
    public string via;


    /// <summary>
    /// Pretty print attribute value
    /// </summary>
    /// <returns>"Tested via " + via</returns>
    [Tested]
    public override string ToString() { return "Tested via " + via; }
  }
}