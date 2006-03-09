# Makefile for C5 book examples

USERGUIDEFILES= Anagrams.cs Antipatterns.cs CollectionSanity.cs EventPatterns.cs \
	Fileindex.cs GConvexHull.cs GNfaToDfa.cs GettingStarted.cs \
	Graph.cs Graphcopy.cs HashCodes.cs Jobqueue.cs \
	KeywordRecognition.cs ListPatterns.cs ListPatterns.cs \
	Locking.cs MultiDictionary.cs PointLocation.cs \
	RandomSelection.cs ReadOnlyPatterns.cs Sets.cs \
	SortedIterationPatterns.cs SortedIterationPatterns.cs \
	SortingPermutation.cs Toposort.cs ViewPatterns.cs WrappedArray.cs

all: c5examples.zip

c5examples.zip: ${USERGUIDEFILES}
	rm -f C5.examples.zip
	zip C5.examples.zip ${USERGUIDEFILES}

clean:
	rm -f C5.examples.zip
	rm -f *.dot
	rm -f *.exe
	rm -f *.ps
	rm -f *.eps

.SUFFIXES :
.SUFFIXES : .cs

