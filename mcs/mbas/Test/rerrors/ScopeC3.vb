'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 23
REM ExpectedError: BC30469
REM ErrorMessage: Reference to a non-shared member requires an object reference.

REM LineNo: 24
REM ExpectedError: BC30469
REM ErrorMessage: Reference to a non-shared member requires an object reference.

Class A
	Sub fun(i as Integer)
		System.Console.WriteLine("Outer Integer {0}",i)
	End Sub
	Sub fun(i as String)
		System.Console.WriteLine("Outer String {0}",i)
	End Sub
	Class AB
		Sub gun()
			fun(1)		'Only Shared functions can be accessed by inner class... error
			fun("Hello")	'Only Shared functions can be accessed by inner class... error
		End Sub		
	End Class
End Class

Module ScopeA
	Sub Main()
		Dim a as A.AB=new A.AB()
		a.gun()
	End Sub
End Module
