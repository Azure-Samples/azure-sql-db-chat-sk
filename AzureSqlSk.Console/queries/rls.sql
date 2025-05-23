create table dbo.access_security
(
    [id] int not null identity primary key,
    [user_account] varchar(100) collate Latin1_General_BIN2 not null,
    [customer_id] int not null
)
go
delete from dbo.access_security;
insert into dbo.access_security 
    (user_account, customer_id) 
values 
    (suser_sname(), 920411),
    (suser_sname(), 290332)
go

--delete from dbo.access_security  where customer_id = 920411

select suser_sname()
select * from dbo.access_security 

create function dbo.check_access_security(@customer_id as int)
returns table
with schemabinding
as
return
    select 1 as authorized from dbo.access_security 
    where [user_account] = suser_sname() and [customer_id] = @customer_id
go

create security policy [access_security_policy]
add filter predicate dbo.check_access_security([id]) on [dbo].[customers],
add filter predicate dbo.check_access_security([customer_id]) on [dbo].[policies],
add filter predicate dbo.check_access_security([customer_id]) on [dbo].[claims],
add filter predicate dbo.check_access_security([customer_id]) on [dbo].[communication_history]
with (state = off)
go

alter security policy [access_security_policy]
with (state = on)
go

select * from sys.security_policies
go

alter security policy [access_security_policy]
with (state = off)
go