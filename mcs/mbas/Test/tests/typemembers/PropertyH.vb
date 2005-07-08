'Author:
'   Satya Sudha K (ksathyasudha@novell.com)
'
' (C) 2005 Novell, Inc.
'
' Properties with types as arrays

Imports System
Module M
	Class C
		Dim a()() As Integer = { new Integer() {1,2,3}, new Integer() {4,5,6}, new Integer() {7,8,9} }
		Property Prop (ByVal i As Integer) As Integer()
			Get
				return a(i)
			End Get
			Set
				a (i) = Value
			End Set
		End Property
	End Class

	Sub Main ()
		Dim c1 As new C()
		c1.Prop (2) = new Integer () {1,2} 
		if (c1.Prop (2).Length <> 2)
			Throw new Exception ("A#1 Properties not working with arguments.")
		End If
		c1.Prop (1)(0) = 7
		if (c1.Prop (1)("0") <> 7)
			Throw new Exception ("A#2 Properties not working with arguments.")
		End If 
	End Sub
End Module
