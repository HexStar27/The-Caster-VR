using System;
using System.Collections.Generic;
using UnityEngine;

public class SpellConstructor : MonoBehaviour
{
	public bool debugMode = false;
	[SerializeField] GameObject hechizoPrefab; //No debería ser GameObject?
	SpellStructure hechizoProcesando;
	Stack<SpellStructure> pilaDePadres;
	Modifier modificadorActual;
	[SerializeField] bool infraccion = false;

	Action<char>[] procesados;

	[SerializeField] Dictionary<char,int>[] estados;
	[SerializeField] Dictionary<char,SpellType> tipos;
	[SerializeField] Dictionary<char,Shape> efectos;

	private void Start()
	{
		//Inicialización de los diccionarios
		tipos = new Dictionary<char, SpellType>();
		foreach (var tipo in Banco.Instancia.st)
		{
			tipos.Add((char)tipo.t, tipo);
		}
		efectos = new Dictionary<char, Shape>();
		foreach (var efecto in Banco.Instancia.sh)
		{
			efectos.Add(efecto.id, efecto);
		}
	}

	private void Awake()
	{
		//Inicilización del autómata
		pilaDePadres = new Stack<SpellStructure>();
		estados = new Dictionary<char, int>[5];
		estados[0] = new Dictionary<char, int>()
		{
			{'P',1},
			{'T',2},
			{'S',3},
			{'M',4},
			{'(',0},
			{')',0}
		};
		estados[1] = new Dictionary<char, int>() //Base/TipoTrigger
		{
			{'0',0},	// None
			{'1',0},	// OnCast
			{'2',0},	// OnTrail
			{'3',0},	// OnHit
			{'4',0},	// OnStay
			{'5',0},	// OnExit
			{'6',0}		// OnFinish
		};
		estados[2] = new Dictionary<char, int>() //Tipos (SpellType)
		{
			{'*',0}
		};
		estados[3] = new Dictionary<char, int>() //Efectos (Shape)
		{
			{'*',0}
		};
		estados[4] = new Dictionary<char, int>() //Modificadores
		{
			{'*',4},
			{'-',0},
		};

		// Ejemplo de cadena:
		// "P0TFSPMHZZDDD-(P3TESEM-)"  ->
		// Pilar de fuego grande y de larga duración que crea una explosión estándar al golpear un interactuable

		procesados = new Action<char>[6];
		procesados[0] = GenerarJerarquia;
		procesados[1] = ProcesarPosicion;
		procesados[2] = ProcesarTipo;
		procesados[3] = ProcesarEfecto;
		procesados[4] = ProcesarModificador;	
	}

	public bool Construir(string conjuracion)
	{
		int estadoActual =  0;
		infraccion = false;
		pilaDePadres.Clear();
		hechizoProcesando = null;
		for (int i = 0; i < conjuracion.Length && estadoActual >= 0 && !infraccion; i++)
		{
			Procesar(estadoActual,conjuracion[i]);
			estadoActual = Siguiente(estadoActual, conjuracion[i]);
		}

		bool error = estadoActual < 0 || infraccion;
		if(error)
		{
			if (hechizoProcesando != null) Destroy(hechizoProcesando);
			pilaDePadres.Clear();
		}

		return error;
	}

	public void ConstruirReckless(string conjuracion)
	{
		int estadoActual =  0;
		infraccion = false;
		pilaDePadres.Clear();
		hechizoProcesando = null;
		for (int i = 0; i < conjuracion.Length && estadoActual >= 0 && !infraccion; i++)
		{
			Procesar(estadoActual, conjuracion[i]);
			estadoActual = Siguiente(estadoActual, conjuracion[i]);
		}

		bool error = estadoActual < 0 || infraccion;
		if (error)
		{
			if (hechizoProcesando != null) Destroy(hechizoProcesando);
			pilaDePadres.Clear();
		}
	}

	private int Siguiente(int actual, char c)
	{
		if (estados[actual].ContainsKey(c))
		{
			return estados[actual][c];
		}
		else if (estados[actual].ContainsKey('*'))
		{
			return estados[actual]['*'];
		}
		return -1;
	}


	//----------------------//
	//	PROCESADO DE DATOS	//
	//----------------------//

	private void Procesar(int e, char c)
	{
		if(e >= 0 && e < procesados.Length)
		{
			if(debugMode) Debug.Log("Procesando estado "+e+" con caracter " + c);
			procesados[e](c);
		}
	}

	private void ProcesarPosicion(char c)
	{
		//Create Base Spell 
		if (c == '0')
		{
			if(hechizoProcesando != null || pilaDePadres.Count > 0)
			{
				if (debugMode) Debug.Log("No se puede crear un hechizo primario porque ya se creó uno");
				infraccion = true;
				return;
			}
			hechizoProcesando = Instantiate(hechizoPrefab).GetComponent<SpellStructure>(); // Y ya estaría...
			hechizoProcesando.name = "H" + c + pilaDePadres.Count;
		}
		else //Set trigger spell
		{
			if (pilaDePadres.Count == 0)
			{
				if (debugMode) Debug.Log("No se puede crear un hechizo hijo si no hay un hechizo primario");
				infraccion = true;
				return;
			}

			hechizoProcesando = Instantiate(hechizoPrefab).GetComponent<SpellStructure>();
			hechizoProcesando.name = "H"+c+pilaDePadres.Count;
			SpellStructure padre = pilaDePadres.Peek();
			hechizoProcesando.Follow(padre.GetRigidbody());
			switch (c)
			{
				case '1':
					padre.OnCast.AddListener(hechizoProcesando.Lanzar);
					break;
				case '2':
					padre.OnTrail.AddListener(hechizoProcesando.Lanzar);
					break;
				case '3':
					padre.OnHit.AddListener(hechizoProcesando.Lanzar);
					break;
				case '4':
					padre.OnStay.AddListener(hechizoProcesando.Lanzar);
					break;
				case '5':
					padre.OnExit.AddListener(hechizoProcesando.Lanzar);
					break;
				case '6':
					padre.OnFinish.AddListener(hechizoProcesando.Lanzar);
					break;
				default:
					if (debugMode) Debug.Log("Posición del hechizo inexistente");
					infraccion = true;
					Destroy(hechizoProcesando);
					break;
			}
		}
	}
	private void ProcesarTipo(char c)
	{
		if (tipos == null)
		{
			if (debugMode) Debug.Log("Diccionario de tipos vacío");
			infraccion = true;
			return;
		}

		SpellType st;
		if(tipos.TryGetValue(c,out st))
		{
			hechizoProcesando.EstablecerTipo(st);
		}
		else
		{
			if (debugMode) Debug.Log("No existe ese tipo en el diccionario");
			infraccion = true;
		}
	}
	private void ProcesarEfecto(char c)
	{
		if (efectos == null)
		{
			if (debugMode) Debug.Log("Diccionario de efectos vacío");
			infraccion = true;
			return;
		}

		Shape sh;
		if (efectos.TryGetValue(c, out sh))
		{
			hechizoProcesando.EstablecerEfecto(sh);
		}
		else
		{
			if (debugMode) Debug.Log("No existe ese efecto en el diccionario");
			infraccion = true;
		}
	}
	private void ProcesarModificador(char c)
	{
		if (modificadorActual == null) modificadorActual = new Modifier();

		char upperC = char.ToUpper(c);
		if (Modifier.keyToBasicas.ContainsKey(upperC))
		{
			string elemento = Modifier.keyToBasicas[upperC];
			modificadorActual.funcionesBasicas[elemento] = upperC == c;
		}
		else if (Modifier.keyToFisicas.ContainsKey(char.ToUpper(c)))
        {
			string elemento = Modifier.keyToFisicas[upperC];
			modificadorActual.funcionesFisicas[elemento] += upperC == c ? 1 : -1;
		}
		else if (c == '-')
		{
			hechizoProcesando.EstablecerModificadores(modificadorActual);
			modificadorActual = new Modifier();
		}
	}

	private void GenerarJerarquia(char c)
	{
		if (c == '(')
		{
			if(hechizoProcesando == null)
			{
				if (debugMode) Debug.Log("No se puede generar jerarquía si no hay ningún hechizo procesandose");
				infraccion = true;
				return;
			}

			pilaDePadres.Push(hechizoProcesando);
			hechizoProcesando = null;
		}
		else if (c == ')')
		{
			if (pilaDePadres.Count == 0)
			{
				if (debugMode) Debug.Log("El padre fue a comprar leche");
				infraccion = true;
				return;
			}

			hechizoProcesando = pilaDePadres.Pop();
		}
	}
}
