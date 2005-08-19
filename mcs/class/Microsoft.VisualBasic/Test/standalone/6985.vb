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
Imports System
Public Class TestClass
    Public Function Test() As String
        Dim fn As Integer
        Dim strOut As String
        Dim strFileName As String
        Dim strPathName As String
        
        '// make sure all files are closed
        Microsoft.VisualBasic.FileSystem.Reset()
        strPathName = System.IO.Directory.GetCurrentDirectory() + "/data/"
        strFileName = "6985.txt"
        'if this file exists - kill it
        If (strFileName = Dir(strPathName & strFileName)) Then
            Kill(strPathName & strFileName)
        End If
        ' Write text to file.
        fn = FreeFile()
        FileOpen(fn, strPathName & strFileName, OpenMode.Output)
        PrintLine(fn, ",") ' Delimiting comma
        PrintLine(fn, "")  ' blank line
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
        Dim strIn As String
        Dim objIn As Object
        Dim b1 As Boolean
        Dim b2 As Boolean
        Dim d1 As Date
        Dim d2 As Date
        Dim ierr1 As Integer
        fn = FreeFile()
        FileOpen(fn, strPathName & strFileName, OpenMode.Input)
        Input(fn, strIn)
        If strIn <> "" Then Return "failed to input Delimiting comma"
        Input(fn, strIn)
        If strIn <> "" Then Return "failed to input blank line"
        Input(fn, strIn)
        If strIn <> "" Then Return "failed to input blank line"
        Input(fn, objIn)
        If Not TypeOf (objIn) Is DBNull Then Return "failed to input DBNull"
        Input(fn, b1)
        If b1 <> True Then Return "failed to input Boolean"
        Input(fn, b2)
        If b2 <> False Then Return "failed to input Boolean"
        Input(fn, d1)
        If d1 <> CDate("#1931-12-30 12:59:59#") Then Return "failed to input first Date"
        Input(fn, d2)
        If d2 <> CDate("#2000-01-01 12:00:00#") Then Return "failed to input second Date"
        Input(fn, objIn)
        If objIn <> 52 Then Return "failed to input Error Number"
        FileClose(fn)
        Return "success"
    End Function
End Class
