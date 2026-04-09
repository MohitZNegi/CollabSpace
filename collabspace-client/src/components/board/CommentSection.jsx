import { useState, useEffect } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import {
    setComments, commentAdded,
    commentEdited, commentDeleted
} from '../../features/comments/commentSlice';
import axiosInstance from '../../api/axiosInstance';
import CommentItem from './CommentItem';
import '../../styles/components/comments.css';

function CommentSection({ cardId }) {
    const dispatch = useDispatch();
    const { user } = useSelector((s) => s.auth);
    const comments = useSelector(
        (s) => s.comments.byCardId[cardId] || []);
    const isLoading = useSelector((s) => s.comments.isLoading);

    const [newContent, setNewContent] = useState('');
    const [isPosting, setIsPosting] = useState(false);

    // Load comments when the card modal opens
    useEffect(() => {
        const load = async () => {
            try {
                const response = await axiosInstance.get(
                    `/cards/${cardId}/comments`);
                dispatch(setComments({
                    cardId,
                    comments: response.data
                }));
            } catch (error) {
                console.error('Failed to load comments:', error);
            }
        };
        load();
    }, [cardId, dispatch]);

    const handlePost = async () => {
        const content = newContent.trim();
        if (!content || isPosting) return;

        setIsPosting(true);
        try {
            const response = await axiosInstance.post(
                `/cards/${cardId}/comments`,
                { content });

            dispatch(commentAdded({ cardId, comment: response.data }));
            setNewContent('');
        } catch (error) {
            console.error('Failed to post comment:', error);
        } finally {
            setIsPosting(false);
        }
    };

    const handleKeyDown = (e) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            handlePost();
        }
    };

    const handleCommentAdded = (comment) => {
        dispatch(commentAdded({ cardId, comment }));
    };

    const handleCommentEdited = (comment) => {
        dispatch(commentEdited({ cardId, comment }));
    };

    const handleCommentDeleted = (commentId) => {
        dispatch(commentDeleted({ cardId, commentId }));
    };

    return (
        <div className="comments-section">
            <span className="comments-title">
                Comments ({comments.length})
            </span>

            {isLoading ? (
                <p className="comment-empty">Loading comments...</p>
            ) : comments.length === 0 ? (
                <p className="comment-empty">
                    No comments yet. Be the first to comment.
                </p>
            ) : (
                <div className="comments-list">
                    {comments.map((comment) => (
                        <CommentItem
                            key={comment.id}
                            comment={comment}
                            cardId={cardId}
                            onCommentAdded={handleCommentAdded}
                            onCommentEdited={handleCommentEdited}
                            onCommentDeleted={handleCommentDeleted}
                            isReply={false}
                        />
                    ))}
                </div>
            )}

            {/* New top-level comment input */}
            <div className="new-comment-form">
                <div className="new-comment-avatar">
                    {user?.username?.[0]?.toUpperCase() || '?'}
                </div>
                <div className="new-comment-input-wrapper">
                    <textarea
                        className="comment-textarea"
                        placeholder="Write a comment... Use @username to mention someone"
                        value={newContent}
                        onChange={(e) => setNewContent(e.target.value)}
                        onKeyDown={handleKeyDown}
                        rows={2}
                    />
                    <div style={{
                        display: 'flex',
                        justifyContent: 'flex-end'
                    }}>
                        <button
                            className="btn-primary"
                            onClick={handlePost}
                            disabled={isPosting || !newContent.trim()}
                            style={{ fontSize: 'var(--font-size-sm)' }}
                        >
                            {isPosting ? 'Posting...' : 'Post comment'}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default CommentSection;