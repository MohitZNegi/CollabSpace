import { useEffect } from 'react';
import { useDispatch } from 'react-redux';
import {
    cardCreated, cardUpdated, cardMoved,
    cardDeleted, userJoinedBoard, userLeftBoard
} from '../features/board/boardSlice';
import {
    joinBoard, leaveBoard, onEvent
} from '../services/signalrService';

// A custom hook encapsulates the SignalR setup and teardown
// for a board. The component just calls this hook with a boardId.
// The hook handles joining, listening, and cleaning up.
// When the component unmounts (user leaves the board page),
// cleanup runs automatically via useEffect's return function.
export const useBoardSignalR = (boardId) => {
    const dispatch = useDispatch();

    useEffect(() => {
        if (!boardId) return;

        // Join the board group on SignalR
        joinBoard(boardId);

        // Register listeners for each event type.
        // Each returns a cleanup function that removes the listener.
        const cleanups = [
            onEvent('CardCreated', (card) =>
                dispatch(cardCreated(card))),

            onEvent('CardUpdated', (card) =>
                dispatch(cardUpdated(card))),

            onEvent('CardMoved', (data) =>
                dispatch(cardMoved(data))),

            onEvent('CardDeleted', (data) =>
                dispatch(cardDeleted(data))),

            onEvent('UserJoinedBoard', (data) =>
                dispatch(userJoinedBoard(data))),

            onEvent('UserLeftBoard', (data) =>
                dispatch(userLeftBoard(data))),
        ];

        // Cleanup runs when boardId changes or component unmounts.
        // This prevents receiving events for boards you are no longer viewing.
        return () => {
            leaveBoard(boardId);
            cleanups.forEach(cleanup => cleanup());
        };
    }, [boardId, dispatch]);
};