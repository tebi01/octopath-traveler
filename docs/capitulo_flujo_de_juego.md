# 5. Flujo de juego

Este capítulo describe cómo progresa una batalla a lo largo de las rondas, cómo se actualiza la cola de turnos y qué eventos pueden modificar el flujo normal del combate.

A diferencia de los capítulos anteriores, aquí el foco no está en definir atributos o acciones aisladas, sino en explicar **cómo se encadenan temporalmente** dentro de una partida. El objetivo es dejar claro qué ocurre al inicio de cada ronda, qué sucede mientras las unidades actúan y qué reglas se aplican cuando el estado del combate cambia de forma dinámica.

---

## 5.1. Estructura general del flujo

En una situación normal, el combate sigue un ciclo repetitivo compuesto por rondas.

Cada ronda se desarrolla bajo la siguiente lógica general:

1. comienza una nueva ronda;
2. se ordena la **cola de turnos actual**;
3. se ordena también la **cola de turnos siguiente**;
4. cada unidad actúa cuando llega su turno;
5. durante la ronda pueden ocurrir eventos que alteren la cola futura;
6. cuando ya no quedan turnos pendientes, la ronda finaliza;
7. se aplican los efectos de cierre de ronda;
8. comienza una nueva ronda con las mismas reglas.

Esto implica que el sistema no solo debe administrar lo que ocurre en el presente inmediato, sino también mantener preparada una proyección del orden futuro de actuación.

---

## 5.2. Inicio de ronda

Al comenzar cada ronda, el sistema debe calcular dos estructuras:

- la **cola de turnos actual**, que determina qué unidades actuarán en la ronda que acaba de comenzar;
- la **cola de turnos siguiente**, que anticipa el orden tentativo de actuación para la próxima ronda.

Ambas colas deben ordenarse usando las reglas de prioridad ya definidas para el sistema de turnos.

### Qué implica esto en diseño

El combate no funciona como una simple lista improvisada turno a turno.  
En cambio, trabaja con una lógica planificada donde el sistema conoce tanto:

- quién actúa ahora;
- como quién debería actuar después, salvo que algún evento modifique ese orden.

Esto es importante porque varias acciones y estados alteran específicamente la **cola de la ronda siguiente**, no necesariamente la actual.

---

## 5.3. Desarrollo de la ronda

Una vez definida la cola, las unidades actúan siguiendo exactamente el orden establecido.

### Regla base

Cada unidad que se encuentre habilitada para actuar ejecuta su turno cuando llega su posición en la cola.

- Los **viajeros** realizan una de sus acciones disponibles.
- Las **bestias** ejecutan automáticamente su habilidad.

Cuando una unidad termina su turno:

- sale de la cola actual;
- el resto de unidades avanza una posición;
- y el combate continúa con la siguiente unidad.

### Consecuencia operativa

La cola de turnos debe comportarse como una estructura dinámica:

- el primer elemento se resuelve;
- luego se elimina de la cola;
- después continúa el siguiente.

Cuando la cola queda vacía, la ronda termina.

---

## 5.4. Alteraciones durante la ronda

El flujo del juego no es completamente rígido.  
Durante el desarrollo de una ronda pueden ocurrir situaciones que modifiquen el estado general del combate y, en algunos casos, también el orden de turnos futuro.

Las alteraciones más importantes que el sistema debe contemplar son:

- entrada de una bestia en **Breaking Point**;
- muerte de una unidad;
- acciones o habilidades que cambien la prioridad en la siguiente ronda;
- uso de **boosting**, que afecta la ganancia posterior de BP.

---

## 5.5. Entrada en Breaking Point

Si en cualquier momento de la ronda una bestia entra en **Breaking Point**, se produce una alteración inmediata del flujo.

### Efectos sobre los turnos

Cuando una bestia entra en Breaking Point:

- pierde su turno en la **ronda actual**, si todavía no había actuado;
- también pierde su turno en la **ronda siguiente**.

Esto significa que el sistema debe revisar el estado de esa unidad en relación con ambas colas:

- si la bestia seguía presente en la cola actual, debe dejar de poder actuar;
- si estaba contemplada en la cola siguiente, debe omitirse o considerarse bloqueada en esa ronda futura.

### Implicancia de diseño

El Breaking Point no es solo un estado pasivo.  
Es una mecánica que puede interrumpir el orden previsto de la batalla, incluso después de que la ronda ya haya comenzado.

Por lo tanto, el motor del combate debe ser capaz de:

- detectar el momento exacto en que una bestia entra en Breaking Point;
- comprobar si ya actuó o no;
- modificar la ronda actual si corresponde;
- y reflejar también ese cambio en la ronda siguiente.

---

## 5.6. Muerte de unidades

Si en cualquier momento de la ronda una unidad muere, esa unidad:

- permanece muerta en el tablero;
- conserva su misma posición visual;
- y no podrá volver a actuar durante el resto del juego.

### Reglas asociadas

La muerte no remueve necesariamente a la unidad del tablero como objeto visible, pero sí la elimina como participante activo del combate.

Esto implica que una unidad muerta:

- no debe recibir turnos nuevos;
- no debe poder ejecutar acciones;
- no debe poder beneficiarse de ganancias automáticas como BP, si se trata de un viajero.

### Condiciones de finalización

Existen dos condiciones terminales directas:

#### Derrota del jugador
Si mueren todos los viajeros, el juego finaliza y el jugador pierde.

#### Victoria del jugador
Si mueren todas las bestias, el juego finaliza y el jugador gana.

Estas condiciones deben evaluarse de forma inmediata cada vez que una unidad muere.

---

## 5.7. Modificación de la cola de la siguiente ronda

El documento establece una regla clave para el funcionamiento del combate:

Si durante una ronda una unidad realiza una acción que altera la cola de turnos de la siguiente ronda, esa cola debe editarse para reflejar el cambio.

### Casos típicos donde esto ocurre

- una unidad usa **Defender**;
- una habilidad aumenta prioridad;
- una habilidad disminuye prioridad;
- una bestia sale de Breaking Point y obtiene prioridad especial;
- una unidad muere y deja de formar parte de futuras colas.

### Implicancia de implementación

La cola siguiente no puede tratarse como algo fijo e inmutable una vez creada al inicio de la ronda.

Debe existir la posibilidad de:

- recalcularla por completo;
- o ajustarla parcialmente según el evento ocurrido.

Lo importante es que, al empezar la nueva ronda, la cola siguiente ya represente correctamente el nuevo estado del combate.

---

## 5.8. Fin de ronda

Se considera que una ronda ha finalizado cuando ya no quedan turnos por jugar.

Esto incluye tanto:

- unidades que efectivamente actuaron;
- como unidades que perdieron su acción por estados como Breaking Point.

Una vez alcanzado ese punto, se aplican los efectos de cierre de ronda.

### Ganancia de BP

Al final de cada ronda:

- cada viajero vivo obtiene **1 BP**;
- excepto aquellos que hayan utilizado **boosting** durante esa ronda.

### Resultado práctico

Esto introduce una fase obligatoria de actualización antes de iniciar la siguiente ronda:

- se otorgan los BP correspondientes;
- se revisan los estados activos;
- se preparan las colas futuras;
- y recién entonces se da paso a la siguiente ronda.

---

## 5.9. Orden de turnos al inicio de la partida

Al inicio del combate, el orden de los turnos se define de forma descendente según la stat **Speed**.

### Reglas de desempate

Si dos o más unidades tienen la misma Speed:

1. se prioriza a los **viajeros** sobre las **bestias**;
2. si el empate persiste, se prioriza a la unidad que esté más a la **izquierda en el tablero**.

### Ejemplo base

Supongamos una batalla con las siguientes unidades y velocidades:

- Ophilia: **291**
- Tressa: **240**
- Therion: **445**
- H’aanit: **350**
- Mutant Mushroom: **131**
- Demon Deer: **100**

Siguiendo las reglas indicadas, el orden de actuación sería:

1. Therion
2. H’aanit
3. Ophilia
4. Tressa
5. Mutant Mushroom
6. Demon Deer

Este ejemplo muestra el caso más simple del sistema: ninguna unidad está afectada por estados o habilidades que alteren prioridad, así que el orden depende únicamente de Speed y de las reglas de desempate.

---

## 5.10. Avance normal de la cola

Después de que una unidad realiza su acción, la cola avanza.

Por ejemplo, si en el caso anterior **Therion** actúa primero, deja de formar parte de la cola actual y el nuevo orden pasa a ser:

1. H’aanit
2. Ophilia
3. Tressa
4. Mutant Mushroom
5. Demon Deer

Esta lógica debe mantenerse de forma consistente durante toda la ronda:

- cada turno resuelto elimina a la unidad correspondiente de la cola actual;
- la ronda continúa hasta que no queden unidades pendientes.

---

## 5.11. Ejemplo de interrupción por Breaking Point

Supongamos ahora que **H’aanit** ataca a **Mutant Mushroom** y reduce sus Shields a 0, provocando su entrada en **Breaking Point**.

En ese caso:

- Mutant Mushroom pierde su turno de la ronda actual, si todavía no había actuado;
- también pierde su turno de la ronda siguiente.

Este ejemplo muestra una alteración importante del flujo: la bestia ya no solo cambia de estado, sino que también afecta la disponibilidad de turnos en dos rondas consecutivas.

---

## 5.12. Recuperación del Breaking Point

Después de transcurridas dos rondas —la actual y la siguiente— la bestia sale del estado de Breaking Point.

Cuando esto ocurre:

- recupera sus **Shields**;
- obtiene **prioridad en el orden de turnos**;
- y vuelve a insertarse en la lógica normal del combate.

### Prioridad al recuperarse

La recuperación del Breaking Point tiene prioridad especial dentro del sistema de ordenamiento.

Esto significa que una bestia que se recupera puede colocarse antes que otras unidades, incluso si existen otros efectos de prioridad en juego.

---

## 5.13. Interacción con BP en el flujo de rondas

El documento también ejemplifica cómo progresa el recurso de **BP** entre rondas.

### Regla general

Al final de cada ronda:

- todos los viajeros vivos reciben **1 BP**;
- salvo quienes hayan gastado boosting durante esa ronda.

### Ejemplo

Si han pasado dos rondas desde el inicio del juego, es esperable que los viajeros acumulen BP, siempre que no los hayan gastado.

Luego, si un viajero —por ejemplo Therion— usa **2 BP** en su turno para potenciar una acción, ese viajero no recibirá el BP adicional al finalizar la ronda, mientras que los demás sí lo recibirán.

### Implicancia táctica

Esto confirma que el uso de boosting no solo impacta la potencia de la acción inmediata, sino también la economía futura del personaje.

---

## 5.14. Interacción entre Defender y recuperación de Breaking Point

El flujo del juego también contempla casos donde coinciden múltiples reglas de prioridad.

### Caso ejemplo

Si en una ronda:

- **Tressa** usa la acción **Defender**;
- y al mismo tiempo **Mutant Mushroom** sale del estado de Breaking Point;

la prioridad del turno en la ronda siguiente se resuelve de la siguiente manera:

1. primero va la bestia que se recupera del Breaking Point;
2. después va el viajero que utilizó Defender;
3. luego se ordena el resto de unidades según las reglas generales.

### Conclusión de diseño

Esto confirma que las prioridades del sistema son jerárquicas y no equivalentes.

No basta con saber que dos unidades tienen “prioridad”; es necesario saber **qué tipo de prioridad** posee cada una y cuál prevalece sobre la otra.

---

## 5.15. Requisitos de modelado recomendados

Para implementar correctamente el flujo de juego, el sistema debería contemplar como mínimo los siguientes elementos:

### Estructuras principales
- una representación formal de la **ronda actual**;
- una **cola de turnos actual**;
- una **cola de turnos siguiente**;
- registro del estado de cada unidad.

### Durante la resolución
- ejecución secuencial de turnos;
- eliminación de la unidad que ya actuó de la cola actual;
- validación de si una unidad puede o no actuar;
- interrupciones por muerte o Breaking Point.

### Para cambios dinámicos
- posibilidad de editar la cola de la siguiente ronda en tiempo real;
- actualización de prioridades por acciones o habilidades;
- bloqueo de turnos futuros por Breaking Point;
- exclusión definitiva de unidades muertas.

### Para el fin de ronda
- detección de cola vacía;
- otorgamiento de BP a viajeros vivos;
- exclusión de BP para quienes usaron boosting;
- transición ordenada hacia la siguiente ronda.

### Para condiciones de término
- verificación inmediata de derrota si todos los viajeros mueren;
- verificación inmediata de victoria si todas las bestias mueren.

---

## 5.16. Resumen

El capítulo de **Flujo de juego** define cómo se encadenan temporalmente todas las reglas del combate.

Su aporte principal es explicar que el sistema no debe pensarse solo como una serie de acciones individuales, sino como un proceso continuo donde:

- las rondas se preparan por adelantado;
- los turnos se consumen dinámicamente;
- los estados pueden alterar la planificación futura;
- las muertes cambian permanentemente el combate;
- y los recursos, como el BP, se actualizan al cierre de cada ciclo.

En otras palabras, el flujo de juego es la capa que conecta:

- el sistema de turnos;
- las acciones de las unidades;
- el Breaking Point;
- la prioridad de la cola;
- y las condiciones de victoria o derrota.

Sin esta lógica de secuencia, el resto de las mecánicas no podría integrarse de forma consistente dentro del combate.
