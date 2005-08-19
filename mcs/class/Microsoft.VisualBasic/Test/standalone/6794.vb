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
        Dim fn1 As Integer
        Dim fn2 As Integer
        Dim caughtException As Boolean
        
        '// make sure all files are closed
        Microsoft.VisualBasic.FileSystem.Reset()
        '// create the file
        fn1 = FreeFile()
        FileOpen(fn1, System.IO.Directory.GetCurrentDirectory() + "/data/6794.txt", OpenMode.Append)
        FileClose(fn1)
        '// Invalid Access, Share, or Mode
        'caughtException = False
        'Try
        'Catch e As ArgumentException
        '   If Err.Number = 5 Then
        '       caughtException = True
        '   End If
        'End Try
        'If caughtException = False Then return "sub test 1 failed"
        '// WriteOnly file is opened for Input
        caughtException = False
        Try
            fn1 = FreeFile()
            FileOpen(fn1, System.IO.Directory.GetCurrentDirectory() + "/data/6794.txt", OpenMode.Input, OpenAccess.Write)
        Catch e As ArgumentException
            If Err.Number = 5 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "sub test 2 failed"
        '// ReadOnly file is opened for Output
        caughtException = False
        Try
            fn1 = FreeFile()
            FileOpen(fn1, System.IO.Directory.GetCurrentDirectory() + "/data/6794.txt", OpenMode.Output, OpenAccess.Read)
        Catch e As ArgumentException
            If Err.Number = 5 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "sub test 3 failed"
        '// ReadOnly file is opened for Append
        caughtException = False
        Try
            fn1 = FreeFile()
            FileOpen(fn1, System.IO.Directory.GetCurrentDirectory() + "/data/6794.txt", OpenMode.Append, OpenAccess.Read)
        Catch e As ArgumentException
            If Err.Number = 5 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "sub test 4 failed"
        '// Record length is negative (and not equal to -1).
        caughtException = False
        Try
            fn1 = FreeFile()
            FileOpen(fn1, System.IO.Directory.GetCurrentDirectory() + "/data/6794.txt", OpenMode.Random, , , 0)
        Catch e As ArgumentException
            If Err.Number = 5 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "sub test 5 failed"
        caughtException = False
        Try
            fn1 = FreeFile()
            FileOpen(fn1, System.IO.Directory.GetCurrentDirectory() + "/data/6794.txt", OpenMode.Random, , , -2)
        Catch e As ArgumentException
            If Err.Number = 5 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "sub test 6 failed"
        '// file number is invalid (<-1 or >255), or file number is already in use.
        caughtException = False
        Try
            fn1 = FreeFile()
            FileOpen(256, System.IO.Directory.GetCurrentDirectory() + "/data/6794.txt", OpenMode.Random)
        Catch e As IOException
            If Err.Number = 52 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "sub test 7 failed"
        '// FileName is already open, or FileName is invalid.
        caughtException = False
        Try
            fn1 = FreeFile()
            FileOpen(fn1, System.IO.Directory.GetCurrentDirectory() + "/data/6794.txt", OpenMode.Random)
            FileOpen(fn1, System.IO.Directory.GetCurrentDirectory() + "/data/6794.txt", OpenMode.Random)
        Catch e As IOException
            If Err.Number = 55 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "sub test 9 failed"
        '// or FileName is invalid.
        caughtException = False
        Try
            fn1 = FreeFile()
            FileOpen(fn1, System.IO.Directory.GetCurrentDirectory() + "/data/notfound.txt", OpenMode.Input)
        Catch e As IOException
            If Err.Number = 53 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "sub test 10 failed"
        Return "success"
    End Function
End Class
