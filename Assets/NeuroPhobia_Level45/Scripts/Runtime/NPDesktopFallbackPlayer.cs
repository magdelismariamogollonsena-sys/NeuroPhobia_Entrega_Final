using UnityEngine;

namespace NeuroPhobia.Level45
{
    /// <summary>
    /// Rig de escritorio heredado. NeuroPhobia es exclusivamente VR (Meta Quest 3):
    /// este componente ya no procesa teclado/mouse y permanece deshabilitado.
    /// Se conserva la clase porque otros scripts del nivel siguen
    /// referenciándola (GetComponent/AddComponent) para resolver la cámara activa.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [DisallowMultipleComponent]
    public sealed class NPDesktopFallbackPlayer : MonoBehaviour
    {
        public Camera playerCamera;
        public float moveSpeed = 3.2f;
        public bool lockCursorOnStart = true;
        public bool allowArrowKeys = true;

        public void LockCursor()
        {
        }

        public void UnlockCursor()
        {
        }

        private void Awake()
        {
            enabled = false;
        }
    }
}
