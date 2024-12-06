using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;

public class PFRequestManager : NetworkBehaviour
{
    Queue<PathRequest> pfRequestQueue = new Queue<PathRequest>();
    PathRequest currentPathRequest;
    static PFRequestManager instance;
    Pathfinding pathfinding;
    bool isProcessingPath;
    void Awake()
    {
        instance = this;
        pathfinding = GetComponent<Pathfinding>();

        if (pathfinding == null)
        {
            Debug.LogError("Pathfinding component not found on PFRequestManager GameObject. This log is in awake");
        }
    }
    public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)
    {
        if (instance == null)
        {
            Debug.LogError("PFRequestManager instance is null. Ensure PFRequestManager is attached to a GameObject in the scene.");
            return;
        }
        if (instance.pathfinding == null)
        {
            Debug.LogError("Pathfinding component is not set on PFRequestManager.");
            return;
        }

        if (NetworkManager.Singleton.IsServer) // Only add requests if this is the server
        {
            PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback);
            instance.pfRequestQueue.Enqueue(newRequest);
            instance.TryProcessNext();
        }
    }
    void TryProcessNext()
    {
        if (instance == null)
        {
            Debug.LogError("PFRequestManager instance is null. Ensure PFRequestManager is attached to a GameObject in the scene.");
            return;
        }
        if(!isProcessingPath && pfRequestQueue.Count > 0)
        {
            if (pathfinding == null)
            {
                Debug.LogError("Pathfinding component is null. this log is in tryprocessnext");
            }

            currentPathRequest = pfRequestQueue.Dequeue();
            isProcessingPath = true;
            pathfinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd);
        }
    }
    public void FinishedProcessingPath(Vector3[] path, bool success)
    {
        if (IsServer)
        {
            currentPathRequest.callback(path, success);
        }

        isProcessingPath = false;
        TryProcessNext();
    }
    public void FinishedProcessingPathold(Vector3[] path, bool success)
    {
        currentPathRequest.callback(path,success);
        isProcessingPath=false;
        TryProcessNext();
    }
    struct PathRequest
    {
        public Vector3 pathStart;
        public Vector3 pathEnd;
        public Action<Vector3[], bool> callback;

        public PathRequest(Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback)
        {
            pathStart = _start;
            pathEnd = _end;
            callback = _callback;
        }
    }
}
