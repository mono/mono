'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)

Imports System

Structure Somestruct
	Dim a as String
	Const b as integer = 25
End Structure


Module M
	Sub Main()
		dim x as Somestruct

		x.a = 10
		If x.a <> 10 then
			Throw new Exception ("Expected x.a = 10 but got " & x.a)
		End If

		dim y as Somestruct = x

		x.a = 20
		If y.a <> 10 then
			Throw new Exception ("Expected y.a = 10 but got " & y.a)
		End If
	End Sub
End Module
