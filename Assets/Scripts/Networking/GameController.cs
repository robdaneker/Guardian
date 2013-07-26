using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour 
{
	[SerializeField]
	private CamControllerClick gameCam;
	[SerializeField]
	private GameObject pSelectorArt;
	private GameObject startCam;
	public GameObject[] cursorArtList;
	public GameObject curCursorArt;
	public GameObject cursorArtTest;
	public AllianceStatus[] alliances;
	//Private
	private Transform playerCam;
	private Transform botCam;
	private Transform player;
	private Transform bot;
	private Camera cam;
	private ClickNavAgent playerController;
	private ClickNavAgent_bot botController;
	private GameObject pSelector;
	private Vector3 sPoint;
	private int groundMask = (1 << 9);
	[SerializeField]
	private int playerNumber;
	private NetworkManager netManager;
	
	public enum AllianceStatus
	{
		allied,
		neutral,
		enemy
	}

	public ClickNavAgent PlayerController {
		get {
			return this.playerController;
		}
	}
	public ClickNavAgent_bot BotController {
		get {
			return this.botController;
		}
	}
	public int PlayerNumber {
		get {
			return this.playerNumber;
		}
		set {
			playerNumber = value;
		}
	}	
	
	
	void Start()
	{

	}
	
	
	void Awake () 
	{
		netManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>();
		alliances = new AllianceStatus[netManager.maxPlayers+1];
		//Set up Local Variables for each player
		if(!networkView.isMine)
		{
			enabled = false;
		}else{

			GameObject[] playerGroup = GameObject.FindGameObjectsWithTag("Player");
			int playerCount = GameObject.FindGameObjectsWithTag("Player").Length;
			if(playerCount>0)
			{
				foreach(GameObject p in playerGroup)
				{
					if(p.networkView.isMine)
					{
						player = p.transform;
						playerController = player.GetComponent<ClickNavAgent>();
						playerCam = player.FindChild("CameraOrigin");
						playerController.GameController = this.gameObject.GetComponent<GameController>();
					}else{
						p.GetComponent<ClickNavAgent>().enabled = false;
						Debug.Log(playerNumber);
					}
				}
				GameObject[] botGroup = GameObject.FindGameObjectsWithTag("Bot");
				foreach(GameObject b in botGroup)
				{
					if(b.networkView.isMine)
					{
						bot = b.transform;
						botController = bot.GetComponent<ClickNavAgent_bot>();
						botCam = bot.FindChild("CameraOrigin");
						playerController.bot = b.transform;
						botController.player = player;
						botController.GameController = this.gameObject.GetComponent<GameController>();
					}else{
						b.GetComponent<ClickNavAgent>().enabled = false;
					}
				}
				GameObject[] cameraGroup = GameObject.FindGameObjectsWithTag("MainCamera");
				foreach(GameObject c in cameraGroup)
				{
					if(c.transform.parent.networkView.isMine)
					{
						gameCam = c.GetComponent<CamControllerClick>();
						playerController.Cam = c.GetComponent<Camera>();
						botController.Cam = c.GetComponent<Camera>();
						cam = c.GetComponent<Camera>();
					}
				}
				playerController.GameCam = gameCam;
				botController.GameCam = gameCam;
				gameCam.Controller = playerController;
				netManager.LocalController = this.transform.GetComponent<GameController>();
				startCam = GameObject.FindGameObjectWithTag("StartCam");
				startCam.SetActive(false);
			}
		}
	}
	
	
	void Update () 
	{
	//////////////////
	//INPUT CONTROLS//
	//////////////////
		//Switch Controller if Tab button is Down
		if(Input.GetButtonDown("Switch Controller"))
		{
			if(gameCam.CameraOrigin == playerCam)
			{
				gameCam.CameraOrigin = botCam;
				botController.IsActive = true;
				playerController.IsActive = false;
				sPoint = bot.position;
			}else{
				gameCam.CameraOrigin = playerCam;
				playerController.IsActive = true;
				botController.IsActive = false;
				sPoint = player.position;
				DestroyImmediate(curCursorArt);
			}
			playerController.DestinationPosition = player.position;
			botController.DestinationPosition = bot.position;
			sPoint.y = sPoint.y + 0.1f;
			if(pSelector)
				DestroyImmediate(pSelector);
			pSelector = Instantiate(pSelectorArt,sPoint,Quaternion.identity) as GameObject;
		}
	}
	
	void OnGUI()
	{
		if(curCursorArt)
		{
			Ray cRay = cam.ScreenPointToRay(Input.mousePosition);
			RaycastHit cHit;
			if(Physics.Raycast(cam.transform.position,cRay.direction,out cHit,1000f,groundMask))
			{	
				Vector3 iPoint = cHit.point;
				iPoint.y = iPoint.y + 0.1f;
				curCursorArt.transform.position = iPoint;
				curCursorArt.GetComponent<ParticleSystem>().startSize = 2*botController.CurAbility.effectRange;
			}
		}
	}
}
