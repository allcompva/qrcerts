USE [QRCerts]
GO
/****** Object:  Trigger [trg_REGISTRO_ALUMNOS_UpdateNombre]    Script Date: 5/5/2026 21:26:12 ******/
DROP TRIGGER IF EXISTS [dbo].[trg_REGISTRO_ALUMNOS_UpdateNombre]
GO
/****** Object:  Trigger [trg_Certificados_Upsert]    Script Date: 5/5/2026 21:26:12 ******/
DROP TRIGGER IF EXISTS [dbo].[trg_Certificados_Upsert]
GO
/****** Object:  Trigger [trg_Certificados_Delete]    Script Date: 5/5/2026 21:26:12 ******/
DROP TRIGGER IF EXISTS [dbo].[trg_Certificados_Delete]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OtecQuotaConfig]') AND type in (N'U'))
ALTER TABLE [dbo].[OtecQuotaConfig] DROP CONSTRAINT IF EXISTS [FK_OtecQuotaConfig_Otec]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrdenCompraHistorial]') AND type in (N'U'))
ALTER TABLE [dbo].[OrdenCompraHistorial] DROP CONSTRAINT IF EXISTS [FK_OrdenCompraHistorial_Orden]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrdenCompra]') AND type in (N'U'))
ALTER TABLE [dbo].[OrdenCompra] DROP CONSTRAINT IF EXISTS [FK_OrdenCompra_Otec]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MoodleConfigs]') AND type in (N'U'))
ALTER TABLE [dbo].[MoodleConfigs] DROP CONSTRAINT IF EXISTS [FK_MoodleConfigs_Otecs]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PdfJobs]') AND type in (N'U'))
ALTER TABLE [dbo].[PdfJobs] DROP CONSTRAINT IF EXISTS [DF__PdfJobs__Created__71D1E811]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Otecs]') AND type in (N'U'))
ALTER TABLE [dbo].[Otecs] DROP CONSTRAINT IF EXISTS [DF__Otecs__moodleHab__02FC7413]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OtecQuotaConfig]') AND type in (N'U'))
ALTER TABLE [dbo].[OtecQuotaConfig] DROP CONSTRAINT IF EXISTS [DF__OtecQuota__Updat__19DFD96B]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OtecQuotaConfig]') AND type in (N'U'))
ALTER TABLE [dbo].[OtecQuotaConfig] DROP CONSTRAINT IF EXISTS [DF__OtecQuota__Creat__18EBB532]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OtecQuotaConfig]') AND type in (N'U'))
ALTER TABLE [dbo].[OtecQuotaConfig] DROP CONSTRAINT IF EXISTS [DF__OtecQuota__Quota__17F790F9]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OtecQuotaConfig]') AND type in (N'U'))
ALTER TABLE [dbo].[OtecQuotaConfig] DROP CONSTRAINT IF EXISTS [DF__OtecQuotaCon__Id__17036CC0]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrdenCompraHistorial]') AND type in (N'U'))
ALTER TABLE [dbo].[OrdenCompraHistorial] DROP CONSTRAINT IF EXISTS [DF__OrdenComp__Creat__25518C17]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrdenCompraHistorial]') AND type in (N'U'))
ALTER TABLE [dbo].[OrdenCompraHistorial] DROP CONSTRAINT IF EXISTS [DF__OrdenCompraH__Id__245D67DE]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrdenCompra]') AND type in (N'U'))
ALTER TABLE [dbo].[OrdenCompra] DROP CONSTRAINT IF EXISTS [DF__OrdenComp__Creat__208CD6FA]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrdenCompra]') AND type in (N'U'))
ALTER TABLE [dbo].[OrdenCompra] DROP CONSTRAINT IF EXISTS [DF__OrdenComp__Activ__1F98B2C1]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrdenCompra]') AND type in (N'U'))
ALTER TABLE [dbo].[OrdenCompra] DROP CONSTRAINT IF EXISTS [DF__OrdenComp__Canti__1EA48E88]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrdenCompra]') AND type in (N'U'))
ALTER TABLE [dbo].[OrdenCompra] DROP CONSTRAINT IF EXISTS [DF__OrdenCompra__Id__1DB06A4F]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MoodleConfigs]') AND type in (N'U'))
ALTER TABLE [dbo].[MoodleConfigs] DROP CONSTRAINT IF EXISTS [DF__MoodleCon__Updat__09A971A2]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MoodleConfigs]') AND type in (N'U'))
ALTER TABLE [dbo].[MoodleConfigs] DROP CONSTRAINT IF EXISTS [DF__MoodleCon__Creat__08B54D69]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MoodleConfigs]') AND type in (N'U'))
ALTER TABLE [dbo].[MoodleConfigs] DROP CONSTRAINT IF EXISTS [DF__MoodleCon__Activ__07C12930]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MoodleConfigs]') AND type in (N'U'))
ALTER TABLE [dbo].[MoodleConfigs] DROP CONSTRAINT IF EXISTS [DF__MoodleConfig__Id__06CD04F7]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Cursos]') AND type in (N'U'))
ALTER TABLE [dbo].[Cursos] DROP CONSTRAINT IF EXISTS [DF_Cursos_certificate_type]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Certificados_historico]') AND type in (N'U'))
ALTER TABLE [dbo].[Certificados_historico] DROP CONSTRAINT IF EXISTS [DF__Certifica__Delet__5FB337D6]
GO
/****** Object:  Index [IX_REGISTRO_ALUMNOS_MoodleIds]    Script Date: 5/5/2026 21:26:12 ******/
DROP INDEX IF EXISTS [IX_REGISTRO_ALUMNOS_MoodleIds] ON [dbo].[REGISTRO_ALUMNOS]
GO
/****** Object:  Index [IX_OrdenCompraHistorial_OtecId]    Script Date: 5/5/2026 21:26:12 ******/
DROP INDEX IF EXISTS [IX_OrdenCompraHistorial_OtecId] ON [dbo].[OrdenCompraHistorial]
GO
/****** Object:  Index [IX_OrdenCompra_OtecId_Activa]    Script Date: 5/5/2026 21:26:12 ******/
DROP INDEX IF EXISTS [IX_OrdenCompra_OtecId_Activa] ON [dbo].[OrdenCompra]
GO
/****** Object:  Table [dbo].[usuarios]    Script Date: 5/5/2026 21:26:12 ******/
DROP TABLE IF EXISTS [dbo].[usuarios]
GO
/****** Object:  Table [dbo].[REGISTRO_ALUMNOS]    Script Date: 5/5/2026 21:26:12 ******/
DROP TABLE IF EXISTS [dbo].[REGISTRO_ALUMNOS]
GO
/****** Object:  Table [dbo].[plantilla_certificados]    Script Date: 5/5/2026 21:26:12 ******/
DROP TABLE IF EXISTS [dbo].[plantilla_certificados]
GO
/****** Object:  Table [dbo].[PdfJobs]    Script Date: 5/5/2026 21:26:12 ******/
DROP TABLE IF EXISTS [dbo].[PdfJobs]
GO
/****** Object:  Table [dbo].[OtecUsers]    Script Date: 5/5/2026 21:26:12 ******/
DROP TABLE IF EXISTS [dbo].[OtecUsers]
GO
/****** Object:  Table [dbo].[Otecs]    Script Date: 5/5/2026 21:26:12 ******/
DROP TABLE IF EXISTS [dbo].[Otecs]
GO
/****** Object:  Table [dbo].[OtecQuotaConfig]    Script Date: 5/5/2026 21:26:12 ******/
DROP TABLE IF EXISTS [dbo].[OtecQuotaConfig]
GO
/****** Object:  Table [dbo].[OrdenCompraHistorial]    Script Date: 5/5/2026 21:26:12 ******/
DROP TABLE IF EXISTS [dbo].[OrdenCompraHistorial]
GO
/****** Object:  Table [dbo].[OrdenCompra]    Script Date: 5/5/2026 21:26:12 ******/
DROP TABLE IF EXISTS [dbo].[OrdenCompra]
GO
/****** Object:  Table [dbo].[MoodleConfigs]    Script Date: 5/5/2026 21:26:12 ******/
DROP TABLE IF EXISTS [dbo].[MoodleConfigs]
GO
/****** Object:  Table [dbo].[Emisiones]    Script Date: 5/5/2026 21:26:12 ******/
DROP TABLE IF EXISTS [dbo].[Emisiones]
GO
/****** Object:  Table [dbo].[Cursos]    Script Date: 5/5/2026 21:26:12 ******/
DROP TABLE IF EXISTS [dbo].[Cursos]
GO
/****** Object:  Table [dbo].[contenido_certificado]    Script Date: 5/5/2026 21:26:12 ******/
DROP TABLE IF EXISTS [dbo].[contenido_certificado]
GO
/****** Object:  Table [dbo].[Certificados_historico]    Script Date: 5/5/2026 21:26:12 ******/
DROP TABLE IF EXISTS [dbo].[Certificados_historico]
GO
/****** Object:  Table [dbo].[Certificados]    Script Date: 5/5/2026 21:26:12 ******/
DROP TABLE IF EXISTS [dbo].[Certificados]
GO
/****** Object:  Table [dbo].[Alumnos]    Script Date: 5/5/2026 21:26:12 ******/
DROP TABLE IF EXISTS [dbo].[Alumnos]
GO
/****** Object:  Table [dbo].[AdminUsers]    Script Date: 5/5/2026 21:26:12 ******/
DROP TABLE IF EXISTS [dbo].[AdminUsers]
GO
USE [master]
GO
/****** Object:  Database [QRCerts]    Script Date: 5/5/2026 21:26:12 ******/
DROP DATABASE IF EXISTS [QRCerts]
GO
/****** Object:  Database [QRCerts]    Script Date: 5/5/2026 21:26:12 ******/
CREATE DATABASE [QRCerts]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'QRCerts', FILENAME = N'/var/opt/mssql/data/QRCerts.mdf' , SIZE = 73728KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'QRCerts_log', FILENAME = N'/var/opt/mssql/data/QRCerts_log.ldf' , SIZE = 270336KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [QRCerts] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [QRCerts].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [QRCerts] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [QRCerts] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [QRCerts] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [QRCerts] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [QRCerts] SET ARITHABORT OFF 
GO
ALTER DATABASE [QRCerts] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [QRCerts] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [QRCerts] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [QRCerts] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [QRCerts] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [QRCerts] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [QRCerts] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [QRCerts] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [QRCerts] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [QRCerts] SET  ENABLE_BROKER 
GO
ALTER DATABASE [QRCerts] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [QRCerts] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [QRCerts] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [QRCerts] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [QRCerts] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [QRCerts] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [QRCerts] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [QRCerts] SET RECOVERY FULL 
GO
ALTER DATABASE [QRCerts] SET  MULTI_USER 
GO
ALTER DATABASE [QRCerts] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [QRCerts] SET DB_CHAINING OFF 
GO
ALTER DATABASE [QRCerts] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [QRCerts] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [QRCerts] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [QRCerts] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [QRCerts] SET QUERY_STORE = ON
GO
ALTER DATABASE [QRCerts] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [QRCerts]
GO
/****** Object:  Table [dbo].[AdminUsers]    Script Date: 5/5/2026 21:26:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AdminUsers](
	[Id] [nvarchar](450) NOT NULL,
	[Username] [nvarchar](100) NOT NULL,
	[PasswordHash] [nvarchar](255) NOT NULL,
	[Email] [nvarchar](200) NULL,
	[FullName] [nvarchar](100) NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[LastLoginAt] [datetime2](7) NULL,
 CONSTRAINT [PK_AdminUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Alumnos]    Script Date: 5/5/2026 21:26:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Alumnos](
	[Id] [uniqueidentifier] NOT NULL,
	[OtecId] [uniqueidentifier] NOT NULL,
	[NombreApellido] [nvarchar](max) NOT NULL,
	[RUT] [nvarchar](450) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[parametros] [varchar](max) NULL,
 CONSTRAINT [PK_Alumnos] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Certificados]    Script Date: 5/5/2026 21:26:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Certificados](
	[Id] [uniqueidentifier] NOT NULL,
	[CursoId] [uniqueidentifier] NOT NULL,
	[AlumnoId] [uniqueidentifier] NOT NULL,
	[Code] [nvarchar](450) NOT NULL,
	[PdfFilename] [nvarchar](450) NOT NULL,
	[IssuedAt] [datetime2](7) NOT NULL,
	[Estado] [tinyint] NOT NULL,
	[url_landing] [varchar](max) NULL,
 CONSTRAINT [PK_Certificados] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Certificados_historico]    Script Date: 5/5/2026 21:26:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Certificados_historico](
	[Id] [uniqueidentifier] NOT NULL,
	[CursoId] [uniqueidentifier] NOT NULL,
	[AlumnoId] [uniqueidentifier] NOT NULL,
	[Code] [nvarchar](450) NOT NULL,
	[PdfFilename] [nvarchar](450) NOT NULL,
	[IssuedAt] [datetime2](7) NOT NULL,
	[Estado] [tinyint] NOT NULL,
	[url_landing] [varchar](max) NULL,
	[IdOriginal] [uniqueidentifier] NOT NULL,
	[DeletedAt] [datetime] NOT NULL,
 CONSTRAINT [PK_Certificados_historico] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[contenido_certificado]    Script Date: 5/5/2026 21:26:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[contenido_certificado](
	[id] [uniqueidentifier] NOT NULL,
	[id_certificado] [uniqueidentifier] NULL,
	[nombre_curso] [varchar](500) NULL,
	[nombre_alumno] [varchar](max) NULL,
	[RUT] [varchar](450) NULL,
	[certificate_type] [varchar](50) NULL,
	[footer_1] [varchar](255) NULL,
	[footer_2] [varchar](255) NULL,
	[contenidoHtml] [varchar](max) NULL,
	[footerHtml] [varchar](max) NULL,
	[fecha] [datetime] NULL,
	[nombre_referencia_curso] [varchar](500) NOT NULL,
	[imagen_fondo] [varchar](max) NULL,
 CONSTRAINT [PK_contenido_certificado] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Cursos]    Script Date: 5/5/2026 21:26:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Cursos](
	[Id] [uniqueidentifier] NOT NULL,
	[OtecId] [uniqueidentifier] NOT NULL,
	[NombreReferencia] [nvarchar](max) NOT NULL,
	[QrDestino] [tinyint] NOT NULL,
	[FondoPath] [nvarchar](max) NOT NULL,
	[LayoutJson] [nvarchar](max) NULL,
	[Estado] [tinyint] NOT NULL,
	[IsFondoBloqueado] [bit] NOT NULL,
	[IsLayoutBloqueado] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
	[footer_1] [varchar](255) NULL,
	[footer_2] [varchar](255) NULL,
	[nombre_visualizar_certificado] [nvarchar](500) NULL,
	[certificate_type] [varchar](50) NOT NULL,
	[contenidoHtml] [varchar](max) NULL,
	[footerHtml] [varchar](max) NULL,
	[vencimiento] [date] NULL,
	[PlantillaId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Cursos] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Emisiones]    Script Date: 5/5/2026 21:26:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Emisiones](
	[Id] [uniqueidentifier] NOT NULL,
	[CursoId] [uniqueidentifier] NOT NULL,
	[Total] [int] NOT NULL,
	[Generados] [int] NOT NULL,
	[Estado] [tinyint] NOT NULL,
	[StartedAt] [datetime2](7) NOT NULL,
	[FinishedAt] [datetime2](7) NULL,
	[Log] [nvarchar](max) NULL,
 CONSTRAINT [PK_Emisiones] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MoodleConfigs]    Script Date: 5/5/2026 21:26:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MoodleConfigs](
	[Id] [uniqueidentifier] NOT NULL,
	[OtecId] [uniqueidentifier] NOT NULL,
	[MoodleUrl] [nvarchar](500) NOT NULL,
	[Token] [nvarchar](500) NOT NULL,
	[Activo] [bit] NOT NULL,
	[UltimaConexionExitosa] [datetime2](7) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_MoodleConfigs_OtecId] UNIQUE NONCLUSTERED 
(
	[OtecId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrdenCompra]    Script Date: 5/5/2026 21:26:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrdenCompra](
	[Id] [uniqueidentifier] NOT NULL,
	[OtecId] [uniqueidentifier] NOT NULL,
	[CantidadComprada] [int] NOT NULL,
	[CantidadUsada] [int] NOT NULL,
	[FechaExpiracion] [datetime2](7) NOT NULL,
	[Activa] [bit] NOT NULL,
	[CreadaPor] [nvarchar](200) NOT NULL,
	[Notas] [nvarchar](500) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrdenCompraHistorial]    Script Date: 5/5/2026 21:26:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrdenCompraHistorial](
	[Id] [uniqueidentifier] NOT NULL,
	[OrdenCompraId] [uniqueidentifier] NOT NULL,
	[OtecId] [uniqueidentifier] NOT NULL,
	[Evento] [nvarchar](50) NOT NULL,
	[Detalle] [nvarchar](500) NULL,
	[CreadaPor] [nvarchar](200) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OtecQuotaConfig]    Script Date: 5/5/2026 21:26:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OtecQuotaConfig](
	[Id] [uniqueidentifier] NOT NULL,
	[OtecId] [uniqueidentifier] NOT NULL,
	[QuotaActivo] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_OtecQuotaConfig_OtecId] UNIQUE NONCLUSTERED 
(
	[OtecId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Otecs]    Script Date: 5/5/2026 21:26:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Otecs](
	[Id] [uniqueidentifier] NOT NULL,
	[Nombre] [nvarchar](max) NOT NULL,
	[Slug] [nvarchar](450) NOT NULL,
	[Estado] [tinyint] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[moodleHabilitado] [bit] NOT NULL,
 CONSTRAINT [PK_Otecs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OtecUsers]    Script Date: 5/5/2026 21:26:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OtecUsers](
	[Id] [uniqueidentifier] NOT NULL,
	[OtecId] [uniqueidentifier] NOT NULL,
	[NombreApellido] [nvarchar](max) NOT NULL,
	[RUT] [nvarchar](max) NOT NULL,
	[Email] [nvarchar](max) NOT NULL,
	[Username] [nvarchar](450) NOT NULL,
	[Estado] [tinyint] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[PasswordHash] [nvarchar](255) NOT NULL,
	[LastLoginAt] [datetime2](7) NULL,
 CONSTRAINT [PK_OtecUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PdfJobs]    Script Date: 5/5/2026 21:26:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PdfJobs](
	[Id] [uniqueidentifier] NOT NULL,
	[OtecId] [uniqueidentifier] NOT NULL,
	[CourseId] [uniqueidentifier] NOT NULL,
	[AlumnoIdsCsv] [nvarchar](max) NOT NULL,
	[TipoJob] [int] NOT NULL,
	[Estado] [int] NOT NULL,
	[Prioridad] [int] NOT NULL,
	[OutputPath] [nvarchar](500) NULL,
	[Error] [nvarchar](max) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[plantilla_certificados]    Script Date: 5/5/2026 21:26:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[plantilla_certificados](
	[id] [uniqueidentifier] NOT NULL,
	[nombre] [varchar](255) NULL,
	[contenido_cursos] [varchar](max) NULL,
	[path_docx] [varchar](500) NULL,
	[id_otec] [uniqueidentifier] NULL,
	[contenido_alumnos] [text] NULL,
 CONSTRAINT [PK_PLANTILLAS] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[REGISTRO_ALUMNOS]    Script Date: 5/5/2026 21:26:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[REGISTRO_ALUMNOS](
	[id_alumno] [uniqueidentifier] NOT NULL,
	[id_curso] [uniqueidentifier] NOT NULL,
	[calificacion] [varchar](255) NULL,
	[observaciones] [varchar](max) NULL,
	[certificado_otorgado] [varchar](255) NULL,
	[motivo_entrega] [varchar](max) NULL,
	[MoodleUserId] [int] NULL,
	[MoodleCourseId] [int] NULL,
 CONSTRAINT [PK_REGISTRO_ALUMNOS] PRIMARY KEY CLUSTERED 
(
	[id_alumno] ASC,
	[id_curso] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usuarios]    Script Date: 5/5/2026 21:26:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usuarios](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[nombre_usuario] [varchar](100) NULL,
	[pass] [varchar](255) NULL,
	[Nombre] [varchar](100) NULL,
	[Apellido] [varchar](100) NULL,
	[dni] [varchar](14) NULL,
	[mail] [varchar](100) NULL,
	[rol] [int] NULL,
	[ID_EMPRESA] [int] NULL,
	[sexo] [char](1) NULL,
	[fecha_nacimiento] [varchar](12) NULL,
	[grado] [varchar](50) NULL,
	[CONFIRMADO] [bit] NULL,
	[CELULAR] [varchar](50) NULL,
	[ID_EMPRESA_CLIENTE] [int] NULL,
 CONSTRAINT [pk_usu] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Index [IX_OrdenCompra_OtecId_Activa]    Script Date: 5/5/2026 21:26:12 ******/
CREATE NONCLUSTERED INDEX [IX_OrdenCompra_OtecId_Activa] ON [dbo].[OrdenCompra]
(
	[OtecId] ASC,
	[Activa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_OrdenCompraHistorial_OtecId]    Script Date: 5/5/2026 21:26:12 ******/
CREATE NONCLUSTERED INDEX [IX_OrdenCompraHistorial_OtecId] ON [dbo].[OrdenCompraHistorial]
(
	[OtecId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_REGISTRO_ALUMNOS_MoodleIds]    Script Date: 5/5/2026 21:26:12 ******/
CREATE NONCLUSTERED INDEX [IX_REGISTRO_ALUMNOS_MoodleIds] ON [dbo].[REGISTRO_ALUMNOS]
(
	[MoodleUserId] ASC,
	[MoodleCourseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Certificados_historico] ADD  DEFAULT (getdate()) FOR [DeletedAt]
GO
ALTER TABLE [dbo].[Cursos] ADD  CONSTRAINT [DF_Cursos_certificate_type]  DEFAULT ('HORIZONTAL') FOR [certificate_type]
GO
ALTER TABLE [dbo].[MoodleConfigs] ADD  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [dbo].[MoodleConfigs] ADD  DEFAULT ((1)) FOR [Activo]
GO
ALTER TABLE [dbo].[MoodleConfigs] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[MoodleConfigs] ADD  DEFAULT (getutcdate()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[OrdenCompra] ADD  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [dbo].[OrdenCompra] ADD  DEFAULT ((0)) FOR [CantidadUsada]
GO
ALTER TABLE [dbo].[OrdenCompra] ADD  DEFAULT ((1)) FOR [Activa]
GO
ALTER TABLE [dbo].[OrdenCompra] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[OrdenCompraHistorial] ADD  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [dbo].[OrdenCompraHistorial] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[OtecQuotaConfig] ADD  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [dbo].[OtecQuotaConfig] ADD  DEFAULT ((0)) FOR [QuotaActivo]
GO
ALTER TABLE [dbo].[OtecQuotaConfig] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[OtecQuotaConfig] ADD  DEFAULT (getutcdate()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[Otecs] ADD  DEFAULT ((0)) FOR [moodleHabilitado]
GO
ALTER TABLE [dbo].[PdfJobs] ADD  DEFAULT (sysdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[MoodleConfigs]  WITH CHECK ADD  CONSTRAINT [FK_MoodleConfigs_Otecs] FOREIGN KEY([OtecId])
REFERENCES [dbo].[Otecs] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MoodleConfigs] CHECK CONSTRAINT [FK_MoodleConfigs_Otecs]
GO
ALTER TABLE [dbo].[OrdenCompra]  WITH CHECK ADD  CONSTRAINT [FK_OrdenCompra_Otec] FOREIGN KEY([OtecId])
REFERENCES [dbo].[Otecs] ([Id])
GO
ALTER TABLE [dbo].[OrdenCompra] CHECK CONSTRAINT [FK_OrdenCompra_Otec]
GO
ALTER TABLE [dbo].[OrdenCompraHistorial]  WITH CHECK ADD  CONSTRAINT [FK_OrdenCompraHistorial_Orden] FOREIGN KEY([OrdenCompraId])
REFERENCES [dbo].[OrdenCompra] ([Id])
GO
ALTER TABLE [dbo].[OrdenCompraHistorial] CHECK CONSTRAINT [FK_OrdenCompraHistorial_Orden]
GO
ALTER TABLE [dbo].[OtecQuotaConfig]  WITH CHECK ADD  CONSTRAINT [FK_OtecQuotaConfig_Otec] FOREIGN KEY([OtecId])
REFERENCES [dbo].[Otecs] ([Id])
GO
ALTER TABLE [dbo].[OtecQuotaConfig] CHECK CONSTRAINT [FK_OtecQuotaConfig_Otec]
GO
/****** Object:  Trigger [dbo].[trg_Certificados_Delete]    Script Date: 5/5/2026 21:26:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TRIGGER [dbo].[trg_Certificados_Delete]
ON [dbo].[Certificados]
FOR DELETE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Certificados_historico (
        IdOriginal,
        DeletedAt,
        Id,
        CursoId,
        AlumnoId,
        Code,
        PdfFilename,
        IssuedAt,
        Estado,
        url_landing
    )
    SELECT
        d.Id AS IdOriginal,
        GETDATE() AS DeletedAt,
        NEWID() AS Id,  -- nuevo Id para el histórico
        d.CursoId,
        d.AlumnoId,
        d.Code,
        d.PdfFilename,
        d.IssuedAt,
        d.Estado,
        d.url_landing
    FROM deleted d;
END;
GO
ALTER TABLE [dbo].[Certificados] ENABLE TRIGGER [trg_Certificados_Delete]
GO
/****** Object:  Trigger [dbo].[trg_Certificados_Upsert]    Script Date: 5/5/2026 21:26:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TRIGGER [dbo].[trg_Certificados_Upsert]
ON [dbo].[Certificados]
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;

    ----------------------------------------------------------------
    -- 1) Borrar certificados anteriores del mismo (CursoId, AlumnoId)
    --    Esto SÍ va a disparar tu trigger FOR DELETE y se va al histórico
    ----------------------------------------------------------------
    DELETE C
    FROM dbo.Certificados AS C
    INNER JOIN inserted AS i
        ON C.CursoId = i.CursoId
       AND C.AlumnoId = i.AlumnoId
       -- por si algún día alguien mete el mismo Id en el insert (raro, pero igual):
       AND C.Id <> i.Id;

    ----------------------------------------------------------------
    -- 2) Insertar realmente los nuevos (lo que vino en inserted)
    ----------------------------------------------------------------
    INSERT INTO dbo.Certificados (
        Id,
        CursoId,
        AlumnoId,
        Code,
        PdfFilename,
        IssuedAt,
        Estado,
        url_landing
    )
    SELECT
        Id,
        CursoId,
        AlumnoId,
        Code,
        PdfFilename,
        IssuedAt,
        Estado,
        url_landing
    FROM inserted;

    ----------------------------------------------------------------
    -- 3) Sacar la foto en contenido_certificado
    ----------------------------------------------------------------
    INSERT INTO dbo.contenido_certificado (
        id,
        id_certificado,
        nombre_curso,
        nombre_referencia_curso,
        nombre_alumno,
        RUT,
        certificate_type,
        footer_1,
        footer_2,
        contenidoHtml,
        footerHtml,
        fecha
    )
    SELECT
        NEWID() AS id,
        i.Id    AS id_certificado,
        ISNULL(cu.nombre_visualizar_certificado, '')      AS nombre_curso,
        ISNULL(cu.NombreReferencia, '')  AS nombre_referencia_curso,
        ISNULL(al.NombreApellido, '')    AS nombre_alumno,
        ISNULL(al.RUT, '')               AS RUT,
        ISNULL(cu.certificate_type, '')  AS certificate_type,
        ISNULL(cu.footer_1, '')          AS footer_1,
        ISNULL(cu.footer_2, '')          AS footer_2,
        ISNULL(cu.contenidoHtml, '')     AS contenidoHtml,
        ISNULL(cu.footerHtml, '')        AS footerHtml,
        GETDATE() AS fecha
    FROM inserted i
    INNER JOIN dbo.Cursos  cu ON cu.Id = i.CursoId
    INNER JOIN dbo.Alumnos al ON al.Id = i.AlumnoId;
END;
GO
ALTER TABLE [dbo].[Certificados] ENABLE TRIGGER [trg_Certificados_Upsert]
GO
/****** Object:  Trigger [dbo].[trg_REGISTRO_ALUMNOS_UpdateNombre]    Script Date: 5/5/2026 21:26:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   TRIGGER [dbo].[trg_REGISTRO_ALUMNOS_UpdateNombre]
ON [dbo].[REGISTRO_ALUMNOS]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        ;WITH cte AS (
            SELECT
                i.id_alumno,
                LTRIM(RTRIM(JSON_VALUE(i.observaciones, '$.NOMBRE'))) AS Nombre
            FROM inserted i
            WHERE 
                i.observaciones IS NOT NULL
                AND ISJSON(i.observaciones) = 1
                AND JSON_VALUE(i.observaciones, '$.NOMBRE') IS NOT NULL
        )
        UPDATE a
        SET NombreApellido = cte.Nombre
        FROM dbo.Alumnos a
        INNER JOIN cte
            -- Si Alumnos.id es UNIQUEIDENTIFIER, convertí; si es varchar, usá = cte.id_alumno
            ON a.id = TRY_CONVERT(uniqueidentifier, cte.id_alumno)
        WHERE
            -- evitar updates innecesarios (nulls y equality)
            ISNULL(a.NombreApellido, '') <> ISNULL(cte.Nombre, '')
            AND ISNULL(cte.Nombre, '') <> '';
    END TRY
    BEGIN CATCH
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrNum INT = ERROR_NUMBER();
        THROW @ErrNum, @ErrMsg, 1;
    END CATCH
END;
GO
ALTER TABLE [dbo].[REGISTRO_ALUMNOS] ENABLE TRIGGER [trg_REGISTRO_ALUMNOS_UpdateNombre]
GO
USE [master]
GO
ALTER DATABASE [QRCerts] SET  READ_WRITE 
GO
