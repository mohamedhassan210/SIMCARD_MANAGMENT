/* ============================================================================
   SIM CARD MANAGEMENT - SEED DATA SCRIPT (updated for INT identity PKs)
   ============================================================================
   Table names follow EF Core's default Code-First pluralization:
   Group -> Groups, User -> Users, Permission -> Permissions,
   GroupPermission -> GroupPermissions, ServiceProvider -> ServiceProviders,
   DocumentType -> DocumentTypes, Document -> Documents,
   DeviceAction -> Actions, Employee -> Employees,
   NonEmployee -> NonEmployees, Quota -> Quotas, Sim -> Sims, Usb -> Usbs,
   Serial -> Serials, Subscription -> Subscriptions,
   DeviceTransfer -> DeviceTransfers, DeviceStatus -> DeviceStatuses,
   ReceiverSignature -> ReceiverSignatures, AuditLog -> AuditLogs,
   UserOtp -> UserOtps.

   All PKs are now INT IDENTITY columns instead of UNIQUEIDENTIFIER, so we
   no longer set explicit Id values or use NEWID(). Instead, OUTPUT clauses
   capture generated IDs into table variables so we can reuse them for FK
   references later in the script (e.g. ServiceProviderId on Quotas/Sims/Usbs).
   ============================================================================ */

SET NOCOUNT ON;
BEGIN TRANSACTION;

BEGIN TRY

/* ----------------------------------------------------------------------
   1. Groups
   ---------------------------------------------------------------------- */
DECLARE @GroupTbl TABLE (Id INT);

INSERT INTO Groups (Name, Description, CreatedAt)
OUTPUT INSERTED.Id INTO @GroupTbl
VALUES (N'Administrators', N'ãÌãæÚÉ ÅÏÇÑÉ ÇáäÙÇã', '2026-07-08 14:25:04.330');

DECLARE @GroupAdminId INT = (SELECT TOP 1 Id FROM @GroupTbl);

/* ----------------------------------------------------------------------
   2. Users
      Per instructions: LastLogin and CreatedAt use "now" (GETDATE())
      instead of the historical timestamps supplied.
   ---------------------------------------------------------------------- */
DECLARE @UserTbl TABLE (Id INT);

INSERT INTO Users (Username, PasswordHash, Email, GroupId, LastLogin, IsActive, CreatedAt)
OUTPUT INSERTED.Id INTO @UserTbl
VALUES (N'Youssef', '$2a$11$n4Ky8Lw08pzk/Cks22QxvOPbHBrBo1YKv6d1tBET28ndjAxlicOl2',
        N'jooyoyo1234@gmail.com', @GroupAdminId, GETDATE(), 1, GETDATE());

DECLARE @UserYoussefId INT = (SELECT TOP 1 Id FROM @UserTbl);

/* ----------------------------------------------------------------------
   3. Permissions
   ---------------------------------------------------------------------- */
DECLARE @PermTbl TABLE (Id INT);

INSERT INTO Permissions (ActionName, ControllerName, Description)
OUTPUT INSERTED.Id INTO @PermTbl
VALUES
(N'Delete',  N'SIM',         N'SIM.Delete'),
(N'Delete',  N'NonEmployee', N'NonEmployee.Delete'),
(N'Create',  N'NonEmployee', N'NonEmployee.Create'),
(N'Edit',    N'SIM',         N'SIM.Edit'),
(N'Details', N'Employee',    N'Employee.Details'),
(N'Delete',  N'Employee',    N'Employee.Delete'),
(N'Create',  N'SIM',         N'SIM.Create'),
(N'Index',   N'NonEmployee', N'NonEmployee.Index'),
(N'Index',   N'Employee',    N'Employee.Index'),
(N'Details', N'NonEmployee', N'NonEmployee.Details'),
(N'Edit',    N'Employee',    N'Employee.Edit'),
(N'Index',   N'SIM',         N'SIM.Index'),
(N'Create',  N'Employee',    N'Employee.Create'),
(N'Edit',    N'NonEmployee', N'NonEmployee.Edit');

/* ----------------------------------------------------------------------
   4. GroupPermissions
      Grant every permission just created to the Administrators group.
   ---------------------------------------------------------------------- */
INSERT INTO GroupPermissions (GroupId, PermissionId)
SELECT @GroupAdminId, Id FROM @PermTbl;

/* ----------------------------------------------------------------------
   5. ServiceProviders (the 4 known Egyptian providers)
   ---------------------------------------------------------------------- */
DECLARE @SPTbl TABLE (Id INT, Name NVARCHAR(100));

INSERT INTO ServiceProviders (Name, DisplayName, IsActive)
OUTPUT INSERTED.Id, INSERTED.Name INTO @SPTbl
VALUES
(N'Orange',   N'ÃæÑäÌ',    1),
(N'WE',       N'æí',       1),
(N'Vodafone', N'ÝæÏÇÝæä',  1),
(N'Etisalat', N'ÇÊÕÇáÇÊ',  1);

DECLARE @OrangeId   INT = (SELECT Id FROM @SPTbl WHERE Name = N'Orange');
DECLARE @WEId       INT = (SELECT Id FROM @SPTbl WHERE Name = N'WE');
DECLARE @VodafoneId INT = (SELECT Id FROM @SPTbl WHERE Name = N'Vodafone');
DECLARE @EtisalatId INT = (SELECT Id FROM @SPTbl WHERE Name = N'Etisalat');

/* ----------------------------------------------------------------------
   6. DocumentTypes
   ---------------------------------------------------------------------- */
INSERT INTO DocumentTypes (Name, DisplayName) VALUES
(N'Return',  N'ÅÐä ÇÓÊáÇã ãÑÊÌÚ'),
(N'Receipt', N'ÅÐä ÇÓÊáÇã ÌÏíÏ');

/* ----------------------------------------------------------------------
   7. DeviceActions (table: Actions)
   ---------------------------------------------------------------------- */
INSERT INTO Actions (Name, ActionStatus, Description) VALUES
(N'Assign Asset', N'Active', N'Initial allocation of line/device');

/* ----------------------------------------------------------------------
   8. Employees (10, different names/positions, not linked to any User)
   ---------------------------------------------------------------------- */
INSERT INTO Employees (Name, Position, NationalID, UserId, CreatedAt, EmpCode, IsActive) VALUES
(N'Ahmed Mohamed El-Sayed', N'Software Engineer',               N'29001010100011', NULL, GETDATE(), N'EMP001', 1),
(N'Sara Ali Hussein',        N'HR Specialist',                   N'29102020200022', NULL, GETDATE(), N'EMP002', 1),
(N'Mostafa Hassan Kamel',    N'Network Administrator',           N'28903030300033', NULL, GETDATE(), N'EMP003', 1),
(N'Mona Ibrahim Adel',       N'Accountant',                      N'29304040400044', NULL, GETDATE(), N'EMP004', 1),
(N'Khaled Youssef Fathy',    N'Sales Manager',                   N'28705050500055', NULL, GETDATE(), N'EMP005', 1),
(N'Nourhan Adel Samir',      N'Marketing Coordinator',           N'29506060600066', NULL, GETDATE(), N'EMP006', 1),
(N'Omar Tarek Mahmoud',      N'IT Support Specialist',           N'29607070700077', NULL, GETDATE(), N'EMP007', 1),
(N'Heba Samir Naguib',       N'Legal Advisor',                   N'28808080800088', NULL, GETDATE(), N'EMP008', 1),
(N'Tamer Fathy Ragab',       N'Procurement Officer',             N'29009090900099', NULL, GETDATE(), N'EMP009', 1),
(N'Rania Mahmoud Zaki',      N'Customer Service Representative', N'29410101000101', NULL, GETDATE(), N'EMP010', 1);

/* ----------------------------------------------------------------------
   9. NonEmployees (10)
   ---------------------------------------------------------------------- */
INSERT INTO NonEmployees (Name, ContactInfo, Type, CreatedAt) VALUES
(N'Mahmoud Contracting Co.', N'01200000001 - contracts@mahmoudco.com',  N'Contractor', GETDATE()),
(N'Ziad Kamal',              N'01200000002',                            N'Visitor',    GETDATE()),
(N'Global Networks Vendor',  N'01200000003 - sales@globalnet.com',      N'Vendor',     GETDATE()),
(N'Amira Fouad',             N'01200000004',                            N'Intern',     GETDATE()),
(N'Tech Support Partners',   N'01200000005 - support@techpartners.com', N'Vendor',     GETDATE()),
(N'Youssef Adly',            N'01200000006',                            N'Visitor',    GETDATE()),
(N'BlueWave Maintenance',    N'01200000007 - info@bluewave.com',        N'Contractor', GETDATE()),
(N'Salma Nabil',             N'01200000008',                            N'Intern',     GETDATE()),
(N'Delta Cleaning Services', N'01200000009 - contact@deltaclean.com',   N'Contractor', GETDATE()),
(N'Karim Adel',              N'01200000010',                            N'Visitor',    GETDATE());

/* ----------------------------------------------------------------------
   10. Quotas (3 per Service Provider = 12)
   ---------------------------------------------------------------------- */
INSERT INTO Quotas (BaseAmount, ExtraAmount, Fees, IsActive, ServiceProviderId) VALUES
-- Orange
(5.00,  0.00, 50.00,  1, @OrangeId),
(10.00, 0.00, 100.00, 1, @OrangeId),
(20.00, 0.00, 200.00, 1, @OrangeId),
-- WE
(5.00,  0.00, 45.00,  1, @WEId),
(10.00, 0.00, 95.00,  1, @WEId),
(20.00, 0.00, 190.00, 1, @WEId),
-- Vodafone
(5.00,  0.00, 55.00,  1, @VodafoneId),
(10.00, 0.00, 105.00, 1, @VodafoneId),
(20.00, 0.00, 210.00, 1, @VodafoneId),
-- Etisalat
(5.00,  0.00, 48.00,  1, @EtisalatId),
(10.00, 0.00, 98.00,  1, @EtisalatId),
(20.00, 0.00, 195.00, 1, @EtisalatId);

/* ----------------------------------------------------------------------
   11. Sims (6 per Service Provider = 24)
   ---------------------------------------------------------------------- */
INSERT INTO Sims (SerialNumber, PhoneNumber, NetworkType, Status, RegisteredAt, ServiceProviderId) VALUES
-- Orange (012)
(N'8920101234560000011', N'01201110001', N'4G', N'Active', GETDATE(), @OrangeId),
(N'8920101234560000012', N'01201110002', N'5G', N'Active', GETDATE(), @OrangeId),
(N'8920101234560000013', N'01201110003', N'4G', N'Active', GETDATE(), @OrangeId),
(N'8920101234560000014', N'01201110004', N'4G', N'Active', GETDATE(), @OrangeId),
(N'8920101234560000015', N'01201110005', N'5G', N'Active', GETDATE(), @OrangeId),
(N'8920101234560000016', N'01201110006', N'4G', N'Active', GETDATE(), @OrangeId),
-- WE (015)
(N'8920102234560000021', N'01501110001', N'4G', N'Active', GETDATE(), @WEId),
(N'8920102234560000022', N'01501110002', N'5G', N'Active', GETDATE(), @WEId),
(N'8920102234560000023', N'01501110003', N'4G', N'Active', GETDATE(), @WEId),
(N'8920102234560000024', N'01501110004', N'4G', N'Active', GETDATE(), @WEId),
(N'8920102234560000025', N'01501110005', N'5G', N'Active', GETDATE(), @WEId),
(N'8920102234560000026', N'01501110006', N'4G', N'Active', GETDATE(), @WEId),
-- Vodafone (010)
(N'8920103234560000031', N'01001110001', N'4G', N'Active', GETDATE(), @VodafoneId),
(N'8920103234560000032', N'01001110002', N'5G', N'Active', GETDATE(), @VodafoneId),
(N'8920103234560000033', N'01001110003', N'4G', N'Active', GETDATE(), @VodafoneId),
(N'8920103234560000034', N'01001110004', N'4G', N'Active', GETDATE(), @VodafoneId),
(N'8920103234560000035', N'01001110005', N'5G', N'Active', GETDATE(), @VodafoneId),
(N'8920103234560000036', N'01001110006', N'4G', N'Active', GETDATE(), @VodafoneId),
-- Etisalat (011)
(N'8920104234560000041', N'01101110001', N'4G', N'Active', GETDATE(), @EtisalatId),
(N'8920104234560000042', N'01101110002', N'5G', N'Active', GETDATE(), @EtisalatId),
(N'8920104234560000043', N'01101110003', N'4G', N'Active', GETDATE(), @EtisalatId),
(N'8920104234560000044', N'01101110004', N'4G', N'Active', GETDATE(), @EtisalatId),
(N'8920104234560000045', N'01101110005', N'5G', N'Active', GETDATE(), @EtisalatId),
(N'8920104234560000046', N'01101110006', N'4G', N'Active', GETDATE(), @EtisalatId);

/* ----------------------------------------------------------------------
   12. Usbs (6 per Service Provider = 24)
   ---------------------------------------------------------------------- */
INSERT INTO Usbs (SerialNumber, Model, Status, RegisteredAt, ServiceProviderId) VALUES
-- Orange
(N'USB-ORG-000001', N'Huawei E3372', N'Active', GETDATE(), @OrangeId),
(N'USB-ORG-000002', N'ZTE MF79U',    N'Active', GETDATE(), @OrangeId),
(N'USB-ORG-000003', N'Huawei E8372', N'Active', GETDATE(), @OrangeId),
(N'USB-ORG-000004', N'Huawei E3372', N'Active', GETDATE(), @OrangeId),
(N'USB-ORG-000005', N'ZTE MF79U',    N'Active', GETDATE(), @OrangeId),
(N'USB-ORG-000006', N'Huawei E8372', N'Active', GETDATE(), @OrangeId),
-- WE
(N'USB-WE--000001', N'Huawei E3372', N'Active', GETDATE(), @WEId),
(N'USB-WE--000002', N'ZTE MF79U',    N'Active', GETDATE(), @WEId),
(N'USB-WE--000003', N'Huawei E8372', N'Active', GETDATE(), @WEId),
(N'USB-WE--000004', N'Huawei E3372', N'Active', GETDATE(), @WEId),
(N'USB-WE--000005', N'ZTE MF79U',    N'Active', GETDATE(), @WEId),
(N'USB-WE--000006', N'Huawei E8372', N'Active', GETDATE(), @WEId),
-- Vodafone
(N'USB-VOD-000001', N'Huawei E3372', N'Active', GETDATE(), @VodafoneId),
(N'USB-VOD-000002', N'ZTE MF79U',    N'Active', GETDATE(), @VodafoneId),
(N'USB-VOD-000003', N'Huawei E8372', N'Active', GETDATE(), @VodafoneId),
(N'USB-VOD-000004', N'Huawei E3372', N'Active', GETDATE(), @VodafoneId),
(N'USB-VOD-000005', N'ZTE MF79U',    N'Active', GETDATE(), @VodafoneId),
(N'USB-VOD-000006', N'Huawei E8372', N'Active', GETDATE(), @VodafoneId),
-- Etisalat
(N'USB-ETS-000001', N'Huawei E3372', N'Active', GETDATE(), @EtisalatId),
(N'USB-ETS-000002', N'ZTE MF79U',    N'Active', GETDATE(), @EtisalatId),
(N'USB-ETS-000003', N'Huawei E8372', N'Active', GETDATE(), @EtisalatId),
(N'USB-ETS-000004', N'Huawei E3372', N'Active', GETDATE(), @EtisalatId),
(N'USB-ETS-000005', N'ZTE MF79U',    N'Active', GETDATE(), @EtisalatId),
(N'USB-ETS-000006', N'Huawei E8372', N'Active', GETDATE(), @EtisalatId);

/* ----------------------------------------------------------------------
   13. Tables intentionally left EMPTY (per instructions):
       Serials, Subscriptions, DeviceTransfers, DeviceStatuses,
       ReceiverSignatures, AuditLogs, Documents, DocumentDetails,
       UserOtps, ItemTypes, DeviceStatusTypes
   ---------------------------------------------------------------------- */

COMMIT TRANSACTION;
PRINT 'Seed data inserted successfully.';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrLine INT = ERROR_LINE();
    RAISERROR('Seed script failed at line %d: %s', 16, 1, @ErrLine, @ErrMsg);
END CATCH
GO