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
        Dim fget As Integer
        Dim str1 As String
        Dim caughtException As Boolean
        
        '// make sure all files are closed
        Microsoft.VisualBasic.FileSystem.Reset()
        '// RecordNumber < 1 and not equal to -1.
        caughtException = False
        Try
            fget = FreeFile()
            FileOpen(fget, System.IO.Directory.GetCurrentDirectory() + "/data/random.txt", OpenMode.Random, , , 22)
            FileGetobject(fget, str1, 0)
        Catch e As ArgumentException
            If Err.Number = 63 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "sub test 1 failed"
        caughtException = False
        Try
            FileGetobject(fget, str1, -2)
        Catch e As ArgumentException
            If Err.Number = 63 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "sub test 2 failed"
        '// file number does not exist
        caughtException = False
        Try
            FileGetobject(256, str1)
        Catch e As IOException
            If Err.Number = 52 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "sub test 3 failed"
        FileClose(fget)
        '// File mode is invalid.
        caughtException = False
        Try
            fget = FreeFile()
            FileOpen(fget, System.IO.Directory.GetCurrentDirectory() + "/data/random.txt", OpenMode.Output, , , 22)
            FileGetobject(fget, str1)
        Catch e As IOException
            If Err.Number = 54 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "sub test 4 failed"
        Return "success"
    End Function
End Class
