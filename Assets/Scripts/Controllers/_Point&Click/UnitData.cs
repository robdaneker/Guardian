using UnityEngine;
using System.Collections;

public class UnitData : MonoBehaviour 
{
	public float maxHealth;
	[SerializeField]
	private float curHealth;
	private GameController gameController;
	public int playerNumber;

	public GameController GameController {
		get {
			return this.gameController;
		}
	}	

	void Awake () 
	{
		curHealth = maxHealth;
		//GetOwner(gameObject,out playerNumber);		
	}
	
	
	public int GetOwner(GameObject gO,out int playerNumber)
	{
		GameObject[] gC = GameObject.FindGameObjectsWithTag("GameController");
		foreach(GameObject g in gC)
		{
			if (g.networkView.isMine && gO.networkView.isMine) 
			{
				return playerNumber = g.GetComponent<GameController>().PlayerNumber;
				break;
			}
		}
		return playerNumber = 0;
	}
	
	
	[RPC]
	public void ApplyDamage(float baseDamage)
	{
		curHealth -= baseDamage;
		if(curHealth<=0)
			networkView.RPC("Death",RPCMode.AllBuffered);
	}
	
	[RPC]
	private void Death()
	{
		DestroyImmediate(this.gameObject);	
	}
}
