using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BancoMateriales : MonoBehaviour
{
	public PhysicMaterial[] materialesFisicos;
	public string[] nombres;

	private Dictionary<string,PhysicMaterial> banco = new Dictionary<string, PhysicMaterial>();

	public static BancoMateriales Instancia { get; set; }

	public void Awake()
	{
		Instancia = this;
		if (nombres.Length < materialesFisicos.Length) Debug.LogError("No hay suficientes nombres para los materiales");
		for (int i = 0; i < materialesFisicos.Length; i++)
		{
			banco.Add(nombres[i], materialesFisicos[i]);
		}
	}

	public PhysicMaterial GetByName(string mat)
	{
		return banco[mat];
	}
}
