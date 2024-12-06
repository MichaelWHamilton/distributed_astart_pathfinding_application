using UnityEngine;
using Unity.Netcode;

public class CollectResource : NetworkBehaviour
{
    public float pickupRadius = 10f; // Radius to detect resources
    public string resourceTag = "resource"; // Tag for resources

    void Update()
    {
        if (!IsOwner) return; // Ensure only the owning client processes this

        // Check for nearby resources in every frame
        Collider[] nearbyResources = Physics.OverlapSphere(transform.position, pickupRadius);

        foreach (Collider collider in nearbyResources)
        {
            // Check if the object has the specified tag
            if (collider.CompareTag(resourceTag))
            {
                // Request the server to handle the resource pickup
                RequestPickupResourceServerRpc(collider.gameObject.GetComponent<NetworkObject>().NetworkObjectId);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestPickupResourceServerRpc(ulong resourceNetworkId)
    {
        // Find the resource object by NetworkObjectId
        NetworkObject resourceObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[resourceNetworkId];
        
        if (resourceObject != null)
        {
            // Notify all clients to destroy the resource
            DestroyResourceClientRpc(resourceNetworkId);

            // Destroy the resource on the server
            resourceObject.Despawn();
        }
    }

    [ClientRpc]
    void DestroyResourceClientRpc(ulong resourceNetworkId)
    {
        // Find and destroy the resource on all clients
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(resourceNetworkId, out var resourceObject))
        {
            Destroy(resourceObject.gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw a wireframe sphere in the editor to visualize the pickup radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
