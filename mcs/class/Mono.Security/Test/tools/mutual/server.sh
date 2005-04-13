openssl s_server -www -cert server_cert.pem -key server_key.pem -Verify client.pem -CAfile ca.pem
