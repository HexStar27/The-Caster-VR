using UnityEngine;

public class KillOnDie : MonoBehaviour
{
    public ObjetoInteractuable ob;
    public GameObject objeivo;

    private void OnEnable()
    {
        ob.onDestroy.AddListener(Kill);
    }

    public void Kill()
    {
        Destroy(objeivo);
        ob.onDestroy.RemoveListener(Kill);
    }
}
