# 📖 Manual de Usuario y Documentación Técnica
**Agente Pagador**

Este documento está dividido en dos partes principales: una guía didáctica sobre cómo utilizar la plataforma y una documentación técnica detallada sobre cómo se construyó y desplegó la solución.

---

## PARTE 1: Guía de Usuario (Cómo usar la aplicación)

La interfaz del Agente Pagador está diseñada para ser extremadamente simple y directa.

### 1. Inicio y Selección de Idioma
Al ingresar a la plataforma, verás el título "Agente Pagador" y la paleta de colores corporativa. 
- En la esquina superior derecha, encontrarás un botón con el ícono de un globo terráqueo.
- Haz clic en este botón para alternar instantáneamente todo el texto de la aplicación entre **Español (ES)** e **Inglés (EN)**.

### 2. Ingesta de Facturas (Subida)
La sección central de la pantalla es la **Ingesta de Facturas**. Aquí es donde entregas los documentos a la Inteligencia Artificial.
1. Haz clic en el botón **"Seleccionar archivo"**.
2. Elige un documento en formato PDF o una Imagen desde tu dispositivo.
3. Una vez seleccionado, el botón inferior se activará. Haz clic en **"Procesar Factura"**.
4. El botón cambiará su texto a "Procesando..." mientras el documento es enviado de forma segura al Agente API.

### 3. Panel de Resultados y Semáforos
Una vez que el archivo es procesado, la IA evalúa las reglas (como umbrales de monto o validación de proveedores) y emite un veredicto.
- Dirígete a la sección inferior llamada **Panel de Resultados**.
- Haz clic en **"Actualizar Resultados"** para sincronizar los datos más recientes desde la base de datos.
- Las facturas aparecerán clasificadas mediante un sistema visual de semáforos:
  - 🟢 **Verde (Pagada):** La factura cumplió todas las reglas y fue aprobada automáticamente.
  - 🟡 **Amarillo (Pendiente/Revisión):** La factura superó el umbral de aprobación automática o requiere validación humana.
  - 🔴 **Rojo (Frenada):** Se detectó una anomalía grave o proveedor no autorizado.

---

## PARTE 2: Documentación Técnica (Cómo se construyó)

Para el HackRadar 2026, el objetivo era crear un sistema robusto, escalable y tolerante a fallos, utilizando las últimas tecnologías disponibles (.NET 10 y Angular 18).

### 1. Arquitectura de Procesamiento Asíncrono
Para evitar que el usuario se quede esperando frente a una pantalla de carga mientras la IA analiza un PDF complejo, implementamos un patrón asíncrono con **RabbitMQ**:
- **La API (El Recepcionista):** Recibe el archivo en el frontend, lo guarda, e inmediatamente envía un mensaje a una cola en RabbitMQ avisando que hay trabajo por hacer. Le devuelve un éxito inmediato al frontend.
- **El Worker (El Cerebro):** Un proceso en segundo plano (integrado en el mismo contenedor para optimizar costos de infraestructura PaaS) escucha esta cola, toma la factura, ejecuta el motor de evaluación de IA y actualiza el estado en PostgreSQL.

### 2. Desafíos Superados en el Despliegue
El despliegue en la nube (Cloudflare y Render) presentó desafíos técnicos de alto nivel que resolvimos sistemáticamente:

*   **Cadena de Conexión de PostgreSQL:** El ORM (Entity Framework Core) no acepta el formato URI nativo (`postgresql://...`) que algunas nubes proporcionan. Se requirió refactorizar la variable de entorno `DB_CONNECTION_STRING` para usar el estándar estricto ADO.NET (`Host=...;Database=...`).
*   **Aislamiento de Virtual Host en RabbitMQ:** Al utilizar CloudAMQP, nos enfrentamos al error `NOT_ALLOWED - access to '/'`. Los planes compartidos en la nube no permiten usar el virtual host raíz. Se solucionó inyectando la propiedad `VirtualHost` directamente en la instanciación del `ConnectionFactory` en C#, mapeándolo al nombre de usuario.
*   **Barreras CORS:** Aseguramos la comunicación entre la API en Render y el Frontend en Cloudflare inyectando de forma dinámica la variable `FRONTEND_URL` en las políticas CORS de ASP.NET Core, permitiendo peticiones seguras inter-dominio.

### 3. Decisiones de Diseño UI/UX
Inicialmente, el proyecto contemplaba un "Modo Oscuro". Sin embargo, durante el desarrollo, priorizamos la identidad de marca (Blinka), caracterizada por la limpieza, legibilidad corporativa y el contraste del verde sobre el blanco. 
- Eliminamos la complejidad del tema oscuro.
- Centralizamos todos los textos en diccionarios estáticos dentro del controlador de Angular, permitiendo una experiencia bilingüe sin latencia, controlada puramente a nivel de estado en el cliente.
- Se implementó la tipografía **Montserrat** a través de Google Fonts para garantizar un aspecto moderno y profesional.
