using UnityEngine;

public abstract class PickUpItem : MonoBehaviour
{
    [SerializeField] private AudioClip pickupSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnPickedUp(collision.gameObject);

        if (pickupSound != null) SoundManager.PlayClip(pickupSound);

        Destroy(gameObject);
    }

    protected abstract void OnPickedUp(GameObject gameObject);
}