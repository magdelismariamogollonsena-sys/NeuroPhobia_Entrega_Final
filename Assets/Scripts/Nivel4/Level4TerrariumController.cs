using UnityEngine;

public class Level4TerrariumController : MonoBehaviour
{
    public Transform lid;
    public Level4HUD hud;
    public float openAngle = 75f;
    public float rotationSpeed = 90f;

    [SerializeField] private bool isOpen;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    private void Start()
    {
        if (lid == null)
            lid = transform;

        closedRotation = lid.localRotation;
        openRotation = closedRotation * Quaternion.Euler(openAngle, 0f, 0f);
    }

    private void Update()
    {
        Quaternion target = isOpen ? openRotation : closedRotation;

        lid.localRotation = Quaternion.RotateTowards(
            lid.localRotation,
            target,
            rotationSpeed * Time.deltaTime
        );
    }

    public void ToggleTerrarium()
    {
        isOpen = !isOpen;

        if (hud != null)
        {
            hud.ShowWarning(
                isOpen
                    ? "Terrario abierto. Acerca la mano lentamente."
                    : "Terrario cerrado.",
                2f
            );
        }
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}
