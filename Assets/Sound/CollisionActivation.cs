using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionActivation : MonoBehaviour
{
	string playerTag = "Player";
	public Animator anim;
	public GameObject obj;

	private void OnTriggerEnter(Collider other)
	{
		if(other.CompareTag(playerTag))
		{
			anim.SetTrigger("Push");
			obj.SetActive(false);
		}
	}
}
