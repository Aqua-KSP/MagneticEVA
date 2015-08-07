using UnityEngine;

namespace MagneticEVA
{
    /// <summary>
    /// This Behavior adds and removes magnetic boots to Kerbals on EVA events.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Flight,false)]
    public class MagneticBootsBehavior : MonoBehaviour
    {
        /// <summary>
        /// Hooks EVA events. Called by the game.
        /// </summary>
        public void Start()
        {
            GameEvents.onCrewOnEva.Add(AddMagneticBoots);
            GameEvents.onCrewBoardVessel.Add(RemoveMagneticBoots);
            Debug.Log("MagneticBoots started");
        }

        /// <summary>
        /// Unhooks EVA events. Called by the game.
        /// </summary>
        public void Destroy()
        {
            GameEvents.onCrewOnEva.Remove(AddMagneticBoots);
            GameEvents.onCrewBoardVessel.Remove(RemoveMagneticBoots);
            Debug.Log("MagneticBoots destroyed");
        }

        /// <summary>
        /// Adds magnetic boots to a Kerbal who just EVA'ed.
        /// </summary>
        /// <param name="action">Vessel/crew part where the Kerbal EVA'ed from and the Kerbal himself</param>
        public void AddMagneticBoots(GameEvents.FromToAction<Part,Part> action)
        {
            if (action.to.Modules["KerbalEVA"] != null)
            {
                if (!action.to.Modules.Contains("MagneticBootsModules"))
                {
                    action.to.AddModule("MagneticBootsModule");

                    Debug.Log("MagneticBoots added");
                }
            }
        }

        /// <summary>
        /// Removes magnetic boots from a Kerbal who is in the process of boarding a crew part.
        /// </summary>
        /// <param name="action">Kerbal who is boarding and the crew part which he wants to board</param>
        private void RemoveMagneticBoots(GameEvents.FromToAction<Part, Part> action)
        {
            if (action.from.Modules["KerbalEVA"] != null)
            {
                if (action.from.Modules.Contains("MagneticBootsModules"))
                {
                    action.from.RemoveModule(action.from.Modules["MagneticBootsModule"]);

                    Debug.Log("MagneticBoots removed");
                }
            }
        }
    }
}
