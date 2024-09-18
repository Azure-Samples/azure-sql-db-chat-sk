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
            "details": "{\"speakers\":[\"Davide Mauri\"],\"track\":\"Data Engineering\",\"language\":\"English\",\"level\":300}",
            "require_embeddings_update": "0"
        }]') with (
            id int '$.id',
            title nvarchar(200) '$.title',
            abstract nvarchar(max) '$.abstract',
            external_id varchar(100) '$.external_id',
            details json '$.details',
            require_embeddings_update bit '$.require_embeddings_update'
        )
go

declare @a as nvarchar(max);
declare @e as vector(1536);
select @a = abstract from [web].[sessions] where id = 9;
exec web.get_embedding @a, @e output;
insert into [web].[sessions_abstracts_embeddings] ([session_id], abstract_vector_text3) values (9, @e);

declare @d as nvarchar(max);
declare @e as vector(1536);
select @d = cast(details as nvarchar(max)) from [web].[sessions] where id = 9;
exec web.get_embedding @d, @e output;
insert into [web].[sessions_details_embeddings] ([session_id], details_vector_text3) values (9, @e);

