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
Imports System.IO
Imports Microsoft.VisualBasic
Public Class TestClass
    Public Function Test() As String
        Dim fn As Integer
        Dim caughtException As Boolean
        
        '// make sure all files are closed
        Microsoft.VisualBasic.FileSystem.Reset()
        '// file number does not exist.
        caughtException = False
        Try
            fn = FreeFile()
            FileOpen(fn, System.IO.Directory.GetCurrentDirectory() + "/data/6796.txt", OpenMode.Output)
            FileWidth(256, 5)
        Catch e As IOException
            If Err.Number = 52 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "sub test 1 failed"
        FileClose(fn)
        '// File mode is invalid.
        'caughtException = False
        'Try
        'Catch e As IOException
        '   If err.Number = 54 then
        '       caughtException = True
        '   End If
        'End Try
        'If caughtException = False Then return "sub test 2 failed"
        '// Numeric expression in the range 0–255
        caughtException = False
        Try
            fn = FreeFile()
            FileOpen(fn, System.IO.Directory.GetCurrentDirectory() + "/data/6797.txt", OpenMode.Output)
            FileWidth(fn, -1)
        Catch e As ArgumentException
            If Err.Number = 5 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "sub test 3 failed"
        FileClose(fn)
        caughtException = False
        Try
            fn = FreeFile()
            FileOpen(fn, System.IO.Directory.GetCurrentDirectory() + "/data/6797.txt", OpenMode.Output)
            FileWidth(fn, 256)
        Catch e As ArgumentException
            If Err.Number = 5 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "sub test 4 failed"
        FileClose(fn)
        Return "success"
    End Function
End Class
