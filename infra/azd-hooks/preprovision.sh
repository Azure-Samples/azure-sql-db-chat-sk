#!/bin/bash

echo "Running preprovision.sh..."

# Get the user principal name of the signed-in user and write it to the .env file for the azd environment
echo "Fetching User Principal Name and setting PRINCIPAL_NAME environment variable..."
PRINCIPAL_NAME=$(az ad signed-in-user show --query 'userPrincipalName' -o tsv)
azd env set "PRINCIPAL_NAME" "$PRINCIPAL_NAME"
echo "PRINCIPAL_NAME: $PRINCIPAL_NAME"

# Get the client IP address and write it to the .env file for the azd environment
echo "Fetching Client IP address and setting CLIENT_IP_ADDRESS environment variable..."
CLIENT_IP_ADDRESS=$(curl -s https://api.ipify.org)
azd env set "CLIENT_IP_ADDRESS" "$CLIENT_IP_ADDRESS"
echo "CLIENT_IP_ADDRESS: $CLIENT_IP_ADDRESS"
