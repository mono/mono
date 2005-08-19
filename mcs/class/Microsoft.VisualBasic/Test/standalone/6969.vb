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
Imports System.IO
Imports System
Public Class TestClass
    Public Function Test() As String
        Dim fn As Integer
        Dim str1 As String
        Dim strFileName As String
        Dim strPathName As String
        
        '// make sure all files are closed
        Microsoft.VisualBasic.FileSystem.Reset()
        strPathName = System.IO.Directory.GetCurrentDirectory() + "/data/"
        strFileName = "6969.txt"
        Dim caughtException As Boolean
        '// file number does not exist.
        caughtException = False
        Try
            InputString(256, 1)
        Catch e As IOException
            If Err.Number = 52 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "sub test 1 failed"
        '// CharCount < 0 or > 2^14.
        caughtException = False
        Try
            InputString(256, -1) '-1 is checked before file number
        Catch e As ArgumentException
            If Err.Number = 5 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "sub test 2 failed"
        '   MS documentation error
        '   reading above 2^14 = 16384 does not throw an exception
        '
        'caughtException = False
        'Try
        '    ' Write more then 16384 text to file.
        '    fn = FreeFile()
        '    FileOpen(fn, strPathName & strFileName, OpenMode.Output)
        '    Print(fn, Space(16384) + "abcd")
        '    FileClose(fn)
        '    'read from the file
        '    fn = FreeFile()
        '    FileOpen(fn, strPathName & strFileName, OpenMode.Input)
        '    str1 = InputString(fn, 16384 + 1) ' 2^14 = 16384
        '    FileClose(fn)
        'Catch e As ArgumentException
        '    If Err.Number = 5 Then
        '        caughtException = True
        '    End If
        'End Try
        'If caughtException = False Then Return "sub test 3 failed"
        '// past end of file
        caughtException = False
        Try
            ' Write text to file.
            fn = FreeFile()
            FileOpen(fn, strPathName & strFileName, OpenMode.Output)
            Print(fn, "abcd")
            FileClose(fn)
            'read from the file
            fn = FreeFile()
            FileOpen(fn, strPathName & strFileName, OpenMode.Input)
            str1 = InputString(fn, 1000)
            FileClose(fn)
        Catch e As EndOfStreamException
            If Err.Number = 62 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "sub test 4 failed"
        Return "success"
    End Function
End Class
