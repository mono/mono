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
    Public Function Test() As Long
        Dim fn As Integer
        Dim location As Long
        Dim wSum As Long
        Dim oneLine As String
        Dim oneChar As Char
        
        '// make sure all files are closed
        Microsoft.VisualBasic.FileSystem.Reset()
        'create and write into file
        fn = FreeFile()
        FileOpen(fn, System.IO.Directory.GetCurrentDirectory() + "/data/6928.txt", OpenMode.Output)
        WriteLine(fn, "abcdefg")
        WriteLine(fn, "abcdefg")
        FileClose(fn)
        'read from file and check the location
        FileOpen(fn, System.IO.Directory.GetCurrentDirectory() + "/data/6928.txt", OpenMode.Binary)
        location = 0
        While location < LOF(fn)
            FileGet(fn, oneChar)
            location = Loc(fn)
            wSum = wSum + location
        End While
        FileClose(fn)
        Return wSum
    End Function
End Class
