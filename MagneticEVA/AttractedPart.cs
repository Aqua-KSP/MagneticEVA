using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MagneticEVA
{
    /// <summary>
    /// Represents an attraction force.
    /// </summary>
    public class Attraction
    {
        /// <summary>
        /// Creates an attraction force. Makes sure there are no NaN values.
        /// </summary>
        /// <param name="forceMagnitude">the strength of the force</param>
        /// <param name="direction">the direction of the force</param>
        public Attraction(float forceMagnitude, Vector3 direction)
        {
            Force = direction.normalized * forceMagnitude;

            if (float.IsNaN(Force.x) || float.IsNaN(Force.y) || float.IsNaN(Force.z))
                Force = Vector3.zero;

        }

        /// <summary>
        /// Attraction force with normalized direction, multiplied by strength.
        /// </summary>
        public Vector3 Force
        {
            get;
            private set;
        }
    }
}
