using UnityEngine;

namespace Components.Misc
{
    public class Rotator : MonoBehaviour
    {
        [SerializeField]
        private Vector3 _angularVelocity;

        private void Update()
        {
            transform.Rotate(_angularVelocity * Time.deltaTime, Space.Self);
        }
    }
}