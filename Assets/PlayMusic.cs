using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayMusic : MonoBehaviour
{
	string playerTag = "Player";
	public AudioSource a;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag(playerTag))
		{
			a.enabled = true;
			a.Play();
		}
	}
}
