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
    Public Function Test() As String
        Dim oDT1 As Byte
        Dim oDT2 As Short
        Dim oDT3 As Integer
        Dim oDT4 As Long
        Dim oDT5 As Single
        Dim oDT6 As Double
        Dim oDT7 As Decimal
        Dim oDT8 As String
        Dim oDT9 As Object
        Dim oDT10 As Boolean
        Dim oDT11 As Char
        Dim oDT12 As Date
        If IsNothing(oDT1) Then Return "failed 1"
        If IsNothing(oDT2) Then Return "failed 2"
        If IsNothing(oDT3) Then Return "failed 3"
        If IsNothing(oDT4) Then Return "failed 4"
        If IsNothing(oDT5) Then Return "failed 5"
        If IsNothing(oDT6) Then Return "failed 6"
        If IsNothing(oDT7) Then Return "failed 7"
        If Not IsNothing(oDT8) Then Return "failed 8"
        If Not IsNothing(oDT9) Then Return "failed 9"
        If IsNothing(oDT10) Then Return "failed 10"
        If IsNothing(oDT11) Then Return "failed 11"
        If IsNothing(oDT12) Then Return "failed 12"
        Return "Success"
    End Function
End Class
