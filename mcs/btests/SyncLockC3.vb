REM LineNo: 13
REM ExpectedError: BC30582
REM ErrorMessage: 'SyncLock' operand cannot be of type 'Boolean' because 'Boolean' is not a reference type.

Imports System

Module SyncLockC3
    Class C
        Private Shared count = 0

        Sub IncrementCount()
            Dim i As Integer
            SyncLock i = 0
                count += 1
                Console.WriteLine(count)
            End SyncLock
        End Sub
    End Class

    Sub Main()
        Dim c As New C()
        c.IncrementCount()
    End Sub

End Module