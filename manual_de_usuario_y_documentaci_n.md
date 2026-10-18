# 📖 Manual de Usuario y Documentación Técnica
**Agente Pagador**
url produccion: https://agentepagador.pages.dev/

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

### 4. Asistente Virtual IA Dani
En la esquina inferior derecha de la pantalla, encontrarás un botón flotante con el ícono de un gato negro arcade.
- Haz clic en el botón para desplegar el widget de chat de **IA Dani**.
- Puedes interactuar directamente con el asistente, quien te explicará la directiva de auditoría de facturas y te asistirá para comprender el significado de los semáforos (Rojo, Amarillo, Verde).
- Escribe tu consulta y presiona "Enviar" o la tecla Enter.

---

## PARTE 2: Documentación Técnica (Cómo se construyó)

Para el HackRadar 2026, el objetivo era crear un sistema robusto, escalable y tolerante a fallos, utilizando las últimas tecnologías disponibles (.NET 10 y Angular 18).

### 1. Arquitectura de Procesamiento Asíncrono
Para evitar que el usuario se quede esperando frente a una pantalla de carga mientras la IA analiza un PDF complejo, implementamos un patrón asíncrono con **RabbitMQ**:
- **La API (El Recepcionista):** Recibe el archivo en el frontend, lo guarda, e inmediatamente envía un mensaje a una cola en RabbitMQ avisando que hay trabajo por hacer. Le devuelve un éxito inmediato al frontend.
- **El Worker (El Cerebro):** Un proceso en segundo plano (integrado en el mismo contenedor para optimizar costos de infraestructura PaaS) escucha esta cola, toma la factura, extrae los datos mediante la API de Groq, ejecuta el motor de evaluación y actualiza el estado en PostgreSQL.

### 2. Desafíos Superados en el Despliegue y Lógica
El despliegue en la nube y la integración de la IA presentaron desafíos técnicos críticos que resolvimos sistemáticamente:

*   **Cadena de Conexión de PostgreSQL:** El ORM (Entity Framework Core) no acepta el formato URI nativo (`postgresql://...`) que algunas nubes proporcionan. Se requirió refactorizar la variable de entorno `DB_CONNECTION_STRING` para usar el estándar estricto ADO.NET (`Host=...;Database=...`).
*   **Aislamiento de Virtual Host en RabbitMQ:** Al utilizar CloudAMQP, nos enfrentamos al error `NOT_ALLOWED - access to '/'`. Se solucionó inyectando la propiedad `VirtualHost` directamente en la instanciación del `ConnectionFactory` en C#, mapeándolo al nombre de usuario.
*   **Formatos Numéricos de la IA (Groq):** Los modelos de lenguaje extraían montos con comas separadoras de miles (ej. 7,500.00), rompiendo la conversión a `decimal` en C#. Se blindó el prompt del Worker instruyendo explícitamente a la IA para devolver un formato numérico puro (7500.00) en un JSON estricto.
*   **Erradicación de Máscaras Visuales en Frontend:** Angular interceptaba los estados para renderizar textos hardcodeados, ocultando la justificación real del motor C#. Se destruyó la reevaluación local en TypeScript y se implementó interpolación directa para garantizar transparencia total en la auditoría desde la base de datos hasta el HTML.
*   **Mocking Determinista de Proveedores:** El uso de UUIDs aleatorios bloqueaba la validación de la capa de datos. Se estableció un flujo determinista donde los proveedores legales reciben un ID específico autorizado, y los fraudulentos reciben un ID vacío, detonando consistentemente el semáforo Rojo por ilegalidad.

### 3. Decisiones de Diseño UI/UX
Inicialmente, el proyecto contemplaba un "Modo Oscuro". Sin embargo, durante el desarrollo, priorizamos la identidad de marca (Blinka), caracterizada por la limpieza, legibilidad corporativa y el contraste del verde sobre el blanco. 
- Eliminamos la complejidad del tema oscuro.
- Centralizamos todos los textos en diccionarios estáticos dentro del controlador de Angular, permitiendo una experiencia bilingüe sin latencia.
- Se implementó la tipografía **Montserrat** a través de Google Fonts para garantizar un aspecto moderno.
- Se integró a "Dani", un chatbot contextual representado por un avatar de gato arcade, inyectando interactividad directa en el frontend sin comprometer el flujo principal de auditoría.
