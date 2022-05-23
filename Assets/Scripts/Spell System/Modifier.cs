using System.Collections.Generic;

public class Modifier
{
	public Dictionary<string,bool> funcionesBasicas;
	public Dictionary<string,float> funcionesFisicas;

	public Modifier()
	{
		funcionesBasicas = new Dictionary<string, bool>();
		funcionesFisicas = new Dictionary<string, float>();

		funcionesBasicas.Add("Rebote", false);
		funcionesBasicas.Add("Gravity", false);
		funcionesBasicas.Add("Trigger", true);
		funcionesBasicas.Add("DieOnHit", true);
		funcionesBasicas.Add("Trail", false);
		funcionesBasicas.Add("OnHit", false);
		funcionesBasicas.Add("OnStay", false);
		funcionesBasicas.Add("OnExit", false);

		funcionesFisicas.Add("Size", 1);
		funcionesFisicas.Add("Duration", 5);
		funcionesFisicas.Add("Repeater", 0);
		funcionesFisicas.Add("Vel",5);
		funcionesFisicas.Add("Acc", 0);
	}

	public readonly static Dictionary<char, string> keyToBasicas = new Dictionary<char, string>()
	{
		{'B',"Rebote"},// Básicas
		{'G',"Gravity"},
		{'T',"Trigger"},
		{'F',"DieOnHit"},
		{'L',"Trail"},
		{'H',"OnHit"},
		{'S',"OnStay"},
		{'E',"OnExit"}
	};
	public readonly static Dictionary<char, string> keyToFisicas = new Dictionary<char, string>()
	{
		{'Z',"Size"},// Físicas
		{'D',"Duration"},
		{'R',"Repeater"},
		{'V',"Vel"},
		{'A',"Acc"}
	};
}