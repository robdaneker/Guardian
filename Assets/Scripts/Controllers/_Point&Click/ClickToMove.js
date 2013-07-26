var dir : Vector3;
var k : Vector3;
var hit : RaycastHit ;
var rot = 0;
var speed : float = 10.0;
var gravity : float = -18.0;
private var vel :Vector3;
 
private var layerMask : int = 1 << 9; 
private var gravityPower : Vector3 = Vector3.zero;
private var cc : CharacterController;
@SerializeField
private var cam : Camera;
 
function Start() {
  cc = GetComponent(CharacterController);
  gravityPower.y = gravity;
}
 
function FixedUpdate()
{       
if (Input.GetMouseButton(1))
    {
        // Cast ray from mouse to point on terrain and get location
        var ray = cam.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(cam.transform.position,ray.direction,hit,1000,layerMask);
 
        // Define direction to move
        dir = hit.point - transform.position;
        // Get rotation smoothly
        var angle = Vector3.Angle(dir, transform.forward);
        var k = Vector3.Cross(transform.forward, dir);
        k.Normalize();
        rot = k[1]*(angle*2.5);
        print(k);
 
        // ---- Move controller ---- //
 
        // Apply Gravity
        //cc.MovePosition(gravityPower * Time.deltaTime);
        // Apply Rotation
        transform.Rotate(Vector3.up*rot*Time.deltaTime*5);
        // Move towards direction (dir) normalized
        cc.Move(dir.normalized * Time.deltaTime * speed);
    }
else
    {
        // if not at clicked location continue moving there
        if (transform.position != hit.point)
        {
            // Define direction to move
            dir = hit.point - transform.position;
            // Get rotation smoothly
            angle = Vector3.Angle(dir, transform.forward);
            k = Vector3.Cross(transform.forward, dir);
            k.Normalize();
            rot = k[1]*(angle*2.5);
 
            // Move controller
 
            // Apply Gravity
            //cc.MovePosition(gravityPower * Time.deltaTime);
            // Apply Rotation
            transform.Rotate(Vector3.up*rot*Time.deltaTime*5);
            // Move towards direction (dir) normalized
            cc.Move(dir.normalized * Time.deltaTime * speed);
        }
    }
} 