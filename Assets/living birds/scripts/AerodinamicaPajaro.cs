using UnityEngine;

/// <summary>
/// Vuelo de un pájaro pequeño.
/// Sustentación L = ½ ρ v² S Cl
/// Arrastre     D = ½ ρ v² S Cd
/// Peso         W = m g
/// Empuje       T a lo largo del pico, modulado por el aleteo.
/// </summary>
public class AerodinamicaPajaro : MonoBehaviour
{
    [Header("Aire y ala")]
    [Tooltip("Densidad del aire ρ, en kg/m³")]
    [SerializeField] float airDensity = 1.225f;
    [Tooltip("Área alar S de un pájaro de escala 1, en m²")]
    [SerializeField] float wingArea = 0.05f;
    [Tooltip("Coeficiente de sustentación Cl")]
    [SerializeField] float liftCoefficient = 1.05f;
    [Tooltip("Coeficiente de arrastre Cd")]
    [SerializeField] float dragCoefficient = 0.4f;

    [Header("Empuje y aleteo")]
    [Tooltip("Empuje medio T, en newtons, a escala 1")]
    [SerializeField] float thrust = 0.42f;
    [Tooltip("Aleteos por segundo")]
    [SerializeField] float flapFrequency = 5f;
    [Tooltip("Segundos de empuje extra al despegar")]
    [SerializeField] float takeoffSeconds = 0.8f;

    [Header("Masa")]
    [Tooltip("Masa a escala 1, en kg")]
    [SerializeField] float mass = 0.08f;

    Rigidbody body;
    bool flying;
    float takeoffTimer;
    float currentWingArea;
    float currentThrust;
    float multiplicadorEmpuje = 1f;
    bool rumboForzado;
    Vector3 rumbo = Vector3.forward;

    public bool EnVuelo => flying;
    public bool RumboForzado => rumboForzado;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        currentWingArea = wingArea;
        currentThrust = thrust;
        rumbo = transform.forward;
        if (body != null)
        {
            body.mass = mass;
            body.useGravity = false;
        }
    }

    public void Configurar(float escalaDelPajaro)
    {
        float scale = Mathf.Max(0.2f, escalaDelPajaro);
        float scale3 = scale * scale * scale;
        currentWingArea = wingArea * scale * scale;
        currentThrust = thrust * scale3;
        if (body != null)
        {
            body.mass = mass * scale3;
        }
    }

    public void ActivarVuelo(bool activo)
    {
        flying = activo;
        if (!activo)
        {
            takeoffTimer = 0f;
            return;
        }

        takeoffTimer = takeoffSeconds;
        if (body != null)
        {
            body.useGravity = false;
            body.isKinematic = false;
        }
    }

    public void FijarRumbo(Vector3 direccion, bool forzado)
    {
        if (direccion.sqrMagnitude < 0.0001f)
        {
            return;
        }

        rumbo = direccion.normalized;
        rumboForzado = forzado;
    }

    public void SoltarRumbo()
    {
        rumboForzado = false;
    }

    public void FijarMultiplicadorEmpuje(float valor)
    {
        multiplicadorEmpuje = Mathf.Max(0.2f, valor);
    }

    void FixedUpdate()
    {
        if (!flying || body == null || body.isKinematic)
        {
            return;
        }

        Vector3 velocity = body.linearVelocity;
        float speed = velocity.magnitude;
        float dynamicPressure = 0.5f * airDensity * speed * speed * currentWingArea;

        Vector3 drag = Vector3.zero;
        Vector3 lift = Vector3.zero;
        if (speed > 0.05f)
        {
            drag = -velocity / speed * (dynamicPressure * dragCoefficient);
        }

        if (speed > 0.35f)
        {
            Vector3 lateral = Vector3.Cross(velocity, Vector3.up);
            if (lateral.sqrMagnitude > 0.0001f)
            {
                Vector3 liftDir = Vector3.Cross(lateral, velocity).normalized;
                if (Vector3.Dot(liftDir, Vector3.up) < 0f)
                {
                    liftDir = -liftDir;
                }

                float flap = 0.65f + 0.35f * Mathf.Sin(Time.time * flapFrequency * Mathf.PI * 2f);
                lift = liftDir * (dynamicPressure * liftCoefficient * flap);
            }
        }

        float flapThrust = 0.55f + 0.45f * Mathf.Sin(Time.time * flapFrequency * Mathf.PI * 2f);
        Vector3 adelante = rumbo.sqrMagnitude > 0.0001f ? rumbo : transform.forward;
        Vector3 thrustForce = adelante * (currentThrust * flapThrust * multiplicadorEmpuje);
        if (takeoffTimer > 0f)
        {
            takeoffTimer -= Time.fixedDeltaTime;
            thrustForce += Vector3.up * (currentThrust * 3.2f);
        }

        Vector3 weight = Vector3.up * (body.mass * Physics.gravity.y);
        body.AddForce(lift + drag + thrustForce + weight, ForceMode.Force);
    }
}
