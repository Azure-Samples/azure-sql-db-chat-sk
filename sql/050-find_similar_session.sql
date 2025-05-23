create or alter procedure web.find_similar_sessions @topic nvarchar(max)
as

declare @e vector(1536);
exec [web].[get_embedding] @topic, @e output;

with similar as 
(
    select top(10)
        s.id,
        s.title,
        s.abstract,
        s.external_id,
        json_value(s.details, '$.speakers[0]') as speakers,
        least(
            vector_distance('cosine', @e, details_vector_text3),
            vector_distance('cosine', @e, abstract_vector_text3)
        ) as distance 
    from 
        [web].[sessions] s
    left join
        [web].[sessions_details_embeddings] e1 on s.id = e1.session_id
    left join
        [web].[sessions_abstracts_embeddings] e2 on s.id = e2.session_id
    order by 
        distance
)
select 
    *,
    1-distance as cosine_similiarity
from 
    similar
where
    distance <= 0.75
order by 
    distance

