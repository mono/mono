Module ExpConversionInttoBool
	Sub Main()
		Dim a As Int = -4
		Dim b as Boolean
		b = CBool(a)
		if b <> True
			Throw new System.Exception("Int to Boolean Conversion is not working properly. Expected True but got " &b)
		End if	
	End Sub
End Module
