#!/bin/sh
declare -i limit=$1

psql -d npgsql_tests -U npgsql_tests -c "create table wordlist (word text);"
declare -i i=0; 
while true
do  
  psql -d npgsql_tests -U npgsql_tests -c "insert into wordlist values ('a');" ; 
  ((i++)); 
  if (( $i >= $limit ))
  then 
    break
  fi
done >/dev/null
psql -d npgsql_tests -U npgsql_tests -c "select count(*) from wordlist;"
