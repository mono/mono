'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 20
REM ExpectedError: BC30456
REM ErrorMessage: 'a1' is not a member of 'A'.

class A
	public dim i as integer
End class

Module gotostmt
	Sub Main()
		Dim a as A = new A()
		With a
			goto .a1
				.i = .i + 1
			.a1:
				.i = .i + 1
			if .i <> 1 then
				Throw new System.Exception("Goto statement not working properly ")
			End If
		End With
 	End Sub
End Module
