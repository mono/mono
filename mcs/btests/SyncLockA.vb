REM LineNo: 14
REM ExpectedError: BC30752
REM ErrorMessage: 'On Error' statements are not valid within 'SyncLock' statements.

Imports System

Module SyncLockA

    Class C

        Private Shared count = 0

        Sub IncrementCount()
            Console.WriteLine("Before acquiring lock, Count is {0}", count)
            SyncLock GetType(C)
                System.Threading.Thread.Sleep(1000)
                count += 1
                Console.WriteLine(count)
            End SyncLock
            Console.WriteLine("After releasing lock, Count is {0}", count)
        End Sub

    End Class

    Sub Main()
        Dim c As New C()

        Dim td1 As New System.Threading.Thread( _
                    AddressOf c.IncrementCount)
        td1.Start()

        c.IncrementCount()
    End Sub

End Module