# 🧾 Agente Pagador - Hackatone 2026

url produccion: https://agentepagador.pages.dev/

**Agente Pagador ** es una solución Full Stack de automatización impulsada por Inteligencia Artificial, diseñada para la ingesta, evaluación y gestión de facturas. Este proyecto fue desarrollado para el hackathon **HackRadar**.

El sistema procesa facturas, evalúa reglas de negocio (ej. umbrales de aprobación humana, límites presupuestarios) y clasifica automáticamente los pagos utilizando un sistema de semáforos (Verde, Amarillo, Rojo).

---

## 🚀 Características Principales

- **Automatización con IA:** Extracción de datos estructurados vía LLM y evaluación basada en reglas de negocio dinámicas.
- **Procesamiento Asíncrono:** Arquitectura orientada a eventos utilizando RabbitMQ para asegurar que el sistema nunca se bloquee bajo alta carga.
- **Evaluación Estricta de Proveedores:** Bloqueo inquebrantable (Semáforo Rojo) y categorización de ilegalidad para entidades no autorizadas en la base de datos.
- **Asistente Virtual Integrado:** Chatbot interactivo "IA Dani" con avatar de gato arcade para asistir en la comprensión de las reglas de auditoría.
- **Interfaz Bilingüe:** Soporte nativo y dinámico para Español (ES) e Inglés (EN).
- **Diseño Responsivo e Intuitivo:** Interfaz limpia construida con Angular, utilizando la paleta de colores corporativa "Blinka" y tipografía Montserrat.

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
- IA y Extracción: **Groq API** (`openai/gpt-oss-20b`).
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
GROQ_API_KEY=tu_api_key_de_groq
FRONTEND_URL=http://localhost:4200 # En producción, la URL de Cloudflare
