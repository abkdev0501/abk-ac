USE [rmncojxg_master]
GO
/****** Object:  StoredProcedure [dbo].[GetAllNotificationList]    Script Date: 22-09-2022 19:12:41 ******/
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
	@RecordFrom int,
	@PageSize int,
	@SortColumn nvarchar(max) = '',
	@SortOrder nvarchar(max) = '',
	@Message nvarchar(max) = null,
	@ClientName nvarchar(max) = null,
	@Type nvarchar(20) = null,
	@OnBDTFrom datetime = null,
	@OnBDTTo datetime = null,
	@OffBDTFrom datetime = null,
	@OffBDTTo datetime = null,
	@IsCompleted bit = null,
	@CreatedBy nvarchar(max) = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	;with CteNotes as
	(
		select
			NotificationId,
			ClientId,
			LTRIM(RTRIM(isnull((u.FullName), 'All'))) as ClientName,
			n.CreatedBy,
			u2.FullName as CreatedByName,
			isnull(IsComplete,0) as IsComplete,
			[Message],
			OffBroadcastDateTime,
			OnBroadcastDateTime,
			n.Type
		from Notifications n
		left join [Users] u on n.ClientId = u.Id
		left join [Users] u2 on n.CreatedBy = u2.Id
	)
	, CteNotesResult as
	(
		select * from
		CteNotes
		where (@Message is null or [Message] like '%'+@Message+'%') 
		and (@ClientName is null or ClientName like '%'+@ClientName+'%')
		and (@CreatedBy is null or CreatedBy like '%'+@CreatedBy+'%')
		and (@Type is null or [Type] in (select [name] from dbo.splitstring(@Type))) 
		and (@IsCompleted is null or IsComplete = @IsCompleted)
		and (@OffBDTFrom is null or OffBroadcastDateTime >= @OffBDTFrom)
		and (@OffBDTTo is null or OffBroadcastDateTime <= @OffBDTTo)
		and (@OnBDTFrom is null or OnBroadcastDateTime >= @OnBDTFrom)
		and (@OnBDTTo is null or OnBroadcastDateTime <= @OnBDTFrom)
	)
	, CteNotes_Count AS (Select COUNT(NotificationId) AS TotalRecords FROM CteNotesResult)

	SELECT * FROM CteNotesResult, CteNotes_Count
	ORDER BY
	-- then default order by taskid
	CASE WHEN @SortColumn='' or @SortColumn is null THEN NotificationId END desc,
	CASE WHEN @SortColumn='Message' AND @SortOrder='Asc' THEN [Message] END ASC,
	CASE WHEN @SortColumn='ClientName' AND @SortOrder='asc' THEN ClientName END ASC,
	CASE WHEN @SortColumn='CreatedByName' AND @SortOrder='asc' THEN CreatedBy END ASC,
	CASE WHEN @SortColumn='TypeString' AND @SortOrder='asc' THEN [Type] END ASC,
	CASE WHEN @SortColumn='IsCompleted' AND @SortOrder='asc' THEN [IsComplete] END ASC,
	CASE WHEN @SortColumn='OffBroadcastDateTimeString' AND @SortOrder='asc' THEN OffBroadcastDateTime END ASC,
	CASE WHEN @SortColumn='OnBroadcastDateTimeString' AND @SortOrder='asc' THEN OnBroadcastDateTime END ASC,

	CASE WHEN @SortColumn='Message' AND @SortOrder='Desc' THEN [Message] END DESC,
	CASE WHEN @SortColumn='ClientName' AND @SortOrder='desc' THEN ClientName END DESC,
	CASE WHEN @SortColumn='CreatedByName' AND @SortOrder='desc' THEN CreatedBy END DESC,
	CASE WHEN @SortColumn='TypeString' AND @SortOrder='desc' THEN [Type] END DESC,
	CASE WHEN @SortColumn='IsCompleted' AND @SortOrder='desc' THEN [IsComplete] END DESC,
	CASE WHEN @SortColumn='OffBroadcastDateTimeString' AND @SortOrder='desc' THEN OffBroadcastDateTime END DESC,
	CASE WHEN @SortColumn='OnBroadcastDateTimeString' AND @SortOrder='desc' THEN OnBroadcastDateTime END DESC

	OFFSET @RecordFrom ROWS 
	FETCH NEXT CASE @PageSize WHEN -1 THEN 5000 ELSE @PageSize END ROWS ONLY

END
