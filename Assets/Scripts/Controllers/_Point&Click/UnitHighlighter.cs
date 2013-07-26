using UnityEngine;
using System.Collections;

public class UnitHighlighter : MonoBehaviour 
{

	private Color[] startColor;
	private Transform[] tGroup;
	private UnitData uData;
	private NetworkManager nManager;
	private Color allyColor = Color.green;
	private Color enemyColor = Color.red;
	private Color neutColor = Color.yellow;
	private Color hColor;

	
	void Awake () 
	{
		tGroup = this.transform.GetComponentsInChildren<Transform>();
		startColor = new Color[tGroup.Length];
		nManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>();
		uData = this.transform.GetComponent<UnitData>();
		int i = 0;
		foreach(Transform t in tGroup)
		{
			if(t.renderer)
			{
				startColor[i] = t.renderer.material.color;
				i++;
			}
		}
	}
	

	void OnMouseEnter()
	{
		Debug.Log("mouseOver");
		int i = 0;
		tGroup = this.transform.GetComponentsInChildren<Transform>();
		
		if(nManager.LocalController.alliances[uData.playerNumber]==GameController.AllianceStatus.allied)
			hColor = allyColor;
		if(nManager.LocalController.alliances[uData.playerNumber]==GameController.AllianceStatus.enemy)
			hColor = enemyColor;
		if(nManager.LocalController.alliances[uData.playerNumber]==GameController.AllianceStatus.neutral)
			hColor = neutColor;
		foreach(Transform t in tGroup)
		{
			if(t.renderer)
			{
				startColor[i] = t.renderer.material.color;
				i++;
				t.renderer.material.color = hColor;
			}
		}
	}
	

	void OnMouseExit()
	{
		int i = 0;
		tGroup = this.transform.GetComponentsInChildren<Transform>();
		foreach(Transform t in tGroup)
		{
			if(t.renderer)
			{
				t.renderer.material.color = startColor[i];
				i++;
			}
		}
	}
}
