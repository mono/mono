
all: test

baseline:
	@for subdir in `ls -d test*`; do \
		rm -f $$subdir/*.png; \
		./harness.sh baseline $$subdir; \
	done

test:
	@for subdir in `ls -d test*`; do \
		case $$subdir in \
		test2* ) (cd $$subdir && gmcs -debug -pkg:dotnet test.cs);; \
		test* ) (cd $$subdir && mcs -debug -pkg:dotnet test.cs);; \
		esac; \
		./harness.sh test $$subdir; \
	done
