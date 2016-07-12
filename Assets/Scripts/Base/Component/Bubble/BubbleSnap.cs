using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Effects;
using Service;

public class BubbleSnap : MonoBehaviour
{
    private Rigidbody2D rigidBody;
    private new CircleCollider2D collider;

    protected void Start()
    {
        gameObject.layer = (int)Layers.Default;
        rigidBody = GetComponent<Rigidbody2D>();
        collider = GetComponent<CircleCollider2D>();

        collider.radius *= GlobalState.Instance.Config.bubbles.shotColliderScale;
    }

    protected void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == StringConstants.Tags.BUBBLES)
        {
            AdjustToGrid();
            var origin = transform.position;

            foreach (var bubble in NearbyBubbles(origin))
            {
                AttachToBubble(bubble.gameObject);
                bubble.GetComponent<BubbleAttachments>().Model.SnapToBubble();
            }

            foreach (var bubble in NearbyBubbles(origin, GlobalState.Instance.Config.impactEffect.radius))
            {
                var bubbleEffectController = bubble.GetComponent<BubbleEffectController>();

                if (bubbleEffectController != null)
                {
                    bubbleEffectController.AddEffect(ImpactShockwaveEffect.Play(bubble.gameObject, origin));
                }
            }

            rigidBody.velocity = Vector2.zero;
            rigidBody.gravityScale = 1.0f;
            rigidBody.isKinematic = true;

            collider.radius /= GlobalState.Instance.Config.bubbles.shotColliderScale;
            gameObject.layer = (int)Layers.GameObjects;

            Destroy(this);
            GlobalState.Instance.Services.Get<EventService>().Dispatch(new BubbleSettlingEvent());

            GetComponent<BubbleAttachments>().Model.CheckForMatches();
            GlobalState.Instance.Services.Get<EventService>().Dispatch(new BubbleSettledEvent { shooter = gameObject });
        }
    }

    private void AdjustToGrid()
    {
        var myPosition = (Vector2)transform.position;
        var nearbyBubbles = NearbyBubbles(transform.position).Select(b => b.gameObject).ToArray();
        var attachPoints = GetAttachmentPoints(nearbyBubbles).OrderBy(p => (p - myPosition).sqrMagnitude).ToArray();

        foreach (var attachPoint in attachPoints)
        {
            if (CanPlaceAtLocation(attachPoint))
            {
                transform.position = attachPoint;
                return;
            }
        }
    }

    private void AttachToBubble(GameObject bubble)
    {
        var attachment = GetComponent<BubbleAttachments>();

        attachment.Attach(bubble);
        attachment.Model.MinimizeDistanceFromRoot();
        attachment.Model.SortNeighbors();
    }

    private IEnumerable<Collider2D> NearbyBubbles(Vector2 location)
    {
        return NearbyBubbles(location, GlobalState.Instance.Config.bubbles.size);
    }

    private IEnumerable<Collider2D> NearbyBubbles(Vector2 location, float radius)
    {
        foreach (var hit in Physics2D.CircleCastAll(location, radius, Vector2.up, 0.0f))
        {
            if ((hit.collider.gameObject != gameObject) && (hit.collider.gameObject.tag == StringConstants.Tags.BUBBLES))
            {
                yield return hit.collider;
            }
        }
    }

    private bool CanPlaceAtLocation(Vector2 location)
    {
        var halfSize = GlobalState.Instance.Config.bubbles.size / 2.0f;

        foreach (var hit in Physics2D.CircleCastAll(location, halfSize * 0.9f, Vector2.up, 0.0f,
                                                    (1 << (int)Layers.GameObjects | 1 << (int)Layers.Walls)))
        {
            if (hit.collider.gameObject != gameObject)
            {
                return false;
            }
        }

        return true;
    }

    private IEnumerable<Vector2> GetAttachmentPoints(IEnumerable<GameObject> bubbles)
    {
        var theta = Mathf.PI / 3.0f;
        var bubbleSize = GlobalState.Instance.Config.bubbles.size;

        foreach (var bubble in bubbles)
        {
            var bubblePosition = (Vector2)bubble.transform.position;

            for (var index = 0; index < 6; index++)
            {
                yield return new Vector2(
                    bubblePosition.x + Mathf.Cos(index * theta) * bubbleSize,
                    bubblePosition.y + Mathf.Sin(index * theta) * bubbleSize
                );
            }
        }
    }
}
