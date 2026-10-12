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
| Pausa | CUMPLE | UI global en Nivel1, Nivel3 y Nivel4, `GameStateManager`, `Time.timeScale` y pausa de `AranaController`. |
| Salida de emergencia | CUMPLE | Confirma, registra métricas disponibles como nivel no completado y vuelve a MainMenu. |
| Resultado por nivel | CUMPLE | CanvasLevelComplete reutilizable para Nivel1, Nivel3 y Nivel4. Nivel2 y Nivel5 quedan bloqueados. |
| Resumen de sesión | CUMPLE | CanvasSessionSummary usa `SessionManager.GetReport()` después de cerrar. |
| Retroalimentación | CUMPLE | Reglas descriptivas basadas en `LevelMetrics`, sin interpretación clínica. |
| SessionReport | CUMPLE | Grado, ID, tiempo, niveles y métricas reales. |
| JSON | CUMPLE | `EndSession()` usa la serialización local existente. Verificar escritura en Play Mode. |
| FastAPIClient | CUMPLE | Se reutilizan los métodos existentes; errores de red no bloquean el flujo. |
| HUD global | CUMPLE | Se conservan los HUD de cada escenario y solo se añade un botón de pausa separado. Las métricas de tiempo aparecen en el resultado/resumen. |
| Nivel1 | CUMPLE | Resultado conectado; progresión a Nivel2 bloqueada por escenario pendiente. |
| Nivel2 | PENDIENTE NIVEL2 | BLOQUEADO: escenario nuevo de la cliente no presente en repositorio. Escena intacta. |
| Nivel3 | CUMPLE | Resultado conectado y avance manual a Nivel4. |
| Nivel4 | CUMPLE | Resultado conectado; progresión a Nivel5 bloqueada. |
| Nivel5 | PENDIENTE NIVEL5 | Falta escenario de apartamento adecuado y posterior configuración visual; después conectar TaskManager, SpawnManager, PhobiaGrade, HUD, métricas y SessionReport. Escena intacta. |
| PC | PENDIENTE PC | Probar mouse, Escape, foco de campos, pausa, reportes y escritura JSON en Editor/PC. |
| Quest | PENDIENTE QUEST | Probar raycast UI, legibilidad y controles. La entrada física de pausa requiere acción segura validada en dispositivo; botón UI siempre visible. |
| IA | FUERA DE ALCANCE | Solo inventario de parámetros. |
| Backend | FUERA DE ALCANCE | Sin implementación de servidor/endpoints. |
| Base de datos | FUERA DE ALCANCE | Sin persistencia remota. |

Verificación estática: compilación C# con proyecto generado y `GlobalSessionUI.cs` incluido temporalmente: 0 errores. No se hizo build Android ni se ejecutó Unity Play Mode. Escenas `.unity` no modificadas; por tanto, el conteo de Missing Scripts en Editor queda pendiente de inspección visual.
