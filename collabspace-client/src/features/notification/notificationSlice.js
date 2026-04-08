import { createSlice } from '@reduxjs/toolkit';

const notificationSlice = createSlice({
    name: 'notifications',
    initialState: {
        items: [],
        unreadCount: 0,
        isLoading: false,
    },
    reducers: {
        setNotifications: (state, action) => {
            state.items = action.payload;
            state.unreadCount = action.payload
                .filter(n => !n.isRead).length;
        },

        // Called when SignalR pushes a new notification.
        // Prepends to the list so newest appears at the top.
        notificationReceived: (state, action) => {
            state.items.unshift(action.payload);
            state.unreadCount += 1;
        },

        // Clears the badge count and marks all as read in local state.
        allMarkedAsRead: (state) => {
            state.items = state.items.map(n => ({ ...n, isRead: true }));
            state.unreadCount = 0;
        },

        setUnreadCount: (state, action) => {
            state.unreadCount = action.payload;
        },
    },
});

export const {
    setNotifications, notificationReceived,
    allMarkedAsRead, setUnreadCount
} = notificationSlice.actions;

export default notificationSlice.reducer;