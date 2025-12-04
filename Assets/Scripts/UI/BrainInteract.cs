using UnityEngine;
using UnityEngine.InputSystem;

public class BrainInteract : MonoBehaviour
{
    public float interactDistance = 2f;

    private Transform player;
    private PlayerInventory inventory;
    private bool isInRange = false;
    private BrainPickup pickup;

    void Start()
    {
        pickup = GetComponent<BrainPickup>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            inventory = p.GetComponent<PlayerInventory>();
        }
    }

    void Update()
    {
        if (player == null || inventory == null) return;

        float dist = Vector3.Distance(player.position, transform.position);

        // je hráč blízko?
        if (dist <= interactDistance)
        {
            if (!isInRange)
            {
                inventory.ShowInteract("Press <b>E</b> to pick up");
                isInRange = true;
            }

            // pickup po stlačení E
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                pickup.Pickup(inventory);
                inventory.HideInteract();
                Destroy(gameObject);
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

    void OnDestroy()
    {
        if (isInRange && inventory != null)
            inventory.HideInteract();
    }
}
