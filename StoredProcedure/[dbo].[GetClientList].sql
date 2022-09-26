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
-- Description:	GetClientList
-- =============================================
ALTER PROCEDURE [dbo].[GetClientList]
	-- Add the parameters for the stored procedure here
	@UserTypeId int,
	@UserId int,
	@RecordFrom int,
	@PageSize int,
	@SortColumn nvarchar(max) = '',
	@SortOrder nvarchar(max) = '',
	@FullName nvarchar(max) = null,
	@UserName nvarchar(max) = null,
	@Address nvarchar(max) = null,
	@City nvarchar(max) = null,
	@PhoneNumber nvarchar(max) = null,
	@Pincode nvarchar(max) = null,
	@GroupName nvarchar(max) = null,
	@AccountantName nvarchar(max) = null,
	@Status bit = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	;with cteClients as
	(
		select 
			u.Id,
			u.[Address],
			u.City,
			u.Pincode,
			u.FullName,
			u.PhoneNumber,
			u.Username,
			ut.UserTypeName as UserType,
			u.Email,
			u.Active,
			u.UserTypeId,
			u.CreatedBy,
			u.AccountantName,
			gm.[Name] as GroupName
		from Users u 
		join UserTypes ut on u.UserTypeId = ut.Id
		left join GroupMasters gm on u.GroupId = gm.GroupId
		where ((@UserTypeId = 4 and u.UserTypeId = 3 and u.ConsultantId = @UserId) or
		( @UserTypeId != 4 and u.UserTypeId = 3 ))
	)
	, cteClientResult as
	(
		select 
		* 
		from cteClients
		where  (@FullName is null or FullName like '%'+@FullName+'%') 
		and (@UserName is null or Username like '%'+@UserName+'%')
		and (@Address is null or [Address] like '%'+@Address+'%')
		and (@City is null or City like '%'+@City+'%')
		and (@PhoneNumber is null or PhoneNumber like '%'+@PhoneNumber+'%')
		and (@Pincode is null or Pincode like '%'+@Pincode+'%')
		and (@GroupName is null or GroupName like '%'+@GroupName+'%')
		and (@AccountantName is null or AccountantName like '%'+@AccountantName+'%')
		and (@Status is null or Active = @Status)
	)
	, CteClient_Count AS (Select COUNT(Id) AS TotalRecords FROM cteClientResult)

	SELECT * FROM cteClientResult, CteClient_Count
	ORDER BY
	-- then default order by taskid
	CASE WHEN @SortColumn='' or @SortColumn is null THEN id END desc,
	CASE WHEN @SortColumn='FullName' AND @SortOrder='Asc' THEN [FullName] END ASC,
	CASE WHEN @SortColumn='Username' AND @SortOrder='asc' THEN Username END ASC,
	CASE WHEN @SortColumn='Address' AND @SortOrder='asc' THEN [Address] END ASC,
	CASE WHEN @SortColumn='City' AND @SortOrder='asc' THEN [City] END ASC,
	CASE WHEN @SortColumn='Pincode' AND @SortOrder='asc' THEN Pincode END ASC,
	CASE WHEN @SortColumn='GroupName' AND @SortOrder='asc' THEN GroupName END ASC,
	CASE WHEN @SortColumn='AccountantName' AND @SortOrder='asc' THEN AccountantName END ASC,

	CASE WHEN @SortColumn='FullName' AND @SortOrder='Desc' THEN [FullName] END DESC,
	CASE WHEN @SortColumn='Username' AND @SortOrder='desc' THEN Username END DESC,
	CASE WHEN @SortColumn='Address' AND @SortOrder='desc' THEN [Address] END DESC,
	CASE WHEN @SortColumn='City' AND @SortOrder='desc' THEN [City] END DESC,
	CASE WHEN @SortColumn='Pincode' AND @SortOrder='desc' THEN Pincode END DESC,
	CASE WHEN @SortColumn='GroupName' AND @SortOrder='desc' THEN GroupName END DESC,
	CASE WHEN @SortColumn='AccountantName' AND @SortOrder='desc' THEN AccountantName END DESC

	OFFSET @RecordFrom ROWS 
	FETCH NEXT CASE @PageSize WHEN -1 THEN 5000 ELSE @PageSize END ROWS ONLY
	
    
END
GO
