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
-- Description:	Document List
-- =============================================
CREATE PROCEDURE [dbo].[DocumentList] 
	-- Add the parameters for the stored procedure here
	@UserId int,
	@UserTypeId int,
	@RecordFrom int,
	@PageSize int,
	@SortColumn nvarchar(max) = '',
	@SortOrder nvarchar(max) = '',
	@Name nvarchar(max) = null,
	@ClientName nvarchar(max) = null,
	@UserName nvarchar(max) = null,
	@DocumentTypeName nvarchar(max) = null,
	@StatusName nvarchar(max) = null,
	@CreatedBy nvarchar(max) =  null,
	@Status bit = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	;with cteDocuments as
	(
		select 
			DocumentId,
			dm.[Name],
			ClientId,
			dt.[Name] as DocumentTypeName,
			dm.IsActive,
			dm.CreatedBy,
			dm.CreatedOn,
			uc.FullName as CreatedByString,
			uc.UserTypeId as AddedBy,
			dm.[Status],
			u.FullName as ClientName,
			u.Username as UserName,
			[FileName]
		from DocumentMasters dm 
		join Users u on dm.ClientId = u.Id
		join Users uc on dm.CreatedBy = uc.Id
		left join DocumentTypes dt on dt.DocumnetTypeId = dm.DocumentType
		where (@UserTypeId = 3 and dm.ClientId = @UserId) or @UserTypeId != 3
	),
	cteDocumentResult as
	(
		select 
		*
		from cteDocuments 
		where (@Name is null or [Name] like '%'+@Name+'%')
		and (@ClientName is null or ClientName like '%'+@ClientName+'%')
		and (@UserName is null or UserName like '%'+@UserName+'%')
		and (@DocumentTypeName is null or DocumentTypeName like '%'+@DocumentTypeName+'%')
		and (@StatusName is null or [Status] in (select [name] from dbo.splitstring(@StatusName))) 
		and (@CreatedBy is null or CreatedBy like '%'+@CreatedBy+'%')
		and (@Status is null or IsActive = @Status)
	)
	, CteDoc_Count AS (Select COUNT(DocumentId) AS TotalRecords FROM cteDocumentResult)

	SELECT * FROM cteDocumentResult, CteDoc_Count
	ORDER BY
	-- then default order by document id
	CASE WHEN @SortColumn='' or @SortColumn is null THEN DocumentId END desc,
	CASE WHEN @SortColumn='Name' AND @SortOrder='Asc' THEN [Name] END ASC,
	CASE WHEN @SortColumn='ClientName' AND @SortOrder='asc' THEN ClientName END ASC,
	CASE WHEN @SortColumn='CreatedBy' AND @SortOrder='asc' THEN CreatedBy END ASC,
	CASE WHEN @SortColumn='UserName' AND @SortOrder='asc' THEN UserName END ASC,
	CASE WHEN @SortColumn='DocumentTypeName' AND @SortOrder='asc' THEN DocumentTypeName END ASC,
	CASE WHEN @SortColumn='StatusName' AND @SortOrder='asc' THEN [Status] END ASC,

	CASE WHEN @SortColumn='Name' AND @SortOrder='Desc' THEN [Name] END DESC,
	CASE WHEN @SortColumn='ClientName' AND @SortOrder='desc' THEN ClientName END DESC,
	CASE WHEN @SortColumn='CreatedBy' AND @SortOrder='desc' THEN CreatedBy END DESC,
	CASE WHEN @SortColumn='UserName' AND @SortOrder='desc' THEN UserName END DESC,
	CASE WHEN @SortColumn='DocumentTypeName' AND @SortOrder='desc' THEN DocumentTypeName END DESC,
	CASE WHEN @SortColumn='StatusName' AND @SortOrder='desc' THEN [Status] END DESC

	OFFSET @RecordFrom ROWS 
	FETCH NEXT CASE @PageSize WHEN -1 THEN 5000 ELSE @PageSize END ROWS ONLY

END
GO
