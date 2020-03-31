# Jobsity Challenge

In order to be able to run this project, a MS-Sql Database needs to be created.
And the following table needs to be created:

    CREATE TABLE [dbo].[tbl_Users]
    (    
        [ID]        [int] IDENTITY(1,1) NOT NULL,  
        [UserName]  [varchar](50) NULL,  
        [Email]     [varchar](50) NULL,  
        [Password]  [varchar](50) NULL,  
        [Photo]     [varchar](50) NULL,  
      
        CONSTRAINT [PK_tbl_Users] PRIMARY KEY CLUSTERED  
        (  
            [ID] ASC  
        )
        WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]  
    ) ON [PRIMARY]

After the table is created, the **web.config** file needs to be updated.
Also, for setting the number of messages that will be cached by the application, there a setting in the same file: ***msgBufferSize***
In the front end, inside the function *RemoveOlderMessages* there is a variable (***maxAllowed***) that determines the total messages to be displayed in the screen.
