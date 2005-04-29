Imports System

Class C
	private i as integer
	public Property p() as Integer
		GET
			return i
		END GET

		SET (ByVal val as Integer)
			i = val
		End SET
	End Property
End Class

Module M
	Sub Main()
		dim o as Object = new C()
		o.p = 10
		if o.p<> 10 then
			throw new System.Exception("#A1 Latebinding not working")
		End if
	End Sub
End Module
