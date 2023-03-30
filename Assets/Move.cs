using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    // speed of movement in m/s
    public float speed = 2.0f;
    // jump height in m
    public float jump = 1.0f;

    // the controller attached to this object
    CharacterController controller;
    // current velocity during a fall or slide
    Vector3 velocity;
    // gravity direction if gravity is in use
    Vector3 gravity;
    // the normal vector of the last touched surface
    Vector3 surface;
    // the angle of the slope of the last touched surface
    float slope;
    // the magnitude of the gravity force
    float g;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        g = Physics.gravity.magnitude;
        if (g > 0.0f)
        {
            gravity = Physics.gravity / g;
        }
        else
        {
            gravity = Vector3.zero;
        }

        surface = -gravity;
        slope = 0.0f;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // if time isn't running, don't process any movement as it may cause
        // a divide by zero below
        if (dt == 0.0f)
        {
            return;
        }

        bool sliding = false;
        bool jumping = Input.GetButtonDown("Jump");

        // only process jumps if we have gravity and are grounded
        if (g > 0.0f && controller.isGrounded)
        {
            // we are sliding if we are grounded and on a slope steeper than
            // the slopeLimit
            sliding = slope > controller.slopeLimit;

            // only allow a jump if we are on the ground and not sliding
            if (!sliding && jumping)
            {
                // up is the upward velocity needed to reach (jump) height
                float up = Mathf.Sqrt(jump * 2.0f * g);

                // clear any current velocity and replace it with an upward
                // velocity opposed to gravity
                velocity = -gravity * up;
            }

            if (sliding)
            {
                // calculate a falling vector along the surface
                Vector3 right = Vector3.Cross(gravity, surface);
                Vector3 fall = Vector3.Cross(surface, right);
                velocity += fall * g * dt;
            }
        }

        // user input
        Vector3 input = Vector3.right * Input.GetAxis("Horizontal") * speed;
        input += Vector3.forward * Input.GetAxis("Vertical") * speed;

        // d = vt + 1/2at^2
        Vector3 move = (input + velocity) * dt + gravity * (0.5f * g * dt * dt);

        // v = u + at
        velocity += gravity * g * dt;

        controller.Move(move);

        // clear the current velocity if we are touching the ground and
        // not sliding
        if (controller.isGrounded && !sliding)
        {
            // set a non-zero downward velocity so that the
            // CharacterController stays on the surface
            velocity = gravity * (controller.skinWidth / dt);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // OnControllerColliderHit can get called even if the component
        // or gameObject is inactive
        if (controller != null)
        {
            // find the centre of the base (foot) of the character controller
            float r = controller.radius;
            float h = controller.height * 0.5f - r;
            Vector3 foot = transform.position + transform.up * -h;

            // find the vector from the foot to the collision point
            Vector3 v = hit.point - foot;

            // ignore unless the hit point is below the foot
            // (in the direction of gravity)
            bool below = Vector3.Dot(gravity, v) > 0.0f;
            if (below)
            {
                // get the normal and slope angle with the hit surface
                surface = hit.normal;
                slope = Vector3.Angle(hit.normal, -gravity);
            }
        }
    }
}
