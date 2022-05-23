using UnityEngine;
using QDollarGestureRecognizer;
using PDollarGestureRecognizer;
using System.Collections.Generic;
using UnityEngine.Events;

public class GestureManager : MonoBehaviour
{
	[System.Serializable] public class OnGesture : UnityEvent { }

	public Camera camara;
	public Transform pencil;
	public LineRenderer line;
	List<Gesture> listaAuxiliarPatrones = new List<Gesture>();
	[SerializeField] Gesture[] patrones;
	Gesture gestoActual;
	BufferCircular<Vector4> puntos;
	public string gestureName = "DefaultName";

	int trazaIdActual = 0;
	readonly int nPuntosMax = 1024;
	int nPuntosActuales = 0;

	bool evenFrame = true;
	bool trazando = false;
	[SerializeField] bool usingPencil = true;

	[Header("\"Diccionario\" para patrones (interacción con eventos)")]
	public List<string> patternNames = new List<string>();
	public List<OnGesture> onGestureDic = new List<OnGesture>();


	//Pen up / Pen down
	public void SetTrazando(bool value)
    {
		trazando = value;
		if(!value)
        {
			GrabarTraza();
        }
    }

	// Fin
	public void TerminarGesto()
	{
		if (trazando) SetTrazando(false);
		trazaIdActual = 0;

		//Transformar buffer en "Gesture" y limpiar buffer
		Point[] p = new Point[nPuntosActuales];
		for(int i = 0; i < nPuntosActuales; i++)
		{
			Vector4 a = puntos.ValueAt_FAST_AND_DANGER(i);
			p[i] = new Point(a.x, a.y, a.z, (int)a.w);
		}
		gestoActual = new Gesture(p,gestureName);

		puntos.Clear();
	}

	public string Recognize()
	{
		return QPointCloudRecognizer.Classify(gestoActual, patrones);
	}

	//Action of gesture
	public void ExecuteGesture()
	{
		onGestureDic[patternNames.BinarySearch(QPointCloudRecognizer.Classify(gestoActual, patrones))].Invoke();
	}

	public void RecognizeDebug()
	{
		Debug.Log(Recognize());
	}

	public void RefreshLine()
	{
		line.positionCount = nPuntosActuales;
		Vector4[] a = puntos.ToArray();
		Vector3[] b = new Vector3[a.Length];
		for(int i = 0; i < a.Length; i++)
		{
			b[i] = camara.ScreenToWorldPoint(new Vector3(a[i].x, a[i].y, a[i].z));
		}
		line.SetPositions(b);
	}

	//Añadir a base
	public void AddPattern()
	{
		listaAuxiliarPatrones.Add(gestoActual);
		patrones = listaAuxiliarPatrones.ToArray();

		Clean();
	}

	//Reset
	public void Clean()
	{
		line.positionCount = 0;
		puntos.Clear();
		trazando = false;
		trazaIdActual = 0;
		nPuntosActuales = 0;
	}

	private void AddPoint()
	{
		Vector4 pos;
		if (usingPencil) //No testeado
		{
			//Normalizacion de Posicion
			Vector3 normalizedPos = pencil.position - camara.transform.position;
			normalizedPos = camara.transform.InverseTransformDirection(normalizedPos);
			//Asignado de indice
			pos = new Vector4(normalizedPos.x, normalizedPos.y, normalizedPos.z, trazaIdActual);
		}
		else
		{
			nPuntosActuales++;
			if (nPuntosActuales >= nPuntosMax) nPuntosActuales = nPuntosMax;
			pos = Input.mousePosition;
			pos.w = trazaIdActual;
		}
		puntos.Add(pos);
	}

	private void GrabarTraza()
	{
		trazaIdActual++;
	}

	private void Awake()
	{
		nPuntosActuales = 0;
		puntos = new BufferCircular<Vector4>(nPuntosMax);
		if (pencil == null) usingPencil = false;
	}

	private void FixedUpdate()
	{
		if(trazando)
		{
			AddPoint();
			if (evenFrame) RefreshLine();
			
			evenFrame = !evenFrame;
		}
	}
}
