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
        
        '// make sure all files are closed
        Microsoft.VisualBasic.FileSystem.Reset()
        Dim i As Integer
        Dim fn As Integer
        Dim mode As OpenMode
        Dim strFileName As String
        Dim strPathName As String
        strPathName = "data/"
        strFileName = "6581.txt"
        fn = FreeFile()
        FileOpen(fn, strPathName & strFileName, OpenMode.Append)
        FileClose(fn)
        fn = FreeFile()
        FileOpen(fn, strPathName & strFileName, OpenMode.Input)
        mode = FileAttr(fn)
        FileClose(fn)
        If mode <> OpenMode.Input Then Return "failed to get OpenMode.Input"
        fn = FreeFile()
        FileOpen(fn, strPathName & strFileName, OpenMode.Output)
        mode = FileAttr(fn)
        FileClose(fn)
        If mode <> OpenMode.Output Then Return "failed to get OpenMode.Output"
        fn = FreeFile()
        FileOpen(fn, strPathName & strFileName, OpenMode.Random)
        mode = FileAttr(fn)
        FileClose(fn)
        If mode <> OpenMode.Random Then Return "failed to get OpenMode.Random"
        fn = FreeFile()
        FileOpen(fn, strPathName & strFileName, OpenMode.Append)
        mode = FileAttr(fn)
        FileClose(fn)
        If mode <> OpenMode.Append Then Return "failed to get OpenMode.Append"
        fn = FreeFile()
        FileOpen(fn, strPathName & strFileName, OpenMode.Binary)
        mode = FileAttr(fn)
        FileClose(fn)
        If mode <> OpenMode.Binary Then Return "failed to get OpenMode.Binary"
        Return "success"
    End Function
End Class
