import { createSlice } from '@reduxjs/toolkit';

const chatSlice = createSlice({
    name: 'chat',
    initialState: {
        // Workspace messages keyed by workspaceId for quick lookup
        workspaceMessages: {},
        // Direct messages keyed by the other user's ID
        directMessages: {},
        // Tracks who is currently typing in each context
        typingUsers: {},
        isLoading: false,
    },
    reducers: {
        // Set the initial message history for a workspace
        setWorkspaceMessages: (state, action) => {
            const { workspaceId, messages } = action.payload;
            state.workspaceMessages[workspaceId] = messages;
        },

        // Called when SignalR broadcasts ReceiveWorkspaceMessage.
        // Appends the new message to the workspace's message list.
        workspaceMessageReceived: (state, action) => {
            const { workspaceId } = action.payload;
            if (!state.workspaceMessages[workspaceId]) {
                state.workspaceMessages[workspaceId] = [];
            }
            // Prevent duplicates if the sender also receives their own
            // broadcast (idempotent insert)
            const exists = state.workspaceMessages[workspaceId]
                .some(m => m.id === action.payload.id);
            if (!exists) {
                state.workspaceMessages[workspaceId].push(action.payload);
            }
        },

        setDirectMessages: (state, action) => {
            const { otherUserId, messages } = action.payload;
            state.directMessages[otherUserId] = messages;
        },

        // Called when SignalR broadcasts ReceiveDirectMessage
        directMessageReceived: (state, action) => {
            const { senderId } = action.payload;
            if (!state.directMessages[senderId]) {
                state.directMessages[senderId] = [];
            }
            state.directMessages[senderId].push(action.payload);
        },

        // Adds a typing indicator. Removes it after a timeout
        // handled in the component using setTimeout.
        typingStarted: (state, action) => {
            const { contextId, userId, username } = action.payload;
            if (!state.typingUsers[contextId]) {
                state.typingUsers[contextId] = {};
            }
            state.typingUsers[contextId][userId] = username;
        },

        typingStopped: (state, action) => {
            const { contextId, userId } = action.payload;
            if (state.typingUsers[contextId]) {
                delete state.typingUsers[contextId][userId];
            }
        },

        setLoading: (state, action) => {
            state.isLoading = action.payload;
        },
    },
});

export const {
    setWorkspaceMessages, workspaceMessageReceived,
    setDirectMessages, directMessageReceived,
    typingStarted, typingStopped, setLoading
} = chatSlice.actions;

export default chatSlice.reducer;