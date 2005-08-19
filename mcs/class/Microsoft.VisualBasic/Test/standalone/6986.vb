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
        Dim strIn As String
        Dim strBuffer As String
        Dim strFileName As String
        Dim strPathName As String
        
        '// make sure all files are closed
        Microsoft.VisualBasic.FileSystem.Reset()
        '// create a file for the test
        Dim SourceFile As String
        Dim DestinationFile As String
        SourceFile = System.IO.Directory.GetCurrentDirectory() + "/data/textfile.txt"
        DestinationFile = System.IO.Directory.GetCurrentDirectory() + "/data/6986.txt"
        Dim f As System.IO.FileInfo = New System.IO.FileInfo(DestinationFile)
        If (f.Exists) Then
            Kill(DestinationFile)
        End If
        FileCopy(SourceFile, DestinationFile)
        strPathName = System.IO.Directory.GetCurrentDirectory() + "/data/"
        strFileName = "6986.txt"
        'if this file exists - kill it
        If (strFileName = Dir(strPathName & strFileName)) Then
            Kill(strPathName & strFileName)
        End If
        ' Write text to file.
        fn = FreeFile()
        FileOpen(fn, strPathName & strFileName, OpenMode.Output)
        PrintLine(fn, ",") ' Delimiting comma
        PrintLine(fn, "") ' blank line
        PrintLine(fn, "#NULL#") ' DBNull
        PrintLine(fn, "#TRUE#") ' True
        PrintLine(fn, "#FALSE#") ' False
        '   #yyyy-mm-dd hh:mm:ss#
        '   The date and/or time represented by the expression
        PrintLine(fn, "#1931-12-30 12:59:59#")
        PrintLine(fn, "#2000-01-01 12:00:00#")
        '   #ERROR errornumber#
        '   errornumber (variable is an object tagged as an error)
        PrintLine(fn, "#ERROR 52#")
        FileClose(fn)
        ' Input text from a file.
        fn = FreeFile()
        FileOpen(fn, strPathName & strFileName, OpenMode.Binary)
        Input(fn, strIn)
        strBuffer = strBuffer & strIn
        Input(fn, strIn)
        strBuffer = strBuffer & strIn
        Input(fn, strIn)
        strBuffer = strBuffer & strIn
        Input(fn, strIn)
        strBuffer = strBuffer & strIn
        Input(fn, strIn)
        strBuffer = strBuffer & strIn
        Input(fn, strIn)
        strBuffer = strBuffer & strIn
        FileClose(fn)
        Return strBuffer
    End Function
End Class
