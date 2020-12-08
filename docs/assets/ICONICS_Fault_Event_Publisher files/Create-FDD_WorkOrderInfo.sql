
CREATE TABLE [dbo].[FDD_WorkOrderInfo](
	[AssetPath] [nvarchar](255) NULL,
	[FaultActiveTime] [datetime] NULL,
	[FaultName] [nvarchar](max) NULL,
	[WorkOrderCreatedOn] [datetime] NULL,
	[WorkOrderId] [nvarchar](255) NULL,
	[WorkOrderModifiedOn] [datetime] NULL,
	[WorkOrderStatus] [nvarchar](50) NULL,
	[WorkOrderUrl] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


