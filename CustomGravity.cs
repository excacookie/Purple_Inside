using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Magic;

internal class CustomGravity : MonoBehaviour
{
    public Vector3 GravityDirection = Vector3.down; // Direction contrôlable
    public float GravityStrength = 9.81f; // Intensité contrôlable

    private Rigidbody? rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody manquant sur " + gameObject.name);
            Destroy(this);
        }
    }

    void FixedUpdate()
    {
        // Applique la gravité de façon continue
        rb.AddForce(GravityDirection.normalized * GravityStrength, ForceMode.Acceleration);
    }
}