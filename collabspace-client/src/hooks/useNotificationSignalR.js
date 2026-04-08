import { useEffect } from 'react';
import { useDispatch } from 'react-redux';
import { notificationReceived } from '../features/notification/notificationSlice';
import { onEvent } from '../services/signalrService';

// This hook is registered at the app level, not per-page.
// Notifications can arrive at any time regardless of what
// page the user is currently viewing.
export const useNotificationSignalR = () => {
    const dispatch = useDispatch();

    useEffect(() => {
        const cleanup = onEvent('ReceiveNotification', (notification) => {
            dispatch(notificationReceived(notification));
        });

        return cleanup;
    }, [dispatch]);
};