' Author:
'   Maverson Eduardo Schulze Rosa (maverson@gmail.com)
'
' GrupoTIC - UFPR - Federal University of Paraná

Imports System

Module M
	Class C
		public b As Byte
	end class
	
	Sub Main()
		Dim o As Object = new C
		o.b = 0

                if o.b <> 0 then
                        throw new System.Exception("LateBinding Not Working")
                end if
	end sub
end module
