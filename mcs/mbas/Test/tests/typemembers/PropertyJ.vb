'Author:
'   Satya Sudha K (ksathyasudha@novell.com)
'
' (C) 2005 Novell, Inc.
'
' Invoking properties with Named Arguments

Imports System
Module M
	Dim a As Integer() = {1,2,3}
	Property Prop (ByVal j As Long) As Integer
		Get
			return a(j)
		End Get
		Set
			throw new exception ("Should not come here")
			a (j) = Value
		End Set
	End Property
	Property Prop (ByVal i As Integer) As Integer
		Get
			throw new exception ("Should not come here")
			return a(i)
		End Get
		Set
			a (i) = Value
		End Set
	End Property
	Sub Main ()
		Prop (i:=2) = 10
		if (Prop (j:=2) <> 10)
			Throw new Exception ("A#1 Properties not working with arguments. Expected 10 but got " & Prop (2))
		End If
	End Sub
End Module
