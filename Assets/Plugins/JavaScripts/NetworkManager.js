#pragma strict

private var btnX = Screen.width*0.005;
private var btnY = Screen.width*0.005;
private var btnH = Screen.width*0.02;
private var btnW = Screen.width*0.1;
private var refreshing : boolean = false;
private var hostData:HostData[];
var gameName:String = "DREAMERTEST";
var gameControllerPrefab:GameObject;
var playerPrefab:GameObject;
var botPrefab:GameObject;
var camRigPrefab:GameObject;
var spawn:Transform;


function startServer()
{
	Network.InitializeServer(32,25000,!Network.HavePublicAddress);
	MasterServer.RegisterHost(gameName,"TestGame","Join to test pl0x");
}


function refreshHostList()
{
	MasterServer.RequestHostList(gameName);
	refreshing = true;
}


function OnServerInitialized()
{
	SpawnPlayer();
}


function OnConnectedToServer()
{
	SpawnPlayer();
}


function SpawnPlayer()
{
	Network.Instantiate(playerPrefab,spawn.position,Quaternion.identity,0);
	Network.Instantiate(botPrefab,spawn.position,Quaternion.identity,0);
	Network.Instantiate(camRigPrefab,spawn.position,Quaternion.identity,0);
	var gameController = Network.Instantiate(gameControllerPrefab,spawn.position,Quaternion.identity,0) as GameObject;
	if(gameController.networkView.isMine)
	{
		
	}
}


function OnMasterServerEvent(mse:MasterServerEvent)
{
	//if(mse == MasterServerEvent.RegistrationSucceeded)
}


function Update()
{
	if(refreshing)
	{
		if(MasterServer.PollHostList().Length > 0)
		{
			refreshing = false;
			Debug.Log(MasterServer.PollHostList().Length);
			hostData = MasterServer.PollHostList();
			Debug.Log(MasterServer.PollHostList());
		}
	}
}


function OnGUI () 
{ 
	if(!Network.isClient && !Network.isServer) 
	{ 
		if (GUI.Button(Rect(Screen.width/2,Screen.height/2,100,20),"Start Server"))
        	startServer();
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
}
 