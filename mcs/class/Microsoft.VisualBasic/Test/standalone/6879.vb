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
        Dim fn As Integer
        Dim strFileName As String
        Dim strPathName As String
        
        '// make sure all files are closed
        Microsoft.VisualBasic.FileSystem.Reset()
        Dim oDT1_1(3) As String
        Dim oDT1_2(3) As String
        Dim oDT2_1 As Boolean = True
        Dim oDT2_2 As Boolean
        Dim oDT3_1 As Byte = 1
        Dim oDT3_2 As Byte
        Dim oDT4_1 As Short = 100
        Dim oDT4_2 As Short
        Dim oDT5_1 As Integer = 1000
        Dim oDT5_2 As Integer
        Dim oDT6_1 As Long = 100000
        Dim oDT6_2 As Long
        Dim oDT7_1 As Char = "c"c
        Dim oDT7_2 As Char
        Dim oDT8_1 As Single = 2.2
        Dim oDT8_2 As Single
        Dim oDT9_1 As Double = 8.8
        Dim oDT9_2 As Double
        Dim oDT10_1 As Decimal = 10000000
        Dim oDT10_2 As Decimal
        Dim oDT11_1 As String = "zzz"
        Dim oDT11_2 As String
        Dim oDT12_1 As Date = #5/31/1993#
        Dim oDT12_2 As Date
        Dim strin As String
        oDT1_1(0) = "abc"
        oDT1_1(1) = "def"
        oDT1_1(2) = "ghi"
        oDT1_1(3) = "jkl"
        strPathName = System.IO.Directory.GetCurrentDirectory() + "/data/"
        strFileName = "6879.txt"
        'if this file exists - kill it
        If (strFileName = Dir(strPathName & strFileName)) Then
            Kill(strPathName & strFileName)
        End If
        ' Write text to file.
        fn = FreeFile()
        FileOpen(fn, strPathName & strFileName, OpenMode.Output)
        Print(fn, oDT1_1)
        Print(fn, oDT2_1)
        Print(fn, oDT3_1)
        Print(fn, oDT4_1)
        Print(fn, oDT5_1)
        Print(fn, oDT6_1)
        Print(fn, oDT7_1)
        Print(fn, oDT8_1)
        Print(fn, oDT9_1)
        Print(fn, oDT10_1)
        Print(fn, oDT11_1)
        Print(fn, oDT12_1)
        FileClose(fn)
        ' Input text from a file.
        fn = FreeFile()
        FileOpen(fn, strPathName & strFileName, OpenMode.Binary)
        strin = Space(1000)
        FileGet(fn, strin)
        FileClose(fn)
        If strin <> "abc           def           ghi           jklTrue 1  100  1000  100000 c 2.2  8.8  10000000 zzz31/05/1993 " Then Return "failed"
        Return "success"
    End Function
End Class
