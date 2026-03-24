# Entrega 1: Octopath Traveler — Resumen en Markdown

> Documento convertido y reorganizado desde el enunciado original para destacar **características**, **flujos del juego**, **reglas de combate**, **formatos de archivos** y **requerimientos de implementación**.

---

## 1. Objetivo de la entrega

En esta primera entrega se debe implementar el **flujo principal del combate** de Octopath Traveler, cubriendo el combate completo **sin habilidades activas funcionales ni habilidades de apoyo aplicadas**, usando únicamente:

- **Ataques básicos** de los viajeros.
- **Una acción simple de las bestias** (`Attack`).
- **Sin debilidades**, **sin BP/boosting efectivo** y **sin Breaking Point**.

### Alcance exacto

Se debe implementar:

1. **Flujo del combate completo**, desde el inicio hasta que exista un ganador.
2. **Validación de equipos**.
3. Acciones de:
   - **Atacar**
   - **Huir**
4. **Cancelar acciones**.
5. **Orden de turnos por stat `Speed`**.
6. **Lectura de unidades desde archivos JSON**.
7. **Cálculo de daño** sin:
   - debilidades,
   - Boost Points efectivos,
   - Breaking Point.

### Grupos de tests a pasar

- `E1-BasicCombat`
- `E1-InvalidTeams`
- `E1-RandomBasicCombat`

---

## 2. Supuestos permitidos en esta entrega

El enunciado permite trabajar bajo estas simplificaciones:

- Los **viajeros solo usarán ataques básicos**.
- Los viajeros **pueden tener habilidades activas** y se puede elegir la opción **usar habilidad**, pero **siempre se cancelará** esa selección.
- Los viajeros **no tendrán habilidades pasivas aplicadas** en esta entrega.
- **No se utilizará BP** realmente, ya que el boosting no forma parte de esta entrega.
- Las **bestias solo usarán la habilidad `Attack`**, que se comporta similar a un ataque básico.
- **Nunca se atacará una debilidad** de una bestia.
- No se debe implementar:
  - daño por debilidad,
  - breaking,
  - sistema de shields roto por debilidad,
  - boosting real.

---

## 3. Flujo general del juego

## 3.1 Selección de archivo de equipos

Al iniciar el programa:

1. Se debe mostrar al usuario la lista de archivos de equipo disponibles en la carpeta `teamsFolder`.
2. El usuario elige uno por índice.
3. El programa carga ese archivo.
4. Se valida el equipo.
5. Si el equipo es inválido, se muestra el mensaje de error y termina el programa.
6. Si es válido, comienza la simulación del combate.

### Formato esperado de salida al inicio

```text
Elige un archivo para cargar los equipos
0: 001.txt
1: 002.txt
2: 003.txt
...
```

---

## 3.2 Validación de equipos

Antes de iniciar el combate, se debe comprobar que el archivo de equipos sea válido.

### Si el archivo es inválido

Se debe mostrar:

```text
Archivo de equipos no válido
```

Y luego el programa termina.

### Qué implica validar

Del enunciado se desprende que la validación debe comprobar que:

- Los nombres de viajeros existan en `characters.json`.
- Los nombres de bestias existan en `enemies.json`.
- Las habilidades activas nombradas existan en `skills.json`.
- Las habilidades pasivas nombradas existan en `passive_skills.json`.

Además, el archivo debe respetar su formato estructural.

---

## 3.3 Inicio de ronda

Si el equipo es válido, el combate comienza en la **ronda 1**.

Cada vez que inicia una ronda se debe mostrar:

```text
----------------------------------------
INICIA RONDA X
----------------------------------------
```

Donde `X` es el número de ronda.

---

## 3.4 Estado general del juego antes de cada turno

Antes del turno de cada unidad se debe mostrar el estado general del combate:

1. **Equipo del jugador**
2. **Equipo del enemigo**
3. **Turnos de la ronda actual**
4. **Turnos de la siguiente ronda**

### Posiciones en tablero

- Los **viajeros** ocupan letras de **A a D**.
- Las **bestias** ocupan letras de **A a E**.
- La posición depende del **orden en que aparecen en el archivo del equipo**.
- Si una casilla no se usa, **no se muestra**.

### Formato para viajeros

```text
A - H'aanit - HP:3096/3096 SP:369/369 BP:1
```

Formato conceptual:

- `Nombre`
- `HP actual / HP máximo`
- `SP actual / SP máximo`
- `BP actual`

### Formato para bestias

```text
A - Meep - HP:1308/1308 Shields:2
```

Formato conceptual:

- `Nombre`
- `HP actual / HP máximo`
- `Shields actuales`

### Importante: unidades muertas también se muestran

Aunque una unidad haya muerto, debe seguir apareciendo en el resumen con `HP:0`.

Ejemplo:

```text
B - Tressa - HP:0/3080 SP:357/357 BP:1
```

---

## 3.5 Orden de turnos

Luego del estado del juego se deben mostrar:

- **Turnos de la ronda actual**
- **Turnos de la siguiente ronda**

### Regla principal

El orden de turnos se define por el stat **`Speed`**.

### Regla para la ronda actual

Solo se muestran las unidades que **todavía no han jugado** en esa ronda.

### Regla para la siguiente ronda

Se muestran todas las unidades que participarán en la próxima ronda, ordenadas según corresponda.

### Ejemplo

```text
----------------------------------------
Turnos de la ronda
1. H'aanit
2. Tressa
3. Meep
----------------------------------------
Turnos de la siguiente ronda
1. H'aanit
2. Tressa
3. Meep
```

Si una unidad ya jugó en la ronda actual, ya no aparece en esa cola.

---

## 4. Flujo del turno de un viajero

Cuando le toca jugar a un viajero, se muestra su menú de acciones:

```text
----------------------------------------
Turno de H'aanit
1: Ataque básico
2: Usar habilidad
3: Defender
4: Huir
```

## 4.1 Acciones relevantes para esta entrega

Aunque el menú muestra 4 acciones, en esta entrega solo se requiere implementar de forma efectiva:

- **Ataque básico**
- **Huir**
- **Cancelar** dentro de submenús

La opción **Usar habilidad** se puede mostrar, pero en tests se asumirá que finalmente se elige **Cancelar**.

La opción **Defender** aparece en menú, pero el enunciado de esta entrega no exige su comportamiento funcional.

---

## 5. Flujo de Ataque Básico

El ataque básico del viajero sigue este flujo:

1. El jugador elige **Ataque básico**.
2. El juego pide seleccionar un **arma** entre las permitidas para ese viajero.
3. Luego pide elegir un **objetivo enemigo vivo**.
4. Luego pregunta cuántos **BP** usar.
5. En esta entrega, siempre se ingresará `0`.
6. Se ejecuta el ataque.
7. Se muestra el resumen del daño.

---

## 5.1 Selección de arma

Si el jugador escoge ataque básico, se debe mostrar:

- Las armas disponibles del viajero
- La opción **Cancelar**

El orden de las armas es el mismo que aparece en `characters.json`.

### Ejemplo

```text
----------------------------------------
Seleccione un arma
1: Axe
2: Bow
3: Cancelar
```

---

## 5.2 Selección de objetivo

Luego se debe mostrar la lista de bestias **vivas** como posibles objetivos.

Cada bestia debe mostrarse con el mismo formato del estado del juego.

### Ejemplo

```text
----------------------------------------
Seleccione un objetivo para H'aanit
1: Meep - HP:1308/1308 Shields:2
2: Cancelar
```

### Restricción importante

- Solo deben mostrarse **bestias vivas**.
- Debe existir la opción **Cancelar**.

---

## 5.3 Selección de BP

Después de seleccionar arma y objetivo, el juego pregunta cuántos BP usar:

```text
----------------------------------------
Seleccione cuantos BP utilizar
```

### Regla especial de esta entrega

- Si el viajero tiene al menos **1 BP**, esta pregunta se muestra.
- Si tiene **0 BP**, no se pregunta.
- En los tests de esta entrega, cuando se pregunte por BP, el input será siempre **0**.
- **No es necesario implementar boosting real**.

---

## 5.4 Resolución del ataque

El ataque básico debe:

1. Calcular el daño.
2. Restarlo al HP del enemigo.
3. Mostrar resumen de la acción.

### Formato esperado

```text
----------------------------------------
H'aanit ataca
Meep recibe 373 de daño de tipo Axe
Meep termina con HP:935
```

### Caso especial: daño 0

Incluso si el daño calculado es `0`, el ataque igual ocurre y se informa:

```text
----------------------------------------
H'aanit ataca
Meep recibe 0 de daño de tipo Axe
Meep termina con HP:1308
```

---

## 6. Flujo de Usar Habilidad

Si el jugador elige **Usar habilidad**, se debe mostrar una lista de habilidades activas disponibles.

### Regla para esta entrega

- No se implementa el efecto de las habilidades.
- Se puede asumir que:
  - siempre habrá SP suficiente,
  - por lo tanto siempre se muestran todas las habilidades activas,
  - y finalmente el usuario elegirá **Cancelar**.

### Ejemplo

```text
----------------------------------------
Seleccione una habilidad para H'aanit
1: Rain of Arrows
2: True Strike
3: Cancelar
```

---

## 7. Flujo de Cancelar

**Cancelar** puede ocurrir en cualquier subflujo y debe devolver al menú de selección de acción del viajero.

### Casos donde aplica

- Cancelar selección de arma
- Cancelar selección de objetivo
- Cancelar selección de habilidad

### Comportamiento esperado

Si el jugador cancela en un punto intermedio, el juego vuelve a:

```text
----------------------------------------
Turno de H'aanit
1: Ataque básico
2: Usar habilidad
3: Defender
4: Huir
```

No debe consumirse el turno por cancelar.

---

## 8. Flujo de Huir

La acción **Huir** termina inmediatamente el combate y da por ganador al enemigo.

### Salida esperada

```text
----------------------------------------
El equipo de viajeros ha huido!
```

Después se debe mostrar el mensaje final de ganador del enemigo.

---

## 9. Flujo del turno de una bestia

Cuando le toca actuar a una bestia:

- **No se le pregunta nada al usuario**.
- La bestia es controlada por la computadora.
- El programa debe anunciar el uso de su habilidad.
- Luego debe mostrar el daño provocado y el HP restante del objetivo.

### Habilidad implementable de bestia

La única habilidad a implementar es:

- **Attack**
  - Descripción: realiza un ataque físico al viajero con mayor HP.
  - Representación resumida: `[Phys, 1.3, Single, 1]`

### Regla de target de `Attack`

La bestia ataca al **viajero con mayor HP actual**.

### Ejemplo de output

```text
----------------------------------------
Meep usa Attack
Tressa recibe 84 de daño físico
Tressa termina con HP:2996
```

---

## 10. Fin del combate y ganador

El combate termina en cualquiera de estos 3 casos:

1. **Todas las bestias mueren**  
   → gana el jugador

2. **Todos los viajeros mueren**  
   → gana el enemigo

3. **El jugador huye**  
   → gana el enemigo

### Mensaje si gana el jugador

```text
----------------------------------------
Gana equipo del jugador
```

### Mensaje si gana el enemigo

```text
----------------------------------------
Gana equipo del enemigo
```

---

## 11. Cálculo de daño

El enunciado indica que los cálculos pueden producir números decimales.

### Regla obligatoria

Cuando el daño calculado tenga decimales:

- Se debe **truncar hacia abajo**.
- En C# se recomienda:
  - `Math.Floor(...)`
  - y luego convertir a entero con `Convert.ToInt32(...)`.

### Restricciones del cálculo en esta entrega

El daño debe calcularse:

- **sin debilidades**
- **sin BP efectivo**
- **sin boosting**
- **sin Breaking Point**

---

## 12. Estructura de archivos de entrada

## 12.1 Archivo de equipos (`.txt`)

El archivo de equipos contiene:

1. Una sección `Player Team`
2. Los viajeros del jugador
3. Una sección `Enemy Team`
4. Las bestias enemigas

### Ejemplo

```text
Player Team
Primrose (Lion Dance, Moonlight Waltz, Peacock Strut) [The Show Goes On]
H'aanit (Rain of Arrows, True Strike, Thunderbird, Leghold Trap)
Cyrus [Elemental Augmentation]
Olberic
Enemy Team
Meep
Devourer of Dreams
```

### Reglas de parsing

Cada viajero puede tener:

- **habilidades activas** entre paréntesis `(...)`
- **habilidades pasivas** entre corchetes `[...]`

Puede tener:

- ambas,
- solo activas,
- solo pasivas,
- ninguna.

Las bestias solo se listan por nombre.

---

## 12.2 `characters.json`

Contiene la información de los viajeros:

- `Name`
- `Stats`
  - `HP`
  - `SP`
  - `PhysAtk`
  - `PhysDef`
  - `ElemAtk`
  - `ElemDef`
  - `Speed`
- `Weapons`

### Ejemplo conceptual

```json
{
  "Name": "Tressa",
  "Stats": {
    "HP": 3080,
    "SP": 357,
    "PhysAtk": 384,
    "PhysDef": 333,
    "ElemAtk": 360,
    "ElemDef": 285,
    "Speed": 240
  },
  "Weapons": ["Spear", "Bow"]
}
```

---

## 12.3 `enemies.json`

Contiene información de bestias:

- `Name`
- `Stats`
  - `HP`
  - `PhysAtk`
  - `PhysDef`
  - `ElemAtk`
  - `ElemDef`
  - `Speed`
- `Skill`
- `Shields`
- `Weaknesses`

### Ejemplo conceptual

```json
{
  "Name": "Meep",
  "Stats": {
    "HP": 1308,
    "PhysAtk": 321,
    "PhysDef": 131,
    "ElemAtk": 327,
    "ElemDef": 77,
    "Speed": 63
  },
  "Skill": "Attack",
  "Shields": 2,
  "Weaknesses": ["Bow", "Stave", "Dark"]
}
```

### Nota para esta entrega

Aunque el archivo incluye debilidades y shields, **no se implementan las mecánicas de debilidad/breaking**, pero sí se debe mostrar `Shields` en el estado del enemigo.

---

## 12.4 `skills.json`

Contiene habilidades activas de viajeros.

Campos:

- `Name`
- `SP`
- `Type`
- `Description`
- `Target`
- `Modifier`
- `Boost`

### Nota importante

En esta entrega, de las habilidades activas solo se usa realmente:

- **el nombre**, para validar equipos,
- y para mostrarlas en el menú de habilidades.

No se implementan sus efectos.

---

## 12.5 `passive_skills.json`

Contiene habilidades pasivas.

Campos:

- `Name`
- `Description`
- `Target`

### Nota

En esta entrega solo se usa su **nombre** para validación de equipos.

---

## 12.6 `beast_skills.json`

Contiene habilidades de bestias.

Campos:

- `name`
- `modifier`
- `description`
- `target`
- `hits`

### Habilidad relevante

```json
{
  "name": "Attack",
  "modifier": 1.3,
  "description": "Realiza un ataque físico al viajero con mayor HP",
  "target": "Single",
  "hits": 1
}
```

---

## 13. Reglas de salida e input

## 13.1 No usar `Console.WriteLine` ni `Console.ReadLine`

El enunciado exige **no usar directamente**:

- `Console.WriteLine(...)`
- `Console.ReadLine()`

Porque los tests **ignoran** lo que se envíe directo a consola.

## 13.2 Se debe usar el objeto `view`

Desde `Game.cs`, se debe usar el objeto `view`, que tiene:

- `ReadLine()`
- `WriteLine(string message)`

### Función de `view`

- `WriteLine(...)` guarda la salida para compararla con los tests.
- `ReadLine()` retorna automáticamente el input definido en el test.

### Consecuencia práctica

Todo lo que quieras que sea evaluado en los tests debe pasar por `view`.

---

## 14. Resumen funcional mínimo a implementar

Para pasar esta entrega, tu programa debería soportar este ciclo:

1. Mostrar archivos de equipos.
2. Leer selección del usuario.
3. Parsear archivo de equipo.
4. Validar viajeros, bestias y habilidades.
5. Si es inválido:
   - mostrar mensaje de error,
   - terminar.
6. Si es válido:
   - iniciar combate.
7. En cada ronda:
   - anunciar ronda,
   - mostrar estado del juego,
   - mostrar turnos actual/siguiente.
8. En turno de viajero:
   - mostrar menú,
   - permitir ataque básico,
   - permitir entrar a usar habilidad y cancelar,
   - permitir cancelar submenús,
   - permitir huir.
9. En turno de bestia:
   - ejecutar `Attack` automáticamente.
10. Repetir hasta detectar ganador.
11. Mostrar mensaje final de ganador.

---

## 15. Casos de flujo importantes que sí o sí deben contemplarse

## 15.1 Equipo inválido

- Debe terminar inmediatamente.

## 15.2 Unidad muerta sigue apareciendo en resúmenes

- Con `HP:0`.

## 15.3 Turno actual no muestra unidades que ya jugaron

- Pero la siguiente ronda sí las vuelve a incluir.

## 15.4 Cancelar no consume turno

- Solo vuelve al menú del viajero.

## 15.5 Huir termina instantáneamente el combate

- Y gana el enemigo.

## 15.6 Daño 0 se informa igual

- El ataque no se omite.

## 15.7 Pregunta de BP aparece solo si el viajero tiene BP > 0

- En la entrega, siempre se responderá `0`.

---

## 16. Rúbrica

La evaluación se divide en **funcionalidad** y **limpieza de código**.

## 16.1 Funcionalidad

Distribución:

- **0.7 puntos**: porcentaje de tests pasados en `E1-InvalidTeams`
- **3.0 puntos**: porcentaje de tests pasados en `E1-BasicCombat`
- **2.3 puntos**: pasar todos los tests de `E1-RandomBasicCombat`

## 16.2 Limpieza de código

Se parte de **6 puntos** y se descuentan por incumplimientos de Clean Code:

- **-2.0 puntos**: capítulo 2
- **-2.5 puntos**: capítulo 3

## 16.3 Nota final

La nota final es el **promedio geométrico** entre:

- puntaje de funcionalidad
- puntaje de limpieza

Y luego se suma el punto base indicado por el enunciado.

---

## 17. Restricción importante

No está permitido:

- modificar los test cases,
- modificar el proyecto `Octopath-Traveler.Tests`.

Hacerlo puede implicar penalización.

---

## 18. Checklist de implementación sugerido

- [ ] Mostrar lista de archivos de equipos.
- [ ] Leer elección del usuario usando `view.ReadLine()`.
- [ ] Parsear correctamente `Player Team` y `Enemy Team`.
- [ ] Validar nombres y habilidades contra JSON.
- [ ] Leer datos desde:
  - [ ] `characters.json`
  - [ ] `enemies.json`
  - [ ] `skills.json`
  - [ ] `passive_skills.json`
  - [ ] `beast_skills.json`
- [ ] Crear entidades de viajeros y bestias con stats actuales y máximos.
- [ ] Implementar orden por `Speed`.
- [ ] Mostrar estado del juego antes de cada turno.
- [ ] Mostrar cola de ronda actual y siguiente ronda.
- [ ] Implementar turno de viajero.
- [ ] Implementar ataque básico.
- [ ] Implementar cancelación de submenús.
- [ ] Implementar opción huir.
- [ ] Implementar turno automático de bestia con `Attack`.
- [ ] Detectar fin de combate.
- [ ] Mostrar ganador.
- [ ] Usar exclusivamente `view.WriteLine()` y `view.ReadLine()` para I/O evaluable.

---

## 19. Resumen ejecutivo

Esta entrega es, en esencia, una implementación del **motor base de combate por turnos** con foco en:

- carga y validación de datos,
- navegación de menús,
- orden de turnos,
- ejecución de ataques simples,
- control del flujo de combate,
- salida exacta compatible con tests.

No se busca todavía implementar el sistema completo de Octopath Traveler, sino una versión reducida del combate con suficientes piezas para probar:

- arquitectura,
- modelado de entidades,
- parsing de archivos,
- control de estado,
- limpieza de código.

