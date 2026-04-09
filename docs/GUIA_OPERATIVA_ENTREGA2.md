# Guia operativa integral (estado actual) - Octopath Traveler

## 1) Objetivo de este documento

Este documento describe el estado real del proyecto hasta hoy, con foco en:
- funcionalidades implementadas,
- archivos y simbolos criticos,
- deuda tecnica y oportunidades de refactorizacion,
- reglas de trabajo para un agente IA o una persona que continue el desarrollo.

Referencia obligatoria de estilo y arquitectura: `docs/clean_code_guidelines.md`.

---

## 2) Estado validado del proyecto

Estado comprobado en esta sesion:
- E2: 207/207 tests pasando.
- E1 (smoke): 122/122 tests pasando.

Notas:
- Este estado aplica al codigo actual de la rama/workspace.
- La suite E3 existe, pero no forma parte de esta certificacion.

---

## 3) Mapa de arquitectura (MVC)

- Model (`Octopath-Traveler-Model/`): entidades, estado mutable de combate, colas de turno, reglas base de estado.
- Controller (`Octopath-Traveler-Controller/`): orquestacion de combate, resolucion de acciones y skills, carga de catalogos y armado de equipos.
- View (`Octopath-Traveler-View/`): entrada/salida, render de mensajes exactos para tests.
- Tests (`Octopath-Traveler.Tests/Tests.cs`): comparacion linea a linea del script generado.
- Data (`data/` y `bin/.../data`): catalogos JSON y casos de prueba.

Regla clave de pruebas:
- La salida se valida por igualdad exacta de texto, linea por linea.
- Cualquier cambio de formato rompe tests aunque la logica sea correcta.

---

## 4) Funcionalidades implementadas (estado real)

## 4.1 Carga de equipos y validacion
Archivo principal: `Octopath-Traveler-Controller/TeamsBuilder.cs`

Implementado:
- Parseo de archivo de equipo (`Player Team` / `Enemy Team`).
- Parseo de viajeros con secciones opcionales `()` (activas) y `[]` (pasivas).
- Validaciones de formato, tamano de equipos y duplicados.
- Validacion de existencia de personajes, bestias, habilidades activas y pasivas.
- Carga de catalogos JSON:
  - `characters.json`, `enemies.json`, `skills.json`, `passive_skills.json`, `beast_skills.json`.
- Construccion de `GameState` (equipos + board + `CombatFlowState`).
- Aplicacion de pasivas base de E2 al construir traveler (ajuste de stats/recursos):
  - `Inner Strength`, `Elemental Augmentation`, `Summon Strength`, `Hale and Hearty`, `Fleefoot`.

## 4.2 Flujo de combate y rondas
Archivo principal: `Octopath-Traveler-Controller/Game.cs`

Implementado:
- `Play()` -> seleccion de archivo -> construccion de estado -> bucle de combate.
- Ciclo de ronda:
  1. recarga BP (ronda > 1),
  2. actualizacion de estados de breaking,
  3. actualizacion de modificadores de prioridad,
  4. construccion de colas (ronda actual y proyectada),
  5. resolucion de turnos hasta agotar cola o terminar batalla.
- Fin de batalla:
  - victoria jugador si no quedan bestias vivas,
  - derrota jugador si no quedan travelers vivos,
  - huir produce derrota del jugador (segun scripts).

## 4.3 Acciones del traveler
Archivo principal: `Octopath-Traveler-Controller/Game.cs`

Implementado:
- Menu principal de turno:
  1) Ataque basico
  2) Usar habilidad
  3) Defender
  4) Huir
- Cancelaciones en seleccion de arma, habilidad y objetivo.
- Consumo de SP para habilidades.
- Solicitud de BP (interfaz), sin sobrecarga completa de boost E3.

## 4.4 Ataque basico y dano
Archivos: `Game.cs`, `CombatFlowState.cs`, `MainConsoleView.cs`

Implementado:
- Ataque basico fisico con modificador base.
- Verificacion de debilidad por arma/elemento.
- Multiplicadores por debilidad y/o breaking point.
- Aplicacion de dano y muerte de unidad.
- Consumo de shield cuando corresponde por debilidad.
- Mensajeria detallada (danio, debilidad, entrada a breaking, HP final).

## 4.5 Breaking Point y escudos
Archivos: `CombatFlowState.cs`, `Game.cs`, `TurnQueueFactory.cs`

Implementado:
- `TryConsumeBeastShield` reduce escudos y activa breaking al llegar a 0.
- Al entrar en breaking:
  - se remueve unidad de cola actual y siguiente,
  - no actua este turno,
  - se inicia contador de rounds de breaking.
- Recuperacion de breaking:
  - restauracion de escudos,
  - prioridad de recuperacion en ronda definida por flags.
- Comportamiento multi-hit alineado a scripts E2:
  - se permite consumir escudo aun cuando el objetivo ya quedo en 0 HP dentro de la misma secuencia de golpes.

## 4.6 Defender y prioridades
Archivos: `Game.cs`, `CombatUnitState.cs`, `TurnQueueFactory.cs`

Implementado:
- Defender marca estado defensivo y prioridad para proxima ronda.
- Mitigacion de dano cuando objetivo esta defendiendo (si la skill no ignora defender).
- Sistema de prioridad por tiers en colas de turno:
  - recuperacion de breaking,
  - prioridad de defender,
  - prioridad aumentada/disminuida,
  - orden base por velocidad/rol/slot segun tier.

## 4.7 Habilidades activas ofensivas del traveler
Archivo principal: `Game.cs`

Implementado (single target o area segun skill):
- Single target: `Holy Light`, `Tradewinds`, `Cross Strike`, `Moonlight Waltz`, `Icicle`, `Amputation`, `Wildfire`, `True Strike`, `Thunderbird`, `Mercy Strike`, `Qilin's Horn`, `Phoenix Storm`.
- Area enemigos: `Fireball`, `Icewind`, `Lightning Bolt`, `Luminescence`, `Trade Tempest`, `Level Slash`, `Night Ode`, `Tiger Rage`, `Yatagarasu`, `Fox Spirit`, `Last Stand`.
- Especiales:
  - `Nightmare Chimera` (elige tipo de arma entre set fisico).
  - `Shooting Stars` (multi-hit por objetivo: Wind/Light/Dark con evaluacion por hit).
- Reglas relevantes:
  - `Mercy Strike` no mata (deja en HP 1).
  - `Last Stand` escala con HP faltante del caster.
  - registro de entrada a breaking por hit cuando aplique.

## 4.8 Habilidades de soporte y curacion
Archivo principal: `Game.cs`

Implementado:
- `Heal Wounds`, `Heal More` (cura de grupo de aliados vivos).
- `First Aid` (cura single target aliado vivo).
- `Revive` (revive aliados muertos a 1 HP, luego HP final segun skill).
- `Vivify` (target aliado muerto; revive + cura, o comportamiento segun estado).
- Menus de seleccion de aliados alineados a scripts:
  - si no hay candidatos validos, se muestra solo opcion `Cancelar`.

## 4.9 Habilidades de control de turno
Archivo principal: `Game.cs`

Implementado:
- `Spearhead`:
  - ataque fisico tipo Spear,
  - aplica prioridad aumentada para siguiente ronda.
- `Leghold Trap`:
  - aplica prioridad reducida,
  - contempla si objetivo aun tiene turno pendiente en la ronda actual,
  - reordena cola restante de ronda actual moviendo el objetivo al final cuando corresponde,
  - maneja acumulacion de duracion entre usos sucesivos segun comportamiento esperado por E2.

## 4.10 IA de bestias y skills de bestias
Archivos: `BeastSkillCatalog.cs`, `Game.cs`, `CombatViewMessages.cs`, `MainConsoleView.cs`

Implementado:
- Catalogo data-driven (`beast_skills.json`) con mapeo a `BeastSkillSpec`.
- Resolucion por metadata:
  - tipo de ataque (`Physical`, `Elemental`, `HalveCurrentHp`),
  - hits,
  - objetivo single o area,
  - regla de seleccion de target (mayor HP, mayor speed, menor phys def, etc.),
  - ignorar defender en skills especificas (ej: halve HP).
- Soporte de ataques de bestia de area con render dedicado.
- Ajustes de formato y redondeo alineados a scripts E2 (incluyendo dano de "mitad del HP actual").

## 4.11 Vista y contratos de salida
Archivos: `MainConsoleView.cs`, `CombatViewMessages.cs`, `View.cs`, `TestingView.cs`

Implementado:
- Render completo de estado por ronda:
  - equipo jugador,
  - equipo enemigo,
  - cola actual,
  - cola siguiente.
- Menus de seleccion con opcion cancelar.
- DTOs de salida para:
  - ataque basico,
  - skills single target,
  - skills area,
  - multi-hit,
  - curaciones,
  - revivir,
  - ataques de bestia single/area.
- Script testing via `View.BuildTestingView(...)` y comparacion exacta en tests.

---

## 5) Archivos criticos por capa

## 5.1 Controller
- `Octopath-Traveler-Controller/Game.cs`
  - Orquestador principal de combate (muy alta complejidad actual).
  - Contiene flujo, resolucion de acciones, reglas de dano, parte de prioridades y skills.
- `Octopath-Traveler-Controller/TeamsBuilder.cs`
  - Parseo/validacion del archivo de equipos + carga de catalogos + construccion de dominio.
- `Octopath-Traveler-Controller/BeastSkillCatalog.cs`
  - Catalogo data-driven de skills de bestias.
- `Octopath-Traveler-Controller/TurnContexts.cs`
  - Contextos tipados para resolver turnos.

## 5.2 Model
- `Octopath-Traveler-Model/CombatFlow/CombatFlowState.cs`
  - Estado mutable de combate, dano, muerte, escudos, rounds/colas.
- `Octopath-Traveler-Model/CombatFlow/TurnQueueFactory.cs`
  - Ordenamiento de turnos por tier/prioridad/velocidad.
- `Octopath-Traveler-Model/CombatFlow/CombatUnitState.cs`
  - Flags de estado por unidad (vida, SP/BP, escudos, prioridades).
- `Octopath-Traveler-Model/GameState.cs`
  - Agregado principal del estado de juego.

## 5.3 View
- `Octopath-Traveler-View/MainConsoleView.cs`
  - Formato de salida y entradas del usuario (contrato estricto de tests).
- `Octopath-Traveler-View/CombatViewMessages.cs`
  - DTOs/records de mensajes de combate.
- `Octopath-Traveler-View/TestingView.cs`
  - Reproduccion de scripts de input para pruebas.

## 5.4 Tests
- `Octopath-Traveler.Tests/Tests.cs`
  - Runner de todos los grupos E1/E2/E3.
  - Comparacion literal de script linea por linea.

## 5.5 Data
- `data/*.json`: fuente de verdad para stats, skills activas/pasivas y skills de bestias.
- `data/E*-*` + `data/E*-*-Tests`: definicion de escenarios y oraculos esperados.

---

## 6) Brechas de documentacion detectadas

Hay divergencia entre documentacion antigua y estado actual del codigo.

Ejemplos:
- `docs/PROJECT_OVERVIEW.md` marca varios features E2 como "no implementadas", pero hoy si estan implementadas y validadas por tests E2.
- `docs/CONTROLLER.md`, `docs/MODEL.md`, `docs/VIEW.md` no reflejan completamente:
  - BeastSkillCatalog data-driven,
  - flujo completo de skills ofensivas/curacion/revive,
  - reglas finas de prioridad/cola,
  - DTOs nuevos de salida.

Accion recomendada: actualizar estos docs luego del refactor para evitar duplicar esfuerzo dos veces.

---

## 7) Deuda tecnica y refactorizaciones recomendadas (Clean Code)

Prioridad alta (impacto fuerte, bajo riesgo funcional si se hace incrementalmente):

1. Dividir `Game.cs` por responsabilidades (SRP)
   - Extraer servicios:
     - `RoundOrchestrator` (prepare/start/close round),
     - `TravelerActionResolver`,
     - `BeastActionResolver`,
     - `DamageResolver`,
     - `PriorityResolver`.
   - Beneficio: reduce complejidad ciclomatica y facilita pruebas unitarias por pieza.

2. Centralizar metadata de skills de traveler
   - Reemplazar switches masivos (`GetSkillSpCost`, `TryGetSingleTargetOffensiveSkill`, `TryGetEnemiesTargetOffensiveSkill`) por catalogo data-driven.
   - Beneficio: OCP, menos puntos de fallo, menor hardcode.

3. Consolidar pipeline de dano
   - Unificar calculo raw + multiplicadores + mitigaciones + side effects (shield/breaking/mercy).
   - Beneficio: evita diferencias de redondeo entre skills.

4. Formalizar politica de prioridad/cola
   - Extraer reglas de prioridad y duracion (defender/leghold/spearhead/break recovery) a una capa dedicada.
   - Beneficio: evita regresiones en orden de turnos y simplifica razonamiento.

Prioridad media:

5. Separar parseo y validacion en `TeamsBuilder.cs`
   - `TeamFileParser`, `TeamValidator`, `CatalogLoader`, `TeamFactory`.
   - Beneficio: funciones mas cortas y cohesion alta.

6. Reducir dependencia de strings literales de skill
   - Introducir constants centralizadas o identificadores tipados.
   - Beneficio: menos errores por typo y mejor trazabilidad.

7. Mejorar aislamiento testable del Controller
   - Inyectar interfaces para catalogos/resolvedores cuando aplique.
   - Beneficio: pruebas mas pequeñas y rapidas.

Prioridad baja:

8. Limpieza de codigo no utilizado / duplicado
   - Revisar metodos de dano legacy no usados o redundantes.
   - Revisar helpers de vista para evitar duplicacion de formato.

---

## 8) Protocolo operativo para un agente IA o desarrollador

## 8.1 Reglas no negociables
- Seguir `docs/clean_code_guidelines.md`.
- Mantener separacion MVC estricta.
- No cambiar formato de salida sin correr tests inmediatamente.
- No tocar `Octopath-Traveler.Tests/Tests.cs` salvo instruccion explicita del usuario.

## 8.2 Flujo de trabajo recomendado (paso a paso)
1. Reproducir fallo con filtro minimo posible (grupo E2 o archivo puntual).
2. Identificar primera linea divergente `[L#]`.
3. Localizar causa en Controller/Model/View.
4. Aplicar cambio pequeno y localizado (sin hardcode por caso de test).
5. Re-ejecutar grupo afectado.
6. Re-ejecutar `TestE2` completo.
7. Smoke de `TestE1`.
8. Documentar decision tecnica y riesgos.

## 8.3 Comandos de referencia
```bash
# E2 completo

dotnet test "Octopath-Traveler.Tests/Octopath-Traveler.Tests.csproj" --filter "FullyQualifiedName~TestE2"

# E1 smoke

dotnet test "Octopath-Traveler.Tests/Octopath-Traveler.Tests.csproj" --filter "FullyQualifiedName~TestE1"

# Grupo especifico E2 (ejemplo)

dotnet test "Octopath-Traveler.Tests/Octopath-Traveler.Tests.csproj" --filter "FullyQualifiedName~TestE2_HealingAndQueueSkills"
```

---

## 9) Backlog de evolucion sugerido

Fase 1 (estabilizacion estructural)
- Extraer resolucion de turnos de `Game.cs` a clases dedicadas.
- Congelar salida de View (golden contract) y cubrir con tests de regresion.

Fase 2 (data-driven traveler skills)
- Migrar metadata de skills de traveler a catalogo.
- Reemplazar switches de skill por dispatch por descriptor.

Fase 3 (hardening de motor de combate)
- Pruebas unitarias de prioridad/colas por regla aislada.
- Pruebas unitarias de dano y breaking por tipo de skill.

Fase 4 (E3 y siguientes)
- Integrar boost/passives avanzadas sobre arquitectura modular ya extraida.

---

## 10) Checklist de handoff para continuar trabajo

Antes de empezar:
- [ ] Leer `docs/clean_code_guidelines.md`.
- [ ] Revisar `docs/GUIA_OPERATIVA_ENTREGA2.md`.
- [ ] Ejecutar E2 completo para confirmar baseline verde.

Durante cambios:
- [ ] Cambios pequenos, con una hipotesis por commit.
- [ ] Nada de condiciones por nombre de test/caso.
- [ ] Mantener mensajes de salida exactamente compatibles.

Antes de cerrar:
- [ ] E2 verde.
- [ ] E1 smoke verde.
- [ ] Actualizar documentacion afectada.

---

## 11) Resumen ejecutivo

Hoy el proyecto tiene E2 funcionalmente implementado y validado por tests, con E1 estable. El mayor riesgo tecnico no es de funcionalidad sino de estructura: `Game.cs` concentra demasiadas responsabilidades. La siguiente ganancia real de calidad es modularizar el controller por reglas de dominio (turnos, dano, prioridades, skills), manteniendo el contrato de salida estricto de la View.

