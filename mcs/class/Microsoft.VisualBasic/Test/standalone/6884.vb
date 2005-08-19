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
        Dim strPathName1 As String
        Dim strPathName2 As String
        Dim fn As Integer
        
        '// make sure all files are closed
        Microsoft.VisualBasic.FileSystem.Reset()
        strPathName1 = System.IO.Directory.GetCurrentDirectory() + "/data/6884/"
        'make directory 
        Try
             Try
                 RmDir(strPathName1)
              Catch
        End Try
            MkDir(strPathName1)
        Catch
        End Try
        
        'put a file in it
        fn = FreeFile()
        FileOpen(fn, strPathName1 & "6884.txt", OpenMode.Output)
        FileClose(fn)
        'if file exists 
        If ("6884.txt" <> Dir(strPathName1 & "6884.txt")) Then
            Return "failed"
        End If
        Kill(strPathName1 & "6884.txt")
        RmDir(strPathName1)
        Return "success"
    End Function
End Class
