declare @e vector(1536)
exec [web].[get_embedding] 'data engineering track', @e output;

with similar_details as 
(
    select top(10)
        e.session_id,
        vector_distance('cosine', @e, details_vector_text3) as distance 
    from 
        [web].[sessions_details_embeddings] e 
    order by 
        distance
),
similar_abstracts as 
(
    select top(10)
        e.session_id,
        vector_distance('cosine', @e, abstract_vector_text3) as distance 
    from 
        [web].[sessions_abstracts_embeddings] e 
    order by 
        distance
),
similar as 
(
    select * from similar_details
    union all
    select * from similar_abstracts
)
select top(10) 
    s.*,
    distance 
from 
    similar si
inner join
    [web].[sessions] s on si.session_id = s.id
order by 
    distance

