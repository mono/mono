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
        ' Produce overflow error
        On Error Resume Next
        Dim zero As Integer = 0
        Dim result As Integer = 8 / zero
        Dim ex As SystemException
        Err.Clear()
        Err.Clear()
        If Err.Description <> "" Then Return "faild to clear Err.Description"
        If Err.Erl <> 0 Then Return "faild to clear Err.Erl"
        ex = Err.GetException
        If Not ex Is Nothing Then Return "faild to clear Err.Erl"
        If Err.GetType().ToString <> "Microsoft.VisualBasic.ErrObject" Then Return "faild to clear Err.GetType"
        If Err.HelpContext <> 0 Then Return "faild to clear Err.HelpContext"
        'If Err.LastDllError <> 0 Then Return "faild to clear Err.LastDllError"
        If Err.Number <> 0 Then Return "faild to clear Err.Number"
        If Err.Source <> "" Then Return "faild to clear Err.Source"
        Return "success"
    End Function
End Class
