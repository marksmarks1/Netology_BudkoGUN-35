using UnityEngine;

[ExecuteAlways]                   
public class EnemyFOV : MonoBehaviour
{
    //-------------- ÏÓÁËÈ×ÍÛÅ ÏÀÐÀÌÅÒÐÛ --------------
    [Header("Field-of-View settings")]
    public float viewRadius = 5f;                 
    public Vector3 viewCenterOffset = Vector3.zero;

    [Header("Detection")]
    public LayerMask playerMask;                 

    //-------------- ÑÂÎÉÑÒÂÀ --------------
    public Vector3 ViewCenterWorld => transform.TransformPoint(viewCenterOffset);

    //-------------- ÆÈÇÍÅÍÍÛÉ ÖÈÊË --------------
    private void Update()
    {
        Collider[] hits = Physics.OverlapSphere(ViewCenterWorld, viewRadius, playerMask);
        if (hits.Length > 0)
        {
            Debug.Log($"{gameObject.name} sees the player!");
        }
    }

    //-------------- GIZMOS --------------
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(viewCenterOffset, viewRadius);
        Gizmos.matrix = old;
    }
}
