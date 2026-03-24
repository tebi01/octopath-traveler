# 3. Unidades

Antes de explicar el flujo del juego, es necesario definir qué es una **unidad** y cuáles son sus atributos.  
En el sistema existen dos grandes tipos de unidades:

- **Viajeros**
- **Bestias**

Ambos comparten ciertos atributos básicos, pero también presentan diferencias importantes que afectan directamente el combate, las decisiones del jugador y la lógica del juego. Estas diferencias deben quedar claramente representadas tanto a nivel de diseño como de implementación.

## Consideraciones generales

- Si el jugador posee **menos de 4 viajeros**, algunos espacios del tablero del equipo jugador quedarán vacíos.
- Si el enemigo posee **menos de 5 bestias**, algunos espacios del tablero enemigo también quedarán vacíos.

Esto implica que el sistema debe contemplar formaciones incompletas y manejar correctamente espacios desocupados en combate.

---

## 3.1. Tipos de unidades

### Viajeros

Los **viajeros** son las unidades controladas por el jugador.  
Se caracterizan por disponer de:

- **habilidades activas**, que pueden ejecutarse durante el combate;
- **habilidades pasivas o de apoyo**, que generan efectos permanentes o automáticos.

Los viajeros están diseñados para ofrecer flexibilidad táctica, ya que pueden atacar, consumir recursos, aplicar efectos y adaptarse a distintos roles dentro del equipo.

#### Atributos de un viajero

##### Nombre
Identificador único de la unidad.

Debe servir para:
- distinguir al viajero de cualquier otra unidad;
- referenciarlo dentro del sistema;
- mostrarlo en interfaz, menús, selección de equipo y combate.

##### Stats
Conjunto de valores numéricos que definen el rendimiento del viajero en combate.

Estos stats determinan, entre otras cosas:
- cuánto daño puede causar;
- cuánto daño puede resistir;
- el orden de actuación en los turnos;
- la cantidad de recursos disponibles para usar habilidades.

##### Armas
Lista de armas que el viajero puede utilizar para realizar un **ataque básico**.

Esto implica que:
- un viajero puede tener acceso a uno o varios tipos de arma;
- el ataque básico no depende de una habilidad activa;
- las armas disponibles influyen en interacciones del combate, como afinidades o debilidades enemigas, cuando corresponda.

##### Habilidades activas
Conjunto de habilidades que el viajero puede usar de manera directa durante su turno.

Estas habilidades pueden servir para:
- atacar;
- aplicar efectos especiales;
- alterar el estado del combate;
- ejecutar acciones con costo asociado.

Normalmente, su uso está ligado al consumo de recursos, especialmente **SP**.

##### Habilidades pasivas
Conjunto de habilidades que actúan de forma permanente o automática, sin necesidad de ser activadas manualmente por el jugador.

Pueden generar efectos sobre:
- el propio viajero;
- aliados;
- enemigos;
- condiciones generales del combate.

Su principal diferencia con las habilidades activas es que no requieren una ejecución explícita durante el turno.

---

### Bestias

Las **bestias** son las unidades enemigas.  
A diferencia de los viajeros, tienen una estructura más simple, pero con mecánicas propias que las vuelven clave dentro del sistema de combate.

Se caracterizan por tener:
- **una única habilidad de combate**;
- **debilidades**;
- **shields**.

Estas propiedades permiten construir un flujo donde el jugador identifica vulnerabilidades del enemigo y busca romper su defensa para obtener ventaja táctica.

#### Atributos de una bestia

##### Nombre
Identificador único de la bestia.

Cumple la misma función estructural que en los viajeros:
- identificación interna;
- referencia en combate;
- visualización en interfaz.

##### Stats
Conjunto de valores numéricos que determinan el desempeño de la bestia en combate.

Estos stats definen:
- su resistencia;
- su capacidad ofensiva;
- su velocidad de acción.

##### Habilidad
Cada bestia posee una única habilidad que utilizará durante el combate.

Esta habilidad puede servir para:
- atacar;
- generar efectos especiales;
- representar el comportamiento ofensivo principal de la bestia.

A diferencia de los viajeros, las bestias no se presentan como unidades de múltiples opciones tácticas, sino como enemigos con un patrón más acotado.

##### Debilidades
Las debilidades indican si la bestia es vulnerable a ciertos tipos de ataque.

Cuando una bestia recibe un ataque al que es débil:
- su defensa frente a ese tipo de acción se ve comprometida;
- puede recibir daño extra;
- se habilita interacción con el sistema de **shields**.

Este atributo es central para la estrategia del jugador.

##### Shields
Valor numérico que representa cuántas veces una bestia puede recibir ataques de un tipo al que es débil antes de entrar en un estado especial.

Cuando este valor llega a **0**, la bestia entra en **Breaking Point**, un estado de debilidad que será desarrollado en otra sección del sistema.

---

# 3.2. Stats

Los **stats** son uno de los componentes más importantes del sistema, ya que determinan el resultado de múltiples acciones dentro del combate.

## Propiedades generales de los stats

- Son **propios de cada unidad**.
- No todas las unidades comparten exactamente los mismos stats.
- Existen diferencias estructurales entre los stats de viajeros y de bestias.
- Algunos stats representan valores máximos.
- Otros representan el estado actual de una unidad durante el combate.

---

## 3.2.1. Stats de viajeros

Los viajeros poseen los siguientes atributos numéricos:

### HP máximo
Cantidad máxima de vida que puede tener la unidad.

Además:
- representa la vida con la que inicia el combate;
- funciona como límite superior del HP actual.

### HP actual
Cantidad de vida que la unidad posee en el momento presente.

Reglas:
- debe estar entre **0** y **HP máximo**;
- si llega a **0**, la unidad muere;
- en términos prácticos, suele llamarse simplemente **HP**.

### SP máximo
Cantidad máxima de **Skill Points** que puede tener la unidad.

Este valor limita el recurso que el viajero puede almacenar para usar habilidades activas.

### SP actual
Cantidad actual de **Skill Points** disponibles.

Reglas:
- debe estar entre **0** y **SP máximo**;
- se consume para utilizar habilidades activas;
- en términos prácticos, suele llamarse simplemente **SP**.

### Phys Atk
Determina la potencia de los ataques físicos del viajero.

Este stat debe influir en:
- ataques básicos físicos;
- habilidades de daño físico;
- cualquier fórmula vinculada a ofensiva física.

### Phys Def
Determina la resistencia de la unidad frente a golpes físicos.

Este valor debe afectar:
- reducción de daño físico recibido;
- interacción con ataques o habilidades físicas enemigas.

### Elem Atk
Determina la potencia de los ataques elementales del viajero.

Debe influir en:
- habilidades mágicas o elementales;
- acciones cuyo daño escale con poder elemental.

### Elem Def
Determina la resistencia de la unidad frente a ataques elementales.

Este valor afecta:
- el daño recibido por habilidades elementales;
- la capacidad de supervivencia frente a enemigos con daño mágico o elemental.

### Speed
Determina la prioridad de la unidad durante los turnos.

Debe utilizarse para:
- calcular el orden de acción;
- resolver qué unidad actúa antes cuando varias están disponibles.

---

## 3.2.2. Stats de bestias

Las bestias comparten gran parte de los stats de los viajeros, pero presentan una diferencia fundamental: **no poseen SP**.

### HP máximo
Cantidad máxima de vida de la bestia.

También representa:
- la vida con la que comienza el combate.

### HP actual
Cantidad de vida actual de la bestia.

Reglas:
- debe estar entre **0** y **HP máximo**;
- si llega a **0**, la bestia muere;
- normalmente se lo denomina simplemente **HP**.

### Phys Atk
Determina la potencia de sus ataques físicos.

### Phys Def
Determina qué tan resistente es frente a ataques físicos.

### Elem Atk
Determina la potencia de sus ataques elementales.

### Elem Def
Determina qué tan resistente es frente a ataques elementales.

### Speed
Determina la prioridad de la bestia en el orden de turnos.

---

# 3.3. Diferencias clave entre viajeros y bestias

## Viajeros
- Son controlados por el jugador.
- Tienen **habilidades activas** y **pasivas**.
- Poseen **armas** para ataques básicos.
- Tienen **SP máximo** y **SP actual**.
- Están pensados para toma de decisiones tácticas.

## Bestias
- Son controladas automáticamente por el sistema.
- Poseen una sola habilidad principal.
- No tienen SP.
- Incorporan mecánicas exclusivas de **debilidades** y **shields**.
- Están diseñadas alrededor del sistema de ruptura o **Breaking Point**.

---

# 3.4. Requisitos de modelado recomendados

A partir de esta descripción, una implementación del sistema de unidades debería contemplar como mínimo:

## Para toda unidad
- nombre;
- stats base;
- HP actual;
- velocidad;
- estado de vida o derrota.

## Para viajeros
- lista de armas;
- lista de habilidades activas;
- lista de habilidades pasivas;
- SP máximo;
- SP actual.

## Para bestias
- habilidad única;
- lista o conjunto de debilidades;
- cantidad actual de shields;
- lógica para entrar en estado de **Breaking Point** al llegar a 0 shields.

---

# 3.5. Resumen

El capítulo de unidades define la base estructural del sistema de combate.  
Toda unidad posee atributos que condicionan su comportamiento, pero viajeros y bestias cumplen funciones distintas:

- los **viajeros** están orientados a la toma de decisiones del jugador, el uso de recursos y la variedad táctica;
- las **bestias** están orientadas a la presión enemiga y a la explotación estratégica de debilidades.

Los **stats** funcionan como la capa numérica que sostiene estas diferencias y permiten resolver daño, resistencia, recursos y prioridad de turnos.
