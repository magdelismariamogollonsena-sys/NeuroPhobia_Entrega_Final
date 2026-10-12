# Checklist final — alcance Unity

| Elemento | Estado | Evidencia o límite |
|---|---|---|
| Bienvenida y consentimiento | CUMPLE | MainMenu abre el panel antes de iniciar la sesión. |
| Captura PhobiaGrade 1..5 | CUMPLE | `StartSession(grade)` recibe la selección y `SessionReport.phobiaGrade` la conserva. La tabla grado → nivel espera definición de la cliente. |
| SessionManager | CUMPLE | Una sola instancia persistente; cierre idempotente y duración de reloj real. |
| MainMenu | CUMPLE | Inicio → consentimiento → evaluación → Nivel1; datos, controles y salir. |
| Perfil, interfaz Unity | CUMPLE | Nombre, correo, validación, guardar local durante la ejecución y volver. |
| Validación visual de datos | CUMPLE | Mensajes específicos para nombre y correo. |
| Persistencia/backend de perfil | FUERA DE ALCANCE | No se envía ni almacena remotamente. |
| Pausa | CUMPLE | UI global en Nivel1, Nivel3, Nivel4 y Nivel5, `GameStateManager`, `Time.timeScale` y pausa de `AranaController`. |
| Salida de emergencia | CUMPLE | Confirma, registra métricas disponibles como nivel no completado y vuelve a MainMenu. |
| Resultado por nivel | CUMPLE | CanvasLevelComplete para Nivel1, Nivel3 y Nivel4; Nivel5 cierra en CanvasSessionSummary. |
| Resumen de sesión | CUMPLE | CanvasSessionSummary usa `SessionManager.GetReport()` después de cerrar. |
| Retroalimentación | CUMPLE | Reglas descriptivas basadas en `LevelMetrics`, sin interpretación clínica. |
| SessionReport | CUMPLE | Grado, ID, tiempo, niveles y métricas reales. |
| JSON | CUMPLE | `EndSession()` usa la serialización local existente. Verificar escritura en Play Mode. |
| FastAPIClient | CUMPLE | Se reutilizan los métodos existentes; errores de red no bloquean el flujo. |
| HUD global | CUMPLE | Se conservan los HUD de cada escenario y solo se añade un botón de pausa separado. Las métricas de tiempo aparecen en el resultado/resumen. |
| Nivel1 | CUMPLE | Resultado conectado; progresión a Nivel2 bloqueada por escenario pendiente. |
| Nivel2 | PENDIENTE NIVEL2 | BLOQUEADO: escenario nuevo de la cliente no presente en repositorio. Escena intacta. |
| Nivel3 | CUMPLE | Resultado conectado y avance manual a Nivel4. |
| Nivel4 | CUMPLE | Resultado conectado; avance manual a Nivel5 disponible. |
| Nivel5 | IMPLEMENTADO / PENDIENTE PRUEBA QUEST | Apartamento conservado; paredes URP y techo; cuatro tareas, PhobiaGrade, SpawnManager, métricas, cierre y resumen conectados. Puerta y nevera conservan el visual estático por la restricción de Transform. Ver REVISION_NIVEL5_FINAL.md. |
| PC | PENDIENTE PC | Probar mouse, Escape, foco de campos, pausa, reportes y escritura JSON en Editor/PC. |
| Quest | PENDIENTE QUEST | Probar raycast UI, legibilidad y controles. La entrada física de pausa requiere acción segura validada en dispositivo; botón UI siempre visible. |
| IA | FUERA DE ALCANCE | Solo inventario de parámetros. |
| Backend | FUERA DE ALCANCE | Sin implementación de servidor/endpoints. |
| Base de datos | FUERA DE ALCANCE | Sin persistencia remota. |

Verificacion estatica Nivel5: Unity 6000.3.7f1 compilo con 0 errores C# y guardo la escena. No se hizo build Android ni Play Mode. Missing Scripts: 0 referencias sin resolver en Assets y PackageCache. Quedan pendientes las pruebas PC y Quest.
