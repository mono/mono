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
        Dim oDT1(2) As Byte
        Dim oDT2(2) As Short
        Dim oDT3(2) As Integer
        Dim oDT4(2) As Long
        Dim oDT5(2) As Single
        Dim oDT6(2) As Double
        Dim oDT7(2) As Decimal
        Dim oDT8(2) As String
        Dim oDT9(2) As Object
        Dim oDT10(2) As Boolean
        Dim oDT11(2) As Char
        Dim oDT12(2) As Date
        If Not IsArray(oDT1) Then Return "failed"
        If Not IsArray(oDT2) Then Return "failed"
        If Not IsArray(oDT3) Then Return "failed"
        If Not IsArray(oDT4) Then Return "failed"
        If Not IsArray(oDT5) Then Return "failed"
        If Not IsArray(oDT6) Then Return "failed"
        If Not IsArray(oDT7) Then Return "failed"
        If Not IsArray(oDT8) Then Return "failed"
        If Not IsArray(oDT9) Then Return "failed"
        If Not IsArray(oDT10) Then Return "failed"
        If Not IsArray(oDT11) Then Return "failed"
        If Not IsArray(oDT12) Then Return "failed"
        Return "Success"
    End Function
End Class
