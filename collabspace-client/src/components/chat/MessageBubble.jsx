// Formats a timestamp into a readable short time string.
// "10:34 AM" rather than a full date for messages from today.
const formatTime = (timestamp) => {
    return new Date(timestamp).toLocaleTimeString([], {
        hour: '2-digit',
        minute: '2-digit',
    });
};

function MessageBubble({ message, isOwn }) {
    return (
        <div className={`message-bubble ${isOwn ? 'own' : 'other'}`}>
            {!isOwn && (
                <span className="message-sender">
                    {message.senderUsername}
                </span>
            )}
            <div className="message-content">
                {message.content}
            </div>
            <span className="message-time">
                {formatTime(message.sentAt)}
                {message.isEdited && (
                    <span className="message-edited"> · edited</span>
                )}
            </span>
        </div>
    );
}

export default MessageBubble;