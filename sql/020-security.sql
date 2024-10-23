if not exists(select * from sys.symmetric_keys where [name] = '##MS_DatabaseMasterKey##')
begin
    create master key encryption by password = N'V3RYStr0NGP@ssw0rd!';
end
go

if exists(select * from sys.[database_scoped_credentials] where name = '$OPENAI_URL$')
begin
	drop database scoped credential [$OPENAI_URL$];
end
go

create database scoped credential [$OPENAI_URL$]
with identity = 'HTTPEndpointHeaders', secret = '{"api-key":"$OPENAI_KEY$"}';
go
