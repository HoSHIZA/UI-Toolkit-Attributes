using UnityEngine;

namespace PiRhoSoft.Utilities
{
	[AddComponentMenu("PiRho Utilities/Euler")]
	public class EulerSample : MonoBehaviour
    {
        [Euler]
        public Quaternion euler = Quaternion.Euler(123, 21, 87);
    }
}