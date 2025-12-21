-- phpMyAdmin SQL Dump
-- version 5.2.2
-- https://www.phpmyadmin.net/
--
-- Host: localhost:3306
-- Generation Time: Dec 20, 2025 at 03:27 AM
-- Server version: 11.4.8-MariaDB-log
-- PHP Version: 8.1.10

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `inmobiliaria_saas`
--

-- --------------------------------------------------------

--
-- Table structure for table `busquedas_guardadas`
--

CREATE TABLE `busquedas_guardadas` (
  `id` int(11) NOT NULL,
  `email` varchar(100) NOT NULL,
  `filtros_json` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin NOT NULL CHECK (json_valid(`filtros_json`)),
  `ultimo_envio` datetime DEFAULT NULL,
  `creado_en` datetime DEFAULT current_timestamp(),
  `actualizado_en` datetime DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `id_inmobiliaria` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `estados_inmobiliaria`
--

CREATE TABLE `estados_inmobiliaria` (
  `id` tinyint(4) NOT NULL,
  `codigo` varchar(30) NOT NULL,
  `descripcion` varchar(100) NOT NULL,
  `activo` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `estados_inmobiliaria`
--

INSERT INTO `estados_inmobiliaria` (`id`, `codigo`, `descripcion`, `activo`) VALUES
(1, 'activa', 'Inmobiliaria operativa', 1),
(2, 'suspendida', 'Suspendida por falta de pago o incumplimiento', 1),
(3, 'cancelada', 'Cuenta cerrada definitivamente', 1);

-- --------------------------------------------------------

--
-- Table structure for table `estados_lead`
--

CREATE TABLE `estados_lead` (
  `id` tinyint(4) NOT NULL,
  `nombre` varchar(30) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `estados_lead`
--

INSERT INTO `estados_lead` (`id`, `nombre`) VALUES
(4, 'cerrado'),
(2, 'contactado'),
(1, 'nuevo'),
(5, 'perdido'),
(3, 'visito');

-- --------------------------------------------------------

--
-- Table structure for table `estados_lead_admin`
--

CREATE TABLE `estados_lead_admin` (
  `id` tinyint(4) NOT NULL,
  `codigo` varchar(30) NOT NULL,
  `descripcion` varchar(100) NOT NULL,
  `activo` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `estados_lead_admin`
--

INSERT INTO `estados_lead_admin` (`id`, `codigo`, `descripcion`, `activo`) VALUES
(1, 'activo', 'Lead operativo', 1),
(2, 'archivado', 'Lead archivado', 1),
(3, 'eliminado', 'Lead eliminado lógicamente', 1);

-- --------------------------------------------------------

--
-- Table structure for table `estados_propiedades_operativas`
--

CREATE TABLE `estados_propiedades_operativas` (
  `id` tinyint(4) NOT NULL,
  `codigo` varchar(30) NOT NULL,
  `nombre` varchar(50) NOT NULL,
  `descripcion` varchar(150) DEFAULT NULL,
  `activo` tinyint(1) NOT NULL DEFAULT 1,
  `color_hex` varchar(7) DEFAULT '#CCCCCC' COMMENT 'Útil para UI (ej: verde para disponible)'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `estados_propiedades_operativas`
--

INSERT INTO `estados_propiedades_operativas` (`id`, `codigo`, `nombre`, `descripcion`, `activo`, `color_hex`) VALUES
(1, 'disponible', 'Disponible', 'Propiedad disponible para alquiler o venta', 1, '#4CAF50'),
(2, 'alquilada', 'Alquilada', 'Propiedad actualmente alquilada', 1, '#2196F3'),
(3, 'vendida', 'Vendida', 'Propiedad ya vendida', 1, '#9E9E9E'),
(4, 'reservada', 'Reservada', 'Propiedad con reserva pendiente', 1, '#FF9800');

-- --------------------------------------------------------

--
-- Table structure for table `estados_propiedad_actidad`
--

CREATE TABLE `estados_propiedad_actidad` (
  `id` tinyint(4) NOT NULL,
  `codigo` varchar(30) NOT NULL,
  `descripcion` varchar(100) NOT NULL,
  `visible` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `estados_propiedad_actidad`
--

INSERT INTO `estados_propiedad_actidad` (`id`, `codigo`, `descripcion`, `visible`) VALUES
(1, 'activa', 'Propiedad visible y operativa', 1),
(2, 'archivada', 'Propiedad archivada', 0),
(3, 'eliminada', 'Propiedad eliminada lógicamente', 0);

-- --------------------------------------------------------

--
-- Table structure for table `estados_suscripcion`
--

CREATE TABLE `estados_suscripcion` (
  `id` tinyint(4) NOT NULL,
  `codigo` varchar(30) NOT NULL,
  `descripcion` varchar(100) NOT NULL,
  `permite_operar` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `estados_suscripcion`
--

INSERT INTO `estados_suscripcion` (`id`, `codigo`, `descripcion`, `permite_operar`) VALUES
(1, 'activa', 'Suscripción activa', 1),
(2, 'pausada', 'Pausada temporalmente', 0),
(3, 'vencida', 'Vencida por falta de pago', 0),
(4, 'cancelada', 'Cancelada definitivamente', 0);

-- --------------------------------------------------------

--
-- Table structure for table `estados_usuario`
--

CREATE TABLE `estados_usuario` (
  `id` tinyint(4) NOT NULL,
  `codigo` varchar(30) NOT NULL,
  `descripcion` varchar(100) NOT NULL,
  `activo` tinyint(1) DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `estados_usuario`
--

INSERT INTO `estados_usuario` (`id`, `codigo`, `descripcion`, `activo`) VALUES
(1, 'activo', 'Usuario habilitado', 1),
(2, 'bloqueado', 'Bloqueado por seguridad o sanción', 1),
(3, 'inactivo', 'Usuario desvinculado de la inmobiliaria', 1);

-- --------------------------------------------------------

--
-- Table structure for table `fuentes_contacto`
--

CREATE TABLE `fuentes_contacto` (
  `id` tinyint(4) NOT NULL,
  `nombre` varchar(30) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `fuentes_contacto`
--

INSERT INTO `fuentes_contacto` (`id`, `nombre`) VALUES
(1, 'directo'),
(3, 'facebook'),
(2, 'google'),
(5, 'idealista'),
(4, 'instagram'),
(8, 'referido'),
(7, 'whatsapp'),
(6, 'zillow');

-- --------------------------------------------------------

--
-- Table structure for table `imagenes_propiedades`
--

CREATE TABLE `imagenes_propiedades` (
  `id` int(11) NOT NULL,
  `id_propiedad` int(11) NOT NULL,
  `url` varchar(500) NOT NULL,
  `orden` tinyint(3) UNSIGNED DEFAULT 0,
  `creado_en` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `imagenes_propiedades`
--

INSERT INTO `imagenes_propiedades` (`id`, `id_propiedad`, `url`, `orden`, `creado_en`) VALUES
(1, 1, 'https://ejemplo.com/img/prop1_1.jpg', 1, '2025-12-17 00:16:55'),
(2, 1, 'https://ejemplo.com/img/prop1_2.jpg', 2, '2025-12-17 00:16:55'),
(3, 1, 'https://ejemplo.com/img/prop1_3.jpg', 3, '2025-12-17 00:16:55'),
(4, 2, 'https://ejemplo.com/img/prop2_1.jpg', 1, '2025-12-17 00:16:55'),
(5, 2, 'https://ejemplo.com/img/prop2_2.jpg', 2, '2025-12-17 00:16:55'),
(6, 4, 'https://ejemplo.com/img/prop4_1.jpg', 1, '2025-12-17 00:16:55'),
(7, 4, 'https://ejemplo.com/img/prop4_interior.jpg', 2, '2025-12-17 00:16:55'),
(8, 4, 'https://ejemplo.com/img/prop4_vista.jpg', 3, '2025-12-17 00:16:55');

-- --------------------------------------------------------

--
-- Table structure for table `inmobiliarias`
--

CREATE TABLE `inmobiliarias` (
  `id` int(11) NOT NULL,
  `nombre` varchar(150) NOT NULL,
  `subdominio` varchar(50) NOT NULL,
  `dominio_personalizado` varchar(100) DEFAULT NULL,
  `id_plan` int(11) DEFAULT 1,
  `creado_en` datetime DEFAULT current_timestamp(),
  `actualizado_en` datetime DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `id_estado` tinyint(4) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `inmobiliarias`
--

INSERT INTO `inmobiliarias` (`id`, `nombre`, `subdominio`, `dominio_personalizado`, `id_plan`, `creado_en`, `actualizado_en`, `id_estado`) VALUES
(1, 'Inmobiliaria Acme', 'acme', NULL, 2, '2025-12-17 00:15:59', '2025-12-17 00:15:59', 1),
(2, 'Propiedades Elite', 'elite', 'panel.propiedadeselite.com', 3, '2025-12-17 00:15:59', '2025-12-17 00:15:59', 1);

-- --------------------------------------------------------

--
-- Table structure for table `leads`
--

CREATE TABLE `leads` (
  `id` int(11) NOT NULL,
  `id_propiedad` int(11) NOT NULL,
  `id_inmobiliaria` int(11) NOT NULL,
  `id_usuario_asignado` int(11) DEFAULT NULL,
  `nombre_completo` varchar(100) NOT NULL,
  `email` varchar(100) DEFAULT NULL,
  `telefono` varchar(20) DEFAULT NULL,
  `mensaje` text DEFAULT NULL,
  `id_fuente` tinyint(4) NOT NULL,
  `id_estado` tinyint(4) NOT NULL,
  `creado_en` datetime DEFAULT current_timestamp(),
  `actualizado_en` datetime DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `id_estado_admin` tinyint(4) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `leads`
--

INSERT INTO `leads` (`id`, `id_propiedad`, `id_inmobiliaria`, `id_usuario_asignado`, `nombre_completo`, `email`, `telefono`, `mensaje`, `id_fuente`, `id_estado`, `creado_en`, `actualizado_en`, `id_estado_admin`) VALUES
(1, 1, 1, 2, 'Javier Fernández', 'javier.f@email.com', '555-1122', '¿Está disponible para visitar esta semana?', 2, 1, '2025-12-17 00:17:03', '2025-12-17 00:17:03', 1),
(2, 2, 1, 3, 'Carla Díaz', 'carla.diaz@email.com', NULL, 'Me interesa agendar una visita el sábado.', 5, 2, '2025-12-17 00:17:03', '2025-12-17 00:17:03', 1),
(3, 3, 1, 2, 'Roberto Alvarado', 'roberto.a@email.com', '555-3344', 'Ya firmamos el contrato de alquiler.', 1, 4, '2025-12-17 00:17:03', '2025-12-17 00:17:03', 1),
(4, 4, 2, 5, 'Sofía Martínez', 'sofia.m@email.com', '555-6677', '¿Tiene disponibilidad para mostrar hoy?', 4, 1, '2025-12-17 00:17:03', '2025-12-17 00:17:03', 1);

-- --------------------------------------------------------

--
-- Table structure for table `lead_estados_historial`
--

CREATE TABLE `lead_estados_historial` (
  `id` int(11) NOT NULL,
  `id_lead` int(11) NOT NULL,
  `id_estado` int(11) NOT NULL,
  `id_usuario` int(11) DEFAULT NULL,
  `comentario` varchar(255) DEFAULT NULL,
  `creado_en` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `planes`
--

CREATE TABLE `planes` (
  `id` int(11) NOT NULL,
  `nombre` varchar(50) NOT NULL,
  `descripcion` varchar(255) DEFAULT NULL,
  `precio_usd` decimal(10,2) NOT NULL,
  `max_propiedades` int(11) DEFAULT NULL,
  `max_usuarios` int(11) DEFAULT NULL,
  `max_leads_mes` int(11) DEFAULT NULL,
  `white_label` tinyint(1) DEFAULT 0,
  `dominio_personalizado` tinyint(1) DEFAULT 0,
  `automatizaciones` tinyint(1) DEFAULT 0,
  `api_acceso` tinyint(1) DEFAULT 0,
  `soporte` enum('email','prioritario','24x7') DEFAULT 'email',
  `activo` tinyint(1) DEFAULT 1,
  `creado_en` datetime DEFAULT current_timestamp(),
  `actualizado_en` datetime DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `historial_estados` tinyint(1) DEFAULT 0,
  `actividades_lead` tinyint(1) DEFAULT 0,
  `tipos_actividad` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_bin DEFAULT NULL CHECK (json_valid(`tipos_actividad`)),
  `automatizacion_leads` tinyint(1) DEFAULT 0,
  `lead_scoring` tinyint(1) DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `planes`
--

INSERT INTO `planes` (`id`, `nombre`, `descripcion`, `precio_usd`, `max_propiedades`, `max_usuarios`, `max_leads_mes`, `white_label`, `dominio_personalizado`, `automatizaciones`, `api_acceso`, `soporte`, `activo`, `creado_en`, `actualizado_en`, `historial_estados`, `actividades_lead`, `tipos_actividad`, `automatizacion_leads`, `lead_scoring`) VALUES
(1, 'FREE', 'Plan gratuito para prueba', 0.00, 10, 1, 20, 0, 0, 0, 0, 'email', 1, '2025-12-18 22:23:12', '2025-12-18 22:34:17', 0, 0, '[\"nota\"]', 0, 0),
(2, 'BASIC', 'Inmobiliarias pequeñas', 19.00, 50, 3, 200, 0, 0, 1, 0, 'email', 1, '2025-12-18 22:23:12', '2025-12-18 22:34:17', 1, 1, '[\"nota\",\"llamada\"]', 0, 0),
(3, 'PRO', 'Plan profesional', 49.00, 300, 10, NULL, 1, 1, 1, 0, 'prioritario', 1, '2025-12-18 22:23:12', '2025-12-18 22:34:17', 1, 1, '[\"nota\",\"llamada\",\"whatsapp\",\"email\",\"visita\"]', 1, 0),
(4, 'PREMIUM', 'Agencias grandes', 99.00, NULL, NULL, NULL, 1, 1, 1, 1, '24x7', 1, '2025-12-18 22:23:12', '2025-12-18 22:34:17', 1, 1, '[\"nota\",\"llamada\",\"whatsapp\",\"email\",\"visita\"]', 1, 1);

-- --------------------------------------------------------

--
-- Table structure for table `propiedades`
--

CREATE TABLE `propiedades` (
  `id` int(11) NOT NULL,
  `id_inmobiliaria` int(11) NOT NULL,
  `id_agente_responsable` int(11) DEFAULT NULL,
  `titulo` varchar(200) NOT NULL,
  `descripcion` text DEFAULT NULL,
  `precio` decimal(14,2) NOT NULL,
  `direccion` varchar(255) NOT NULL,
  `latitud` decimal(10,8) DEFAULT NULL,
  `longitud` decimal(11,8) DEFAULT NULL,
  `publicada_en` datetime DEFAULT NULL,
  `creado_en` datetime DEFAULT current_timestamp(),
  `actualizado_en` datetime DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `id_estado_admin` tinyint(4) NOT NULL DEFAULT 1,
  `id_estado_operativo` tinyint(4) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `propiedades`
--

INSERT INTO `propiedades` (`id`, `id_inmobiliaria`, `id_agente_responsable`, `titulo`, `descripcion`, `precio`, `direccion`, `latitud`, `longitud`, `publicada_en`, `creado_en`, `actualizado_en`, `id_estado_admin`, `id_estado_operativo`) VALUES
(1, 1, 2, 'Departamento moderno en Palermo', '2 ambientes, cocina integrada, balcón.', 250000.00, 'Av. Santa Fe 1234, CABA', -34.59972200, -58.38194400, '2025-12-12 00:16:39', '2025-12-17 00:16:39', '2025-12-17 00:16:39', 1, 1),
(2, 1, 3, 'Casa con jardín en Belgrano', '3 dormitorios, cochera, patio trasero.', 480000.00, 'Juramento 2345, CABA', -34.56722200, -58.45277800, '2025-12-15 00:16:39', '2025-12-17 00:16:39', '2025-12-17 00:16:39', 1, 1),
(3, 1, 2, 'Monoambiente en Microcentro', 'Ideal para estudiantes o ejecutivos.', 180000.00, 'Cordoba 567, CABA', -34.60333300, -58.38166700, '2025-12-07 00:16:39', '2025-12-17 00:16:39', '2025-12-17 00:16:39', 1, 1),
(4, 2, 5, 'Penthouse de lujo en Puerto Madero', 'Vistas al río, amenities, seguridad 24h.', 1200000.00, 'Alicia Moreau de Justo 1500, CABA', -34.61750000, -58.36333300, '2025-12-17 00:16:39', '2025-12-17 00:16:39', '2025-12-17 00:16:39', 1, 1);

-- --------------------------------------------------------

--
-- Table structure for table `roles`
--

CREATE TABLE `roles` (
  `id` tinyint(4) NOT NULL,
  `nombre` varchar(30) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `roles`
--

INSERT INTO `roles` (`id`, `nombre`) VALUES
(1, 'administrador'),
(2, 'agente'),
(3, 'asistente');

-- --------------------------------------------------------

--
-- Table structure for table `suscripciones`
--

CREATE TABLE `suscripciones` (
  `id` int(11) NOT NULL,
  `id_inmobiliaria` int(11) NOT NULL,
  `id_plan` int(11) NOT NULL,
  `inicio` datetime NOT NULL,
  `fin` datetime DEFAULT NULL,
  `renovacion_automatica` tinyint(1) DEFAULT 1,
  `creado_en` datetime DEFAULT current_timestamp(),
  `actualizado_en` datetime DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `id_estado` tinyint(4) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `uso_mensual`
--

CREATE TABLE `uso_mensual` (
  `id` int(11) NOT NULL,
  `id_inmobiliaria` int(11) NOT NULL,
  `mes` char(7) NOT NULL,
  `leads_generados` int(11) DEFAULT 0,
  `creado_en` datetime DEFAULT current_timestamp(),
  `actualizado_en` datetime DEFAULT current_timestamp() ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- --------------------------------------------------------

--
-- Table structure for table `usuarios`
--

CREATE TABLE `usuarios` (
  `id` int(11) NOT NULL,
  `id_inmobiliaria` int(11) NOT NULL,
  `nombre` varchar(100) NOT NULL,
  `email` varchar(100) NOT NULL,
  `hash_contrasena` varchar(255) NOT NULL,
  `telefono` varchar(20) DEFAULT NULL,
  `id_rol` tinyint(4) NOT NULL,
  `creado_en` datetime DEFAULT current_timestamp(),
  `actualizado_en` datetime DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `id_estado` tinyint(4) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `usuarios`
--

INSERT INTO `usuarios` (`id`, `id_inmobiliaria`, `nombre`, `email`, `hash_contrasena`, `telefono`, `id_rol`, `creado_en`, `actualizado_en`, `id_estado`) VALUES
(1, 1, 'Carlos Méndez', 'carlos@acme.com', 'AHf3Warur1VnCpA/W7+ZmxMq4r18Tca0yIgF/N0LXiQ=', '555-1234', 1, '2025-12-17 00:16:27', '2025-12-20 00:21:15', 1),
(2, 1, 'Ana López', 'ana@acme.com', '$2y$10$def456...', '555-5678', 2, '2025-12-17 00:16:27', '2025-12-17 00:16:27', 1),
(3, 1, 'Luis Ramírez', 'luis@acme.com', '$2y$10$ghi789...', '555-9012', 2, '2025-12-17 00:16:27', '2025-12-17 00:16:27', 1),
(4, 1, 'Marta Silva', 'marta@acme.com', '$2y$10$jkl012...', '555-3456', 3, '2025-12-17 00:16:27', '2025-12-17 00:16:27', 1),
(5, 2, 'Elena Torres', 'elena@elite.com', '$2y$10$mno345...', '555-7890', 1, '2025-12-17 00:16:27', '2025-12-17 00:16:27', 1);

--
-- Indexes for dumped tables
--

--
-- Indexes for table `busquedas_guardadas`
--
ALTER TABLE `busquedas_guardadas`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_email` (`email`),
  ADD KEY `idx_inmob_email` (`id_inmobiliaria`,`email`);

--
-- Indexes for table `estados_inmobiliaria`
--
ALTER TABLE `estados_inmobiliaria`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `uq_codigo` (`codigo`);

--
-- Indexes for table `estados_lead`
--
ALTER TABLE `estados_lead`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `nombre` (`nombre`);

--
-- Indexes for table `estados_lead_admin`
--
ALTER TABLE `estados_lead_admin`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `uq_codigo` (`codigo`);

--
-- Indexes for table `estados_propiedades_operativas`
--
ALTER TABLE `estados_propiedades_operativas`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `uq_codigo` (`codigo`);

--
-- Indexes for table `estados_propiedad_actidad`
--
ALTER TABLE `estados_propiedad_actidad`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `uq_codigo` (`codigo`);

--
-- Indexes for table `estados_suscripcion`
--
ALTER TABLE `estados_suscripcion`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `uq_codigo` (`codigo`);

--
-- Indexes for table `estados_usuario`
--
ALTER TABLE `estados_usuario`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `uq_codigo` (`codigo`);

--
-- Indexes for table `fuentes_contacto`
--
ALTER TABLE `fuentes_contacto`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `nombre` (`nombre`);

--
-- Indexes for table `imagenes_propiedades`
--
ALTER TABLE `imagenes_propiedades`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_propiedad_orden` (`id_propiedad`,`orden`);

--
-- Indexes for table `inmobiliarias`
--
ALTER TABLE `inmobiliarias`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `subdominio` (`subdominio`),
  ADD KEY `idx_subdominio` (`subdominio`),
  ADD KEY `fk_inmob_estado` (`id_estado`);

--
-- Indexes for table `leads`
--
ALTER TABLE `leads`
  ADD PRIMARY KEY (`id`),
  ADD KEY `id_fuente` (`id_fuente`),
  ADD KEY `id_estado` (`id_estado`),
  ADD KEY `idx_inmob_estado_lead` (`id_inmobiliaria`,`id_estado`),
  ADD KEY `idx_propiedad` (`id_propiedad`),
  ADD KEY `idx_usuario_asignado` (`id_usuario_asignado`),
  ADD KEY `fk_lead_estado_admin` (`id_estado_admin`);

--
-- Indexes for table `lead_estados_historial`
--
ALTER TABLE `lead_estados_historial`
  ADD PRIMARY KEY (`id`),
  ADD KEY `id_lead` (`id_lead`),
  ADD KEY `id_usuario` (`id_usuario`);

--
-- Indexes for table `planes`
--
ALTER TABLE `planes`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `uq_nombre` (`nombre`);

--
-- Indexes for table `propiedades`
--
ALTER TABLE `propiedades`
  ADD PRIMARY KEY (`id`),
  ADD KEY `id_agente_responsable` (`id_agente_responsable`),
  ADD KEY `idx_inmob_estado` (`id_inmobiliaria`),
  ADD KEY `idx_publicada` (`publicada_en`),
  ADD KEY `fk_prop_estado_admin` (`id_estado_admin`),
  ADD KEY `fk_prop_estado_operativo` (`id_estado_operativo`);

--
-- Indexes for table `roles`
--
ALTER TABLE `roles`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `nombre` (`nombre`);

--
-- Indexes for table `suscripciones`
--
ALTER TABLE `suscripciones`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_plan` (`id_plan`),
  ADD KEY `fk_suscripcion_estado` (`id_estado`);

--
-- Indexes for table `uso_mensual`
--
ALTER TABLE `uso_mensual`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `uq_inmob_mes` (`id_inmobiliaria`,`mes`);

--
-- Indexes for table `usuarios`
--
ALTER TABLE `usuarios`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `unic_inmob_email` (`id_inmobiliaria`,`email`),
  ADD KEY `id_rol` (`id_rol`),
  ADD KEY `idx_inmob_rol` (`id_inmobiliaria`,`id_rol`),
  ADD KEY `fk_usuario_estado` (`id_estado`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `busquedas_guardadas`
--
ALTER TABLE `busquedas_guardadas`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `estados_inmobiliaria`
--
ALTER TABLE `estados_inmobiliaria`
  MODIFY `id` tinyint(4) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT for table `estados_lead`
--
ALTER TABLE `estados_lead`
  MODIFY `id` tinyint(4) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT for table `estados_lead_admin`
--
ALTER TABLE `estados_lead_admin`
  MODIFY `id` tinyint(4) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT for table `estados_propiedades_operativas`
--
ALTER TABLE `estados_propiedades_operativas`
  MODIFY `id` tinyint(4) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT for table `estados_propiedad_actidad`
--
ALTER TABLE `estados_propiedad_actidad`
  MODIFY `id` tinyint(4) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT for table `estados_suscripcion`
--
ALTER TABLE `estados_suscripcion`
  MODIFY `id` tinyint(4) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT for table `estados_usuario`
--
ALTER TABLE `estados_usuario`
  MODIFY `id` tinyint(4) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT for table `fuentes_contacto`
--
ALTER TABLE `fuentes_contacto`
  MODIFY `id` tinyint(4) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=9;

--
-- AUTO_INCREMENT for table `imagenes_propiedades`
--
ALTER TABLE `imagenes_propiedades`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=9;

--
-- AUTO_INCREMENT for table `inmobiliarias`
--
ALTER TABLE `inmobiliarias`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT for table `leads`
--
ALTER TABLE `leads`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT for table `lead_estados_historial`
--
ALTER TABLE `lead_estados_historial`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `planes`
--
ALTER TABLE `planes`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT for table `propiedades`
--
ALTER TABLE `propiedades`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT for table `roles`
--
ALTER TABLE `roles`
  MODIFY `id` tinyint(4) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT for table `suscripciones`
--
ALTER TABLE `suscripciones`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `uso_mensual`
--
ALTER TABLE `uso_mensual`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `usuarios`
--
ALTER TABLE `usuarios`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `imagenes_propiedades`
--
ALTER TABLE `imagenes_propiedades`
  ADD CONSTRAINT `imagenes_propiedades_ibfk_1` FOREIGN KEY (`id_propiedad`) REFERENCES `propiedades` (`id`);

--
-- Constraints for table `inmobiliarias`
--
ALTER TABLE `inmobiliarias`
  ADD CONSTRAINT `fk_inmob_estado` FOREIGN KEY (`id_estado`) REFERENCES `estados_inmobiliaria` (`id`);

--
-- Constraints for table `leads`
--
ALTER TABLE `leads`
  ADD CONSTRAINT `fk_lead_estado_admin` FOREIGN KEY (`id_estado_admin`) REFERENCES `estados_lead_admin` (`id`),
  ADD CONSTRAINT `leads_ibfk_1` FOREIGN KEY (`id_propiedad`) REFERENCES `propiedades` (`id`),
  ADD CONSTRAINT `leads_ibfk_2` FOREIGN KEY (`id_inmobiliaria`) REFERENCES `inmobiliarias` (`id`),
  ADD CONSTRAINT `leads_ibfk_3` FOREIGN KEY (`id_usuario_asignado`) REFERENCES `usuarios` (`id`) ON DELETE SET NULL,
  ADD CONSTRAINT `leads_ibfk_4` FOREIGN KEY (`id_fuente`) REFERENCES `fuentes_contacto` (`id`),
  ADD CONSTRAINT `leads_ibfk_5` FOREIGN KEY (`id_estado`) REFERENCES `estados_lead` (`id`);

--
-- Constraints for table `lead_estados_historial`
--
ALTER TABLE `lead_estados_historial`
  ADD CONSTRAINT `lead_estados_historial_ibfk_1` FOREIGN KEY (`id_lead`) REFERENCES `leads` (`id`),
  ADD CONSTRAINT `lead_estados_historial_ibfk_2` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id`);

--
-- Constraints for table `propiedades`
--
ALTER TABLE `propiedades`
  ADD CONSTRAINT `fk_prop_estado_admin` FOREIGN KEY (`id_estado_admin`) REFERENCES `estados_propiedad_actidad` (`id`),
  ADD CONSTRAINT `fk_prop_estado_operativo` FOREIGN KEY (`id_estado_operativo`) REFERENCES `estados_propiedades_operativas` (`id`),
  ADD CONSTRAINT `propiedades_ibfk_1` FOREIGN KEY (`id_inmobiliaria`) REFERENCES `inmobiliarias` (`id`),
  ADD CONSTRAINT `propiedades_ibfk_2` FOREIGN KEY (`id_agente_responsable`) REFERENCES `usuarios` (`id`) ON DELETE SET NULL;

--
-- Constraints for table `suscripciones`
--
ALTER TABLE `suscripciones`
  ADD CONSTRAINT `fk_suscripcion_estado` FOREIGN KEY (`id_estado`) REFERENCES `estados_suscripcion` (`id`),
  ADD CONSTRAINT `suscripciones_ibfk_1` FOREIGN KEY (`id_inmobiliaria`) REFERENCES `inmobiliarias` (`id`),
  ADD CONSTRAINT `suscripciones_ibfk_2` FOREIGN KEY (`id_plan`) REFERENCES `planes` (`id`);

--
-- Constraints for table `uso_mensual`
--
ALTER TABLE `uso_mensual`
  ADD CONSTRAINT `uso_mensual_ibfk_1` FOREIGN KEY (`id_inmobiliaria`) REFERENCES `inmobiliarias` (`id`);

--
-- Constraints for table `usuarios`
--
ALTER TABLE `usuarios`
  ADD CONSTRAINT `fk_usuario_estado` FOREIGN KEY (`id_estado`) REFERENCES `estados_usuario` (`id`),
  ADD CONSTRAINT `usuarios_ibfk_1` FOREIGN KEY (`id_inmobiliaria`) REFERENCES `inmobiliarias` (`id`),
  ADD CONSTRAINT `usuarios_ibfk_2` FOREIGN KEY (`id_rol`) REFERENCES `roles` (`id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
