
/****** Object:  StoredProcedure [rmncolxw].[dbo.GetAllUsers]    Script Date: 22-09-2022 20:09:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Prakash Ahir
-- Create date: 22-09-22
-- Description:	Get All Users
-- =============================================
CREATE PROCEDURE [dbo].[GetAllUsers]
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		U.Id,
		Address,
		City,
		Pincode,
		FullName,
		PhoneNumber,
		Username,
		UserTypeName,
		Email,
		Active,
		UserTypeId,
		CreatedBy
	FROM Users U
	JOIN UserTypes UT ON U.UserTypeId = UT.Id

END
