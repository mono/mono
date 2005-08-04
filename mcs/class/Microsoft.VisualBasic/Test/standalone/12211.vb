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
        Dim oDT1 As Byte = 1
        Dim oDT2 As Short = 1
        Dim oDT3 As Integer = 1
        Dim oDT4 As Long = 1000
        Dim oDT5 As Single = 1.1
        Dim oDT6 As Double = 2.2
        Dim oDT7 As Decimal = 1000
        Dim oDT8 As String = "abc"
        Dim oDT9 As Object = oDT1
        Dim oDT10 As Boolean = True
        Dim oDT11 As Char = "c"c
        Dim oDT12 As Date = #5/31/1993#
        Dim oDT13 As String = ""
        If Not IsNumeric(oDT1) Then Return "failed 1"
        If Not IsNumeric(oDT2) Then Return "failed 2"
        If Not IsNumeric(oDT3) Then Return "failed 3"
        If Not IsNumeric(oDT4) Then Return "failed 4"
        If Not IsNumeric(oDT5) Then Return "failed 5"
        If Not IsNumeric(oDT6) Then Return "failed 6"
        If Not IsNumeric(oDT7) Then Return "failed 7"
        If IsNumeric(oDT8) Then Return "failed 8"
        If Not IsNumeric(oDT9) Then Return "failed 9"
        If Not IsNumeric(oDT10) Then Return "failed 10"
        If IsNumeric(oDT11) Then Return "failed 11"
        If IsNumeric(oDT12) Then Return "failed 12"
        If IsNumeric(oDT13) Then Return "failed 13"
        Return "Success"
    End Function
End Class
