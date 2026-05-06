-- ============================================
-- QRCerts: Tablas para sistema de cuotas
-- Ejecutar contra la base QRCerts
-- ============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OtecQuotaConfig')
BEGIN
    CREATE TABLE OtecQuotaConfig (
        Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
        OtecId UNIQUEIDENTIFIER NOT NULL,
        QuotaActivo BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT UQ_OtecQuotaConfig_OtecId UNIQUE (OtecId),
        CONSTRAINT FK_OtecQuotaConfig_Otec FOREIGN KEY (OtecId) REFERENCES Otecs(Id)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrdenCompra')
BEGIN
    CREATE TABLE OrdenCompra (
        Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
        OtecId UNIQUEIDENTIFIER NOT NULL,
        CantidadComprada INT NOT NULL,
        CantidadUsada INT NOT NULL DEFAULT 0,
        FechaExpiracion DATETIME2 NOT NULL,
        Activa BIT NOT NULL DEFAULT 1,
        CreadaPor NVARCHAR(200) NOT NULL,
        Notas NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_OrdenCompra_Otec FOREIGN KEY (OtecId) REFERENCES Otecs(Id)
    );
    CREATE INDEX IX_OrdenCompra_OtecId_Activa ON OrdenCompra (OtecId, Activa);
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrdenCompraHistorial')
BEGIN
    CREATE TABLE OrdenCompraHistorial (
        Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
        OrdenCompraId UNIQUEIDENTIFIER NOT NULL,
        OtecId UNIQUEIDENTIFIER NOT NULL,
        Evento NVARCHAR(50) NOT NULL,
        Detalle NVARCHAR(500) NULL,
        CreadaPor NVARCHAR(200) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_OrdenCompraHistorial_Orden FOREIGN KEY (OrdenCompraId) REFERENCES OrdenCompra(Id)
    );
    CREATE INDEX IX_OrdenCompraHistorial_OtecId ON OrdenCompraHistorial (OtecId);
END
GO
