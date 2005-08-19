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
        Dim str1 As String
        Dim strFileName As String
        Dim strPathName As String
        Dim i As Integer
        strPathName = System.IO.Directory.GetCurrentDirectory() + "/data/"
        strFileName = "hidden.txt"
        'test all enums
        For i = 0 To 64
            str1 = Dir(System.IO.Directory.GetCurrentDirectory() + "/data/*.ini", i)
            If str1 = "" Then Return i.ToString
        Next i
        'check if directory has Archive files
        'str1 = Dir(strPathName , FileAttribute.Archive)
        'If str1 = "" Then Return "failed to find an archive file"
        ''check if this file exists
        'If (str1 <> Dir(strPathName & str1)) Then Return "failed to locate an archive file"
        'check if directory has Hidden files
        str1 = Dir(strPathName, FileAttribute.Hidden)
        If str1 = "" Then Return "failed to find a hidden file"
        'check if this file exists
        If (str1 <> Dir(strPathName & str1, FileAttribute.Hidden)) Then Return "failed to locate a hidden file"
        'check if directory has ReadOnly files
        str1 = Dir(strPathName, FileAttribute.ReadOnly)
        If str1 = "" Then Return "failed to find a readOnly file"
        'check if this file exists
        If (str1 <> Dir(strPathName & str1, FileAttribute.ReadOnly)) Then Return "failed to locate a ReadOnly file"
        'check if directory has System files
        str1 = Dir(strPathName, FileAttribute.System)
        If str1 = "" Then Return "failed to find a system file"
        'check if this file exists
        If (str1 <> Dir(strPathName & str1, FileAttribute.System)) Then Return "failed to locate a system file"
        Return "success"
    End Function
End Class
