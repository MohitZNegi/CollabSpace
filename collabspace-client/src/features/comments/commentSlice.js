import { createSlice } from '@reduxjs/toolkit';

const commentSlice = createSlice({
    name: 'comments',
    initialState: {
        // Comments keyed by cardId for efficient lookup
        byCardId: {},
        isLoading: false,
    },
    reducers: {
        setComments: (state, action) => {
            const { cardId, comments } = action.payload;
            state.byCardId[cardId] = comments;
        },

        // Adds a new top-level comment or reply in the correct position.
        // For replies, finds the parent and appends to its replies array.
        commentAdded: (state, action) => {
            const { cardId, comment } = action.payload;
            if (!state.byCardId[cardId]) {
                state.byCardId[cardId] = [];
            }

            if (!comment.parentCommentId) {
                // Top-level comment goes at the end of the list
                state.byCardId[cardId].push(comment);
            } else {
                // Reply: find the parent and append there
                const addReply = (comments) => {
                    for (const c of comments) {
                        if (c.id === comment.parentCommentId) {
                            c.replies = c.replies || [];
                            c.replies.push(comment);
                            return true;
                        }
                        if (c.replies?.length && addReply(c.replies)) {
                            return true;
                        }
                    }
                    return false;
                };
                addReply(state.byCardId[cardId]);
            }
        },

        commentEdited: (state, action) => {
            const { cardId, comment } = action.payload;
            if (!state.byCardId[cardId]) return;

            // Recursively find and update the comment at any nesting level
            const updateComment = (comments) => {
                for (let i = 0; i < comments.length; i++) {
                    if (comments[i].id === comment.id) {
                        comments[i] = { ...comments[i], ...comment };
                        return true;
                    }
                    if (comments[i].replies?.length
                        && updateComment(comments[i].replies)) {
                        return true;
                    }
                }
                return false;
            };
            updateComment(state.byCardId[cardId]);
        },

        commentDeleted: (state, action) => {
            const { cardId, commentId } = action.payload;
            if (!state.byCardId[cardId]) return;

            const removeComment = (comments) => {
                const index = comments.findIndex(c => c.id === commentId);
                if (index !== -1) {
                    comments.splice(index, 1);
                    return true;
                }
                for (const c of comments) {
                    if (c.replies?.length
                        && removeComment(c.replies)) return true;
                }
                return false;
            };
            removeComment(state.byCardId[cardId]);
        },

        setLoading: (state, action) => {
            state.isLoading = action.payload;
        },
    },
});

export const {
    setComments, commentAdded,
    commentEdited, commentDeleted, setLoading
} = commentSlice.actions;

export default commentSlice.reducer;