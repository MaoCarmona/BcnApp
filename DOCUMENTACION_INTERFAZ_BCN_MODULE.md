# MANUAL DE USUARIO BCN MODULE
## Sistema de Integración y Consolidación de Datos Operativos

**Versión:** 1.0  
**Fecha:** Enero 2024  
**Plataforma:** ASP.NET MVC  
**Empresa:** Ecopetrol  

---

## 📋 ÍNDICE

1. [INTRODUCCIÓN](#introducción)
2. [ACCESO AL SISTEMA](#acceso-al-sistema)
3. [INTERFAZ PRINCIPAL](#interfaz-principal)
4. [MÓDULO 1: INTEGRAR INFORMACIÓN](#módulo-1-integrar-información)
5. [MÓDULO 2: CONSOLIDAR INFORMACIÓN](#módulo-2-consolidar-información)
6. [MÓDULO 3: TRANSFORMACIÓN LOGÍSTICA](#módulo-3-transformación-logística)
7. [MÓDULO 4: ENVÍO BCN WS-ARES](#módulo-4-envío-bcn-ws-ares)
8. [MÓDULO 5: CONFIGURACIÓN](#módulo-5-configuración)
9. [FUNCIONALIDADES COMUNES](#funcionalidades-comunes)
10. [FLUJO DE TRABAJO](#flujo-de-trabajo)
11. [SOLUCIÓN DE PROBLEMAS](#solución-de-problemas)

---

## 📖 INTRODUCCIÓN

El **BCN Module** es una aplicación web desarrollada en ASP.NET MVC que permite la gestión integral de datos operativos de Ecopetrol. Este manual describe el funcionamiento y uso de la interfaz de usuario.

### 🎯 Funcionalidades Principales
- Integración de datos desde sistemas AORA, ROMSS y BCN
- Consolidación y balance de información operativa
- Transformación logística y gestión de costos
- Envío de datos al sistema ARES
- Configuración de parámetros del sistema

---

## 🚪 ACCESO AL SISTEMA

### Requisitos Previos
- Navegador web compatible (Chrome, Firefox, Edge)
- Credenciales de acceso autorizadas
- Conexión a la red corporativa

### Pasos de Acceso
1. Abrir el navegador web
2. Ingresar la URL del sistema BCN Module
3. Introducir credenciales de usuario
4. Hacer clic en "Iniciar Sesión"

---

## 🖥️ INTERFAZ PRINCIPAL

### Elementos de la Pantalla
- **Barra de Navegación**: Menú principal con los 5 módulos
- **Panel de Fechas**: Selector de rangos de fechas
- **Área de Contenido**: Visualización de datos y controles
- **Barra de Estado**: Información del sistema y mensajes

### Navegación entre Módulos
- Hacer clic en el nombre del módulo deseado
- El módulo activo se resalta en la barra de navegación
- Cada módulo mantiene su estado independiente

---

## 🔄 MÓDULO 1: INTEGRAR INFORMACIÓN

### Descripción
Este módulo permite capturar y procesar datos operativos desde sistemas externos.

### Opciones Disponibles

| **Opción** | **Nombre** | **Acción** | **Fechas Especiales** |
|------------|------------|------------|------------------------|
| **01** | AORA: Inventario Operativo | 🔵 Ejecutar, 👁️ Ver | ✅ Modal de selección |
| **02** | AORA: Movimientos Operativos | 🔵 Ejecutar, 👁️ Ver | ❌ Rango estándar |
| **03** | AORA: Flujos Operativos | 🔵 Ejecutar, 👁️ Ver | ❌ Rango estándar |
| **04** | ROMSS: Inventario Operativo | 🔵 Ejecutar, 👁️ Ver | ✅ Modal de selección |
| **05** | ROMSS: Movimientos Operativos | 🔵 Ejecutar, 👁️ Ver | ❌ Rango estándar |
| **06** | BCN: Foto Inventario Operativo | 🔵 Ejecutar, 👁️ Ver | ✅ Modal de selección |
| **07** | BCN: Movimientos | 🔵 Ejecutar, 👁️ Ver | ❌ Rango estándar |
| **08** | BCN: Balance Operativo | 👁️ Solo Ver | ❌ Rango estándar |
| **09** | WebService: Movimientos Logísticos | 🔵 Ejecutar, 👁️ Ver | ❌ Rango estándar |
| **10** | ARES: Movimientos HPI | 🔵 Ejecutar, 👁️ Ver | ❌ Rango estándar |

### Cómo Usar

#### Paso 1: Seleccionar Opción
- Hacer clic en el número de la opción deseada
- El sistema resalta la opción seleccionada

#### Paso 2: Configurar Fechas
- **Fechas Estándar**: Usar panel lateral de fechas
- **Fechas Especiales**: Aparece modal para elegir fecha inicial o final

#### Paso 3: Ejecutar Operación
- Hacer clic en 🔵 **Ejecutar** para procesar datos
- Hacer clic en 👁️ **Ver** para consultar datos existentes

### Estructura de Datos Visualizada

**Columnas Principales:**
- Item, Producto, Almacén
- Foto Inv., VoBo, API
- Volumen Total, Bombeable, Remanente
- Masa Total, Bombeable, Remanente
- Unidades de Medida y Estado

---

## 🔗 MÓDULO 2: CONSOLIDAR INFORMACIÓN

### Descripción
Este módulo consolida y agrega los datos previamente integrados.

### Opciones Disponibles

| **Opción** | **Nombre** | **Tipo** | **Retorno** |
|------------|------------|----------|-------------|
| **01** | BCN: Inventarios | Consolidación + Consulta | ✅ Datos consolidados |
| **02** | BCN: Movimientos | Consolidación + Consulta | ✅ Datos consolidados |
| **03** | BCN: Balance ALMACEN | Consolidación + Consulta | ✅ Balance por almacén |
| **04** | BCN: Balance POOL | Consolidación + Consulta | ✅ Balance por pool |
| **05** | BCN: Balance UNIDAD DE PROCESO | Consolidación + Consulta | ✅ Balance por UP |
| **06** | BCN: Foto Inventario | Consolidación + Consulta | ✅ Fotos consolidadas |
| **07** | BCN: Aplicar Regla de Balance | Solo Procesamiento | ❌ Solo confirmación |
| **08** | BCN: Diferencia Balance | Solo Procesamiento | ❌ Solo confirmación |

### Cómo Usar

#### Opciones de Consolidación (01-06)
1. Seleccionar opción deseada
2. Configurar fechas de consolidación
3. Hacer clic en **Ejecutar**
4. Revisar resultados consolidados

#### Opciones de Solo Procesamiento (07-08)
1. Seleccionar opción
2. Configurar fechas
3. Hacer clic en **Ejecutar**
4. Confirmar procesamiento

---

## 🚚 MÓDULO 3: TRANSFORMACIÓN LOGÍSTICA

### Descripción
Este módulo procesa información logística y genera balances por centro logístico (CeLo).

### Opciones Disponibles

| **Opción** | **Nombre** | **CeLo Objetivo** | **Retorno** |
|------------|------------|-------------------|-------------|
| **01** | Movimientos Logísticos | Todos los CeLos | ✅ Datos procesados |
| **02** | Movimientos de Costos | Todos los CeLos | ✅ Datos procesados |
| **03** | Balance GRB CeLo: 2000 | GRB (2000) | ✅ Balance específico |
| **04** | Balance Reexpido CeLo: 3501 | Reexpido (3501) | ✅ Balance específico |
| **05** | Balance Impala CeLo: 4130 | Impala (4130) | ✅ Balance específico |

### Cómo Usar

1. Seleccionar tipo de procesamiento logístico
2. Configurar rango de fechas
3. Hacer clic en **Ejecutar**
4. Revisar movimientos o balances generados

### Datos Visualizados

**Movimientos Logísticos:**
- ID Message, Clase Movimiento, Descripción
- Fechas de inicio y fin
- Recursos origen y destino
- Productos, valores contables
- Centros de costo y estado

---

## 📤 MÓDULO 4: ENVÍO BCN WS-ARES

### Descripción
Este módulo prepara y envía datos al sistema ARES, además de permitir revisión de estado.

### Opciones Disponibles

| **Opción** | **Nombre** | **Función** | **WebService** |
|------------|------------|-------------|----------------|
| **01** | Inventario Logístico | Preparación + Envío | ARES |
| **02** | Movimiento Logístico | Preparación + Envío | ARES |
| **03** | Movimiento de Costos | Preparación + Envío | ARES |
| **04** | Rev. Procesamiento Logístico | Revisión de Estado | SAP ECC ECP |
| **05** | Rev. Procesamiento Costo | Revisión de Estado | SAP ECC ECP |
| **06** | Rev. Comparativo Inventario | Comparación entre Sistemas | SAP ECC ECP |
| **07** | Rev. Comparativo Costos | Comparación entre Sistemas | SAP ECC ECP |

### Cómo Usar

#### Opciones de Envío (01-03)
1. Seleccionar tipo de datos a enviar
2. Configurar fechas de consulta
3. Revisar datos preparados
4. Seleccionar registros con checkboxes
5. Hacer clic en **"Enviar a ARES (X)"**

#### Opciones de Revisión (04-07)
1. Seleccionar tipo de revisión
2. Configurar fecha de consulta
3. Hacer clic en **Ejecutar**
4. Revisar estado o comparativo

### Sistema de Selección ARES

**Funcionalidades:**
- ✅ **Checkboxes individuales** por registro
- ✅ **Seleccionar Todo** para marcar todos los filtrados
- ✅ **Deseleccionar Todo** para limpiar selección
- ✅ **Contador de selección** en tiempo real
- ✅ **Botón de envío** con cantidad seleccionada

---

## ⚙️ MÓDULO 5: CONFIGURACIÓN

### Descripción
Este módulo permite gestionar parámetros del sistema y configuraciones de conexión.

### Opciones Disponibles

| **Opción** | **Nombre** | **Estado** | **Prioridad** |
|------------|------------|------------|---------------|
| **01** | Gestionar Credenciales | ✅ FUNCIONAL | 🔴 ALTA |
| **02** | Configuración Homologación | 🚧 EN CONSTRUCCIÓN | 🟡 MEDIA |
| **03** | Configuración de Forzamiento | 🚧 EN CONSTRUCCIÓN | 🟡 MEDIA |
| **04** | Configuración de Reglas | 🚧 EN CONSTRUCCIÓN | 🟡 MEDIA |
| **05** | Configuración Logística | 🚧 EN CONSTRUCCIÓN | 🟢 BAJA |
| **06** | Configuración de Costos | 🚧 EN CONSTRUCCIÓN | 🟢 BAJA |

### Gestionar Credenciales (Opción 01)

#### Campos Configurables
- **DB ARES**: Cadena de conexión a base de datos ARES
- **DB ROMSS**: Cadena de conexión a base de datos ROMSS
- **DB BCN**: Cadena de conexión a base de datos BCN
- **URL ARES**: Endpoint del servicio ARES

#### Cómo Configurar
1. Hacer clic en **Gestionar Credenciales**
2. Completar todos los campos requeridos
3. Hacer clic en **Guardar Cambios**
4. Confirmar mensaje de éxito

#### Características de Seguridad
- 🔐 Cadenas de conexión codificadas en Base64
- 🔒 Validación automática de formato
- 💾 Persistencia segura en configuración
- 🔄 Actualización sin recarga de página

---

## 🔧 FUNCIONALIDADES COMUNES

### 📅 Gestión de Fechas

**Panel de Fechas:**
- **Fechas de Inicio y Fin**: Controles de fecha en panel lateral
- **Navegación Rápida**: Botones "Día anterior" y "Día siguiente"
- **Validación Automática**: No permite fechas futuras
- **Modal de Selección**: Para opciones que requieren fecha específica

### 🎮 Botones de Acción

**Botones Disponibles:**
- 🔵 **Ejecutar**: Inicia procesamiento de la operación
- 👁️ **Ver**: Muestra datos ya procesados
- 🔴 **Cancelar**: Interrumpe operación en curso
- 🖨️ **Imprimir**: Genera vista de impresión
- 📊 **Exportar Excel**: Descarga datos en formato XLSX

**Lógica de Habilitación:**
- **Opción 08 del módulo Integrar**: Solo permite visualización
- **Otras opciones**: Permiten ejecución completa
- **Durante procesamiento**: Se ocultan botones de ejecución

### 🔍 Sistema de Filtros

**Capacidades:**
- **Búsqueda Global**: Campo de texto para buscar en toda la tabla
- **Filtros por Columna**: Filtros individuales para cada columna
- **Paginación**: Control de registros por página (10, 50, 100, Todos)
- **Limpieza**: Botón para limpiar búsquedas y filtros

---

## 📋 FLUJO DE TRABAJO

### 🔄 Proceso Operativo Estándar

#### Fase 1: Configuración
1. Acceder al sistema
2. Ir a **Configuración → Gestionar Credenciales**
3. Configurar todas las conexiones de base de datos
4. Guardar cambios

#### Fase 2: Integración
1. Ir a **Módulo Integrar Información**
2. Seleccionar opción específica (ej: AORA Inventario)
3. Configurar fechas de consulta
4. Hacer clic en **Ejecutar**
5. Revisar resultados en tabla

#### Fase 3: Consolidación
1. Cambiar a **Módulo Consolidar Información**
2. Seleccionar opción de consolidación
3. Configurar fechas de consolidación
4. Hacer clic en **Ejecutar**
5. Validar balances consolidados

#### Fase 4: Envío ARES
1. Ir a **Módulo Envío BCN WS-ARES**
2. Seleccionar tipo de datos a enviar
3. Revisar datos preparados
4. Seleccionar registros con checkboxes
5. Hacer clic en **Enviar a ARES**

### 📊 Dependencias entre Módulos

| **Secuencia** | **Dependencia** | **Descripción** |
|---------------|------------------|-----------------|
| **Integrar → Consolidar** | 🔴 CRÍTICA | Los datos consolidados dependen de la integración previa |
| **Consolidar → ARES** | 🟡 IMPORTANTE | Los balances consolidados se envían a ARES |
| **Configuración → Todos** | 🔴 CRÍTICA | Sin configuración no funcionan las conexiones |

---

## 🚨 SOLUCIÓN DE PROBLEMAS

### ⚠️ Problemas Comunes

#### Error de Conexión a Base de Datos
**Síntoma:** Mensaje "Error de conexión" al ejecutar operaciones
**Solución:**
1. Verificar configuración en **Módulo Configuración**
2. Comprobar conectividad de red
3. Validar credenciales de base de datos

#### Fechas No Válidas
**Síntoma:** Sistema no permite seleccionar fechas
**Solución:**
1. Verificar que las fechas no sean futuras
2. Asegurar que fecha fin sea posterior a fecha inicio
3. Usar botones de navegación rápida

#### Datos No Aparecen
**Síntoma:** Tabla vacía después de ejecutar operación
**Solución:**
1. Verificar rango de fechas seleccionado
2. Comprobar que existan datos en el período
3. Usar botón **Ver** para consultar datos existentes

### 🔧 Verificaciones de Sistema

#### Antes de Usar
- ✅ Credenciales configuradas correctamente
- ✅ Conexión a red corporativa activa
- ✅ Navegador web actualizado

#### Durante Operaciones
- ✅ Monitorear mensajes de estado
- ✅ Verificar que las fechas sean válidas
- ✅ Confirmar selección de opciones correctas

#### Después de Operaciones
- ✅ Revisar resultados en tablas
- ✅ Validar calidad de datos mostrados
- ✅ Confirmar envío exitoso a ARES (si aplica)

