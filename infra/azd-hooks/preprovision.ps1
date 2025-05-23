Write-Host "Running preprovision.ps1..."

# Get the user principal name of the signed-in user and write it to the .env file for the azd environment
Write-Host "Fetching User Principal Name and setting PRINCIPAL_NAME environment variable..."
$env:PRINCIPAL_NAME = $(az ad signed-in-user show --query 'userPrincipalName' -o tsv)
azd env set "PRINCIPAL_NAME" "$env:PRINCIPAL_NAME"
Write-Host "PRINCIPAL_NAME: $env:PRINCIPAL_NAME"

# Get the client IP address and write it to the .env file for the azd environment
Write-Host "Fetching Client IP address and setting CLIENT_IP_ADDRESS environment variable..."
$env:CLIENT_IP_ADDRESS = Invoke-RestMethod -Uri "https://api.ipify.org"
azd env set "CLIENT_IP_ADDRESS" "$env:CLIENT_IP_ADDRESS"
Write-Host "CLIENT_IP_ADDRESS: $env:CLIENT_IP_ADDRESS"