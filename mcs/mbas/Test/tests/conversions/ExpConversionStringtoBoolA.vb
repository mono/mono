'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ExpConversionStringtoBooleanA
	Sub Main()
			Dim a as Boolean
			Dim b as String= "True"
			a = CBool(b)
			if a <> True
				Throw new System.Exception("Conversion of String to Boolean not working. Expected True but got " &a) 
			End if		
	End Sub
End Module
