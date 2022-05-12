using UnityEditor.Animations;
using UnityEngine;

[CreateAssetMenu(fileName ="Shape",menuName ="Spells/Shape")]
public class Shape : ScriptableObject
{
	public char id;
	public Mesh mesh;
	public Mesh meshColision;
	public Vector3 nextCastPosition = Vector3.zero;
	public AnimatorController animController;
}
