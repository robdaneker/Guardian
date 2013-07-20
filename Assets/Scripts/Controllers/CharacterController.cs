using UnityEngine;
using System.Collections;

public class CharacterController : MonoBehaviour 
{
	[SerializeField]
	private CameraController gameCam;
	private Animator animator;
	private float directionDamper = .25f;
	[SerializeField]
	private float directionSpeed;
	[SerializeField]
	private float rotationDegPerSec = 120f;
	//Private Globals
	private float speed = 0.0f;
	private float direction = 0.0f;
	private float charAngle = 0.0f;
	private float horizontal = 0.0f;
	private float vertical = 0.0f;
	private int locomotionID = 0;
	private AnimatorStateInfo stateInfo;

	public float Speed {
		get {
			return this.speed;
		}
	}
	
	public float LocomotionThreshold{get{return 0.2f;}}
	
	void Start () 
	{
		animator = GetComponent<Animator>();
		if(animator.layerCount >= 2)
			animator.SetLayerWeight(1,1);
		//Hash Animation names for Performance
		locomotionID = Animator.StringToHash("Base Layer.Locomotion");
	}
	

	void Update () 
	{
		if(animator)
		{
			stateInfo = animator.GetCurrentAnimatorStateInfo(0);
			if(Input.GetButton("Jump"))
			{
				animator.SetBool("Jump",true);
			}else{
				animator.SetBool("Jump",false);
			}
		//Step 1: Pull out Input Values	
			horizontal = Input.GetAxis("Horizontal");
			vertical = Input.GetAxis("Vertical");
			direction = 0f;
			charAngle = 0f;
		//Step 2: Translate Input Controls to World Space from Camera Position
			InputtoWorldSpace(this.transform,gameCam.transform,ref direction, ref speed, ref charAngle);
		//Step 3: Send Input Values to Animator
			if(inLocomotion() && (charAngle>=90 || charAngle<= -90))
			{
				animator.SetBool("Pivot",true);
				animator.SetFloat("Angle",charAngle);
			}else{
				animator.SetBool("Pivot",false);
				animator.SetFloat("Angle",charAngle);
			}
			charAngle=0f;
			animator.SetFloat("Speed", speed);
			animator.SetFloat("Direction",direction,directionDamper, Time.deltaTime);
		}
	}
	
	
	void FixedUpdate()
	{
		//Rotate the Player if moving horizontally
		if(inLocomotion() && ((direction >= 0 && horizontal >= 0) || (direction < 0 && horizontal < 0)))
		{
			Vector3 rotationAmount = Vector3.Lerp(Vector3.zero, new Vector3(0f, rotationDegPerSec * (horizontal < 0f ? -1f : 1f), 0f), Mathf.Abs(horizontal));
			Quaternion deltaRotation = Quaternion.Euler(rotationAmount * Time.deltaTime);
			this.transform.rotation = (this.transform.rotation * deltaRotation);
		}			
	}
	
	
	public void InputtoWorldSpace(Transform root, Transform camera, ref float directionOut, ref float speedOut, ref float angleOut)
	{
		//This Method translates the characters forward direction as a "joystick" would move in relationship to the fixed camera.
		Vector3 rootDirection = root.forward;
		Vector3 inputDirection = new Vector3(horizontal,0,vertical);
		speedOut = inputDirection.sqrMagnitude;
		
		Vector3 camDirection = camera.forward;
		camDirection.y = 0.0f;
		Quaternion refShift = Quaternion.FromToRotation(Vector3.forward,camDirection);
		
		Vector3 moveDirection = refShift * inputDirection;
		Vector3 axisSin = Vector3.Cross(moveDirection,rootDirection);
		
		float angleRootToMove = Vector3.Angle(rootDirection,moveDirection) * (axisSin.y >= 0 ? -1f : 1f);
		angleOut = angleRootToMove;
		angleRootToMove /= 180f;
		directionOut = angleRootToMove * directionSpeed;
	}
		
		
	public bool inLocomotion()
	{
		return stateInfo.nameHash == locomotionID;	
	}
}
