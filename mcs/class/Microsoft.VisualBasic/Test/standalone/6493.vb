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
Imports Microsoft.VisualBasic.FileSystem
Public Class TestClass
    Public Function Test() As String
        Dim str1 As String
        ' wildcards character * and ?
        '
        'check if directory has files
        str1 = Dir(System.IO.Directory.GetCurrentDirectory() + "/data/*.txt")
        If str1 = "" Then Return "failed to check if directory has files"
        'make sure that this file exists
        If (str1 <> Dir(System.IO.Directory.GetCurrentDirectory() + "/data/" & str1)) Then Return "failed at sub test 1"
        'check if directory has files with any extension
        str1 = Dir(System.IO.Directory.GetCurrentDirectory() + "/data/*.???")
        If str1 = "" Then Return "failed to check if directory has files with any extension"
        'make sure that this file exists
        If (str1 <> Dir(System.IO.Directory.GetCurrentDirectory() + "/data/" & str1)) Then Return "failed at sub test 2"
        'check if directory has files with any extension
        str1 = Dir(System.IO.Directory.GetCurrentDirectory() + "/data/*.?x?")
        If str1 = "" Then Return "failed to check if directory has files with any extension"
        'make sure that this file exists
        If (str1 <> Dir(System.IO.Directory.GetCurrentDirectory() + "/data/" & str1)) Then Return "failed at sub test 3"
        'check if directory has files that start with w and whatever
        str1 = Dir(System.IO.Directory.GetCurrentDirectory() + "/data/t*.?x?")
        If str1 = "" Then Return "failed to check if directory has files that start with"
        'make sure that this file exists
        If (str1 <> Dir(System.IO.Directory.GetCurrentDirectory() + "/data/" & str1)) Then Return "failed at sub test 4"
        Return "success"
    End Function
End Class
