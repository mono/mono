'Author:
'   Satya Sudha K (ksathyasudha@novell.com)
'
' (C) 2005 Novell, Inc.
'
' Properties as function arguments

Imports System
Module M
	Function F(ByRef p As Integer) as Integer
		p += 1
		return p
	End Function

	Dim a As Integer = 9
	Property Prop As Integer
		Get
			return a
		End Get
		Set
			a = value
		End Set
	End property
   
	Sub Main()
		F(Prop)
		if (Prop <> 10)
			Throw New System.Exception("#A1, Unexcepted Behaviour in Arguments_ByReferenceA.vb")
		end if
	End Sub 
End Module

