REM LineNo: 15
REM ExpectedError: BC30755
REM ErrorMessage: 'Goto label' is not valid because 'label' is inside a 'SyncLock' 
REM               statement that does not contain this statement.

Imports System

Module SyncLockC1
    Class C

        Private Shared count = 0

        Sub IncrementCount()
            Console.WriteLine(count)
            GoTo label
            SyncLock GetType(C)
label:
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