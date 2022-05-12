using UnityEngine;
using UnityEngine.VFX;

[CreateAssetMenu(fileName = "SpellType", menuName = "Spells/Type")]
public class SpellType : ScriptableObject
{
	public enum Type
	{
		Fuego			= 'F',
		Agua			= 'A',
		Viento			= 'V',
		Tierra			= 'T',
		Desintegracion	= 'D',
		Explosion		= 'E',
		Puro			= 'P'
	};
	public Type t;
	public Material material;
	public VisualEffectAsset[] vfxa;
	public bool[] usarVFXPreCasteo;
}
