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
        On Error GoTo Handle_Exception
        
        '// make sure all files are closed
        Microsoft.VisualBasic.FileSystem.Reset()
        '// Target file(s) open.
        caughtException = False
        fn = FreeFile()
        FileOpen(fn, System.IO.Directory.GetCurrentDirectory() + "/data/6829.txt", OpenMode.Output)
        Kill(System.IO.Directory.GetCurrentDirectory() + "/data/6829.txt")
        If caughtException = False Then Return "sub test 1 failed"
        FileClose(fn)
        '// Target file(s) missing.
        caughtException = False
        Kill(System.IO.Directory.GetCurrentDirectory() + "/data/notfound.txt")
        If caughtException = False Then Return "sub test 2 failed"
        Return "success"
Handle_Exception:
        Select Case Err.Number
            Case 55 'Target file(s) open.
                caughtException = True
                Resume Next
            Case 53 'Target file(s) missing.
                caughtException = 2
                Resume Next
        End Select
    End Function
End Class
