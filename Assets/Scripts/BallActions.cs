using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raavanan
{
    public class BallActions : MonoBehaviour
    {
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag.Contains("Finish") || collision.gameObject.tag.Contains("Stumps"))
            {
                MainController._Instance.ResetVairables();
            }
        }
    }
}
