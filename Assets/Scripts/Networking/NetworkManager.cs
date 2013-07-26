using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {

	private float btnX = Screen.width*0.005f;
	private float btnY = Screen.width*0.005f;
	private float btnH = Screen.width*0.02f;
	private float btnW = Screen.width*0.1f;
	private bool refreshing = false;
	private HostData[] hostData;
	private bool hostDatabool = false;
	private string gameName = "DREAMERTEST";
	[SerializeField]
	private int[] playerNumber;//used to assign player numbers to GameControllers. #0 is saved to track open player slots.
	public GameObject gameControllerPrefab;
	public GameObject playerPrefab;
	public GameObject botPrefab;
	public GameObject camRigPrefab;
	public Transform spawn;
	public int maxPlayers;
	public GameController[] gControllers;
	private GameController.AllianceStatus[] alliances;
	private GameController localController;

	public GameController LocalController {
		get {
			return this.localController;
		}
		set {
			localController = value;
		}
	}	
	
	
	void Start()
	{
		playerNumber = new int[maxPlayers + 1];
		gControllers = new GameController[maxPlayers + 1];
	}
	
	
	void startServer()
	{
		bool useNat = !Network.HavePublicAddress();
		Network.InitializeServer(32,25000,useNat);
		MasterServer.RegisterHost(gameName,"TestGame","Join to test pl0x");
	}
	
	
	void refreshHostList()
	{
		MasterServer.RequestHostList(gameName);
		refreshing = true;
	}
	
	
	void OnServerInitialized()
	{
		SpawnPlayer();
	}
	
	
	void OnConnectedToServer()
	{
		SpawnPlayer();
	}
	
	
	public void SpawnPlayer()
	{
		GameObject player = Network.Instantiate(playerPrefab,spawn.position,Quaternion.identity,0) as GameObject;
		GameObject bot = Network.Instantiate(botPrefab,spawn.position,Quaternion.identity,0) as GameObject;
		GameObject cam = Network.Instantiate(camRigPrefab,spawn.position,Quaternion.identity,0) as GameObject;
		GameObject gameController = Network.Instantiate(gameControllerPrefab,spawn.position,Quaternion.identity,0) as GameObject;
		for(int i=1; i <= (maxPlayers+1);i++)
		{
			if(playerNumber[i]==0)
			{
				gameController.GetComponent<GameController>().PlayerNumber=i;
				player.GetComponent<UnitData>().playerNumber=i;
				bot.GetComponent<UnitData>().playerNumber=i;
				playerNumber[i]=i;
				gControllers[i]=gameController.GetComponent<GameController>();
				alliances = gameController.GetComponent<GameController>().alliances;
				alliances[0]= GameController.AllianceStatus.enemy;
				for(int p=1;p<=maxPlayers;p++)
				{
					if(i==p)
						{alliances[p]= GameController.AllianceStatus.allied;
					}else{alliances[p]= GameController.AllianceStatus.neutral;}
				}
				break;
			}
		}
	}
	
	
	void OnMasterServerEvent(MasterServerEvent mse)
	{
		if(mse == MasterServerEvent.RegistrationSucceeded)
			Debug.Log("Registration Succeeded!");
	}
	
	
	void Update()
	{
		if(refreshing)
		{
			if(MasterServer.PollHostList().Length > 0)
			{
				refreshing = false;
				Debug.Log(MasterServer.PollHostList().Length);
				hostData = MasterServer.PollHostList();
				hostDatabool = true;
			}
		}
	}
	
	
	void OnGUI () 
	{ 
		if(!Network.isClient && !Network.isServer) 
		{ 
			if (GUI.Button(new Rect(Screen.width/2f,Screen.height/2f,100f,20f),"Start Server"))
	        	startServer();
	 		if (GUI.Button(new Rect(Screen.width/2f,Screen.height/2f + 30f,100f,20f),"Refresh Hosts"))
	       		refreshHostList();
	        if(hostDatabool) 
	        {
	        	for(int i = 0; i<hostData.Length; i++) 
	        	{
	       			if(GUI.Button(new Rect(Screen.width/2,Screen.height/2f + 60f,100f,20f),hostData[i].gameName))
	                	Network.Connect(hostData[i]);
	          	}
	 		}
		} 
	}
	
	public int GetTriggeringPlayer()
	{
		for(int pN = 0;pN<=gControllers.Length;pN++)
		{
			if(gControllers[pN]!=null)
			{
				if (gControllers[pN].transform.networkView.isMine)
				{
					int number = gControllers[pN].PlayerNumber;
					return number;
					break;
				}
			}
		}
		int noNumber = 0;
		return noNumber;
	}
}