import { useEffect, useRef } from 'react';
import { useDispatch } from 'react-redux';
import {
    workspaceMessageReceived,
    directMessageReceived,
    typingStarted,
    typingStopped
} from '../features/chat/chatSlice';
import { joinWorkspace, leaveWorkspace, onEvent }
    from '../services/signalrService';

export const useChatSignalR = (workspaceId) => {
    const dispatch = useDispatch();
    // useRef stores typing timers without causing re-renders.
    // Each key is a userId, each value is a timeout ID.
    // When a new typing event arrives, we clear the existing
    // timeout and start a new one.
    const typingTimers = useRef({});

    useEffect(() => {
        if (!workspaceId) return;

        joinWorkspace(workspaceId);

        const cleanups = [
            onEvent('ReceiveWorkspaceMessage', (message) => {
                dispatch(workspaceMessageReceived(message));
            }),

            onEvent('UserTyping', (data) => {
                dispatch(typingStarted({
                    contextId: data.contextId,
                    userId: data.userId,
                    username: data.username,
                }));

                // Clear any existing timer for this user
                if (typingTimers.current[data.userId]) {
                    clearTimeout(typingTimers.current[data.userId]);
                }

                // Auto-remove the indicator after 3 seconds of silence.
                typingTimers.current[data.userId] = setTimeout(() => {
                    dispatch(typingStopped({
                        contextId: data.contextId,
                        userId: data.userId,
                    }));
                    delete typingTimers.current[data.userId];
                }, 3000);
            }),
        ];

        return () => {
            leaveWorkspace(workspaceId);
            cleanups.forEach(cleanup => cleanup());
            // Clear all pending typing timers on unmount
            Object.values(typingTimers.current)
                .forEach(clearTimeout);
        };
    }, [workspaceId, dispatch]);
};

// Separate hook for direct message events.
// Registered at the app level rather than per-conversation
// because DMs can arrive from anyone at any time.
export const useDirectMessageSignalR = () => {
    const dispatch = useDispatch();
    const typingTimers = useRef({});

    useEffect(() => {
        const cleanups = [
            onEvent('ReceiveDirectMessage', (message) => {
                dispatch(directMessageReceived(message));
            }),

            onEvent('UserTyping', (data) => {
                if (data.context !== 'direct') return;
                dispatch(typingStarted({
                    contextId: data.contextId,
                    userId: data.userId,
                    username: data.username,
                }));

                if (typingTimers.current[data.userId]) {
                    clearTimeout(typingTimers.current[data.userId]);
                }

                typingTimers.current[data.userId] = setTimeout(() => {
                    dispatch(typingStopped({
                        contextId: data.contextId,
                        userId: data.userId,
                    }));
                }, 3000);
            }),
        ];

        return () => {
            cleanups.forEach(cleanup => cleanup());
            Object.values(typingTimers.current).forEach(clearTimeout);
        };
    }, [dispatch]);
};