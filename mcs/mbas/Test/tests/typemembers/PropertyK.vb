'Author:
'   Satya Sudha K (ksathyasudha@novell.com)
'
' (C) 2005 Novell, Inc.
'
' Two overloaded and applicable properties 

Imports System
Module M
	Class C
		Dim a() As Integer = {7, 8, 9}
		Property Prop  As Integer()
			Get
				throw new Exception ("Should not come here")
			End Get
			Set
				throw new Exception ("Should not come here")
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
	End Class

	Sub Main ()
		Dim c1 As new C()
		c1.Prop (2) = 1 
		if (c1.Prop (2) <> 1)
			Throw new Exception ("A#1 Properties not working with arguments.")
		End If
	End Sub
End Module
