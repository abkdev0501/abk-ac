USE [rmncojxg_master]
GO
/****** Object:  StoredProcedure [dbo].[GetAllNotification]    Script Date: 12-09-2022 13:49:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Prakash Ahir
-- Create date: 12-09-22
-- Description:	Get All Notification list
-- =============================================
ALTER PROCEDURE [dbo].[GetAllNotificationList]
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select
		NotificationId,
		ClientId,
		isnull(u.FullName, 'All') as ClientName,
		n.CreatedBy,
		u2.FullName as CreatedByName,
		isnull(IsComplete,0) as IsComplete,
		[Message],
		OffBroadcastDateTime,
		OnBroadcastDateTime,
		n.Type
	from Notifications n
	left join [Users] u on n.ClientId = u.Id
	left join [Users] u2 on n.CreatedBy = u2.CreatedBy
    
END
