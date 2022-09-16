USE [rmncojxg_master]
GO
/****** Object:  StoredProcedure [dbo].[GetAllNotification]    Script Date: 12-09-2022 14:48:35 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Prakash Ahir
-- Create date: 12-09-22
-- Description:	Get notification by id
-- EXEC [dbo].[GetNotificationById] @Id = 4
-- =============================================
CREATE PROCEDURE [dbo].[GetNotificationById]
	-- Add the parameters for the stored procedure here
	@Id int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
		select
			NotificationId,
			ClientId,
			CreatedBy,
			isnull(IsComplete,0) as IsComplete,
			[Message],
			OffBroadcastDateTime,
			OnBroadcastDateTime,
			[Type]
		from Notifications
		where NotificationId = @Id
END
