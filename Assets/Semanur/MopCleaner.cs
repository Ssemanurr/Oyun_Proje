using UnityEngine;


public class MopCleaner : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Dirt"))
        {
            Destroy(other.gameObject);
        }
    }
}
