'Unhandled Exception: System.ArgumentException: 'SyncLock' operand cannot be of type
'Boolean' because 'Boolean' is not a reference type.

Option Explicit
Imports System
Imports Nunit.Framework

<TestFixture> _
Public Class SyncLock1
    Class C
        Private Shared count = 0
        Sub IncrementCount()
            SyncLock count = 0
                count += 1
            End SyncLock
        End Sub
    End Class
                <Test, ExpectedException (GetType (System.ArgumentException))> _
                Public Sub TestForArgumentException ()
        	Dim c As New C()
        	c.IncrementCount()
                End Sub
End Class
