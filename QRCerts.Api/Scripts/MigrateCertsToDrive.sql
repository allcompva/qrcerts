-- ============================================
-- Script para identificar certificados a migrar
-- Los que tienen url_landing NULL o vacío necesitan
-- que se les suba el PDF a Drive
-- ============================================

-- Contar certificados por estado de migración
SELECT
    'Total' as Estado, COUNT(*) as Cantidad FROM Certificados
UNION ALL
SELECT
    'Con Drive ID' as Estado, COUNT(*) FROM Certificados WHERE url_landing IS NOT NULL AND url_landing != '' AND LEN(url_landing) > 10
UNION ALL
SELECT
    'Sin Drive ID (a migrar)' as Estado, COUNT(*) FROM Certificados WHERE url_landing IS NULL OR url_landing = '' OR LEN(url_landing) <= 10

-- Listar certificados a migrar con datos del OTEC para organizar carpetas
SELECT
    c.Id, c.AlumnoId, c.CursoId, c.PdfFilename,
    o.Slug as OtecSlug,
    o.Nombre as OtecNombre
FROM Certificados c
INNER JOIN Cursos cu ON c.CursoId = cu.Id
INNER JOIN Otecs o ON cu.OtecId = o.Id
WHERE c.url_landing IS NULL OR c.url_landing = '' OR LEN(c.url_landing) <= 10
ORDER BY o.Slug, c.IssuedAt
