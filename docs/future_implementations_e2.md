# Future Implementations E2

## Objetivo de este documento
Este archivo explica:
- que se removio del modulo `CombatFlow` durante la limpieza de E1,
- por que se removio,
- y como reintroducir un motor desacoplado para las siguientes entregas (E2+)
  sin romper el flujo actual ni los tests existentes.

## Estado actual (post-limpieza E1)
Durante E1 se privilegio un flujo simple y estable, orquestado desde `Game`.
Para reducir ruido y deuda tecnica, se removio el scaffold de un motor desacoplado
que no estaba siendo usado en runtime.

### Archivos eliminados en `Octopath-Traveler-Model/CombatFlow`
- `CombatContracts.cs`
- `DeclaredAction.cs`
- `CombatFlowEngine.cs`

### Enums removidos de `CombatEnums.cs`
- `CombatActionType`
- `TurnResolution`

### Metodos removidos de `CombatFlowState.cs`
- `BeginTurn()`
- `DeclareAction(...)`
- `CancelTurn()`
- `EndRound()`
- propiedad `CurrentDeclaredAction`

### Que quedo activo en CombatFlow
- `CombatFlowState.cs`
- `CombatUnitState.cs`
- `CombatViewSnapshot.cs`
- `RoundState.cs`
- `TurnEntry.cs`
- `TurnQueue.cs`
- `TurnQueueFactory.cs`
- `UnitReference.cs`
- `CombatEnums.cs` (`CombatPhase`, `BattleResult`, `CombatantKind`)

## Por que se hizo este cambio
1. El flujo real de E1 no utilizaba el motor desacoplado.
2. Habia tipos y contratos sin consumidores reales.
3. Mantener codigo no usado elevaba complejidad y costo de mantenimiento.
4. La limpieza ayudo a cumplir objetivos de Clean Code en E1.

## Principio para E2 en adelante
Reintroducir un motor desacoplado solo cuando exista una necesidad funcional real,
por ejemplo:
- multiples politicas de decision (humano, IA, script),
- efectos complejos de habilidades,
- telemetria/event sourcing de combate,
- integracion con vistas alternativas sin tocar dominio.

## Plan recomendado para reintroduccion (incremental y seguro)

### Paso 1: Reintroducir contratos minimos
Crear nuevamente un archivo de contratos (nombre sugerido: `CombatContracts.cs`) con:
- `ITravelerActionSelector`
- `IBeastActionPolicy`
- `ICombatEventPort`

Regla: contratos pequenos, orientados al dominio, sin strings de UI.

### Paso 2: Reintroducir acciones declaradas
Crear nuevamente `DeclaredAction.cs` con jerarquia minima:
- `DeclaredAction` (base)
- `TravelerDeclaredAction`
- `BeastDeclaredAction`

Regla: modelar intencion de accion, no resolver efectos en estos tipos.

### Paso 3: Extender enums solo cuando se usen
Agregar `CombatActionType` y `TurnResolution` en `CombatEnums.cs`.
Regla: no agregar valores "por si acaso"; cada valor debe tener uso real.

### Paso 4: Reintroducir `CombatFlowEngine`
Crear `CombatFlowEngine.cs` como coordinador desacoplado:
- recibir dependencias por constructor (selectors/policies/event port),
- coordinar ronda -> turno -> accion,
- delegar mutaciones en `CombatFlowState`.

Regla: `Game` debe pasar de orquestador detallado a orquestador de alto nivel.

### Paso 5: Integracion gradual con `Game`
Migrar en capas:
1. Mantener `Game` actual y ejecutar engine en paralelo (modo sombra opcional).
2. Igualar outputs.
3. Cambiar `Game` para usar engine como camino principal.

Regla: no mover todo de una vez.

### Paso 6: Compatibilidad de salida
Los tests actuales dependen de output exacto.
Toda impresion debe seguir saliendo por View y mantener formato.
El engine no debe imprimir.

## Criterios de aceptacion para la reintroduccion
- Ningun cambio de output en E1.
- `dotnet test` completo sigue en verde.
- Los nuevos tipos tienen al menos un consumidor real.
- `Game` queda mas pequeno (mejor orquestacion, menos logica detallada).

## Riesgos frecuentes y mitigacion

### Riesgo: cambiar orden de impresiones
Mitigacion:
- no tocar literales de `MainConsoleView`,
- validar linea a linea con tests.

### Riesgo: duplicar fuentes de verdad de estado
Mitigacion:
- mantener `CombatFlowState` como unica fuente mutable.

### Riesgo: contratos demasiado amplios
Mitigacion:
- interfaces pequenas, orientadas a casos reales.

## Checklist corto para E2
- [ ] Reintroducir contratos minimos usados.
- [ ] Reintroducir acciones declaradas usadas.
- [ ] Agregar enums solo si hay consumidores.
- [ ] Crear `CombatFlowEngine` con responsabilidad unica.
- [ ] Integrar gradualmente en `Game`.
- [ ] Mantener View como unica capa de I/O.
- [ ] Ejecutar test suite completa.

## Nota final
La limpieza de E1 no bloquea E2; solo removio capas no utilizadas.
La base actual permite reintroducir el motor desacoplado en forma controlada,
con menor riesgo y mejor trazabilidad de cambios.

