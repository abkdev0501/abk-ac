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
-- Create date: 23-09-22
-- Description:	Task create
-- =============================================
CREATE PROCEDURE [dbo].[TaskCreate]
	-- Add the parameters for the stored procedure here
	@UserTypeId int,
	@CurrentUserId int,
	@TaskName nvarchar(max),
	@Active bit,
	@StatusId int,
	@Description nvarchar(max),
	@Remarks nvarchar(max),
	@UserComment nvarchar(max),
	@UserId int,
	@IsChargeble bit,
	@ClientId int = null,
	@Priorities int = null,
	@CompletedOn datetime = null,
	@DueDate datetime = null,
	@ChargeAmount decimal = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @id int;
	declare @InvoiceCreate bit = 0;

	insert into Tasks
	(
		[Name],
		[Description],
		[CreatedBy],
		[CreatedOn],
		[Status],
		[Active],
		[ClientId],
		[Priorities],
		[Remarks],
		[IsChargeble],
		[CompletedOn],
		[AddedBy],
		[ChargeAmount]
	)
	values
	(
		@TaskName,
		@Description,
		@UserTypeId,
		getdate(),
		@StatusId,
		@Active,
		@ClientId,
		@Priorities,
		@Remarks,
		@IsChargeble,
		@CompletedOn,
		@CurrentUserId,
		@ChargeAmount
	)

	set @id = SCOPE_IDENTITY();

	insert into UserTasks
	(
		[TaskId],
		[UserId],
		[Comment],
		[Status],
		[Active],
		[CreatedBy],
		[CreatedOn],
		[DueDate],
		[AddedBy]
	)
	values
	(
		@id,
		@UserId,
		@UserComment,
		@StatusId,
		@Active,
		@UserTypeId,
		getdate(),
		@DueDate,
		@CurrentUserId
	)

	if(@IsChargeble = 1 and @Active = 1)
	begin
		set @InvoiceCreate = 1
	end

	select @InvoiceCreate as InvoiceCreate 

END
GO
