'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 21
REM ExpectedError: BC30574
REM ErrorMessage:  Option Strict On disallows late binding.

Option Strict

Class C
	  Sub fun(ByRef a as Integer)	
	  End Sub
End Class

Module M
        Sub Main()
		   Dim a as Object = 10	
		   Dim o as Object = new C()	
		   o.fun(a)               		
        End Sub
End Module
