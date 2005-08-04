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
Imports Microsoft.VisualBasic.Collection
Public Class TestClass
    Public Function Test() As Integer
        'BeginCode
        Dim s1 As String = "a"
        Dim s2 As String = "b"
        Dim s3 As String = "c"
        Dim s4 As String = "d"
        Dim col As New Microsoft.VisualBasic.Collection()
        col.Add(s1, Nothing, Nothing, 0)
        col.Add(s2, Nothing, Nothing, 1)
        col.Add(s3, Nothing, Nothing, 1)
        col.Add(s4, Nothing, Nothing, 3)
        If col.Count <> 4 Then Return 2
        If col(4).ToString <> "d" Then Return 4
        If col(1).ToString <> "a" Then Return 8
        If col(2).ToString <> "c" Then Return 16
        Return 1
        'EndCode
    End Function
End Class
