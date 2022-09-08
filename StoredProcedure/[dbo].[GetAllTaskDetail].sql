USE [rmncojxg_master]
GO
/****** Object:  StoredProcedure [dbo].[GetAllTaskDetail]    Script Date: 07-09-2022 18:00:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Prakash Ahir
-- Create date: 30-08-22
-- Description:	Get All Task Details
-- =============================================
ALTER PROCEDURE [dbo].[GetAllTaskDetail]
	-- Add the parameters for the stored procedure here
	@UserTypeId int,
	@UserId int,
	@RecordFrom int,
	@PageSize int,
	@SortColumn nvarchar(max) = '',
	@SortOrder nvarchar(max) = '',
	@TaskName nvarchar(max) = null,
	@UserName nvarchar(max) = null,
	@ClientName nvarchar(max) = null,
	@Description nvarchar(max) = null,
	@Status nvarchar(max) = null,
	@UserComment nvarchar(max) = null,
	@IsChargeble bit = null,
	@Priorities nvarchar(max) = null,
	@CreatedBy nvarchar(max) = null,
	@CreatedOnFrom datetime = null,
	@CreatedOnTo datetime = null,
	@DueDateFrom datetime = null,
	@DueDateTo datetime = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SET @SortColumn = CASE WHEN @SortColumn != '' THEN LTRIM((RTRIM(@SortColumn))) ELSE @SortColumn END;
	SET @SortOrder =  CASE WHEN @SortOrder != '' THEN LTRIM((RTRIM(@SortOrder))) ELSE @SortOrder END;

	;with cteUserTask as
	(
		-- user
		select
			t.id as TaskId,
			ut.id as TaskUserId,
			ut.Comment as UserComment,
			ut.UserId as UserId,
			t.[Description] as [Description],
			t.[Name] as TaskName,
			ut.DueDate as DueDate,
			ut.CreatedBy as CreatedBy,
			isnull(u.FullName,'') as CreatedByString,
			ut.CreatedOn as CreatedOn,
			isnull(un.FullName,'') as UserName,
			t.Priorities as Priorities,
			ut.[Status] as StatusId,
			tat.FullName as ClientName,
			isnull(t.IsChargeble,0) as IsChargeble,
			t.ChargeAmount as ChargeAmount
		from Tasks t
		join UserTasks ut on t.id = ut.TaskId
		join Users tat on tat.Id = isnull(t.ClientId,0)
		left join Users u on ut.AddedBy = u.Id
		left join Users un on ut.UserId = un.Id
		where @UserTypeId = 3 and tat.Id = @UserId
		union all 
		-- admin
		select
			t.id as TaskId,
			ut.id as TaskUserId,
			ut.Comment as UserComment,
			ut.UserId as UserId,
			t.[Description] as [Description],
			t.[Name] as TaskName,
			ut.DueDate as DueDate,
			ut.CreatedBy as CreatedBy,
			isnull(u.FullName,'') as CreatedByString,
			ut.CreatedOn as CreatedOn,
			isnull(un.FullName,'') as UserName,
			t.Priorities as Priorities,
			ut.[Status] as StatusId,
			tat.FullName as ClientName,
			isnull(t.IsChargeble,0) as IsChargeble,
			t.ChargeAmount as ChargeAmount
		from Tasks t
		join UserTasks ut on t.id = ut.TaskId
		join Users tat on isnull(t.ClientId,0) = tat.id
		left join Users u on ut.AddedBy = u.Id
		left join Users un on ut.UserId = un.Id
		where @UserTypeId = 2 and 
		(tat.Id = @UserId or ut.UserId = @UserId or ut.AddedBy = @UserId)
		union all 
		-- others
		select
			t.id as TaskId,
			ut.id as TaskUserId,
			ut.Comment as UserComment,
			ut.UserId as UserId,
			t.[Description] as [Description],
			t.[Name] as TaskName,
			ut.DueDate as DueDate,
			ut.CreatedBy as CreatedBy,
			isnull(u.FullName,'') as CreatedByString,
			ut.CreatedOn as CreatedOn,
			isnull(un.FullName,'') as UserName,
			t.Priorities as Priorities,
			ut.[Status] as StatusId,
			uc.FullName as ClientName,
			isnull(t.IsChargeble,0) as IsChargeble,
			t.ChargeAmount as ChargeAmount
		from Tasks t
		join UserTasks ut on t.id = ut.TaskId
		left join Users u on ut.AddedBy = u.Id
		left join Users un on ut.UserId = un.Id
		left join Users uc on uc.Id = isnull(t.ClientId,0)
		where (@UserTypeId != 2 and @UserTypeId != 3)
	)
	, cteColumnFilter as 
	(
		select * 
		from cteUserTask
		where (@TaskName is null or TaskName like '%'+@TaskName+'%') 
		and (@UserName is null or UserName like '%'+@UserName+'%')
		and (@ClientName is null or ClientName like '%'+@ClientName+'%')
		and (@Description is null or [Description] like '%'+@Description+'%' )
		and (@Status is null or StatusId in (select [name] from dbo.splitstring(@Status)))
		and (@UserComment is null or UserComment like '%'+@UserComment+'%')
		and (@IsChargeble is null or IsChargeble = @IsChargeble)
		and (@Priorities is null or Priorities in (select [name] from dbo.splitstring(@Priorities)))
		and (@CreatedBy is null or @CreatedBy like '%'+CreatedByString+'%')
		and (@CreatedOnFrom is null or CreatedOn >= @CreatedOnFrom)
		and (@CreatedOnTo is null or CreatedOn <= @CreatedOnTo)
		and (@DueDateFrom is null or DueDate >= @DueDateFrom)
		and (@DueDateTo is null or DueDate <= @DueDateTo)
	)
	, cteUserTask_Count AS (Select COUNT(TaskId) AS TotalRecords FROM cteColumnFilter)

	SELECT * FROM cteColumnFilter, cteUserTask_Count
	ORDER BY
	-- then default order by taskid
	CASE WHEN @SortColumn='' THEN StatusId END asc,
	CASE WHEN @SortColumn='' THEN TaskId END desc,
	--asc by column
	CASE WHEN @SortColumn='TaskId' AND @SortOrder='asc' THEN TaskId END ASC,
	CASE WHEN @SortColumn='TaskUserId' AND @SortOrder='asc' THEN TaskUserId END ASC,
	CASE WHEN @SortColumn='UserComment' AND @SortOrder='asc' THEN UserComment END ASC,
	CASE WHEN @SortColumn='UserId' AND @SortOrder='asc' THEN UserId END ASC,
	CASE WHEN @SortColumn='Description' AND @SortOrder='asc' THEN [Description] END ASC,
	CASE WHEN @SortColumn='TaskName' AND @SortOrder='asc' THEN TaskName END ASC,
	CASE WHEN @SortColumn='DueDate' AND @SortOrder='asc' THEN DueDate END ASC,
	CASE WHEN @SortColumn='CreatedBy' AND @SortOrder='asc' THEN CreatedBy END ASC,
	CASE WHEN @SortColumn='CreatedByString' AND @SortOrder='asc' THEN CreatedByString END ASC,
	CASE WHEN @SortColumn='CreatedOn' AND @SortOrder='asc' THEN CreatedOn END ASC,
	CASE WHEN @SortColumn='UserName' AND @SortOrder='asc' THEN UserName END ASC,
	CASE WHEN @SortColumn='Priorities' AND @SortOrder='asc' THEN Priorities END ASC,
	CASE WHEN @SortColumn='StatusId' AND @SortOrder='asc' THEN StatusId END ASC,
	CASE WHEN @SortColumn='ClientName' AND @SortOrder='asc' THEN ClientName END ASC,
	CASE WHEN @SortColumn='IsChargeble' AND @SortOrder='asc' THEN IsChargeble END ASC,
	CASE WHEN @SortColumn='ChargeAmount' AND @SortOrder='asc' THEN ChargeAmount END ASC,
	--desc by columns
	CASE WHEN @SortColumn='TaskId' AND @SortOrder='desc' THEN TaskId END DESC,
	CASE WHEN @SortColumn='TaskUserId' AND @SortOrder='desc' THEN TaskUserId END DESC,
	CASE WHEN @SortColumn='UserComment' AND @SortOrder='desc' THEN UserComment END DESC,
	CASE WHEN @SortColumn='UserId' AND @SortOrder='desc' THEN UserId END DESC,
	CASE WHEN @SortColumn='Description' AND @SortOrder='desc' THEN [Description] END DESC,
	CASE WHEN @SortColumn='TaskName' AND @SortOrder='desc' THEN TaskName END DESC,
	CASE WHEN @SortColumn='DueDate' AND @SortOrder='desc' THEN DueDate END DESC,
	CASE WHEN @SortColumn='CreatedBy' AND @SortOrder='desc' THEN CreatedBy END DESC,
	CASE WHEN @SortColumn='CreatedByString' AND @SortOrder='desc' THEN CreatedByString END DESC,
	CASE WHEN @SortColumn='CreatedOn' AND @SortOrder='desc' THEN CreatedOn END DESC,
	CASE WHEN @SortColumn='UserName' AND @SortOrder='desc' THEN UserName END DESC,
	CASE WHEN @SortColumn='Priorities' AND @SortOrder='desc' THEN Priorities END DESC,
	CASE WHEN @SortColumn='StatusId' AND @SortOrder='desc' THEN StatusId END DESC,
	CASE WHEN @SortColumn='ClientName' AND @SortOrder='desc' THEN ClientName END DESC,
	CASE WHEN @SortColumn='IsChargeble' AND @SortOrder='desc' THEN IsChargeble END DESC,
	CASE WHEN @SortColumn='ChargeAmount' AND @SortOrder='desc' THEN ChargeAmount END DESC

	OFFSET @RecordFrom ROWS 
	FETCH NEXT CASE @PageSize WHEN -1 THEN 5000 ELSE @PageSize END ROWS ONLY

END
