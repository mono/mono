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
        Dim w As Long
        
        '// make sure all files are closed
        Microsoft.VisualBasic.FileSystem.Reset()
        '// create a file for the test
        Dim SourceFile As String
        Dim DestinationFile As String
        SourceFile = System.IO.Directory.GetCurrentDirectory() + "/data/textfile.txt"
        DestinationFile = System.IO.Directory.GetCurrentDirectory() + "/data/6615.txt"
        Dim f As System.IO.FileInfo = New System.IO.FileInfo(DestinationFile)
        If (f.Exists) Then
            Kill(DestinationFile)
        End If
        FileCopy(SourceFile, DestinationFile)
        fn = FreeFile()
        FileOpen(fn, System.IO.Directory.GetCurrentDirectory() + "/data/6615.txt", OpenMode.Input)
        w = LOF(fn)  ' Get length of file.
        FileClose(fn)
        Return "success"
    End Function
End Class
