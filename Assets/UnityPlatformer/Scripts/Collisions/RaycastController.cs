/**!
The MIT License (MIT)

Copyright (c) 2015 Sebastian
Original file: https://github.com/SebLague/2DPlatformer-Tutorial/blob/master/Episode%2011/RaycastController.cs

Modifications (c) 2016 Luis Lafuente

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
**/

﻿using System;
﻿using UnityEngine;
using System.Collections;

namespace UnityPlatformer {
  /// <summary>
  /// Raycast helper
  /// </summary>
  [RequireComponent (typeof (BoxCollider2D))]
  public class RaycastController : MonoBehaviour {
    /// <summary>
    /// Static geometry mask
    /// </summary>
    public LayerMask collisionMask;
    /// <summary>
    /// How far from then env the Character must be.
    ///
    /// NOTE: must be less than skinWidth, to allow continuous ground contact
    /// </summary>
    public float minDistanceToEnv = 0.1f;
    /// <summary>
    /// Defines how far in from the edges of the collider rays are we going to cast from.
    ///
    /// NOTE: This value must be greater than minDistanceToEnv
    /// </summary>
    public float skinWidth = 0.2f;
    /// <summary>
    /// How many rays to check horizontal collisions
    /// </summary>
    public int horizontalRayCount = 4;
    /// <summary>
    /// How many rays to check vertical collisions
    /// </summary>
    public int verticalRayCount = 4;
    /// <summary>
    /// Horizontal space between vertical rays
    /// </summary>
    internal float horizontalRaySpacing;
    /// <summary>
    /// Vertical space between horizontal rays
    /// </summary>
    internal float verticalRaySpacing;
    /// <summary>
    /// BoxCollider2D
    /// </summary>
    internal BoxCollider2D box;
    /// <summary>
    /// Raycast origins
    /// </summary>
    internal RaycastOrigins raycastOrigins;
    /// <summary>
    /// horizontal rays result, to not allocate them each frame
    /// </summary>
    internal RaycastHit2D[] horizontalRays;
    /// <summary>
    /// vertical rays result, to not allocate them each frame
    /// </summary>
    internal RaycastHit2D[] verticalRays;
    /// <summary>
    /// cache: shrinked box.bounds
    /// </summary>
    internal Bounds bounds;
    /// <summary>
    /// cache: Mathf.Sqrt(skinWidth + skinWidth)
    /// </summary>
    internal float skinWidthMagnitude;
    /// <summary>
    /// Recalculate everything
    /// </summary>
    public virtual void OnEnable() {
      box = GetComponent<BoxCollider2D> ();
      CalculateRaySpacing ();
      UpdateInnerBounds();

      skinWidthMagnitude = Mathf.Sqrt(skinWidth + skinWidth);

      if (horizontalRays == null) {
        horizontalRays = new RaycastHit2D[horizontalRayCount];
      }

      if (verticalRays == null) {
        verticalRays = new RaycastHit2D[verticalRayCount];
      }
    }
    /// <summary>
    /// Recalculate shrinked the bounds
    /// </summary>
    public void UpdateInnerBounds() {
      bounds = box.bounds;
      // * 2 so it's shrink skinWidth by each side
      bounds.Expand (skinWidth * -2);
    }
    /// <summary>
    /// Recalculate raycastOrigins
    /// </summary>
    public void UpdateRaycastOrigins() {
      UpdateInnerBounds();
      CalculateRaySpacing();

      // cache
      Vector3 min = bounds.min;
      Vector3 max = bounds.max;
      float half_width = bounds.size.x * 0.5f;

      raycastOrigins.bottomLeft = new Vector2 (min.x, min.y);
      raycastOrigins.bottomCenter = new Vector2 (min.x + half_width, min.y);
      raycastOrigins.bottomRight = new Vector2 (max.x, min.y);
      raycastOrigins.topLeft = new Vector2 (min.x, max.y);
      raycastOrigins.topCenter = new Vector2 (min.x + half_width, max.y);
      raycastOrigins.topRight = new Vector2 (max.x, max.y);
    }

    /// <summary>
    /// Recalculate distance between rays (horizontalRaySpacing & verticalRaySpacing)
    /// </summary>
    public void CalculateRaySpacing() {
      horizontalRayCount = Mathf.Clamp (horizontalRayCount, 2, int.MaxValue);
      verticalRayCount = Mathf.Clamp (verticalRayCount, 2, int.MaxValue);

      horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
      verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }
    /// <summary>
    /// Call Physics2D.Raycast and Draw the ray to debug
    /// </summary>
    public RaycastHit2D Raycast(Vector2 origin, Vector2 direction, float rayLength, int mask, Color? color = null) {
      Debug.DrawRay(origin, direction * rayLength, color ?? Color.red);

      Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0,220,0) * new Vector3(0,0,1);
      Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0,140,0) * new Vector3(0,0,1);
      Debug.DrawRay(origin + direction * rayLength, right * rayLength * 0.2f, color ?? Color.red);
      Debug.DrawRay(origin + direction * rayLength, left * rayLength * 0.2f, color ?? Color.red);

      return Physics2D.Raycast(origin, direction, rayLength, mask);
    }
    /// <summary>
    /// Raycast Origins
    /// </summary>
    [Serializable]
    public struct RaycastOrigins {
      /// <summary>
      /// Top left
      /// </summary>
      public Vector2 topLeft;
      /// <summary>
      /// Top center
      /// </summary>
      public Vector2 topCenter;
      /// <summary>
      /// Top right
      /// </summary>
      public Vector2 topRight;
      /// <summary>
      /// Bottom left
      /// </summary>
      public Vector2 bottomLeft;
      /// <summary>
      /// Bottom center
      /// </summary>
      public Vector2 bottomCenter;
      /// <summary>
      /// Bottom right
      /// </summary>
      public Vector2 bottomRight;
    };

    /// <summary>
    /// Return RaycastHit2D of Raycasting at given index
    /// </summary>
    public RaycastHit2D VerticalRay(float directionY, int index, float rayLength, ref Vector3 velocity, Color? c = null) {
        Vector2 rayOrigin = (directionY == -1) ?
          raycastOrigins.bottomLeft :
          raycastOrigins.topLeft;

        rayOrigin += Vector2.right * (verticalRaySpacing * index + velocity.x);
        RaycastHit2D hit = Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask, c ?? Color.red);

        return hit;
    }
    /// <summary>
    /// Return RaycastHit2D of Raycasting at bottom center.
    /// </summary>
    public RaycastHit2D FeetRay(float rayLength, LayerMask mask) {
      return Raycast(raycastOrigins.bottomCenter, Vector2.down, rayLength, mask, Color.blue);
    }
    /// <summary>
    /// Callback for iterate rays
    /// </summary>
    public delegate void RayItr(ref RaycastHit2D hit, ref Vector3 velocity, int dir, int idx);
    /// <summary>
    /// Iterate over all right/horizontal rays
    /// </summary>
    public void ForeachRightRay(float rayLength, ref Vector3 velocity, RayItr itr) {
      if (velocity.x > 0) {
        rayLength += velocity.x;
      }

      Vector3 origin = raycastOrigins.bottomRight;
      origin.y += velocity.y;

      for (int i = 0; i < horizontalRayCount; i ++) {

        horizontalRays[i] = Raycast(origin, Vector2.right, rayLength, collisionMask, new Color(1, 0, 0, 0.5f));
        origin.y += horizontalRaySpacing;

        itr(ref horizontalRays[i], ref velocity, 1, i);
      }
    }
    /// <summary>
    /// Iterate over all left/horizontal rays
    /// </summary>
    public void ForeachLeftRay(float rayLength, ref Vector3 velocity, RayItr itr) {
      if (velocity.x < 0) {
        rayLength -= velocity.x;
      }

      Vector3 origin = raycastOrigins.bottomLeft;
      origin.y += velocity.y;

      for (int i = 0; i < horizontalRayCount; i ++) {

        horizontalRays[i] = Raycast(origin, Vector2.left, rayLength, collisionMask, new Color(1, 0, 0, 0.5f));
        origin.y += horizontalRaySpacing;

        itr(ref horizontalRays[i], ref velocity, -1, i);
      }
    }
    /// <summary>
    /// Iterate over all head/vertical rays
    /// </summary>
    public void ForeachHeadRay(float rayLength, ref Vector3 velocity, RayItr itr) {
      if (velocity.y > 0) {
        rayLength += velocity.y;
      }

      Vector3 origin = raycastOrigins.topLeft;
      origin.x += velocity.x;

      for (int i = 0; i < verticalRayCount; i ++) {

        verticalRays[i] = Raycast(origin, Vector2.up, rayLength, collisionMask, new Color(1, 0, 0, 0.5f));
        origin.x += verticalRaySpacing;

        itr(ref verticalRays[i], ref velocity, 1, i);
      }
    }
    /// <summary>
    /// Iterate over all feet/vertical rays
    /// </summary>
    public void ForeachFeetRay(float rayLength, ref Vector3 velocity, RayItr itr) {
      Vector3 origin = raycastOrigins.bottomLeft;
      origin.x += velocity.x;
      float length;

      for (int i = 0; i < verticalRayCount; i ++) {
        length = velocity.y < 0 ? rayLength - velocity.y : rayLength;

        verticalRays[i] = Raycast(origin, Vector2.down, length, collisionMask, new Color(1, 0, 0, 0.5f));
        origin.x += verticalRaySpacing;

        itr(ref verticalRays[i], ref velocity, -1, i);
      }
    }
    /// <summary>
    /// Return RaycastHit2D of Raycasting at bottom left.
    /// </summary>
    public RaycastHit2D LeftFeetRay(float rayLength, Vector3 velocity) {
      if (velocity.y < 0) {
        rayLength -= velocity.y;
      }

      Vector3 origin = raycastOrigins.bottomLeft;
      origin.x += velocity.x;

      return Raycast(origin, Vector2.down, rayLength, collisionMask, Color.cyan);
    }
    /// <summary>
    /// Return RaycastHit2D of Raycasting at bottom right.
    /// </summary>
    public RaycastHit2D RightFeetRay(float rayLength, Vector3 velocity) {
      if (velocity.y < 0) {
        rayLength -= velocity.y;
      }

      Vector3 origin = raycastOrigins.bottomLeft;
      origin.x += velocity.x + verticalRaySpacing * verticalRayCount;

      return Raycast(origin, Vector2.down, rayLength, collisionMask, Color.magenta);
    }
  }
}
