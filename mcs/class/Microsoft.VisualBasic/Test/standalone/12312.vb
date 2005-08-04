  '
  ' Copyright (c) 2002-2003 Mainsoft Corporation.
  '
  ' Permission is hereby granted, free of charge, to any person obtaining a
  ' copy of this software and associated documentation files (the "Software"),
  ' to deal in the Software without restriction, including without limitation
  ' the rights to use, copy, modify, merge, publish, distribute, sublicense,
  ' and/or sell copies of the Software, and to permit persons to whom the
  ' Software is furnished to do so, subject to the following conditions:
  ' 
  ' The above copyright notice and this permission notice shall be included in
  ' all copies or substantial portions of the Software.
  ' 
  ' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  ' IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  ' FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  ' AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  ' LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
  ' FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
  ' DEALINGS IN THE SOFTWARE.
  '
Imports Microsoft.VisualBasic
Public Class TestClass
    'The expressions in the argument list can include function calls. 
    'As part of preparing the argument list for the call to Choose, 
    'the Visual Basic compiler calls every function in every expression. 
    'This means that you cannot rely on a particular function 
    'not being called if a different expression is selected by Index.
    Private m_str As String
    Public Function Test() As String
        Dim i As Integer
        Dim str1 As String
        i = 2
        str1 = CStr(Choose(i, foo1("a", "b"), foo2("c")))
        Return str1
    End Function
    Public Function foo1(ByVal val1 As String, ByVal val2 As String) As String
        m_str = m_str & val1 & val2
        Return "foo1" & m_str
    End Function
    Public Function foo2(ByVal val3 As String) As String
        m_str = m_str & val3
        Return "foo2" & m_str
    End Function
End Class
