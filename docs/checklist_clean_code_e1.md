# Checklist de limpieza manual (E1)

## Objetivo
Esta guia esta pensada para que limpies el codigo manualmente sin romper los tests de E1.

## Plan de trabajo recomendado
- [ ] Leer primero el flujo completo de punta a punta.
- [ ] Clasificar archivos por nivel de riesgo antes de editar.
- [ ] Limpiar en capas (bajo riesgo -> medio -> alto).
- [ ] Ejecutar tests por grupo despues de cada bloque pequeno de cambios.
- [ ] Dejar cambios sensibles de formato/output para el final (o evitarlos).

---

## 1) Mapa rapido de interacciones

### Flujo principal
- `Octopath-Traveler-Controller/Game.cs`
  - Punto de entrada del combate.
  - Pide archivo de equipos a la vista.
  - Llama a `TeamsBuilder` para cargar y validar.
  - Ejecuta rondas y turnos.
- `Octopath-Traveler-Controller/TeamsBuilder.cs`
  - Carga JSONs (`characters`, `enemies`, `skills`, `passive_skills`).
  - Parsea el `.txt` de equipos.
  - Valida reglas de E1.
  - Construye `GameState`.
- `Octopath-Traveler-View/MainConsoleView.cs`
  - Todo input/output observable por tests.
  - Menus, estado de ronda, mensajes de ataque, ganador, etc.
- `Octopath-Traveler-Model/CombatFlow/*`
  - Estado mutable del combate (HP/SP/BP/shields, colas de turnos).
  - Soporte de ronda/turno/acciones.

### Reglas de oro
- Todo texto mostrado al usuario pasa por View.
- Orden de lineas y formato de strings son altamente sensibles.
- Si cambias una regla de target/dano/orden de turnos, se rompe facil.

---

## 2) Riesgo por archivo (donde tocar con cuidado)

### Riesgo bajo (ideal para empezar)
- `Octopath-Traveler-View/TeamsInfo.cs`
- `Octopath-Traveler-Model/CombatFlow/UnitReference.cs`
- `Octopath-Traveler-Model/CombatFlow/TurnEntry.cs`
- `Octopath-Traveler-Model/CombatFlow/RoundState.cs`

Puedes limpiar:
- nombres de variables locales,
- orden de miembros,
- extraer metodos privados pequenos,
- simplificar condicionales triviales.

### Riesgo medio
- `Octopath-Traveler-Model/Traveler.cs`, `Beast.cs`, `CombatStats.cs`, `SkillPoints.cs`
- `Octopath-Traveler-Model/GameState.cs`
- `Octopath-Traveler-Model/CombatFlow/CombatFlowState.cs`
- `Octopath-Traveler-Model/CombatFlow/TurnQueueFactory.cs`

Puedes limpiar:
- validaciones repetidas,
- cohesion de metodos,
- factorizar utilidades internas.

Evita:
- cambiar contratos publicos,
- cambiar semantica de validacion,
- cambiar ordenes de sort.

### Riesgo alto (dejar para el final)
- `Octopath-Traveler-Controller/Game.cs`
- `Octopath-Traveler-Controller/TeamsBuilder.cs`
- `Octopath-Traveler-View/MainConsoleView.cs`

Evita tocar si no es estrictamente necesario:
- textos impresos,
- secuencia de impresiones,
- parseo de archivo y reglas de invalidacion,
- formulas de dano y eleccion de target.

---

## 3) Checklist archivo por archivo

## `Octopath-Traveler-Controller/Game.cs`
Responsabilidad: orquesta combate E1 completo.

- [ ] Mantener `Play()` como capa de orquestacion (sin logica de formato).
- [ ] No cambiar orden de llamados de impresion/turnos.
- [ ] Si refactorizas `RunCombat()`, que sea solo extract method.
- [ ] Mantener formulas de dano y `Math.Floor`.
- [ ] Mantener regla de target de bestia (viajero con mayor HP, desempate por posicion).

## `Octopath-Traveler-Controller/TeamsBuilder.cs`
Responsabilidad: carga catalogos + parseo + validacion + construccion de estado.

- [ ] Extraer helpers de parseo en metodos privados cortos.
- [ ] Reemplazar magic strings por constantes privadas (`"Player Team"`, `"Enemy Team"`).
- [ ] Mantener regex y semantica de parseo intacta.
- [ ] Mantener reglas E1: tamanos maximos, duplicados, skills validas.
- [ ] No cambiar tipo de excepcion ni flujo de error global.

## `Octopath-Traveler-View/MainConsoleView.cs`
Responsabilidad: salida exacta para tests.

- [ ] No tocar textos literales si no es obligatorio.
- [ ] No agregar/quitar espacios, guiones o separadores.
- [ ] Si refactorizas, hacerlo sobre estructura interna (metodos privados), no contenido.
- [ ] Mantener orden de bloques: estado -> turnos ronda -> turnos siguiente.
- [ ] Mantener orden exacto de menus y opciones.

## `Octopath-Traveler-Model/CombatFlow/RoundState.cs`
Responsabilidad: administrar colas de ronda y turnos consumidos.

- [ ] Mantener invariantes (`number > 0`, colas no nulas).
- [ ] `PeekCurrentTurn()` no debe consumir.
- [ ] `ConsumeCurrentTurn()` debe consumir y registrar en `ResolvedTurns`.
- [ ] `ReplaceNextQueue()` solo reemplaza cola futura.
- [ ] Puedes convertir expresiones simples a expression-bodied members si mejora lectura.

## `Octopath-Traveler-Model/CombatFlow/TurnQueueFactory.cs`
Responsabilidad: construir orden de turnos deterministico.

- [ ] No cambiar criterios de ordenamiento.
- [ ] Si limpias, solo renombrado y extraccion de comun.
- [ ] No cambiar filtros de unidades vivas/actuables.

## `Octopath-Traveler-Model/CombatFlow/CombatFlowState.cs`
Responsabilidad: estado mutable del combate.

- [ ] Mantener actualizacion de HP y muerte sincronizada.
- [ ] Mantener `BuildViewSnapshot()` con orden por posicion de tablero.
- [ ] No cambiar reglas de `ApplyDamage` ni de remocion de colas al morir.
- [ ] Si extraes metodos, verificar que no cambie el momento de transicion de fase.

## `Octopath-Traveler-Model/*` (unidades)
Responsabilidad: datos e invariantes de dominio.

- [ ] Mantener clases como modelos de datos sin mutadores publicos innecesarios.
- [ ] Mantener validaciones de duplicados y maximos de skills.
- [ ] Evitar agregar logica de flujo de juego aqui.

---

## 4) Propuesta de limpieza por etapas (muy segura)

### Etapa A: lectura y naming (bajo riesgo)
- [ ] Renombrar variables locales ambiguas (`value`, `list`, `result`) por terminos de dominio.
- [ ] Ordenar miembros por convencion: const -> fields -> ctor -> public -> private.
- [ ] Eliminar comentarios obvios o desactualizados.

### Etapa B: extract method (medio)
- [ ] Extraer secciones largas en `Game.cs` y `TeamsBuilder.cs`.
- [ ] Limitar metodos a una responsabilidad clara.
- [ ] Mantener firmas publicas igual.

### Etapa C: consolidacion (alto riesgo controlado)
- [ ] Refactor leve en View solo si no toca strings.
- [ ] Revisar comparacion visual de output (linea por linea) tras cambios.

---

## 5) Matriz "que NO tocar" (si quieres cero riesgo)
- No cambiar literales de texto en `MainConsoleView`.
- No cambiar orden de impresiones.
- No cambiar sort de turnos.
- No cambiar formula de dano.
- No cambiar reglas de validacion de equipos E1.

---

## 6) Verificacion minima despues de cada bloque

```bash
cd "/Users/eberosg/Documents/u_2026/Detallado_2/Octopath"
dotnet test Octopath-Traveler.Tests/Octopath-Traveler.Tests.csproj --filter "FullyQualifiedName~TestE1_BasicCombat"
dotnet test Octopath-Traveler.Tests/Octopath-Traveler.Tests.csproj --filter "FullyQualifiedName~TestE1_InvalidTeams"
dotnet test Octopath-Traveler.Tests/Octopath-Traveler.Tests.csproj --filter "FullyQualifiedName~TestE1_RandomBasicCombat"
```

Si esos 3 grupos pasan, la limpieza probablemente no altero comportamiento observable.

---

## 7) Senales de alerta temprana (rollback inmediato)
- Un test falla en una linea de separador o menu: casi seguro tocaste formato/salida.
- Falla en casos invalidos: probablemente parseo/validacion.
- Falla con dano u objetivo: probablemente formula o target selection.
- Falla en orden de turnos: probablemente `TurnQueueFactory` o transicion de ronda.

---

## 8) Sugerencia de estrategia de commits
- Commit 1: renombres y orden de miembros (sin cambios funcionales).
- Commit 2: extract methods en Controller.
- Commit 3: limpieza menor en Model/CombatFlow.
- Commit 4 (opcional): limpieza menor en View (sin tocar strings).

Asi puedes revertir rapido el bloque que rompa algo.

