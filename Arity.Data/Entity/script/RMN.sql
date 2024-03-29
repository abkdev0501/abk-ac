USE RMN
GO

/****** Object:  Table [dbo].[Client_Particular_Mapping]    Script Date: 03/15/2020 10:17:37 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Client_Particular_Mapping](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CompanyId] [int] NULL,
	[particularId] [int] NULL,
 CONSTRAINT [PK_Client_Particular_Mapping] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Company_Client_Mapping]    Script Date: 03/15/2020 10:17:37 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Company_Client_Mapping](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CompanyId] [int] NULL,
	[UserId] [int] NULL,
 CONSTRAINT [PK_Company_Client_Mapping] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Company_master]    Script Date: 03/15/2020 10:17:37 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Company_master](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CompanyName] [varchar](max) NULL,
	[CompanyBanner] [varchar](max) NULL,
	[Address] [varchar](max) NULL,
	[IsActive] [bit] NULL,
	[Type] [varchar](max) NULL,
	[PreferedColor] [int] NULL,
	[Prefix] [varchar](max) NULL,
 CONSTRAINT [PK_Company] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Consultants]    Script Date: 03/15/2020 10:17:37 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Consultants](
	[ConsultantId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](1000) NOT NULL,
	[Address] [varchar](max) NULL,
	[City] [varchar](1000) NULL,
	[Mobile] [varchar](100) NULL,
	[Notes] [varchar](max) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [int] NULL,
 CONSTRAINT [PK_Consultants] PRIMARY KEY CLUSTERED 
(
	[ConsultantId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DocumentMaster]    Script Date: 03/15/2020 10:17:37 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DocumentMaster](
	[DocumentId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](max) NOT NULL,
	[ClientId] [int] NOT NULL,
	[DocumentType] [int] NOT NULL,
	[Status] [int] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[ModifiedBy] [int] NULL,
	[ModifiedOn] [datetime] NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_DocumentMaster] PRIMARY KEY CLUSTERED 
(
	[DocumentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InvoiceDetails]    Script Date: 03/15/2020 10:17:37 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InvoiceDetails](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Invoice_Number] [nvarchar](50) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[ClientId] [bigint] NOT NULL,
	[CompanyId] [bigint] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[InvoiceDate] [datetime] NOT NULL,
 CONSTRAINT [PK_InvoiceDetails] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InvoiceParticulars]    Script Date: 03/15/2020 10:17:37 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InvoiceParticulars](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[InvoiceId] [bigint] NOT NULL,
	[ParticularId] [bigint] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[year] [nvarchar](50) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL,
 CONSTRAINT [PK_InvoiceParticulars] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InvoiceReciepts]    Script Date: 03/15/2020 10:17:37 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InvoiceReciepts](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[InvoiceId] [bigint] NULL,
	[RecieptId] [bigint] NULL,
	[CreatedBy] [int] NOT NULL,
 CONSTRAINT [PK_InvoiceRecieptMapping] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InvoiceTrackings]    Script Date: 03/15/2020 10:17:37 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InvoiceTrackings](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[InvoiceId] [int] NOT NULL,
	[UserId] [int] NULL,
	[Comment] [nvarchar](max) NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedBy] [int] NOT NULL,
 CONSTRAINT [PK_InvoiceTracking] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Particulars]    Script Date: 03/15/2020 10:17:37 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Particulars](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ParticularSF] [nvarchar](50) NOT NULL,
	[ParticularFF] [nvarchar](350) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[IsExclude] [bit] NULL,
	[CreatedBy] [int] NOT NULL,
 CONSTRAINT [PK_Particulars] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RecieptDetails]    Script Date: 03/15/2020 10:17:37 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecieptDetails](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[RecieptNo] [nvarchar](50) NOT NULL,
	[TotalAmount] [decimal](18, 2) NOT NULL,
	[Discount] [decimal](18, 2) NOT NULL,
	[BankName] [nvarchar](100) NULL,
	[ChequeNumber] [nvarchar](50) NULL,
	[Status] [bit] NULL,
	[UpdatedDate] [datetime] NOT NULL,
	[RecieptDate] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL,
 CONSTRAINT [PK_RecieptDetails] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Roles]    Script Date: 03/15/2020 10:17:37 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Roles](
	[RoleName] [nvarchar](150) NOT NULL,
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tasks]    Script Date: 03/15/2020 10:17:37 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tasks](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](1000) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[Status] [int] NOT NULL,
	[ModifiedOn] [datetime] NULL,
	[Active] [bit] NOT NULL,
	[ClientId] [int] NULL,
	[Priorities] [int] NULL,
	[Remarks] [varchar](max) NULL,
	[IsChargeble] [bit] NULL,
	[CompletedOn] [datetime] NULL,
	[AddedBy] [int] NOT NULL,
 CONSTRAINT [PK_Task] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[User_Role]    Script Date: 03/15/2020 10:17:37 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User_Role](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [bigint] NOT NULL,
	[RoleId] [bigint] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_User_Role] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 03/15/2020 10:17:37 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[Username] [nvarchar](100) NOT NULL,
	[Password] [nvarchar](50) NOT NULL,
	[FullName] [nvarchar](150) NOT NULL,
	[Address] [nvarchar](250) NOT NULL,
	[City] [nvarchar](50) NOT NULL,
	[PhoneNumber] [nvarchar](20) NULL,
	[Pincode] [nvarchar](10) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDated] [datetime] NOT NULL,
	[UserTypeId] [bigint] NOT NULL,
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Email] [varchar](100) NULL,
	[Active] [bit] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[GroupId] [int] NULL,
	[MasterMobile] [varchar](100) NULL,
	[AccountantName] [varchar](1000) NULL,
	[AccountantMobile] [varchar](100) NULL,
	[ContactPerson] [varchar](1000) NULL,
	[ConsultantId] [int] NULL,
	[Remarks] [varchar](max) NULL,
	[BusinessType] [int] NULL,
	[BusinessStatus] [int] NULL,
	[PanNumber] [varchar](1000) NULL,
	[TANNumber] [varchar](1000) NULL,
	[GSTIN] [varchar](1000) NULL,
	[EFFDate] [datetime] NULL,
	[JURISDICTION] [varchar](1000) NULL,
	[RTNType] [varchar](max) NULL,
	[CommodityName] [varchar](max) NULL,
	[CommodityHSN] [varchar](1000) NULL,
	[GSTRate] [varchar](max) NULL,
	[ApplicableRate] [datetime] NULL,
	[ServiceTypes] [int] NULL,
	[Rates] [varchar](1000) NULL,
 CONSTRAINT [PK_UserMaster] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserTasks]    Script Date: 03/15/2020 10:17:37 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserTasks](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[TaskId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[Comment] [nvarchar](max) NULL,
	[Status] [int] NOT NULL,
	[Active] [bit] NOT NULL,
	[ModifiedOn] [datetime] NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[DueDate] [datetime] NULL,
	[AddedBy] [int] NOT NULL,
 CONSTRAINT [PK_UserTask] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserTypes]    Script Date: 03/15/2020 10:17:37 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserTypes](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UserTypeName] [nvarchar](50) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_UserTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Company_Client_Mapping] ON 

INSERT [dbo].[Company_Client_Mapping] ([Id], [CompanyId], [UserId]) VALUES (1, 1, 5)
INSERT [dbo].[Company_Client_Mapping] ([Id], [CompanyId], [UserId]) VALUES (2, 2, 5)
SET IDENTITY_INSERT [dbo].[Company_Client_Mapping] OFF
SET IDENTITY_INSERT [dbo].[Company_master] ON 

INSERT [dbo].[Company_master] ([Id], [CompanyName], [CompanyBanner], [Address], [IsActive], [Type], [PreferedColor], [Prefix]) VALUES (1, N'R. M. Narshana', NULL, N'1, Prerana Apartment, 2, Talav Street, Junagadh.', 1, N'Tax Consultant', 1, NULL)
INSERT [dbo].[Company_master] ([Id], [CompanyName], [CompanyBanner], [Address], [IsActive], [Type], [PreferedColor], [Prefix]) VALUES (2, N'Nilesh Narshana & Associates', NULL, N'1, Prerana Apartment, 2, Talav Street, Junagadh.', 1, N'Chartered Accountants', 2, NULL)
SET IDENTITY_INSERT [dbo].[Company_master] OFF
SET IDENTITY_INSERT [dbo].[Particulars] ON 

INSERT [dbo].[Particulars] ([Id], [ParticularSF], [ParticularFF], [CreatedDate], [UpdatedDate], [IsExclude], [CreatedBy]) VALUES (1, N'IT', N'Incom Tax', CAST(N'2020-02-16T21:36:20.873' AS DateTime), CAST(N'2020-02-16T21:36:20.873' AS DateTime), 0, 0)
INSERT [dbo].[Particulars] ([Id], [ParticularSF], [ParticularFF], [CreatedDate], [UpdatedDate], [IsExclude], [CreatedBy]) VALUES (2, N'GST', N'Goods Tax', CAST(N'2020-02-16T21:36:44.297' AS DateTime), CAST(N'2020-02-16T21:36:44.297' AS DateTime), 0, 0)
INSERT [dbo].[Particulars] ([Id], [ParticularSF], [ParticularFF], [CreatedDate], [UpdatedDate], [IsExclude], [CreatedBy]) VALUES (3, N'PaperFee', N'Paper Fees', CAST(N'2020-02-16T21:37:05.863' AS DateTime), CAST(N'2020-02-16T21:39:13.487' AS DateTime), 1, 0)
SET IDENTITY_INSERT [dbo].[Particulars] OFF
SET IDENTITY_INSERT [dbo].[Roles] ON 

INSERT [dbo].[Roles] ([RoleName], [Id]) VALUES (N'MasterAdmin', 1)
INSERT [dbo].[Roles] ([RoleName], [Id]) VALUES (N'Admin', 2)
INSERT [dbo].[Roles] ([RoleName], [Id]) VALUES (N'User', 10002)
SET IDENTITY_INSERT [dbo].[Roles] OFF
SET IDENTITY_INSERT [dbo].[Tasks] ON 

INSERT [dbo].[Tasks] ([id], [Name], [Description], [CreatedBy], [CreatedOn], [Status], [ModifiedOn], [Active], [ClientId], [Priorities], [Remarks], [IsChargeble], [CompletedOn], [AddedBy]) VALUES (1, N'task 1', N'work on this', 1, CAST(N'2020-03-14T23:50:07.190' AS DateTime), 2, CAST(N'2020-03-14T23:54:08.487' AS DateTime), 0, 5, 3, NULL, 1, NULL, 1)
SET IDENTITY_INSERT [dbo].[Tasks] OFF
SET IDENTITY_INSERT [dbo].[User_Role] ON 

INSERT [dbo].[User_Role] ([Id], [UserId], [RoleId], [CreatedDate], [UpdatedDate]) VALUES (1, 1, 1, CAST(N'2020-12-12T00:00:00.000' AS DateTime), CAST(N'2020-01-12T16:05:30.670' AS DateTime))
INSERT [dbo].[User_Role] ([Id], [UserId], [RoleId], [CreatedDate], [UpdatedDate]) VALUES (6, 10007, 2, CAST(N'2020-01-29T02:21:19.767' AS DateTime), CAST(N'2020-01-29T02:21:19.767' AS DateTime))
INSERT [dbo].[User_Role] ([Id], [UserId], [RoleId], [CreatedDate], [UpdatedDate]) VALUES (7, 10008, 10002, CAST(N'2020-02-16T22:40:41.937' AS DateTime), CAST(N'2020-02-16T22:40:41.937' AS DateTime))
INSERT [dbo].[User_Role] ([Id], [UserId], [RoleId], [CreatedDate], [UpdatedDate]) VALUES (8, 4, 2, CAST(N'2020-02-23T18:45:17.237' AS DateTime), CAST(N'2020-02-23T18:45:17.237' AS DateTime))
INSERT [dbo].[User_Role] ([Id], [UserId], [RoleId], [CreatedDate], [UpdatedDate]) VALUES (9, 4, 2, CAST(N'2020-02-23T18:45:17.237' AS DateTime), CAST(N'2020-02-23T18:45:17.237' AS DateTime))
INSERT [dbo].[User_Role] ([Id], [UserId], [RoleId], [CreatedDate], [UpdatedDate]) VALUES (10, 5, 3, CAST(N'2020-03-14T22:59:56.570' AS DateTime), CAST(N'2020-03-14T22:59:56.570' AS DateTime))
INSERT [dbo].[User_Role] ([Id], [UserId], [RoleId], [CreatedDate], [UpdatedDate]) VALUES (11, 5, 3, CAST(N'2020-03-14T22:59:56.573' AS DateTime), CAST(N'2020-03-14T22:59:56.573' AS DateTime))
SET IDENTITY_INSERT [dbo].[User_Role] OFF
SET IDENTITY_INSERT [dbo].[Users] ON 

INSERT [dbo].[Users] ([Username], [Password], [FullName], [Address], [City], [PhoneNumber], [Pincode], [CreatedDate], [UpdatedDated], [UserTypeId], [Id], [Email], [Active], [CreatedBy], [GroupId], [MasterMobile], [AccountantName], [AccountantMobile], [ContactPerson], [ConsultantId], [Remarks], [BusinessType], [BusinessStatus], [PanNumber], [TANNumber], [GSTIN], [EFFDate], [JURISDICTION], [RTNType], [CommodityName], [CommodityHSN], [GSTRate], [ApplicableRate], [ServiceTypes], [Rates]) VALUES (N'admin', N'wAFT6KoRyDM=', N'Master Admin', N'Junagadh', N'Junagadh', NULL, NULL, CAST(N'2020-02-23T00:00:00.000' AS DateTime), CAST(N'2020-02-23T00:00:00.000' AS DateTime), 1, 1, N'admin@mail.com', 1, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[Users] ([Username], [Password], [FullName], [Address], [City], [PhoneNumber], [Pincode], [CreatedDate], [UpdatedDated], [UserTypeId], [Id], [Email], [Active], [CreatedBy], [GroupId], [MasterMobile], [AccountantName], [AccountantMobile], [ContactPerson], [ConsultantId], [Remarks], [BusinessType], [BusinessStatus], [PanNumber], [TANNumber], [GSTIN], [EFFDate], [JURISDICTION], [RTNType], [CommodityName], [CommodityHSN], [GSTRate], [ApplicableRate], [ServiceTypes], [Rates]) VALUES (N'nirav', N'ND9p25oCkmU+30gk6LKJSA==', N'nirav', N'abd', N'abd', NULL, NULL, CAST(N'2020-02-23T18:45:17.180' AS DateTime), CAST(N'2020-02-23T18:45:17.177' AS DateTime), 2, 4, N'nirav@mail.com', 1, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[Users] ([Username], [Password], [FullName], [Address], [City], [PhoneNumber], [Pincode], [CreatedDate], [UpdatedDated], [UserTypeId], [Id], [Email], [Active], [CreatedBy], [GroupId], [MasterMobile], [AccountantName], [AccountantMobile], [ContactPerson], [ConsultantId], [Remarks], [BusinessType], [BusinessStatus], [PanNumber], [TANNumber], [GSTIN], [EFFDate], [JURISDICTION], [RTNType], [CommodityName], [CommodityHSN], [GSTRate], [ApplicableRate], [ServiceTypes], [Rates]) VALUES (N'bk', N'b/+XkvqRyHF84doqCMvTKw==', N'Bk', N'test', N'test', NULL, NULL, CAST(N'2020-03-14T22:59:56.393' AS DateTime), CAST(N'2020-03-14T22:59:56.393' AS DateTime), 3, 5, N'bk@mail.com', 1, 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
SET IDENTITY_INSERT [dbo].[Users] OFF
SET IDENTITY_INSERT [dbo].[UserTasks] ON 

INSERT [dbo].[UserTasks] ([id], [TaskId], [UserId], [Comment], [Status], [Active], [ModifiedOn], [CreatedBy], [CreatedOn], [DueDate], [AddedBy]) VALUES (2, 2, 2, N'i just complete this task', 1, 0, CAST(N'2020-02-16T21:03:30.033' AS DateTime), 1, CAST(N'2020-02-16T20:02:29.937' AS DateTime), CAST(N'2020-01-01T00:00:00.000' AS DateTime), 1)
INSERT [dbo].[UserTasks] ([id], [TaskId], [UserId], [Comment], [Status], [Active], [ModifiedOn], [CreatedBy], [CreatedOn], [DueDate], [AddedBy]) VALUES (3, 3, 10007, NULL, 2, 1, NULL, 1, CAST(N'2020-02-16T21:06:29.260' AS DateTime), NULL, 1)
INSERT [dbo].[UserTasks] ([id], [TaskId], [UserId], [Comment], [Status], [Active], [ModifiedOn], [CreatedBy], [CreatedOn], [DueDate], [AddedBy]) VALUES (4, 4, 10007, NULL, 1, 0, NULL, 10002, CAST(N'2020-02-16T21:19:19.280' AS DateTime), CAST(N'2020-02-01T00:00:00.000' AS DateTime), 1)
INSERT [dbo].[UserTasks] ([id], [TaskId], [UserId], [Comment], [Status], [Active], [ModifiedOn], [CreatedBy], [CreatedOn], [DueDate], [AddedBy]) VALUES (5, 1, 4, NULL, 2, 0, CAST(N'2020-03-14T23:54:08.500' AS DateTime), 1, CAST(N'2020-03-14T23:50:07.220' AS DateTime), NULL, 1)
SET IDENTITY_INSERT [dbo].[UserTasks] OFF
SET IDENTITY_INSERT [dbo].[UserTypes] ON 

INSERT [dbo].[UserTypes] ([Id], [UserTypeName], [CreatedDate], [UpdatedDate]) VALUES (1, N'MasterAdmin', CAST(N'2020-02-23T00:00:00.000' AS DateTime), CAST(N'2020-02-23T00:00:00.000' AS DateTime))
INSERT [dbo].[UserTypes] ([Id], [UserTypeName], [CreatedDate], [UpdatedDate]) VALUES (2, N'Admin Staff', CAST(N'2020-02-23T00:00:00.000' AS DateTime), CAST(N'2020-02-23T00:00:00.000' AS DateTime))
INSERT [dbo].[UserTypes] ([Id], [UserTypeName], [CreatedDate], [UpdatedDate]) VALUES (3, N'Client', CAST(N'2020-02-23T00:00:00.000' AS DateTime), CAST(N'2020-02-23T00:00:00.000' AS DateTime))
SET IDENTITY_INSERT [dbo].[UserTypes] OFF
ALTER TABLE [dbo].[InvoiceDetails] ADD  CONSTRAINT [DF_InvoiceDetails_UpdatedDate]  DEFAULT (getdate()) FOR [UpdatedDate]
GO
ALTER TABLE [dbo].[InvoiceParticulars] ADD  CONSTRAINT [DF_InvoiceParticulars_Amount]  DEFAULT ((0)) FOR [Amount]
GO
ALTER TABLE [dbo].[InvoiceParticulars] ADD  CONSTRAINT [DF_InvoiceParticulars_UpdatedDate]  DEFAULT (getdate()) FOR [UpdatedDate]
GO
ALTER TABLE [dbo].[Particulars] ADD  CONSTRAINT [DF_Particulars_UpdatedDate]  DEFAULT (getdate()) FOR [UpdatedDate]
GO
ALTER TABLE [dbo].[RecieptDetails] ADD  CONSTRAINT [DF_RecieptDetails_UpdatedDate]  DEFAULT (getdate()) FOR [UpdatedDate]
GO
ALTER TABLE [dbo].[User_Role] ADD  CONSTRAINT [DF_User_Role_UpdatedDate]  DEFAULT (getdate()) FOR [UpdatedDate]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Table_1_UpdateDated]  DEFAULT (getdate()) FOR [UpdatedDated]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_Active]  DEFAULT ((0)) FOR [Active]
GO
ALTER TABLE [dbo].[UserTasks] ADD  CONSTRAINT [DF_UserTasks_AddedBy]  DEFAULT ((1)) FOR [AddedBy]
GO
ALTER TABLE [dbo].[UserTypes] ADD  CONSTRAINT [DF_UserTypes_UpdatedDate]  DEFAULT (getdate()) FOR [UpdatedDate]
GO
USE [master]
GO
ALTER DATABASE [RMN] SET  READ_WRITE 
GO
