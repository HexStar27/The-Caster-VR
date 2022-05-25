using UnityEngine;
using UnityEngine.InputSystem;

public class PincelMano : MonoBehaviour
{
    public GestureManager gm;
    bool trazando = false;
    public void Draw() //A Der
    {
        trazando = !trazando;
        gm.SetTrazando(trazando);
    }
    public void ResetDraw() //B Izq
    {
        gm.Clean();
        gm.RefreshLine();
    }

    public void Fin() //B Der
    {
        gm.TerminarGesto();
    }

    public void EXE() //A Izq
    {
        gm.AddPattern();
        //gm.ExecuteGesture();
    }
}
