namespace CollabSpace.Models.Constants
{
    // Constants prevent typos when creating or querying notifications.
    // Using a static class with string constants instead of an enum
    // means the values are stored as readable strings in the database
    // without any conversion logic.
    public static class NotificationTypes
    {
        public const string CardUpdated = "CardUpdated";
        public const string CardAssigned = "CardAssigned";
        public const string CommentAdded = "CommentAdded";
        public const string Mention = "Mention";
        public const string MemberJoined = "MemberJoined";
        public const string MemberRemoved = "MemberRemoved";
    }
}