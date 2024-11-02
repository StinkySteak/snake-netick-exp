using Netick;
using Netick.Unity;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterSnake : NetworkBehaviour
{
    [Networked]
    public Vector2 SnakeHeadPosition { get; set; }
    [Networked]
    public float SnakeHeadRotation { get; set; }

    public Transform InterpolationTarget;
    public float Speed = 5f;

    [Networked(size: 500)]
    public readonly NetworkArray<Angle> Angles = new NetworkArray<Angle>(500);

    public List<Transform> SnakeParts;

    public float DistanceBetweenParts = 1.0f;

    public int BodyParts = 30;

    public override void NetworkStart()
    {
        if (IsServer)
        {
            InputSource = Sandbox.LocalPlayer;

            for (int i = 0; i < BodyParts; i++)
            {
                var newBodyPart = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                newBodyPart.transform.position = new Vector3(-1 * i, 0, 0);
                SnakeParts.Add(newBodyPart.transform);
            }
        }

        if (IsInputSource)
        {
            Camera.main.GetComponent<CameraController>().Target = SnakeParts[0];
        }
    }

    public override void NetworkUpdate()
    {
        if (!IsInputSource) return;

        Vector2 cursorPositionInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        PlayerInput input = new();
        input.MoveDirection = cursorPositionInWorld;

        Sandbox.SetInput(input);
    }

    public override void NetworkFixedUpdate()
    {
        if (FetchInput(out PlayerInput snakeInput))
        {
            Vector3 target = snakeInput.MoveDirection;
            target.z = 0;
            var head = SnakeParts[0];

            head.transform.right = (target - head.transform.position).normalized;
            head.transform.position = head.transform.position + (head.transform.right * Speed) * Time.fixedDeltaTime;

            var parent = SnakeParts[0];

            for (int i = 1; i < (SnakeParts.Count); i++)
            {
                var child = SnakeParts[i];

                var dir = (parent.transform.position - child.transform.position).normalized;

                child.transform.position = parent.transform.position + (-dir * DistanceBetweenParts);
                child.transform.right = dir;
                parent = child;
            }
        }
    }

    public override void GameEngineIntoNetcode()
    {
        Transform parent = SnakeParts[0]; // the head
        SnakeHeadPosition = parent.transform.position;
        SnakeHeadRotation = Mathf.Atan2(parent.transform.right.y, parent.transform.right.x);
        // we start at the child after the head, that's why i = 1, and

        int startIndex = 1;

        for (int i = startIndex; i < SnakeParts.Count; i++)
        {
            var child = SnakeParts[i];
            float angle = Mathf.Atan2(child.transform.right.y, child.transform.right.x);

            // the head is not child of this array, so we start at i-1
            Angles[i - 1] = new(angle);

            parent = SnakeParts[i];
        }
    }

    public override void NetcodeIntoGameEngine()
    {
        Transform parent = SnakeParts[0]; // the head
        parent.transform.position = SnakeHeadPosition;
        parent.transform.right = new Vector3(Mathf.Cos(SnakeHeadRotation), Mathf.Sin(SnakeHeadRotation), 0f);

        // we start at the child after the head, that's why i = 1, and
        int startIndex = 1;

        for (int i = startIndex; i < SnakeParts.Count; i++)
        {
            var child = SnakeParts[i];
            var angle = Angles[i - 1];
            var direction = -new Vector3(Mathf.Cos(angle.Value), Mathf.Sin(angle.Value), 0f);
            child.transform.position = parent.transform.position + (DistanceBetweenParts * direction);

            // the direction is pointing from the parent to the child.
            // we need the inverse for the rotation of the child.

            child.transform.right = direction;
            parent = SnakeParts[i];
        }
    }

    private void OnDrawGizmos()
    {
        if (Object == null || !Object.HasValidId) return;


    }
}

public struct PlayerInput : INetworkInput
{
    public Vector2 MoveDirection;
}

[Networked]
public struct Angle
{
    [Networked(precision: 0.01f)]
    public float Value { get; set; }

    public Angle(float angle)
    {
        Value = angle;
    }
}