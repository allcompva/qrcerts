-- Script para agregar campos MoodleUserId y MoodleCourseId a REGISTRO_ALUMNOS
-- Ejecutar en la base de datos QRCerts

-- Agregar columna MoodleUserId si no existe
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'REGISTRO_ALUMNOS') AND name = 'MoodleUserId')
BEGIN
    ALTER TABLE REGISTRO_ALUMNOS ADD MoodleUserId INT NULL;
    PRINT 'Columna MoodleUserId agregada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La columna MoodleUserId ya existe.';
END
GO

-- Agregar columna MoodleCourseId si no existe
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'REGISTRO_ALUMNOS') AND name = 'MoodleCourseId')
BEGIN
    ALTER TABLE REGISTRO_ALUMNOS ADD MoodleCourseId INT NULL;
    PRINT 'Columna MoodleCourseId agregada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La columna MoodleCourseId ya existe.';
END
GO

-- Crear indice para busqueda rapida por MoodleUserId y MoodleCourseId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_REGISTRO_ALUMNOS_MoodleIds' AND object_id = OBJECT_ID(N'REGISTRO_ALUMNOS'))
BEGIN
    CREATE INDEX IX_REGISTRO_ALUMNOS_MoodleIds ON REGISTRO_ALUMNOS(MoodleUserId, MoodleCourseId);
    PRINT 'Indice IX_REGISTRO_ALUMNOS_MoodleIds creado exitosamente.';
END
ELSE
BEGIN
    PRINT 'El indice IX_REGISTRO_ALUMNOS_MoodleIds ya existe.';
END
GO

PRINT 'Script completado exitosamente.';
