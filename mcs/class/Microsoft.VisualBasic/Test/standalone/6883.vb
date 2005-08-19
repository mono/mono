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
        Dim strPathName As String
        Dim OldName As String
        Dim NewName As String
        
        '// make sure all files are closed
        Microsoft.VisualBasic.FileSystem.Reset()
        strPathName = System.IO.Directory.GetCurrentDirectory() + "/data/"
        OldName = "rename.txt"
        NewName = "rename2.txt"
        ' create the file
        fn = FreeFile()
        FileOpen(fn, strPathName & OldName, OpenMode.Output)
        FileClose(fn)
        'if the target name file exists - kill it
        If (NewName = Dir(strPathName & NewName)) Then
            Kill(strPathName & NewName)
        End If
        ' Rename file.
        Rename(strPathName & OldName, strPathName & NewName)
        'if new file exists 
        If (NewName <> Dir(strPathName & NewName)) Then
            Return "failed"
        End If
        Return "success"
    End Function
End Class
