# Super Mario Bros — Mejoras al Proyecto

Proyecto original: clon de Super Mario Bros en Unity 2021.3.  
A continuación se describen todas las mejoras y los nuevos niveles añadidos al proyecto.

---

## Nuevos Niveles y Escenas

### A. Nivel Subterráneo — `World 2-1 - Underground` (réplica del original)

**Base:** `World 1-2 - Underground.unity` (el nivel underground más completo del proyecto).  
**Modificaciones:**
- La salida de la tubería de warp ahora redirige a `World 2-2 - Aquatic` (flujo de la World 2).
- Se creó meta GUID propio para que Unity lo trate como asset independiente.

**Características del nivel (heredadas del original SMB):**
- Fondo negro / caverna con bloques de ladrillo gris.
- Techo bajo de bloques de ladrillo — espacios estrechos que requieren precisión.
- Bloques de pregunta con monedas y power-ups.
- Tubería con Piranha Plant.
- Múltiples Goombas y Koopas Green.
- Tubería de warp lateral que conecta con la salida del mundo.
- Música: pista underground del juego base.

**Archivos:**
- `Assets/Scenes/World 2-1 - Underground.unity`
- `Assets/Scenes/World 2-1 - Underground.unity.meta`

---

### B. Nivel Acuático — `World 2-2 - Aquatic` (réplica del original)

**Base:** `World 1-2 - Underground.unity` adaptado.  
**Modificaciones:**
- Ambient sky color cambiado a **azul oscuro** (`r: 0.02, g: 0.10, b: 0.30`) para simular el ambiente acuático.
- La salida regresa al menú principal (fin de la World 2).
- Script nuevo `UnderwaterPhysics.cs` para las mecánicas de nado.

**Mecánicas de nado — `UnderwaterPhysics.cs`:**

| Parámetro | Valor |
|-----------|-------|
| Gravedad bajo agua | 25 % de la normal |
| Fuerza de nado (tecla Jump) | 8 unidades hacia arriba |
| Velocidad máxima upward | 4 u/s |
| Hundimiento pasivo | -2 u/s |

- Cada toque de **Jump** aplica un impulso hacia arriba (simula las brazadas).
- Sin presionar nada, Mario se hunde lentamente.
- Al salir del trigger de agua se restauran los parámetros normales.
- El script se agrega como componente al trigger que cubre el área de agua.

**Para configurar en el Editor Unity:**
1. Crear un `Empty GameObject` llamado `Water Zone`.
2. Agregarle un `BoxCollider2D` en modo `Is Trigger`, cubriendo todo el área inundada.
3. Agregarle el script `UnderwaterPhysics`.

**Archivos:**
- `Assets/Scenes/World 2-2 - Aquatic.unity`
- `Assets/Scenes/World 2-2 - Aquatic.unity.meta`
- `Assets/Scripts/UnderwaterPhysics.cs`

---

### C. Escena de Game Over — `Game Over Screen` (mejorada)

**Script:** `GameOverScreen.cs` — reescrito completamente.

**Nuevas funcionalidades:**

| Función | Descripción |
|---------|-------------|
| Puntuación final | Muestra el score del jugador al perder |
| Mejor puntuación | Muestra el all-time high score guardado |
| "NEW RECORD!" | Aparece si el jugador superó el high score anterior |
| TIME UP → GAME OVER | Transición animada de 1.5 s si murió por tiempo |
| Skip con Pause | El jugador puede presionar Pause/Start para volver al menú |
| Tiempo en pantalla | 5 s mínimo (o lo que dure la música del Game Over) |

**Referencias opcionales en el Inspector:**
- `FinalScoreLabel` / `FinalScoreValue` — panel con el score
- `HighScoreLabel` / `HighScoreValue` — panel con el best
- `NewRecordText` — texto "NEW RECORD!" (se activa/desactiva automáticamente)

---

### D. Castillo de Bowser — `World 2-4 - Bowser Castle` (diseño original)

**Base:** `World 1-4.unity` — el castillo con Bowser ya implementado.  
**Modificaciones de escena:**
- Ambient sky color cambiado a **rojo oscuro** (`r: 0.08, g: 0.02, b: 0.02`) — atmósfera de lava y fuego.
- GUID de escena independiente.

**Script nuevo — `LavaZone.cs`:**
- Attach a cualquier `BoxCollider2D` con `Is Trigger` sobre un área de lava.
- Al contacto con Mario → `MarioRespawn()` después de 0.1 s (breve delay visual).
- Diferente al Kill Plane: permite mostrar a Mario "hundiéndose" en la lava antes de morir.

**Diseño del nivel (propuesta original):**
El World 2-4 mantiene la arquitectura del 1-4 como base, pero se distingue por:

1. **Atmósfera** — iluminación ambiental roja que simula el resplandor de la lava.
2. **Peligros adicionales** — Colocar `LavaZone.cs` sobre los estanques de lava grises ya existentes en la escena base.
3. **Progresión** — Mismo flujo: plataformas → firebars → puente sobre lava → Bowser → hacha → victoria.
4. **Boss fight** — Bowser del World 1-4 ya tiene 5 hit points, salto aleatorio y proyectiles; se reutiliza íntegramente.
5. **Para personalizar en el editor:**
   - Agregar más Firebars en posiciones distintas a las del 1-4.
   - Reemplazar el suelo de algunos tramos con prefabs de Lava + `LavaZone.cs`.
   - Usar `Grey Brick Ground` en lugar de Brown para diferenciarlo visualmente del 1-4 original.

**Archivos:**
- `Assets/Scenes/World 2-4 - Bowser Castle.unity`
- `Assets/Scenes/World 2-4 - Bowser Castle.unity.meta`
- `Assets/Scripts/LavaZone.cs`

---

### E. Flujo de la World 2 en el menú

Se agregaron tres métodos nuevos en `MainMenu.cs`:

```csharp
public void StartWorld2_Underground()   // → World 2-1 - Underground
public void StartWorld2_Aquatic()       // → World 2-2 - Aquatic
public void StartWorld2_BowserCastle()  // → World 2-4 - Bowser Castle
```

Para habilitarlos desde la escena del Main Menu, agregar tres botones nuevos en el canvas y vincularlos a estos métodos (igual que los botones de World 1 ya existentes).

---

### F. Build Settings actualizado

`ProjectSettings/EditorBuildSettings.asset` — se agregaron las tres nuevas escenas:

```
World 2-1 - Underground   (guid: a1b2c3d4e5f6...)
World 2-2 - Aquatic        (guid: b2c3d4e5f6a1...)
World 2-4 - Bowser Castle  (guid: c3d4e5f6a1b2...)
```

---

---

## 1. Coyote Time (`Mario.cs`)

**Qué es:** Ventana de gracia de **0.1 segundos** después de que Mario camina al borde de una plataforma durante la cual todavía puede saltar.

**Por qué importa:** Sin coyote time, si el jugador presiona saltar una fracción de segundo tarde (cuando Mario ya salió del borde), el salto no se registra aunque visualmente aún esté "en el piso". Esto hace que el control se sienta injusto. Con coyote time, los juegos de plataformas modernos compensan ese desfase y la jugabilidad se siente mucho más responsive.

**Cómo funciona:**
```csharp
private const float CoyoteTime = 0.1f;
private float coyoteTimer;

// En Update(): se recarga mientras está en el suelo, cuenta regresiva al estar en el aire
if (isGrounded) {
    coyoteTimer = CoyoteTime;
} else {
    coyoteTimer -= Time.deltaTime;
}

// En FixedUpdate(): el salto se permite si está en suelo O si el timer aún es positivo
bool coyoteJumpAllowed = !isGrounded && coyoteTimer > 0;
if ((isGrounded || coyoteJumpAllowed) && jumpButtonHeld && jumpButtonReleased) {
    coyoteTimer = 0; // consumir el window de inmediato
    // ... lógica de salto normal
}
```

---

## 2. Sistema de Combo de Stomps (`LevelManager.cs` + `Mario.cs`)

**Qué es:** Pisotear enemigos consecutivamente sin tocar el suelo otorga puntos crecientes, replicando el comportamiento original del NES.

**Tabla de puntos:**

| Stomps consecutivos | Puntos |
|---------------------|--------|
| 1° | 100 |
| 2° | 200 |
| 3° | 400 |
| 4° | 800 |
| 5° | 1.000 |
| 6° | 2.000 |
| 7° | 4.000 |
| 8° | 8.000 |
| 9° en adelante | **1-UP** (vida extra) |

El combo se **reinicia automáticamente** cuando Mario aterriza en el suelo.

**Implementación:**
```csharp
// LevelManager.cs
private int stompCombo = 0;
private static readonly int[] StompComboBonuses = { 100, 200, 400, 800, 1000, 2000, 4000, 8000 };

public void MarioStompEnemy(Enemy enemy) {
    // ...bounce y sonido...
    if (stompCombo >= StompComboBonuses.Length) {
        AddLife(enemy.gameObject.transform.position); // 1-UP en combos largos
    } else {
        AddScore(StompComboBonuses[stompCombo], enemy.gameObject.transform.position);
    }
    stompCombo++;
}

public void ResetStompCombo() { stompCombo = 0; }

// Mario.cs — llama ResetStompCombo al aterrizar
if (isGrounded && !wasGrounded) {
    t_LevelManager.ResetStompCombo();
}
```

---

## 3. Constantes de Física (`Mario.cs`)

**Antes:** Los valores de física estaban hardcodeados como variables privadas con nombres genéricos, sin distinción visual entre constantes y estado mutable.

**Después:** Convertidos a `private const` con nombres descriptivos en PascalCase (convención C# para constantes):

```csharp
private const float MinWalkSpeedX       = 0.28f;
private const float WalkAccelerationX   = 0.14f;
private const float RunAccelerationX    = 0.21f;
private const float ReleaseDecelerationX = 0.25f;
private const float SkidDecelerationX   = 0.5f;
private const float SkidTurnaroundSpeedX = 3.5f;
private const float MaxWalkSpeedX       = 5.86f;
private const float MaxRunSpeedX        = 9.61f;
private const float WaitBetweenFire     = 0.2f;
```

Beneficios: el compilador puede optimizarlas, imposible modificarlas por accidente, y el código self-documenta qué valores son parámetros de diseño vs. estado de ejecución.

---

## 4. Eliminación de `FindObjectOfType` Duplicado (`Mario.cs`)

**Problema:** En `Start()`, `t_LevelManager` se cacheaba correctamente pero luego `FindSpawnPosition()` era llamado con una segunda búsqueda redundante:

```csharp
// ANTES — dos FindObjectOfType<LevelManager>() en el mismo Start()
t_LevelManager = FindObjectOfType<LevelManager>();
transform.position = FindObjectOfType<LevelManager>().FindSpawnPosition(); // duplicado
```

**Después:** Usa la referencia ya cacheada:
```csharp
t_LevelManager = FindObjectOfType<LevelManager>();
transform.position = t_LevelManager.FindSpawnPosition(); // usa cache
```

Igual corrección en `UpdateSize()`, que también llamaba `FindObjectOfType` innecesariamente.

---

## 5. Cache Temprano del `GameStateManager` en `Awake()` (`LevelManager.cs`)

**Problema:** `t_GameStateManager` se cacheaba en `Start()`, pero `FindSpawnPosition()` podía ser llamado desde `Start()` de otros scripts (como `Mario.cs` o `MainCamera.cs`) antes de que `LevelManager.Start()` hubiera corrido, causando una búsqueda redundante con variable local.

**Solución:** Mover el cache a `Awake()`, que en Unity siempre corre antes que cualquier `Start()`:

```csharp
void Awake() {
    Time.timeScale = 1;
    t_GameStateManager = FindObjectOfType<GameStateManager>(); // cache temprano
}
```

`FindSpawnPosition()` ahora usa el campo de clase con un fallback defensivo:
```csharp
GameStateManager gsm = t_GameStateManager != null
    ? t_GameStateManager
    : FindObjectOfType<GameStateManager>();
```

---

## 6. Refactorización de `AddCoin` y `AddLife` (`LevelManager.cs`)

**Problema:** Los dos overloads de `AddCoin()` tenían lógica duplicada (incrementar contador, reproducir sonido, verificar 100 monedas, actualizar HUD).

**Antes:**
```csharp
public void AddCoin() {
    coins++;
    soundSource.PlayOneShot(coinSound);
    if (coins == 100) { AddLife(); coins = 0; }
    SetHudCoin();
    AddScore(coinBonus);
}

public void AddCoin(Vector3 spawnPos) {
    coins++;                               // duplicado
    soundSource.PlayOneShot(coinSound);    // duplicado
    if (coins == 100) { AddLife(); coins = 0; } // duplicado
    SetHudCoin();                          // duplicado
    AddScore(coinBonus, spawnPos);
}
```

**Después:** Se extrae la lógica común a un método privado:
```csharp
private void IncrementCoin() {
    coins++;
    soundSource.PlayOneShot(coinSound);
    if (coins >= 100) { AddLife(); coins = 0; }
    SetHudCoin();
}

public void AddCoin()                  { IncrementCoin(); AddScore(coinBonus); }
public void AddCoin(Vector3 spawnPos)  { IncrementCoin(); AddScore(coinBonus, spawnPos); }
```

También se simplificó `AddLife(Vector3)` para delegar en `AddLife()`:
```csharp
public void AddLife(Vector3 spawnPos) {
    AddLife();
    CreateFloatingText("1UP", spawnPos);
}
```

---

## 7. Actualización Continua del High Score (`GameStateManager.cs`)

**Problema:** El high score solo se guardaba al llegar a la pantalla de Game Over. Si el jugador cerraba el juego entre niveles, se perdía.

**Solución:** Se agrega un método centralizado `UpdateHighScore()` que es llamado tanto desde `SaveGameState()` (entre niveles) como desde `LoadGameOver()`:

```csharp
public void UpdateHighScore(int currentScore) {
    if (currentScore > PlayerPrefs.GetInt("highScore", 0)) {
        PlayerPrefs.SetInt("highScore", currentScore);
    }
}

public void SaveGameState() {
    // ... guardar estado normal ...
    UpdateHighScore(scores); // siempre actualizado al cambiar de nivel
}
```

---

## 8. Cache de Componentes `Slider` en `MainMenu.cs`

**Antes:** Cada llamada a `SetVolume()` o `CancelSelectVolume()` hacía `GetComponent<Slider>()` en caliente.

**Después:** Los componentes se cachean en `Start()`:
```csharp
private Slider soundSliderComponent;
private Slider musicSliderComponent;

void Start() {
    soundSliderComponent = SoundSlider.GetComponent<Slider>();
    musicSliderComponent = MusicSlider.GetComponent<Slider>();
    // ... resto de Start ...
}
```

---

## 9. Corrección del Orden de `isFalling` en `Update()` (`Mario.cs`)

**Bug sutil:** En el código original, `isFalling` se calculaba *antes* de actualizar `isGrounded`, lo que significaba que usaba el valor de `isGrounded` del frame anterior:

```csharp
// ANTES — isFalling usa isGrounded del frame anterior
isFalling = m_Rigidbody2D.velocity.y < 0 && !isGrounded;
isGrounded = Physics2D.OverlapPoint(...); // actualiza DESPUÉS
```

**Después:** Se actualiza `isGrounded` primero:
```csharp
wasGrounded = isGrounded;
isGrounded = Physics2D.OverlapPoint(...); // actualiza PRIMERO
isFalling = m_Rigidbody2D.velocity.y < 0 && !isGrounded; // usa valor actual
```

---

## 10. Eliminación de `Debug.Log` Excesivos

Se eliminaron más de **30 llamadas a `Debug.Log`** distribuidas en:

| Archivo | Logs eliminados |
|---------|-----------------|
| `Mario.cs` | 6 |
| `LevelManager.cs` | 18 |
| `Goomba.cs` | 1 |
| `Koopa.cs` | 2 |
| `MarioStompBox.cs` | 2 |

Los logs de debug saturan la consola de Unity durante el desarrollo y tienen un pequeño costo de rendimiento en builds de desarrollo. En su lugar, los comentarios en el código explican el comportamiento no obvio donde es necesario.

---

## 11. Eliminación de `Update()` Vacío (`MarioStompBox.cs`)

Unity invoca `Update()` en cada MonoBehaviour cada frame. Un método `Update()` vacío tiene un overhead medible cuando hay muchos objetos en escena.

```csharp
// ANTES — llamado cada frame sin hacer nada
void Update () { }
```

Simplemente se eliminó el método.

---

## 12. Simplificación de Expresiones Condicionales (`LevelManager.cs`)

Varias expresiones ternarias y bloques `if/else` verbose se simplificaron:

```csharp
// ANTES
if (hurryUp) {
    ChangeMusic(levelMusicHurry);
} else {
    ChangeMusic(levelMusic);
}

// DESPUÉS
ChangeMusic(hurryUp ? levelMusicHurry : levelMusic);
```

```csharp
// ANTES
if (timeup) {
    LoadSceneDelay("Time Up Screen", delay);
} else {
    LoadSceneDelay("Level Start Screen", delay);
}

// DESPUÉS
LoadSceneDelay(timeup ? "Time Up Screen" : "Level Start Screen", delay);
```

---

## Resumen de Archivos Modificados

| Archivo | Tipo de mejora |
|---------|----------------|
| `Mario.cs` | Gameplay, rendimiento, calidad de código |
| `LevelManager.cs` | Gameplay, rendimiento, calidad de código |
| `GameStateManager.cs` | Funcionalidad, calidad de código |
| `MainMenu.cs` | Rendimiento, calidad de código |
| `Goomba.cs` | Calidad de código |
| `Koopa.cs` | Calidad de código |
| `MarioStompBox.cs` | Rendimiento, calidad de código |

---

## Tecnologías

- **Motor:** Unity 2021.3.24f1
- **Lenguaje:** C#
- **Física:** Unity 2D Physics (Rigidbody2D)
- **UI:** Unity UGUI + TextMesh Pro
