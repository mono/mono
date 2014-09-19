SRC = link.c
OBJ = libmono-profiler-link.so

all:
	gcc -g -rdynamic -Wall -shared -o $(OBJ) $(SRC) `pkg-config --cflags --libs mono`

clean:
	rm -f $(OBJ)
