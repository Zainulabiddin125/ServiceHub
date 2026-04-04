-- Run this script against your ServiceHub database to create the MachineFormatLogs table.

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_NAME = 'MachineFormatLogs'
)
BEGIN
    CREATE TABLE MachineFormatLogs (
        Id                   INT            IDENTITY(1,1) PRIMARY KEY,
        MachineId            INT            NOT NULL,
        MachineIP            NVARCHAR(50)   NOT NULL,
        MachineName          NVARCHAR(200)  NULL,
        Status               NVARCHAR(50)   NOT NULL DEFAULT 'Success',
        ErrorMessage         NVARCHAR(MAX)  NULL,
        RequestedByUserId    NVARCHAR(450)  NULL,
        RequestedByUserName  NVARCHAR(256)  NULL,
        RequestedAt          DATETIME2      NOT NULL DEFAULT GETDATE(),
        ExecutedAt           DATETIME2      NULL,

        CONSTRAINT FK_MachineFormatLogs_Machine
            FOREIGN KEY (MachineId) REFERENCES AttendenceMachines(Id)
    );

    CREATE INDEX IX_MachineFormatLogs_MachineId
        ON MachineFormatLogs (MachineId);

    PRINT 'MachineFormatLogs table created successfully.';
END
ELSE
BEGIN
    PRINT 'MachineFormatLogs table already exists — skipped.';
END
