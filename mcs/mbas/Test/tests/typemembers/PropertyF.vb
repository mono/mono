'Author:
'   Satya Sudha K (ksathyasudha@novell.com)
'
' (C) 2005 Novell, Inc.
'
' Properties taking arguments, overloading of properties

Imports System
Module M
	Dim a As Integer() = {1,2,3}
	Property Prop (ByVal i As Integer, ByVal j As Integer) As Integer
		Get
			return a(i)
		End Get
		Set
			a (i) = Value
		End Set
	End Property
	Property Prop (ByVal i As Integer) As Integer
		Get
			return a(i)
		End Get
		Set
			a (i) = Value
		End Set
	End Property
	Sub Main ()
		Prop (2) = 10
		if (Prop (2) <> 10)
			Throw new Exception ("A#1 Properties not working with arguments. Expected 10 but got " & Prop (2))
		End If
	End Sub
End Module
