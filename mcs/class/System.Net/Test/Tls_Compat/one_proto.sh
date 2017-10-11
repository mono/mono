#!/bin/bash -u

set -u

MASTER_PID=$1
CIPHER=$2
ROLE=$3
PORT=$4
NUM_CLIENTS=$5
SIGNATURE_TYPE=$6
PROTO=$7

handler()
{
    kill -9 `jobs -p` || true
    kill -9 $MASTER_PID
}

trap handler SIGINT 

CA="ca.crt"
CA_CERT_PFX="ca.pfx"

INTERMEDIATE_CERT="intermediate.crt"
INTERMEDIATE_PFX="intermediate.pfx"
INTERMEDIATE_KEY="intermediate.key"

INTERMED_SIGNED_CERT="intermed_signed.crt"
INTERMED_SIGNED_PFX="intermed_signed.pfx"
INTERMED_SIGNED_KEY="intermed_signed.key"

DIRECTLY_SIGNED_CERT="directly_signed.crt"
DIRECTLY_SIGNED_PFX="directly_signed.pfx"
DIRECTLY_SIGNED_KEY="directly_signed.key"

CA_CERT_PASS=""
CERT_PASS=""

if [ $SIGNATURE_TYPE == "directly" ]
then
  CERT=$DIRECTLY_SIGNED_CERT 
  CERT_PFX=$DIRECTLY_SIGNED_PFX
  OPENSSL_SERVER_CA=$CA
  KEYFILE=$DIRECTLY_SIGNED_KEY
else
  CERT=$INTERMED_SIGNED_CERT 
  CERT_PFX=$INTERMED_SIGNED_PFX
  OPENSSL_SERVER_CA=$INTERMEDIATE_CERT
  KEYFILE=$INTERMED_SIGNED_KEY
fi

SERVER_CMD_OPENSSL="openssl s_server $PROTO -accept $PORT -cert $CERT -key $KEYFILE -CAfile $OPENSSL_SERVER_CA -verify_return_error -cipher $CIPHER -serverpref -www"
CLIENT_CMD_OPENSSL="openssl s_client $PROTO -showcerts -connect localhost:$PORT -servername localhost -verify_return_error -CAfile $CA -cipher $CIPHER"

MONO=${MONO_RUNTIME:-mono}
SERVER_CMD_MONO="$MONO --debug Harness.exe server 127.0.0.1 $PORT $NUM_CLIENTS $CERT_PFX $CERT_PASS"
CLIENT_CMD_MONO="$MONO --debug Harness.exe client localhost $PORT $NUM_CLIENTS $CA_CERT_PFX $CA_CERT_PASS"

if [ $ROLE == "test_server" ]
then
  SERVER=$SERVER_CMD_MONO
  CLIENT=$CLIENT_CMD_OPENSSL

  echo "$SERVER &"
  $SERVER &
  SERVER_PID=$!

  sleep 2

  for i in {1..$NUM_CLIENTS}
  do
    echo "$CLIENT"
    $CLIENT
  done

  kill $SERVER_PID

elif [ $ROLE == "test_client" ]
then
  SERVER=$SERVER_CMD_OPENSSL
  CLIENT=$CLIENT_CMD_MONO

  echo "$SERVER &"
  $SERVER &
  SERVER_PID=$!

  sleep 2

  echo "$CLIENT &"
  $CLIENT &
  CLIENT_PID=$!

  wait $CLIENT_PID
  kill -9 $SERVER_PID || true

else
  echo "No proper role given"
fi


