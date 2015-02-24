#!/bin/bash -u

GIT_ROOT=`dirname $0`
GIT_ROOT="`( cd \"$GIT_ROOT\" && pwd )`"

PORT=$1
NUM_CLIENTS=$2

ciphers=(
'AES256-SHA'
'DES-CBC3-SHA'
'AES128-SHA'
'RC4-SHA'
'RC4-MD5'
'RC4-MD5'
'DES-CBC-SHA'
)

protocol_versions=(
'-tls1_2'
'-tls1_1'
'-tls1'
)

DEFAULT_PROTOCOL='-tls1'

echo "Starting tests. This PID is $$"

printf "Testing Server Directly Signed\n"

for cipher in "${ciphers[@]}"
do
    printf "\n\nTesting Cipher $cipher\n"
    bash $GIT_ROOT/one_proto.sh $$ $cipher test_server $PORT $NUM_CLIENTS directly $DEFAULT_PROTOCOL 
done

printf "\n\nTesting Client Directly Signed\n\n"

for cipher in "${ciphers[@]}"
do
    printf "\n\nTesting Cipher $cipher\n"
    bash $GIT_ROOT/one_proto.sh $$ $cipher test_client $PORT $NUM_CLIENTS directly $DEFAULT_PROTOCOL 
done

printf "\n\nTesting Client Intermediately Signed\n\n"

for cipher in "${ciphers[@]}"
do
    printf "\n\nTesting Cipher $cipher\n"
    bash $GIT_ROOT/one_proto.sh $$ $cipher test_client $PORT $NUM_CLIENTS indirectly $DEFAULT_PROTOCOL 
done

printf "\n\nTesting Server With Different Protocol Versions\n"

for proto in "${protocol_versions[@]}"
do
    printf "\n\nTesting Proto $proto\n"
    bash $GIT_ROOT/one_proto.sh $$ ${ciphers[1]} test_server $PORT $NUM_CLIENTS directly $proto
done

printf "\n\nTesting Client With Different Protocol Versions\n"

for proto in "${protocol_versions[@]}"
do
    printf "\n\nTesting Proto $proto\n"
    bash $GIT_ROOT/one_proto.sh $$ ${ciphers[1]} test_client $PORT $NUM_CLIENTS directly $proto
done
