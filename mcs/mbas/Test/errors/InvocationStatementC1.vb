'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 24
REM ExpectedError: BC30451
REM ErrorMessage:  Name 'fun1' is not declared.

Imports A

Namespace A
  public Module Test
    Public Function fun()
    End Function
    Private Function fun1()
    End Function
  End module
End Namespace

Module SyncLockB
	Sub Main()
		Call fun()  'This is correct 		
		Call fun1() 'This is wrong
	End Sub
End Module
