/// TODO: 
/// -Terminar "Establecer Modificadores"
/// -Implementar las funciones de Aplicación de Tipos
/// -Ver si es necesario añadir los OnCollision****

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(Animator)), RequireComponent(typeof(Rigidbody))]
public class SpellV1 : SpellStructure
{
	public static string tagInteractuable = "Interactuable";

	[SerializeField] SpellType _tipo;
	[SerializeField] Shape _efecto;
	/// El transform de un GameObject hijo de este, sirve para que unity se encargue de calcular dinámicamente 
	/// de dónde saldrán los hechizos activados por un trigger de este objeto.
	[SerializeField] Transform _referenciaCasteoPorTrigger;
	[SerializeField] VisualEffect[] _vfx = new VisualEffect[0];
	bool[] _useVFXPreCasted = new bool[0];
	Action<Collider> _efectoTipo = (Collider c)=>{ };

	[SerializeField] Animator _anim;
	[SerializeField] MeshRenderer _meshRen;
	[SerializeField] MeshFilter _meshFil;
	[SerializeField] MeshCollider _meshCol;

	Vector3 orientation = Vector3.zero;
	protected bool willBeTrigger = false;

	public bool cast = false;

	public override void Lanzar()
	{
		int n = _vfx.Length;
		for (int i = 0; i < n; i++)
		{
			_vfx[i].enabled = true;
		}
		base.Lanzar();
		_meshCol.isTrigger = willBeTrigger;
		//Debug.Log("2" + _meshCol.isTrigger);
	}

	public override void EstablecerModificadores(Modifier modificadores)
	{
		if(modificadores.funcionesFisicas.TryGetValue("Size",out float s))
		{
			trans.localScale = s * Vector3.one;
		}
		if(modificadores.funcionesFisicas.TryGetValue("Duration", out s))
		{
			duracion = s;
		}
		if (modificadores.funcionesFisicas.TryGetValue("Repeater", out s))
		{
			activacionesDesdeSpawn = s;
		}
		if (modificadores.funcionesFisicas.TryGetValue("Vel", out s))
		{
			velocidadInicial = s;
		}
		if (modificadores.funcionesFisicas.TryGetValue("Acc", out s))
		{
			aceleracion = s;
		}

		if(modificadores.funcionesBasicas.TryGetValue("Rebote",out bool v))
		{
			if(v) _meshCol.sharedMaterial = BancoMateriales.Instancia.GetByName("Rebote");
		}
		if (modificadores.funcionesBasicas.TryGetValue("Gravity", out v))
		{
			_rb.useGravity = v;
		}
		if (modificadores.funcionesBasicas.TryGetValue("Trigger", out v))
		{
			willBeTrigger = v; //No hace nada...
			_meshCol.isTrigger = v;
		}
		if (modificadores.funcionesBasicas.TryGetValue("DieOnHit", out v))
		{
			dieOnHit = v;
		}
		if (modificadores.funcionesBasicas.TryGetValue("Trail", out v))
		{
			activateTrail = v;
		}
		if (modificadores.funcionesBasicas.TryGetValue("OnHit", out v))
		{
			enableCollisionEvents[0] = v;
		}
		if (modificadores.funcionesBasicas.TryGetValue("OnStay", out v))
		{
			enableCollisionEvents[1] = v;
		}
		if (modificadores.funcionesBasicas.TryGetValue("OnExit", out v))
		{
			enableCollisionEvents[2] = v;
		}
	}

	public override void EstablecerEfecto(Shape efecto)
	{
		//base.EstablecerEfecto(efecto);
		this._efecto = efecto;

		_referenciaCasteoPorTrigger.localPosition = efecto.nextCastPosition;
		//if (efecto.animController != null) 
			//_anim.runtimeAnimatorController = efecto.animController;
		if (efecto.mesh != null) _meshFil.mesh = efecto.mesh;
		if (efecto.meshColision != null) _meshCol.sharedMesh = efecto.mesh;
	}

	public override void EstablecerTipo(SpellType tipo)
	{
		//base.EstablecerTipo(tipo);
		this._tipo = tipo;
		if (tipo.material != null) _meshRen.material = tipo.material;
		if(tipo.vfxa != null)
		{
			int n = tipo.vfxa.Length;
			_vfx = new VisualEffect[n];
			for(int i = 0; i < n; i++)
			{
				_vfx[i] = gameObject.AddComponent<VisualEffect>();
				_vfx[i].enabled = false;
			}

			_useVFXPreCasted = new bool[n];
			if (tipo.usarVFXPreCasteo != null)
			{
				if (tipo.usarVFXPreCasteo.Length > n) Debug.LogError("Hay más estados que efectos visuales en el SpellType..." + tipo.ToString());
				else for(int i = 0; i < n; i++) _useVFXPreCasted[i] = tipo.usarVFXPreCasteo[i];
			}
		}
		EstablecerEfectoDeTipo(tipo.t);
	}

	protected override void SafeDestroy(UnityEngine.Object obj)
	{
		StartCoroutine(EsperarVFX(obj));
	}

	private IEnumerator EsperarVFX(UnityEngine.Object obj)
	{
		if (_vfx.Length <= 0) yield return null;
		else
		{
			WaitWhile esperar = new WaitWhile(()=>{return _vfx[0].aliveParticleCount > 0; });
			yield return esperar;
			Destroy(obj);
		}
	}

	public override void Follow(Rigidbody padreFicticio)
	{
		base.Follow(padreFicticio);
		if(padreFicticio != null)
		{
			for(int i = 0; i < _vfx.Length; i++)
			{
				if(_useVFXPreCasted[i])
				{
					_vfx[i].enabled = true;
				}
			}
		}
		_meshCol.isTrigger = following || isTrigger;
		Debug.Log("1" + _meshCol.isTrigger);
	}

	private void AssingSpeedOnCast()
	{
		if (_rb.velocity != Vector3.zero)
		{
			_rb.velocity = velocidadInicial * _rb.velocity.normalized;
		}
		else
		{
			if (fatherT != null) orientation = fatherT.velocity.normalized;
			else orientation = trans.localRotation.eulerAngles;
			_rb.velocity = velocidadInicial * orientation;
		}
	}

	protected override void Start()
	{
		OnCast.AddListener(AssingSpeedOnCast);
	}

	protected virtual void FixedUpdate()
	{
		if(cast) StartCoroutine(Ejecucion());
		if(casted && aceleracion != 0)
		{
			_rb.AddForce(Time.fixedDeltaTime * aceleracion * orientation);
		}
	}

	protected override void OnTriggerEnter(Collider other)
	{
		base.OnTriggerEnter(other);
		if (other.CompareTag(tagInteractuable))
        {
			_efectoTipo(other);
		}
		if (dieOnHit && casted) Destroy(gameObject);
	}
	protected override void OnTriggerStay(Collider other)
	{
		base.OnTriggerStay(other);
		if (other.CompareTag(tagInteractuable)) _efectoTipo(other);
	}
	protected override void OnTriggerExit(Collider other)
	{
		base.OnTriggerExit(other);
		if (other.CompareTag(tagInteractuable)) _efectoTipo(other);
	}

	///                                     ///
	/// Funciones para Aplicación de Tipos  ///
	///                                     ///
	protected void Agua(Collider col)
	{
		ObjetoInteractuable oi= col.GetComponent<ObjetoInteractuable>();
		oi.interacciones["Mojar"](50);
	}
	protected void Fuego(Collider col)
	{
		ObjetoInteractuable oi = col.GetComponent<ObjetoInteractuable>();
		oi.interacciones["Quemar"](50);
	}
	protected void Viento(Collider col)
	{
		ObjetoInteractuable oi = col.GetComponent<ObjetoInteractuable>();
		oi.interacciones["Romper"](1);
		Vector3 dir = (col.transform.position - trans.position).normalized;
		col.attachedRigidbody.AddForce(dir*200);
	}
	protected void Tierra(Collider col)
	{
		ObjetoInteractuable oi = col.GetComponent<ObjetoInteractuable>();
		oi.interacciones["Romper"](5);
	}
	protected void Desintegracion(Collider col)
	{
		ObjetoInteractuable oi = col.GetComponent<ObjetoInteractuable>();
		oi.interacciones["Romper"](1000);
	}
	protected void Explosion(Collider col)
	{
		ObjetoInteractuable oi = col.GetComponent<ObjetoInteractuable>();
		float dist = (trans.position - col.ClosestPoint(trans.position)).magnitude + 1;
		oi.interacciones["Romper"](10/dist);
	}
	protected void Puro(Collider col)
	{
		ObjetoInteractuable oi = col.GetComponent<ObjetoInteractuable>();
		oi.interacciones["Romper"](10);
	}
	protected void EstablecerEfectoDeTipo(SpellType.Type tipo)
	{
		switch (tipo)
		{
			case SpellType.Type.Fuego: 
				_efectoTipo = Fuego;
				break;
			case SpellType.Type.Agua: 
				_efectoTipo = Agua;
				break;
			case SpellType.Type.Viento: 
				_efectoTipo = Viento;
				break;
			case SpellType.Type.Tierra:
				_efectoTipo = Tierra;
				break;
			case SpellType.Type.Desintegracion:
				_efectoTipo = Desintegracion;
				break;
			case SpellType.Type.Explosion:
				_efectoTipo = Explosion;
				break;
			case SpellType.Type.Puro:
				_efectoTipo = Puro;
				break;
		}
	}
	
}
