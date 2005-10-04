MCS=mcs
sources=src/AssemblyInfo.cs src/Complex.cs src/ComplexArray.cs src/ComplexF.cs src/ComplexMath.cs src/ComplexStats.cs src/Fourier.cs src/FourierDirection.cs

.SUFFIXES: .cs .exe .dll

all: Exocortex.DSP.dll

Exocortex.DSP.dll: $(sources)
	$(MCS) -t:library $(sources) -out:Exocortex.DSP.dll

