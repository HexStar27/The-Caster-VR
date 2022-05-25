using UnityEngine;
using UnityEngine.Events;

public class LockOpener : MonoBehaviour
{
    public LayerMask lockLayer;

    [System.Serializable]
    public class OnEvent : UnityEvent {};
    [SerializeField]OnEvent onUse = new OnEvent();

    private void OnCollisionEnter(Collision collision)
    {
        if((1<<collision.collider.gameObject.layer & lockLayer.value) > 0)
        {
            collision.gameObject.SetActive(false); //Alomejor lo cambio por una llamada o una animación, o no
            onUse.Invoke();

            Destroy(gameObject);
        }
    }
}
