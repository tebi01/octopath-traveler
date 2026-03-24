# 6. Combate

En este capítulo se detallan los aspectos más específicos del combate, especialmente el cálculo del daño, la interacción con debilidades y **Breaking Point**, y las reglas de manejo de decimales.

El objetivo de esta sección es dejar definido cómo convertir los stats y los modificadores del sistema en una cantidad concreta de daño que luego será restada al HP del objetivo.

---

## 6.1. Cálculo del daño

El daño que realiza una unidad depende de dos elementos principales:

- las **stats** de atacante y defensor;
- el **tipo de ataque** que se esté ejecutando.

El resultado del cálculo es un número que se resta al **HP actual** de la unidad atacada. fileciteturn5file0

### Fórmula base del daño

El documento define el daño base con la siguiente estructura conceptual:

**Daño = [stat ofensiva] × [Modificador] ÷ [stat defensiva]**

Donde:

- **[stat ofensiva]** corresponde a la stat ofensiva relevante del atacante;
- **[Modificador]** representa la potencia del ataque utilizado;
- **[stat defensiva]** corresponde a la stat defensiva relevante del objetivo. fileciteturn5file0

### Selección de stats según el tipo de ataque

La fórmula no siempre usa las mismas stats.  
Depende del tipo de ataque que se esté resolviendo:

#### Ataque físico
Debe calcularse usando:

- **Phys Atk** del atacante;
- **Phys Def** del defensor. fileciteturn5file0

#### Ataque elemental
Debe calcularse usando:

- **Elem Atk** del atacante;
- **Elem Def** del defensor. fileciteturn5file0

### Modificador del ataque

La variable **[Modificador]** toma el valor que corresponda según la potencia de la acción utilizada.

Regla explícita del documento:

- los **ataques básicos** tienen modificador **1.3**;
- cada **habilidad** tiene su propio modificador. fileciteturn5file0

Esto implica que el sistema debe permitir que cada habilidad defina internamente su potencia base de forma independiente.

### Ejemplo de daño base

El documento presenta el siguiente caso:

#### Unidad atacante: Tressa
- Max HP: **275**
- Max SP: **50**
- Phys Atk: **88**
- Phys Def: **80**
- Elem Atk: **88**
- Elem Def: **80**
- Speed: **72**

#### Unidad objetivo: Meep
- Max HP: **212**
- Phys Atk: **106**
- Phys Def: **20**
- Elem Atk: **106**
- Elem Def: **17**
- Speed: **75** fileciteturn5file0

Si Tressa ataca a Meep con un **ataque básico de tipo Spear**, el cálculo queda así:

- stat ofensiva: **Phys Atk = 88**
- modificador: **1.3**
- stat defensiva: **Phys Def = 20**

Resultado intermedio:

**88 × 1.3 ÷ 20 = 94.4**

Luego, aplicando la regla de truncamiento:

**94.4 → 94**

Por lo tanto:

- Tressa inflige **94 de daño** a Meep;
- Meep pasa de **212 HP** a **118 HP**. fileciteturn5file0

### Implicancias de diseño

Este sistema requiere, como mínimo:

- identificar automáticamente si un ataque es físico o elemental;
- seleccionar la pareja correcta de stats;
- aplicar el modificador correspondiente;
- devolver un daño base antes de considerar debilidades o estados.  

---

## 6.2. Daño con debilidades y Breaking Point

Al daño base calculado en la sección anterior se le pueden aplicar multiplicadores adicionales si el objetivo:

- es **débil** al tipo de ataque recibido;
- se encuentra en **Breaking Point**;
- o ambas cosas al mismo tiempo. fileciteturn5file0

### Bonificación por debilidad

Si el objetivo recibe un ataque de un tipo al que es débil:

- recibe **50 % de daño adicional**. fileciteturn5file0

En términos prácticos:

**Daño final = Daño base × 1.5**

### Bonificación por Breaking Point

Si el objetivo está en estado de **Breaking Point**:

- recibe **50 % de daño adicional** de cualquier ataque. fileciteturn5file0

En términos prácticos:

**Daño final = Daño base × 1.5**

### Acumulación de modificadores

Ambos modificadores son **acumulables**.  
Eso significa que si una unidad:

- está en **Breaking Point**;
- y además recibe un ataque correspondiente a una de sus **debilidades**;

entonces el daño final se multiplica por **2** respecto del daño base. fileciteturn5file0

### Tabla de comportamiento

#### Estado normal
- **Ataque normal** → daño final = **daño base**
- **Ataque con debilidad** → daño final = **daño base × 1.5**

#### En Breaking Point
- **Ataque normal** → daño final = **daño base × 1.5**
- **Ataque con debilidad** → daño final = **daño base × 2** fileciteturn5file0

### Ejemplo con debilidad

El documento reutiliza el caso de Tressa y Meep, pero ahora Tressa ataca con un **ataque básico de tipo Bow**, tipo al que Meep es débil. fileciteturn5file0

Primero se calcula el daño base:

**88 × 1.3 ÷ 20 = 94.4**

Luego se aplica el multiplicador por debilidad:

**94.4 × 1.5 = 141.6**

Finalmente, se trunca:

**141.6 → 141**

Por lo tanto:

- Tressa inflige **141 de daño**;
- Meep pasa de **212 HP** a **71 HP**. fileciteturn5file0

### Regla del daño mínimo

El documento establece una restricción adicional muy importante:

- el daño realizado siempre debe ser **mayor o igual a 0**;
- si un cálculo da un valor menor a 0, el daño aplicado debe ser **0**. fileciteturn5file0

Esto implica que la implementación debe clavar el resultado mínimo en cero antes de aplicarlo al HP del objetivo.

---

## 6.3. Manejo de decimales

Para facilitar el testeo automático del juego, el sistema evita trabajar con números decimales como resultado final del daño. fileciteturn5file0

### Regla general

Cuando un cálculo produce decimales:

- el valor final debe **truncarse hacia abajo**;
- es decir, debe tomarse el entero más cercano por debajo. fileciteturn5file0

### Momento del truncamiento

La regla más importante de esta sección es que **no se truncan pasos intermedios**.  
Lo que se trunca es:

- el **resultado final del cálculo del daño**,
- después de haber aplicado **todos los modificadores correspondientes**. fileciteturn5file0

### Ejemplo conceptual

#### Ataque básico en estado normal
Si un personaje ataca con:

- **X** como stat ofensiva,
- **M** como modificador del ataque,
- **Y** como stat defensiva,

entonces el valor a truncar será el resultado final de:

**X × M ÷ Y** fileciteturn5file0

#### Ataque con debilidad
Si además existe un multiplicador extra **R** por debilidad o ruptura, entonces el valor a truncar será:

**(X × M ÷ Y) × R** fileciteturn5file0

### Implicancia de implementación

La lógica correcta debería ser:

1. calcular daño base;
2. aplicar multiplicadores por debilidad y/o Breaking Point;
3. asegurar que el daño no sea menor a 0;
4. truncar el resultado final;
5. restar ese daño al HP del objetivo.

---

# 7. Habilidades

El documento introduce aquí el sistema de habilidades del juego y comienza describiendo sus tipos generales, sus posibles objetivos y la noción de efectos. fileciteturn5file0

## 7.1. Tipos de habilidades

En el sistema existen tres grupos funcionales de habilidades:

- **habilidades activas**;
- **habilidades pasivas**;
- **habilidades de bestias**. fileciteturn5file0

### Habilidades activas

Son acciones que se ejecutan durante el turno del viajero.

Esto implica que:

- consumen la acción del turno;
- forman parte de la toma de decisiones directa del jugador;
- se resuelven de forma explícita al ser elegidas. fileciteturn5file0

### Habilidades pasivas

Se activan automáticamente, sin intervención directa del jugador, siempre que se cumplan ciertas condiciones. fileciteturn5file0

Esto implica que el sistema debe soportar:

- activación por condición;
- evaluación automática;
- aplicación de efectos sin selección manual en ese momento.

### Habilidades de bestias

Funcionan de manera similar a las habilidades activas, pero son utilizadas por bestias dentro de su lógica automática de combate. fileciteturn5file0

### Estructura conceptual común

Independientemente del tipo, el documento indica que toda habilidad puede describirse como:

- un **conjunto de efectos**;
- aplicado sobre un **objetivo**. fileciteturn5file0

Esto es importante porque sugiere un modelo flexible donde una habilidad no necesita limitarse a un único efecto.

---

## 7.2. Objetivos

El documento define **seis formas de seleccionar objetivos** para una habilidad. fileciteturn5file0

### Single
Selecciona un único personaje entre los enemigos.

En el caso de las bestias:

- selecciona un único personaje entre los viajeros. fileciteturn5file0

### Enemies
Afecta a todos los personajes enemigos.

En el caso de las bestias:

- afecta a todos los viajeros. fileciteturn5file0

### User
Afecta al personaje que utilizó la habilidad. fileciteturn5file0

### Ally
Selecciona un personaje entre los aliados, incluyendo la posibilidad de seleccionar a la misma unidad que usó la habilidad. fileciteturn5file0

Regla especial:

- estas habilidades solo pueden seleccionarse sobre unidades vivas;
- la única excepción son las habilidades cuyo efecto sea **revivir**. fileciteturn5file0

### Party
Afecta a todos los personajes aliados, incluyendo al personaje que utilizó la habilidad. fileciteturn5file0

### Any
Puede seleccionar:

- un aliado;
- a sí mismo;
- o a una bestia. fileciteturn5file0

### Implicancias de diseño

Este sistema de objetivos exige que el motor de habilidades pueda distinguir al menos:

- objetivo único enemigo;
- todos los enemigos;
- uno mismo;
- un aliado individual;
- todo el grupo aliado;
- selección libre entre varias categorías válidas.

---

## 7.3. Efectos

El documento alcanza a introducir esta sección, pero en el fragmento disponible solo se establece la base general. fileciteturn5file0

### Regla general

Toda habilidad tiene al menos un efecto. fileciteturn5file0

Además:

- en habilidades activas y de bestias, esos efectos se producen al usar la habilidad como acción del turno;
- en habilidades pasivas, esos efectos se activan cuando se cumplen las condiciones correspondientes. fileciteturn5file0

### Estado del capítulo en este archivo

En el contenido disponible del PDF, la sección de **categorías de efectos** queda solamente introducida y no alcanza a desarrollarse por completo. fileciteturn5file0

---

## 7.4. Requisitos de modelado recomendados

A partir de este capítulo, una implementación debería contemplar como mínimo:

### Para daño
- identificación del tipo de ataque;
- selección correcta de stat ofensiva y defensiva;
- modificador propio por ataque o habilidad;
- multiplicadores por debilidad y Breaking Point;
- daño mínimo de 0;
- truncamiento final hacia abajo.

### Para habilidades
- separación entre activas, pasivas y de bestias;
- definición explícita del objetivo;
- soporte para múltiples efectos por habilidad;
- evaluación automática de condiciones en pasivas;
- validación de objetivos vivos o muertos según corresponda.

---

## 7.5. Resumen

El capítulo de **Combate** define cómo se transforma una acción ofensiva en daño concreto sobre el HP del objetivo.

Su aporte principal es establecer:

- una fórmula base dependiente de stats ofensivas y defensivas;
- modificadores por potencia del ataque;
- bonificaciones por debilidad y Breaking Point;
- y una regla estricta de truncamiento para evitar ambigüedades en los cálculos. fileciteturn5file0

A su vez, el comienzo del capítulo de **Habilidades** introduce una estructura general donde toda habilidad se compone de:

- un objetivo;
- uno o más efectos;
- y una lógica de activación según su tipo. fileciteturn5file0
