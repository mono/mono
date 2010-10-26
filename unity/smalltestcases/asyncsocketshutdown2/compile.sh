ROOT=../../..

#static link:
gcc test.cpp -I$ROOT/mono/eglib/src -I$ROOT/builds/headers -Z -L$ROOT/builds/embedruntimes/osx -lmono -liconv -lstdc++-static -g -O0 -arch i386

#dynamic link
#gcc test.cpp -I$ROOT/mono/eglib/src -I$ROOT/builds/headers -Z -L$ROOT/builds/embedruntimes/osx -lstdc++-static -g -O0 -arch i386

echo "run with ./a.out"
