if not exists(select * from sys.symmetric_keys where [name] = '##MS_DatabaseMasterKey##')
begin
    create master key encryption by password = N'V3RYStr0NGP@ssw0rd!';
end
go

if exists(select * from sys.[database_scoped_credentials] where name = 'https://<name>.openai.azure.com/')
begin
	drop database scoped credential [https://<name>.openai.azure.com/];
end
go

create database scoped credential [https://<name>.openai.azure.com/]
with identity = 'HTTPEndpointHeaders', secret = '{"api-key":"<key>"}';
go

create schema [web] AUTHORIZATION [dbo];
go

