using Debug = UnityEngine.Debug;

namespace Magic.Component;

internal class CustomGravity : MonoBehaviour
{
    public Vector3 GravityDirection = Vector3.down; // Direction contrôlable
    public float GravityStrength = 9.81f; // Intensité contrôlable

#pragma warning disable CS8618 // Un champ non-nullable doit contenir une valeur autre que Null lors de la fermeture du constructeur. Envisagez d’ajouter le modificateur « required » ou de déclarer le champ comme pouvant accepter la valeur Null.
    private Rigidbody rb;
#pragma warning restore CS8618 // Un champ non-nullable doit contenir une valeur autre que Null lors de la fermeture du constructeur. Envisagez d’ajouter le modificateur « required » ou de déclarer le champ comme pouvant accepter la valeur Null.

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody manquant sur " + gameObject.name);
            Destroy(this);
        }
    }

    private void FixedUpdate()
    {
        // Applique la gravité de façon continue
        rb.AddForce(GravityDirection.normalized * GravityStrength, ForceMode.Acceleration);
    }
}