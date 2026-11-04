using UnityEngine;

/// <summary>
/// El cuerpo mira hacia la velocidad.
/// q(t) = Slerp(q, LookRotation(v) con banqueo, 1 - e^(-k Δt))
/// El cabeceo sale de la componente vertical de v.
/// El banqueo es proporcional a la rapidez del giro.
/// </summary>
public class OrientacionPajaro : MonoBehaviour
{
    [Tooltip("Qué tan rápido el cuerpo alcanza la dirección de la velocidad, en 1/s")]
    [SerializeField] float agudeza = 8f;
    [Tooltip("Grados de inclinación por cada grado/s de giro")]
    [SerializeField] float banqueoPorGiro = 0.28f;
    [Tooltip("Inclinación máxima en un giro, en grados")]
    [SerializeField] float banqueoMaximo = 38f;

    Rigidbody cuerpo;
    AerodinamicaPajaro vuelo;
    Vector3 planoAnterior;
    float banqueo;

    void Awake()
    {
        cuerpo = GetComponent<Rigidbody>();
        vuelo = GetComponent<AerodinamicaPajaro>();
    }

    void LateUpdate()
    {
        if (vuelo == null || cuerpo == null || !vuelo.EnVuelo || cuerpo.isKinematic)
        {
            planoAnterior = Vector3.zero;
            banqueo = 0f;
            return;
        }

        if (!vuelo.RumboForzado)
        {
            vuelo.FijarRumbo(transform.forward, false);
        }

        Vector3 velocidad = cuerpo.linearVelocity;
        if (velocidad.sqrMagnitude < 0.25f)
        {
            return;
        }

        Vector3 plano = Vector3.ProjectOnPlane(velocidad, Vector3.up);
        float objetivo = 0f;
        if (plano.sqrMagnitude > 0.05f && planoAnterior.sqrMagnitude > 0.05f)
        {
            float giro = Vector3.SignedAngle(planoAnterior, plano, Vector3.up);
            float gradosPorSegundo = giro / Mathf.Max(Time.deltaTime, 0.01f);
            objetivo = Mathf.Clamp(-gradosPorSegundo * banqueoPorGiro, -banqueoMaximo, banqueoMaximo);
        }

        planoAnterior = plano;
        float seguimiento = 1f - Mathf.Exp(-agudeza * Time.deltaTime);
        banqueo = Mathf.Lerp(banqueo, objetivo, seguimiento);

        Quaternion mirarVelocidad = Quaternion.LookRotation(velocidad.normalized, Vector3.up);
        Quaternion conBanqueo = mirarVelocidad * Quaternion.AngleAxis(banqueo, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, conBanqueo, seguimiento);
    }
}
