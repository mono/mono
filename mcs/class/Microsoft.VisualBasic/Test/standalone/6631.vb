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
        Dim SourceFile As String
        Dim DestinationFile As String
        Dim caughtException As Boolean
        
        '// make sure all files are closed
        Microsoft.VisualBasic.FileSystem.Reset()
        '// create a file for the test
        SourceFile = System.IO.Directory.GetCurrentDirectory() + "/data/textfile.txt"
        DestinationFile = System.IO.Directory.GetCurrentDirectory() + "/data/6631.txt"
        Dim f As System.IO.FileInfo = New System.IO.FileInfo(DestinationFile)
        If (f.Exists) Then
            Kill(DestinationFile)
        End If
        FileCopy(SourceFile, DestinationFile)
        '// Source or Destination is invalid or not specified.
        caughtException = False
        Try
            SourceFile = ""
            DestinationFile = System.IO.Directory.GetCurrentDirectory() + "/data/copy1.txt"
            FileCopy(SourceFile, DestinationFile)
        Catch e As ArgumentException
            If Err.Number = 52 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "sub test 1 failed"
        caughtException = False
        Try
            SourceFile = System.IO.Directory.GetCurrentDirectory() + "/data/textfile.txt"
            DestinationFile = ""
            FileCopy(SourceFile, DestinationFile)
        Catch e As ArgumentException
            If Err.Number = 52 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "sub test 2 failed"
        '// File is already open.
        caughtException = False
        Try
            SourceFile = System.IO.Directory.GetCurrentDirectory() + "/data/textfile.txt"
            DestinationFile = System.IO.Directory.GetCurrentDirectory() + "/data/copy1.txt"
            fn1 = FreeFile()
            FileOpen(fn1, SourceFile, OpenMode.Input)
            FileCopy(SourceFile, DestinationFile)
        Catch e As IOException
            FileClose(fn1)
            If Err.Number = 55 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "sub test 3 failed"
        '// File does not exist.
        '// missing target directory
        caughtException = False
        Try
            SourceFile = System.IO.Directory.GetCurrentDirectory() + "/bad_directory/textfile.txt"
            DestinationFile = System.IO.Directory.GetCurrentDirectory() + "/data/textfile.tmp"
            FileCopy(SourceFile, DestinationFile)
        Catch e As FileNotFoundException
            If Err.Number = 53 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "sub test 4 failed"
        '// missing target directory
        caughtException = False
        Try
            SourceFile = System.IO.Directory.GetCurrentDirectory() + "/data/textfile.txt"
            DestinationFile = System.IO.Directory.GetCurrentDirectory() + "/bad_directory/textfile.tmp"
            FileCopy(SourceFile, DestinationFile)
        Catch e As DirectoryNotFoundException
            If Err.Number = 55 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "sub test 5 failed"
        '// missing source file
        caughtException = False
        Try
            SourceFile = System.IO.Directory.GetCurrentDirectory() + "/data/textfile.mis"
            DestinationFile = System.IO.Directory.GetCurrentDirectory() + "/data/textfile.tmp"
            FileCopy(SourceFile, DestinationFile)
        Catch e As FileNotFoundException
            If Err.Number = 53 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "sub test 6 failed"
        '// not specified target file
        '// The target directory already exists
        caughtException = False
        Try
            SourceFile = System.IO.Directory.GetCurrentDirectory() + "/data/textfile.txt"
            DestinationFile = System.IO.Directory.GetCurrentDirectory() + "/data/"
            FileCopy(SourceFile, DestinationFile)
        Catch e As IOException
            If Err.Number = 55 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "sub test 7 failed"
        Return "success"
    End Function
End Class
