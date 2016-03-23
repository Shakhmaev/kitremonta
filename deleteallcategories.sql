while (select count(1) from Categories) > 0
begin
	declare @a int;
	SELECT @a = (select TOP 1 CategoryId FROM Categories ORDER BY CategoryId DESc);
	print @a;
	delete from Categories where CategoryId = ( SELECT TOP 1 CategoryId FROM Categories ORDER BY CategoryId DESc);
end;