using UnityEngine;
using System.Collections;

public class ClickController : MonoBehaviour 
{
	
	Vector3 dir;
	Vector3 k;
	RaycastHit hit;
	Ray ray;
	float rot = 0f;
	float speed = 0.0f;
	float speedDamper = 0.2f;
	float angle;
	float dist;
	[SerializeField]
	private GameObject pMovePoint;

	private int layerMask = 1 << 9; 
	private Vector3 gravityPower = Vector3.zero;
	private CharacterController cc;
	
	private CamController gameCam;
	[SerializeField]
	private Camera cam;
	private Animator animator;
	public bool gameController = false;			// This script will not turn run until the gameController is set to true in the GameController
	private Transform myTransform;				// this transform
	private Vector3 destinationPosition;		// The destination Point
	private float destinationDistance;			// The distance between myTransform and destinationPosition 
	public float moveSpeed;						// The Speed the character will move
	private float curHealth;
	[SerializeField]
	private float maxHealth;
	//Private Globals
	private float direction = 0.0f;
	private float charAngle = 0.0f;
	private float directionDamper = 0.25f;
	private int locomotionID = 0;
	private AnimatorStateInfo stateInfo;
	private bool isActive = false;				//controller status
	private NetworkRigidbody networkSync;		//used for Networking players
	
	
	public CamController GameCam {
		get {
			return this.gameCam;
		}
		set {
			gameCam = value;
		}
	}	
	public bool IsActive {
		get {
			return this.isActive;
		}
		set {
			isActive = value;
		}
	}
	public float Speed {
		get {
			return this.speed;
		}
	}
	public float LocomotionThreshold{get{return 0.2f;}}
	
	
	void Awake () 
	{
		//if(!networkView.isMine)
		//{
		//	enabled = false;	
		//}else{
			myTransform = transform;
			destinationPosition = myTransform.position;
			animator = transform.GetComponent<Animator>();
			if(animator.layerCount >= 2)
				animator.SetLayerWeight(1,1);
			if(this.gameObject.tag == "Player") //set the Player to the default controller at game start
				isActive = true;
			networkSync = transform.GetComponent<NetworkRigidbody>();
			//Hash Animation names for Performance
			locomotionID = Animator.StringToHash("Base Layer.Locomotion");
		//}
	}

	
	void Update ()
	{
		if(Input.GetMouseButtonDown(1))
		{
			Ray pRay = cam.ScreenPointToRay(Input.mousePosition);
			RaycastHit pHit;
	        Physics.Raycast(cam.transform.position,pRay.direction,out pHit,1000f,layerMask);
			Instantiate(pMovePoint,pHit.point,Quaternion.identity);
			Debug.Log ("p");
		}	
	}
	
		
	void FixedUpdate () 
	{
		if (Input.GetMouseButton(1))
	    {
	        // Cast ray from mouse to point on terrain and get location
	        ray = cam.ScreenPointToRay(Input.mousePosition);
	        Physics.Raycast(cam.transform.position,ray.direction,out hit,1000f,layerMask);
	 
	        // Define direction to move
	        dir = hit.point - transform.position;
			dist = Vector3.Distance(hit.point,transform.position);
	        // Get rotation smoothly
	        angle = Vector3.Angle(dir, transform.forward);
	        k = Vector3.Cross(transform.forward, dir);
	        k.Normalize();
	        rot = k[1] * (angle*2.5f);
			speed = dist;
			animator.SetFloat("Speed", speed);
			animator.SetFloat("Angle",rot);			
			if(speed > 0.05f && (rot>=225f || rot<= -225f) && !animator.GetBool("Pivot"))
				{animator.SetBool("Pivot",true);
			}else{
				animator.SetBool("Pivot",false);}
			animator.SetFloat("Direction",rot,directionDamper, Time.deltaTime);
	    }
		else
	    {
			destinationPosition = hit.point;
			StartCoroutine(MoveToPoint(destinationPosition));
	    }
	}	
	
	
	IEnumerator MoveToPoint(Vector3 dest)
	{
		while(dist > 0.1f && !Input.GetMouseButtonDown(1))
		{
			// Define direction to move
	        dir = dest - transform.position;
			dist = Vector3.Distance(dest,transform.position);
	        // Get rotation smoothly
	        angle = Vector3.Angle(dir, transform.forward);
	        k = Vector3.Cross(transform.forward, dir);
	        k.Normalize();
	        rot = k[1] * (angle*2.5f);
			speed = dist;
			animator.SetFloat("Speed", speed);
			animator.SetFloat("Angle",rot);			
			if(speed > 0.05f && (rot>=225f || rot<= -225f))
				{animator.SetBool("Pivot",true);
			}else{
				animator.SetBool("Pivot",false);}
			animator.SetFloat("Direction",rot,directionDamper, Time.deltaTime);
			yield return null;
		}
			speed = Mathf.Lerp (speed,0f,speedDamper*Time.deltaTime);
			animator.SetFloat("Speed", speed);
	}
}