' Unhandled Exception: System.ArgumentException: 'SyncLock' operand cannot 
' be of type 'Boolean' because 'Boolean' is not a reference type.

Imports System

Module SyncLock1
    Class C
        Private Shared count = 0

        Sub IncrementCount()
            SyncLock count = 0
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