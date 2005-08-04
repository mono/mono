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
Imports System
Imports Microsoft.VisualBasic
Public Class TestClass
    Public Function Test() As String
        Dim i As Integer
        Dim Arr(6) As Integer
        Dim Arr2(2, 4, 6) As Integer
        Dim o As Object
        Dim caughtException As Boolean
        '// Array is Nothing.
        caughtException = False
        Try
            Arr = Nothing
            i = LBound(Arr)
        Catch e As ArgumentNullException
            If Err.Number = 9 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "failed at sub test 1"
        '// Array is Nothing.
        caughtException = False
        Try
            i = LBound(o)
        Catch e As ArgumentNullException
            If Err.Number = 9 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "failed at sub test 2"
        '// Rank < 1 or Rank is greater than the rank of Array
        caughtException = False
        Try
            i = LBound(Arr2, 0)
        Catch e As RankException
            If Err.Number = 9 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "failed at sub test 3"
        caughtException = False
        Try
            i = LBound(Arr2, 4)
        Catch e As RankException
            If Err.Number = 9 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "failed at sub test 4"
        Return "success"
    End Function
End Class
