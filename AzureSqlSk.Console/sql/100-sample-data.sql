DELETE FROM [dbo].[communication_history]
DELETE FROM [dbo].[policies]
DELETE FROM [dbo].[claims] 
DELETE FROM [dbo].[customers]
GO

INSERT INTO [dbo].[customers] ([id], [first_name], [last_name], [address], [city], [state], [zip], [country], [email], [details])
VALUES 
(1539726, 'John', 'Doe', '123 Old Road', 'Bellevue', 'WA', '98007', 'USA', 'johndoe@contoso.com', '{"active-policies": ["car", "renters"]}'),
(920411, 'John', 'Doe', '456 Main St', 'Redmond', 'WA', '98052', 'USA', 'jd@adventureworks.com', '{"active-policies": ["umbrella", "car", "homeowners"]}'),
(290332, 'John', 'Doe', '789 Lake Road', 'Kirkland', 'WA', '98017', 'USA', 'johnd2@contoso.com', '{"active-policies": ["car", "boat"]}')
go

INSERT INTO [dbo].[claims] ([id], [customer_id], [claim_type], [claim_date], [details])
VALUES 
(9323, 1539726, 'auto', '2021-03-06', 'customer was rear-ended, small damage'),
(9802, 920411, 'auto', '2021-03-06', 'customer was rear-ended, small damage to the rear bumper. The bumper was replaced by a in-network shop and the customer didn''t have to pay anything.'),
(10323, 920411, 'auto', '2022-07-03', 'customer had a broken glass due to a stone hitting the left door window. The window was replaced and the customer was reimbursed for the cost of the window and the labor.'),
(11237, 920411, 'auto', '2023-01-02', 'faulty battery, customer was towed to the closest car shop and faulty batter was replaced. Customer was reimbursed for the cost of the battery and the towing service');
go

INSERT INTO [dbo].[policies] ([id], [customer_id], [type], [premium], [payment_type], [start_date], [duration], [payment_amount], [additional_notes])
VALUES
(1203, 920411, 'auto', 2400.66, 'monthly', '2021-07-10', '6 months', 400.11, 'all payments in good standing. no missed or late payment so far.'),
(4053, 920411, 'auto', 2617.26, 'monthly', '2022-01-10', '6 months', 436.21, 'all payments in good standing. no missed or late payment so far.'),
(10223, 920411, 'auto', 2936.04, 'monthly', '2022-07-10', '6 months', 489.34, 'all payments in good standing. no missed or late payment so far.'),
(12005, 920411, 'auto', 2936.04, 'monthly', '2023-01-10', '6 months', 489.34, 'all payments in good standing. no missed or late payment so far.'),
(16884, 920411, 'auto', 2936.04, 'monthly', '2023-07-10', '6 months', 489.34, 'all payments in good standing. no missed or late payment so far.'),
(23456, 920411, 'auto', 3001.32, 'monthly', '2024-01-10', '6 months', 500.22, 'all payments in good standing. no missed or late payment so far.'),
(67832, 920411, 'auto', 3212.45, 'monthly', '2024-07-10', '6 months', 535.41, 'all payments in good standing. no missed or late payment so far.')
go

INSERT INTO [dbo].[communication_history] ([id], [customer_id], [communication_type], [communication_date], [details])
VALUES
(9213, 920411, 'mail', '2023-08-11', 'customer reached out to understand why the premium increased by 10%, customer was informed that the premium increased due to the increase in the cost of the parts and labor. Customer was satisfied with the explanation and decided to keep the policy.'),
(12012, 920411, 'phone', '2023-09-20', 'customer reached our to understand if his policy also covers rented cars. Customer was informed that the policy do cover rented cars for up to 15 days. Car must be retned in the US and Canada. Customer was very happy with the information.'),
(14053, 920411, 'phone', '2024-01-16', 'customer reached out to ask to have heads up when premium increases. Customer was informed that we will send an email 30 days before the premium increases. Customer was happy with the information, but not happy that he didn''t receive anything this time.'),
(18928, 920411, 'mail', '2024-07-17', 'customer very unhappy about the premium is increased again. almost furious as he didn''t got any upfront information about that. evaluating to move to another company.'),
(19022, 920411, 'phone', '2024-07-17', 'john reached out to ask for a meeting with agent urgently. he didn''t provide any details about the reason for the meeting.')
go

-- create loop to call get_embeddings for each row in [dbo].[communication_history]. use a forward only cursor
DECLARE @id INT, @details NVARCHAR(MAX);
DECLARE @e VECTOR(1536)
DECLARE C CURSOR LOCAL FORWARD_ONLY FOR
SELECT [id], [details] FROM [dbo].[communication_history];
OPEN C;
FETCH NEXT FROM C INTO @id, @details;
WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC [dbo].[get_embedding] @details, @e OUTPUT;
    UPDATE [dbo].[communication_history] SET [embedding] = @e WHERE [id] = @id;
    FETCH NEXT FROM C INTO @id, @details;
END
CLOSE C;
DEALLOCATE C;
go

/*
EXEC dbo.find_communication_history_by_subject 920411, 'premium increase'
GO

EXEC dbo.find_communication_history_by_subject 920411, 'car insurance details'
GO

EXEC dbo.find_communication_history_by_subject 920411, ''
GO
*/