# 4. Sistemas de juego

Una vez conformados los equipos, el jugador deberá tomar decisiones sobre sus viajeros para intentar vencer al equipo enemigo de bestias. Por su parte, el equipo de bestias será controlado de forma automática.

Antes de profundizar en las mecánicas del combate, es necesario comprender los elementos esenciales del sistema de juego:

- los ataques;
- el sistema de rondas y turnos;
- las acciones disponibles para cada tipo de unidad;
- el boosting;
- los shields;
- el breaking point;
- y las reglas de armado de la cola de turnos.

Este capítulo define la lógica central del combate y establece cómo progresa una batalla desde el inicio de una ronda hasta la resolución de todas las acciones.

---

## 4.1. Turnos

Octopath Traveler es un juego por turnos, donde el orden en que actúan las unidades puede variar según sus atributos y las decisiones que se tomen durante el combate.

### Estructura general

El combate se desarrolla en **rondas**.  
En cada ronda, todas las unidades participantes —viajeros y bestias— tienen la oportunidad de actuar una vez.

Cada acción ejecutada por una unidad corresponde a un **turno**.

En condiciones normales:
- cada unidad dispone de un turno por ronda;
- cuando todas las unidades han actuado, la ronda termina;
- al terminar una ronda, comienza la siguiente.

### Excepciones a la regla base

Aunque el sistema general establece un turno por unidad en cada ronda, ciertas habilidades pueden modificar esta lógica.

Por ejemplo:
- algunas habilidades pueden otorgar a un viajero la posibilidad de actuar nuevamente;
- ciertas mecánicas pueden alterar el orden original de actuación;
- el estado de **Breaking Point** puede impedir que una bestia tome su turno.

Esto significa que el sistema de turnos no debe modelarse como una secuencia rígida, sino como una estructura dinámica afectada por estados, habilidades y prioridades temporales.

### Orden de los turnos

De forma base, el orden de actuación dentro de cada ronda se determina según la **Speed** de las unidades presentes en combate.

Con esos valores se construye una **cola de turnos**, donde:
- las unidades con mayor velocidad actúan antes;
- las unidades con menor velocidad actúan después.

Sin embargo, esta cola no depende únicamente de la velocidad. También puede alterarse por:
- habilidades que aumentan o disminuyen prioridad;
- la acción **Defender**;
- la explotación de debilidades;
- el ingreso o salida del estado de **Breaking Point**.

Por lo tanto, la velocidad actúa como criterio base, pero no como único criterio de orden.

---

## 4.2. Tipos de ataque y debilidades

En el juego existen distintos tipos de ataques, que pueden resultar útiles según la situación y el enemigo enfrentado.

Los tipos de ataque se clasifican en dos grandes categorías:

### Ataques físicos

Son aquellos cuyo daño se determina en función de:
- **Phys Atk** del atacante;
- **Phys Def** del objetivo.

Los tipos de ataque físico son:

- **Sword**
- **Spear**
- **Axe**
- **Dagger**
- **Bow**
- **Stave**

### Ataques elementales

Son aquellos cuyo daño se determina en función de:
- **Elem Atk** del atacante;
- **Elem Def** del objetivo.

Los tipos de ataque elemental son:

- **Fire**
- **Ice**
- **Lightning**
- **Wind**
- **Light**
- **Dark**

### Debilidades enemigas

Cada bestia del equipo enemigo tiene al menos una debilidad asociada a uno o más tipos de ataque, ya sean físicos o elementales.

Atacar a una bestia con un tipo al que es débil tiene efectos importantes en el combate:
- la bestia recibe una mayor cantidad de daño;
- se favorece la reducción de sus **Shields**;
- puede alterarse el flujo de la batalla al acercarla al estado de **Breaking Point**.

### Casos especiales

Existen excepciones que el sistema debe contemplar:

- puede haber ataques físicos o elementales que no tengan tipo específico;
- puede haber ataques que no sean ni físicos ni elementales.

Esto implica que la lógica del combate no debe asumir que todo ataque pertenece necesariamente a una categoría tradicional o que siempre interactúa con debilidades.

---

## 4.3. Acciones

Las unidades pueden realizar distintas acciones cuando llega su turno.  
Estas acciones generan efectos diversos dentro del combate y constituyen la base de la toma de decisiones del jugador.

Aquí existe una diferencia fundamental entre viajeros y bestias:

- los **viajeros** pueden escoger entre varias acciones;
- las **bestias** actúan automáticamente y no eligen entre múltiples opciones.

### Acciones de los viajeros

Cada vez que llega el turno de un viajero, el jugador debe seleccionar una de las siguientes opciones:

#### Ataque básico

El viajero selecciona una de sus armas y la utiliza para atacar a un enemigo.

Características:
- todas las armas corresponden a tipos físicos;
- por lo tanto, el ataque básico siempre es un **ataque físico**;
- el tipo concreto del ataque depende del arma utilizada;
- esta acción puede interactuar con debilidades del enemigo.

#### Usar habilidad activa

El jugador selecciona una de las habilidades activas del viajero.

Estas habilidades pueden:
- infligir daño físico;
- infligir daño elemental;
- o generar otros efectos sobre el combate.

El sistema debe permitir que una habilidad activa no sea solo ofensiva, sino también de soporte, alteración o control.

#### Defender

El viajero gasta su turno para aumentar su resistencia.

Efectos de esta acción:
- durante el resto de la ronda, el viajero recibe **50 % del daño** que recibiría normalmente;
- además, obtiene **prioridad de turno en la siguiente ronda**.

Esta acción tiene un doble valor táctico:
- reduce el daño inmediato;
- reposiciona estratégicamente al viajero en la cola futura.

#### Huir

El viajero abandona el combate.

Consecuencia:
- el juego termina inmediatamente;
- los enemigos son considerados ganadores.

Esto implica que huir no es una acción de combate parcial, sino una condición de finalización del enfrentamiento.

### Acciones de las bestias

Cada vez que llega el turno de una bestia, esta solo tiene la opción de usar su habilidad.

Características:
- las bestias no eligen entre acciones alternativas;
- su comportamiento está automatizado;
- la habilidad puede causar daño físico, daño elemental o producir otros efectos sobre el combate;
- el objetivo y el efecto concreto deben estar descritos por la habilidad misma.

Por diseño, las bestias no requieren un sistema de selección manual de acciones, sino una lógica automática de ejecución.

---

## 4.4. Boosting

Cada viajero dispone de una cantidad de **Boost Points**, abreviados como **BP**.

Durante el turno de un viajero, el jugador puede gastar BP para ejecutar su acción con **boosting**, amplificando su efecto.

### Efecto del boosting según la acción

#### Ataque básico con boosting

Se realiza un ataque adicional por cada BP gastado.

Ejemplo:
- si un viajero usa ataque básico con **2 BP**,
- ejecutará su ataque normal,
- más **2 ataques adicionales**.

Esto significa que el boosting sobre ataque básico incrementa directamente la cantidad de golpes realizados.

#### Habilidad activa con boosting

El boosting potencia el efecto de la habilidad utilizada.

La forma exacta de esta mejora depende de cada habilidad.  
Por ejemplo:
- una habilidad de daño puede infligir más daño por cada BP gastado;
- otras habilidades podrían extender duración, intensidad o magnitud de sus efectos.

Por lo tanto, el sistema debe permitir que cada habilidad defina su propia regla de escalado por BP.

#### Acciones que no permiten boosting

Las acciones:
- **Defender**
- **Huir**

no permiten gastar BP.

### Reglas de BP

Cada viajero sigue las siguientes reglas respecto a los Boost Points:

#### BP inicial
Cada viajero inicia el combate con **1 BP**.

#### Ganancia de BP
Al final de cada ronda, todos los viajeros vivos obtienen **1 BP adicional**.

#### Límite de acumulación
Un viajero puede almacenar hasta un máximo de **5 BP**.

#### Límite de gasto por acción
Aunque un viajero tenga más de 3 BP acumulados, al realizar una acción con boosting solo puede gastar **hasta 3 BP** en esa acción.

#### Penalización por usar boosting
Si un viajero realizó una acción con boosting en una ronda, **no recibirá 1 BP al final de esa ronda**.

### Implicancia de diseño

El sistema de BP genera una economía de recursos táctica:
- guardar BP permite planificar acciones más fuertes en rondas futuras;
- gastar BP produce un aumento inmediato de poder;
- pero usar boosting también sacrifica la ganancia automática de la ronda.

Esto obliga al jugador a balancear agresividad presente contra preparación futura.

---

## 4.5. Shields

Cada bestia posee una cantidad de **Shields** asociada.

Este valor indica cuántas veces debe ser golpeada en alguna de sus debilidades antes de entrar en estado de **Breaking Point**.

### Función de los Shields

Los Shields actúan como una capa de resistencia táctica.

Mientras una bestia conserve Shields:
- puede seguir actuando normalmente;
- aún no entra en estado de ruptura;
- el jugador debe seguir explotando sus debilidades para quebrarla.

### Regla de reducción

Una bestia pierde Shields cuando es golpeada con un ataque correspondiente a una de sus debilidades.

Sin embargo, existe una condición importante:
- si el ataque hace **0 de daño**, entonces la bestia **no pierde Shields** por ese ataque.

Esto significa que no basta con acertar un tipo de debilidad; el ataque también debe impactar efectivamente para reducir Shields.

### Entrada en Breaking Point

Cuando los Shields de una bestia llegan a **0**, esta entra al estado de **Breaking Point**.

Este estado dura:
- la ronda actual;
- y la siguiente ronda.

### Efectos del Breaking Point

Mientras una bestia está en Breaking Point:
- no puede realizar ningún tipo de acción;
- recibe daño adicional de cualquier ataque que reciba.

Además:
- si todavía no había actuado en la ronda actual al momento de entrar en Breaking Point,
  pierde también ese turno actual.

### Recuperación

Después de transcurridas las dos rondas del Breaking Point:
- la bestia sale de ese estado;
- reinicia su cantidad de Shields;
- obtiene prioridad en el orden de turnos de la siguiente ronda.

Esto convierte al Breaking Point en una desventaja temporal muy fuerte, seguida por una recuperación con reposicionamiento preferente en la cola futura.

---

## 4.6. Manejo de la cola de turnos

Dentro de una ronda, todas las unidades —viajeros y bestias— deben ordenarse bajo ciertas reglas para determinar cuándo será su turno de actuar.

La cola de turnos no se construye únicamente por velocidad. Existe un algoritmo de prioridad que organiza a las unidades según estados y efectos especiales.

## Algoritmo de construcción de la cola

### 1. Bestias que se recuperan del Breaking Point

Se ubican primero en la cola las bestias que se recuperen del estado de Breaking Point.

Si dos o más bestias se recuperan en la misma ronda:
1. se ordenan por **Speed**, de mayor a menor;
2. si persiste el empate, se ordenan por **orden de tablero**, de izquierda a derecha.

### 2. Viajeros que usaron Defender en la ronda anterior

Luego se ubican los viajeros que hayan utilizado la acción **Defender** en la ronda previa.

Si dos o más viajeros cumplen esta condición:
1. se ordenan por **Speed**, de mayor a menor;
2. si empatan, se ordenan por **orden de tablero**.

### 3. Unidades con prioridad aumentada por habilidades

Después se posicionan las unidades que, por efecto de alguna habilidad, hayan aumentado su prioridad en la ronda.

Si dos o más unidades están en esta categoría:
1. se prioriza a los **viajeros** sobre las **bestias**;
2. si aún hay empate, se ordenan por **Speed**;
3. si el empate persiste, se usa el **orden de tablero**.

### 4. Unidades sin condiciones especiales

Luego se posicionan las unidades que no pertenecen a ninguna de las categorías anteriores.

Reglas:
1. se ordenan por **Speed**, de mayor a menor;
2. si dos unidades empatan en velocidad, se prioriza a los **viajeros** sobre las **bestias**;
3. si el empate ocurre entre unidades del mismo tipo, se usa el **orden de tablero**.

### 5. Unidades con prioridad disminuida por habilidades

Finalmente se ubican las unidades que, por efecto de alguna habilidad, hayan disminuido su prioridad en la ronda.

Si dos o más unidades están en esta categoría:
1. se prioriza a los **viajeros** sobre las **bestias**;
2. luego se ordenan por **Speed**;
3. si persiste el empate, se usa el **orden de tablero**.

### 6. Unidades presentes en múltiples categorías

Si una misma unidad pertenece a dos o más categorías, prevalece la categoría **más prioritaria**.

Ejemplo:
- si una bestia se recupera del Breaking Point,
- y además está afectada por una habilidad que disminuye su prioridad,
- se aplica la categoría de recuperación del Breaking Point,
- por lo que debe ubicarse de las primeras en la cola.

## Observaciones de diseño

Este sistema implica que la cola de turnos debe construirse a partir de una lógica jerárquica, no solo de ordenamiento numérico.

La implementación debe contemplar:
- categorías excluyentes con prioridad jerárquica;
- desempates por múltiples criterios;
- efectos persistentes entre rondas;
- y reglas especiales para casos donde una unidad cumple más de una condición.

---

# 5. Flujo de juego

Una vez definidos los elementos y sistemas del combate, se puede describir el flujo general del programa durante una batalla.

En condiciones normales, el flujo del juego será el siguiente:

## 5.1. Inicio de ronda

Comienza una ronda y se ordenan:
- la **cola de turnos actual**;
- y la **cola de turnos siguiente**,

siguiendo las reglas de prioridad descritas anteriormente.

Esto implica que el sistema no solo debe calcular el presente inmediato, sino también anticipar el orden futuro en función de estados ya activos.

## 5.2. Resolución de turnos

Siguiendo el orden definido, cada unidad actúa cuando llega su turno, realizando alguna de las acciones disponibles según su tipo:

- los viajeros eligen entre sus acciones posibles;
- las bestias ejecutan automáticamente su habilidad.

## 5.3. Interrupción por Breaking Point

Si en cualquier momento de la ronda una bestia entra en **Breaking Point**:
- pierde su turno en la ronda actual, si todavía no había actuado;
- y también pierde su turno en la ronda siguiente.

Esto significa que el flujo del combate puede alterarse dinámicamente incluso después de que una ronda ya comenzó.

## 5.4. Fin de ronda y transición

Una vez que todas las unidades habilitadas hayan actuado o perdido su acción por estado, la ronda termina.

Al finalizar la ronda:
- se aplican los efectos de fin de ronda, como la ganancia de BP de los viajeros vivos cuando corresponda;
- se actualizan las duraciones de estados;
- se prepara la siguiente cola de turnos;
- y comienza una nueva ronda.

---

## 5.5. Resumen operativo del sistema de juego

El sistema completo puede entenderse como un ciclo repetido:

1. se inicia la ronda;
2. se ordenan las colas de turno;
3. cada unidad actúa o pierde su acción si corresponde;
4. se resuelven efectos del combate;
5. se actualizan BP, estados y prioridades;
6. comienza la siguiente ronda.

---

## 5.6. Requisitos de modelado recomendados

Para implementar correctamente este capítulo, el sistema debería contemplar como mínimo:

### Para la lógica de combate
- una estructura de rondas;
- una estructura de turnos dentro de cada ronda;
- una cola de turnos actual y una cola de turnos siguiente;
- reglas de interrupción y reordenamiento.

### Para las acciones
- selección de acción para viajeros;
- ejecución automática para bestias;
- validación de acciones habilitadas;
- resolución de efectos por tipo de acción.

### Para los ataques
- clasificación entre ataques físicos y elementales;
- tipos concretos de ataque;
- interacción con debilidades;
- posibilidad de ataques sin tipo o fuera de categorías tradicionales.

### Para BP
- BP actual por viajero;
- límite máximo acumulable;
- límite máximo gastable por acción;
- ganancia al final de ronda;
- bloqueo de ganancia si hubo boosting.

### Para Shields y Breaking Point
- cantidad actual de Shields por bestia;
- reducción por golpes efectivos a debilidades;
- ingreso al estado de Breaking Point;
- duración del estado;
- incapacidad de actuar;
- reinicio de Shields al recuperarse;
- prioridad especial al salir del estado.

### Para prioridad y cola
- prioridad por recuperación de Breaking Point;
- prioridad por Defender;
- prioridad o desprioridad por habilidades;
- desempates por Speed;
- desempates por tipo de unidad;
- desempates por orden de tablero;
- resolución de conflictos cuando una unidad pertenece a varias categorías.

---

## 5.7. Resumen

El capítulo de **Sistemas de juego** define el corazón del combate.

Su función es establecer:
- cómo se organiza una ronda;
- cómo se determina el orden de actuación;
- qué acciones puede realizar cada unidad;
- cómo funcionan los recursos de boosting;
- cómo las debilidades afectan a las bestias;
- y cómo el **Breaking Point** altera profundamente el ritmo de la batalla.

En conjunto, estas reglas convierten el combate en un sistema táctico donde no solo importa el daño, sino también el orden de turnos, la administración de recursos y la manipulación del estado del enemigo.
