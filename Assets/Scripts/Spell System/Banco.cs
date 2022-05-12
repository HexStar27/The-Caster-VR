using UnityEngine;

public class Banco : MonoBehaviour
{
	public SpellType[] st;
	public Shape[] sh;

	public static Banco Instancia { get; set; }

	public void Awake()
	{
		Instancia = this;
	}
}
