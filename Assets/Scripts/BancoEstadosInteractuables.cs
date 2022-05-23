using UnityEngine;

public class BancoEstadosInteractuables : MonoBehaviour
{
    public static BancoEstadosInteractuables Instancia;
    public Material[] materiales;
    public GameObject[] particleSystems;

    private void Awake()
    {
        Instancia = this;
    }
}
