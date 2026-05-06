USE [QRCerts]
GO
INSERT [dbo].[AdminUsers] ([Id], [Username], [PasswordHash], 
[Email], [FullName], [IsActive], [CreatedAt], [LastLoginAt]) 
VALUES (N'3375165f-b9f2-4e63-97ca-b597d36160e2', N'admin', 
N'$2a$11$tTX/ep7QeJu84Kn2beEBbO6BS8PRPExqHFpnSyU8JeXy8Mx7frYaG', 
N'admin@qrcerts.com', N'Administrador del Sistema', 1, 
CAST(N'2025-09-24T01:19:34.5841164' AS DateTime2), 
CAST(N'2026-04-22T22:36:16.3033333' AS DateTime2))
GO
