using System.Collections.Generic;
using UnityEngine;

public class MeshSockets : MonoBehaviour
{
    public enum SocketId { Spine, RightHip, RightHand }

    Dictionary<SocketId, MeshSocket> socketMap = new Dictionary<SocketId, MeshSocket>();

    void Awake()                                  
    {
        socketMap.Clear();

        var sockets = GetComponentsInChildren<MeshSocket>(true);
        foreach (var s in sockets)
            socketMap[s.socketId] = s;
    }

    public void Attach(Transform objectTransform, SocketId socketId)
    {
        if (!socketMap.TryGetValue(socketId, out var socket))
        {
            return;
        }
        socket.Attach(objectTransform);
    }
}
