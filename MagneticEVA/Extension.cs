using UnityEngine;

namespace MagneticEVA
{
    /// <summary>
    /// Class extension method for transforms.
    /// </summary>
    public static class Extension
    {
        /// <summary>
        /// Searches for a specific transform.
        /// </summary>
        /// <param name="target">where to search in</param>
        /// <param name="name">transform to search for</param>
        /// <returns>the transform or null</returns>
        /// <remarks>
        /// This is copied from the KerbalFoundries plugin, licensed under the GPL v2 license.
        /// </remarks>
        /// <see cref="http://forum.kerbalspaceprogram.com/threads/84102-PARTS-PLUGIN-1-0-x-V1-8G-Kerbal-Foundries-wheels-anti-grav-repulsors-and-tracks"/>
        public static Transform Search(this Transform target, string name)
        {
            if (Equals(target.name, name))
                return target;

            for (int i = 0; i < target.childCount; ++i)
            {
                var result = Search(target.GetChild(i), name);
                if (!Equals(result, null))
                    return result;
            }
            return null;
        }
    }
}
