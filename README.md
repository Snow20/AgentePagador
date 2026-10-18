# 🧾 Agente Pagador Blinka - HackRadar 2026

**Agente Pagador Blinka** es una solución Full Stack de automatización impulsada por Inteligencia Artificial, diseñada para la ingesta, evaluación y gestión de facturas. Este proyecto fue desarrollado para el hackathon **HackRadar**.

El sistema procesa facturas, evalúa reglas de negocio (ej. umbrales de aprobación humana) y clasifica automáticamente los pagos utilizando un sistema de semáforos (Verde, Amarillo, Rojo).

---

## 🚀 Características Principales

- **Automatización con IA:** Evaluación de facturas basada en reglas de negocio dinámicas.
- **Procesamiento Asíncrono:** Arquitectura orientada a eventos utilizando RabbitMQ para asegurar que el sistema nunca se bloquee bajo alta carga.
- **Interfaz Bilingüe:** Soporte nativo y dinámico para Español (ES) e Inglés (EN).
- **Diseño Responsivo e Intuitivo:** Interfaz limpia construida con Angular, utilizando la paleta de colores corporativa "Blinka" (Verde y Blanco) y tipografía Montserrat.

---

## 🛠️ Stack Tecnológico y Arquitectura

El proyecto utiliza una arquitectura separada (Frontend desacoplado) con un Backend monolítico que integra un *Worker* en segundo plano.

**Frontend:**
- Framework: **Angular 18** (con SSR).
- Estilos: CSS puro con variables de entorno (Tema Claro exclusivo).
- Alojamiento: **Cloudflare Pages**.

**Backend:**
- Framework: **ASP.NET Core 10** (C#).
- ORM: **Entity Framework Core**.
- Base de Datos: **PostgreSQL 16** (Alojado en Render).
- Message Broker: **RabbitMQ** (Alojado en CloudAMQP).
- Alojamiento: **Render** (Contenedor Docker unificado para API y Worker).

---

## ⚙️ Configuración y Despliegue Local

Para ejecutar este proyecto en un entorno local, asegúrate de tener instalados Node.js, .NET 10 SDK, y acceso a instancias de PostgreSQL y RabbitMQ.

### 1. Variables de Entorno (Backend)
El servicio web requiere las siguientes variables de entorno para funcionar correctamente:

```env
DB_CONNECTION_STRING=Host=tu_host;Database=tu_db;Username=tu_user;Password=tu_pass;Port=5432
MQ_HOST=tu_host_cloudamqp
MQ_USER=tu_usuario_amqp
MQ_PASS=tu_password_amqp
FRONTEND_URL=http://localhost:4200 # En producción, la URL de Cloudflare
```

### 2. Ejecutar el Backend
```bash
cd backend/src
dotnet restore
dotnet run --project Agent.Api
```

### 3. Ejecutar el Frontend
```bash
cd frontend
npm install
npm run start
```

---

## 📄 Licencia
© Copyright hecho por Fenrirsoft 2026. Todos los derechos reservados.