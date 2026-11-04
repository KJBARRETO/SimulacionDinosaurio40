using UnityEngine;

/// <summary>
/// Rebote de la pelota contra un contacto.
/// En la normal: v_n' = -e · v_n
/// En la tangente: v_t' = v_t · (1 - μ)
/// La energía normal queda multiplicada por e².
/// Si |v_n'| baja del umbral, el rebote se apaga y la pelota se queda quieta.
/// </summary>
public class RebotePelota : MonoBehaviour
{
    [Tooltip("Coeficiente de restitución e. 0 = no rebota, 1 = no pierde altura")]
    [SerializeField] [Range(0f, 1f)] float restitucion = 0.72f;
    [Tooltip("Fricción tangencial μ. 0 = resbala, 1 = frena todo el deslizamiento")]
    [SerializeField] [Range(0f, 1f)] float friccionTangencial = 0.22f;
    [Tooltip("Velocidad normal de salida, en m/s, por debajo de la cual ya no rebota")]
    [SerializeField] float umbralRebote = 0.7f;
    [Tooltip("Rapidez total, en m/s, bajo la cual la pelota se detiene")]
    [SerializeField] float umbralReposo = 0.35f;

    Rigidbody cuerpo;
    Vector3 velocidadAntes;

    public float UltimaEnergiaNormal { get; private set; }

    void Awake()
    {
        cuerpo = GetComponent<Rigidbody>();
        SphereCollider esfera = GetComponent<SphereCollider>();
        if (esfera != null)
        {
            esfera.providesContacts = true;
            PhysicsMaterial material = new PhysicsMaterial("PelotaSinReboteDelMotor")
            {
                bounciness = 0f,
                dynamicFriction = 0f,
                staticFriction = 0f,
                bounceCombine = PhysicsMaterialCombine.Minimum,
                frictionCombine = PhysicsMaterialCombine.Minimum
            };
            esfera.material = material;
        }
    }

    void FixedUpdate()
    {
        if (cuerpo != null)
        {
            velocidadAntes = cuerpo.linearVelocity;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        AplicarRebote(collision);
    }

    void AplicarRebote(Collision collision)
    {
        if (cuerpo == null || collision.contactCount == 0)
        {
            return;
        }

        Vector3 normal = collision.GetContact(0).normal;
        if (normal.sqrMagnitude < 0.0001f)
        {
            return;
        }
        normal.Normalize();

        Vector3 velocidadOtro = Vector3.zero;
        if (collision.rigidbody != null)
        {
            velocidadOtro = collision.rigidbody.linearVelocity;
        }

        Vector3 relativa = velocidadAntes - velocidadOtro;
        float velocidadNormal = Vector3.Dot(relativa, normal);
        if (velocidadNormal > -0.05f)
        {
            return;
        }

        float e = Mathf.Clamp01(restitucion);
        float salidaNormal = -e * velocidadNormal;
        UltimaEnergiaNormal = e * e;

        if (salidaNormal < umbralRebote)
        {
            salidaNormal = 0f;
        }

        Vector3 tangencial = relativa - normal * velocidadNormal;
        tangencial *= 1f - Mathf.Clamp01(friccionTangencial);

        Vector3 nueva = velocidadOtro + tangencial + normal * salidaNormal;
        if (nueva.magnitude < umbralReposo)
        {
            nueva = Vector3.zero;
            cuerpo.angularVelocity = Vector3.zero;
        }

        cuerpo.linearVelocity = nueva;
        velocidadAntes = nueva;
    }

    public static void AsegurarEnEscena(Transform dinosaurio)
    {
        if (dinosaurio == null || FindFirstObjectByType<RebotePelota>() != null)
        {
            return;
        }

        GameObject pelota = GameObject.Find("Beach Ball");
        if (pelota == null)
        {
            return;
        }

        pelota.isStatic = false;

        Rigidbody cuerpo = pelota.GetComponent<Rigidbody>();
        if (cuerpo == null)
        {
            cuerpo = pelota.AddComponent<Rigidbody>();
        }

        cuerpo.mass = 0.4f;
        cuerpo.useGravity = true;
        cuerpo.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        cuerpo.interpolation = RigidbodyInterpolation.Interpolate;

        if (pelota.GetComponent<RebotePelota>() == null)
        {
            pelota.AddComponent<RebotePelota>();
        }
    }
}
