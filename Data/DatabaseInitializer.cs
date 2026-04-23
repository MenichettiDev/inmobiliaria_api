using Microsoft.EntityFrameworkCore;

namespace inmobiliariaApi.Data
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            try
            {
                logger.LogInformation("Iniciando migración y verificación de tablas de la base de datos...");
                
                try {
                    await context.Database.MigrateAsync();
                } catch (Exception ex) {
                    logger.LogWarning("Migración automática falló o ya estaba aplicada: {Message}", ex.Message);
                }

                // 1. Tabla: refresh_tokens
                try {
                    await context.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS `refresh_tokens` (
                            `Id` int(11) NOT NULL AUTO_INCREMENT,
                            `id_usuario` int(11) NOT NULL,
                            `id_inmobiliaria` int(11) NOT NULL,
                            `Token` varchar(512) NOT NULL,
                            `expira_en` datetime NOT NULL,
                            `creado_en` datetime NOT NULL DEFAULT current_timestamp(),
                            `revocado_en` datetime DEFAULT NULL,
                            `reemplazado_por` varchar(512) DEFAULT NULL,
                            `ip_origen` varchar(45) DEFAULT NULL,
                            PRIMARY KEY (`Id`),
                            UNIQUE KEY `idx_token` (`Token`),
                            KEY `fk_refresh_tokens_usuario` (`id_usuario`)
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
                    ");
                } catch (Exception ex) {
                    logger.LogWarning("No se pudo crear/verificar refresh_tokens (posiblemente falte tabla usuarios): {Message}", ex.Message);
                }

                // 1b. Columnas thumbnail en imagenes_propiedades
                try {
                    await context.Database.ExecuteSqlRawAsync(@"
                        ALTER TABLE `imagenes_propiedades`
                            ADD COLUMN IF NOT EXISTS `thumbnail_url` varchar(500) NULL,
                            ADD COLUMN IF NOT EXISTS `thumbnail_key` varchar(500) NULL;
                    ");
                } catch (Exception ex) {
                    logger.LogWarning("No se pudo agregar columnas thumbnail a imagenes_propiedades: {Message}", ex.Message);
                }

                // 2. Tabla: pagos_suscripcion
                try {
                    await context.Database.ExecuteSqlRawAsync(@"
                        CREATE TABLE IF NOT EXISTS `pagos_suscripcion` (
                            `Id` int(11) NOT NULL AUTO_INCREMENT,
                            `id_inmobiliaria` int(11) NOT NULL,
                            `id_plan` int(11) NOT NULL,
                            `id_suscripcion` int(11) DEFAULT NULL,
                            `mp_preference_id` varchar(100) NOT NULL,
                            `mp_payment_id` bigint(20) DEFAULT NULL,
                            `estado` varchar(50) NOT NULL,
                            `monto` decimal(10,2) NOT NULL,
                            `moneda` varchar(3) NOT NULL DEFAULT 'ARS',
                            `dias_plan` int(11) NOT NULL DEFAULT 30,
                            `creado_en` datetime NOT NULL DEFAULT current_timestamp(),
                            `actualizado_en` datetime NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
                            PRIMARY KEY (`Id`)
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
                    ");
                } catch (Exception ex) {
                    logger.LogWarning("No se pudo crear/verificar pagos_suscripcion: {Message}", ex.Message);
                }

                logger.LogInformation("Verificación de tablas completada.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error crítico durante la inicialización de la base de datos.");
                throw;
            }
        }
    }
}
