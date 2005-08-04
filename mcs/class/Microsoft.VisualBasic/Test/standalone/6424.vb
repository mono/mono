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
Imports System
Imports Microsoft.VisualBasic
Public Class TestClass
    Public Function Test() As String
        Dim s1 As String = "a"
        Dim s2 As String = "b"
        Dim s3 As String = "c"
        Dim s4 As String = "d"
        Dim col As New Microsoft.VisualBasic.Collection()
        Dim caughtException As Boolean
        '// Index doesn't match an existing member of the collection.
        '// nothing in Collection yet
        caughtException = False
        Try
            Dim o As Object = col(0)
        Catch e As IndexOutOfRangeException
            If Err.Number = 9 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "failed at sub test 1"
        col.Add("Baseball", "Base", Nothing, Nothing)
        col.Add("Football", "Foot", Nothing, Nothing)
        col.Add("Basketball", "Basket", Nothing, Nothing)
        col.Add("Volleyball", "Volley", Nothing, Nothing)
        '// only 4 elements
        caughtException = False
        Try
            Dim o As Object = col(5)
        Catch e As IndexOutOfRangeException
            If Err.Number = 9 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "failed at sub test 2"
        '// Collection class is 1-based
        caughtException = False
        Try
            Dim o As Object = col(0)
        Catch e As IndexOutOfRangeException
            If Err.Number = 9 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "failed at sub test 3"
        '// argument does not refer to an existing member of the collection
        '// no member with Key == "Kick"
        caughtException = False
        Try
            Dim o As Object = col("Kick")
        Catch e As ArgumentException
            '// FIXME
            '// VB Language Reference says IndexOutOfRangeException 
            '// here, but MS throws ArgumentException
            '// AssertEquals("#E07", typeof(IndexOutOfRangeException), e.GetType())
            If Err.Number = 5 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "failed at sub test 4"
        '// Both Before and After are specified
        '// can't specify both Before and After
        caughtException = False
        Try
            col.Add("Kickball", "Kick", "Volley", "Foot")
        Catch e As ArgumentException
            If Err.Number = 5 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "failed at sub test 5"
        '// The specified Key already exists
        '// Key "Foot" already exists
        caughtException = False
        Try
            col.Add("Kickball", "Foot", Nothing, Nothing)
        Catch e As ArgumentException
            If Err.Number = 457 Then    'MS document err.number 5
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "failed at sub test 6"
        '// Index doesn't match an existing member of the collection
        '// no Key "Golf" exists
        caughtException = False
        Try
            col.Remove("Golf")
        Catch e As ArgumentException
            If Err.Number = 5 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "failed at sub test 7"
        '// no Index 10 exists
        caughtException = False
        Try
            col.Remove(10)
        Catch e As IndexOutOfRangeException
            If Err.Number = 9 Then
                caughtException = True
            End If
        End Try
        If caughtException = False Then Return "failed at sub test 8"
        Return "success"
    End Function
End Class
