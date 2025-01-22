using UnityEngine;
using UnityEngine.UI;

public class Collectible : MonoBehaviour
{
    [HideInInspector]
    public EndlessRoadManager manager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            manager.OnCollectibleTriggered(this.gameObject);
        }
    }
}
