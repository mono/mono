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
        Dim s5 As String = "e"
        Dim col As New Microsoft.VisualBasic.Collection()
        col.Add(s1, "key1", Nothing, Nothing)
        col.Add(s2, "key2", 1, Nothing)
        col.Add(s3, "key3", 1, Nothing)
        col.Add(s4, "key4", "key3", Nothing)
        col.Add(s5, "key5", "key2", Nothing)
        If col.Count <> 5 Then Return 2
        If col(4).ToString <> "b" Then Return 4
        If col(1).ToString <> "d" Then Return 8
        If col("key2").ToString <> "b" Then Return 16
        Return 1
    End Function
End Class
