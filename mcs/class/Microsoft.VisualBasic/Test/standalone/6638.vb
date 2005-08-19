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
        Dim i As Integer
        Dim fn As Integer
        Dim strFileName As String
        Dim strPathName As String
        Dim str1 As String
        
        '// make sure all files are closed
        Microsoft.VisualBasic.FileSystem.Reset()
        strPathName = System.IO.Directory.GetCurrentDirectory() + "/data/"
        strFileName = "6638.txt"
        'if this file exists - kill it
        If (strFileName = Dir(strPathName & strFileName)) Then
            Kill(strPathName & strFileName)
        End If
        fn = FreeFile()
        FileOpen(fn, strPathName & strFileName, OpenMode.Output)
        FileWidth(fn, 5)   ' Set output line width to 5.
        For i = 0 To 9   ' Loop 10 times.
            Print(fn, Chr(48 + i))   ' Prints five characters per line.
        Next
        FileClose(fn)
        'now, read the file to see if FileWidth was correct
        fn = FreeFile()
        FileOpen(fn, strPathName & strFileName, OpenMode.Input)
        'read 2 lines
        str1 = LineInput(fn)
        If str1 <> "01234" Then Return "failed"
        str1 = LineInput(fn)
        If str1 <> "56789" Then Return "failed"
        FileClose(fn)
        'if this file exists - kill it
        If (strFileName = Dir(strPathName & strFileName)) Then
            Kill(strPathName & strFileName)
        End If
        Return "success"
    End Function
End Class
