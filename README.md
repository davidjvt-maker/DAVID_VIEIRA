# Sistema de Distribución de Ítems de Trabajo

## Descripción General

Sistema de microservicios en .NET 9 que implementa distribución inteligente de ítems de trabajo basada en:
- Fechas de entrega próximas (< 3 días)
- Carga de trabajo de usuarios
- Relevancia de ítems (Alta/Mediana/Baja)
- Saturación de usuarios (máx. 3 ítems de alta relevancia)

### Servicios

#### 1. **UserService**
- **Puerto HTTPS:** 7021
- **Puerto HTTP:** 5116
- **Endpoint:** `GET /api/users`
- Retorna lista de usuarios disponibles para asignación

#### 2. **ItemsWorkService**
- **Puerto HTTPS:** 7288
- **Puerto HTTP:** 5207
- **Endpoints:**
  - `GET /api/workitems` — Todos los ítems
  - `GET /api/workitems/user/{username}` — Ítems por usuario
  - `POST /api/workitems/distribute` — Crear y distribuir ítem

## Tecnología

- **.NET:** 9.0

## Ejemplos de Uso

### 1. Crear y Distribuir Ítem

**Request:**
```bash
POST https://localhost:7288/api/workitems/distribute
Content-Type: application/json

{
  "id": "00000000-0000-0000-0000-000000000000",
  "title": "Implementar autenticación",
  "deliveryDate": "2026-07-10T10:00:00",
  "relevance": 0,
  "status": 0,
  "assignedUsername": null
}
```

**Response:**
```json
{
  "message": "Asignado exitosamente",
  "item": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Implementar autenticación",
    "deliveryDate": "2026-07-10T10:00:00",
    "relevance": 0,
    "status": 0,
    "assignedUsername": "usuario_juan"
  }
}
```

### 2. Obtener Todos los Ítems

**Request:**
```bash
GET https://localhost:7288/api/workitems
```

**Response:**
```json
[
  {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Implementar autenticación",
    "deliveryDate": "2026-07-10T10:00:00",
    "relevance": 0,
    "status": 0,
    "assignedUsername": "usuario_juan"
  }
]
```

### 3. Obtener Ítems por Usuario

**Request:**
```bash
GET https://localhost:7288/api/workitems/user/usuario_juan
```

**Response:**
```json
[
  {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Implementar autenticación",
    "deliveryDate": "2026-07-10T10:00:00",
    "relevance": 0,
    "status": 0,
    "assignedUsername": "usuario_juan"
  }
]
```

### 4. Obtener Usuarios

**Request:**
```bash
GET https://localhost:7021/api/users
```

**Response:**
```json
[
  "usuario_juan",
  "usuario_maria",
  "usuario_pedro"
]
```


