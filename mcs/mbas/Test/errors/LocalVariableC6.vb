'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 13
REM ExpectedError: BC30247
REM ErrorMessage:  'Shared' is not valid on a local variable declaration.

Module M
	Sub fun()
	 	 Static Dim y as Integer = 10
	 	 Shared Dim y1 as Integer = 10
	end Sub
      Sub Main()		
      End Sub
End Module
