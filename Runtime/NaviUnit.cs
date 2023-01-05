using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public class v3Event : UnityEvent<Vector3>
    {
    }

public class NaviUnit : MonoBehaviour
{
    public Transform target;
	public Vector3 destination;
	public bool arrived = true;

	public bool stepForward = false;
	public bool autoUpdatePath = false;

	[SerializeField]
    public SO_NaviUnitData navUnitData;
	//public SO_NavUnitData.Settings naveUnitSettings;

	const float minPathUpdateTime = .001f;
	const float pathUpdateMoveThreshold = .5f;
	Navi_Path path;
	Vector3 lastDestination;

	[HideInInspector]
	public v3Event navUnitStepped;



    public void Start()
        {
		arrived = true;
		lastDestination = destination;

		if (target)
			lastDestination = target.position;
		//	SetDestination(new Vector3(-41.1300011f, 0, -46.1100006f));
        }

    public void SetDestination(Vector3 val)
        {
			destination = val;
			RunUpdatePath();
        }

	public void SetTarget(Transform val)
        {
			target = val;
			RunUpdatePath();
        }

	public void StopNaviUnit()
		{
			stepForward = false;
		}

	public void RunUpdatePath()
        {
		
			lastDestination = destination;

			//if (target)
			//lastDestination = target.position;
		
			StopAllCoroutines();
			if (autoUpdatePath)
				{
				//	StopAllCoroutines();
					StartCoroutine(AutoUpdatePath());
				}
			else
				ManualUpdatePath();
		}

	public bool TargetHasMoved(Vector3 val00, Vector3 val01, float threshold = 0.01f)
        {
			float val02 = Mathf.Abs(val00.sqrMagnitude - val01.sqrMagnitude);
			//Debug.Log("Target Movement : " + val02);
			return val02 > 0.01f;
        }
	IEnumerator AutoUpdatePath()
        {
			if (Time.timeSinceLevelLoad < .001f)
				{
					yield return new WaitForSeconds(.001f);
				}

			yield return new WaitForSeconds(minPathUpdateTime);

			RequestData requestData = new RequestData { pathStart = transform.position, pathEnd =destination, includeDiagonal = navUnitData.settings.canCrossDiagonal, simplifyPath = navUnitData.settings.canCrossDiagonal };

			NaviSurface.RequestPath(requestData, OnPathFound);

			while (true)
				{
					yield return new WaitForSeconds(minPathUpdateTime);

					if (target)
						{
							destination = target.position;
							//Debug.Log("hasTarget : " + destination);

						}

					//yield return new WaitUntil(() => HasMoved(lastDestination, destination) == true);
					//yield return new WaitUntil(() => HasMoved(lastDestination, destination) == true);

					if (TargetHasMoved(lastDestination, destination) == true)
						{
							
							StopCoroutine(FollowPath());
						//	yield return new WaitForSeconds(minPathUpdateTime);
							//path = null;
							
							ReCalc();

							//NavSurface.RequestPath(transform.position, destination, OnPathFound);

							//lastDestination = destination;

						}
				}
		}

	public void ReCalc()
        {
			StopAllCoroutines();
			RunUpdatePath();
        }

	public void ManualUpdatePath()
		{
			path = null;
		
			RequestData requestData = new RequestData { pathStart = transform.position, pathEnd = destination, includeDiagonal = navUnitData.settings.canCrossDiagonal, simplifyPath = navUnitData.settings.canCrossDiagonal };

			NaviSurface.RequestPath(requestData, OnPathFound);
		}
	/*
	IEnumerator UpdatePath()
		{

		if (Time.timeSinceLevelLoad < .3f)
			{
			yield return new WaitForSeconds(.3f);
			}


		RequestData requestData = new RequestData { pathStart = transform.position, pathEnd = destination, includeDiagonal = navUnitData.settings.canCrossDiagonal, simplifyPath = navUnitData.settings.canCrossDiagonal };
		NavSurface.RequestPath(requestData, OnPathFound);

		

		float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
		Vector3 oldDestination = destination;

		while (true)
			{
			yield return new WaitForSeconds(minPathUpdateTime);
			if ((destination - oldDestination).sqrMagnitude > sqrMoveThreshold)
				{
				NavSurface.RequestPath(requestData, OnPathFound);
				oldDestination = destination;
				}
			}
		}
	*/

	public void OnPathFound(CallBackData callBackData)
		{
			SO_NaviUnitData.Settings settings = navUnitData.settings;

			if (callBackData.pathSuccessful)
				{
					path = new Navi_Path(callBackData.waypoints, transform.position, settings.turnDst, settings.stoppingDist);

					StopCoroutine(FollowPath());
					//StopAllCoroutines();
					StartCoroutine(FollowPath());
				}

		}

	IEnumerator FollowPath()
		{

			SO_NaviUnitData.Settings settings = navUnitData.settings;
			bool followingPath = true;
			int pathIndex = 0;
			int currentSteps = 0;
			float speedPercent = 1;
			if(path != null)
			if(path.lookPoints.Length > 0)
			transform.LookAt(path.lookPoints[0]);

			while (followingPath)
				{
					
					arrived = false;
					Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);

					while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
						{
							if (pathIndex == path.finishLineIndex)
								{
									arrived = true;
									followingPath = false;
									break;
								}
							else
								{
									yield return new WaitUntil(() => stepForward == true);

									pathIndex++;
									currentSteps++;
									
									navUnitStepped.Invoke(transform.position);
									

                                    yield return new WaitForSeconds(settings.stepSpeed);

									if (currentSteps > 0) // If currentSteps less than 0 then no step limit.
										if (currentSteps >= settings.stepLimit)
											{
												currentSteps = 0;
												stepForward = false;
											}
								}
						}
					
					if (followingPath )
						{

							if (pathIndex >= path.slowDownIndex && settings.stoppingDist > 0)
								{
									speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D) / settings.stoppingDist);
							
									if (speedPercent < 0.01f)
										{
											followingPath = false;
										}
								}
								
							Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
							transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * settings.turnSpeed*100);
							transform.Translate(Vector3.forward * Time.deltaTime * settings.speed * speedPercent, Space.Self);
						}

					yield return null;

				}
		}


#if UNITY_EDITOR
    public void OnDrawGizmos()
		{
		if (path != null)
			{
				path.DrawWithGizmos();
			}
		}

#endif
    }


