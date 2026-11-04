using UnityEngine;

/// <summary>
/// Huida: el pájaro toma el vector opuesto al dinosaurio,
/// lo suaviza y aumenta el empuje. La velocidad de crucero
/// queda por debajo de la carrera del dinosaurio para que pueda alcanzarlo.
/// </summary>
public class HuidaPajaro : MonoBehaviour
{
    [Tooltip("Distancia a la que el pájaro nota al dinosaurio y huye, en metros")]
    [SerializeField] float radioDeteccion = 14f;
    [Tooltip("Distancia a la que el dinosaurio atrapa al pájaro, en metros")]
    [SerializeField] float radioAlcance = 2.4f;
    [Tooltip("Cuánto sube el empuje durante la huida. 2 = el doble, todavía alcanzable")]
    [SerializeField] float multiplicadorHuida = 1.85f;
    [Tooltip("Qué tan rápido gira hacia la ruta de escape, en 1/s")]
    [SerializeField] float suavizado = 4.5f;
    [Tooltip("Altura sobre el dinosaurio a partir de la cual el pájaro baja para poder ser alcanzado")]
    [SerializeField] float alturaAlcanzable = 2.6f;

    Transform dinosaurio;
    AerodinamicaPajaro vuelo;
    Pajaro pajaro;
    Vector3 direccionEscape = Vector3.forward;
    bool huyendo;
    bool atrapado;

    void Awake()
    {
        vuelo = GetComponent<AerodinamicaPajaro>();
        pajaro = GetComponent<Pajaro>();
    }

    void Start()
    {
        DinoVehicle.DinoVehicleController control = FindFirstObjectByType<DinoVehicle.DinoVehicleController>();
        if (control != null)
        {
            dinosaurio = control.transform;
        }
    }

    void Update()
    {
        if (atrapado || dinosaurio == null)
        {
            return;
        }

        Vector3 separacion = transform.position - dinosaurio.position;
        float altura = separacion.y;
        separacion.y = 0f;
        float distancia = separacion.magnitude;

        if (distancia < radioAlcance && Mathf.Abs(altura) < 3.2f)
        {
            Atrapar();
            return;
        }

        if (distancia > radioDeteccion || separacion.sqrMagnitude < 0.0001f)
        {
            if (huyendo)
            {
                huyendo = false;
                if (vuelo != null)
                {
                    vuelo.FijarMultiplicadorEmpuje(1f);
                    vuelo.SoltarRumbo();
                }
            }
            return;
        }

        if (!huyendo)
        {
            huyendo = true;
            SendMessage("Flee", SendMessageOptions.DontRequireReceiver);
        }

        Vector3 escape = separacion.normalized;
        escape.y = altura > alturaAlcanzable ? -0.25f : 0.22f;
        escape.Normalize();

        float avance = 1f - Mathf.Exp(-suavizado * Time.deltaTime);
        direccionEscape = Vector3.Slerp(direccionEscape, escape, avance);

        if (vuelo != null)
        {
            vuelo.FijarRumbo(direccionEscape, true);
            vuelo.FijarMultiplicadorEmpuje(multiplicadorHuida);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (EsDinosaurio(collision.collider))
        {
            Atrapar();
        }
    }

    void OnTriggerEnter(Collider otro)
    {
        if (EsDinosaurio(otro))
        {
            Atrapar();
        }
    }

    bool EsDinosaurio(Collider otro)
    {
        return dinosaurio != null && otro.transform.root == dinosaurio;
    }

    void Atrapar()
    {
        if (atrapado)
        {
            return;
        }

        atrapado = true;
        huyendo = false;
        if (vuelo != null)
        {
            vuelo.ActivarVuelo(false);
            vuelo.FijarMultiplicadorEmpuje(1f);
            vuelo.SoltarRumbo();
        }

        if (pajaro != null)
        {
            pajaro.KillBird();
        }
    }
}
