/// Esta clase añade físicas de durabilidad, mojado, quemadura y desintegrado.
/// Está diseñado para que sea reutilizable por un ObjectPool
/// TODO:   
/// -Crear shader "desintegrar"
/// -Crear particle system para el fuego
/// -¿Será mejor cambiar el Diccionario para que la Key sea un enum? De esta forma 
///     no tendría que mirar el código cada vez que quiera mirar qué funciones puedo ejecutar.

using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjetoInteractuable : MonoBehaviour
{
    // Consexiones
    private Renderer _renderer;
    private Collider _col;
    private Rigidbody _rb;
    public Dictionary<string,Action<double>> interacciones = new Dictionary<string, Action<double>>();

    // Getters & Setters
    public double ConstitucionRestante
    {
        get => constitucionRestante;
        private set
        {
            constitucionRestante = value;
            if (ConstitucionRestante <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
    public double TiempoFuego
    {
        get => tiempoFuego;
        private set
        {
            tiempoFuego = value;
            if (tiempoFuego < 0 || debilidadFuego <= 0) tiempoFuego = 0;
        }
    }
    public double Mojadez
    {
        get => mojadez;
        private set
        {
            mojadez = value;
            if (mojadez < 0) mojadez = 0;

            _col.material.staticFriction = friccionS / (float)(mojadez+1);
            _col.material.dynamicFriction = friccionD / (float)(mojadez+1);
        }
    }

    // Variables :v
    double constitucionRestante;
    [SerializeField] double constitucionInicial = 10;
    [SerializeField] double debilidadFuego = 0, tiempoFuego = 0, mojadez = 0;
    float friccionD,friccionS;
    float old_vel;

    //Materiales
    Material materialPropio;
    int materialActual = 0;
    GameObject particulillas;

    private void Awake()
    {
        if (_col == null) if(!TryGetComponent(out _col)) Debug.LogError("FALTA COLLIDER");
        if (_rb == null) if(!TryGetComponent(out _rb)) Debug.LogError("FALTA RIGIDBODY");
        if (_renderer == null) if (!TryGetComponent(out _renderer)) Debug.LogError("FALTA RENDERER???");

        materialPropio = _renderer.material;

        interacciones.Add("Quemar", Quemar);
        interacciones.Add("Desintegrar", Desintegrar);
        interacciones.Add("Mojar", Mojar);
        interacciones.Add("Romper", Romper);
    }

    private void OnEnable()
    {
        CambiarMaterial(0);
        friccionD = _col.material.dynamicFriction;
        friccionS = _col.material.staticFriction;

        ConstitucionRestante = constitucionInicial;
        TiempoFuego = 0;
        Mojadez = 0;
        _rb.angularDrag = 0.05f;
    }

    private void Quemar(double cantidad)
    {
        TiempoFuego += cantidad;
    }

    private void Desintegrar(double cantidad)
    {
        if(cantidad > constitucionRestante)
        {
            //Desintegrar
            _rb.useGravity = false;
            _rb.angularDrag = 10;
            _col.enabled = false;
            Mojadez = 0;
            TiempoFuego = 0;

            //TODO: Asignar shader desintegrar
        }
        else
        {
            //Sino, sólo hace algo de daño...
            constitucionRestante = cantidad / 2;
        }
    }

    private void Mojar(double cantidad)
    {
        Mojadez += cantidad/(1+Mojadez);
        if (cantidad > 0 && debilidadFuego > 0) TiempoFuego -= cantidad * (1 / debilidadFuego);
    }

    private void Romper(double cantidad)
    {
        ConstitucionRestante -= cantidad;
    }

    private void FixedUpdate()
    {
        double t = Time.fixedDeltaTime;
        float accel = _rb.velocity.magnitude - old_vel;
        float friccion = (_rb.angularVelocity.magnitude + accel)*0.5f;

        if (TiempoFuego > 0) // Quemándose
        {
            TiempoFuego -= t + Math.Sqrt(Mojadez)*t*0.7f + friccion;

            ConstitucionRestante -= (t * debilidadFuego / (1+Mojadez));
            Mojadez -= t * debilidadFuego*0.85f;

            CambiarMaterial(2);//Fuego
        }
        else if (Mojadez > 0)
        {
            Mojadez -= t*Mojadez*0.2 + friccion;
            CambiarMaterial(1);//Agua
        }
        else CambiarMaterial(0);//Nada

        old_vel = _rb.velocity.magnitude;
    }

    //0 -> Normar
    //1 -> Mojado
    //2 -> Quemado
    public void CambiarMaterial(int material) 
    {
        if (material < BancoEstadosInteractuables.Instancia.materiales.Length && materialActual != material)
        {
            materialActual = material;
            
            if (material == 0) _renderer.material = materialPropio;
            else _renderer.material = BancoEstadosInteractuables.Instancia.materiales[material];

            //Aplicar partículas
            if (particulillas != null) Destroy(particulillas);
            if(material != 0) particulillas = Instantiate(BancoEstadosInteractuables.Instancia.particleSystems[material], transform);
        }
    }

    public bool IsBurning() { return TiempoFuego > 0; }
    public bool IsWet() { return Mojadez > 0; }
}
