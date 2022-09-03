USE [rmncolxw]
GO
/****** Object:  StoredProcedure [dbo].[GetAllNotification]    Script Date: 30-08-2022 18:51:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Prakash Ahir
-- Create date: 30-08-22
-- Description:	Get All Notification
-- UserTypeId detail :
--		MasterAdmin = 1,
--		Admin = 2,
--		User = 3,
--		Consultant = 4
-- EXEC [dbo].[GetAllNotification] @UserId = 4, @UserType = 2, @Type = 1
-- =============================================
ALTER PROCEDURE [dbo].[GetAllNotification]
	-- Add the parameters for the stored procedure here
	@UserId int, 
	@UserType int, 
	@Type int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	if(@UserType = 3)
	begin
		select
			NotificationId,
			ClientId,
			CreatedBy,
			isnull(IsComplete,0) as IsComplete,
			[Message],
			OffBroadcastDateTime,
			OnBroadcastDateTime
		from Notifications
		where [Type] = @Type
		and (ClientId = 0 or ClientId = @UserId)
		and isnull(IsComplete,0) = 0
		and getdate() >= OnBroadcastDateTime
		and getdate() <= OffBroadcastDateTime
		order by ClientId
	end
	else
	begin
		select
			NotificationId,
			ClientId,
			CreatedBy,
			isnull(IsComplete,0) as IsComplete,
			[Message],
			OffBroadcastDateTime,
			OnBroadcastDateTime
		from Notifications
		where [Type] = 1
		and [Type] = @Type
		and isnull(IsComplete,0) = 0
		and getdate() >= OnBroadcastDateTime
		and getdate() <= OffBroadcastDateTime
		order by ClientId
	end
    
END
