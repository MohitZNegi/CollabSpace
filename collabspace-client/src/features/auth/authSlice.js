import { createSlice } from '@reduxjs/toolkit';
import { loginUser, registerUser } from './authThunks';
import { stopConnection } from '../../services/signalrService';

// Read any persisted auth state from localStorage on app load.
// This is how the session persists across page refreshes (US-03).
// If the user refreshed the page, the store re-initialises from
// localStorage rather than losing the session.
const storedUser = localStorage.getItem('user');

const initialState = {
    user: storedUser ? JSON.parse(storedUser) : null,
    accessToken: localStorage.getItem('accessToken') || null,
    isLoading: false,
    error: null,
};

const authSlice = createSlice({
    name: 'auth',
    initialState,

    // Reducers handle synchronous state changes.
    // Each reducer is a pure function: same input always gives same output.
    reducers: {
        logout: (state) => {
            state.user = null;
            state.accessToken = null;
            state.error = null;
            localStorage.removeItem('accessToken');
            localStorage.removeItem('refreshToken');
            localStorage.removeItem('user');
            // Stop the SignalR connection when the user logs out.
            stopConnection();
        },
        clearError: (state) => {
            state.error = null;
        },
    },

    // extraReducers handle the three states of async thunks:
    // pending (request in flight), fulfilled (success), rejected (failure).
    // This gives you loading spinners and error messages for free.
    extraReducers: (builder) => {
        builder
            // LOGIN
            .addCase(loginUser.pending, (state) => {
                state.isLoading = true;
                state.error = null;
            })
            .addCase(loginUser.fulfilled, (state, action) => {
                state.isLoading = false;
                state.user = action.payload.user;
                state.accessToken = action.payload.accessToken;
            })
            .addCase(loginUser.rejected, (state, action) => {
                state.isLoading = false;
                state.error = action.payload;
            })

            // REGISTER
            .addCase(registerUser.pending, (state) => {
                state.isLoading = true;
                state.error = null;
            })
            .addCase(registerUser.fulfilled, (state, action) => {
                state.isLoading = false;
                state.user = action.payload.user;
                state.accessToken = action.payload.accessToken;
            })
            .addCase(registerUser.rejected, (state, action) => {
                state.isLoading = false;
                state.error = action.payload;
            });
    },
});

export const { logout, clearError } = authSlice.actions;
export default authSlice.reducer;