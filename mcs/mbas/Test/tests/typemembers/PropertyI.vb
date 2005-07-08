'Author:
'   Satya Sudha K (ksathyasudha@novell.com)
'
' (C) 2005 Novell, Inc.
'
' Properties taking/returning types containing default properties

Imports System
Module M
	Class C
		Dim a As Integer () = {1,2,3,4,5}
		Default Property Item(ByVal i As Integer)
			Get
				return a(i)
			End Get
			Set
				a (i) = Value
			End Set
		End Property
	End Class
	Class C1
		Dim inst As new C()
		public sub New()
			inst = new C()
		End Sub
		Public Property Prop As C
			Get
				return inst
			End Get
			Set
				inst = Value
			End Set
		End Property
	End Class
	Sub Main ()
		Dim a As new C1()
		a.Prop(1) = 3
		if (a.Prop (1) <> 3)
			throw new Exception ("A#1 Property not working")
		End if
	End Sub
End Module
