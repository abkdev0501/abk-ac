-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Prakash Ahir
-- Create date: 22-09-22
-- Description:	Get Task By Id
-- =============================================
CREATE PROCEDURE [dbo].[GetTaskById]
	-- Add the parameters for the stored procedure here
	@Id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

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
		ut.[Status] as StatusId,
		t.Active as Active,
		t.ClientId as ClientId,
		t.CompletedOn as CompletedOn,
		t.Priorities as Priorities,
		t.Remarks as Remarks,
		isnull(t.IsChargeble, 0) as IsChargeble,
		t.ChargeAmount as ChargeAmount
	from Tasks t
	join UserTasks ut on t.id = ut.TaskId
	join Users u on ut.UserId = u.Id
	where t.id = @Id

END
GO
