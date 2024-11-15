CREATE OR ALTER PROCEDURE pass.find_communication_history_by_subject @customerId INT, @subject NVARCHAR(MAX)
AS
IF ((@subject IS NULL) OR (@subject =''))
BEGIN

    SELECT TOP(10)
        id,
        [communication_type],
        [communication_date] as [date],
        details
    FROM 
        [pass].[communication_history] e 
    WHERE
        e.customer_id = @customerId
    ORDER BY    
        [date] DESC

END ELSE BEGIN 

    DECLARE @e vector(1536)
    EXEC [pass].[get_embedding] @subject, @e OUTPUT;

    SELECT TOP(10)
        id,
        [communication_type],
        [communication_date] as [date],
        details,
        VECTOR_DISTANCE('cosine', @e, [embedding]) as distance 
    FROM 
        [pass].[communication_history] e 
    WHERE
        e.customer_id = @customerId
    AND 
        VECTOR_DISTANCE('cosine', @e, [embedding]) < 0.6
    ORDER BY    
        distance

END
GO

