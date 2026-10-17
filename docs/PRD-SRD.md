# PRD-SRD — Agente Pagador (Reto Blinka: "Agentes que pagan por la empresa")

> **Estado:** BORRADOR v0.2 · incorpora el informe pre-PRD aprobado por el equipo
> **Origen:** Reto Blinka (hackatón), workflow de lluvia de ideas "Semáforo", revisiones cruzadas y respuestas de los organizadores
> **Ubicación en el repo:** `docs/PRD-SRD.md`
> **Regla de este documento:** define el *qué* y el *para quién*. No contiene decisiones técnicas (stack, modelo, framework).

> Los valores marcados con **(propuesto)** son metas o detalles sugeridos que el equipo aún debe confirmar. Lo que sigue abierto está en la sección 11.

---

## 1. Resumen ejecutivo

Las empresas con operaciones en varios países reciben decenas de facturas al mes y las pagan casi a mano: alguien las lee, las compara con lo acordado, pide aprobación por chat o correo, crea el pago y luego concilia todo en una hoja de cálculo.

Este proyecto construye un **agente** que lee cada factura y decide una de tres cosas, siguiendo las reglas que define la empresa:

| Semáforo | Desenlace | Significado |
|---|---|---|
| 🟢 Verde | **Pagar** | Cumple todas las reglas. El agente decide solo. |
| 🟡 Amarillo | **Escalar** | Pasa a una persona para revisión. El agente no resuelve el caso por su cuenta. |
| 🔴 Rojo | **Frenar** | No procede. Queda detenida y visible para la persona que deba decidir. |

Cada decisión queda registrada con su explicación. El agente **nunca** aprueba un pago fuera de las reglas sin aprobación humana.

El alcance termina en la decisión: **no se mueve dinero real** ni se conecta a una plataforma de pagos. "Pagar" significa "decisión registrada: aprobada para pago". El entregable es un agente funcionando sobre un conjunto de facturas simuladas, con una pantalla donde la empresa define sus reglas y ve qué pagó el agente, qué frenó y qué dejó pendiente de aprobación, y por qué. Se presenta como **demo en vivo** más un **repositorio en GitHub**.

**Pregunta detonadora (HMW):** ¿Cómo podríamos lograr que un agente evalúe facturas de múltiples canales de forma autónoma sin aprobar jamás un pago fuera de las reglas definidas por la empresa?

---

## 2. Objetivos y métricas de éxito

| # | Objetivo | Métrica | Meta | Cómo se mide |
|---|----------|---------|------|--------------|
| O1 | **Autonomía** | % de facturas del set de prueba cuya decisión del agente (Pagar, Frenar o Escalar) coincide con la decisión esperada, sin intervención humana. Una escalada o un frenado correctos cuentan como decisión correcta (confirmado por los organizadores). | ≥ 85 % **(propuesto)** | Comparar la decisión del agente con la decisión esperada de cada factura |
| O2 | **Seguridad** | Número de pagos aprobados fuera de las reglas | **0** (innegociable: aprobar un pago indebido descalifica) | Prueba automática de seguridad (HU-12) y revisión de los casos trampa |
| O3 | **Explicabilidad** | % de decisiones que una persona ajena al equipo entiende leyendo solo el registro | 100 % **(propuesto)** | Prueba de lectura con alguien ajeno al equipo |
| O4 | **Configurabilidad** | Tiempo para definir o cambiar un juego completo de reglas en pantalla | ≤ 1 minuto **(propuesto)** | Cronometrar la configuración |
| O5 | **Cobertura de casos trampa** | Casos trampa del reto y adicionales con datos y resultado esperado | 100 % de los casos de la sección 7.1 | Lista de verificación |

**Prioridad cuando hay conflicto:** seguridad > explicabilidad > autonomía. Ante la duda, el agente escala; nunca paga.

---

## 3. Usuarios objetivo (personas)

### Persona 1 — Analista de finanzas
- **Quién es:** persona del equipo de finanzas de una empresa constituida en EE. UU. con proveedores y contratistas en Latinoamérica y otras regiones.
- **Qué hace hoy:** revisa facturas, las compara con lo acordado, busca los datos de quien cobra, crea pagos a mano (muchas veces en otra moneda y hacia otro país) y concilia en una hoja de cálculo.
- **Dolor:** tarea repetitiva cada mes que depende de que alguien esté disponible.
- **Necesita:** que el agente resuelva lo rutinario y le pase solo lo que requiere criterio, con el motivo a la vista.

### Persona 2 — Aprobador (responsable financiero, legal, de compras o gerencia)
- **Quién es:** quien autoriza pagos grandes o fuera de lo normal.
- **Dolor:** recibe pedidos de aprobación por chat o correo, sin contexto.
- **Necesita:** una cola de pendientes con la factura, la regla que se activó y botones claros para aprobar o rechazar.

### Persona 3 — Administrador de reglas
- **Quién es:** quien define la política de pagos de la empresa (puede ser la misma persona que el analista o el aprobador).
- **Necesita:** definir límites, beneficiarios permitidos y umbrales sin tocar código.

---

## 4. Historias de usuario y criterios de aceptación

Formato: *Como [rol], quiero [acción] para [beneficio]*.

### HU-01 · Configurar reglas de pago
Como administrador de reglas, quiero definir las reglas de mi empresa en una pantalla para que el agente decida dentro de mis límites.
- [ ] Puedo definir el **límite por pago**.
- [ ] Puedo definir el **límite por periodo** (presupuesto).
- [ ] Puedo definir la **lista de beneficiarios permitidos**.
- [ ] Puedo definir el **umbral de monto desde el cual se exige aprobación humana**.
- [ ] Puedo definir las **monedas permitidas** y la **tasa de cambio fija** de la demo.
- [ ] Al guardar, las reglas se aplican a las decisiones siguientes y quedan visibles.
- [ ] Un valor inválido (límite negativo, campo vacío) se rechaza con un mensaje claro.
- [ ] Un monto **igual** al límite cumple la regla; solo uno **mayor** la activa.

### HU-02 · Leer una factura
Como analista de finanzas, quiero que el agente extraiga de cada factura a quién se paga, cuánto, en qué moneda y para cuándo, para no capturarlo a mano.
- [ ] Extrae **beneficiario, monto, moneda y fecha de vencimiento**, además del periodo de servicio y el concepto.
- [ ] Acepta facturas por los canales del reto: **PDF, imagen, correo o mensaje de chat**.
- [ ] Acepta facturas en **más de un idioma** (confirmado por los organizadores).
- [ ] Si un dato obligatorio falta, es ilegible o es ambiguo (por ejemplo "1.500" vs "1,500"), la factura **no se paga**: se escala con el motivo "no se pudo leer con certeza {campo}".
- [ ] Lo extraído se cruza contra las fuentes (el beneficiario debe existir, la moneda debe ser una permitida).
- [ ] El registro muestra los datos extraídos junto a la decisión.

### HU-03 · Decidir: pagar, escalar o frenar
Como analista de finanzas, quiero que el agente decida el destino de cada factura para que solo me lleguen los casos que requieren criterio.
- [ ] Cada factura termina en **uno** de tres desenlaces: **Pagar (Verde), Escalar (Amarillo) o Frenar (Rojo)**.
- [ ] **Pagar** solo ocurre si se cumplen *todas* las reglas.
- [ ] **Escalar** significa que el agente no hace nada más que pasar el caso a una persona.
- [ ] **Frenar** significa que no procede; la factura queda detenida y visible para la persona que deba decidir.
- [ ] Si fallan varias reglas, gana el desenlace más restrictivo (**Frenar > Escalar > Pagar**) y el registro lista **todos** los motivos.
- [ ] **El agente nunca resuelve de Amarillo a Verde por su cuenta.** Un Amarillo solo avanza por decisión de una persona.
- [ ] Antes de registrar un "Pagar", el sistema **verifica de nuevo** contra todas las reglas; si la verificación falla o no puede completarse, la factura se **escala** (ver supuesto S-08).

### HU-04 · Detectar cobros duplicados
Como analista de finanzas, quiero que el agente detecte cobros repetidos para no pagar dos veces lo mismo.
- [ ] Marca como duplicada una factura con el mismo proveedor, monto y mes de servicio, aunque llegue por otro canal o con otro nombre de archivo (no se usa solo el folio).
- [ ] La segunda se **frena** y el registro enlaza con la primera.
- [ ] Una factura similar de otro mes de servicio **no** se marca como duplicada.

### HU-05 · Controlar beneficiarios
Como administrador de reglas, quiero que solo se pague a beneficiarios autorizados para evitar pagos a desconocidos.
- [ ] Un beneficiario fuera de la lista permitida se **escala** (responsable sugerido: Compras).
- [ ] El registro indica que el beneficiario no está en la lista.

### HU-06 · Comparar contra lo acordado
Como analista de finanzas, quiero que el agente compare cada factura con lo acordado para detectar cobros que no cuadran.
- [ ] Compara el monto facturado con el monto pactado del proveedor.
- [ ] Si el monto supera lo pactado, o no hay acuerdo vigente registrado, la factura se **escala** (responsable sugerido: Legal) y muestra "facturado X, acordado Y" o "sin acuerdo registrado".
- [ ] El agente no asume un acuerdo que no existe.
- [ ] **Caso documental acotado (v1):** si la factura trae un anexo o cláusula de ajuste, el agente cita la cláusula encontrada y el resultado de la verificación por código (monto ≤ cláusula, vigente) como **evidencia para la persona**. La factura sigue escalada; el agente no la aprueba.

### HU-07 · Exigir aprobación humana por monto
Como aprobador, quiero que los pagos grandes me lleguen para autorizarlos.
- [ ] Una factura cuyo monto es **mayor** que el umbral pasa a **Escalar** (responsable sugerido: Gerencia).
- [ ] El pedido muestra la factura, el monto, el umbral y la regla activada.

### HU-08 · Respetar el presupuesto del periodo
Como administrador de reglas, quiero que el agente no exceda el presupuesto del periodo para no sobregirar a la empresa.
- [ ] El agente lleva la **cuenta corriente** del periodo: cada "Pagar" descuenta saldo.
- [ ] Las facturas se procesan en orden fijo: **fecha de vencimiento** y luego **orden de llegada**.
- [ ] Una factura pendiente de aprobación **no reserva** saldo.
- [ ] Una factura que haría superar el saldo del periodo se **frena** (responsable sugerido: Finanzas).
- [ ] El registro muestra **saldo antes y saldo después**.
- [ ] El presupuesto se asigna al mes de la **fecha de pago**.

### HU-09 · Resolver pendientes y reabrir frenadas
Como aprobador, quiero aprobar o rechazar los casos escalados y revisar los frenados para destrabar los pagos legítimos.
- [ ] Veo la lista de **pendientes** con su motivo.
- [ ] Al aprobar, veo el **saldo actual** del periodo.
- [ ] Puedo **aprobar** o **rechazar**; mi decisión queda registrada con mi etiqueta de origen.
- [ ] Puedo **reabrir** una factura frenada y escalarla a una persona.
- [ ] Un pago pasa a "Pagar" por aprobación humana explícita, nunca por reintento del agente.

### HU-10 · Consultar y exportar el registro
Como persona de finanzas o auditora, quiero leer por qué el agente pagó, frenó o escaló cada factura para confiar en él.
- [ ] Cada decisión registra: factura, datos extraídos, reglas evaluadas y su resultado, desenlace, color, origen de la decisión y explicación.
- [ ] La explicación se arma **con plantillas** a partir de los resultados reales de las reglas; la IA solo aporta la cita de cláusula.
- [ ] El registro se **ve en pantalla** y se puede **exportar** a un archivo (formato por confirmar, P-08).
- [ ] El registro no se puede editar desde la pantalla de reglas.
- [ ] No contiene lenguaje de fraude: usa "no coincide con el registro del proveedor".

### HU-11 · Contar con datos simulados completos
Como equipo del proyecto, queremos datasets y facturas ficticias para demostrar el comportamiento del agente.
- [ ] Los datasets de la sección 8 existen y están documentados.
- [ ] Las facturas cubren normales y casos trampa, en varios canales e idiomas.
- [ ] Cada factura tiene una **decisión esperada** documentada.
- [ ] Hay dos conjuntos: **desarrollo** y **prueba**, y casos de frontera (en el límite, justo por encima, moneda con decimales).
- [ ] No hay ningún dato real de clientes ni de empresas.

### HU-12 · Verificar la seguridad automáticamente
Como equipo del proyecto, queremos comprobar que nunca se aprueba un pago indebido.
- [ ] Una prueba automática corre todo el set y **falla** si una factura cuya decisión esperada no es "Pagar" termina como "Pagar".
- [ ] La prueba incluye los casos de frontera y los casos trampa.
- [ ] Dentro del agente rige la verificación final de HU-03: si falla, escala.

### HU-13 · Medir la autonomía
Como equipo del proyecto, queremos medir cuánto acierta el agente sin intervención humana.
- [ ] El porcentaje se calcula comparando la decisión del agente con la decisión esperada de cada factura.
- [ ] La medición **no** va dentro del registro por factura.
- [ ] Se mide sobre el set de **prueba**, no sobre el de desarrollo.

### HU-14 · (Opcional) Verificar la cuenta registrada del proveedor
Como administrador de reglas, quiero activar la comparación entre la cuenta de pago de la factura y la cuenta registrada del proveedor.
- [ ] Es una regla **opcional** que se activa o desactiva en la pantalla de reglas.
- [ ] Si no coincide, la factura se **escala** (responsable sugerido: Compras) con el texto "la cuenta no coincide con el registro del proveedor".
- [ ] Se trata como validación de coincidencia, nunca como detección de fraude.

---

## 5. Alcance

### Incluido
- [x] Leer facturas (PDF, imagen, correo, chat; más de un idioma) y extraer a quién, cuánto, en qué moneda y para cuándo.
- [x] Decidir Pagar / Escalar / Frenar con reglas configurables.
- [x] Semáforo como capa visual de esos tres desenlaces.
- [x] Presupuesto corriente y orden de procesamiento fijo.
- [x] Registro explicado de cada decisión, visible en pantalla y exportable.
- [x] Pantalla de reglas y pantalla de resultados con aprobación humana.
- [x] Datasets simulados y set de facturas con casos normales, trampa y de frontera.
- [x] Verificación de seguridad automática.
- [x] Demo en vivo y repositorio en GitHub.

### Excluido
- [ ] Ejecutar pagos reales o mover dinero, ni siquiera en etapas posteriores del prototipo.
- [ ] Conectarse a una plataforma de pagos o a un banco.
- [ ] Abrir cuentas.
- [ ] Verificar la identidad de las empresas.
- [ ] Decisiones de cumplimiento o fraude (y cualquier lenguaje que las sugiera).
- [ ] El diseño de la app de Blinka.
- [ ] Etapas de adopción (Piloto, Semiautomático, Escala) como funcionalidad: quedan como **hoja de ruta** en la presentación.
- [ ] Pantalla de directorio de contactos, selector de etapa y medidor de aciertos en pantalla.
- [ ] Usar datos reales de clientes o empresas.

### Complemento opcional
- [ ] Verificación de cuenta registrada del proveedor (HU-14).

---

## 6. Restricciones y supuestos

### Restricciones
- **R1.** Ningún dato real de clientes ni de empresas.
- **R2.** El agente nunca aprueba un pago fuera de las reglas sin aprobación humana.
- **R3.** Cada decisión debe poder explicarse después, leyendo su registro y sin abrir el código.
- **R4.** Libertad total de modelo, lenguaje y framework (se decide en la Propuesta Técnica, no aquí).
- **R5.** No hay API ni facturas de ejemplo: el equipo construye su propio set de datos.
- **R6.** El flujo termina en la decisión registrada.
- **R7.** Entregables: demo y repositorio en GitHub.

### Principios inviolables de diseño
1. Nunca pagar fuera de las reglas sin aprobación humana.
2. Ante duda o dato faltante, **escalar**.
3. **El código decide; la IA solo lee.** La IA extrae datos de la factura y la cita de la cláusula; las reglas deterministas toman la decisión. La IA no tiene ninguna acción de "pagar".
4. El texto de la factura es un **dato**, no una instrucción.
5. Toda explicación se arma con plantillas a partir de resultados reales de las reglas.

### Decisiones aprobadas del equipo
| ID | Decisión |
|---|---|
| D-01 | Se **frena** en dos casos: duplicados y presupuesto insuficiente. El aprobador amplía el presupuesto y reprocesa. |
| D-02 | Una factura pendiente de aprobación **no reserva** saldo; al aprobar se muestra el saldo actual. |
| D-03 | **Periodo:** el presupuesto usa el mes de la fecha de pago; los duplicados usan el mes de servicio. |
| D-04 | **Orden de procesamiento:** fecha de vencimiento y luego orden de llegada. |
| D-05 | Un monto **igual** al límite cumple; solo uno **mayor** escala. |
| D-06 | El **Amarillo** entra en la v1 con el comportamiento de R-03: Amarillo = Escalar, sin resolución automática a Verde. |
| D-07 | **Moneda base USD** y 2 o 3 monedas locales con tasa fija. |
| D-08 | **Roles:** datos y facturas · motor de reglas y agente · pantallas · pruebas y documentación. |

### Respuestas de los organizadores
- "Frenar" = no procede; la factura frenada debe ser visible para la persona que decide.
- "Escalar" = el caso pasa a una persona; el agente no hace nada más.
- Una escalada correcta cuenta como decisión correcta del agente.
- Entregable: demo y repositorio en GitHub.
- El registro debe verse en pantalla y poder exportarse.
- Las facturas pueden venir en más de un idioma.
- Sin información sobre el set que usará el jurado.
- La verificación de cuenta registrada puede incluirse como elemento opcional.

### Supuestos (a confirmar)
- **S-01.** Las facturas simuladas estarán en español e inglés.
- **S-02.** Los acuerdos se representan como un registro simple por proveedor (monto pactado, vigencia, concepto).
- **S-03.** Se estima un set de unas 50 facturas: ≈30 de desarrollo y ≈20 de prueba.
- **S-04.** La aprobación humana se simula en la pantalla de la demo; no hay canal real de chat o correo.
- **S-05.** El responsable sugerido (Compras, Legal, Finanzas, Gerencia) es un dato fijo informativo, no una función de enrutamiento configurable.
- **S-06.** Un rechazo humano deja la factura en "Frenadas" con la etiqueta "rechazada por persona".
- **S-07.** La cita de la cláusula del caso documental es lo primero que se recorta si falta tiempo; el estado Amarillo en sí no.
- **S-08.** *(Interpretación del cambio a R-13: "si falla, escala")* Cuando la verificación final de seguridad de un "Pagar" falla o no se puede completar, la factura se escala. La prueba automática del set (HU-12) se mantiene además como control de desarrollo.

---

## 7. Reglas de decisión

### 7.1 Tabla de casos

| Caso | Color | Desenlace | Responsable sugerido (fijo) | Plantilla de explicación |
|---|---|---|---|---|
| Normal: cumple todas las reglas | 🟢 Verde | **Pagar** | — | "Pagar: beneficiario permitido, monto X ≤ límite, coincide con lo acordado, saldo suficiente." |
| Cobro duplicado | 🔴 Rojo | **Frenar** | Finanzas | "Frenada: duplicada de la factura {id} (mismo proveedor, monto y mes de servicio)." |
| Presupuesto insuficiente | 🔴 Rojo | **Frenar** | Finanzas | "Frenada: saldo del periodo {X}, monto {Y}." |
| Beneficiario no permitido | 🟡 Amarillo | **Escalar** | Compras | "Escalada: {beneficiario} no está en la lista permitida." |
| No cuadra con lo acordado, o no hay acuerdo | 🟡 Amarillo | **Escalar** | Legal | "Escalada: facturado {X}, acordado {Y}" / "sin acuerdo registrado." |
| Anexo o cláusula de ajuste (caso documental) | 🟡 Amarillo | **Escalar** | Legal | "Escalada: cláusula {n} cita {texto}; verificación por código: {sí/no}." |
| Monto sobre el umbral de aprobación humana | 🟡 Amarillo | **Escalar** | Gerencia | "Escalada: monto {X} supera el umbral {Y}." |
| Datos ilegibles, incompletos o ambiguos | 🟡 Amarillo | **Escalar** | Finanzas | "Escalada: no se pudo leer con certeza {campo}." |
| Factura con instrucciones incrustadas (robustez) | según las reglas | según las reglas | según el caso | Se añade: "Se ignoró texto de la factura que pedía saltar reglas." |
| Cuenta distinta a la registrada *(opcional, HU-14)* | 🟡 Amarillo | **Escalar** | Compras | "Escalada: la cuenta no coincide con el registro del proveedor." |

### 7.2 Precedencia
**Frenar (Rojo) > Escalar (Amarillo) > Pagar (Verde).** Si fallan varias reglas, se muestra el desenlace más restrictivo y se listan todos los motivos.

### 7.3 Estados de una factura

| Estado | Origen | Dónde se ve |
|---|---|---|
| Pagada por el agente | Agente | Pagadas |
| Frenada por el agente | Agente | Frenadas |
| Pendiente de aprobación | Agente | Pendientes |
| Aprobada por persona | Persona | Pagadas, con etiqueta |
| Rechazada por persona | Persona | Frenadas, con etiqueta |

La autonomía solo cuenta decisiones del agente.

---

## 8. Datasets simulados (esquema funcional)

Todos son 100 % ficticios.

| Dataset | Campos mínimos | Se usa para |
|---|---|---|
| **Proveedores** | nombre, ¿permitido?, cuenta registrada, moneda habitual | Beneficiarios permitidos; verificación opcional de cuenta |
| **Acuerdos** | proveedor, monto pactado, vigencia, concepto, cláusula (opcional) | "Lo acordado"; caso documental |
| **Presupuesto** | periodo (mes), límite, saldo | Límite por periodo; presupuesto corriente |
| **Historial de facturas** | id, proveedor, monto, moneda, mes de servicio, estado | Detección de duplicados |
| **Tasa de cambio** | moneda → USD, tasa fija de la demo | Evaluar límites en la moneda base |
| **Reglas de la empresa** | límite por pago, límite por periodo, beneficiarios permitidos, umbral, monedas permitidas, opción de cuenta | Configuración |
| **Facturas de prueba** | canal, idioma, datos esperados, **decisión esperada**, conjunto (desarrollo o prueba) | Medición de autonomía y seguridad |

---

## 9. Pantallas

### 9.1 Pantalla de reglas
Límite por pago · límite por periodo · beneficiarios permitidos · umbral de aprobación humana · monedas permitidas y tasa · (opcional) verificación de cuenta registrada.

### 9.2 Pantalla de resultados
- **Tres vistas:** Pagadas · Frenadas · Pendientes de aprobación.
- Cada factura tiene una **fila expandible** con datos extraídos, reglas evaluadas, motivos, color, origen de la decisión, responsable sugerido y explicación.
- En **Pendientes**: botones Aprobar y Rechazar, con el saldo actual visible.
- En **Frenadas**: acción para reabrir y escalar a una persona.
- Registro **visible en pantalla** y **exportable**.

---

## 10. Riesgos

| # | Riesgo | Probabilidad | Impacto | Mitigación |
|---|--------|:---:|:---:|-----------|
| RK1 | El agente aprueba un pago indebido (descalifica) | Media | **Crítico** | Principios inviolables; verificación final antes de "Pagar"; prueba automática (HU-12) |
| RK2 | Lectura errónea de montos o monedas en PDF o imagen | Alta | Crítico | Validar formato y campos; cruzar con fuentes; ante ambigüedad escalar; casos trampa de lectura |
| RK3 | Instrucciones incrustadas en una factura influyen en el agente | Media | Alto | La IA no tiene acción de pagar; el código decide; caso trampa de robustez |
| RK4 | Explicaciones vagas que no se entienden sin el código | Media | Alto | Plantillas; prueba de lectura con persona ajena |
| RK5 | Falta de tiempo para agente, pantallas y datos a la vez | Alta | Alto | Bloques de 45 min; núcleo primero; cláusula citada y verificación de cuenta como recortables |
| RK6 | Autonomía inflada por ajustar reglas con las mismas facturas con que se mide | Media | Medio | Sets de desarrollo y de prueba separados; intercambio con otro equipo |
| RK7 | Resultados distintos según el orden de procesamiento | Baja | Medio | Orden fijo (D-04) |
| RK8 | Set del jurado distinto al nuestro | Media | Medio | Probar con sets de otros equipos; facturas en varios canales e idiomas |
| RK9 | Reglas mal configuradas producen decisiones absurdas | Baja | Medio | Validar al guardar; mostrar siempre la regla aplicada |
| RK10 | Fuga de datos reales al armar el set | Baja | Alto | Solo datos ficticios; revisión antes de subir al repo |

---

## 11. Preguntas pendientes

- [ ] **P-01 · Plazo.** ¿Cuándo es la entrega y cuánto dura la demo?
- [ ] **P-02 · Equipo.** ¿Quién toma cada uno de los cuatro roles de D-08?
- [ ] **P-03 · Monedas.** Además de USD, ¿qué 2 o 3 monedas locales y qué tasas fijas?
- [ ] **P-04 · Idiomas.** ¿Español e inglés bastan para "más de un idioma"?
- [ ] **P-05 · Alcance de los límites.** ¿Son globales o por proveedor?
- [ ] **P-06 · Tolerancia.** ¿Se acepta una diferencia pequeña contra lo acordado, o cualquier diferencia escala?
- [ ] **P-07 · Fechas.** ¿Qué hace el agente con facturas ya vencidas o con fecha de pago muy lejana?
- [ ] **P-08 · Exportación.** ¿Qué formato de archivo: CSV, JSON o ambos?
- [ ] **P-09 · Tamaño del set.** ¿Confirmamos ≈50 facturas (30 desarrollo y 20 prueba)?
- [ ] **P-10 · Intercambio de sets.** ¿Con qué equipo intercambiamos facturas para la prueba ciega?
- [ ] **P-11 · Interpretación de R-13 / S-08.** ¿Confirmamos que "si falla, escala" se refiere a la verificación final antes de cada "Pagar"?
- [ ] **P-12 · Caso documental.** ¿Qué anexo o cláusula concreta usamos como único caso de v1?
- [ ] **P-13 · Jurado.** Sin información sobre su set: ¿tenemos alguna vía para preguntar de nuevo?

---

## 12. Historial del documento

| Versión | Fecha | Cambio |
|---|---|---|
| 0.1 | 2026-10-16 | Borrador inicial a partir del enunciado del reto y el workflow de lluvia de ideas |
| 0.2 | 2026-10-17 | Incorpora el informe pre-PRD aprobado (R-01 a R-14, D-01 a D-08). Cambios del equipo: R-03 (Verde = pagar, Amarillo = escalar, Rojo = frenar; el agente no resuelve Amarillo a Verde), R-13 (si falla la verificación, escala), D-06 (Amarillo entra en v1 con ese comportamiento). Incorpora las respuestas de los organizadores. Agrega las secciones de reglas de decisión, datasets y pantallas. |
