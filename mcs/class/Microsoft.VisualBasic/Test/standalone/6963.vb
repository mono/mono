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
        Dim c1 As Char
        Dim cVBCr As Char
        Dim cVBLf As Char
        Dim strFileName As String
        Dim strPathName As String
        Dim str1 As String
        Dim str2 As String
        
        '// make sure all files are closed
        Microsoft.VisualBasic.FileSystem.Reset()
        strPathName = System.IO.Directory.GetCurrentDirectory() + "/data/"
        strFileName = "6963.txt"
        'if this file exists - kill it
        If (strFileName = Dir(strPathName & strFileName)) Then
            Kill(strPathName & strFileName)
        End If
        ' Write text to file.
        fn = FreeFile()
        FileOpen(fn, strPathName & strFileName, OpenMode.Output)
        PrintLine(fn, "1234", "", "abcd")
        FileClose(fn)
        ' Input binary text from a file.
        fn = FreeFile()
        FileOpen(fn, strPathName & strFileName, OpenMode.Binary)
        FileGet(fn, c1) 'read the first string
        str1 = str1 & c1
        FileGet(fn, c1)
        str1 = str1 & c1
        FileGet(fn, c1)
        str1 = str1 & c1
        FileGet(fn, c1)
        str1 = str1 & c1
        FileGet(fn, cVBCr) 'read the Carridge
        FileGet(fn, cVBLf) 'read the line feed
        'omit output
        FileGet(fn, cVBCr) 'read the Carridge
        FileGet(fn, cVBLf) 'read the line feed
        FileGet(fn, c1) 'read the second string
        str2 = str2 & c1
        FileGet(fn, c1)
        str2 = str2 & c1
        FileGet(fn, c1)
        str2 = str2 & c1
        FileGet(fn, c1)
        str2 = str2 & c1
        FileClose(fn)
        Return "failed to understand the documentation" ' rafi ;(
        Return "success"
    End Function
End Class
