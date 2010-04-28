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

using C5;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

#pragma warning disable 1711
namespace C5
{
  /// <summary>
  /// 
  /// </summary>
  public delegate void Act();
  /// <summary>
  /// <para>
  /// The type Act[T] corresponds to System.Action[T] in the .Net
  /// Framework class library.
  ///</para>
  /// </summary>
  /// <typeparam name="A1"></typeparam>
  /// <param name="x1"></param>
  public delegate void Act<A1>(A1 x1);
  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="A1"></typeparam>
  /// <typeparam name="A2"></typeparam>
  /// <param name="x1"></param>
  /// <param name="x2"></param>
  public delegate void Act<A1, A2>(A1 x1, A2 x2);
  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="A1"></typeparam>
  /// <typeparam name="A2"></typeparam>
  /// <typeparam name="A3"></typeparam>
  /// <param name="x1"></param>
  /// <param name="x2"></param>
  /// <param name="x3"></param>
  public delegate void Act<A1, A2, A3>(A1 x1, A2 x2, A3 x3);
  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="A1"></typeparam>
  /// <typeparam name="A2"></typeparam>
  /// <typeparam name="A3"></typeparam>
  /// <typeparam name="A4"></typeparam>
  /// <param name="x1"></param>
  /// <param name="x2"></param>
  /// <param name="x3"></param>
  /// <param name="x4"></param>
  public delegate void Act<A1, A2, A3, A4>(A1 x1, A2 x2, A3 x3, A4 x4);

  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="R"></typeparam>
  /// <returns></returns>
  public delegate R Fun<R>();
  /// <summary>
  /// Delegate type Fun[A1,R] is the type of functions (methods) from A1
  /// to R, used to compute some transformation for a given collection
  /// item. 
  /// <para>
  /// The type Fun[T,U] corresponds to System.Converter[T,U] in the .Net
  /// Framework class library, and the type Fun[T,bool] corresponds
  /// System.Predicate[T] in the .Net Framework class library.</para>
  /// </summary>
  /// <typeparam name="A1"></typeparam>
  /// <typeparam name="R"></typeparam>
  /// <param name="x1"></param>
  /// <returns></returns>
  public delegate R Fun<A1, R>(A1 x1);
  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="A1"></typeparam>
  /// <typeparam name="A2"></typeparam>
  /// <typeparam name="R"></typeparam>
  /// <param name="x1"></param>
  /// <param name="x2"></param>
  /// <returns></returns>
  public delegate R Fun<A1, A2, R>(A1 x1, A2 x2);
  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="A1"></typeparam>
  /// <typeparam name="A2"></typeparam>
  /// <typeparam name="A3"></typeparam>
  /// <typeparam name="R"></typeparam>
  /// <param name="x1"></param>
  /// <param name="x2"></param>
  /// <param name="x3"></param>
  /// <returns></returns>
  public delegate R Fun<A1, A2, A3, R>(A1 x1, A2 x2, A3 x3);
  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="A1"></typeparam>
  /// <typeparam name="A2"></typeparam>
  /// <typeparam name="A3"></typeparam>
  /// <typeparam name="A4"></typeparam>
  /// <typeparam name="R"></typeparam>
  /// <param name="x1"></param>
  /// <param name="x2"></param>
  /// <param name="x3"></param>
  /// <param name="x4"></param>
  /// <returns></returns>
  public delegate R Fun<A1, A2, A3, A4, R>(A1 x1, A2 x2, A3 x3, A4 x4);
}
