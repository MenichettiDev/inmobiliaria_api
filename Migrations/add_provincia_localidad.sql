-- =====================================================
-- Migration: provincias + localidades
-- Matches actual DB schema
-- =====================================================

-- 1. Tabla provincias
CREATE TABLE IF NOT EXISTS `provincias` (
    `id_provincia` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `codigo_indec`  VARCHAR(50)  NULL,
    `nombre`        VARCHAR(45)  NOT NULL,
    `activo`        TINYINT(1)   NOT NULL DEFAULT 1,
    PRIMARY KEY (`id_provincia`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 2. Tabla localidades
CREATE TABLE IF NOT EXISTS `localidades` (
    `id_localidad`  BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    `codigo_postal` VARCHAR(10)  NULL,
    `latitud`       VARCHAR(100) NULL,
    `longitud`      VARCHAR(100) NULL,
    `municipio`     VARCHAR(100) NULL,
    `nombre`        VARCHAR(45)  NOT NULL,
    `id_partido`    BIGINT UNSIGNED NULL,
    `id_provincia`  BIGINT UNSIGNED NOT NULL,
    PRIMARY KEY (`id_localidad`),
    CONSTRAINT `FK_localidades_provincias`
        FOREIGN KEY (`id_provincia`) REFERENCES `provincias` (`id_provincia`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 3. FK en inmobiliarias → provincias (nullable)
ALTER TABLE `inmobiliarias`
    ADD COLUMN IF NOT EXISTS `id_provincia` BIGINT UNSIGNED NULL,
    ADD CONSTRAINT `FK_inmobiliarias_provincias`
        FOREIGN KEY (`id_provincia`) REFERENCES `provincias` (`id_provincia`) ON DELETE SET NULL;

-- 4. FK en propiedades → localidades (nullable)
ALTER TABLE `propiedades`
    ADD COLUMN IF NOT EXISTS `id_localidad` BIGINT UNSIGNED NULL,
    ADD CONSTRAINT `FK_propiedades_localidades`
        FOREIGN KEY (`id_localidad`) REFERENCES `localidades` (`id_localidad`) ON DELETE SET NULL;
