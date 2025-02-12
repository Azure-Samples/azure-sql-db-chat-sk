CREATE OR ALTER PROCEDURE dbo.find_communication_history_by_subject 
@customerId INT, 
@subject NVARCHAR(MAX) = 'all'
AS
IF ((@subject IS NULL) OR (TRIM(@subjec)='') OR (TRIM(LOWER(@subject))='all'))
BEGIN

    SELECT TOP(10)
        id,
        [communication_type],
        [communication_date] as [date],
        details
    FROM 
        [dbo].[communication_history] e 
    WHERE
        e.customer_id = @customerId
    ORDER BY    
        [date] DESC

END ELSE BEGIN 

    DECLARE @e vector(1536)
    EXEC [dbo].[get_embedding] @subject, @e OUTPUT;

    SELECT TOP(10)
        id,
        [communication_type],
        [communication_date] as [date],
        details,
        VECTOR_DISTANCE('cosine', @e, [embedding]) as distance 
    FROM 
        [dbo].[communication_history] e 
    WHERE
        e.customer_id = @customerId
    AND 
        VECTOR_DISTANCE('cosine', @e, [embedding]) < 0.6
    ORDER BY    
        distance

END
GO

