#!/bin/bash
openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout BestStore.key -out BestStore.crt
# 產生無密碼的 pfx 檔案
# openssl pkcs12 -export -out BestStore.pfx -inkey BestStore.key -in BestStore.crt
# 產生有密碼的 pfx 檔案
openssl pkcs12 -export -out BestStore.pfx -inkey BestStore.key -in BestStore.crt -passout pass:Seraph964375