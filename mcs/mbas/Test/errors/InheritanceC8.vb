'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 24
REM ExpectedError: BC30456
REM ErrorMessage: 'b' is not a member of 'P'.

Class P
	Public i as Integer
	Public s as String
	Public l as Long
End Class

Class P1
	Inherits P
	Public b as Byte
End Class

Module M
	Sub Main()
		Dim a as P1 = new P()
		p.b = 10		
	End Sub
End Module


