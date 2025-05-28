using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Dirt : MonoBehaviour
{
    [Tooltip("Tag of the broom object.")]
    [SerializeField] private string broomTag = "Broom";

    private void OnTriggerEnter(Collider other)
    {
        // Eðer sahnede “Broom” tag’li bir þey girerse
        if (other.CompareTag(broomTag))
        {
            // Lekeyi yok et (veya Fade out animasyonu oynatabilirsin)
            Destroy(gameObject);
        }
    }
}
