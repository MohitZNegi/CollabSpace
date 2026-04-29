import { useEffect, useRef, useState } from 'react';
import { useSelector } from 'react-redux';
import axiosInstance from '../../api/axiosInstance';
import '../../styles/components/comments.css';

// CommentItem renders itself and then recursively renders
// each of its replies as nested CommentItem components.
// This recursion naturally handles any depth of threading.
function CommentItem({ comment, cardId, onCommentAdded,
    onCommentEdited, onCommentDeleted, highlightedCommentId = null,
    isReply = false }) {

    const { user } = useSelector((s) => s.auth);
    const commentRef = useRef(null);
    const [showReply, setShowReply] = useState(false);
    const [isEditing, setIsEditing] = useState(false);
    const [replyContent, setReplyContent] = useState('');
    const [editContent, setEditContent] = useState(comment.content);
    const [isSubmitting, setIsSubmitting] = useState(false);

    const isOwn = comment.userId === user?.id;
    const isAdmin = user?.globalRole === 'Admin';
    const isHighlighted = highlightedCommentId === comment.id;

    useEffect(() => {
        if (isHighlighted) {
            commentRef.current?.scrollIntoView({
                behavior: 'smooth',
                block: 'center',
            });
        }
    }, [isHighlighted]);

    const formatTime = (timestamp) => {
        const diff = Math.floor(
            (new Date() - new Date(timestamp)) / 60000);
        if (diff < 1) return 'Just now';
        if (diff < 60) return `${diff}m ago`;
        if (diff < 1440) return `${Math.floor(diff / 60)}h ago`;
        return new Date(timestamp).toLocaleDateString();
    };

    const handleReply = async () => {
        const content = replyContent.trim();
        if (!content || isSubmitting) return;

        setIsSubmitting(true);
        try {
            const response = await axiosInstance.post(
                `/cards/${cardId}/comments`,
                { content, parentCommentId: comment.id });

            onCommentAdded(response.data);
            setReplyContent('');
            setShowReply(false);
        } catch (error) {
            console.error('Failed to post reply:', error);
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleEdit = async () => {
        const content = editContent.trim();
        if (!content || isSubmitting) return;

        setIsSubmitting(true);
        try {
            const response = await axiosInstance.patch(
                `/comments/${comment.id}`,
                { content });

            onCommentEdited(response.data);
            setIsEditing(false);
        } catch (error) {
            console.error('Failed to edit comment:', error);
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleDelete = async () => {
        if (!window.confirm('Delete this comment?')) return;

        try {
            await axiosInstance.delete(`/comments/${comment.id}`);
            onCommentDeleted(comment.id);
        } catch (error) {
            console.error('Failed to delete comment:', error);
        }
    };

    return (
        <div>
            <div
                ref={commentRef}
                className={`comment-item ${isReply ? 'reply' : ''} ${
                    isHighlighted ? 'highlighted' : ''
                }`}
            >
                <div className="comment-avatar">
                    {comment.username[0].toUpperCase()}
                </div>

                <div className="comment-body">
                    <div className="comment-header">
                        <span className="comment-username">
                            {comment.username}
                        </span>
                        <span className="comment-time">
                            {formatTime(comment.createdAt)}
                        </span>
                        {comment.isEdited && (
                            <span className="comment-edited-label">
                                edited
                            </span>
                        )}
                    </div>

                    {isEditing ? (
                        <div className="comment-input-row">
                            <textarea
                                className="comment-textarea"
                                value={editContent}
                                onChange={(e) =>
                                    setEditContent(e.target.value)}
                                autoFocus
                                rows={2}
                            />
                            <div style={{
                                display: 'flex',
                                flexDirection: 'column', gap: '0.3rem'
                            }}>
                                <button
                                    className="btn-primary"
                                    onClick={handleEdit}
                                    disabled={isSubmitting}
                                    style={{
                                        padding: '0.3rem 0.7rem',
                                        fontSize: '0.8rem'
                                    }}
                                >
                                    Save
                                </button>
                                <button
                                    className="btn-secondary"
                                    onClick={() => {
                                        setIsEditing(false);
                                        setEditContent(comment.content);
                                    }}
                                    style={{
                                        padding: '0.3rem 0.7rem',
                                        fontSize: '0.8rem'
                                    }}
                                >
                                    Cancel
                                </button>
                            </div>
                        </div>
                    ) : (
                        <p className="comment-content">{comment.content}</p>
                    )}

                    <div className="comment-actions">
                        {/* Only show reply on top-level comments
                            to prevent deeply nested threading */}
                        {!isReply && (
                            <button
                                className="comment-action-btn"
                                onClick={() => setShowReply(p => !p)}
                            >
                                Reply
                            </button>
                        )}
                        {isOwn && !isEditing && (
                            <button
                                className="comment-action-btn"
                                onClick={() => setIsEditing(true)}
                            >
                                Edit
                            </button>
                        )}
                        {(isOwn || isAdmin) && (
                            <button
                                className="comment-action-btn delete"
                                onClick={handleDelete}
                            >
                                Delete
                            </button>
                        )}
                    </div>

                    {/* Inline reply form */}
                    {showReply && (
                        <div className="comment-reply-form">
                            <div className="comment-input-row">
                                <textarea
                                    className="comment-textarea"
                                    placeholder={`Reply to ${comment.username}...`}
                                    value={replyContent}
                                    onChange={(e) =>
                                        setReplyContent(e.target.value)}
                                    autoFocus
                                    rows={2}
                                />
                                <div style={{
                                    display: 'flex',
                                    flexDirection: 'column', gap: '0.3rem'
                                }}>
                                    <button
                                        className="btn-primary"
                                        onClick={handleReply}
                                        disabled={isSubmitting
                                            || !replyContent.trim()}
                                        style={{
                                            padding: '0.3rem 0.7rem',
                                            fontSize: '0.8rem'
                                        }}
                                    >
                                        Post
                                    </button>
                                    <button
                                        className="btn-secondary"
                                        onClick={() => {
                                            setShowReply(false);
                                            setReplyContent('');
                                        }}
                                        style={{
                                            padding: '0.3rem 0.7rem',
                                            fontSize: '0.8rem'
                                        }}
                                    >
                                        Cancel
                                    </button>
                                </div>
                            </div>
                        </div>
                    )}
                </div>
            </div>

            {/* Recursively render replies.
                isReply=true prevents reply buttons on nested comments,
                keeping threading to one level deep in the UI. */}
            {comment.replies?.map((reply) => (
                <CommentItem
                    key={reply.id}
                    comment={reply}
                    cardId={cardId}
                    onCommentAdded={onCommentAdded}
                    onCommentEdited={onCommentEdited}
                    onCommentDeleted={onCommentDeleted}
                    highlightedCommentId={highlightedCommentId}
                    isReply={true}
                />
            ))}
        </div>
    );
}

export default CommentItem;
