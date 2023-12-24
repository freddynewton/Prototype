using Game.Data.CharacterMovementData;
using System;
using UnityEngine;

namespace Game.Character
{
    [RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
    public class CharacterMovementMotor : MonoBehaviour
    {
        /// <summary>
        /// Contains the current grounding information
        /// </summary>
        [NonSerialized]
        public CharacterGroundingReport GroundingStatus = new CharacterGroundingReport();

        /// <summary>
        /// Contains the previous grounding information
        /// </summary>
        [NonSerialized]
        public CharacterTransientGroundingReport LastGroundingStatus = new CharacterTransientGroundingReport();

        [NonSerialized]
        public Vector3 Velocity = new();

        [Header("Capsule Settings")]
        [SerializeField]
        private float capsuleRadius = 0.5f;

        [SerializeField]
        private float capsuleHeight = 2f;

        [SerializeField]
        private CapsuleCollider capsule;

        [Header("Grounding Settings")]
        [SerializeField]
        private float groundDetectionExtraDistance = 0.1f;

        [SerializeField]
        [Range(0, 90)]
        private float maxStableSlopeAngle = 60f;

        [SerializeField]
        private LayerMask groundCheckLayers = -1;

        [Header("Step Settings")]
        [SerializeField]
        private float maxStepHeight = 0.5f;

        [SerializeField]
        private float minRequiredStepDepth = 0.1f;

        [Header("Ledge Settings")]
        [SerializeField]
        private float maxStableDistanceFromLedge = 0.5f;

        [SerializeField]
        private float maxVelocityForLedgeSnap = 0.5f;

        [SerializeField]
        [Range(0, 180)]
        private float maxStableDenivelationAngle = 180f;

        private Rigidbody Rigidbody { get; set; }

        private void Awake()
        {
            // Assign capsule
            capsule = GetComponent<CapsuleCollider>();
            if (capsule == null)
            {
                capsule = gameObject.AddComponent<CapsuleCollider>();
            }

            capsule.radius = capsuleRadius;
            capsule.height = capsuleHeight;
            capsule.direction = 1;
            capsule.center = Vector3.up * (capsuleHeight * 0.5f);

            // Assign rigidbody
            Rigidbody = GetComponent<Rigidbody>();
            if (Rigidbody == null)
            {
                Rigidbody = gameObject.AddComponent<Rigidbody>();
            }

            Rigidbody.isKinematic = true;
            Rigidbody.useGravity = false;
            Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        private void Update()
        {
            CheckGrounding();
            CheckSteps();
            CheckLegdes();
            ApplyVelocity();
        }

        private void ApplyVelocity()
        {
            transform.position += Velocity * Time.deltaTime;
        }

        private void CheckLegdes()
        {
            if (GroundingStatus.FoundAnyGround)
            {
                if (GroundingStatus.SnappingPrevented)
                {
                    return;
                }

                if (GroundingStatus.IsStableOnGround)
                {
                    if (GroundingStatus.GroundNormal != Vector3.up)
                    {
                        if (Vector3.Angle(GroundingStatus.GroundNormal, transform.forward) > maxStableDenivelationAngle)
                        {
                            return;
                        }
                    }

                    Vector3 capsuleBottom = transform.position + transform.up * capsule.radius;
                    Vector3 capsuleTop = transform.position + transform.up * (capsule.height - capsule.radius);

                    if (Physics.Raycast(capsuleBottom, transform.forward, out RaycastHit hit, capsule.radius + maxStableDistanceFromLedge, groundCheckLayers, QueryTriggerInteraction.Ignore))
                    {
                        if (hit.collider != null && !hit.collider.isTrigger)
                        {
                            if (hit.normal == Vector3.up)
                            {
                                if (Physics.Raycast(capsuleTop, transform.forward, out hit, capsule.radius + maxStableDistanceFromLedge, groundCheckLayers, QueryTriggerInteraction.Ignore))
                                {
                                    if (hit.collider != null && !hit.collider.isTrigger)
                                    {
                                        if (hit.normal == Vector3.up)
                                        {
                                            transform.position = transform.position + transform.forward * hit.distance;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CheckSteps()
        {
            if (GroundingStatus.FoundAnyGround)
            {
                Vector3 capsuleBottom = transform.position + transform.up * capsule.radius;
                Vector3 capsuleTop = transform.position + transform.up * (capsule.height - capsule.radius);

                // Cast capsule bottom
                if (Physics.Raycast(capsuleBottom, -transform.up, out RaycastHit hit, maxStepHeight, groundCheckLayers, QueryTriggerInteraction.Ignore))
                {
                    if (Vector3.Dot(transform.up, hit.normal) > 0f)
                    {
                        float hitDistance = hit.point.y - capsuleBottom.y;
                        if (hitDistance >= minRequiredStepDepth && hitDistance <= maxStepHeight)
                        {
                            transform.position = transform.position + transform.up * hitDistance;
                        }
                    }
                }

                // Cast capsule top
                if (Physics.Raycast(capsuleTop, -transform.up, out hit, maxStepHeight, groundCheckLayers, QueryTriggerInteraction.Ignore))
                {
                    if (Vector3.Dot(transform.up, hit.normal) > 0f)
                    {
                        float hitDistance = hit.point.y - capsuleTop.y;
                        if (hitDistance >= minRequiredStepDepth && hitDistance <= maxStepHeight)
                        {
                            transform.position = transform.position + transform.up * hitDistance;
                        }
                    }
                }
            }
        }

        private void CheckGrounding()
        {
            LastGroundingStatus.CopyFrom(GroundingStatus);

            float radius = capsule.radius * (1f - groundDetectionExtraDistance);
            float bottom = transform.position.y + capsule.radius * groundDetectionExtraDistance;
            float top = transform.position.y + capsule.height - capsule.radius * groundDetectionExtraDistance;

            GroundingStatus.FoundAnyGround = false;
            GroundingStatus.IsStableOnGround = false;
            GroundingStatus.SnappingPrevented = false;
            GroundingStatus.GroundNormal = Vector3.up;
            GroundingStatus.InnerGroundNormal = Vector3.up;
            GroundingStatus.OuterGroundNormal = Vector3.up;
            GroundingStatus.GroundPoint = Vector3.zero;
            GroundingStatus.GroundCollider = null;
            GroundingStatus.TimeSinceGrounded = float.MaxValue;

            float groundCheckDistance = capsule.radius + groundDetectionExtraDistance;

            Vector3 raycastOrigin = transform.position + (Vector3.up * groundCheckDistance);
            Vector3 downwardDir = -Vector3.up;

            if (Physics.Raycast(raycastOrigin, downwardDir, out RaycastHit hit, groundCheckDistance, groundCheckLayers, QueryTriggerInteraction.Ignore))
            {
                GroundingStatus.FoundAnyGround = true;
                GroundingStatus.GroundCollider = hit.collider;
                GroundingStatus.GroundPoint = hit.point;
                GroundingStatus.GroundNormal = hit.normal;

                float dot = Vector3.Dot(transform.up, hit.normal);
                GroundingStatus.IsStableOnGround = dot > 0f && dot >= Mathf.Cos(maxStableSlopeAngle * Mathf.Deg2Rad);

                GroundingStatus.InnerGroundNormal = hit.normal;

                Vector3 rejection = Vector3.ProjectOnPlane(transform.up, hit.normal);
                GroundingStatus.OuterGroundNormal = Vector3.RotateTowards(hit.normal, rejection, maxStableSlopeAngle * Mathf.Deg2Rad, 0f);

                if (GroundingStatus.IsStableOnGround)
                {
                    GroundingStatus.SnappingPrevented = false;
                    GroundingStatus.TimeSinceGrounded = 0f;
                }
            }

            if (GroundingStatus.FoundAnyGround && !GroundingStatus.IsStableOnGround)
            {
                if (Physics.Raycast(transform.position, -transform.up, out hit, capsule.height * 0.5f + groundCheckDistance, groundCheckLayers, QueryTriggerInteraction.Ignore))
                {
                    if (Vector3.Dot(transform.up, hit.normal) > 0f)
                    {
                        GroundingStatus.IsStableOnGround = true;
                        GroundingStatus.GroundCollider = hit.collider;
                        GroundingStatus.GroundPoint = hit.point;
                        GroundingStatus.GroundNormal = hit.normal;

                        GroundingStatus.InnerGroundNormal = hit.normal;

                        Vector3 rejection = Vector3.ProjectOnPlane(transform.up, hit.normal);
                        GroundingStatus.OuterGroundNormal = Vector3.RotateTowards(hit.normal, rejection, maxStableSlopeAngle * Mathf.Deg2Rad, 0f);

                        GroundingStatus.SnappingPrevented = true;
                        GroundingStatus.TimeSinceGrounded = 0f;
                    }
                }
            }
        }
    }
}
