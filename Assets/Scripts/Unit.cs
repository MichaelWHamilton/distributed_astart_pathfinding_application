using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using UnityEngine;

public class Unit : NetworkBehaviour
{
   //public Transform target;
   public Vector3 targetPosition;
   float speed = 20;
   Vector3[] path;
   int targetIndex;
   Ray ray;
   RaycastHit hit;
   void Update()
   {
        if (!IsOwner) return;
        if (Input.GetMouseButtonDown(1)) // Right-click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 targetPosition = hit.point + hit.normal * 0.1f;
                Debug.Log($"Client {NetworkObjectId}: Right-click detected.");

                if (IsHost)
                {
                    PFRequestManager.RequestPath(transform.position, targetPosition, OnPathFound);
                }
                else
                {
                    RequestPathServerRpc(targetPosition);
                }
            }
        }        
   }

   [ServerRpc]
    public void RequestPathServerRpc(Vector3 targetPosition)
    {
        if (!IsServer) return;
        PFRequestManager.RequestPath(transform.position, targetPosition, OnPathFound);
    }


    public override void OnNetworkSpawn()
    {
        transform.position = new Vector3 (-10, 2, -10);
        if (IsOwner)
        {
            //playerCamera.enabled = true; // Enable camera for the local player
            Debug.Log("owner: " + NetworkObjectId);
            transform.position = new Vector3 (-10, 2, -10);
            //transform.rotation = new Quaternion(0,180,0,0);
            Debug.Log("Player spawned at: " + transform.position);
        }
        else
        {
            //playerCamera.enabled = false; // Disable camera for remote players
            //Debug.Log("Camera disabled for non-owner: " + NetworkObjectId);
        }
    }

   public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
   {
        if (pathSuccessful && newPath.Length > 0)
        {
            path = newPath;
            targetIndex = 0;
            StopCoroutine(FollowPath());
            StartCoroutine(FollowPath());
            if (!IsServer) SyncPathClientRpc(newPath);
        }
        else
        {
            Debug.LogError("Pathfinding failed or returned an empty path.");
        }
   }
   IEnumerator FollowPath()
   {
        if(path.Length <= 0)
        {
            Debug.Log("path is empty and trying to route");
        }
        Vector3 currentWaypoint = path[0];
        while(true)
        {
            if(transform.position == currentWaypoint)
            {
                targetIndex++;
                if(targetIndex >= path.Length)
                {
                    yield break;
                }
                currentWaypoint = path[targetIndex];
            }
            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed*Time.deltaTime);
            yield return null;
        }
   }

   public void OnDrawGizmos()
   {
        if(path!=null)
        {
            for(int i = targetIndex; i< path.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(path[i], Vector3.one);

                if(i==targetIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i]);
                }
                else
                {
                    Gizmos.DrawLine(path[i-1],path[i]);
                }
            }
        }
   }

   [ClientRpc]
    private void SyncPathClientRpc(Vector3[] newPath)
    {
        path = newPath;
        targetIndex = 0;
        StopCoroutine(FollowPath());
        StartCoroutine(FollowPath());
    }

   
}


