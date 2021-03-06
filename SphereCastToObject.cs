﻿using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Basic.UnityPhysics
{
    [TaskCategory("Basic/Physics")]
    [TaskDescription("Casts a sphere against all colliders in the scene. Returns success if a collider was hit.")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=117")]
    public class SphereCastToObject : Action
    {
        [Tooltip("Start ray at game object position. \nOr use From Position parameter.")]
        public SharedGameObject fromGameObject;
   

        [Tooltip("Start ray at a vector3 world position. \nOr use Game Object parameter.")]
        public SharedVector3 fromPosition;

        [Tooltip("The radius of the shpere.")]
        public SharedFloat radius;

        [Tooltip("A vector3 direction vector")]
        public SharedVector3 direction;

        [Tooltip("A Target Object for the direction. Overrides Direction if used.")]
        public SharedGameObject endObject;

        [Tooltip("Cast the ray in world or local space. Note if no Game Object is specfied, the direction is in world space.")]
        public Space space;

        [Tooltip("The length of the ray. Set to -1 for infinity.")]
        public SharedFloat distance;

        [Tooltip("Set to true to ignore colliders set to trigger.")]
        public SharedBool ignoreTriggerColliders;

        public SharedFloat hitDistance;


        [Tooltip("Set how often to cast a ray. 0 = once, don't repeat; 1 = everyFrame; 2 = every other frame... \nSince raycasts can get expensive use the highest repeat interval you can get away with.")]
        public SharedInt repeatInterval;
       
        [Tooltip("Pick only from these layers.")]
        public LayerMask layerMask;

        [Tooltip("The color to use for the debug line.")]
        public SharedColor debugColor;

        [Tooltip("Draw a debug line. Note: Check Gizmos in the Game View to see it in game.")]
        public SharedBool debug;

        int repeat;

        GameObject storeHitObject;

        public override TaskStatus OnUpdate()
        {

            repeat = repeatInterval.Value;

            if (distance.Value == 0)
            {
                return TaskStatus.Failure;

            }

            var goFr = fromGameObject.Value.gameObject;
            var originPos = goFr != null ? goFr.transform.position : fromPosition.Value;
            var rayLength = Mathf.Infinity;
            var goTo = endObject.Value;
            var dirVector = direction.Value;
            var next = true;

            if (distance.Value > 0)
            {
                rayLength = distance.Value;
            }

            if (goTo != null)
            {
                dirVector = goTo.transform.position - originPos;
                next = false;
            }

            if (goFr != null && space == Space.Self && next)
            {
                dirVector = goFr.transform.TransformDirection(direction.Value);
            }

            if(endObject.Value.activeInHierarchy != true)
            {
                return TaskStatus.Failure;

            }

            rayLength = Vector3.Distance(goFr.gameObject.transform.position, goTo.gameObject.transform.position);

            RaycastHit hitInfo;

            if (ignoreTriggerColliders.Value == true)
            {
                Physics.SphereCast(originPos, radius.Value, dirVector, out hitInfo, rayLength, layerMask, QueryTriggerInteraction.Ignore);
                if(hitInfo.collider.gameObject != null)
                {
                    storeHitObject = hitInfo.collider.gameObject;
                    hitDistance.Value = hitInfo.distance;     
                    //Debug.Log(storeHitObject);
                }
                else
                {
                    return TaskStatus.Failure;
                }         
            } 
            else
            {
                Physics.SphereCast(originPos, radius.Value, dirVector, out hitInfo, rayLength, layerMask, QueryTriggerInteraction.Collide);

                if (hitInfo.collider.gameObject != null)
                {
                    storeHitObject = hitInfo.collider.gameObject;
                    hitDistance.Value = hitInfo.distance;
                    //Debug.Log(storeHitObject);
                } 
                else
                {
                    return TaskStatus.Failure;
                }
            }
            var didHit = hitInfo.collider != null;        

            if (didHit && storeHitObject == endObject.Value )
            {
                //return success
                return TaskStatus.Success;
            }
            if (didHit && storeHitObject != endObject.Value)
            {
                return TaskStatus.Failure;

            }
                   

            if (!didHit)
            {
                return TaskStatus.Failure;
                //return failure
            }         

            if (debug.Value && next)
            {
                var debugRayLength = Mathf.Min(rayLength, 1000);
                Debug.DrawLine(originPos, originPos + dirVector * debugRayLength, debugColor.Value);
            }

            if (debug.Value && !next)
            {
                var debugRayLength = Mathf.Min(rayLength, 1000);
                var d = (goTo.transform.position - originPos).normalized * debugRayLength + originPos;
                Debug.DrawLine(originPos, d, debugColor.Value);
            }
            return TaskStatus.Failure;


        }

        public override void OnReset()
        {
            fromGameObject = null;
            fromPosition = Vector3.zero;
            radius = 0;
            direction = Vector3.zero;
            distance = -1;
            layerMask = -1;
            space = Space.Self;
            endObject = null;

        }

        
    }
}
