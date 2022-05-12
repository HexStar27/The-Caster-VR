using UnityEditor;
using UnityEngine;

public abstract class SpellStructure : MonoBehaviour
{
    public float duracion = 5;
    public float activacionesDesdeSpawn = 0;
    public float velocidadInicial = 10;
    public float aceleracion = 0;
    public bool isTrigger = true; // Sirve para cuando sea hijo de un hechizo, así pueden cambiar las particulas y tal
    public bool dieOnHit = true;
    public bool activateTrail = false;
    public bool[] enableCollisionEvents = { true, false, false};
    //                                      Enter Stay   Exit

    public class OnAction : UnityEngine.Events.UnityEvent { }
    public class OnFollow : UnityEngine.Events.UnityEvent<Transform> { }
    /**
     * Un Hechizo puede concatenar otros hechizos:
     *      -Al iniciar su lanzamiento
     *      -Durante su trayectoria
     *      -Al chocar con un objeto
     *      -Al terminar su duración
    **/
    public OnAction OnCast = new OnAction();
    public OnAction OnTrail = new OnAction();
    public OnAction OnHit = new OnAction();
    public OnAction OnStay = new OnAction();
    public OnAction OnExit = new OnAction();
    public OnAction OnFinish = new OnAction();

    public OnFollow onFollow = new OnFollow();

    protected Transform trans;
    [SerializeField] protected Rigidbody _rb;
    [SerializeField] protected Rigidbody fatherT;
    private float interv = 0;
    private float contador = 0;
    protected bool casted = false;
    protected bool following = false;

    protected bool beenHit = false;

    public virtual void Lanzar()
    {
        beenHit = false;
        casted = true;

        if (activacionesDesdeSpawn > 0) interv = duracion / (activacionesDesdeSpawn + 1);
        contador = interv;

        OnCast.Invoke();
    }

    public virtual void Chocar(Collider target)
    {
        OnHit.Invoke();
    }

    public virtual void Terminar()
    {
        OnFinish.Invoke();
        SafeDestroy(gameObject);
    }

    public virtual void Follow(Rigidbody padreFicticio)
    {
        fatherT = padreFicticio;
        following = padreFicticio != null;
    }

    public Rigidbody GetRigidbody()
    {
        return _rb;
    }

    public bool HasBeenHit() { return beenHit; }

    protected virtual void Start()
    {
    }

    protected virtual void Awake()
    {
        trans = transform;
        _rb = GetComponent<Rigidbody>();
    }

    protected virtual void Update()
    {
        if (casted)
        {
            if (activateTrail && interv > 0)
            {
                contador -= Time.deltaTime;

                if (contador <= 0)
                {
                    OnTrail.Invoke();
                    contador += interv;
                }
            }
        }
        else if(following)
        {
            if(fatherT.gameObject != null) trans.position = fatherT.position;
            else
            {
                SafeDestroy(gameObject); //Cuidado, no se si podría llegar a haber algun problema con referencias nulas...
            }
        }
    }

    protected virtual void OnDestroy()
    {
        StopAllCoroutines();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        beenHit = true;
        Chocar(other);
        if(dieOnHit)
        {
            Terminar();
        }
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        if (!dieOnHit)
        {
            OnStay.Invoke();
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        beenHit = true;
        if (!dieOnHit)
        {
            OnExit.Invoke();
        }
    }

    public System.Collections.IEnumerator Ejecucion()
    {
        Lanzar();   
        yield return new WaitForSeconds(duracion);
        if (!(beenHit && dieOnHit)) Terminar();
    }

    public virtual void EstablecerTipo(SpellType tipo)
    {

    }
    public virtual void EstablecerEfecto(Shape efecto)
    {

    }
    public virtual void EstablecerModificadores(Modifier modificadores)
    {

    }

    protected virtual void SafeDestroy(Object obj)
    {
        Destroy(obj);
    }
}