/*
delete from [web].[sessions_abstracts_embeddings];
delete from [web].[sessions_details_embeddings];
delete from [web].[sessions];
go
*/

insert into
    [web].[sessions]
select 
    * 
from 
    openjson(
        '[{
            "id": 9,
            "title": "Azure SQL and SQL Server: All Things Developers",
            "abstract": "Over the past two decades, SQL Server has undergone a remarkable evolution, emerging as the most widely deployed database in the world. A great number of new features have been announced for Azure SQL and SQL Server to support developers in being more efficient and productive when creating new solutions and applications or when modernizing existing ones. In this session, we go over all the lastest released features  such as JSON, Data API builder, calling REST endpoints, Azure function integrations and much more, so that you''ll learn how to take advantage of them right away.",
            "external_id": "683984",
            "details": {"speakers":["Davide Mauri"],"track":"Data Engineering","language":"English","level":300}
        }]') with (
            id int '$.id',
            title nvarchar(200) '$.title',
            abstract nvarchar(max) '$.abstract',
            external_id varchar(100) '$.external_id',
            details nvarchar(max) '$.details' as json
        )
go

insert into
    [web].[sessions]
select 
    * 
from 
    openjson(
        '[{
            "id": 2,
            "title": "Erkläre meiner techniknahen Frau Fabric und seine Fähigkeit, Daten sofort zu analysieren",
            "abstract": "Das Erlernen einer neuen Technologie wie Fabric ähnelt dem Erlernen einer neuen Sprache. Es gibt unbekannte Wörter, Grammatik und Konzepte, aber vieles ist ähnlich zu dem, was du bereits kennst. Wenn du dich im Umfeld von Microsoft Data befindest, bin ich mir sicher, dass du gefragt wurdest: Was ist Microsoft Fabric? Und Wofür können wir es verwenden? So haben meine techniknahe Ehefrau und ich diese Fragen geklärt und die Sprachbarriere überwunden :-) Wir werden auch zeigen, wie du Echtzeitanalysen über dein Smart Home in Microsoft Fabric abfragen kannst. Also, wenn du keine Ahnung hast, was Fabric ist, oder wenn du Fabric jemandem erklären musst, der keine Ahnung hat, könnten diese 10 Minuten nützlich für dich sein.  Explaining Fabric and its ability to analyse data instantly to my tech-adjacent wife  Learning a new technology like Fabric is similar learning a new language. There are unfamiliar words, grammar, and concepts but a lot is similar to what you already know. If you are around Microsoft Data I am certain that you will have been asked. What is Microsoft Fabric ? And What can we use it for? This is how my tech-adjacent wife, and I resolved those questions and broke down the language barrier :-) We will also show how you can query Real-Time Analytics about your smart home in Microsoft Fabric. So if you have no idea what Fabric is or you have to explain Fabric to someone who has no idea maybe these 10 minutes may be useful for you.",
            "external_id": "456",
            "details": {"speakers":["Rob Sewell"],"track":"Analytics & Data Science","language":"German","level":100}
        }]') with (
            id int '$.id',
            title nvarchar(200) '$.title',
            abstract nvarchar(max) '$.abstract',
            external_id varchar(100) '$.external_id',
            details nvarchar(max) '$.details' as json
        )
go

declare @a as nvarchar(max);
declare @e as vector(1536);
select @a = abstract from [web].[sessions] where id = 9;
exec web.get_embedding @a, @e output;
insert into [web].[sessions_abstracts_embeddings] ([session_id], abstract_vector_text3) values (9, @e);
go

declare @d as nvarchar(max);
declare @e as vector(1536);
select @d = cast(details as nvarchar(max)) from [web].[sessions] where id = 9;
exec web.get_embedding @d, @e output;
insert into [web].[sessions_details_embeddings] ([session_id], details_vector_text3) values (9, @e);
go
