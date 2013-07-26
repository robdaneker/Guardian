
	var gameName : String = "TEST";
	private var refreshing : boolean = false;
	private var hostData : HostData[];
	private var playerNumber : int[];//used to assign player numbers to GameControllers. #0 is saved to track open player slots.
	var gameControllerPrefab : GameObject;
	var playerPrefab : GameObject;
	var botPrefab : GameObject;
	var camRigPrefab : GameObject;
	var spawn : Transform;
	var maxPlayers : int;
	//public GameController[] gControllers : GameController;
	//public GameController.AllianceStatus[] alliances;

function Start()
{
	playerNumber = new int[maxPlayers + 1];
	//gControllers = new GameController[maxPlayers + 1];
}


function Update () 
{
	if(refreshing) 
	{
		if(MasterServer.PollHostList().Length > 0) 
		{           
			refreshing = false;
			hostData = MasterServer.PollHostList();
		}
	}
}


function startServer () 
{
	Network.InitializeServer(32,25001, !Network.HavePublicAddress);
	MasterServer.RegisterHost(gameName, "Tutorial Game", " this is a tutorial");
}



function OnServerInitialized () 
{
	spawnPlayer();
}

function OnConnectedToServer () 
{
	spawnPlayer();
}


function spawnPlayer () 
{
	Network.Instantiate(playerPrefab, spawn.position, Quaternion.identity, 0);
}


function OnMasterServerEvent(mse:MasterServerEvent) 
{
	if(mse == MasterServerEvent.RegistrationSucceeded) 
	{
		Debug.Log("Registered Server");
	}    
}


function refreshHostList () 
{
	MasterServer.RequestHostList(gameName);
	refreshing = true;
}

function OnGUI () 
{
	if(!Network.isClient && !Network.isServer) 
	{
		if (GUI.Button(Rect(Screen.width/2,Screen.height/2,100,20),"Start Server"))
			startServer();
	}
	if (GUI.Button(Rect(Screen.width/2,Screen.height/2 + 30,100,20),"Refresh Hosts"))
		refreshHostList();
	if(hostData) 
	{
		for(var i:int = 0; i<hostData.length; i++) 
		{
			if(GUI.Button(Rect(Screen.width/2,Screen.height/2 + 60,100,20),hostData[i].gameName))
				Network.Connect(hostData[i]);
		}
	}
}