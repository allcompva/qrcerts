-- =============================================
-- Script: CreateMoodleTables.sql
-- Description: Creates tables for Moodle integration
-- Date: 2026-01-21
-- =============================================

-- Tabla MoodleConfigs: Almacena la configuración de conexión Moodle por OTEC
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MoodleConfigs')
BEGIN
    CREATE TABLE MoodleConfigs (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        OtecId UNIQUEIDENTIFIER NOT NULL,
        MoodleUrl NVARCHAR(500) NOT NULL,
        Token NVARCHAR(500) NOT NULL,
        Activo BIT NOT NULL DEFAULT 1,
        UltimaConexionExitosa DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_MoodleConfigs_Otecs FOREIGN KEY (OtecId) REFERENCES Otecs(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_MoodleConfigs_OtecId UNIQUE (OtecId)
    );

    PRINT 'Table MoodleConfigs created successfully.';
END
ELSE
BEGIN
    PRINT 'Table MoodleConfigs already exists.';
END
GO

-- Tabla MoodleCursosImportados: Registro de cursos importados desde Moodle
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MoodleCursosImportados')
BEGIN
    CREATE TABLE MoodleCursosImportados (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        OtecId UNIQUEIDENTIFIER NOT NULL,
        MoodleCourseId INT NOT NULL,
        NombreCurso NVARCHAR(500) NOT NULL,
        ShortName NVARCHAR(200) NOT NULL DEFAULT '',
        Categoria NVARCHAR(200) NOT NULL DEFAULT '',
        CursoLocalId UNIQUEIDENTIFIER NULL,
        CantidadAlumnos INT NOT NULL DEFAULT 0,
        UltimaSync DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_MoodleCursosImportados_Otecs FOREIGN KEY (OtecId) REFERENCES Otecs(Id) ON DELETE CASCADE,
        CONSTRAINT FK_MoodleCursosImportados_Cursos FOREIGN KEY (CursoLocalId) REFERENCES Cursos(Id) ON DELETE SET NULL,
        CONSTRAINT UQ_MoodleCursosImportados_OtecCourse UNIQUE (OtecId, MoodleCourseId)
    );

    PRINT 'Table MoodleCursosImportados created successfully.';
END
ELSE
BEGIN
    PRINT 'Table MoodleCursosImportados already exists.';
END
GO

-- Tabla MoodleFieldMappings: Mapeo de campos Moodle a variables de certificado
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MoodleFieldMappings')
BEGIN
    CREATE TABLE MoodleFieldMappings (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        CursoId UNIQUEIDENTIFIER NOT NULL,
        CampoMoodle NVARCHAR(200) NOT NULL,
        VariableCertificado NVARCHAR(200) NOT NULL,
        Orden INT NOT NULL DEFAULT 0,
        EsObligatorio BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_MoodleFieldMappings_Cursos FOREIGN KEY (CursoId) REFERENCES Cursos(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_MoodleFieldMappings_CursoCampo UNIQUE (CursoId, CampoMoodle)
    );

    PRINT 'Table MoodleFieldMappings created successfully.';
END
ELSE
BEGIN
    PRINT 'Table MoodleFieldMappings already exists.';
END
GO

-- Indices adicionales para mejorar rendimiento
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MoodleConfigs_OtecId')
BEGIN
    CREATE INDEX IX_MoodleConfigs_OtecId ON MoodleConfigs(OtecId);
    PRINT 'Index IX_MoodleConfigs_OtecId created.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MoodleCursosImportados_OtecId')
BEGIN
    CREATE INDEX IX_MoodleCursosImportados_OtecId ON MoodleCursosImportados(OtecId);
    PRINT 'Index IX_MoodleCursosImportados_OtecId created.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MoodleFieldMappings_CursoId')
BEGIN
    CREATE INDEX IX_MoodleFieldMappings_CursoId ON MoodleFieldMappings(CursoId);
    PRINT 'Index IX_MoodleFieldMappings_CursoId created.';
END
GO

PRINT 'Moodle integration tables setup completed.';
