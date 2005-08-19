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
        Dim strOut1 As String
        Dim strOut2 As String
        Dim strIn1 As String
        Dim strIn2 As String
        Dim strFileName As String
        Dim strPathName As String
        
        '// make sure all files are closed
        Microsoft.VisualBasic.FileSystem.Reset()
        strPathName = System.IO.Directory.GetCurrentDirectory() + "/data/"
        strFileName = "6875.txt"
        'if this file exists - kill it
        If (strFileName = Dir(strPathName & strFileName)) Then
            Kill(strPathName & strFileName)
        End If
        strOut1 = "abcdefgh"
        strOut2 = "ijklmnop"
        ' Write text to file.
        fn = FreeFile()
        FileOpen(fn, strPathName & strFileName, OpenMode.Output)
        ' Multiple expressions separated with a comma will be aligned on tab boundaries
        Print(fn, strOut1, strOut2)
        FileClose(fn)
        ' Input text from a file.
        fn = FreeFile()
        FileOpen(fn, strPathName & strFileName, OpenMode.Input)
        Input(fn, strIn1) 'the whole line is read
        FileClose(fn)
        'compare str1 and str2
        If strOut1 & "      " & strOut2 <> strIn1 Then Return "failed"
        Return "success"
    End Function
End Class
