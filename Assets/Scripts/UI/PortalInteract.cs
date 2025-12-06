using UnityEngine;
using UnityEngine.InputSystem;

public class PortalInteract : MonoBehaviour
{
    [Header("Interact nastavenia")]
    [Tooltip("Maximálna vzdialenosť, z ktorej môže hráč interagovať s portálom.")]
    public float interactDistance = 2.5f;

    [Tooltip("Text, ktorý sa zobrazí, keď je hráč v dosahu.")]
    public string interactMessage = "Press <b>E</b> to enter";

    private Transform player;
    private PlayerInventory inventory;
    private FinalPortal finalPortal;

    private bool isInRange = false;

    private void Start()
    {
        finalPortal = GetComponent<FinalPortal>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            inventory = p.GetComponent<PlayerInventory>();
        }
    }

    private void Update()
    {
        if (player == null || inventory == null || finalPortal == null)
            return;

        float dist = Vector3.Distance(player.position, transform.position);

        if (dist <= interactDistance)
        {
            if (!isInRange)
            {
                inventory.ShowInteract(interactMessage);
                isInRange = true;
            }

            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                finalPortal.Enter();
                inventory.HideInteract();
            }
        }
        else
        {
            if (isInRange)
            {
                inventory.HideInteract();
                isInRange = false;
            }
        }
    }

    private void OnDestroy()
    {
        if (isInRange && inventory != null)
        {
            inventory.HideInteract();
        }
    }
}
