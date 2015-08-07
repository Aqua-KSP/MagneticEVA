using System.Collections.Generic;
using UnityEngine;

namespace MagneticEVA
{
    /// <summary>
    /// This class represents the magnetic boots on a Kerbal.
    /// Note that each Kerbal has two(!) boots.
    /// </summary>
    /// <remarks>
    /// The design isn't good. Magnetic forces are calculated for a point between the feet.
    /// Then isn't it better to track the Kerbal instead of the feet transforms?
    /// </remarks>
    public class MagneticBootsModule : PartModule
    {
        #region private attributes
        /// <summary>
        /// If the boots are toggled on or off.
        /// </summary>
        private bool isActive = false;

        /// <summary>
        /// If the Kerbal is colliding with something.
        /// </summary>
        private bool isColliding = false;
        
        /// <summary>
        /// What object the Kerbal is colliding with.
        /// </summary>
        private GameObject collidingObject;

        /// <summary>
        /// The (hopefully) exact collision point.
        /// </summary>
        private ContactPoint collisionPoint;

        /// <summary>
        /// Custom GameObject which is attached to the left foot.
        /// </summary>
        /// <remarks>
        /// Needed because the foot transform itself is deactivated when a Kerbal switched to walk mode.
        /// </remarks>
        /// <see cref="http://forum.kerbalspaceprogram.com/threads/127278-How-to-force-Kerbals-to-stand-instead-of-float?p=2119959&viewfull=1#post2119959"/>
        private GameObject leftFoot;

        /// <summary>
        /// Custom GameObject which is attached to the right foot.
        /// </summary>
        /// <remarks>
        /// Needed because the foot transform itself is deactivated when a Kerbal switched to walk mode.
        /// </remarks>
        /// <see cref="http://forum.kerbalspaceprogram.com/threads/127278-How-to-force-Kerbals-to-stand-instead-of-float?p=2119959&viewfull=1#post2119959"/>
        private GameObject rightFoot;

        /// <summary>
        /// A vessel within this range (in meters) is checked if the magnetic boots are attracted to it.
        /// </summary>
        /// <remarks>
        /// The physics bubble around a vessel has a radius of roughly 2.5 km.
        /// </remarks>
        private const float minVesselRange = 2500f;
        #endregion private attributes

        #region KSPFields
        /// <summary>
        /// Displays the range (in meters) of the magnets. If parts are within this range the magnets will be attracted to it.
        /// </summary>
        /// <remarks>Calculated by EstimatedRange().</remarks>
        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Est. mag. range", guiUnits = "m")]
        public float range = 0f;

        /// <summary>
        /// Strength of a magnet (per boot!). Higher strength value results in farther range and increased acceleration to nearby parts.
        /// Note that the smaller the distance to a part the more attraction force there is.
        /// </summary>
        /// <remarks>
        /// Unit is arbitrary because there's no real magnetic field calculation. Maybe I'll change that in the future.
        /// </remarks>
        [UI_FloatRange(minValue = 0.05f, maxValue = 1f, stepIncrement = 0.05f)]
        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Magnetic force per boot")]
        public float magneticForce = 0.1f;
        #endregion KSPFields

        #region KSPEvents
        /// <summary>
        /// Enables/disables magnetic boots in the part (Kerbal) context menu.
        /// </summary>
        [KSPEvent(externalToEVAOnly = true, active = true, guiActive = true, guiName = "Toggle Magnetic Boots")]
        public void ToggleBoots()
        {
            isActive = !isActive;
            Debug.Log("MagneticBoots toogled: " + isActive);
        }
        #endregion KSPEvents

        /// <summary>
        /// Sets up left and right foot GameObjects. Called by the game.
        /// </summary>
        public void Start()
        {
            KerbalEVA evaModule = (KerbalEVA) this.part.Modules["KerbalEVA"];

            leftFoot = new GameObject();
            leftFoot.name = "leftFoot";
            leftFoot.transform.position = transform.Search("footCollider_l").position;
            leftFoot.transform.rotation = transform.Search("footCollider_l").rotation;
            leftFoot.transform.parent = evaModule.gameObject.transform;

            rightFoot = new GameObject();
            rightFoot.name = "rightFoot";
            rightFoot.transform.position = transform.Search("footCollider_r").position;
            rightFoot.transform.rotation = transform.Search("footCollider_r").rotation;
            rightFoot.transform.parent = evaModule.gameObject.transform;
                        
            //KerbalEVA eva = (KerbalEVA) this.part.Modules["KerbalEVA"];
            //System.Collections.Generic.List<KFSMState> stateList = (System.Collections.Generic.List<KFSMState>) eva.fsm.GetType().GetField("States", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(eva.fsm);
            //stateList.ForEach(state => Debug.Log("[MagneticEVA] state: " + state.name));
        }

        /// <summary>
        /// Called by the game when a collision starts. Forces the Kerbal to walk mode.
        /// </summary>
        /// <param name="c">Collision information</param>
        public void OnCollisionEnter(Collision c)
        {
            if (isActive)
            {
                Debug.Log("MagneticBoots Collision detected");
                Debug.Log("State: " + this.vessel.evaController.fsm.CurrentState.name);
                Debug.Log("Collided with: " + GetPartByGameObject(c.collider.gameObject).name);
                
                this.part.GetComponent<Vessel>().Landed = true;
                isColliding = true;
            }
        }

        /// <summary>
        /// Called by the game when a collision continues. Used to query latests collision information.
        /// </summary>
        /// <param name="c">collision information</param>
        public void OnCollisionStay(Collision c)
        {
            if (isActive)
            {
                collidingObject = c.collider.gameObject;
                collisionPoint = c.contacts[0];
            }
        }

        /// <summary>
        /// Applies magnetic attraction forces to the Kerbal and the part he collides with (if there's one).
        /// Called by the game when it's time to calculate physics.
        /// </summary>
        /// <remarks>
        /// To do:
        ///     Apply attraction forces to parts within range of the magnets.
        ///     Fix wonky walk behavior of the Kerbal. There's definitly something going wrong.
        /// </remarks>
        public void FixedUpdate()
        {
            if (isActive)
            {
                range = EstimatedRange(); // update range value in part context menu

                if (isColliding)
                {
                    this.rigidbody.AddForceAtPosition(-collisionPoint.normal.normalized * magneticForce / 2f, collisionPoint.point, ForceMode.Force);
                    this.rigidbody.AddForceAtPosition(-collisionPoint.normal.normalized * magneticForce / 2f, collisionPoint.point, ForceMode.Force);

                    Part collidingPart = GetPartByGameObject(collidingObject);
                    collidingPart.rigidbody.AddForceAtPosition(collisionPoint.normal.normalized * magneticForce, collisionPoint.point, ForceMode.Force);

                    Debug.Log("Attraction force: " + magneticForce);
                }
                else
                {
                    Vector3 attractingForce = CalculateAttractionForce();
                    this.rigidbody.AddForceAtPosition(-attractingForce / 2f, leftFoot.transform.position, ForceMode.Force);
                    this.rigidbody.AddForceAtPosition(-attractingForce / 2f, rightFoot.transform.position, ForceMode.Force);

                    // To do: Add attraction force to nearby parts.
                    // Could be fun to use the magnets to collect floating parts and throw vessels in range out of their current orbit.
                    // Jeb recommends this!

                    Debug.Log("Attraction force: " + attractingForce.magnitude);
                }
            }
        }

        /// <summary>
        /// Called by the game when a collision stops.
        /// </summary>
        /// <param name="c">collision information</param>
        public void OnCollisionExit(Collision c)
        {
            if (isActive)
            {
                Debug.Log("[MagneticBoots] Collision ended");
                Debug.Log("State: " + this.vessel.evaController.fsm.CurrentState.name);
                this.part.GetComponent<Vessel>().Landed = false;
                isColliding = false;
            }
        }

        /// <summary>
        /// Finds the part by the GameObject belongs to.
        /// </summary>
        /// <param name="gameObject">GameObject of a part</param>
        /// <returns>the part</returns>
        /// <remarks>Thanks goes to xEvilReeperx! :-)</remarks>
        /// <see cref="http://forum.kerbalspaceprogram.com/threads/7544-The-official-unoffical-help-a-fellow-plugin-developer-thread?p=2124761&viewfull=1#post2124761"/>
        public Part GetPartByGameObject(GameObject gameObject)
        {
            return gameObject.GetComponentInParent<Part>();
        }

        /// <summary>
        /// Calculates a point between the feet.
        /// </summary>
        /// <returns>point</returns>
        public Vector3 GetCenterOfAttraction()
        {
            return (leftFoot.transform.position + rightFoot.transform.position) / 2f;
        }

        /// <summary>
        /// Scans for parts nearby and calculates a sum of all magnetic attraction forces to these parts.
        /// </summary>
        public Vector3 CalculateAttractionForce()
        {
            List<Attraction> attractionForces = new List<Attraction>();
            List<Vessel> nearbyVessels = FlightGlobals.fetch.vessels.FindAll(NearbyVessels);

            foreach (Vessel vessel in nearbyVessels)
                vessel.Parts.FindAll(PartsInRange).ForEach(part => attractionForces.Add(CreateAttractionTo(part)));
            
            return CombineAttractionForces(attractionForces);
        }

        /// <summary>
        /// Checks if a vessel is close enough to check for magnetic interaction. This is the case when the distance is below minVesselRange.
        /// </summary>
        /// <param name="vessel">Vessel to check</param>
        /// <returns>True if the vessel is within minVesselRange.</returns>
        public bool NearbyVessels(Vessel vessel)
        {
            if (Vector3.Distance(this.vessel.transform.position, vessel.transform.position) < minVesselRange)
                return true;

            return false;
        }

        /// <summary>
        /// Checks if a part's center is close enough for the magnets to act on.
        /// </summary>
        /// <param name="part">Part to check</param>
        /// <returns>True, if a part's center is within maxRange.</returns>
        public bool PartsInRange(Part part)
        {
            float distance = Vector3.Distance(part.transform.position, this.vessel.transform.position);

            if (MagneticForceAt(distance) > ((UI_FloatRange) Fields["magneticForce"].uiControlFlight).minValue)
                return true;

            return false;
        }

        /// <summary>
        /// Calculates strength of the magnetic field at distance.
        /// </summary>
        /// <param name="distance"></param>
        /// <returns>magnetic field strength</returns>
        public float MagneticForceAt(float distance)
        {
            return Mathf.Min(magneticForce / Mathf.Pow(distance, 2f), ((UI_FloatRange) Fields["magneticForce"].uiControlFlight).maxValue);

        }

        /// <summary>
        /// Calculates the distance where the magnetic field has a strength of minMagneticForce.
        /// </summary>
        /// <returns>distance</returns>
        public float EstimatedRange()
        {
            return Mathf.Sqrt(magneticForce / ((UI_FloatRange) Fields["magneticForce"].uiControlFlight).minValue);
        }

        /// <summary>
        /// Calculates the distance between two parts.
        /// </summary>
        /// <param name="a">First part</param>
        /// <param name="b">Second part</param>
        /// <returns>distance</returns>
        public float DistanceBetween(Part a, Part b)
        {
            return DistanceBetween(a.transform, b);
        }

        /// <summary>
        /// Calculates the distance between a transform and a part.
        /// </summary>
        /// <param name="a">transform</param>
        /// <param name="b">part</param>
        /// <returns>distance</returns>
        public float DistanceBetween(Transform a, Part b)
        {
            return Vector3.Distance(a.position, b.transform.position);
        }

        /// <summary>
        /// Calculates the attraction force between magnet and a part.
        /// </summary>
        /// <param name="part">Part which a magnet is attracted to</param>
        /// <returns>attraction force</returns>
        public Attraction CreateAttractionTo(Part part)
        {
            return new Attraction(MagneticForceAt(DistanceBetween(this.part, part)), this.part.transform.position - part.transform.position);
        }

        /// <summary>
        /// Sums up all attraction forces to finally get a direction and force strength.
        /// </summary>
        /// <param name="attractionForces"></param>
        /// <returns>sum of forces</returns>
        public Vector3 CombineAttractionForces(List<Attraction> attractionForces)
        {
            Vector3 combined = Vector3.zero;

            attractionForces.ForEach(attraction => combined += attraction.Force);

            return combined;
        }


        
        
        /* KFSMState values:
            [LOG 16:55:34.923] [MagneticEVA] state: Idle (Grounded)
            [LOG 16:55:34.923] [MagneticEVA] state: Walk (Arcade)
            [LOG 16:55:34.924] [MagneticEVA] state: Walk (FPS)
            [LOG 16:55:34.924] [MagneticEVA] state: Turn to Heading
            [LOG 16:55:34.925] [MagneticEVA] state: Run (Arcade)
            [LOG 16:55:34.926] [MagneticEVA] state: Run (FPS)
            [LOG 16:55:34.926] [MagneticEVA] state: Low G Bound (Grounded - Arcade)
            [LOG 16:55:34.927] [MagneticEVA] state: Low G Bound (Grounded - FPS)
            [LOG 16:55:34.928] [MagneticEVA] state: Low G Bound (floating)
            [LOG 16:55:34.928] [MagneticEVA] state: Ragdoll
            [LOG 16:55:34.929] [MagneticEVA] state: Recover
            [LOG 16:55:34.930] [MagneticEVA] state: Jumping
            [LOG 16:55:34.930] [MagneticEVA] state: Idle (Floating)
            [LOG 16:55:34.931] [MagneticEVA] state: Landing
            [LOG 16:55:34.932] [MagneticEVA] state: Swim (Idle)
            [LOG 16:55:34.932] [MagneticEVA] state: Swim (fwd)
            [LOG 16:55:34.933] [MagneticEVA] state: Ladder (Acquire)
            [LOG 16:55:34.934] [MagneticEVA] state: Ladder (Idle)
            [LOG 16:55:34.934] [MagneticEVA] state: Ladder (Lean)
            [LOG 16:55:34.935] [MagneticEVA] state: Ladder (Climb)
            [LOG 16:55:34.936] [MagneticEVA] state: Ladder (Descend)
            [LOG 16:55:34.936] [MagneticEVA] state: Ladder (Pushoff)
            [LOG 16:55:34.937] [MagneticEVA] state: Clamber (P1)
            [LOG 16:55:34.937] [MagneticEVA] state: Clamber (P2)
            [LOG 16:55:34.938] [MagneticEVA] state: Clamber (P3)
            [LOG 16:55:34.939] [MagneticEVA] state: Flag-plant Terrain Acquire
            [LOG 16:55:34.940] [MagneticEVA] state: Planting Flag
            [LOG 16:55:34.940] [MagneticEVA] state: Seated (Command)
            [LOG 16:55:34.941] [MagneticEVA] state: Grappled
         */
    }
}
