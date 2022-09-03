USE [rmncolxw]
GO
/****** Object:  StoredProcedure [dbo].[GetAllTask]    Script Date: 30-08-2022 16:37:51 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Prakash Ahir
-- Create date: 30/08/22
-- Description:	Get All Task
-- UserTypeId detail :
--		MasterAdmin = 1,
--		Admin = 2,
--		User = 3,
--		Consultant = 4
-- Status Detail : 
--		InProgress = 1,
--		Assigned = 2,
--		Cancel = 3,
--		Complete = 4,
--		OnHold = 5,
--		Pending = 6,
--		Unknown = 7
-- EXEC [dbo].[GetAllTask] @UserId = 4, @TypeId = 2
-- =============================================
ALTER PROCEDURE [dbo].[GetAllTask]
	-- Add the parameters for the stored procedure here
	@UserId int,
	@TypeId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	if(@TypeId = 3)
	begin
		select 
			t.id as TaskId,
			ut.id as TaskUserId,
			ut.Comment as UserComment,
			ut.UserId as UserId,
			t.[Description] as [Description],
			t.[Name] as TaskName,
			ut.DueDate as DueDate,
			ut.CreatedBy as CreatedBy,
			ut.CreatedOn as CreatedOn,
			u.Username as UserName,
			c.FullName as ClientName,
			t.Remarks as Remarks,
			ut.Status as StatusId,
			t.ChargeAmount as ChargeAmount
		from [dbo].[Tasks] t
		JOIN [dbo].[UserTasks] ut on t.id = ut.TaskId
		JOIN [dbo].[Users] u on u.Id = ut.UserId
		JOIN [dbo].[Users] c ON ISNULL(t.ClientId, 0) = c.Id
		where ut.UserId = @UserId and
		t.[Status] not in (5,3) 
	end
	else
	begin
		select 
			t.id as TaskId,
			ut.id as TaskUserId,
			ut.Comment as UserComment,
			ut.UserId as UserId,
			t.[Description] as [Description],
			t.[Name] as TaskName,
			ut.DueDate as DueDate,
			ut.CreatedBy as CreatedBy,
			ut.CreatedOn as CreatedOn,
			u.Username as UserName,
			c.FullName as ClientName,
			t.Remarks as Remarks,
			ut.Status as StatusId,
			t.ChargeAmount as ChargeAmount
		from [dbo].[Tasks] t
		JOIN [dbo].[UserTasks] ut on t.id = ut.TaskId
		JOIN [dbo].[Users] u on u.Id = ut.UserId
		JOIN [dbo].[Users] c ON ISNULL(t.ClientId, 0) = c.Id
		where ut.UserId = @UserId and
		t.[Status] not in (3,4,5) 
	end
    
END
