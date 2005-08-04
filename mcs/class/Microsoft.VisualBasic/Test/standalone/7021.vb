
'
' Author:
'   Sathya Sudha (ksathyasudha@novell.com)
' Copyright (C) 2004 Novell, Inc (http://www.novell.com)
'
' Permission is hereby granted, free of charge, to any person obtaining
' a copy of this software and associated documentation files (the
' "Software"), to deal in the Software without restriction, including
' without limitation the rights to use, copy, modify, merge, publish,
' distribute, sublicense, and/or sell copies of the Software, and to
' permit persons to whom the Software is furnished to do so, subject to
' the following conditions:
' 
' The above copyright notice and this permission notice shall be
' included in all copies or substantial portions of the Software.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
' EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
' MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
' NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
' LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
' OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
' WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
'
Imports System
Imports System.IO
Imports Microsoft.VisualBasic
Public Class TestClass
    Public Function Test() As Integer
        Dim fput As Integer
        Dim item1 As Short = 0
        Dim item2 As Integer =1 
        Dim item3 As Single = 2
        Dim item4 As Double =3
        Dim item5 As Decimal = 400
        Dim item6 As Byte = 5
        Dim item7 As Boolean = True
        Dim item8 As Date = #3/4/2003#
        Dim item9 As String = "sudha"
        Dim caughtException As Boolean
        Dim strFileName As String
        Dim strPathName As String
        
        '// make sure all files are closed
        Microsoft.VisualBasic.FileSystem.Reset()
        strPathName = System.IO.Directory.GetCurrentDirectory() 
        strFileName = "/6748.txt"
	System.Console.WriteLine(strPathName & strFileName)
        'if this file exists - kill it
        If (strFileName = Dir(strPathName & strFileName)) Then
            Kill(strPathName & strFileName)
        End If
        '// RecordNumber < 1 and not equal to -1.
        caughtException = False
            fput = FreeFile()
            FileOpen(fput, strPathName & strFileName, OpenMode.Random, , ,22) 
            FilePut(fput, item1, 1)
            FilePut(fput, item2, 2)
            FilePut(fput, item3, 3)
            FilePut(fput, item4, 4)
            FilePut(fput, item5, 5)
            FilePut(fput, item6, 6)
            FilePut(fput, item7, 7)
            FilePut(fput, item8, 8)
            FilePut(fput, item9, 9)
	FileClose(fput)
        Return 0
    End Function
End Class
