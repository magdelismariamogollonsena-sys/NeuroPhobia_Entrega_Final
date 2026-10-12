# Revisión de Nivel2 — 2026-10-11

## Estado

**Pendiente de guardar la escena.** `Assets/Scenes/Nivel2.unity` sigue conteniendo únicamente la luz, el prefab XR Origin y el simulador XR inactivo. El parque descrito en `INSTRUCCIONES.txt` todavía no está serializado en esa escena. No se debe considerar Nivel2 implementado ni probarlo como si lo estuviera.

Se inició una sola apertura de Unity 6000.3.7f1 para ensamblar Nivel2. Unity encontró `Library` sin base de datos de assets, compiló C# sin errores y comenzó una importación extensa. Se detuvo antes de que guardara la escena para respetar la restricción de no ejecutar ciclos largos de batchmode. No hubo build, commit ni push.

## Scripts reutilizados

- `NeuroPhobiaLevel2StableGaze`: mirada continua de 30 s, ángulo de 12°, gracia de 0,35 s, mejor intento, HUD y carga automática de `Nivel3` tras 5 s.
- `NeuroPhobiaLevel2SafeDistance`: límite de 8 m para XR Origin; el script ya contempla un jugador PC si está presente y activo.
- `Level2SpiderIdleLife`: movimiento sutil de la araña.
- `MetricsCollector`, `SessionManager` y `LevelMetrics`: métricas existentes; no se creó otro colector.
- `NeuroPhobiaNivel2ParqueGrandeFinal`: generador del parque de la cliente incluido en este repositorio.

## Objetos y referencias preparados

`NPLevel2SceneCompletion` está disponible en `Tools > NeuroPhobia > Nivel 2 > Completar escena y jugabilidad` una vez que Unity termine la importación normal. Su ejecución, aún pendiente, está limitada a `Assets/Scenes/Nivel2.unity` y genera/conecta:

- `NP_ENV_NIVEL2_PARQUE_GRANDE_FINAL`, con el parque de 60 × 50 m del paquete de la cliente.
- `NP_WolfSpider_Nivel2_Static`, usando `Wolf_spider.fbx`, collider y `Level2SpiderIdleLife`.
- XR Origin existente, activado y situado frente a la araña, sin cambios en ajustes globales XR.
- `NP_NIVEL2_FINAL` con mirada, distancia segura y `MetricsCollector` enlazados a araña y cámara.

El HUD de mirada se preparó como Canvas World Space delante de la cámara activa. El aviso de distancia del script existente requiere comprobación visual física en Quest.

## Mecánica y métricas preparadas

La mirada acumula 30 s de exposición. Al apartarla más de 0,35 s, el intento actual vuelve a cero y se registra un intento en `MetricsCollector`. Al completar, se registra `LevelMetrics` con nivel 2, resultado positivo y tiempo de mirada, si existe `SessionManager`, y luego se carga `Nivel3`. El controlador de distancia devuelve al jugador al exterior de 8 m.

## Verificación corta

- C#: compilación inicial de Unity completada sin errores; solo advertencias CS0618 de otros scripts. Las últimas ediciones de este informe y del Canvas no se recompilaron después de detener la importación.
- Missing Scripts: no se detectaron GUID de scripts faltantes en el YAML actual de Nivel2; la escena ensamblada aún no existe y por tanto no fue verificada.
- Referencias críticas: preparadas en el ensamblador, pero aún no serializadas ni verificadas en la escena.
- Input Actions/cámara/controladores: el prefab XR Origin existente permanece en Nivel2; no se cambiaron sus assets ni la configuración XR. El `NeuroPhobiaControlModeSelector` y `NeuroPhobiaDesktopController` actuales son VR-only, por lo que no se puede afirmar control de teclado/ratón.
- EventSystem: no se añadió interacción UI; la transición es automática. El EventSystem y el HUD deben comprobarse en Play y Quest.

## Prueba manual pendiente en PC

Terminar la importación en Unity, ejecutar una vez el menú de ensamblaje y guardar Nivel2. Abrir la escena, comprobar el parque y la araña, Play con simulación XR disponible, mirada y reinicio al apartar la vista, retorno al límite de 8 m, HUD, registro de métricas y carga de Nivel3. Verificar Missing Scripts y consola tras el guardado.

## Prueba física pendiente en Quest

Confirmar posición y escala del parque/araña, cámara y controladores XR, legibilidad del HUD y aviso de distancia, movimiento del origen al entrar a menos de 8 m, exposición de 30 s, resultado de métricas y transición a Nivel3. Sin build completa en esta revisión.
