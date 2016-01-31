using UnityEngine;
using System.Collections;

public class BoltEffect : MonoBehaviour
{
    public LineRenderer lRend;
	public GameObject target;
	public Vector3 zapPoint;
	public float arcLength = 1.0f;
	public float arcVariation = 1.0f;
	public float inaccuracy = 0.5f;
	public float timeOfZap = 0.15f;
	public float zapTimer;
	public bool trackTarget = false;
	public AudioSource zap;

	void Start()
	{
		if (lRend == null)
		{
			lRend = GetComponent<LineRenderer>();
		}
		zapTimer = 0;
		lRend.SetVertexCount(1);

		zap = AudioManager.Instance.MakeSource("Bolt Zap");

		zap.volume = 0;
		zap.loop = true;
		zap.Play();
	}

	void Update()
	{
		if (zapTimer > 0)
		{
			zap.volume = .3f;
			if (trackTarget)
			{
				//Debug.LogError("Thing\n");
				zapPoint = target.transform.position;
			}

			Vector3 lastPoint = transform.position;
			int i = 1;
			lRend.SetWidth(.15f, .15f);
			lRend.SetPosition(0, transform.position);//make the origin of the LR the same as the transform
			while (Vector3.Distance(zapPoint, lastPoint) > 3.0f)
			{//was the last arc not touching the target?
				lRend.SetVertexCount(i + 1);//then we need a new vertex in our line renderer
				Vector3 fwd = zapPoint - lastPoint;//gives the direction to our target from the end of the last arc
				fwd.Normalize();//makes the direction to scale
				fwd = Randomize(fwd, inaccuracy);//we don't want a straight line to the target though
				fwd *= Random.Range(arcLength * arcVariation, arcLength);//nature is never too uniform
				fwd += lastPoint;//point + distance * direction = new point. this is where our new arc ends
				lRend.SetPosition(i, fwd);//this tells the line renderer where to draw to
				i++;
				lastPoint = fwd;//so we know where we are starting from for the next arc
			}
			lRend.SetVertexCount(i + 1);
			lRend.SetPosition(i, zapPoint);
			//lightTrace.TraceLight(gameObject.transform.position, target.transform.position);
			zapTimer = zapTimer - Time.deltaTime;
		}
		else
		{
			zap.volume = 0f;
			lRend.SetVertexCount(1);
		}
	}

	private Vector3 Randomize(Vector3 newVector, float devation)
	{
		newVector += new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)) * devation;
		newVector.Normalize();
		return newVector;
	}

	public void ZapPoint(Vector3 newPoint)
	{
		trackTarget = false;
		zapTimer = timeOfZap;
		zapPoint = newPoint;
	}

	public void ZapTarget(GameObject newTarget)
	{
		trackTarget = true;
		target = newTarget;
		zapTimer = timeOfZap;


	}
}
