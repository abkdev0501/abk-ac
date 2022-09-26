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
-- Author:		Prakas Ahit
-- Create date: 22-09-22
-- Description:	Task Update
-- =============================================
alter PROCEDURE [dbo].[TaskUpdate]
	-- Add the parameters for the stored procedure here
	@Id int,
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

	declare @InvoiceCreate bit = 0;
	declare @OldTaskStatus bit = 1;

	select 
		@OldTaskStatus =  Active
	from Tasks where id = @Id

	update Tasks
	set 
		[Name] = @TaskName,
		[Active] = @Active,
		[Status] = @StatusId,
		[Description] = @Description,
		ClientId = @ClientId,
		Priorities = @Priorities,
		Remarks = @Remarks,
		IsChargeble = @IsChargeble,
		CompletedOn = @CompletedOn,
		ModifiedOn = GETDATE(),
		ChargeAmount = @ChargeAmount
	where id = @Id

	update UserTasks
	set
		[Status] = @StatusId,
		[Comment] = @UserComment,
		UserId = @UserId,
		ModifiedOn = GETDATE(),
		DueDate = @DueDate
	where TaskId = @Id

	if(@IsChargeble = 1 and @OldTaskStatus != @Active and @Active = 1)
	begin
		set @InvoiceCreate = 1
	end

	select @InvoiceCreate as InvoiceCreate

END
GO
