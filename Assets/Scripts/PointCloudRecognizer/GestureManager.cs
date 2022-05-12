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
	BufferCircular<Vector3> puntos;
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

	public void GrabarTraza()
	{
		trazaIdActual++;
	}

	public void TerminarGesto()
	{
		trazaIdActual = 0;

		//Transformar buffer en "Gesture" y limpiar buffer
		Point[] p = new Point[nPuntosActuales];
		for(int i = 0; i < nPuntosActuales; i++)
		{
			Vector3 a = puntos.ValueAt_FAST_AND_DANGER(i);
			p[i] = new Point(a.x, a.y, 0, (int)a.z);
		}
		gestoActual = new Gesture(p,gestureName);

		puntos.Clear();
	}

	public string Recognize()
	{
		return QPointCloudRecognizer.Classify(gestoActual, patrones);
	}

	public void ExecuteGesture()
	{
		onGestureDic[patternNames.BinarySearch(QPointCloudRecognizer.Classify(gestoActual, patrones))].Invoke();
	}

	public void RecognizeDebug()
	{
		Debug.Log(Recognize());
	}

	private void AddPoint()
	{
		Vector3 pos;
		if (usingPencil) pos = pencil.position;
		else
		{
			nPuntosActuales++;
			if (nPuntosActuales >= nPuntosMax) nPuntosActuales = nPuntosMax;
			pos = Input.mousePosition;
			pos.z = trazaIdActual;
		}
		puntos.Add(pos);
	}

	public void RefreshLine()
	{
		line.positionCount = nPuntosActuales;
		Vector3[] a = puntos.ToArray();
		for(int i = 0; i < a.Length; i++)
		{
			a[i] = camara.ScreenToWorldPoint(new Vector3(a[i].x, a[i].y, 1));
		}
		line.SetPositions(a);
	}

	public void AddPattern()
	{
		listaAuxiliarPatrones.Add(gestoActual);
		patrones = listaAuxiliarPatrones.ToArray();

		Clean();
	}

	public void Clean()
	{
		line.positionCount = 0;
		puntos.Clear();
		trazando = false;
		trazaIdActual = 0;
		nPuntosActuales = 0;
	}

	private void Awake()
	{
		nPuntosActuales = 0;
		puntos = new BufferCircular<Vector3>(nPuntosMax);
		if (pencil == null) usingPencil = false;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			trazando = true;
		}
		else if (Input.GetKeyUp(KeyCode.Mouse0))
		{
			trazando = false;
			GrabarTraza();
		}

		if(Input.GetKeyDown(KeyCode.Space))
		{
			TerminarGesto();
		}
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
