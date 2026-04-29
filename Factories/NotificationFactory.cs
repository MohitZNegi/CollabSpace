using CollabSpace.Models;
using CollabSpace.Models.Constants;

namespace CollabSpace.Factories
{
    public static class NotificationFactory
    {
        // Each method knows exactly what a specific notification type
        // needs: who receives it, what the message says, and what
        // entity it links to. Callers provide context, not construction.

        public static Notification CardUpdated(
            Guid recipientId, string cardTitle,
            string updatedByUsername, Guid cardId,
            Guid boardId, Guid workspaceId)
        {
            return Create(
                recipientId,
                NotificationTypes.CardUpdated,
                $"{updatedByUsername} updated card \"{cardTitle}\"",
                cardId,
                $"/workspaces/{workspaceId}/boards/{boardId}?card={cardId}");
        }

        public static Notification CardAssigned(
            Guid recipientId, string cardTitle,
            string assignedByUsername, Guid cardId,
            Guid boardId, Guid workspaceId)
        {
            return Create(
                recipientId,
                NotificationTypes.CardAssigned,
                $"{assignedByUsername} assigned you to \"{cardTitle}\"",
                cardId,
                $"/workspaces/{workspaceId}/boards/{boardId}?card={cardId}");
        }

        public static Notification CommentAdded(
            Guid recipientId, string cardTitle,
            string commenterUsername, Guid commentId,
            Guid cardId, Guid boardId, Guid workspaceId)
        {
            return Create(
                recipientId,
                NotificationTypes.CommentAdded,
                $"{commenterUsername} commented on \"{cardTitle}\"",
                commentId,
                $"/workspaces/{workspaceId}/boards/{boardId}?card={cardId}&comment={commentId}");
        }

        public static Notification Mention(
            Guid recipientId, string mentionedByUsername,
            string context, Guid referenceId,
            string? navigationUrl = null)
        {
            return Create(
                recipientId,
                NotificationTypes.Mention,
                $"{mentionedByUsername} mentioned you in {context}",
                referenceId,
                navigationUrl);
        }

        public static Notification MemberJoined(
            Guid recipientId, string newMemberUsername,
            string workspaceName, Guid workspaceId)
        {
            return Create(
                recipientId,
                NotificationTypes.MemberJoined,
                $"{newMemberUsername} joined {workspaceName}",
                workspaceId,
                $"/workspaces/{workspaceId}");
        }

        public static Notification MemberRemoved(
            Guid recipientId, string workspaceName, Guid workspaceId)
        {
            return Create(
                recipientId,
                NotificationTypes.MemberRemoved,
                $"You were removed from {workspaceName}",
                workspaceId,
                "/dashboard");
        }

        // Private builder: all public methods funnel through here.
        // Guarantees every notification has an ID, timestamp, and
        // IsRead defaulting to false without repeating that logic.
        private static Notification Create(
            Guid recipientId, string type,
            string message, Guid referenceId,
            string? navigationUrl = null)
        {
            return new Notification
            {
                Id = Guid.NewGuid(),
                RecipientUserId = recipientId,
                Type = type,
                Message = message,
                ReferenceId = referenceId,
                NavigationUrl = navigationUrl,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
