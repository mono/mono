REM LineNo: 14
REM ExpectedError: BC30752
REM ErrorMessage: 'On Error' statements are not valid within 'SyncLock' statements.

Imports System

Module SyncLockC2

    Class C

        Sub f4()
            Dim i As Integer = 0
            SyncLock GetType(C)
                On Error GoTo ErrorHandler
                i = 5 / i
            End SyncLock
            Exit Sub
ErrorHandler:
            i = 5
            Resume   ' Execution resumes with the statement that caused the error
        End Sub

    End Class

    Sub Main()
        Dim c As New C()
        c.f4()
    End Sub

End Module