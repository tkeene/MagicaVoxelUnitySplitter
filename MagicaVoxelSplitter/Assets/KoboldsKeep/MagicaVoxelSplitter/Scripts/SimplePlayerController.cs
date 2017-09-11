using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KoboldsKeep
{
    namespace MagicaVoxelSplitter
    {
        public class SimplePlayerController : MonoBehaviour
        {
            public float raycastHeight = 100.0f;
            public float speed = 5.0f;
            public float characterHeight = 0.5f;

            public KeyCode up = KeyCode.W;
            public KeyCode down = KeyCode.S;
            public KeyCode left = KeyCode.A;
            public KeyCode right = KeyCode.D;

            public Vector3 cameraOffset = new Vector3(-10, 10, -10);

            // Update is called once per frame
            void Update()
            {
                Vector2 input = Vector2.zero;
                if (Input.GetKey(up))
                {
                    input += Vector2.up;
                }
                if (Input.GetKey(down))
                {
                    input += Vector2.down;
                }
                if (Input.GetKey(left))
                {
                    input += Vector2.left;
                }
                if (Input.GetKey(right))
                {
                    input += Vector2.right;
                }
                if (input != Vector2.zero)
                {
                    Vector3 right = Camera.main.transform.right;
                    Vector3 up = Quaternion.AngleAxis(angle: -90.0f, axis: Vector3.up) * right;
                    Vector3 worldSpaceOffset = right * input.x + up * input.y;
                    worldSpaceOffset *= speed * Time.deltaTime;
                    Vector3 targetPosition = transform.position + worldSpaceOffset;
                    RaycastHit hitInfo;
                    if (Physics.Raycast(transform.position + Vector3.up * raycastHeight, Vector3.down, out hitInfo, raycastHeight * 2.0f))
                    {
                        Debug.Log(hitInfo.collider.material.name);
                        targetPosition.y = hitInfo.point.y + characterHeight;
                    }
                    transform.position = targetPosition;
                }

                Camera.main.transform.position = transform.position + cameraOffset;
                Camera.main.transform.LookAt(transform.position);
            }
        }
    }
}