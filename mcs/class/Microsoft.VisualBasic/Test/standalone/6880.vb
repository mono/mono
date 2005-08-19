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
Imports System
Public Class TestClass
    Public Function Test() As String
        'if Output list data is DBNull, Null is written to the file
        Dim fn As Integer
        Dim str1 As String
        Dim str2 As String
        Dim strFileName As String
        Dim strPathName As String
        
        '// make sure all files are closed
        Microsoft.VisualBasic.FileSystem.Reset()
        strPathName = System.IO.Directory.GetCurrentDirectory() + "/data/"
        strFileName = "6880.txt"
        'if this file exists - kill it
        If (strFileName = Dir(strPathName & strFileName)) Then
            Kill(strPathName & strFileName)
        End If
        Dim c As Char 'c is null
        ' Write text to file.
        fn = FreeFile()
        FileOpen(fn, strPathName & strFileName, OpenMode.Output)
        Print(fn, "dbnull:", DBNull.Value)
        ' Generate an overflow exception.
        On Error Resume Next
        Dim zero As Integer
        Dim result As Integer
        zero = 0
        result = 8 / zero
        Print(fn, "err object:", Err)
        FileClose(fn)
        ' Input text from a file.
        fn = FreeFile()
        FileOpen(fn, strPathName & strFileName, OpenMode.Input)
        str2 = Space(100)
        Input(fn, str2)
        FileClose(fn)
        Return str2
    End Function
End Class
