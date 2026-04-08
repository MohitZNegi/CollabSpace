import { useState, useEffect, useRef } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import {
    setNotifications,
    allMarkedAsRead
} from '../features/notifications/notificationSlice';
import axiosInstance from '../api/axiosInstance';
import '../styles/components/notifications.css';

function NotificationBell() {
    const dispatch = useDispatch();
    const { items, unreadCount } = useSelector((s) => s.notifications);
    const [isOpen, setIsOpen] = useState(false);
    const panelRef = useRef(null);

    useEffect(() => {
        const load = async () => {
            try {
                const response = await axiosInstance.get('/notifications');
                dispatch(setNotifications(response.data));
            } catch (error) {
                console.error('Failed to load notifications:', error);
            }
        };
        load();
    }, [dispatch]);

    useEffect(() => {
        const handleClickOutside = (e) => {
            if (panelRef.current && !panelRef.current.contains(e.target)) {
                setIsOpen(false);
            }
        };
        document.addEventListener('mousedown', handleClickOutside);
        return () => document.removeEventListener('mousedown', handleClickOutside);
    }, []);

    const handleMarkAllRead = async () => {
        try {
            await axiosInstance.patch('/notifications/read-all');
            dispatch(allMarkedAsRead());
        } catch (error) {
            console.error('Failed to mark as read:', error);
        }
    };

    const formatTime = (timestamp) => {
        const diff = Math.floor((new Date() - new Date(timestamp)) / 60000);
        if (diff < 1) return 'Just now';
        if (diff < 60) return `${diff}m ago`;
        if (diff < 1440) return `${Math.floor(diff / 60)}h ago`;
        return new Date(timestamp).toLocaleDateString();
    };

    return (
        <div className="notification-bell-wrapper" ref={panelRef}>
            <button
                className="notification-bell-btn"
                onClick={() => setIsOpen((prev) => !prev)}
                aria-label="Notifications"
            >
                &#128276;
                {unreadCount > 0 && (
                    <span className="notification-badge">
                        {unreadCount > 9 ? '9+' : unreadCount}
                    </span>
                )}
            </button>

            {isOpen && (
                <div className="notification-panel">
                    <div className="notification-panel-header">
                        <span className="notification-panel-title">
                            Notifications
                        </span>
                        {unreadCount > 0 && (
                            <button
                                className="notification-mark-read-btn"
                                onClick={handleMarkAllRead}
                            >
                                Mark all as read
                            </button>
                        )}
                    </div>

                    <div className="notification-list">
                        {items.length === 0 ? (
                            <div className="notification-empty">
                                No notifications yet
                            </div>
                        ) : (
                            items.map((notification) => (
                                <div
                                    key={notification.id}
                                    className={`notification-item ${notification.isRead ? 'read' : 'unread'
                                        }`}
                                >
                                    <div className={`notification-dot ${notification.isRead ? 'read' : 'unread'
                                        }`} />
                                    <div className="notification-item-body">
                                        <p className="notification-message">
                                            {notification.message}
                                        </p>
                                        <span className="notification-time">
                                            {formatTime(notification.createdAt)}
                                        </span>
                                    </div>
                                </div>
                            ))
                        )}
                    </div>
                </div>
            )}
        </div>
    );
}

export default NotificationBell;