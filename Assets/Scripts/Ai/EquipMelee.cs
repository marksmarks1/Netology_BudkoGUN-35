using UnityEngine;

public class EquipMelee : MonoBehaviour
{
    public Transform meleeStick;             
    MeshSockets sockets;

    void Awake() { sockets = GetComponent<MeshSockets>(); }

    void Start()
    {
        if (!meleeStick || !sockets) {  return; }

        sockets.Attach(meleeStick, MeshSockets.SocketId.RightHand);
        meleeStick.localPosition = Vector3.zero;           
        meleeStick.localRotation = Quaternion.identity;
    }
}
