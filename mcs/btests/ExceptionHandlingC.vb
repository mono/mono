
Imports System
Imports Microsoft.VisualBasic

Module ExceptionHandlingC

    Sub f1()
        On Error GoTo ErrorHandler
        Dim i As Integer = 0
        i = 1 / i
        Console.WriteLine(i)
        Exit Sub
ErrorHandler:
        Throw New Exception("AA")
        Resume Next
    End Sub

    Sub f2()
        On Error GoTo ErrorHandler
        f1()
        Exit Sub
ErrorHandler:
        If Err.Description <> "AA" Then
            Throw New Exception("#EHC1 - Error statement failed")
        End If
        Resume Next
    End Sub

    Sub f3()
        On Error GoTo ErrorHandler
        Throw New DivideByZeroException()
        Exit Sub
ErrorHandler:
        If Not TypeOf Err.GetException Is DivideByZeroException Then
            Throw New Exception("#EHC2 - Error statement failed")
        End If
        Resume Next
    End Sub

    Sub f4()
        On Error GoTo ErrorHandler
        Dim i As Integer = 0
        i = 5 / i
        On Error GoTo 0
        If i <> 1 Then
            Throw New Exception("#EHC3 - Error Statement failed")
        End If
        Exit Sub
ErrorHandler:
        i = 5
        Resume   ' Execution resumes with the statement that caused the error
    End Sub

    Sub f5()
        On Error GoTo ErrorHandler
        Error 6    ' Overflow Exception
        Exit Sub
ErrorHandler:
        If Err.Number <> 6 Then
            Throw New Exception("#EHC4 - Error Statement failed")
        End If
        Resume Next
    End Sub

    Sub f6()
        On Error GoTo ErrorHandler
        Dim i As Integer = 0, j As Integer
        i = 1 / i

        On Error GoTo 0  ' Disable error handler
        On Error Resume Next

        i = 0
        i = 1 / i ' create error 
        If Err.Number = 6 Then  ' handle error
            Err.Clear()
        Else
            Throw New Exception("#EHC5 - Error Statement failed")
        End If

        i = 1 / i
        On Error GoTo -1
        If Err.Number <> 0 Then
            Throw New Exception("#EHC6 - Error Statement failed")
        End If

        Exit Sub
ErrorHandler:
        Select Case Err.Number
            Case 6
                i = 1
            Case Else
                Throw New Exception("#EHC7 - Error Statement failed")
        End Select
        Resume
    End Sub

    Sub Main()
        f2() : f3() : f4() : f5() : f6()
    End Sub

End Module

